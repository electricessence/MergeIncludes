## Directives

| Directive | Purpose | Example |
|-----------|---------|---------|
| `#include` | Include file content | `#include ./section.md` |
| `#require` | Include only once | `#require ./header.txt` |
| `##include` | Show literal text | `##include ./example.txt` |

### Works in any file format
- Plain text: `#include ./file.txt`
- Markdown: `<!-- #include ./section.md -->`
- Code: `// #include ./module.js`
- Config: `# #include ./settings.yml`

### Wildcards
```text
##include ./docs/*.md        # All .md files
##include ./chapters/0*.txt  # Files starting with 0
```
