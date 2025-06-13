# MergeIncludes
 Recursively reads any text based file and replaces #include and #require statements with the content of other files.

## Usage
Uses a standard CLI. 

### Get HelpMergeIncludes.exe --help
### Merge a File
The following will create a file called `MyFile.merged.txt`.MergeIncludes.exe ./MyFile.txt
### Display Options
You can control how the file inclusion tree is displayed using the `-d` or `--display` option:
MergeIncludes.exe ./MyFile.txt -d Simple         # Shows simple file tree with IDs
MergeIncludes.exe ./MyFile.txt -d WithFolders    # Shows files with folder information
MergeIncludes.exe ./MyFile.txt -d FullPaths      # Shows files with relative paths
MergeIncludes.exe ./MyFile.txt -d Both           # Shows both simple and folder trees
MergeIncludes.exe ./MyFile.txt -d RepeatsOnly    # Default - Only shows IDs for files that repeat
MergeIncludes.exe ./MyFile.txt -d FolderGrouped  # Shows folder headers with files grouped under them
## Syntax

`MergeIncludes` reads line by line and include statements must start at the beginning of the line.

The `#include` directive will insert the contents at that location and `#require` will only insert contents when it's first referenced.

Any line beginning `##` (double pound) will be considered a comment and will simply not be included in the output.

### Markdown & HTML<!-- ## Comment -->
<!-- #include ./filepath.md -->
### Script// ## Comment
// #include ./filepath.md
### Text## Comment
#include ./filepath.md
## Wild-Cards

File names (not directories) can contain wild-cards. Order should be alphabetical, but not guaranteed.

The rules for each directive still apply:
* When using `#require`, if any one of the files has already been included it will be skipped.
* When using `#include`, all entries will be listed.


### Examples
#require ./sample-*.txt#require ./sample-??.txt#require ./*.txt