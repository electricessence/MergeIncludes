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

### 🎭 **Meta Demo: This README!**
```bash
# This very README is built using MergeIncludes!
cd docs
MergeIncludes ./README-template.md -o ../README.md

# Watch mode for documentation development
MergeIncludes ./README-template.md --watch -o ../README.md
```

*Yes, you're reading documentation that was assembled by the very tool it describes! 🤯*

---
