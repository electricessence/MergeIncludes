## 💡 Real-World Use Cases

### 📚 **Technical Documentation**
```
📁 docs/
├── 📄 user-guide.md          ← Root document
├── 📁 chapters/
│   ├── 📄 installation.md
│   ├── 📄 configuration.md
│   └── 📄 troubleshooting.md
├── 📁 examples/
│   ├── 📄 basic-usage.md
│   └── 📄 advanced-tips.md
└── 📁 shared/
    ├── 📄 header.md
    └── 📄 footer.md
```

```bash
# Build complete documentation
MergeIncludes ./docs/user-guide.md -o ./dist/complete-guide.md --watch
```

### 🔧 **Configuration Management**
```
📁 config/
├── 📄 production.yml         ← Environment config
├── 📁 modules/
│   ├── 📄 database.yml
│   ├── 📄 logging.yml
│   └── 📄 security.yml
└── 📁 secrets/
    └── 📄 api-keys.yml
```

```bash
# Deploy with merged configuration
MergeIncludes ./config/production.yml -o ./deploy/app-config.yml
```

### 🌐 **Static Site Generation**
```
📁 site/
├── 📄 index.html             ← Main template
├── 📁 components/
│   ├── 📄 header.html
│   ├── 📄 navigation.html
│   └── 📄 footer.html
└── 📁 content/
    ├── 📄 hero-section.html
    └── 📄 features.html
```

### 📊 **Report Assembly**
```
📁 reports/
├── 📄 monthly-report.md      ← Report template
├── 📁 sections/
│   ├── 📄 executive-summary.md
│   ├── 📄 financial-data.md
│   └── 📄 recommendations.md
└── 📁 data/
    ├── 📄 charts.md
    └── 📄 tables.md
```

### 🎯 **Living Documentation (Like This README!)**
```
📁 docs/
├── 📄 README-template.md     ← Main template
├── 📁 sections/              ← Modular sections
└── 📁 shared/                ← Reusable components
```

*This very README demonstrates the power of modular documentation!*

---
