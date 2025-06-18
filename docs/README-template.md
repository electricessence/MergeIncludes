# MergeIncludes

**Merge modular text files with `#include` or `#require`directives.**

![MergeIncludes Logo](Logo.png)

*This README is built using MergeIncludes itself*

<!-- #include ./shared/badges.md -->

## What it does

MergeIncludes processes text files containing `#include` directives and merges them into a single output file. Perfect for documentation, configuration files, and any text assembly workflow.

**Input**: Multiple modular files with include directives  
**Output**: Single unified file with all content merged

![README Generation](./docs/images/merge-include-readme.png)

<!-- #include ./sections/*.md -->

## Terminal Output

MergeIncludes shows you what it's doing with clear tree visualization:

![Tree Visualization](./docs/assets/screenshots/tree-visualization.png)

Watch mode rebuilds automatically when files change:

![Watch Mode](./docs/assets/screenshots/watch-mode.png)

## Self-Demonstrating

This README is built from modular sections using wildcard includes:

```bash
# Build this README (uses wildcard: ./sections/*.md)
MergeIncludes ./docs/README-template.md -o ./README.md

# Watch for changes  
./docs/update-readme.ps1 -Watch
```

The wildcard includes all numbered sections in order, demonstrating the guaranteed ordering feature.

![Before and After](./docs/assets/screenshots/before-after.png)

<!-- #include ./shared/footer.md -->
