using System.Collections.Concurrent;

namespace MergeIncludes.Optimization;

/// <summary>
/// Caches frequently computed file path operations to avoid repeated allocations
/// </summary>
internal static class PathCache
{
	// Use concurrent dictionaries for thread safety during parallel operations
	private static readonly ConcurrentDictionary<string, string> FullPathCache = new();
	private static readonly ConcurrentDictionary<string, string> RelativePathCache = new();
	private static readonly ConcurrentDictionary<string, DirectoryInfo> DirectoryCache = new();

	/// <summary>
	/// Gets or computes the full path for a file, using cache for repeated requests
	/// </summary>
	public static string GetFullPath(string path)
	{
		return FullPathCache.GetOrAdd(path, Path.GetFullPath);
	}

	/// <summary>
	/// Gets or computes a relative path between two directories, with caching
	/// </summary>
	public static string GetRelativePath(string relativeTo, string path)
	{
		var key = $"{relativeTo}|{path}";
		return RelativePathCache.GetOrAdd(key, _ => Path.GetRelativePath(relativeTo, path));
	}

	/// <summary>
	/// Gets or computes DirectoryInfo with caching to avoid repeated FileInfo operations
	/// </summary>
	public static DirectoryInfo GetDirectory(string path)
	{
		return DirectoryCache.GetOrAdd(path, p => new DirectoryInfo(Path.GetDirectoryName(p) ?? p));
	}

	/// <summary>
	/// Clears all caches - useful for testing or memory management
	/// </summary>
	public static void ClearAll()
	{
		FullPathCache.Clear();
		RelativePathCache.Clear();
		DirectoryCache.Clear();
	}
}
