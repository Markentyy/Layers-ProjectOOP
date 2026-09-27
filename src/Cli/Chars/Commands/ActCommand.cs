using Cli.Engine;

using Infra.Display;
using Core.GameSystem;
namespace Cli.Chars
{
    /// <summary>
    /// Performs attack, heal and ability actions between characters.
    /// </summary>
    public sealed class ActCommand : IShellCommand
    {
        private readonly CharsWorld _world;

        /// <summary>
        /// Gets the command verb.
        /// </summary>
        public string Name => "act";

        /// <summary>
        /// Gets the one line help summary.
        /// </summary>
        public string Summary => "Attack, heal or use an ability on a target.";

        /// <summary>
        /// Gets the detailed usage line.
        /// </summary>
        public string Usage => "act <attack|heal|ability> <actor> <target> [--id <ability>] [--amount <n>]";

        /// <summary>
        /// Initializes an act command bound to the given world.
        /// </summary>
        /// <param name="world">The characters world.</param>
        public ActCommand(CharsWorld world)
        {
            ArgumentNullException.ThrowIfNull(world);
            _world = world;
        }

        /// <summary>
        /// Executes the requested action.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="command">The parsed command line.</param>
        public void Execute(IDisplay console, ParsedCommand command)
        {
            command.ExpectArgCount(3, 3, Usage);
            command.ExpectOptions(Usage, "id", "amount");
            string action = command.RequireArg(0, "an action: act <attack|heal|ability> ...");
            string key = action.ToLowerInvariant();
            if (key != "attack" && key != "heal" && key != "ability")
                throw new CommandException($"Unknown action '{action}'. Use attack, heal or ability.");
            KeyValuePair<string, Character> actor =
                _world.ResolveCharacter(command.RequireArg(1, "an actor."));
            KeyValuePair<string, Character> target =
                _world.ResolveCharacter(command.RequireArg(2, "a target."));

            switch (key)
            {
                case "attack":
                    ActAttack(console, actor.Value, target.Value);
                    break;
                case "heal":
                    ActHeal(console, command, target.Value);
                    break;
                default:
                    ActAbility(console, command, actor, target.Value);
                    break;
            }
        }

        /// <summary>
        /// Performs a standard attack and reports the damage.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="attacker">The attacking character.</param>
        /// <param name="target">The target character.</param>
        private static void ActAttack(IDisplay console, Character attacker, Character target)
        {
            int damage = attacker.Attack(target);
            if (target.IsDefeated)
                console.WriteLine($"{target.Name} has been defeated!");
            console.WriteLine($"{attacker.Name} attacks {target.Name} for {damage} damage!");
        }

        /// <summary>
        /// Heals the target by the option amount, or to full when omitted.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="command">The parsed command line.</param>
        /// <param name="target">The character to heal.</param>
        private static void ActHeal(IDisplay console, ParsedCommand command, Character target)
        {
            int amount;
            string? raw = command.GetOption("amount");
            if (raw is null)
            {
                amount = target.MaxHealth - target.Health;
            }
            else if (!int.TryParse(raw, out amount) || amount < 0)
            {
                throw new CommandException("Option --amount must be a number >= 0.");
            }

            target.Heal(amount);
            console.WriteLine($"{target.Name} heals for {amount} HP. Current HP: {target.Health}/{target.MaxHealth}");
        }

        /// <summary>
        /// Uses a learned ability of the actor against the target.
        /// </summary>
        /// <param name="console">The console for output.</param>
        /// <param name="command">The parsed command line.</param>
        /// <param name="actor">The ability user id and instance.</param>
        /// <param name="target">The target character.</param>
        private void ActAbility(
            IDisplay console,
            ParsedCommand command,
            KeyValuePair<string, Character> actor,
            Character target)
        {
            KeyValuePair<string, Ability> ability =
                _world.ResolveAbility(command.RequireOption("id"));
            if (!_world.BookOf(actor.Key).Contains(ability.Key, StringComparer.OrdinalIgnoreCase))
            {
                throw new CommandException(
                    $"{actor.Value.Name} does not know '{ability.Value.Name}'. " +
                    $"Teach it first: add --char_id {actor.Key} --id {ability.Key}");
            }

            console.WriteLine($"{actor.Value.Name} uses special ability: [{ability.Value.Name}] on {target.Name}!");
            int damage = actor.Value.UseAbility(ability.Value, target);
            console.WriteLine($"It deals {damage} damage!");
            if (target.IsDefeated)
                console.WriteLine($"{target.Name} has been defeated!");
        }
    }
}
