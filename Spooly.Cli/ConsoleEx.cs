using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Spooly;

public static class ConsoleEx
{
	public enum Severity
	{
		Safe,
		Unsafe,
		Critical
	}

	private static ConsoleColor GetSeverityColor(Severity severity)
		=> severity switch
		{
			Severity.Safe => ConsoleColor.White,
			Severity.Unsafe => ConsoleColor.DarkYellow,
			Severity.Critical => ConsoleColor.Red,
			_ => ConsoleColor.White
		};

	private static void WithColor(ConsoleColor color, Action action)
	{
		var prev = Console.ForegroundColor;
		try
		{
			Console.ForegroundColor = color;
			action();
		}
		finally
		{
			Console.ForegroundColor = prev;
		}
	}
	private static readonly Regex HoursRegex = new(
		"^\\s*(?:(?<h>\\d+)\\s*h)?\\s*(?:(?<m>\\d+)\\s*m)?\\s*(?:(?<s>\\d+)\\s*s)?\\s*$",
		RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

	public static void PrintHeader(string title)
	{
		Console.WriteLine(title);
		Console.WriteLine(new string('-', title.Length));
		Console.WriteLine();
	}

	public static void Pause()
	{
		Console.WriteLine();
		Console.Write("Press Enter to continue...");
		Console.ReadLine();
	}

	public static void ShowMessage(string message)
	{
		Console.WriteLine();
		Console.WriteLine(message);
		Console.WriteLine();
		Pause();
	}

	public static void ShowMessage(string message, Severity severity)
	{
		Console.WriteLine();
		WithColor(GetSeverityColor(severity), () => Console.WriteLine(message));
		Console.WriteLine();
		Pause();
	}

	public static void DrawMenuItem(string message, Severity severity = Severity.Safe, int? index = null)
	{
		if (index is null)
			WithColor(GetSeverityColor(severity), () => Console.WriteLine(message));
		else
			WithColor(GetSeverityColor(severity), () => Console.WriteLine($"{index}) {message}"));
	}

	public static void ShowInline(string message, Severity severity)
		=> WithColor(GetSeverityColor(severity), () => Console.WriteLine(message));

	public static void WriteInline(string message, Severity severity)
		=> WithColor(GetSeverityColor(severity), () => Console.Write(message));

	public static void RequestConfirmation(string msg, Action next)
	{
		Console.WriteLine();
		Console.Write($"{msg} (y/n): ");
		while (true)
		{
			var key = Console.ReadKey(intercept: true);
			var ch = char.ToLowerInvariant(key.KeyChar);
			if (ch == 'y')
			{
				Console.WriteLine('y');
				next();
				return;
			}
			if (ch == 'n')
			{
				Console.WriteLine('n');
				return;
			}
		}
	}

	public static void RequestConfirmation(string msg, Severity severity, Action next)
	{
		Console.WriteLine();
		WithColor(GetSeverityColor(severity), () => Console.Write($"{msg} (y/n): "));
		while (true)
		{
			var key = Console.ReadKey(intercept: true);
			var ch = char.ToLowerInvariant(key.KeyChar);
			if (ch == 'y')
			{
				Console.WriteLine('y');
				next();
				return;
			}
			if (ch == 'n')
			{
				Console.WriteLine('n');
				return;
			}
		}
	}

	public static string ReadMenuChoice(string prompt)
	{
		Console.Write($"{prompt}: ");
		while (true)
		{
			var key = Console.ReadKey(intercept: true);
			if (key.Key == ConsoleKey.Enter)
			{
				continue;
			}

			if (key.Key == ConsoleKey.Escape)
				return "0";

			var ch = key.KeyChar;
			if (char.IsControl(ch))
			{
				continue;
			}

			Console.WriteLine(ch);
			return ch.ToString();
		}
	}

	public static string ReadRequiredString(string label)
	{
		while (true)
		{
			Console.Write($"{label}: ");
			var value = Console.ReadLine()?.Trim();
			if (!string.IsNullOrWhiteSpace(value))
			{
				return value;
			}

			Console.WriteLine("Value is required.");
		}
	}

	public static int ReadInt(string label, int min, int max)
	{
		while (true)
		{
			Console.Write($"{label}: ");
			var input = Console.ReadLine()?.Trim();
			if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
				&& value >= min && value <= max)
			{
				return value;
			}

			Console.WriteLine($"Please enter a valid integer between {min} and {max}.");
		}
	}

