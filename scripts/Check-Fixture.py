"""Check that the portable fixture inputs and Web Forms file bindings agree.

No claim of executing ASP.NET or LDAP is made by this check.
"""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
APP = ROOT / "aspnetapp"


def records(path):
    text = path.read_text(encoding="utf-8")
    result = []
    for block in re.split(r"\n\s*\n", text.strip()):
        record = {}
        for line in block.splitlines():
            if not line or line.startswith("#"):
                continue
            key, separator, value = line.partition(": ")
            if separator:
                record.setdefault(key, []).append(value)
        if "dn" in record:
            result.append(record)
    return result


def main():
    ET.parse(APP / "Web.config")
    ET.parse(APP / "aspnetapp.csproj")
    config = ET.parse(APP / "Web.config").getroot()
    settings = {item.attrib["key"]: item.attrib["value"] for item in config.findall("appSettings/add")}
    assert settings["FAKE_LDAP_BASE_DN"] == "dc=example,dc=test"
    assert settings["FAKE_LDAP_BIND_DN"] == "cn=admin,dc=example,dc=test"

    for pattern in ("*.aspx", "*.ascx", "*.Master"):
        for page in APP.rglob(pattern):
            text = page.read_text(encoding="utf-8-sig")
            behind = re.search(r'CodeBehind="([^"]+)"', text)
            if not behind:
                continue
            assert (page.parent / behind[1]).is_file(), str(page)
            designer = page.with_name(page.name + ".designer.cs")
            assert designer.is_file(), str(page)
            # Named outer server controls must have designer fields. Row templates
            # have their own naming containers and are excluded.
            outer = re.sub(r"<ItemTemplate>.*?</ItemTemplate>", "", text, flags=re.S)
            ids = re.findall(r'<(?:asp|legacy):[\w]+\b[^>]*\bID="([^"]+)"', outer)
            for control_id in ids:
                if control_id.endswith("Content"):
                    continue
                assert re.search(r"\b" + control_id + r"\s*;", designer.read_text()), (str(page), control_id)
            for source in re.findall(r'Src="~/([^"]+)"', text):
                assert (APP / source).is_file(), source

    seed = records(ROOT / "ldap/bootstrap/entries.ldif")
    teams = [x for x in seed if "fakeTeam" in x.get("objectClass", [])]
    members = [x for x in seed if "inetOrgPerson" in x.get("objectClass", [])]
    assert len(teams) == 4 and len(members) == 2
    seen = set()
    for entry in seed:
        dn = entry["dn"][0]
        assert dn not in seen, dn
        seen.add(dn)
        if "fakeTeam" in entry["objectClass"]:
            assert entry["fakeStatus"][0] in {"active", "planned", "archived"}
            assert entry.get("displayName")
        else:
            assert dn.split(",", 1)[1] in seen, "Parent must precede child"
            assert entry["mail"][0].endswith("@example.test")
    assert any(x["fakeStatus"] == ["planned"] for x in teams)
    assert any(x["fakeStatus"] == ["archived"] for x in teams)
    assert any("fakeStartDate" not in x for x in teams)
    print("Fixture XML, code-behind/designer bindings, linked controls and synthetic seed checks passed.")


if __name__ == "__main__":
    main()
