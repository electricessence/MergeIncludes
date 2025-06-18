## How it works

### Before: Modular files
```
📁 project/
├── main.txt
├── sections/
│   ├── intro.txt
│   └── features.txt  
└── footer.txt
```

**main.txt:**
```text
# My Project

##include ./sections/intro.txt
##include ./sections/features.txt
##include ./footer.txt
```

### After: Unified result

Run `MergeIncludes main.txt` and get `main.merged.txt` with all content combined.

![Basic Usage](./docs/assets/screenshots/basic-usage.png)
