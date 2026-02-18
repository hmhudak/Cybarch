using System.ComponentModel.DataAnnotations;

namespace Cybarch.Web.Models;

/// <summary>
/// Inventory položka postavy.
/// ItemId odkazuje na katalóg predmetov v rulesete (nie je to DB tabuľka predmetov).
/// </summary>
public class CharacterItem
{
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    [Required, MaxLength(100)]
    public string ItemId { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
}
