using Cli.Chars;
using Cli.Engine;
using Cli.Text;
using Infra.Data;
using Infra.Display;

// Composition root: wires the layers together.
//   dotnet run -- --text   text document mode
//   dotnet run -- --chars  characters mode
// Without options a short dialog asks for the mode.
string mode = SelectMode(args);

IDisplay display = new ConsoleDisplay();
if (mode == "text")
{
    TextNavigator navigator = new(TextSeed.CreateDemoDocument());
    Repl.Run(display, TextMode.Intro, TextMode.CreateCommands(navigator, display, new JsonTextStore()));
}
else
{
    CharsWorld world = new();
    Repl.Run(display, CharsMode.Intro, CharsMode.CreateCommands(world, display, new JsonCharsStore()));
}

static string SelectMode(string[] args)
{
    foreach (string arg in args)
    {
        if (arg == "--text")
            return "text";
        if (arg == "--chars")
            return "chars";
        Console.WriteLine($"Unknown option '{arg}'. Use --text or --chars.");
    }

    return AskMode();
}

static string AskMode()
{
    Console.WriteLine("Select mode: 1) text  2) chars");
    while (true)
    {
        Console.Write("mode: ");
        string? input = Console.ReadLine();
        if (input is null)
            return "text";
        string choice = input.Trim().ToLowerInvariant();
        if (choice is "1" or "text" or "--text")
            return "text";
        if (choice is "2" or "chars" or "--chars")
            return "chars";
        Console.WriteLine("Enter 1/text or 2/chars.");
    }
}
