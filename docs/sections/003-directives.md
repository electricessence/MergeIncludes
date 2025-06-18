## 🧠 Directive Behavior

| Directive  | Effect                               |
| ---------- | ------------------------------------ |
| `#include` | Always includes target file content. |
| `#require` | Includes only once per session.      |

Both directives support:
- **Wildcards**: `#include sections/*.md`
- **Relative paths**: `#include ../shared/header.txt`
- **Nested includes**: Files can include other files recursively
