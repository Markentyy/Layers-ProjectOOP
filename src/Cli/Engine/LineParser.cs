namespace Cli.Engine
{
    /// <summary>
    /// Splits raw input lines into verbs, arguments and --options.
    /// Double quotes group words with spaces into one token.
    /// </summary>
    public static class LineParser
    {
        /// <summary>
        /// Parses one input line into a structured command.
        /// </summary>
        /// <param name="line">The raw input line.</param>
        /// <returns>The parsed command.</returns>
        public static ParsedCommand Parse(string line)
        {
            List<string> tokens = Tokenize(line);
            if (tokens.Count == 0)
                throw new CommandException("Empty command.");

            RejectPlaceholders(line, tokens);
            string verb = tokens[0].ToLowerInvariant();
            List<string> args = new();
            Dictionary<string, string?> options =
                new(StringComparer.OrdinalIgnoreCase);

            int i = 1;
            while (i < tokens.Count)
            {
                string token = tokens[i];
                if (token.StartsWith("--", StringComparison.Ordinal) && token.Length > 2)
                {
                    string body = token[2..];
                    string name;
                    string? value;
                    int eq = body.IndexOf('=');
                    if (eq >= 0)
                    {
                        name = body[..eq].ToLowerInvariant();
                        value = body[(eq + 1)..];
                    }
                    else if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith("--", StringComparison.Ordinal))
                    {
                        name = body.ToLowerInvariant();
                        value = tokens[i + 1];
                        i++;
                    }
                    else
                    {
                        name = body.ToLowerInvariant();
                        value = null;
                    }
                    options[name] = value;
                }
                else
                {
                    args.Add(token);
                }
                i++;
            }

            return new ParsedCommand(verb, args, options);
        }

        /// <summary>
        /// Rejects literal angle brackets: they only mark placeholders in help.
        /// Suggests the same line with brackets removed.
        /// </summary>
        /// <param name="line">The raw input line.</param>
        /// <param name="tokens">The extracted tokens.</param>
        private static void RejectPlaceholders(string line, List<string> tokens)
        {
            bool hit = false;
            foreach (string token in tokens)
            {
                string value = token;
                int eq = token.IndexOf('=');
                if (token.StartsWith("--", StringComparison.Ordinal) && eq >= 0)
                    value = token[(eq + 1)..];
                if (value.Length > 2 && value.StartsWith("<", StringComparison.Ordinal) &&
                    value.EndsWith(">", StringComparison.Ordinal))
                {
                    hit = true;
                    break;
                }
            }
            if (!hit)
                return;

            string suggestion = System.Text.RegularExpressions.Regex.Replace(line, "<([^<>]*)>", "$1").Trim();
            string hint = "Wrong format: angle brackets <> must not be typed.";
            if (!suggestion.Equals(line.Trim(), StringComparison.Ordinal))
                hint += $" Example: {suggestion}";
            throw new CommandException(hint);
        }

        /// <summary>
        /// Splits a line on whitespace, keeping double quoted parts together.
        /// </summary>
        /// <param name="line">The raw input line.</param>
        /// <returns>The extracted tokens without surrounding quotes.</returns>
        public static List<string> Tokenize(string line)
        {
            List<string> tokens = new();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;
            bool hasCurrent = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    hasCurrent = true;
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (hasCurrent)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                        hasCurrent = false;
                    }
                }
                else
                {
                    current.Append(c);
                    hasCurrent = true;
                }
            }

            if (inQuotes)
                throw new CommandException("Unterminated quoted string.");
            if (hasCurrent)
                tokens.Add(current.ToString());

            return tokens;
        }
    }
}
