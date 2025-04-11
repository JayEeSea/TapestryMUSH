using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TapestryMUSH.Data.Models;

public class Player
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FlagsJson { get; set; } = "[]";

    [NotMapped]
    public HashSet<string> Flags
    {
        get
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FlagsJson))
                    return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var parsed = JsonSerializer.Deserialize<HashSet<string>>(FlagsJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return parsed != null
                    ? new HashSet<string>(parsed, StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }
        set
        {
            FlagsJson = JsonSerializer.Serialize(value);
        }
    }

    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
    public DateTime DateCreated {  get; set; } = DateTime.UtcNow;
    public int? CurrentRoomId { get; set; }
    public Room? CurrentRoom { get; set; }
}
