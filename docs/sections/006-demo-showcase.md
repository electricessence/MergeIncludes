## 🎬 See It In Action

### Beautiful Tree Visualization
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

### Self-Demonstrating Example
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

### Watch Mode Magic
```bash
$ MergeIncludes ./docs/README-template.md --watch -o ./README.md
🔍 Watching for changes... Press any key to stop.

📝 File changed: ./docs/sections/quick-start.md
🔄 Rebuilding README.md... Done! ✨
```

---
