# Command Refactoring Plan for Better Testing

## Current Issues
1. The `Merge()` method is private and doesn't expose the merged content
2. Console output and file writing are tightly coupled
3. Hard to unit test the actual merged content vs console output

## Proposed Refactoring

### 1. Add MergeResult Class
```csharp
public class MergeResult
{
    public bool Success { get; init; }
    public string? MergedContent { get; init; }
    public List<FileInfo> ProcessedFiles { get; init; } = [];
    public Dictionary<string, List<string>> FileRelationships { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
```

### 2. Refactor Command Class
```csharp
public class Command
{
    // Existing constructor and fields...

    // New public method for testing
    public async Task<MergeResult> MergeToMemoryAsync()
    {
        // Contains the core merge logic, returns MergeResult
        // Does NOT write to file or console
    }

    // Existing execute method becomes a wrapper
    public async Task<int> ExecuteAsync(Settings settings)
    {
        var result = await MergeToMemoryAsync();
        
        if (result.Success)
        {
            // Write merged content to file
            await File.WriteAllTextAsync(outputPath, result.MergedContent);
            
            // Display console output
            DisplayFileTrees(result.FileRelationships);
            DisplaySuccessPanel();
        }
        else
        {
            // Display error
            DisplayErrorPanel(result.ErrorMessage);
        }
        
        return result.Success ? 0 : 1;
    }
}
```

### 3. Clean Unit Tests
```csharp
[Theory]
[InlineData("01_BasicInclusion", "root.txt")]
[InlineData("02_DuplicateReferences", "root.txt")]
[InlineData("03_CircularReferences", "root.txt")] // Should fail
[InlineData("04_FolderNavigation", "root.txt")]
[InlineData("05_ConsecutiveIncludes", "root.txt")]
[InlineData("06_ComplexStructure", "root.txt")]
public async Task MergeScenarios_Test(string scenario, string rootFile)
{
    // Arrange
    var testFile = Path.Combine("TestScenarios", scenario, rootFile);
    var settings = new Settings { RootPath = testFile, DisplayMode = DisplayMode.Default };
    var console = new TestConsole();
    var command = new Command(console, settings);

    // Act
    var result = await command.MergeToMemoryAsync();

    // Assert - Verify the merged content
    await Verify(result.MergedContent)
        .UseDirectory("Snapshots/MergedContent")
        .UseFileName($"{scenario}_MergedContent");

    // Assert - Verify the console output
    await command.ExecuteAsync(settings); // This displays to TestConsole
    await Verify(console.Output)
        .UseDirectory("Snapshots/ConsoleOutput")  
        .UseFileName($"{scenario}_ConsoleOutput");
}
```

## Benefits
1. **Separation of Concerns**: Merge logic separated from I/O and console output
2. **Testability**: Can verify both merged content AND console output independently
3. **Clean Tests**: Single parameterized test covers all scenarios
4. **Maintainability**: Adding new test scenarios is just adding InlineData
5. **Performance**: Tests run faster without file I/O during merge logic testing
