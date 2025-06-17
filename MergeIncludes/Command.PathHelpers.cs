namespace MergeIncludes;

public sealed partial class CombineCommand
{
	/// <summary>
	/// Gets a user-friendly representation of the folder path
	/// </summary>
	private static string GetRelativeFolderPath(DirectoryInfo? directory, DirectoryInfo? baseDirectory = null)
	{
		if (directory == null)
			return "unknown location";

		if (baseDirectory == null)
			return directory.Name;

		// Try to get a relative path
		try
		{
			string relativePath = Path.GetRelativePath(baseDirectory.FullName, directory.FullName);
			// If the path starts with "..", we've gone above the base directory
			if (relativePath.StartsWith(".."))
			{
				// Just use the directory name
				return directory.Name;
			}

			if (relativePath == ".")
				return baseDirectory.Name;

			return relativePath;
		}
		catch
		{
			return directory.Name;
		}
	}

	/// <summary>
	/// Tries to find the workspace or project root directory based on common project files
	/// </summary>
	private static DirectoryInfo GetWorkspaceRoot(DirectoryInfo startDirectory)
	{
		var currentDir = startDirectory;

		// Try to locate common project indicators going up the directory tree
		while (currentDir != null && currentDir.Parent != null)
		{
			// Check for common project/solution files
			if (currentDir.EnumerateFiles("*.sln").Any() ||
				currentDir.EnumerateFiles("*.csproj").Any() ||
				currentDir.EnumerateFiles("*.fsproj").Any() ||
				currentDir.EnumerateFiles("*.vbproj").Any() ||
				currentDir.EnumerateDirectories(".git").Any() ||
				currentDir.EnumerateFiles("package.json").Any())
			{
				return currentDir;
			}

			// Go up one level
			currentDir = currentDir.Parent;
		}

		// If we couldn't find a project root, just return the original directory
		return startDirectory;
	}

	/// <summary>
	/// Gets a project-relative path that's more descriptive than just the immediate directory
	/// </summary>
	private static string GetProjectRelativePath(DirectoryInfo? directory, DirectoryInfo workspaceRoot)
	{
		if (directory == null)
			return "unknown location";

		try
		{
			string relativePath = Path.GetRelativePath(workspaceRoot.FullName, directory.FullName);

			// If we're at or below the project root
			if (!relativePath.StartsWith(".."))
			{
				if (relativePath == ".")
					return workspaceRoot.Name;

				// Get a meaningful name by finding the first or second level directories
				var pathParts = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

				if (pathParts.Length == 0)
					return workspaceRoot.Name;

				if (pathParts.Length == 1)
					return pathParts[0];

				// Show the first two directory levels if they exist
				return $"{pathParts[0]}/{pathParts[1]}";
			}

			// If we're above the project root, just return the directory name
			return directory.Name;
		}
		catch
		{
			return directory.Name;
		}
	}

	/// <summary>
	/// Gets a user-friendly representation of a file path relative to a base directory
	/// </summary>
	/// <param name="filePath">The full path of the file</param>
	/// <param name="baseDirectoryPath">The base directory to make the path relative to</param>
	/// <returns>A user-friendly relative path</returns>
	private static string GetRelativeFilePath(string filePath, string baseDirectoryPath)
	{
		try
		{
			var relativePath = Path.GetRelativePath(baseDirectoryPath, filePath);

			// If the path starts with "..", it means the file is outside the base directory
			if (relativePath.StartsWith(".."))
			{
				// Try to find a common ancestor by going up in the directory hierarchy
				var fileDir = Path.GetDirectoryName(filePath);
				var baseDir = baseDirectoryPath;
				var filePathComponents = fileDir?.Split(Path.DirectorySeparatorChar) ?? [];
				var baseDirComponents = baseDir.Split(Path.DirectorySeparatorChar);

				// Find the common prefix
				int commonPrefixLength = 0;
				int minLength = Math.Min(filePathComponents.Length, baseDirComponents.Length);

				for (int i = 0; i < minLength; i++)
				{
					if (string.Equals(filePathComponents[i], baseDirComponents[i], StringComparison.OrdinalIgnoreCase))
						commonPrefixLength++;
					else
						break;
				}

				if (commonPrefixLength > 0)
				{
					// Build a path showing how to get from common ancestor to file
					var commonAncestor = string.Join(Path.DirectorySeparatorChar.ToString(),
						filePathComponents.Take(commonPrefixLength));

					// Calculate path segments from common ancestor to file
					var relativeSegments = new List<string>();

					// Add ".." for each segment from base dir to common ancestor
					int baseDirToCommonAncestorSteps = baseDirComponents.Length - commonPrefixLength;
					for (int i = 0; i < baseDirToCommonAncestorSteps; i++)
					{
						relativeSegments.Add("..");
					}

					// Add segments from common ancestor to file
					for (int i = commonPrefixLength; i < filePathComponents.Length; i++)
					{
						relativeSegments.Add(filePathComponents[i]);
					}

					// Add the filename
					relativeSegments.Add(Path.GetFileName(filePath));

					return string.Join(Path.DirectorySeparatorChar.ToString(), relativeSegments);
				}

				// If no common ancestor (different drives), use just the filename with a hint
				if (Path.GetPathRoot(filePath) != Path.GetPathRoot(baseDirectoryPath))
					return $"{Path.GetFileName(filePath)} (in different drive)";

				// If we can't compute a relative path, just return the filename with its immediate parent
				var parentDir = Path.GetFileName(Path.GetDirectoryName(filePath) ?? "");
				return $"{Path.GetFileName(filePath)} (in {parentDir})";
			}

			// Normal case - file is within base directory hierarchy
			return relativePath;
		}
		catch
		{
			// If any exception occurs, just return the file name as a fallback
			return Path.GetFileName(filePath);
		}
	}

	/// <summary>
	/// Gets a user-friendly representation of a file path relative to the execution directory
	/// </summary>
	/// <param name="file">The file to get a relative path for</param>
	/// <param name="useWorkspaceIfNearer">Whether to use the workspace root as base if it's nearer than execution dir</param>
	/// <returns>A user-friendly relative path from execution directory</returns>
	private static string GetExecutionRelativeFilePath(FileInfo file, bool useWorkspaceIfNearer = true)
	{
		try
		{
			if (file == null)
				return "unknown file";

			string executionDir = GetExecutionDirectory();
			string executionRelative = Path.GetRelativePath(executionDir, file.FullName);

			// If it's not below execution directory or we want to try workspace root
			if (useWorkspaceIfNearer && (executionRelative.StartsWith("..") || Path.IsPathRooted(executionRelative)))
			{
				// Try using the workspace root as an alternative
				var workspaceRoot = GetWorkspaceRoot(file.Directory ?? new DirectoryInfo(Directory.GetCurrentDirectory()));
				string workspaceRelative = Path.GetRelativePath(workspaceRoot.FullName, file.FullName);
				// If the workspace path is shorter and not going outside the workspace
				if (!workspaceRelative.StartsWith("..") &&
					(workspaceRelative.Length < executionRelative.Length || executionRelative.StartsWith("..")))
				{
					return $"./{workspaceRelative}";
				}
			}

			// If within execution directory, just return the relative path with ./ prefix
			if (!executionRelative.StartsWith("..") && !Path.IsPathRooted(executionRelative))
			{
				return $"./{executionRelative}";
			}

			// Otherwise fall back to the standard relative path method
			return GetRelativeFilePath(file.FullName, Path.GetDirectoryName(file.FullName) ?? "");
		}
		catch
		{
			// If any exception occurs, return just the filename as a fallback
			return file.Name;
		}
	}
}