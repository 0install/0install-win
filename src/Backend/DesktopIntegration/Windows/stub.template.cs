// Embedded source template used by StubBuilder class

using System.Diagnostics;
using System.Reflection;
using System.Text;

[assembly: AssemblyTitle("[TITLE]")]

public static class Stub
{
	public static int Main(string[] args)
	{
		ProcessStartInfo startInfo = new ProcessStartInfo("[EXE]", "[ARGUMENTS] " + GetArguments(args));
		startInfo.UseShellExecute = false;
		Process process = Process.Start(startInfo);
		process.WaitForExit();
		return process.ExitCode;
	}

	private static string GetArguments(string[] args)
	{
		StringBuilder output = new StringBuilder();
		bool first = true;
		for (int i = 0; i < args.Length; i++)
		{
			// No separator before first or after last line
			if (first) first = false;
			else output.Append(' ');

			output.Append(Escape(args[i]));
		}

		return output.ToString();
	}

	private static string Escape(string value)
	{
		value = value.Replace("\"", "\\\"");
		if (ContainsWhitespace(value)) value = "\"" + value + "\"";
		return value;
	}

	private static bool ContainsWhitespace(string text)
	{
		return text.Contains(" ") || text.Contains("\t") || text.Contains("\n") || text.Contains("\r");
	}
}
