using System.Reflection;
using System.Text;
using Spectre.Console;
using Spectre.Console.Testing;

namespace MergeIncludes.Tests;

[UsesVerify]
public class CombineCommandTests
{
    [Fact]
    public async Task SimpleTreeDisplayMode_ShowsCorrectStructure()
    {
        // Arrange
        var console = new TestConsole();
        var command = new CombineCommand(console);
        var rootFile = new FileInfo(Path.GetFullPath(@".\sample.txt"));
        var fileRelationships = CreateTestFileRelationships(rootFile);

        // Act
        InvokeDisplayFileTrees(command, rootFile, fileRelationships, TreeDisplayMode.Simple);

        // Assert - Verify console output
        var output = console.Output;
        await Verify(output)
            .UseDirectory("Snapshots")
            .UseFileName("SimpleTreeDisplay");
    }

    [Fact]
    public async Task FolderStructureTreeDisplayMode_ShowsCorrectStructure()
    {
        // Arrange
        var console = new TestConsole();
        var command = new CombineCommand(console);
        var rootFile = new FileInfo(Path.GetFullPath(@".\sample.txt"));
        var fileRelationships = CreateTestFileRelationships(rootFile);

        // Act
        InvokeDisplayFileTrees(command, rootFile, fileRelationships, TreeDisplayMode.WithFolders);

        // Assert - Verify console output
        var output = console.Output;
        await Verify(output)
            .UseDirectory("Snapshots")
            .UseFileName("FolderTreeDisplay");
    }

    [Fact]
    public async Task FullPathTreeDisplayMode_ShowsCorrectStructure()
    {
        // Arrange
        var console = new TestConsole();
        var command = new CombineCommand(console);
        var rootFile = new FileInfo(Path.GetFullPath(@".\sample.txt"));
        var fileRelationships = CreateTestFileRelationships(rootFile);

        // Act
        InvokeDisplayFileTrees(command, rootFile, fileRelationships, TreeDisplayMode.FullPaths);

        // Assert - Verify console output
        var output = console.Output;
        await Verify(output)
            .UseDirectory("Snapshots")
            .UseFileName("FullPathTreeDisplay");
    }

    [Fact]
    public async Task BothTreeDisplayMode_ShowsCorrectStructure()
    {
        // Arrange
        var console = new TestConsole();
        var command = new CombineCommand(console);
        var rootFile = new FileInfo(Path.GetFullPath(@".\sample.txt"));
        var fileRelationships = CreateTestFileRelationships(rootFile);

        // Act
        InvokeDisplayFileTrees(command, rootFile, fileRelationships, TreeDisplayMode.Both);

        // Assert - Verify console output
        var output = console.Output;
        await Verify(output)
            .UseDirectory("Snapshots")
            .UseFileName("BothTreeDisplay");
    }

    [Fact]
    public async Task TreeDisplayWithRepeatedDependencies_ShowsRepeatMarkings()
    {
        // Arrange
        var console = new TestConsole();
        var command = new CombineCommand(console);
        var rootFile = new FileInfo(Path.GetFullPath(@".\sample.txt"));
        var fileRelationships = CreateTestFileRelationshipsWithRepeats(rootFile);

        // Act
        InvokeDisplayFileTrees(command, rootFile, fileRelationships, TreeDisplayMode.Simple);

        // Assert - Verify console output
        var output = console.Output;
        await Verify(output)
            .UseDirectory("Snapshots")
            .UseFileName("RepeatedDependenciesTreeDisplay");
    }

    // Helper method to create a test file relationship dictionary
    private Dictionary<string, List<string>> CreateTestFileRelationships(FileInfo rootFile)
    {
        var fileRelationships = new Dictionary<string, List<string>>();
        
        // Add sample01.txt and sample02.txt as children of sample.txt for testing
        fileRelationships[rootFile.FullName] = new List<string>
        {
            Path.GetFullPath(@".\sample01.txt"),
            Path.GetFullPath(@".\sample02.txt")
        };

        // Add sample02-01.txt as a child of sample02.txt for testing
        var sample02Path = Path.GetFullPath(@".\sample02.txt");
        fileRelationships[sample02Path] = new List<string>
        {
            Path.GetFullPath(@".\sample02-01.txt")
        };

        return fileRelationships;
    }

    // Helper method to create a test file relationship dictionary with repeated dependencies
    private Dictionary<string, List<string>> CreateTestFileRelationshipsWithRepeats(FileInfo rootFile)
    {
        var fileRelationships = new Dictionary<string, List<string>>();
        
        // Add sample01.txt and sample02.txt as children of sample.txt for testing
        fileRelationships[rootFile.FullName] = new List<string>
        {
            Path.GetFullPath(@".\sample01.txt"),
            Path.GetFullPath(@".\sample02.txt")
        };

        // Add sample02-01.txt as a child of sample02.txt for testing
        var sample02Path = Path.GetFullPath(@".\sample02.txt");
        fileRelationships[sample02Path] = new List<string>
        {
            Path.GetFullPath(@".\sample02-01.txt")
        };

        // Add sample01.txt again as a child of sample02-01.txt for testing repeated dependencies
        var sample0201Path = Path.GetFullPath(@".\sample02-01.txt");
        fileRelationships[sample0201Path] = new List<string>
        {
            Path.GetFullPath(@".\sample01.txt")  // This is a repeat of the file included by the root
        };

        return fileRelationships;
    }

    // Helper method to invoke the private DisplayFileTrees method using reflection
    private void InvokeDisplayFileTrees(
        CombineCommand command, 
        FileInfo rootFile, 
        Dictionary<string, List<string>> fileRelationships,
        TreeDisplayMode mode)
    {
        var methodInfo = typeof(CombineCommand).GetMethod(
            "DisplayFileTrees", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (methodInfo == null)
        {
            throw new InvalidOperationException("Could not find the DisplayFileTrees method");
        }
        
        methodInfo.Invoke(command, new object[] { rootFile, fileRelationships, mode });
    }
}