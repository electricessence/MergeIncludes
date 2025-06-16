# MergeIncludes

**MergeIncludes** is a powerful command-line tool that recursively processes text-based files, replacing `#include` and `#require` statements with the content of referenced files. Perfect for building composite documents, generating documentation, or preparing deployable files from modular components.

[![Built for .NET 9](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- **Recursive Processing** - Process nested include directives to any depth
- **Smart Include Logic** - Support for both `#include` and `#require` directives
- **Wildcard Support** - Reference multiple files with a single directive
- **Comment Stripping** - Remove development comments from output files 
- **File Tree Visualization** - See the inclusion hierarchy with clickable file links
- **Watch Mode** - Automatically rebuild when source files change
- **Cross-Platform** - Works on Windows, macOS, and Linux

## Installation

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) or later

### Install via .NET Tool (recommended)
dotnet tool install --global MergeIncludes
### Build from Source
git clone https://github.com/electricessence/MergeIncludes.git
cd MergeIncludes
dotnet build -c Release
## Basic Usage

### Getting Help
MergeIncludes --help
### Merge a File
MergeIncludes ./MyFile.txt
This command creates a file called `MyFile.merged.txt` in the same directory as `MyFile.txt`.

### Specify Output File
MergeIncludes ./MyFile.txt -o ./output/Combined.txt
## Directive Syntax

MergeIncludes supports several include directive formats to work with different file types:

### Plain Text Files
#include ./relative/path/to/file.txt
#require ./relative/path/to/file.txt
## This is a comment that won't appear in the output
### Markdown & HTML Files
<!-- #include ./relative/path/to/file.md -->
<!-- ## This is a comment that won't appear in the output -->
### Script Files (JavaScript, Python, etc.)
// #include ./relative/path/to/script.js
// ## This is a comment that won't appear in the output
### Directive Behavior

- `#include` - Always inserts the contents of the referenced file, even if it has been included before
- `#require` - Only inserts the contents when the file is first referenced (similar to C/C++ `#include` guards)
- Lines beginning with `##` are treated as comments and removed from the output

## Wildcard Support

File paths (not directories) can contain wildcards to include multiple files:
#require ./templates/*.html
#require ./modules/mod-??.js
Matching files are processed in alphabetical order. The rules for each directive still apply:

- With `#require`, if any one of the matching files has already been included, it will be skipped
- With `#include`, all matching files will be included

## Display Options

Control how the file inclusion tree is displayed using the `-d` or `--display` option:
MergeIncludes ./MyFile.txt -d Default     # Side-by-side trees (folder structure and references)
MergeIncludes ./MyFile.txt -d FullPath    # Simple list with full file paths
When running in Windows Terminal, file paths in the output are clickable links for easy navigation.

## Advanced Options

### Watch Mode

Monitor files for changes and automatically rebuild when they change:
MergeIncludes ./MyFile.txt --watch
Press any key to stop watching.

### Whitespace Control
MergeIncludes ./MyFile.txt --trim false   # Preserve all whitespace (default is to trim)
MergeIncludes ./MyFile.txt --pad 2        # Add 2 blank lines at the end (default is 1)
## Examples

### Basic Document Assembly

**main.md**:# My Document

## Introduction
<!-- #include ./intro.md -->

## Main Content
<!-- #include ./content/*.md -->

## Conclusion
<!-- #include ./conclusion.md -->
### Script Assembly with Conditional Includes

**build.js**:// Core library
// #require ./lib/core.js

// Feature modules - only included once even if required multiple times
// #require ./features/feature-a.js
// #require ./features/feature-b.js

// Always include latest configuration
// #include ./config.js

// Development helpers (commented out in production)
// ## #include ./debug-tools.js
## Error Handling

MergeIncludes provides clear error messages for common issues:

- Missing include files
- Recursive inclusion loops
- Invalid file permissions

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.