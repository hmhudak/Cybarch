using System.ComponentModel.DataAnnotations;

namespace Cybarch.Web.Models;

public class CharacterCurrency
{
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    [Required, MaxLength(50)]
    public string DenominationKey { get; set; } = string.Empty; // "zl","st","md"...

    public int Amount { get; set; }
}
