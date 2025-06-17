# Test Structure Refactoring Plan

## Optimal Structure

```
MergeIncludes.Tests/
├── TestScenarios/                    # Consolidated test data (renamed from TestCases)
│   ├── 01_BasicInclusion/           # Simple file inclusion
│   │   ├── root.txt
│   │   ├── SubFolder1/child1.txt
│   │   └── SubFolder2/child2.txt
│   ├── 02_DuplicateReferences/      # Files included multiple times
│   │   ├── root.txt
│   │   ├── shared.txt
│   │   └── SubFolder1/component.txt
│   ├── 03_CircularReferences/       # Circular dependency detection
│   │   ├── root.txt
│   │   ├── moduleA.txt
│   │   └── moduleB.txt
│   ├── 04_FolderNavigation/         # Cross-folder includes
│   │   ├── root.txt
│   │   ├── SubA/fileA.txt
│   │   └── SubB/fileB.txt
│   ├── 05_ConsecutiveIncludes/      # Multiple includes in sequence
│   │   ├── root.txt
│   │   ├── file1.txt
│   │   └── file2.txt
│   └── 06_ComplexStructure/         # Complex real-world scenario
│       ├── root.txt
│       ├── local.txt
│       ├── SubFolder1/component1.txt
│       └── SubFolder2/component2.txt
└── ActualOutputTests.cs             # Simplified, non-redundant tests
```

## Redundant Tests to Remove

### From ActualOutputTests.cs:
1. **FolderJumping_ActualOutput** AND **OrganizedFolderJumping_ActualOutput** - Keep only organized version
2. **TestDuplicates_ActualOutput** AND **DuplicateReferences_ActualOutput** - Keep only organized version  
3. **SimpleRootFile_ActualOutput** AND **BasicStructure_ActualOutput** - Keep only organized version
4. **ConsecutiveSameFolder_ActualOutput** AND **SimpleConsecutive_ActualOutput** - Consolidate into one

### From TestFiles directory:
- `test-duplicates.txt` (redundant with TestCases version)
- `unique-names-test.txt` (redundant with TestCases version)  
- `root-file.txt` (redundant with other root files)
- `consecutive-same-folder-test.txt` (consolidate with simple-consecutive)
- Various loose files: `file4.txt`, `file6.txt`, `unique4.txt`, `unique6.txt`

## Final Clean Structure

### 6 Core Test Scenarios (in TestScenarios/):
1. **BasicInclusion** - Simple includes, good for basic functionality
2. **DuplicateReferences** - Files included multiple times  
3. **CircularReferences** - True circular dependencies (should fail)
4. **FolderNavigation** - Cross-folder includes, relative paths
5. **ConsecutiveIncludes** - Multiple sequential includes
6. **ComplexStructure** - Real-world complex scenario

### 6 Corresponding Tests (in ActualOutputTests.cs):
1. `BasicInclusion_Test()`
2. `DuplicateReferences_Test()`  
3. `CircularReferences_Test()`
4. `FolderNavigation_Test()`
5. `ConsecutiveIncludes_Test()`
6. `ComplexStructure_Test()`
