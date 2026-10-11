using System;
using System.Configuration;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Text;

namespace aspnetapp.Infrastructure
{
    // Connection and encoding only. Queries and mutations intentionally live in code-behind.
    public static class LegacyDirectory
    {
        public static string BaseDn { get { return Setting("FAKE_LDAP_BASE_DN"); } }
        public static string TeamsDn { get { return "ou=Teams," + BaseDn; } }

        public static LdapConnection Open()
        {
            var connection = new LdapConnection(
                new LdapDirectoryIdentifier(Setting("FAKE_LDAP_HOST"), int.Parse(Setting("FAKE_LDAP_PORT"))),
                new NetworkCredential(Setting("FAKE_LDAP_BIND_DN"), Setting("FAKE_LDAP_PASSWORD")),
                AuthType.Basic);
            connection.Timeout = TimeSpan.FromSeconds(5);
            connection.SessionOptions.ProtocolVersion = 3;
            connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
            try { connection.Bind(); return connection; }
            catch { connection.Dispose(); throw; }
        }

        public static string TeamDn(string id)
        {
            if (!LegacyRules.ValidKey(id)) throw new ArgumentException("Invalid team ID.");
            return "ou=" + id + "," + TeamsDn;
        }

        public static string MemberDn(string teamId, string memberId)
        {
            if (!LegacyRules.ValidKey(memberId)) throw new ArgumentException("Invalid member ID.");
            return "uid=" + teamId + "." + memberId + "," + TeamDn(teamId);
        }

        public static string Value(SearchResultEntry entry, string attribute)
        {
            var values = entry.Attributes[attribute];
            return values == null || values.Count == 0 ? "" : Convert.ToString(values[0]);
        }

        public static string EscapeFilter(string value)
        {
            var result = new StringBuilder();
            foreach (var c in value ?? "")
            {
                switch (c)
                {
                    case '\\': result.Append("\\5c"); break;
                    case '*': result.Append("\\2a"); break;
                    case '(': result.Append("\\28"); break;
                    case ')': result.Append("\\29"); break;
                    case '\0': result.Append("\\00"); break;
                    default: result.Append(c); break;
                }
            }
            return result.ToString();
        }

        public static DirectoryAttributeModification Replace(string name, string value)
        {
            var modification = new DirectoryAttributeModification { Name = name, Operation = DirectoryAttributeOperation.Replace };
            // Replacing with zero values removes an optional attribute, including on repeated saves.
            if (!string.IsNullOrEmpty(value)) modification.Add(value);
            return modification;
        }

        private static string Setting(string name)
        {
            return Environment.GetEnvironmentVariable(name) ?? ConfigurationManager.AppSettings[name];
        }
    }
}
