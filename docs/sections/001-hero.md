## 🚀 Overview

Many scripting and configuration languages lack native support for modular file structures. `MergeIncludes` addresses that by recursively resolving custom `#include` and `#require` statements—merging multiple files into a single output.

It's designed to be flexible and supports any file type, from source code to plain text.

## 🔧 Key Features

- 🧩 Recursively resolves `#include` and `#require` statements
- ➕ Supports nested, relative includes
- ♻️ `#require` avoids duplicate includes
- 🌟 Wildcard patterns like `*.md` or `sections/*.txt`
- 👁️ Tree visualization shows file structure
- ⚡ Watch mode for live rebuilds
- 🔗 Clickable terminal paths (where supported)
- 🛡️ Circular reference detection
