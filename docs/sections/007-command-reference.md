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

# Build this README (meta!)
MergeIncludes ./docs/README-template.md -o ./README.md
```

---
