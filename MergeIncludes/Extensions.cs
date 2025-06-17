using Spectre.Console;
using Spectre.Console.Extensions;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Throw;

namespace MergeIncludes;
public static partial class Extensions
{
	const string INCLUDE = "include";
	const string REQUIRE = "require";
	const string EXACT = "exact";
	const string METHOD = "method";
	const string FILE = "file";
	const string CommentPatternText = "##.+";
	const string IncludePatternText = @$"#(?<{METHOD}>{INCLUDE}|{REQUIRE})(?<{EXACT}>-{EXACT})?\s+(?<{FILE}>.+)";

	[GeneratedRegex(
		@$"^(//\s*)?{IncludePatternText}|^(<!--\s*){IncludePatternText}(\s*-->)",
		RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex GetIncludePattern();

	[GeneratedRegex(
		@$"^(//\s*)?{CommentPatternText}|^(<!--\s*){CommentPatternText}(\s*-->)",
		RegexOptions.Compiled)]
	private static partial Regex GetCommentPattern();

	public static IAsyncEnumerable<string> MergeIncludesAsync(
		this FileInfo root,
		MergeOptions? options = null,
		Action<FileInfo>? onFileAccessed = null)
	{
		root.ThrowIfNull();
		Contract.EndContractBlock();

		Dictionary<string, FileInfo> registry = [];
		Register(root.FullName, registry, onFileAccessed);
		return MergeIncludesAsync(root.FullName, registry, options, onFileAccessed);
	}

	static IEnumerable<FileInfo> Register(
		string filePath,
		Dictionary<string, FileInfo> registry,
		Action<FileInfo>? onFileAccessed = null,
		bool newOnly = false)
	{
		filePath.ThrowIfNull().OnlyInDebug();
		registry.ThrowIfNull().OnlyInDebug();
		Contract.EndContractBlock();

		if (registry.TryGetValue(filePath, out var file))
		{
			if (!newOnly) yield return file;
			yield break;
		}

		var dir = Path.GetDirectoryName(filePath)!;
		var fileName = Path.GetFileName(filePath);
		var files = Directory.GetFiles(dir, fileName);

		if (files is null || files.Length == 0)
			throw new FileNotFoundException(string.Empty, filePath);

		foreach (var fp in files)
		{
			if (registry.TryGetValue(fp, out var fi))
			{
				if (!newOnly) yield return fi;
				continue;
			}

			fi = new FileInfo(fp);
			registry.Add(fp, fi);
			onFileAccessed?.Invoke(fi);
			yield return fi;
		}
	}

	static async IAsyncEnumerable<string> MergeIncludesAsync(
		string rootFileName,
		Dictionary<string, FileInfo> registry,
		MergeOptions? options = null,
		Action<FileInfo>? onFileAccessed = null,
		HashSet<string>? active = null,
		bool newOnly = false)
	{
		rootFileName.ThrowIfNull().OnlyInDebug();
		registry.ThrowIfNull().OnlyInDebug();
		Contract.EndContractBlock();

		var files = Register(rootFileName, registry, onFileAccessed, newOnly);

		active ??= [];

		foreach (var root in files)
		{
			await foreach (var e in MergeIncludesAsyncCore(root, registry, options, active, onFileAccessed))
			{
				yield return e;
			}
		}

		static async IAsyncEnumerable<string> MergeIncludesAsyncCore(
			FileInfo root,
			Dictionary<string, FileInfo> registry,
			MergeOptions? options,
			HashSet<string> active,
			Action<FileInfo>? onFileAccessed)
		{
			var rootFileName = root.FullName;
			if (!active.Add(rootFileName))
				throw new UnreachableException();

			var commentPattern = GetCommentPattern();
			var includePattern = GetIncludePattern();
			using var file = root.OpenRead();
			using var reader = new StreamReader(file);
			Lazy<string> path = new(() => root.Directory?.FullName.ThrowIfNull()!);
			var trimming = options?.Trim == true;
			var trimLeading = trimming;
			var lineNumber = 0;
			List<string>? whiteSpace = trimming ? [] : null;

		more:
			var line = await reader.ReadLineAsync().ConfigureAwait(false);
			if (line is null)
			{
				var pad = options?.Padding ?? 0;
				for (var i = 0; i < pad; i++)
					yield return string.Empty;

				active.Remove(rootFileName);
				yield break;
			}

			lineNumber++;

			// Trimming enabled?  Track the whitespace.
			if (trimming && string.IsNullOrWhiteSpace(line))
			{
				whiteSpace!.Add(line);
				goto more;
			}

			if (trimLeading)
			{
				trimLeading = false;
				whiteSpace!.Clear();
			}

			if (trimming && whiteSpace!.Count != 0)
			{
				foreach (var w in whiteSpace)
					yield return w;

				whiteSpace.Clear();
			}

			if (commentPattern.IsMatch(line))
			{
				goto more;
			}			var include = includePattern.Match(line);
			if (!include.Success)
			{
				yield return line;
				goto more;
			}
			
			// Use ReadOnlySpan to avoid string allocation for file group
			var fileGroup = include.Groups[FILE];
			var includePath = Path.GetFullPath(Path.Combine(path.Value, fileGroup.Value));
			if (active.Contains(includePath))
				throw new InvalidOperationException($"Detected recursive reference to {includePath}.");

			var require = include.Groups[METHOD].ValueSpan.Equals(REQUIRE, StringComparison.OrdinalIgnoreCase);
			// Require means to only include it once.
			if (require && registry.ContainsKey(includePath))
				goto more;

			// Invoke callback for every include, not just first registration
			if (registry.TryGetValue(includePath, out var includeFileInfo))
			{
				onFileAccessed?.Invoke(includeFileInfo);
			}

			var exact = include.Groups[EXACT].Success;

			List<string> included = [];
			try
			{
				await foreach (var n in MergeIncludesAsync(includePath, registry, exact ? null : options, onFileAccessed, active, require))
				{
					included.Add(n);
				}
			}
			catch (FileNotFoundException ex)
			{
				// String empty means it hasn't been captured yet.
				// If the message is not empty, it was created by the code below and should be thrown as is.
				if (ex.Message != string.Empty)
					throw;

				throw new FileNotFoundException($"Could not find include file on line {lineNumber} in:{Environment.NewLine}{root.FullName}:{lineNumber}", includePath, ex);
			}

			foreach (var n in included)
			{
				yield return n;
			}

			trimLeading = trimming;

			goto more;
		}
	}

	/// <summary>
	/// Creates a formatted Spectre.Console Panel from a FileNotFoundException.
	/// </summary>
	/// <param name="exception">The FileNotFoundException to format.</param>
	/// <param name="heading">Optional custom heading for the panel.</param>
	/// <returns>A Panel containing the formatted error message and file name.</returns>
	public static PanelBuilder ToPanel(this FileNotFoundException exception, string heading = "Error")
	{
		var panel = new PanelBuilder($"[red]{heading}:[/]");
		if (!string.IsNullOrWhiteSpace(exception.FileName))
			panel.Add(new Text(exception.FileName));
		panel.Add(new Text(exception.Message, Color.Yellow));
		return panel;
	}

	/// <summary>
	/// Creates a formatted Spectre.Console Panel from an Exception.
	/// </summary>
	/// <param name="exception">The exception to format.</param>
	/// <param name="heading">Optional custom heading for the panel.</param>
	/// <returns>A Panel containing the formatted error message.</returns>
	public static PanelBuilder ToPanel(this Exception exception, string heading = "Error")
	{
		var panel = new PanelBuilder($"[red]{heading}:[/]");
		panel.Add(new Text(exception.Message));
		return panel;
	}

	/// <summary>
	/// Writes the FileNotFoundException as a formatted panel to the console.
	/// </summary>
	/// <param name="exception">The FileNotFoundException to write.</param>
	/// <param name="heading">Optional custom heading for the panel.</param>
	/// <param name="console">The console to write to. Defaults to AnsiConsole.Console if not provided.</param>
	public static void WriteToConsole(this FileNotFoundException exception, string heading = "Error", IAnsiConsole? console = null)
		=> (console ?? AnsiConsole.Console).Write(exception.ToPanel(heading));

	/// <summary>
	/// Writes any Exception as a formatted panel to the console.
	/// </summary>
	/// <param name="exception">The exception to write.</param>
	/// <param name="heading">Optional custom heading for the panel.</param>
	/// <param name="console">The console to write to. Defaults to AnsiConsole.Console if not provided.</param>
	public static void WriteToConsole(this Exception exception, string heading = "Error", IAnsiConsole? console = null)
		=> (console ?? AnsiConsole.Console).Write(exception.ToPanel(heading));
}