## Self-Demonstrating

This README is built from modular sections using MergeIncludes itself:

![README Generation](./docs/images/merge-include-readme.png)

The template uses wildcard includes to automatically gather all numbered sections:

```bash
# Build this README (uses wildcard: ./sections/*.md)
MergeIncludes ./docs/README-template.md -o ./README.md

# Watch for changes during development
./docs/update-readme.ps1 -Watch
```

The wildcard `./sections/*.md` includes all numbered sections in alphabetical order, demonstrating the reliable ordering feature.
