using System.Diagnostics;
using System.Text.RegularExpressions;
using Throw;

namespace MergeIncludes;
public static partial class Extensions
{
	const string FILE = "file";
	[GeneratedRegex(@$"^(//\s*)?#include\s+(?<{FILE}>.+)", RegexOptions.Compiled)]
	private static partial Regex GetIncludePattern();

	public static IAsyncEnumerable<string> MergeIncludesAsync(
		this FileInfo root,
		HashSet<string>? includes = null)
	{
		if (!root.Exists)
			throw new FileNotFoundException(root.FullName);

		includes ??= new();
		if (includes.Contains(root.FullName))
			throw new InvalidOperationException($"Detected recursive reference to {root.FullName}.");

		return MergeIncludesAsyncCore(root, includes);

		static async IAsyncEnumerable<string> MergeIncludesAsyncCore(
			FileInfo root,
			HashSet<string> includes)
		{
			var rootFileName = root.FullName;
			if (!includes.Add(rootFileName))
				throw new UnreachableException();

			var includePattern = GetIncludePattern();
			using var file = root.OpenRead();
			using var reader = new StreamReader(file);
			Lazy<string> path = new(() => root.Directory?.FullName.ThrowIfNull()!);
			var lineNumber = 0;

		more:
			var line = await reader.ReadLineAsync().ConfigureAwait(false);
			if (line is null)
			{
				includes.Remove(rootFileName);
				yield break;
			}

			lineNumber++;

			var include = includePattern.Match(line);
			if (!include.Success)
			{
				yield return line;
				goto more;
			}

			var includePath = Path.Combine(path.Value, include.Groups[FILE].Value);
			IAsyncEnumerable<string> included;
			try
			{
				included = new FileInfo(includePath).MergeIncludesAsync(includes);
			}
			catch(FileNotFoundException ex)
			{
				throw new FileNotFoundException($"Could not find include file on line {lineNumber} in {root.FullName}", includePath, ex);
			}

			await foreach (var n in included)
			{
				yield return n;
			}

			goto more;
		}
	}
}
