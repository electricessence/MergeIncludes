## Feature Index

Complete reference of MergeIncludes features.

### Core Features

#### [Tree Visualization](./tree-visualization.md)
Console output showing folder structure and include relationships with reference numbering.

#### [Wildcard Patterns](./wildcard-patterns.md)  
Basic wildcard support (`*.txt`) for including multiple files from a directory.

#### [Circular Reference Detection](./circular-reference-detection.md)
Prevents infinite include loops by failing the merge when detected.

#### [Escape Sequences](./escape-sequences.md)
Use `##include` to output literal `#include` text in merged files.

### User Experience Features

#### [Clickable Links](./clickable-links.md)
File paths become clickable in Windows Terminal (disabled in VS Code).

#### [Watch Mode](./watch-mode.md)
Real-time file monitoring with automatic re-merging when changes are detected.

---

### Quick Reference

| Feature | Command Flag | Description |
|---------|--------------|-------------|
| Watch Mode | `--watch` | Monitor files for changes |
| Display Mode | `--display-mode` | Control tree output format |
| Trim Whitespace | `--trim` | Remove leading/trailing whitespace |
| Output File | `-o`, `--output` | Specify output file path |

### Common Use Cases

- **Documentation assembly** - Combine multiple markdown files
- **Configuration management** - Merge environment-specific configs  
- **Content workflows** - Assemble modular text content
