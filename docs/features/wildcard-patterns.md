## Wildcard Include Patterns

Include multiple files at once using simple wildcard patterns.

### Basic Wildcard Syntax

```
#include ./files/*.txt        ← All .txt files in files/ folder
```

### Supported Patterns

| Pattern | Description | Example |
|---------|-------------|---------|
| `*.ext` | All files with extension | `*.txt`, `*.md`, `*.json` |
| `prefix*` | Files starting with prefix | `config-*.txt` |
| `*suffix` | Files ending with suffix | `*-template.md` |

**Note:** Does not support recursive patterns like `**/*` - only single directory wildcards.

### Processing Order

Files are processed in **alphabetical order** by filename.

### Example

```
project/
├── config/
│   ├── database.conf
│   └── security.conf
└── main.txt
```

In `main.txt`:
```
#include ./config/*.conf
```

Results in:
1. `database.conf` content
2. `security.conf` content
