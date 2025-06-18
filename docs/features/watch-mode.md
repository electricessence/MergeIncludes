## Watch Mode

Monitor your files for changes and automatically re-merge when modifications are detected.

### Enabling Watch Mode

```bash
MergeIncludes main.txt --watch
```

### How It Works

1. **Initial merge** - Processes files normally
2. **File monitoring** - Watches all included files for changes  
3. **Auto re-merge** - Re-processes when files are modified
4. **Live feedback** - Shows what changed and when

### Example Output

```
🔄 Watching files for changes. (Press any key to stop watching.)

Changes detected: (12/17/2025 2:34:15 PM)
📄 docs/sections/intro.md
📄 docs/sections/features.md

📁 docs / README-template.md
├── 📁 sections ├── intro.md      ← Updated
│   ├──         ├── features.md   ← Updated  
│   └──         └── conclusion.md
└── 📁 shared   └── footer.md

✅ README.md updated successfully!
```

### Stopping Watch Mode

- **Any key press** - Stops monitoring and exits
- **Ctrl+C** - Force termination
