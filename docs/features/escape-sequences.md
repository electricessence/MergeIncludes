## Escape Sequences

Use escape sequences to include literal `#include` text in your merged output.

### Basic Escaping

When you need literal `#include` text in your output, use **double hash**:

```
##include ./config.txt
```

**Becomes:** `#include ./config.txt` in the merged output

### Common Use Cases

#### Documentation Examples
```markdown
To include a file, use:
##include ./path/to/file.txt
```

#### Configuration Comments  
```
# This line would normally include a file:
##include ./settings.conf
```

#### Code Examples
```javascript
// Example of include directive:
##include ./module.js
```

### Processing Rules

| Input | Output | Action |
|-------|--------|--------|
| `#include file.txt` | *(file contents)* | Processes include |
| `##include file.txt` | `#include file.txt` | Literal text |
| `###include file.txt` | `##include file.txt` | Reduces by one # |

### Nested Escaping

Multiple hash symbols are reduced by one:

```
####include ./file.txt  →  ###include ./file.txt
###include ./file.txt   →  ##include ./file.txt  
##include ./file.txt    →  #include ./file.txt
#include ./file.txt     →  (processes the include)
```

### Real-World Example

**Input file (tutorial.md):**
```markdown
# How to Use Includes

To include another file in your document:

##include ./example.txt

This will merge the contents of example.txt into your document.
```

**Output:**
```markdown
# How to Use Includes

To include another file in your document:

#include ./example.txt

This will merge the contents of example.txt into your document.
```

### Technical Details

- **Pattern matching:** `^##include\s+(.+)$`
- **Line-by-line processing:** Each line evaluated independently  
- **Whitespace preserved:** Maintains original indentation
- **Case sensitive:** Only `##include` triggers escaping
- **UTF-8 safe:** Works with international characters
