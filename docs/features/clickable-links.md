## Clickable File Links

File paths in the console output become clickable links in supported terminals.

### Supported Terminals

✅ **Windows Terminal** - Full hyperlink support  
❌ **VS Code Terminal** - Disabled (plain text paths)
✅ **Modern terminals** with hyperlink support  
❌ **Legacy terminals** - Shows plain text paths  

### How It Works

When running MergeIncludes in Windows Terminal, file paths in the tree output become clickable:

- Click file paths → Opens file in default editor
- Click folder paths → Opens folder in file explorer

### Example

**In Windows Terminal:**
```
📁 docs/sections/intro.md    ← Clickable
└── footer.md                ← Clickable
```

**In other terminals:**
```
docs/sections/intro.md       ← Plain text
└── footer.md                ← Plain text
```

No configuration required - automatically detects terminal capabilities.
