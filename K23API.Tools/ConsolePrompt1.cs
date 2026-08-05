namespace K23API.Tools;

public static class ConsolePrompt1
{
    public static void Heading(string text)
    {
        Console.WriteLine();
        Write(ConsoleColor.White, $"== {text} ==");
    }

    public static void Info(string text) => Console.WriteLine(text);

    public static void Success(string text) => Write(ConsoleColor.Green, text);

    public static void Warn(string text) => Write(ConsoleColor.Yellow, text);

    public static void Error(string text) => Write(ConsoleColor.Red, $"ERROR: {text}");

    public static void Hint(string text) => Write(ConsoleColor.DarkGray, text);

    public static string? ReadRequired(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var value = Console.ReadLine();

            if (value is null) return null;
            if (!string.IsNullOrWhiteSpace(value)) return value.Trim();

            Warn($"{label} is required.");
        }
    }

    public static string? ReadOptional(string label)
    {
        Console.Write($"{label} (optional): ");
        return Console.ReadLine()?.Trim();
    }

    public static bool Confirm(string question)
    {
        Console.Write($"{question} [y/N]: ");
        var answer = Console.ReadLine();

        return answer is not null && answer.Trim().Equals("y", StringComparison.OrdinalIgnoreCase);
    }

    public static bool ConfirmTyped(string question, string expected)
    {
        Console.Write($"{question} (type {expected} to continue): ");
        var answer = Console.ReadLine();

        return answer is not null && answer.Trim().Equals(expected, StringComparison.Ordinal);
    }

    public static int? ChooseOne(string title, IReadOnlyList<string> choices, string exitLabel)
    {
        while (true)
        {
            Heading(title);

            for (var index = 0; index < choices.Count; index++) Console.WriteLine($"  {index + 1}. {choices[index]}");
            Console.WriteLine($"  0. {exitLabel}");

            Console.Write("Select: ");
            var answer = Console.ReadLine();

            if (answer is null) return null;
            if (!int.TryParse(answer.Trim(), out var selected) || selected < 0 || selected > choices.Count)
            {
                Warn("Enter one of the listed numbers.");
                continue;
            }

            return selected == 0 ? null : selected - 1;
        }
    }

    public static IReadOnlyList<int>? ChooseMany(string title, IReadOnlyList<string> choices)
    {
        Heading(title);

        for (var index = 0; index < choices.Count; index++) Console.WriteLine($"  {index + 1}. {choices[index]}");

        Console.Write("Select (comma separated, blank for none): ");
        var answer = Console.ReadLine();

        if (answer is null) return null;
        if (string.IsNullOrWhiteSpace(answer)) return [];

        var selected = new List<int>();

        foreach (var token in answer.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(token, out var choice) || choice < 1 || choice > choices.Count)
            {
                Warn($"'{token}' is not one of the listed numbers, skipping it.");
                continue;
            }

            if (!selected.Contains(choice - 1)) selected.Add(choice - 1);
        }

        return selected;
    }

    private static void Write(ConsoleColor color, string text)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = previous;
    }
}
