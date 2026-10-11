using System;
using aspnetapp.Infrastructure;

internal static class Program
{
    private static int assertions;
    private static void Check(bool success, string message)
    {
        assertions++;
        if (!success) throw new Exception(message);
    }

    private static void Main()
    {
        Check(LegacyRules.ValidKey("abc"), "minimum key length");
        Check(LegacyRules.ValidKey("a" + new string('b', 31)), "maximum key length");
        foreach (var key in new[] { null, "ab", "Alpha", "1abc", "abc\n", "abc,ou=Other", "a" + new string('b', 32) })
            Check(!LegacyRules.ValidKey(key), "reject invalid key");
        Check(!LegacyRules.Future(""), "no date is not a future date");
        Check(!LegacyRules.Future(DateTime.UtcNow.ToString("yyyy-MM-dd")), "today is included by default");
        Check(LegacyRules.Future(DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd")), "tomorrow is future");
        Check(LegacyRules.TeamError("abc", "Team", "active", "", "") == null, "active can omit date");
        Check(LegacyRules.TeamError("abc", "Team", "planned", "", "") != null, "planned requires date");
        Check(LegacyRules.TeamError("abc", "Team", "planned", "2020-01-01", "") != null, "planned requires future");
        Check(LegacyRules.TeamError("abc", "Team", "planned", DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"), "") == null, "valid planned team");
        Check(LegacyRules.TeamError("abc", "Team", "active", "2025-02-30", "") != null, "invalid calendar date");
        Check(LegacyRules.TeamError("abc", "ab", "active", "", "") != null, "short name");
        Check(LegacyRules.TeamError("abc", "Team", "invalid", "", "") != null, "invalid status");
        Check(LegacyRules.TeamError("abc", "Team", "active", "", new string('x', 501)) != null, "long description");
        Check(LegacyRules.MemberError("abc", "Example Name", "Name", "name@example.test") == null, "valid member");
        Check(LegacyRules.MemberError("abc", "Example Name", "", "name@example.test") != null, "surname required");
        Check(LegacyRules.MemberError("abc", "Example Name", "Name", "invalid@") != null, "invalid email");
        Check(LegacyDirectory.EscapeFilter("*(a)\\\0") == "\\2a\\28a\\29\\5c\\00", "LDAP filter metacharacters escaped");
        Check(LegacyDirectory.EscapeFilter("Équipe") == "Équipe", "ordinary Unicode preserved");
        Console.WriteLine(assertions + " behavior assertions passed.");
    }
}
