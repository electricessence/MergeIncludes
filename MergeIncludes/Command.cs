using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Extensions;
using System.Diagnostics.CodeAnalysis;
using Throw;

namespace MergeIncludes;

public sealed class CombineCommand : AsyncCommand<Settings>
{
	private readonly IAnsiConsole _console;

	public CombineCommand() : this(AnsiConsole.Console)
	{
	}

	public CombineCommand(IAnsiConsole console)
	{
		_console = console ?? throw new ArgumentNullException(nameof(console));
	}

	public override ValidationResult Validate(
		[NotNull] CommandContext context,
		[NotNull] Settings settings)
	{
		var rootPath = settings.RootFilePath;
		if (string.IsNullOrWhiteSpace(rootPath)) return ValidationResult.Error("Root file path can't be empty or white space.");
		if (!File.Exists(rootPath)) return ValidationResult.Error("Root file not found.");

		settings.Trim ??= true;

		return base.Validate(context, settings);
	}

	public override async Task<int> ExecuteAsync(
		[NotNull] CommandContext __,
		[NotNull] Settings o)
	{
		o.ThrowIfNull().OnlyInDebug();
		var rootFile = o.GetRootFile();
		var outputFile = o.GetOutputFile(rootFile);

		var files = await Merge();

		if (!o.Watch) return 0;

		using var cancelSource = new CancellationTokenSource();
		var token = cancelSource.Token;
		_ = Task.Run(async () =>
			await _console.Status()
				.StartAsync("[turquoise2]Watching files for changes. (Press any key to stop watching.)[/]",
				async _ =>
				{
					while (!token.IsCancellationRequested)
					{
						try
						{
							int count = 0;
							await foreach (var file in FileWatcher.WatchAsync(files, 1000, token))
							{
								if (count++ == 0)
								{
									var now = DateTimeOffset.Now;
									_console.MarkupLine($"[yellow]Changes detected:[/] ({now:d} {now:T})");
								}

								_console.Write(new TextPath(file));
							}

							_console.WriteLine();

							if (token.IsCancellationRequested)
								return;

							files = await Merge();
						}
						catch (OperationCanceledException)
						{
							return;
						}
						catch (FileNotFoundException ex)
						{
							ex.WriteToConsole(console: _console);
						}
						catch (Exception ex)
						{
							ex.WriteToConsole(console: _console);
						}
					}
				}));

		Console.ReadKey();
		cancelSource.Cancel();

		return 0;

		async ValueTask<List<FileInfo>> Merge()
		{
			var existed = outputFile.Exists;
			if (existed)
				outputFile.Attributes &= ~FileAttributes.ReadOnly;

			var list = new List<FileInfo>();
			var fileRelationships = new Dictionary<string, List<string>>();
			
			// Stack to track the current inclusion path
			var includeStack = new Stack<string>();
			includeStack.Push(rootFile.FullName);

			// Use a MemoryStream to buffer the output instead of writing directly to the file
			using var memoryStream = new MemoryStream();
			using var writer = new StreamWriter(memoryStream);

			try
			{
				// Process the files and write to the memory buffer
				await foreach (var line in rootFile.MergeIncludesAsync(o, info =>
				{
					if (info.FullName == outputFile.FullName)
						throw new InvalidOperationException("Attempting to include the output file.");

					// Track all files for the flat list (used for watching)
					list.Add(info);
					
					// Get the parent file that included this file
					var parentFile = includeStack.Peek();
					
					// Record the parent-child relationship
					if (!fileRelationships.ContainsKey(parentFile))
					{
						fileRelationships[parentFile] = new List<string>();
					}
					
					fileRelationships[parentFile].Add(info.FullName);
					
					// Push this file onto the stack as it might include other files
					includeStack.Push(info.FullName);
					
					// Note: We don't pop from the stack here because we don't know when the
					// file processing is complete. The stack will accumulate the current branch
					// of file inclusions.
				}))
				{
					await writer.WriteLineAsync(line);
					
					// If we detect a line without an include statement, we can assume we've moved back up
					// in the file hierarchy by one level (this is a simplification but generally works)
					if (!line.Contains("#include") && !line.Contains("#require") && includeStack.Count > 1)
					{
						includeStack.Pop();
					}
				}

				await writer.FlushAsync();

				// If we got here, everything was successful, so now write to the actual file
				using var output = outputFile.Open(FileMode.Create, FileAccess.Write);
				memoryStream.Position = 0;
				await memoryStream.CopyToAsync(output);

				// Create and render the file tree based on the selected display mode
				DisplayFileTrees(rootFile, fileRelationships, o.DisplayMode);

				// Helps prevent accidental editing by user.
				if (existed)
					outputFile.Attributes |= FileAttributes.ReadOnly;
				else
					outputFile.Attributes = FileAttributes.ReadOnly;

				var mergePath = new TextPath(outputFile.FullName);
				_console.Write(new Panel(mergePath)
				{
					Header = new PanelHeader("[springgreen1]Successfully merged include references to:[/]")
				});

				return list;
			}
			catch
			{
				// If there was an error, we don't modify the output file
				// since we only wrote to the memory stream
				throw;
			}
		}
	}

