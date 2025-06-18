## Quick Start

### Installation

Download the latest release from [GitHub Releases](https://github.com/electrified/merge-includes/releases) or build from source:

```bash
git clone https://github.com/electrified/merge-includes.git
cd merge-includes
dotnet build -c Release
```

### Basic Usage

1. **Simple merge**:
   ```bash
   MergeIncludes input.txt -o output.txt
   ```

2. **With tree visualization**:
   ```bash
   MergeIncludes input.txt -o output.txt --tree
   ```

3. **Watch mode for live updates**:
   ```bash
   MergeIncludes input.txt -o output.txt --watch
   ```

### Example File Structure

**main.md**:
```markdown
# My Document
\#include intro.md
\#include sections/*.md
\#include footer.md
```

**intro.md**:
```markdown
Welcome to my document!
```

**sections/chapter1.md**:
```markdown
## Chapter 1
Content here...
```

Run `MergeIncludes main.md -o complete.md` to merge everything into one file.
