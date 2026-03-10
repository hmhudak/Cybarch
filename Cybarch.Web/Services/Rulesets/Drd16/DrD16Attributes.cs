namespace Cybarch.Web.Services.Rulesets.Drd16
{
    public record AttributeRange(
        int StrengthMin, int StrengthMax,
        int IntelligenceMin, int IntelligenceMax,
        int EnduranceMin, int EnduranceMax,
        int DexterityMin, int DexterityMax,
        int CharismaMin, int CharismaMax
    );

    public static class AttributeRanges
    {
        // Kľúč: (Rasa, Povolanie)
        public static readonly Dictionary<(string Race, string Profession), AttributeRange> Data = new()
        {
            // ── Barbar ──────────────────────────────────────────────────────────
            [("barbar", "alchymista")] = new(10, 15, 8, 13, 11, 16, 12, 17, 1, 16),
            [("barbar", "hranicar")] = new(14, 19, 8, 13, 11, 16, 8, 13, 1, 16),
            [("barbar", "kouzelnik")] = new(10, 15, 10, 15, 11, 16, 8, 13, 10, 15),
            [("barbar", "valecnik")] = new(16, 21, 6, 11, 14, 19, 8, 13, 1, 16),
            [("barbar", "zlodej")] = new(10, 15, 6, 11, 11, 16, 13, 18, 9, 14),

            // ── Človek ──────────────────────────────────────────────────────────
            [("clovek", "alchymista")] = new(6, 16, 12, 17, 9, 14, 13, 18, 2, 17),
            [("clovek", "hranicar")] = new(11, 16, 12, 17, 9, 14, 9, 14, 2, 17),
            [("clovek", "kouzelnik")] = new(6, 16, 14, 19, 9, 14, 9, 14, 13, 18),
            [("clovek", "valecnik")] = new(13, 18, 10, 15, 13, 18, 9, 14, 2, 17),
            [("clovek", "zlodej")] = new(6, 16, 10, 15, 9, 14, 14, 19, 12, 17),

            // ── elf ─────────────────────────────────────────────────────────────
            [("elf", "alchymista")] = new(6, 11, 15, 20, 6, 11, 15, 20, 8, 18),
            [("elf", "hranicar")] = new(11, 16, 15, 20, 6, 11, 10, 15, 8, 18),
            [("elf", "kouzelnik")] = new(6, 11, 17, 22, 6, 11, 10, 15, 15, 20),
            [("elf", "valecnik")] = new(13, 18, 12, 17, 12, 17, 10, 15, 8, 18),
            [("elf", "zlodej")] = new(6, 11, 12, 17, 6, 11, 16, 21, 14, 19),

            // ── hobit ───────────────────────────────────────────────────────────
            [("hobit", "alchymista")] = new(3, 8, 10, 15, 8, 13, 15, 20, 8, 18),
            [("hobit", "hranicar")] = new(6, 11, 10, 15, 8, 13, 11, 16, 8, 18),
            [("hobit", "kouzelnik")] = new(3, 8, 12, 17, 8, 13, 11, 16, 16, 21),
            [("hobit", "valecnik")] = new(8, 13, 10, 15, 13, 18, 11, 16, 8, 18),
            [("hobit", "zlodej")] = new(3, 8, 10, 15, 8, 13, 16, 21, 15, 20),

            // ── Kroll ───────────────────────────────────────────────────────────
            [("kroll", "alchymista")] = new(11, 16, 6, 11, 13, 18, 10, 15, 1, 11),
            [("kroll", "hranicar")] = new(17, 22, 6, 11, 13, 18, 5, 10, 1, 11),
            [("kroll", "kouzelnik")] = new(11, 16, 8, 13, 13, 18, 5, 10, 8, 13),
            [("kroll", "valecnik")] = new(19, 24, 2, 7, 16, 21, 5, 10, 1, 11),
            [("kroll", "zlodej")] = new(11, 16, 2, 7, 13, 18, 11, 16, 7, 12),

            // ── Kuduk ───────────────────────────────────────────────────────────
            [("kuduk", "alchymista")] = new(5, 10, 10, 15, 10, 15, 14, 19, 7, 12),
            [("kuduk", "hranicar")] = new(8, 13, 10, 15, 10, 15, 10, 15, 7, 12),
            [("kuduk", "kouzelnik")] = new(5, 10, 12, 17, 10, 15, 10, 15, 13, 18),
            [("kuduk", "valecnik")] = new(10, 15, 9, 14, 14, 19, 10, 15, 7, 12),
            [("kuduk", "zlodej")] = new(5, 10, 9, 14, 10, 15, 15, 20, 12, 17),

            // ── Trpaslik ────────────────────────────────────────────────────────
            [("trpaslik", "alchymista")] = new(7, 12, 10, 15, 12, 17, 11, 16, 7, 12),
            [("trpaslik", "hranicar")] = new(12, 17, 10, 15, 12, 17, 7, 12, 7, 12),
            [("trpaslik", "kouzelnik")] = new(7, 12, 12, 17, 12, 17, 7, 12, 11, 16),
            [("trpaslik", "valecnik")] = new(14, 19, 8, 13, 17, 22, 7, 12, 7, 12),
            [("trpaslik", "zlodej")] = new(7, 12, 8, 13, 12, 17, 12, 17, 10, 15),
        };

        // Pomocná metóda pre pohodlný prístup
        public static AttributeRange? Get(string race, string profession)
            => Data.TryGetValue((race, profession), out var range) ? range : null;
    }
}
