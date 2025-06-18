## Quick Start

### Install
```bash
dotnet tool install --global MergeIncludes
```

### Use
```bash
# Merge a file (creates MyFile.merged.txt)
MergeIncludes MyFile.txt

# Custom output path
MergeIncludes MyFile.txt -o Output.txt

# Watch for changes
MergeIncludes MyFile.txt --watch
```

That's it. Really.

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
