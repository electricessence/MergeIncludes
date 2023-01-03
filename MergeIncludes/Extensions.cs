using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Throw;

namespace MergeIncludes;
public static partial class Extensions
{
	const string INCLUDE = "include";
	const string REQUIRE = "require";
	const string METHOD = "method";
	const string FILE = "file";
	const string IncludePatternText = @$"#(?<{METHOD}>{INCLUDE}|{REQUIRE})\s+(?<{FILE}>.+)";

	[GeneratedRegex(
		@$"^(//\s*)?{IncludePatternText}|^(<!--\s*){IncludePatternText}(\s*-->)",
		RegexOptions.Compiled
		| RegexOptions.IgnoreCase)]
	private static partial Regex GetIncludePattern();

	public static IAsyncEnumerable<string> MergeIncludesAsync(
		this FileInfo root,
		MergeOptions? options = null,
		Action<FileInfo>? onFileAccessed = null)
	{
		root.ThrowIfNull();
		Contract.EndContractBlock();

		Dictionary<string, FileInfo> registry = new();
		Register(root.FullName, registry, onFileAccessed);
		return MergeIncludesAsync(root.FullName, registry, options, onFileAccessed);
	}

	static FileInfo Register(
		string filePath,
		Dictionary<string, FileInfo> registry,
		Action<FileInfo>? onFileAccessed = null)
	{
		filePath.ThrowIfNull().OnlyInDebug();
		registry.ThrowIfNull().OnlyInDebug();
		Contract.EndContractBlock();

		if (registry.TryGetValue(filePath, out var file))
			return file;

		file = new FileInfo(filePath);
		if (!file.Exists)
			throw new FileNotFoundException(file.FullName);

		registry.Add(filePath, file);
		onFileAccessed?.Invoke(file);
		return file;
	}

	static IAsyncEnumerable<string> MergeIncludesAsync(
		string rootFileName,
		Dictionary<string, FileInfo> registry,
		MergeOptions? options = null,
		Action<FileInfo>? onFileAccessed = null,
		HashSet<string>? active = null)
	{
		rootFileName.ThrowIfNull().OnlyInDebug();
		registry.ThrowIfNull().OnlyInDebug();
		Contract.EndContractBlock();

		var root = Register(rootFileName, registry, onFileAccessed);

		active ??= new();

		return MergeIncludesAsyncCore(root, registry, options, active, onFileAccessed);

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

			var includePattern = GetIncludePattern();
			using var file = root.OpenRead();
			using var reader = new StreamReader(file);
			Lazy<string> path = new(() => root.Directory?.FullName.ThrowIfNull()!);
			var trimming = options?.Trim == true;
			var trimLeading = trimming;
			var lineNumber = 0;
			List<string>? whiteSpace = trimming ? new List<string>() : null;

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

			if(trimming && whiteSpace!.Count != 0)
			{
				foreach (var w in whiteSpace)
					yield return w;

				whiteSpace.Clear();
			}

			var include = includePattern.Match(line);
			if (!include.Success)
			{
				yield return line;
				goto more;
			}

			var includePath = Path.Combine(path.Value, include.Groups[FILE].Value);
			if (active.Contains(includePath))
				throw new InvalidOperationException($"Detected recursive reference to {includePath}.");

			var require = include.Groups[METHOD].ValueSpan.Equals(REQUIRE, StringComparison.OrdinalIgnoreCase);
			// Require means to only include it once.
			if (require && registry.ContainsKey(includePath))
				goto more;

			IAsyncEnumerable<string> included;
			try
			{
				included = MergeIncludesAsync(includePath, registry, options, onFileAccessed, active);
			}
			catch (FileNotFoundException ex)
			{
				throw new FileNotFoundException($"Could not find include file on line {lineNumber} in {root.FullName}", includePath, ex);
			}

			await foreach (var n in included)
			{
				yield return n;
			}

			trimLeading = trimming;

			goto more;
		}
	}
}
