# 🔄 MergeIncludes

> **The most elegant way to merge modular text files with stunning visualization**

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![MIT License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen?style=for-the-badge)]()
[![Version](https://img.shields.io/badge/Version-1.0.0-blue?style=for-the-badge)]()

Transform your modular text projects into unified masterpieces. **MergeIncludes** recursively processes `#include` directives with beautiful tree visualization, smart terminal integration, and blazing-fast performance.

Perfect for **documentation assembly**, **configuration management**, **code generation**, and **complex text workflows**.

---

## ✨ Why MergeIncludes?

### 🎯 **Built for Modern Workflows**
- **Recursive Processing** - Handle nested includes to unlimited depth
- **Smart Directives** - `#include`, `#require`, wildcards, and comments
- **Watch Mode** - Auto-rebuild on file changes for rapid iteration
- **Cross-Platform** - Windows, macOS, Linux ready

### � **Beautiful Terminal Experience**
- **Interactive Trees** - Visual file structure with clickable links (Windows Terminal)
- **Terminal-Aware** - Optimized display for VS Code, Windows Terminal, and standard terminals
- **Smart Detection** - Automatically adapts to your terminal's capabilities
- **Professional Output** - Clean, formatted results every time

### ⚡ **Performance Optimized**
- **Memory Efficient** - Advanced string pooling and caching
- **Fast Processing** - Optimized for large file hierarchies
- **Circular Detection** - Smart prevention of infinite loops
- **Error Resilient** - Graceful handling of missing files and edge cases

---

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

### Your First Merge
```bash
# Basic merge (creates MyDocument.merged.txt)
MergeIncludes ./MyDocument.txt

# Custom output
MergeIncludes ./docs/main.md -o ./dist/complete-guide.md

# Watch for changes
MergeIncludes ./project/root.txt --watch
```

---

## 🎬 See It In Action

### Beautiful Tree Visualization
```
╭─Root File──────────────────────────────────────────╮
│ ./docs/user-guide.md                               │
╰────────────────────────────────────────────────────╯
╭─Structure─────────────────────────────────────────────────╮
│ 📁 docs                 / user-guide.md                  │
│ ├── 📁 chapters           ├── introduction.md [1]        │
│ ├── 📁 sections           ├── getting-started.md [2]     │
│ │   └── 📁 examples       │   └── basic-example.md [3]   │
│ ├── 📁 sections           ├── advanced-usage.md [4]      │
│ │   └── 📁 examples       │   └── complex-example.md [5] │
│ └── 📁 shared             └── footer.md [6]              │
╰───────────────────────────────────────────────────────────╯
╭─Reference Tree────────────────────────────────────────────╮
│ user-guide.md                                             │
│ ├── introduction.md [1]                                   │
│ ├── getting-started.md [2]                                │
│ │   └── basic-example.md [3]                              │
│ ├── advanced-usage.md [4]                                 │
│ │   ├── complex-example.md [5]                            │
│ │   └── basic-example.md [3] ⚠️                           │
│ └── footer.md [6]                                         │
╰───────────────────────────────────────────────────────────╯
```

### Watch Mode Magic
```bash
$ MergeIncludes ./project/main.txt --watch
🔍 Watching for changes... Press any key to stop.

📝 File changed: ./project/sections/intro.txt
🔄 Rebuilding... Done! ✨
```

---

## 📖 Directive Reference

| Directive | Behavior | Example |
|-----------|----------|---------|
| `#include` | Always includes content (allows duplicates) | `#include ./sections/intro.md` |
| `#require` | Include only once (prevents duplicates) | `#require ./config/settings.json` |
| **Wildcards** | Include multiple files | `#include ./docs/*.md` |
| **Comments** | Ignored in output | `## This won't appear` |

### Supported in Any File Type
```text
📝 Markdown         →  <!-- #include ./section.md -->
🌐 HTML             →  <!-- #include ./component.html -->
⚙️  Configuration   →  # include ./config.yaml
🔧 JavaScript       →  // #include ./module.js
🐍 Python           →  # #include ./utilities.py
📄 Plain Text       →  #include ./content.txt
```

---

## 💡 Real-World Use Cases

### 📚 **Technical Documentation**
```
📁 docs/
├── 📄 user-guide.md          ← Root document
├── 📁 chapters/
│   ├── 📄 installation.md
│   ├── 📄 configuration.md
│   └── 📄 troubleshooting.md
├── 📁 examples/
│   ├── 📄 basic-usage.md
│   └── 📄 advanced-tips.md
└── 📁 shared/
    ├── 📄 header.md
    └── 📄 footer.md
```

```bash
# Build complete documentation
MergeIncludes ./docs/user-guide.md -o ./dist/complete-guide.md --watch
```

### 🔧 **Configuration Management**
```
📁 config/
├── 📄 production.yml         ← Environment config
├── 📁 modules/
│   ├── 📄 database.yml
│   ├── 📄 logging.yml
│   └── 📄 security.yml
└── 📁 secrets/
    └── 📄 api-keys.yml
```

```bash
# Deploy with merged configuration
MergeIncludes ./config/production.yml -o ./deploy/app-config.yml
```

### 🌐 **Static Site Generation**
```
📁 site/
├── 📄 index.html             ← Main template
├── 📁 components/
│   ├── 📄 header.html
│   ├── 📄 navigation.html
│   └── 📄 footer.html
└── 📁 content/
    ├── 📄 hero-section.html
    └── 📄 features.html
```

### 📊 **Report Assembly**
```
📁 reports/
├── 📄 monthly-report.md      ← Report template
├── 📁 sections/
│   ├── 📄 executive-summary.md
│   ├── 📄 financial-data.md
│   └── 📄 recommendations.md
└── 📁 data/
    ├── 📄 charts.md
    └── 📄 tables.md
```

---

## ⚙️ Command Reference

```bash
USAGE:
    MergeIncludes <ROOT_FILE> [OPTIONS]

ARGUMENTS:
    <ROOT_FILE>                The file to start processing from

OPTIONS:
    -o, --out <OUTPUT>         Custom output file path
    -d, --display <MODE>       Tree display mode:
                                 Default      - Side-by-side trees
                                 FullPath     - Full file paths  
                                 RelativePath - Relative paths
    -w, --watch               Watch for file changes
    -t, --trim <ENABLED>      Trim empty lines (default: true)
    -p, --pad <LINES>         Add padding lines (default: 1)
        --tree                Show tree visualization only
        --hide-path           Hide source paths in merged output
    -h, --help                Show help information
    -v, --version             Show version information
```

### Examples
```bash
# Basic merge
MergeIncludes ./main.txt

# Custom output with tree display
MergeIncludes ./docs/guide.md -o ./output/complete.md -d FullPath

# Watch mode for development
MergeIncludes ./project/main.txt --watch --trim false

# Show tree structure only
MergeIncludes ./config/app.yml --tree
```

---

## 🔍 Advanced Features

### 🎯 **Smart Wildcard Processing**
```text
# Include all markdown files in order
#include ./chapters/*.md

# Include specific patterns
#include ./modules/core-*.js
#include ./data/report-??-*.csv
```

### 🔄 **Duplicate Detection & Warnings**
- **Visual Indicators**: `⚠️` symbols mark duplicate references
- **Reference Numbers**: `[1]`, `[2]`, `[3]` track file usage
- **Smart Handling**: `#require` prevents duplicates, `#include` allows them

### 📺 **Terminal Integration**
- **Windows Terminal**: Clickable file links and rich colors
- **VS Code Terminal**: Clean display optimized for integrated terminals
- **Standard Terminals**: Full functionality with graceful fallbacks

### ⚡ **Performance Features**
- **String Pooling**: Minimizes memory allocations
- **Path Caching**: Optimizes file system operations  
- **Lazy Evaluation**: Processes files only when needed
- **Circular Detection**: Prevents infinite recursion

---

## 🛠️ Technical Specifications

### Requirements
- **.NET 9.0** or higher
- **Windows 10+**, **macOS 10.15+**, or **Linux** (any modern distribution)
- **Terminal**: Any ANSI-compatible terminal (enhanced features in Windows Terminal)

### File Support
- **Text Files**: `.txt`, `.md`, `.html`, `.css`, `.js`, `.py`, `.yml`, `.json`, etc.
- **Encoding**: UTF-8, UTF-16, ASCII
- **Size Limits**: Optimized for files up to 100MB (larger files supported)
- **Path Lengths**: Full Windows long path support

### Error Handling
- **Missing Files**: Clear warnings with suggested fixes
- **Circular References**: Automatic detection and prevention
- **Permission Issues**: Helpful error messages
- **Invalid Syntax**: Detailed line-by-line feedback

---

## 🤝 Contributing

We welcome contributions! Here's how to get started:

1. **Fork** the repository
2. **Create** your feature branch: `git checkout -b feature/amazing-feature`
3. **Commit** your changes: `git commit -m 'Add amazing feature'`
4. **Push** to the branch: `git push origin feature/amazing-feature`
5. **Open** a Pull Request

### Development Setup
```bash
git clone <repository-url>
cd MergeIncludes
dotnet restore
dotnet build
dotnet test
```

---

## � License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 🆘 Support & Community

- 🐛 **Bug Reports**: [GitHub Issues](../../issues)
- 💡 **Feature Requests**: [GitHub Discussions](../../discussions)
- 📚 **Documentation**: [Wiki](../../wiki)
- 💬 **Community**: [Discussions](../../discussions)

---

<div align="center">

**⭐ Star this repo if MergeIncludes improves your workflow! ⭐**

*Built with ❤️ using .NET 9 and modern C# practices*

**[Download Latest Release](../../releases) • [View Documentation](../../wiki) • [Join Community](../../discussions)**

</div>
