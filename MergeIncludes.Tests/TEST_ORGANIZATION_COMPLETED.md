# Test Organization Summary

## Current Test Structure

The test files have been reorganized into a logical, scenario-based structure under `TestScenarios/`:

### Organized Test Scenarios

1. **01_BasicInclusion/** - Simple include scenarios
   - `root.txt` - Basic test with simple includes

2. **02_DuplicateReferences/** - Tests for duplicate file handling
   - `root.txt` - Test with shared file referenced multiple times
   - `test-duplicates.txt` - Test file references with ID numbering
   - `real-duplicates.txt` - File with intentional duplicate includes
   - `shared.txt` - Shared file referenced by multiple includes
   - `SubFolder1/` & `SubFolder2/` - Component files

3. **03_CircularReferences/** - Tests for circular reference detection
   - `root.txt` - Manual circular reference test
   - `simple-recursive.txt` & `simple-recursive01.txt` - Simple circular reference pair
   - `manual-circular.txt` & `manual-circular-child.txt` - Manual circular test
   - `moduleA.txt` & `moduleB.txt` - Module circular reference test

4. **04_FolderNavigation/** - Tests for unique filenames per folder
   - `root.txt` - Test with unique filenames per folder
   - Various `SubFolder1/` & `SubFolder2/` files with unique names

5. **05_ConsecutiveIncludes/** - Tests for consecutive file inclusions
   - `root.txt` - Basic consecutive includes
   - `consecutive-same-folder.txt` - Consecutive files from same subfolder
   - `simple-consecutive.txt` - Simple consecutive test
   - `test.txt` - Additional consecutive test

6. **06_ComplexStructure/** - Complex nested scenarios
   - `root.txt` - Complex file structure with multiple dependencies

7. **07_WildcardIncludes/** - Tests for wildcard patterns
   - `root.txt` - Test using wildcard patterns (#require)
   - `sample01.txt`, `sample02.txt`, `sample02-01.txt` - Sample files for wildcard matching

8. **08_CommentedIncludes/** - Tests for commented includes
   - `root.txt` - Test with various comment styles around includes

9. **10_DetailedFolderJumping/** - Detailed folder navigation tests
   - `root.txt` - Test jumping between different directories with detailed patterns

### Shared Resources

**Shared/** - Common files used across multiple test scenarios
- `file4.txt`, `file6.txt`, `root-file.txt` - Common files
- `SubFolder1/` & `SubFolder2/` - Standard test folder structures
- `SubA/` & `SubB/` - Additional test folders
- `MainFolder/` - Complex test scenarios
- `AnotherFolder/` - Additional test folder structure

## Benefits of This Organization

1. **Clear Scenario Separation** - Each test scenario is self-contained and clearly named
2. **Reduced Duplication** - Shared files are centralized in the Shared/ directory
3. **Git History Preserved** - All moves were done with `git mv` to preserve file history
4. **Logical Grouping** - Related tests are grouped by functionality
5. **Easy Navigation** - Numbered folders make it easy to find specific test types
6. **Maintainable** - New test scenarios can be easily added following the pattern

## Cleanup Completed

- ✅ Removed temporary output files (`test-*-output.txt`) from root directory
- ✅ Consolidated duplicate test files
- ✅ Moved scattered test files to organized TestScenarios structure
- ✅ Removed redundant `.merged.txt` files (these are artifacts)
- ✅ Deleted empty TestFiles directory
- ✅ Updated include paths to reference Shared resources
- ✅ Used `git mv` throughout to preserve file history

## Next Steps

The test files are now properly organized. The next phase should focus on:

1. **Update Test Classes** - Modify ActualOutputTests.cs and other test classes to use the new structure
2. **Parameterized Tests** - Create [Theory]/[InlineData] tests that iterate through all scenarios
3. **Verify Integration** - Ensure all scenarios work with the MergeToMemoryAsync method
4. **Documentation** - Update any remaining documentation to reference the new structure