	public static decimal ReadDecimal(string label, decimal min)
	{
		while (true)
		{
			Console.Write($"{label}: ");
			var input = Console.ReadLine()?.Trim()?.Replace(',', '.');
			if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) && value >= min)
			{
				return value;
			}

			Console.WriteLine($"Please enter a valid number >= {min}.");
		}
	}

	public static decimal ReadDecimal(string label, decimal min, decimal defaultValue)
	{
		while (true)
		{
			Console.Write($"{label}: ");
			var raw = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(raw))
			{
				if (defaultValue >= min)
				{
					return defaultValue;
				}

				Console.WriteLine($"Please enter a valid number >= {min}.");
				continue;
			}

			var input = raw.Trim().Replace(',', '.');
			if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) && value >= min)
			{
				return value;
			}

			Console.WriteLine($"Please enter a valid number >= {min}.");
		}
	}

	public static decimal ReadHoursDecimal(string label, decimal min)
	{
		while (true)
		{
			Console.Write($"{label}: ");
			var inputRaw = Console.ReadLine()?.Trim();
			if (string.IsNullOrWhiteSpace(inputRaw))
			{
				Console.WriteLine($"Please enter a valid number >= {min}.");
				continue;
			}

			if (TryParseHours(inputRaw, out var value) && value >= min)
			{
				return value;
			}

			Console.WriteLine($"Please enter a valid time >= {min}. Examples: 1h10m, 15m35s, 1:10, 01:10, 1:10:05, 15:35, 1.12");
		}
	}

	private static bool TryParseHours(string inputRaw, out decimal hours)
	{
		hours = 0;
		var input = inputRaw.Trim();

		var colonIndex = input.IndexOf(':');
		if (colonIndex >= 0)
		{
			var parts = input.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			switch (parts.Length)
			{
				case 2:
					if (int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var a)
						&& int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var b)
						&& a >= 0 && b >= 0 && b < 60)
					{
						if (a >= 60)
						{
							hours = a + (b / 60m);
							return true;
						}

						hours = (a / 60m) + (b / 3600m);
						return true;
					}
					break;
				case 3:
					if (int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var h)
						&& int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var m)
						&& int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var s)
						&& h >= 0 && m >= 0 && s >= 0 && m < 60 && s < 60)
					{
						hours = h + (m / 60m) + (s / 3600m);
						return true;
					}
					break;
			}

			return false;
		}

		var match = HoursRegex.Match(input);
		if (match.Success)
		{
			var hText = match.Groups["h"].Value;
			var mText = match.Groups["m"].Value;
			var sText = match.Groups["s"].Value;
			if (!string.IsNullOrEmpty(hText) || !string.IsNullOrEmpty(mText) || !string.IsNullOrEmpty(sText))
			{
				var h = string.IsNullOrEmpty(hText) ? 0 : int.Parse(hText, CultureInfo.InvariantCulture);
				var m = string.IsNullOrEmpty(mText) ? 0 : int.Parse(mText, CultureInfo.InvariantCulture);
				var s = string.IsNullOrEmpty(sText) ? 0 : int.Parse(sText, CultureInfo.InvariantCulture);
				if (h >= 0 && m >= 0 && s >= 0)
				{
					hours = h + (m / 60m) + (s / 3600m);
					return true;
				}
			}
		}

		var normalized = input.Replace(',', '.');
		return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out hours);
	}
}
