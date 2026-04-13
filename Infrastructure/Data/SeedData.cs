using Messages.Database.Entities;

namespace Infrastructure.Data;

public static class SeedData
{
    public static void Seed(CosmereContext db)
    {
        SeedUsers(db);
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    private static void SeedUsers(CosmereContext db)
    {
        var users = new[]
        {
            ("soul",        "Soul",        "Soul"),
            ("albert",      "Albert",      "Albert"),
            ("guizmo",      "Guizmo",      "Guizmo"),
            ("kaligula",    "Kaligula",    "Kaligula"),
            ("hanol",       "Hanol",       "Hanol"),
            ("raito",       "Raito",       "Raito"),
            ("rocapequena", "Rocapequena", "Roca Pequena"),
        };

        foreach (var (username, password, displayName) in users)
        {
            if (!db.Users.Any(u => u.Username == username))
            {
                db.Users.Add(new UserEntity
                {
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    DisplayName = displayName
                });
            }
        }
    }

}
