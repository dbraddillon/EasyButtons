using SQLite;

namespace EasyButtons.Models;

[Table("Buttons")]
public class EasyButton
{
    [PrimaryKey]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Label { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
    /// <summary>Hex color string, e.g. "#E53935"</summary>
    public string Color { get; set; } = "#E53935";
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Path to a sound file. When set, tapping the button plays this sound instead of launching a URI.</summary>
    public string? SoundPath { get; set; }
}
