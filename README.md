# 🔄 MergeIncludes

> **A powerful, modern CLI tool for merging text files with beautiful visualization**

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![MIT License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen?style=for-the-badge)]()
[![Version](https://img.shields.io/badge/Version-1.0.0-blue?style=for-the-badge)]()

**MergeIncludes** transforms modular text projects into unified documents with recursive `#include` processing, beautiful tree visualization, and smart terminal integration. Perfect for documentation, configuration files, code generation, and complex text assembly workflows.

## ✨ Key Features

- 🔄 **Recursive Processing** - Handle nested includes to any depth
- 🎯 **Smart Directives** - Support `#include`, `#require`, and wildcards  
- 🌳 **Beautiful Visualization** - Interactive file trees with clickable links (Windows Terminal)
- ⚡ **Watch Mode** - Auto-rebuild on file changes
- 🎨 **Terminal-Aware** - Optimized display for different terminal capabilities
- 🔧 **Flexible Output** - Custom output paths and formatting options
- 🌐 **Cross-Platform** - Windows, macOS, and Linux support

## 🚀 Quick Start

### Installation

```bash
# Install as global tool (recommended)
dotnet tool install --global MergeIncludes

# Or build from source
git clone <repository-url>
cd MergeIncludes
dotnet build -c Release
```

### Basic Usage

```bash
# Merge a file (creates MyFile.merged.txt)
MergeIncludes ./MyFile.txt

# Custom output location
MergeIncludes ./docs/main.md -o ./dist/complete.md

# Watch for changes
MergeIncludes ./project/root.txt --watch
```

## 📖 How It Works

MergeIncludes processes files containing special directives and replaces them with the content of referenced files:

### Input Structure
```
📁 project/
├── 📄 main.txt
├── 📁 sections/
│   ├── 📄 intro.txt
│   ├── 📄 features.txt
│   └── 📄 conclusion.txt
└── 📁 shared/
    └── 📄 footer.txt
```

### main.txt
```text
# My Project Documentation

#include ./sections/intro.txt
#include ./sections/features.txt
#include ./sections/conclusion.txt

---
#include ./shared/footer.txt
```

### Result
**MergeIncludes** combines everything into a single `main.merged.txt` file with all content merged.

---

## 🎯 Directive Syntax

| Directive | Description | Example |
|-----------|-------------|---------|
| `#include` | Include file content | `#include ./path/file.txt` |
| `#require` | Same as include | `#require ./config/settings.json` |
| **Wildcards** | Include multiple files | `#include ./docs/*.md` |
| **Comments** | Ignored in output | `## This won't appear` |

### Supported File Types

```text
📄 Plain Text       →  #include ./file.txt
📝 Markdown         →  <!-- #include ./section.md -->
🌐 HTML             →  <!-- #include ./component.html -->
⚙️  Configuration   →  # include ./config.json
🔧 Scripts          →  // include ./module.js
```

---

## 🖼️ Beautiful Terminal Output

### Windows Terminal (with Hyperlinks)
*[Screenshot Placeholder: Windows Terminal with clickable file links and colored tree structure]*

### VS Code Terminal (Clean Display)  
*[Screenshot Placeholder: VS Code terminal with clean tree structure, no link corruption]*

### Example Tree Visualization
```
╭─Root File──────────────────────────────────╮
│ ./project/main.txt                         │
╰────────────────────────────────────────────╯
╭─Structure─────────────────────────────────────────╮
│ 📁 project              / main.txt               │
│ ├── 📁 sections           ├── intro.txt [1]      │
│ ├── 📁 sections           ├── features.txt [2]   │
│ ├── 📁 sections           ├── conclusion.txt [3] │
│ └── 📁 shared             └── footer.txt [4]     │
╰───────────────────────────────────────────────────╯
╭─Successfully merged include references to:────────╮
│ ./project/main.merged.txt                         │
╰───────────────────────────────────────────────────╯
```

---

## ⚙️ Command Line Options

```bash
USAGE:
    MergeIncludes <ROOT_FILEPATH> [OPTIONS]

ARGUMENTS:
    <ROOT_FILEPATH>    The root file to start processing from

OPTIONS:
    -o, --out <OUTPUT_FILEPATH>     Custom output file path
    -d, --display <DISPLAY_MODE>    Tree display mode:
                                      Default      - Side-by-side trees
                                      FullPath     - Full file paths
                                      RelativePath - Relative paths
    -w, --watch                     Watch for file changes
    -t, --trim <TRIM_ENABLED>       Trim empty lines (default: true)
    -p, --pad <LINE_PADDING>        Add padding lines (default: 1)
        --hide-path                 Hide source paths in output
    -h, --help                      Show help information
```

---

## 💡 Real-World Examples

### 📚 Documentation Assembly
```bash
# Combine scattered documentation into a single guide
MergeIncludes ./docs/user-guide.md -o ./dist/complete-guide.md
```

### 🔧 Configuration Merging  
```bash
# Merge modular config files for deployment
MergeIncludes ./config/production.conf -o ./deploy/app.conf
```

### 📄 Report Generation
```bash
# Build complex reports from sections
MergeIncludes ./reports/monthly-template.txt --watch
```

### 🌐 Static Site Generation
```bash
# Assemble web pages from components
MergeIncludes ./src/index.html -o ./public/index.html
```

---

## 🔍 Advanced Features

### Wildcard Includes
```text
# Include all markdown files from a directory
#include ./chapters/*.md

# Include specific file patterns
#include ./modules/core-*.js
```

### Watch Mode with Auto-Rebuild
```bash
# Automatically rebuild when any referenced file changes
MergeIncludes ./project/main.txt --watch
```
*[Screenshot Placeholder: Terminal showing watch mode detecting file changes and auto-rebuilding]*

### Smart Terminal Detection
- **Windows Terminal**: Rich hyperlinks and enhanced colors
- **VS Code Terminal**: Clean display optimized for embedded terminals  
- **Standard Terminals**: Fallback to plain text with full functionality

---

## 🏗️ Project Structure Examples

### Complex Documentation Project
```
📁 my-docs/
├── 📄 manual.txt                 ← Root file
├── 📁 chapters/
│   ├── 📄 01-introduction.md
│   ├── 📄 02-installation.md  
│   ├── 📄 03-usage.md
│   └── 📄 04-advanced.md
├── 📁 shared/
│   ├── 📄 header.md
│   ├── 📄 footer.md
│   └── 📄 legal.md
└── 📁 examples/
    ├── 📄 basic-example.txt
    └── 📄 advanced-example.txt
```

### Configuration Management
```
📁 config/
├── 📄 app.conf                  ← Root configuration
├── 📁 environments/
│   ├── 📄 development.conf
│   ├── 📄 staging.conf
│   └── 📄 production.conf
├── 📁 modules/
│   ├── 📄 database.conf
│   ├── 📄 logging.conf
│   └── 📄 security.conf
└── 📁 secrets/
    └── 📄 api-keys.conf
```

---

## 🔧 Technical Details

### Performance Features
- **Optimized String Operations** - Uses `StringBuilder` and string pooling
- **Path Caching** - Efficient file system operations
- **Smart Terminal Detection** - Cached capability detection
- **Memory Efficient** - Minimal allocations during processing

### Error Handling
- **Circular Reference Detection** - Prevents infinite loops
- **Missing File Warnings** - Clear error messages
- **Path Validation** - Robust file system handling

### Terminal Capabilities
- **Hyperlink Support Detection** - Windows Terminal, iTerm2, and more
- **Fallback Rendering** - Clean output in all terminals
- **Color Support** - Rich formatting where available

---

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 🆘 Support

- 📋 **Issues**: [GitHub Issues](../../issues)
- 💬 **Discussions**: [GitHub Discussions](../../discussions)  
- 📧 **Contact**: [Your Contact Info]

---

<div align="center">

**⭐ Star this repo if MergeIncludes helps your workflow! ⭐**

*Built with ❤️ and .NET 9*

</div>
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