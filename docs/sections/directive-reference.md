## 📖 Directive Reference

Transform any text file into a powerful template with these simple directives:

| Directive | Behavior | Perfect For | Example |
|-----------|----------|-------------|---------|
| `#include` | Always includes content<br/>*(allows duplicates)* | Dynamic content, shared components | `#include ./sections/intro.md` |
| `#require` | Include once only<br/>*(prevents duplicates)* | Libraries, configuration, headers | `#require ./config/database.yml` |
| `*.wildcards` | Include multiple files<br/>*(alphabetical order)* | Batch processing, all files in folder | `#include ./chapters/ch*.md` |
| `## comments` | Ignored in output<br/>*(template notes)* | Development notes, TODOs | `## TODO: Add examples` |

### 🔍 Critical Parsing Rules

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

### Works in Any File Format 📄
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

### 🃏 Wildcard Patterns

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

### ⚠️ Important: Code Block Processing

**MergeIncludes processes directives even inside code blocks and comments!**

This powerful feature allows for:
- **Template generation** - Build code files from includes
- **Documentation assembly** - Include real code examples
- **Configuration management** - Merge configs within larger files

**To prevent processing in examples:**
- **Indent directives** (not at line start)
- **Use escape techniques** shown in examples above
- **Comment out** with extra characters: `# // #include ./file.txt`