	private void DisplayFileTrees(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships, TreeDisplayMode mode)
	{
		// Display trees based on the selected mode
		switch (mode)
		{
			case TreeDisplayMode.Simple:
				DisplaySimpleFileTree(rootFile, fileRelationships);
				break;
			
			case TreeDisplayMode.WithFolders:
				DisplayFolderStructureTree(rootFile, fileRelationships);
				break;
			
			case TreeDisplayMode.FullPaths:
				DisplayFullPathsTree(rootFile, fileRelationships);
				break;
			
			case TreeDisplayMode.Both:
				DisplaySimpleFileTree(rootFile, fileRelationships);
				DisplayFolderStructureTree(rootFile, fileRelationships);
				break;
			
			default:
				DisplaySimpleFileTree(rootFile, fileRelationships);
				break;
		}
	}

	private void DisplaySimpleFileTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Create a tree with the root file as the root node
		var rootName = Path.GetFileName(rootFile.FullName);
		var tree = new Tree(new Text(rootName, Color.LightSkyBlue1));
		tree.Guide = TreeGuide.Line;

		// Recursively build the tree
		if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
		{
			foreach (var childPath in children)
			{
				var fileName = Path.GetFileName(childPath);
				var childNode = tree.AddNode(new Text(fileName, Color.PaleTurquoise1));
				AddChildrenToSimpleTree(childNode, childPath, fileRelationships);
			}
		}

