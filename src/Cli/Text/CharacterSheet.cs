using Cli.Engine;
using Core.GameSystem;
using Core.TextSystem;
using Infra.Data;

using Infra.WebApi;
namespace Cli.Text
{
    /// <summary>
    /// Builds a structured text representation of one game character:
    /// lore, talents and gear rendered as sections, headings and paragraphs.
    /// </summary>
    public static class CharacterSheet
    {
        private const int MaxDescriptionChars = 300;

        /// <summary>
        /// Builds a printable document for the given character.
        /// </summary>
        /// <param name="id">The character id.</param>
        /// <param name="character">The domain character.</param>
        /// <param name="lore">The remote lore, or null for hand-made heroes.</param>
        /// <param name="items">The equipped items with their ids.</param>
        /// <param name="abilities">The known abilities with their ids.</param>
        /// <returns>The sheet document.</returns>
        public static TextDocument Build(
            string id,
            Character character,
            GenshinCharacterDto? lore,
            IReadOnlyList<KeyValuePair<string, Equipment>> items,
            IReadOnlyList<KeyValuePair<string, Ability>> abilities)
        {
            ArgumentNullException.ThrowIfNull(character);
            TextDocument document = new();

            Section hero = new(Slug(character.Name));
            hero.AddChild(new Heading(1, character.Name));
            hero.AddChild(new Paragraph(Headline(id, character, lore)));
            if (lore is not null && !string.IsNullOrWhiteSpace(lore.Description))
                hero.AddChild(new Paragraph(lore.Description));

            if (lore is not null && lore.SkillTalents.Count > 0)
            {
                Section talents = new("talents");
                talents.AddChild(new Heading(2, "Talents"));
                foreach (GenshinTalentDto talent in lore.SkillTalents)
                {
                    talents.AddChild(new Heading(2, talent.Name));
                    talents.AddChild(new Paragraph($"{talent.Unlock}. {Short(talent.Description)}"));
                }
                hero.AddChild(talents);
            }

            Section gear = new("gear");
            gear.AddChild(new Heading(2, "Gear"));
            if (items.Count == 0 && abilities.Count == 0)
            {
                gear.AddChild(new Paragraph("Nothing equipped or learned yet."));
            }
            else
            {
                foreach (KeyValuePair<string, Equipment> entry in items)
                {
                    gear.AddChild(new Paragraph(
                        $"{entry.Value.Name} [{entry.Key}] (+{entry.Value.AttackBonus} ATK, +{entry.Value.ArmorBonus} ARM)"));
                }
                foreach (KeyValuePair<string, Ability> entry in abilities)
                {
                    gear.AddChild(new Paragraph(
                        $"{entry.Value.Name} [{entry.Key}] (x{entry.Value.DamageMultiplier})"));
                }
            }
            hero.AddChild(gear);

            document.AddElement(hero);
            return document;
        }

        /// <summary>
        /// Builds the one line headline with stats and origin.
        /// </summary>
        /// <param name="id">The character id.</param>
        /// <param name="character">The domain character.</param>
        /// <param name="lore">The remote lore, or null.</param>
        /// <returns>The headline text.</returns>
        private static string Headline(string id, Character character, GenshinCharacterDto? lore)
        {
            string origin = lore is null
                ? "hand-made hero"
                : $"{lore.Title}, {lore.Vision}, {lore.Nation}, {lore.Rarity} stars";
            return $"[{id}] {character.Health}/{character.MaxHealth} HP, " +
                $"{character.TotalAttack} ATK, {character.TotalArmor} ARM. {origin}.";
        }

        /// <summary>
        /// Makes a path-safe section title from a name.
        /// </summary>
        /// <param name="name">The display name.</param>
        /// <returns>The lower case dash form.</returns>
        private static string Slug(string name) =>
            string.Join("-", name.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        /// <summary>
        /// Shortens a long description to the readable limit.
        /// </summary>
        /// <param name="text">The full text.</param>
        /// <returns>The trimmed text.</returns>
        private static string Short(string text)
        {
            if (text.Length <= MaxDescriptionChars)
                return text;
            return text[..MaxDescriptionChars] + " ...";
        }
    }
}
