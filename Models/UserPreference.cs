using SQLite;
namespace ssk.Models;

[Table("user_preferences")]
public class UserPreference
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Uniqueness is enforced in the repository, not via attribute
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public UserPreference() { }

    public UserPreference(string key, string value)
    {
        Key = key;
        Value = value;
    }
}
