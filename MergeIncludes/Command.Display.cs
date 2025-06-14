using Spectre.Console;
using System.Linq;

namespace MergeIncludes;

public sealed partial class CombineCommand
{
	/// <summary>
	/// Displays the tree with simple filenames and IDs for all files
	/// </summary>
	private void DisplaySimpleFileTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		try 
		{
			// Create a tree with the root file as the root node
			var rootName = Path.GetFileName(rootFile.FullName);

			// Create file ID map and assign the root file ID 0
			var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			var nextId = 0;
			fileIds[rootFile.FullName] = nextId++;

			// Format ID with the appropriate number of digits based on total files
			int totalFiles = CountAllFilesInTree(rootFile, fileRelationships);
			string idFormat = GetIdFormat(totalFiles);

			var rootText = new Text($"{rootName} {string.Format(idFormat, fileIds[rootFile.FullName])}", Color.LightSkyBlue1);
			var tree = new Tree(rootText)
			{
				Guide = TreeGuide.Line
			};

			// Recursively build the tree
			if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
			{
				// Filter out any self-references
				var validChildren = children.Where(path => 
					!string.IsNullOrEmpty(path) && 
					!string.Equals(path, rootFile.FullName, StringComparison.OrdinalIgnoreCase)).ToList();
					
				foreach (var childPath in validChildren)
				{
					var fileName = Path.GetFileName(childPath);

					// Check if this file has been seen before
					if (!fileIds.TryGetValue(childPath, out int fileId))
					{
						// If not, assign it a new ID
						fileId = nextId++;
						fileIds[childPath] = fileId;
					}

					// Display the ID for this file
					string displayText = $"{fileName} {string.Format(idFormat, fileId)}";

					// Use yellow for repeats - those that would be encountered again
					bool isRepeat = IsFileRepeatedInRelationships(childPath, fileRelationships);
					var style = isRepeat ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);

					var childNode = tree.AddNode(new Text(displayText, style));

					// Continue building the tree with cycle detection
					var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootFile.FullName };
					if (fileRelationships.ContainsKey(childPath))
					{
						AddChildrenToSimpleTree(childNode, childPath, fileRelationships, fileIds, ref nextId, idFormat, visitedPaths);
					}
				}
			}

			// Write the tree to the console - match the exact padding from the test files
			var panel = new Panel(tree)
			{
				Header = new PanelHeader("[white]Files included in merge:[/]"),
				Expand = true,
				Border = BoxBorder.Rounded,
				Padding = new Padding(1, 0, 1, 0)
			};
			_console.Write(panel);
		}
		catch (Exception ex)
		{
			_console.MarkupLine($"[red]Error displaying simple file tree:[/] {ex.Message}");
		}
	}

	/// <summary>
	/// Displays the tree showing only IDs for files that are repeated
	/// </summary>
	private void DisplayRepeatsOnlyFileTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		try 
		{
			// Create a tree with the root file as the root node
			var rootName = Path.GetFileName(rootFile.FullName);

			// First pass: identify all repeated files
			var repeatedFiles = FindRepeatedFiles(rootFile, fileRelationships);

			// Create file ID map - only assign IDs to files that are repeated
			var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			// If root is repeated, assign it ID 0
			if (repeatedFiles.Contains(rootFile.FullName))
			{
				fileIds[rootFile.FullName] = 0;
			}

			// Start assigning IDs for other repeated files at 1
			var nextId = 1;

			// Assign IDs to repeated files, skipping the root file which was already handled
			foreach (var repeatedFile in repeatedFiles)
			{
				if (string.Equals(repeatedFile, rootFile.FullName, StringComparison.OrdinalIgnoreCase))
					continue; // Skip root file as we already assigned it ID 0

				fileIds[repeatedFile] = nextId++;
			}

			// Format ID with the appropriate number of digits based on number of repeated files
			string idFormat = GetIdFormat(repeatedFiles.Count > 0 ? repeatedFiles.Count : 1);

			// Don't show ID for root file unless it's repeated elsewhere in the tree
			string rootDisplay = repeatedFiles.Contains(rootFile.FullName) ?
				$"{rootName} {string.Format(idFormat, fileIds[rootFile.FullName])}" : rootName;

			var rootText = new Text(rootDisplay, Color.LightSkyBlue1);
			var tree = new Tree(rootText)
			{
				Guide = TreeGuide.Line
			};

			// Recursively build the tree
			if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
			{
				// Filter out any self-references
				var validChildren = children.Where(path => 
					!string.IsNullOrEmpty(path) && 
					!string.Equals(path, rootFile.FullName, StringComparison.OrdinalIgnoreCase)).ToList();
				
				foreach (var childPath in validChildren)
				{
					var fileName = Path.GetFileName(childPath);

					// Check if this file is repeated
					bool isRepeated = repeatedFiles.Contains(childPath);

					// Display text - only include ID if the file is repeated
					string displayText = isRepeated && fileIds.TryGetValue(childPath, out int fileId) ?
						$"{fileName} {string.Format(idFormat, fileId)}" : fileName;

					// Use yellow for repeated files
					var style = isRepeated ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);

					var childNode = tree.AddNode(new Text(displayText, style));

					// Continue building the tree with cycle detection
					var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootFile.FullName };
					if (fileRelationships.ContainsKey(childPath))
					{
						AddChildrenToRepeatsOnlyTree(childNode, childPath, fileRelationships, fileIds, repeatedFiles, idFormat, visitedPaths);
					}
				}
			}

			// Write the tree to the console - match the exact padding from the test files
			var panel = new Panel(tree)
			{
				Header = new PanelHeader("[white]Files included in merge:[/]"),
				Expand = true,
				Border = BoxBorder.Rounded,
				Padding = new Padding(1, 0, 1, 0)
			};
			_console.Write(panel);
		}
		catch (Exception ex)
		{
			_console.MarkupLine($"[red]Error displaying repeats-only file tree:[/] {ex.Message}");
		}
	}

	/// <summary>
	/// Displays a tree with files showing folder labels
	/// </summary>
	private void DisplayFolderGroupedTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		try
		{
			// Get the base directory of the root file to calculate relative paths
			var baseDirectory = rootFile.Directory ?? throw new InvalidOperationException("Root file directory cannot be null");

			// Get the workspace root directory
			var workspaceRoot = GetWorkspaceRoot(baseDirectory);

			// First pass: identify all repeated files - use the safer implementation now
			var repeatedFiles = FindRepeatedFiles(rootFile, fileRelationships);

			// Create file ID map - only assign IDs for repeated files 
			var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			// If root is repeated, always assign it ID 0
			if (repeatedFiles.Contains(rootFile.FullName))
			{
				fileIds[rootFile.FullName] = 0;
			}

			// Start assigning IDs at 1 for other repeated files
			var nextId = 1;

			// Assign IDs to repeated files, skipping the root file
			foreach (var repeatedFile in repeatedFiles)
			{
				if (string.Equals(repeatedFile, rootFile.FullName, StringComparison.OrdinalIgnoreCase))
					continue; // Skip root file already handled

				fileIds[repeatedFile] = nextId++;
			}

			// Format ID with the appropriate number of digits
			string idFormat = GetIdFormat(Math.Max(repeatedFiles.Count, 1));

			// Root node display
			var rootName = Path.GetFileName(rootFile.FullName);
			string rootDisplay = repeatedFiles.Contains(rootFile.FullName) ?
				$"{rootName} {string.Format(idFormat, 0)}" : rootName;

			var rootText = new Text(rootDisplay, Color.LightSkyBlue1);
			var tree = new Tree(rootText)
			{
				Guide = TreeGuide.Line
			};

			// Get root directory canonical path - safely
			string rootDirPath;
			try
			{
				rootDirPath = GetCanonicalPath(rootFile.DirectoryName ?? string.Empty);
			}
			catch
			{
				// Use a fallback if canonical path fails
				rootDirPath = rootFile.DirectoryName?.ToLowerInvariant() ?? string.Empty;
			}

			// Use case-insensitive dictionary for directory nodes
			var directoryNodesMap = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);

			// Process children
			if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
			{
				// First, check and remove any self-references in the root's children
				List<string> validChildren = children
				    .Where(path => !string.IsNullOrEmpty(path) && 
				                  !string.Equals(path, rootFile.FullName, StringComparison.OrdinalIgnoreCase))
				    .ToList();
				
				// Group files by directory using canonical paths
				var filesByDirectory = new Dictionary<string, List<FileInfo>>(StringComparer.OrdinalIgnoreCase);

				// First pass - group files by their canonical directory paths
				foreach (var childPath in validChildren)
				{
					try
					{
						var fileInfo = new FileInfo(childPath);
						var dirPath = GetCanonicalPath(fileInfo.DirectoryName ?? string.Empty);

						if (!filesByDirectory.TryGetValue(dirPath, out var fileList))
						{
							fileList = new List<FileInfo>();
							filesByDirectory[dirPath] = fileList;
						}

						filesByDirectory[dirPath].Add(fileInfo);
					}
					catch (Exception ex)
					{
						// Log or handle the error - use a safe fallback
						var fileName = Path.GetFileName(childPath);
						var style = new Style(Color.Red);
						var errorNode = tree.AddNode(new Text($"{fileName ?? "Unknown"} (Error: {ex.Message})", style));
					}
				}

				try
				{
					// Process each directory group
					foreach (var dirEntry in filesByDirectory)
					{
						var dirPath = dirEntry.Key;
						var dirFiles = dirEntry.Value;

						// Skip root directory as we don't need a folder header for it
						if (string.Equals(dirPath, rootDirPath, StringComparison.OrdinalIgnoreCase))
						{
							// Files in the same directory as root - just add them directly
							foreach (var fileInfo in dirFiles)
							{
								try
								{
									var fileName = fileInfo.Name;
									bool isRepeated = repeatedFiles.Contains(fileInfo.FullName);

									string displayText = isRepeated && fileIds.TryGetValue(fileInfo.FullName, out int fileId) ?
										$"{fileName} {string.Format(idFormat, fileId)}" : fileName;

									var style = isRepeated ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
									var fileNode = tree.AddNode(new Text(displayText, style));

									// Add children recursively - with cycle tracking
									// Initialize visited paths with the root file to prevent cycles back to root
									var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootFile.FullName };
									
									if (fileRelationships.ContainsKey(fileInfo.FullName))
									{
										AddChildrenToFolderGroupedTree(fileNode, fileInfo.FullName, fileRelationships,
											workspaceRoot, fileIds, repeatedFiles, directoryNodesMap, idFormat, visitedPaths);
									}
								}
								catch (Exception ex)
								{
									var style = new Style(Color.Red);
									var errorNode = tree.AddNode(new Text($"Error processing file: {ex.Message}", style));
								}
							}
						}
						else
						{
							try
							{
								// Create a folder header for this directory
								TreeNode folderNode;

								if (directoryNodesMap.TryGetValue(dirPath, out var existingNode))
								{
									// Use existing folder node
									folderNode = existingNode;
								}
								else
								{
									// Create a new folder header
									DirectoryInfo dirInfo;
									try
									{
										dirInfo = new DirectoryInfo(dirPath);
									}
									catch
									{
										// Fallback for invalid paths
										dirInfo = new DirectoryInfo(Path.GetDirectoryName(rootFile.FullName) ?? ".");
									}

									string folderName;
									try
									{
										folderName = GetProjectRelativePath(dirInfo, workspaceRoot);
									}
									catch
									{
										// Fallback if path calculation fails
										folderName = dirPath;
									}

									// Use [DIR] instead of emoji for better console compatibility
									var folderText = new Text($"[DIR] {folderName}", new Style(Color.Yellow3));
									folderNode = tree.AddNode(folderText);
									directoryNodesMap[dirPath] = folderNode;
								}

								// Add files under the folder header
								foreach (var fileInfo in dirFiles)
								{
									try
									{
										var fileName = fileInfo.Name;
										bool isRepeated = repeatedFiles.Contains(fileInfo.FullName);

										string displayText = isRepeated && fileIds.TryGetValue(fileInfo.FullName, out int fileId) ?
											$"{fileName} {string.Format(idFormat, fileId)}" : fileName;

										var style = isRepeated ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);
										var fileNode = folderNode.AddNode(new Text(displayText, style));

										// Add children recursively - with cycle tracking
										// Initialize visited paths with the root file to prevent cycles back to root
										var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootFile.FullName };
										
										if (fileRelationships.ContainsKey(fileInfo.FullName))
										{
											AddChildrenToFolderGroupedTree(fileNode, fileInfo.FullName, fileRelationships,
												workspaceRoot, fileIds, repeatedFiles, directoryNodesMap, idFormat, visitedPaths);
										}
									}
									catch (Exception ex)
									{
										var style = new Style(Color.Red);
										var errorNode = folderNode.AddNode(new Text($"Error processing file: {ex.Message}", style));
									}
								}
							}
							catch (Exception ex)
							{
								var style = new Style(Color.Red);
								var errorNode = tree.AddNode(new Text($"Error processing directory: {ex.Message}", style));
							}
						}
					}
				}
				catch (Exception ex)
				{
					_console.MarkupLine($"[red]Error processing directory groups:[/] {ex.Message}");
				}
			}

			// Write the tree to the console
			var panel = new Panel(tree)
			{
				Header = new PanelHeader("[white]Files included in merge:[/]"),
				Expand = true,
				Border = BoxBorder.Rounded,
				Padding = new Padding(1, 0, 1, 0)
			};
			_console.Write(panel);
		}
		catch (Exception ex)
		{
			_console.MarkupLine($"[red]Error displaying folder grouped tree:[/] {ex.Message}");
			// Fallback to a simpler display mode
			try
			{
				DisplayRepeatsOnlyFileTree(rootFile, fileRelationships);
			}
			catch
			{
				_console.MarkupLine("[red]Could not display file tree due to an error.[/]");
			}
		}
	}

	/// <summary>
	/// Displays the tree with folder structure information
	/// </summary>
	private void DisplayFolderStructureTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		try
		{
			// Get the base directory of the root file to calculate relative paths
			var baseDirectory = rootFile.Directory ?? throw new InvalidOperationException("Root file directory cannot be null");

			// Get the workspace root directory (try to detect project structure)
			var workspaceRoot = GetWorkspaceRoot(baseDirectory);

			// Create a tree with the root file as the root node
			var rootName = rootFile.Name;
			var projectFolder = GetProjectRelativePath(rootFile.Directory, workspaceRoot);

			// Create file ID map and assign the root file ID 0
			var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			var nextId = 0;
			fileIds[rootFile.FullName] = nextId++;

			// Format ID with the appropriate number of digits based on total files
			int totalFiles = CountAllFilesInTree(rootFile, fileRelationships);
			string idFormat = GetIdFormat(totalFiles);

			// Create a styled text node for the root
			var rootText = new Text($"{rootName} {string.Format(idFormat, fileIds[rootFile.FullName])} (in {projectFolder})", Color.LightSkyBlue1);
			var tree = new Tree(rootText)
			{
				Guide = TreeGuide.Line
			};

			// Recursively build the tree
			if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
			{
				// Filter out any self-references
				var validChildren = children.Where(path => 
					!string.IsNullOrEmpty(path) && 
					!string.Equals(path, rootFile.FullName, StringComparison.OrdinalIgnoreCase)).ToList();
				
				foreach (var childPath in validChildren)
				{
					var fileInfo = new FileInfo(childPath);
					var folderName = GetProjectRelativePath(fileInfo.Directory, workspaceRoot);

					// Check if this file has been seen before
					if (!fileIds.TryGetValue(childPath, out int fileId))
					{
						// If not, assign it a new ID
						fileId = nextId++;
						fileIds[childPath] = fileId;
					}

					// Display the ID for this file along with folder info
					string displayText = $"{fileInfo.Name} {string.Format(idFormat, fileId)} (in {folderName})";

					// Use yellow for repeats - those that would be encountered again
					bool isRepeat = IsFileRepeatedInRelationships(childPath, fileRelationships);
					var style = isRepeat ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);

					var childNode = tree.AddNode(new Text(displayText, style));

					// Continue building the tree with cycle detection
					var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootFile.FullName };
					if (fileRelationships.ContainsKey(childPath))
					{
						AddChildrenToFolderTreeWithText(childNode, childPath, fileRelationships, workspaceRoot, fileIds, ref nextId, idFormat, visitedPaths);
					}
				}
			}

			// Write the tree to the console - match the exact padding from the test files
			var panel = new Panel(tree)
			{
				Header = new PanelHeader("[white]Files included in merge with folder structure:[/]"),
				Expand = true,
				Border = BoxBorder.Rounded,
				Padding = new Padding(1, 0, 1, 0)
			};
			_console.Write(panel);
		}
		catch (Exception ex)
		{
			_console.MarkupLine($"[red]Error displaying folder structure tree:[/] {ex.Message}");
		}
	}

	/// <summary>
	/// Displays the tree with full path information for each file
	/// </summary>
	private void DisplayFullPathsTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		try
		{
			// Get the base directory of the root file to calculate relative paths
			var baseDirectory = rootFile.Directory ?? throw new InvalidOperationException("Root file directory cannot be null");

			// Create file ID map and assign the root file ID 0
			var fileIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			var nextId = 0;
			fileIds[rootFile.FullName] = nextId++;

			// Format ID with the appropriate number of digits based on total files
			int totalFiles = CountAllFilesInTree(rootFile, fileRelationships);
			string idFormat = GetIdFormat(totalFiles);

			// Create a tree with the root file as the root node - using just the name for the root
			var rootText = new Text($"{rootFile.Name} {string.Format(idFormat, fileIds[rootFile.FullName])}", Color.LightSkyBlue1);
			var tree = new Tree(rootText)
			{
				Guide = TreeGuide.Line
			};

			// Recursively build the tree
			if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
			{
				// Filter out any self-references
				var validChildren = children.Where(path => 
					!string.IsNullOrEmpty(path) && 
					!string.Equals(path, rootFile.FullName, StringComparison.OrdinalIgnoreCase)).ToList();
				
				foreach (var childPath in validChildren)
				{
					var childFile = new FileInfo(childPath);
					// Get a path relative to the root file directory
					var relativePath = GetRelativeFilePath(childFile.FullName, baseDirectory.FullName);

					// Check if this file has been seen before
					if (!fileIds.TryGetValue(childPath, out int fileId))
					{
						// If not, assign it a new ID
						fileId = nextId++;
						fileIds[childPath] = fileId;
					}

					// Add the ID to the display text
					string displayText;
					if (relativePath.Contains(" (in "))
					{
						var parts = relativePath.Split(new[] { " (in " }, 2, StringSplitOptions.None);
						displayText = $"{parts[0]} {string.Format(idFormat, fileId)} (in {parts[1]}";
					}
					else
					{
						displayText = $"{relativePath} {string.Format(idFormat, fileId)}";
					}

					// Use yellow for repeats - those that would be encountered again
					bool isRepeat = IsFileRepeatedInRelationships(childPath, fileRelationships);
					var style = isRepeat ? new Style(Color.Yellow) : new Style(Color.PaleTurquoise1);

					var text = new Text(displayText, style);
					var childNode = tree.AddNode(text);

					// Continue building the tree with cycle detection
					var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootFile.FullName };
					if (fileRelationships.ContainsKey(childPath))
					{
						AddChildrenToRelativePathTreeWithText(childNode, childPath, fileRelationships, baseDirectory.FullName, fileIds, ref nextId, idFormat, visitedPaths);
					}
				}
			}

			// Write the tree to the console - match the exact padding from the test files
			var panel = new Panel(tree)
			{
				Header = new PanelHeader("[white]Files included in merge with paths:[/]"),
				Expand = true,
				Border = BoxBorder.Rounded,
				Padding = new Padding(1, 0, 1, 0)
			};
			_console.Write(panel);
		}
		catch (Exception ex)
		{
			_console.MarkupLine($"[red]Error displaying full paths tree:[/] {ex.Message}");
		}
	}
}