#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
compose=(docker compose)
mkdir -p artifacts
ldap() {
  "${compose[@]}" exec -T ldap sh -c 'tool=$1; shift; exec "$tool" -x -H ldap://127.0.0.1:389 -D "$LDAP_INIT_ROOT_USER_DN" -w "$LDAP_INIT_ROOT_USER_PW" "$@"' sh "$@"
}
search() { ldap ldapsearch -LLL "$@"; }
teams='ou=Teams,dc=example,dc=test'
testdn="ou=smoke-$(date +%s)-${RANDOM},${teams}"
testkey=${testdn#ou=}
testkey=${testkey%%,*}
child="uid=child,${testdn}"
cleanup() {
  ldap ldapdelete "$child" >/dev/null 2>&1 || true
  ldap ldapdelete "$testdn" >/dev/null 2>&1 || true
}
trap cleanup EXIT

output=$(search -b "$teams" -s one '(objectClass=fakeTeam)' ou)
[[ $(printf '%s\n' "$output" | grep -c '^dn:') == 4 ]]
output=$(search -b "ou=alpha,$teams" -s one '(objectClass=inetOrgPerson)' uid)
[[ $(printf '%s\n' "$output" | grep -c '^dn:') == 2 ]]
output=$(search -b "$teams" -s one '(&(objectClass=fakeTeam)(!(fakeStatus=archived)))' ou)
[[ $(printf '%s\n' "$output" | grep -c '^dn:') == 3 ]]
output=$(search -b "$teams" -s one '(ou=\2a)' ou)
[[ $output != *'dn:'* ]]

ldap ldapadd <<EOF
dn: $testdn
objectClass: top
objectClass: organizationalUnit
objectClass: fakeTeam
ou: $testkey
displayName: Smoke test team
fakeStatus: active

dn: $child
objectClass: top
objectClass: person
objectClass: organizationalPerson
objectClass: inetOrgPerson
uid: child
cn: ${testdn%%,*}-child
sn: Example
employeeNumber: child
displayName: Smoke Member
mail: smoke@example.test
EOF
ldap ldapmodify <<EOF
dn: $testdn
changetype: modify
replace: description
description: Changed by smoke check
EOF
output=$(search -b "$testdn" -s base '(objectClass=*)' description)
[[ $output == *'description: Changed by smoke check'* ]]
if ldap ldapdelete "$testdn" 2>artifacts/ldap-delete-error.txt; then
  echo 'ERROR: LDAP allowed deletion of a nonempty parent' >&2; exit 1
fi
grep -q 'not allowed on non-leaf\|Not allowed on nonleaf\|(66)' artifacts/ldap-delete-error.txt
cleanup
trap - EXIT
echo 'LDAP seed, scope, filter escaping, custom schema, create/update/delete and non-leaf protection passed.'
