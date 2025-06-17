# Test File Organization for Unit Testing

This document outlines the organized test files for faster iteration during development.

## Organized Test Cases (TestCases folder)

### BasicStructure
- **File**: `TestCases/BasicStructure/simple-root.txt`
- **Purpose**: Tests basic file inclusion with simple folder structure
- **Unit Test**: `BasicStructure_ActualOutput()`

### DuplicateReferences  
- **File**: `TestCases/DuplicateReferences/root-duplicates.txt`
- **Purpose**: Tests handling of duplicate file references
- **Unit Test**: `DuplicateReferences_ActualOutput()`

### CircularReferences
- **File**: `TestCases/CircularReferences/circular-root.txt`
- **Purpose**: Tests proper circular reference detection and error handling
- **Unit Test**: `OrganizedCircularReference_ActualOutput()`

### FolderJumping
- **File**: `TestCases/FolderJumping/unique-names.txt`
- **Purpose**: Tests includes that jump between different folders
- **Unit Test**: `OrganizedFolderJumping_ActualOutput()`

## Legacy Test Files (TestFiles folder)

### Core Functionality Tests
- **SimpleConsecutive**: `TestFiles/simple-consecutive.txt` - `SimpleConsecutive_ActualOutput()`
- **RootFile**: `TestFiles/MainFolder/root.txt` - `RootFile_ActualOutput()`
- **SimpleRootFile**: `TestFiles/root-file.txt` - `SimpleRootFile_ActualOutput()`

### Specific Scenario Tests
- **TestDuplicates**: `TestFiles/test-duplicates.txt` - `TestDuplicates_ActualOutput()`
- **FolderJumpingTest**: `TestFiles/folder-jumping-test.txt` - `FolderJumpingTest_ActualOutput()`
- **ConsecutiveSameFolder**: `TestFiles/consecutive-same-folder-test.txt` - `ConsecutiveSameFolder_ActualOutput()`
- **ComplexCircular**: `TestFiles/MainFolder/complex-root.txt` - `ComplexCircular_ActualOutput()`
- **CircularReference**: `TestFiles/test-circular.txt` - `CircularReference_ActualOutput()` (manual test file)

### Folder Structure Tests
- **FolderJumping**: `TestFiles/unique-names-test.txt` - `FolderJumping_ActualOutput()`

## Test Coverage Matrix

| Scenario | Organized Test | Legacy Test | Coverage |
|----------|---------------|-------------|----------|
| Basic Structure | ✅ BasicStructure | ✅ SimpleRootFile | Good |
| Duplicate References | ✅ DuplicateReferences | ✅ TestDuplicates | Good |
| Circular References | ✅ OrganizedCircularReference | ✅ CircularReference | Good |
| Folder Jumping | ✅ OrganizedFolderJumping | ✅ FolderJumpingTest | Good |
| Complex Scenarios | - | ✅ ComplexCircular | Good |
| Consecutive Files | - | ✅ SimpleConsecutive | Good |
| Same Folder | - | ✅ ConsecutiveSameFolder | Good |

## Running Tests

To run all tests:
```bash
dotnet test MergeIncludes.Tests --filter "ActualOutputTests"
```

To run specific test:
```bash
dotnet test MergeIncludes.Tests --filter "BasicStructure_ActualOutput"
```

## Benefits for Development

1. **Faster Iteration**: Unit tests run much faster than manual CLI testing
2. **Regression Detection**: Automated verification of output changes
3. **Organized Scenarios**: Clear separation of test cases by purpose
4. **Snapshot Testing**: Uses Verify library to track output changes
5. **Comprehensive Coverage**: Both organized and legacy test files covered

## Next Steps

1. Run initial test suite to establish baselines
2. Use tests for rapid iteration on tree display improvements
3. Add new test cases as needed for edge scenarios
4. Gradually migrate legacy test files to organized structure
