using SQLite;

namespace ssk.Models;

[Table("user_accounts")]
public class UserAccount
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(50), Unique, NotNull]
    public string Username { get; set; } = string.Empty;

    [MaxLength(256), NotNull]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }
}
