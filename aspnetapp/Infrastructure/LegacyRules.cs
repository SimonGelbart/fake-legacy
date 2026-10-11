using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace aspnetapp.Infrastructure
{
    public static class LegacyRules
    {
        public static bool ValidKey(string value)
        {
            return value != null && Regex.IsMatch(value, "\\A[a-z][a-z0-9-]{2,31}\\z");
        }

        public static bool Future(string value)
        {
            DateTime date;
            return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out date) && date.Date > DateTime.UtcNow.Date;
        }

        public static string TeamError(string id, string name, string status, string startDate, string description)
        {
            if (!ValidKey(id)) return "Team ID must be 3–32 lowercase letters, digits or hyphens, starting with a letter.";
            if (name.Length < 3 || name.Length > 80) return "Display name must be 3–80 characters.";
            if (description.Length > 500) return "Description must be at most 500 characters.";
            if (status != "active" && status != "planned" && status != "archived") return "Choose a valid status.";
            DateTime parsed;
            if (startDate.Length > 0 && !DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return "Start date must use yyyy-MM-dd.";
            if (status == "planned" && !Future(startDate)) return "Planned teams need a future start date.";
            return null;
        }

        public static string MemberError(string id, string name, string surname, string email)
        {
            if (!ValidKey(id)) return "Member ID must be 3–32 lowercase letters, digits or hyphens, starting with a letter.";
            if (name.Length < 3 || name.Length > 80 || surname.Length == 0 || surname.Length > 80)
                return "Full name (3–80 characters) and surname (1–80 characters) are required.";
            if (!Regex.IsMatch(email, "\\A[^\\s@]+@[^\\s@]+\\.[^\\s@]+\\z") || email.Length > 254)
                return "Enter a valid email address.";
            return null;
        }
    }
}
