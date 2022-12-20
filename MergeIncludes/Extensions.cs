using System.Diagnostics;
using System.Diagnostics.Contracts;
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
		Action<FileInfo>? onFileAccessed = null)
	{
		root.ThrowIfNull();
		Contract.EndContractBlock();

		Dictionary<string, FileInfo> registry = new();
		Register(root.FullName, registry, onFileAccessed);
		return MergeIncludesAsync(root.FullName, registry, onFileAccessed);
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
		Action<FileInfo>? onFileAccessed = null,
		HashSet<string>? active = null)
	{
		rootFileName.ThrowIfNull().OnlyInDebug();
		registry.ThrowIfNull().OnlyInDebug();
		Contract.EndContractBlock();

		var root = Register(rootFileName, registry, onFileAccessed);

		active ??= new();
		if (active.Contains(root.FullName))
			throw new InvalidOperationException($"Detected recursive reference to {root.FullName}.");

		return MergeIncludesAsyncCore(root, registry, active, onFileAccessed);

		static async IAsyncEnumerable<string> MergeIncludesAsyncCore(
			FileInfo root,
			Dictionary<string, FileInfo> registry,
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
			var lineNumber = 0;

		more:
			var line = await reader.ReadLineAsync().ConfigureAwait(false);
			if (line is null)
			{
				active.Remove(rootFileName);
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
				included = MergeIncludesAsync(includePath, registry, onFileAccessed);
			}
			catch (FileNotFoundException ex)
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
