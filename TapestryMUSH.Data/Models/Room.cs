using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TapestryMUSH.Data.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ExitsJson { get; set; } = "{}";

    // Exits
    public int? NorthRoomId { get; set; }
    public int? SouthRoomId { get; set; }
    public int? EastRoomId { get; set; }
    public int? WestRoomId { get; set; }

    public Room? NorthRoom { get; set; }
    public Room? SouthRoom { get; set; }
    public Room? EastRoom { get; set; }
    public Room? WestRoom { get; set; }

    public ICollection<Player> PlayersHere { get; set; } = new List<Player>();

    [NotMapped]
    public Dictionary<string, int> Exits
    {
        get
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, int>>(ExitsJson)
                       ?? new Dictionary<string, int>();
            }
            catch
            {
                return new Dictionary<string, int>(); // Prevent crash on bad JSON
            }
        }
        set => ExitsJson = JsonSerializer.Serialize(value);
    }
}
