# 🔄 MergeIncludes

> **The most elegant way to merge modular text files with stunning visualization**
> 
> *🎯 This README demonstrates MergeIncludes in action - it's built from modular sections using the tool itself!*

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![MIT License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen?style=for-the-badge)]()
[![Version](https://img.shields.io/badge/Version-1.0.0-blue?style=for-the-badge)]()

Perfect for **documentation assembly**, **configuration management**, **code generation**, and **complex text workflows**.

---

Transform your modular text projects into unified masterpieces. **MergeIncludes** recursively processes `#include` directives with beautiful tree visualization, smart terminal integration, and blazing-fast performance.


- **Recursive Processing** - Handle nested includes to unlimited depth
- **Smart Directives** - `#include`, `#require`, wildcards, and comments
- **Watch Mode** - Auto-rebuild on file changes for rapid iteration
- **Cross-Platform** - Windows, macOS, Linux ready

- **Interactive Trees** - Visual file structure with clickable links (Windows Terminal)
- **Terminal-Aware** - Optimized display for VS Code, Windows Terminal, and standard terminals
- **Smart Detection** - Automatically adapts to your terminal's capabilities
- **Professional Output** - Clean, formatted results every time

- **Memory Efficient** - Advanced string pooling and caching
- **Fast Processing** - Optimized for large file hierarchies
- **Circular Detection** - Smart prevention of infinite loops
- **Error Resilient** - Graceful handling of missing files and edge cases

---


```bash
# Install as global tool (recommended)
dotnet tool install --global MergeIncludes

# Or build from source
git clone <repository-url>
cd MergeIncludes
dotnet build -c Release
```

```bash
# Basic merge (creates MyDocument.merged.txt)
MergeIncludes ./MyDocument.txt

# Custom output
MergeIncludes ./docs/main.md -o ./dist/complete-guide.md

# Watch for changes
MergeIncludes ./project/root.txt --watch
```

```bash
# This very README is built using MergeIncludes!
cd docs
MergeIncludes ./README-template.md -o ../README.md

# Watch mode for documentation development
MergeIncludes ./README-template.md --watch -o ../README.md
```

*Yes, you're reading documentation that was assembled by the very tool it describes! 🤯*

---


```
╭─Root File──────────────────────────────────────────╮
│ ./docs/README-template.md                          │
╰────────────────────────────────────────────────────╯
╭─Structure─────────────────────────────────────────────────╮
│ 📁 docs                 / README-template.md             │
│ ├── 📁 shared             ├── badges.md [1]              │
│ ├── 📁 sections           ├── why-mergeincludes.md [2]   │
│ │   ├──                   ├── quick-start.md [3]         │
│ │   ├──                   ├── demo-showcase.md [4]       │
│ │   ├──                   ├── directive-reference.md [5] │
│ │   ├──                   ├── use-cases.md [6]           │
│ │   ├──                   ├── command-reference.md [7]   │
│ │   ├──                   ├── advanced-features.md [8]   │
│ │   └──                   ├── technical-specs.md [9]     │
│ └── 📁 shared             └── footer.md [10]             │
╰───────────────────────────────────────────────────────────╯
```

This README demonstrates MergeIncludes in real-world use:

**📁 Documentation Structure:**
```
docs/
├── 📄 README-template.md     ← Main template (you're reading its output!)
├── 📁 sections/
│   ├── 📄 why-mergeincludes.md
│   ├── 📄 quick-start.md
│   ├── 📄 demo-showcase.md
│   ├── 📄 directive-reference.md
│   ├── 📄 use-cases.md
│   ├── 📄 command-reference.md
│   ├── 📄 advanced-features.md
│   └── 📄 technical-specs.md
└── 📁 shared/
    ├── 📄 badges.md
    └── 📄 footer.md
```

**🔄 Build Process:**
```bash
# Generate this README from modular sections
MergeIncludes ./docs/README-template.md -o ./README.md
```

```bash
$ MergeIncludes ./docs/README-template.md --watch -o ./README.md
🔍 Watching for changes... Press any key to stop.

📝 File changed: ./docs/sections/quick-start.md
🔄 Rebuilding README.md... Done! ✨
```

---


Transform any text file into a powerful template with these simple directives:

| Directive | Behavior | Perfect For | Example |
|-----------|----------|-------------|---------|
| `#include` | Always includes content<br/>*(allows duplicates)* | Dynamic content, shared components | `#include ./sections/intro.md` |
| `#require` | Include once only<br/>*(prevents duplicates)* | Libraries, configuration, headers | `#require ./config/database.yml` |
| `*.wildcards` | Include multiple files<br/>*(alphabetical order)* | Batch processing, all files in folder | `#include ./chapters/ch*.md` |
| `## comments` | Ignored in output<br/>*(template notes)* | Development notes, TODOs | `## TODO: Add examples` |


**⚠️ Directives MUST start at the beginning of a line:**

```text
✅ VALID:
    #include ./file.txt
    // #include ./module.js
    <!-- #include ./component.html -->
    # This is a comment with include ./config.yml

❌ INVALID (not processed):
        #include ./file.txt        # Indented - ignored
    text #include ./file.txt       # Mid-line - ignored
```

**Comment Integration:**
- Comment prefixes (`//`, `#`, `<!-- -->`) are part of the directive
- Allows natural integration into any file format
- Comments are automatically stripped from output

```text
📝 Markdown         <!-- #include ./section.md -->
🌐 HTML             <!-- #include ./component.html -->
⚙️  YAML/Config     # #include ./database.yml
🔧 JavaScript       // #include ./utils.js
🐍 Python           # #include ./helpers.py
📄 Plain Text       #include ./content.txt
🎨 CSS              /* #include ./variables.css */
📊 JSON             // #include ./schema.json
🦀 Rust             // #include ./module.rs
🔷 TypeScript       // #include ./types.ts
🏗️  Dockerfile      # #include ./build-steps
```


Process multiple files with patterns (alphabetical order):

```text
Examples (escaped to prevent processing):
  #include ./docs/01-*.md     → 01-intro.md, 01-setup.md
  #include ./modules/core*.js → core.js, core-utils.js  
  #include ./config/*.yml     → All YAML files
```

**Wildcard Behavior:**
- `#include` with wildcards: All matching files included (duplicates allowed)
- `#require` with wildcards: Each file included only once across all patterns


**MergeIncludes processes directives even inside code blocks and comments!**

This powerful feature allows for:
- **Template generation** - Build code files from includes
- **Documentation assembly** - Include real code examples
- **Configuration management** - Merge configs within larger files

**To prevent processing in examples:**
- **Indent directives** (not at line start)
- **Use escape techniques** shown in examples above
- **Comment out** with extra characters: `# // #include ./file.txt`


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

```
📁 docs/
├── 📄 README-template.md     ← Main template
├── 📁 sections/              ← Modular sections
└── 📁 shared/                ← Reusable components
```

*This very README demonstrates the power of modular documentation!*

---


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

```bash
# Basic merge
MergeIncludes ./main.txt

# Custom output with tree display
MergeIncludes ./docs/guide.md -o ./output/complete.md -d FullPath

# Watch mode for development
MergeIncludes ./project/main.txt --watch --trim false

# Show tree structure only
MergeIncludes ./config/app.yml --tree

# Build this README (meta!)
MergeIncludes ./docs/README-template.md -o ./README.md
```

---


- **Visual Indicators**: `⚠️` symbols mark duplicate references
- **Reference Numbers**: `[1]`, `[2]`, `[3]` track file usage
- **Smart Handling**: `#require` prevents duplicates, `#include` allows them

- **Windows Terminal**: Clickable file links and rich colors
- **VS Code Terminal**: Clean display optimized for integrated terminals
- **Standard Terminals**: Full functionality with graceful fallbacks

- **String Pooling**: Minimizes memory allocations
- **Path Caching**: Optimizes file system operations  
- **Lazy Evaluation**: Processes files only when needed
- **Circular Detection**: Prevents infinite recursion

- **Default**: Side-by-side folder structure and reference trees
- **FullPath**: Complete file paths for debugging
- **RelativePath**: Clean relative paths for documentation

---


- **.NET 9.0** or higher
- **Windows 10+**, **macOS 10.15+**, or **Linux** (any modern distribution)
- **Terminal**: Any ANSI-compatible terminal (enhanced features in Windows Terminal)

- **Text Files**: `.txt`, `.md`, `.html`, `.css`, `.js`, `.py`, `.yml`, `.json`, etc.
- **Encoding**: UTF-8, UTF-16, ASCII
- **Size Limits**: Optimized for files up to 100MB (larger files supported)
- **Path Lengths**: Full Windows long path support

- **Missing Files**: Clear warnings with suggested fixes
- **Circular References**: Automatic detection and prevention
- **Permission Issues**: Helpful error messages
- **Invalid Syntax**: Detailed line-by-line feedback

---


We welcome contributions! Here's how to get started:

1. **Fork** the repository
2. **Create** your feature branch: `git checkout -b feature/amazing-feature`
3. **Commit** your changes: `git commit -m 'Add amazing feature'`
4. **Push** to the branch: `git push origin feature/amazing-feature`
5. **Open** a Pull Request

```bash
git clone <repository-url>
cd MergeIncludes
dotnet restore
dotnet build
dotnet test
```

```bash
# Update this README from modular source
cd docs
MergeIncludes ./README-template.md -o ../README.md

# Watch mode for documentation development
MergeIncludes ./README-template.md --watch -o ../README.md
```

---


This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---


- 🐛 **Bug Reports**: [GitHub Issues](../../issues)
- 💡 **Feature Requests**: [GitHub Discussions](../../discussions)
- 📚 **Documentation**: [Wiki](../../wiki)
- 💬 **Community**: [Discussions](../../discussions)

---

<div align="center">

**⭐ Star this repo if MergeIncludes improves your workflow! ⭐**

*Built with ❤️ using .NET 9 and modern C# practices*

**This README was assembled using MergeIncludes itself - a self-demonstrating example! �✨**

**[Download Latest Release](../../releases) • [View Documentation](../../wiki) • [Join Community](../../discussions)**

</div>


**This README is built using MergeIncludes itself!** 🎉

```bash
# See the magic - build this README from its modular parts
MergeIncludes ./docs/README-template.md -o ./README.md

# Watch the docs update live as you edit sections
MergeIncludes ./docs/README-template.md -o ./README.md --watch
```

**Source Structure:**
```
📁 docs/
├── 📄 README-template.md     ← This template
├── 📁 sections/
│   ├── 📄 quick-start.md
│   ├── 📄 directive-reference.md
│   ├── 📄 use-cases.md
│   └── 📄 command-reference.md
└── 📁 shared/
    ├── 📄 badges.md
    ├── 📄 hero.md
    └── 📄 footer.md
```

**Result:** The complete README.md you're reading right now! 📖✨

---

