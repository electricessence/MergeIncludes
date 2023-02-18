# MergeIncludes
 Recursively reads any text based file and replaces #include and #require statements with the content of other files.

## Usage
Uses a standard CLI. 

### Get Help
```ps
MergeIncludes.exe --help
```

### Merge a File
The following will create a file called `MyFile.merged.txt`.
```
MergeIncludes.exe ./MyFile.txt
```
## Syntax

`MergeIncludes` reads line by line and include statements must start at the beginning of the line.

The `#include` directive will insert the contents at that location and `#require` will only insert contents when it's first referenced.

Any line beginning `##` (double pound) will be considered a comment and will simply not be included in the output.

### Markdown & HTML
```md
<!-- ## Comment -->
<!-- #include ./filepath.md -->
```

### Script
```js
// ## Comment
// #include ./filepath.md
```

### Text
```
## Comment
#include ./filepath.md
```
