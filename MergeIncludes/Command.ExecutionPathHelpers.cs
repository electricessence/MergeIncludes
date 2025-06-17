using System.Reflection;

namespace MergeIncludes;

public sealed partial class CombineCommand
{
	/// <summary>
	/// Gets the directory from which the application is executing
	/// </summary>
	/// <returns>The execution directory path</returns>
	public static string GetExecutionDirectory()
	{
		try
		{
			// Best option for .NET 6+ including single-file deployments
			if (!string.IsNullOrEmpty(Environment.ProcessPath))
			{
				return Path.GetDirectoryName(Environment.ProcessPath) ?? Directory.GetCurrentDirectory();
			}

			// Fallback to AppContext.BaseDirectory which is reliable in most modern .NET apps
			// even when Assembly.Location returns empty string (like in single-file apps)
			string? baseDirectory = AppContext.BaseDirectory;
			if (!string.IsNullOrEmpty(baseDirectory))
			{
				return baseDirectory;
			}

			// Traditional approach - works in most non-single-file scenarios
			string? assemblyLocation = Assembly.GetEntryAssembly()?.Location;
			if (!string.IsNullOrEmpty(assemblyLocation))
			{
				return Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
			}

			// If all else fails, use the current directory
			return Directory.GetCurrentDirectory();
		}
		catch
		{
			// In case of any exceptions, fall back to current directory
			return Directory.GetCurrentDirectory();
		}
	}

	/// <summary>
	/// Makes a path relative to the execution directory if possible
	/// </summary>
	/// <param name="path">The absolute path to convert</param>
	/// <returns>A path relative to the execution directory, or the original path if not possible</returns>
	public static string MakePathRelativeToExecution(string path)
	{
		try
		{
			string executionDir = GetExecutionDirectory();
			string relativePath = Path.GetRelativePath(executionDir, path);

			// If path is not below execution dir, Path.GetRelativePath will return paths starting with ".."
			// For usability, we may want to return the original path in those cases
			if (relativePath.StartsWith("..") || Path.IsPathRooted(relativePath))
			{
				return path;
			}

			return relativePath;
		}
		catch
		{
			// If anything fails, return the original path
			return path;
		}
	}

	/// <summary>
	/// Makes a file path relative to the execution directory with smart formatting
	/// </summary>
	/// <param name="file">The FileInfo object to get a relative path for</param>
	/// <returns>A user-friendly relative path</returns>
	public static string GetExecutionRelativePath(FileInfo file)
	{
		try
		{
			if (file == null)
				return "unknown file";

			string executionDir = GetExecutionDirectory();
			string relativePath = Path.GetRelativePath(executionDir, file.FullName);

			// If the path starts with ".." or is on a different drive, it's not below the execution directory
			if (relativePath.StartsWith("..") || Path.IsPathRooted(relativePath))
			{
				// For paths outside the execution directory, show them with a hint
				string fileName = file.Name;
				string parentDir = file.Directory?.Name ?? "unknown";
				return $"{fileName} (in {parentDir})";
			}

			// If the path is within the execution directory, return it as a relative path
			return relativePath;
		}
		catch
		{
			// If any exception occurs, return just the filename as a fallback
			return file.Name;
		}
	}
}