		// Write the tree to the console
		_console.Write(new Panel(tree)
		{
			Header = new PanelHeader("[white]Files included in merge:[/]"),
			Expand = true,
			Border = BoxBorder.Rounded
		});
	}

	private void DisplayFolderStructureTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Get the base directory of the root file to calculate relative paths
		var baseDirectory = rootFile.Directory ?? throw new InvalidOperationException("Root file directory cannot be null");
		
		// Create a tree with the root file as the root node - safely display without markup
		var rootName = rootFile.Name;
		var folderPath = GetRelativeFolderPath(rootFile.Directory);
		
		// Create a styled text node for the root
		var rootText = new Text($"{rootName} (in {folderPath})", Color.LightSkyBlue1);
		var tree = new Tree(rootText);
		tree.Guide = TreeGuide.Line;

		// Recursively build the tree
		if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
		{
			foreach (var childPath in children)
			{
				var fileInfo = new FileInfo(childPath);
				var folderName = GetRelativeFolderPath(fileInfo.Directory, baseDirectory);
				
				// Create styled text for the child
				var nodeText = new Text($"{fileInfo.Name} (in {folderName})", Color.PaleTurquoise1);
				var childNode = tree.AddNode(nodeText);
				AddChildrenToFolderTreeWithText(childNode, childPath, fileRelationships, baseDirectory);
			}
		}

		// Write the tree to the console
		_console.Write(new Panel(tree)
		{
			Header = new PanelHeader("[white]Files included in merge with folder structure:[/]"),
			Expand = true,
			Border = BoxBorder.Rounded
		});
	}

	private void DisplayFullPathsTree(FileInfo rootFile, Dictionary<string, List<string>> fileRelationships)
	{
		// Get the base directory of the root file to calculate relative paths
		var baseDirectory = rootFile.Directory ?? throw new InvalidOperationException("Root file directory cannot be null");
		
		// Create a tree with the root file as the root node - using just the name for the root
		var tree = new Tree(new Text(rootFile.Name, Color.LightSkyBlue1));
		tree.Guide = TreeGuide.Line;

		// Recursively build the tree
		if (fileRelationships.TryGetValue(rootFile.FullName, out var children))
		{
			foreach (var childPath in children)
			{
				var childFile = new FileInfo(childPath);
				// Get a path relative to the root file directory
				var relativePath = GetRelativeFilePath(childFile.FullName, baseDirectory.FullName);
				
				// Handle the special case with location info by creating a styled text
				if (relativePath.Contains(" (in "))
				{
					var parts = relativePath.Split(new[] { " (in " }, 2, StringSplitOptions.None);
					var fileName = parts[0];
					var locationInfo = parts.Length > 1 ? " (in " + parts[1] : "";
					
					var text = new Text(fileName + locationInfo, Color.PaleTurquoise1);
					var childNode = tree.AddNode(text);
					AddChildrenToRelativePathTreeWithText(childNode, childPath, fileRelationships, baseDirectory.FullName);
				}
				else
				{
					var text = new Text(relativePath, Color.PaleTurquoise1);
					var childNode = tree.AddNode(text);
					AddChildrenToRelativePathTreeWithText(childNode, childPath, fileRelationships, baseDirectory.FullName);
				}
			}
		}

		// Write the tree to the console
		_console.Write(new Panel(tree)
		{
			Header = new PanelHeader("[white]Files included in merge with paths:[/]"),
			Expand = true,
			Border = BoxBorder.Rounded
		});
	}

	private void AddChildrenToSimpleTree(TreeNode parentNode, string parentPath, Dictionary<string, List<string>> fileRelationships)
	{
		if (!fileRelationships.TryGetValue(parentPath, out var children))
		{
			return;
		}

		foreach (var childPath in children)
		{
			var fileName = Path.GetFileName(childPath);
			var childNode = parentNode.AddNode(new Text(fileName, Color.PaleTurquoise1));
			AddChildrenToSimpleTree(childNode, childPath, fileRelationships);
		}
	}

	private void AddChildrenToFolderTreeWithText(TreeNode parentNode, string parentPath, Dictionary<string, List<string>> fileRelationships, DirectoryInfo baseDirectory)
	{
		if (!fileRelationships.TryGetValue(parentPath, out var children))
		{
			return;
		}

		foreach (var childPath in children)
		{
			var fileInfo = new FileInfo(childPath);
			var folderName = GetRelativeFolderPath(fileInfo.Directory, baseDirectory);
			
			var nodeText = new Text($"{fileInfo.Name} (in {folderName})", Color.PaleTurquoise1);
			var childNode = parentNode.AddNode(nodeText);
			AddChildrenToFolderTreeWithText(childNode, childPath, fileRelationships, baseDirectory);
		}
	}
	
	private void AddChildrenToRelativePathTreeWithText(TreeNode parentNode, string parentPath, Dictionary<string, List<string>> fileRelationships, string baseDirectoryPath)
	{
		if (!fileRelationships.TryGetValue(parentPath, out var children))
		{
			return;
		}

		foreach (var childPath in children)
		{
			// Get a path relative to the root file directory
			var relativePath = GetRelativeFilePath(childPath, baseDirectoryPath);
			
			// Handle the special case with location info
			if (relativePath.Contains(" (in "))
			{
				var parts = relativePath.Split(new[] { " (in " }, 2, StringSplitOptions.None);
				var fileName = parts[0];
				var locationInfo = parts.Length > 1 ? " (in " + parts[1] : "";
				
				var text = new Text(fileName + locationInfo, Color.PaleTurquoise1);
				var childNode = parentNode.AddNode(text);
				AddChildrenToRelativePathTreeWithText(childNode, childPath, fileRelationships, baseDirectoryPath);
			}
			else
			{
				var text = new Text(relativePath, Color.PaleTurquoise1);
				var childNode = parentNode.AddNode(text);
				AddChildrenToRelativePathTreeWithText(childNode, childPath, fileRelationships, baseDirectoryPath);
			}
		}
	}
	
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
				var filePathComponents = fileDir?.Split(Path.DirectorySeparatorChar) ?? Array.Empty<string>();
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
}
