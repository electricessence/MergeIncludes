## Tree Visualization

Shows your file structure and include relationships in a clear tree format.

### Display Modes

#### Default Mode
Shows both folder structure and reference relationships side-by-side.

#### Full Path Mode  
```
MergeIncludes main.txt --display-mode fullpath
```
Shows complete file paths instead of relative names.

#### Relative Path Mode
```
MergeIncludes main.txt --display-mode relative
```
Shows paths relative to execution directory.

### Reference Numbering

**Only repeated files** get reference numbers:

```
📁 project / main.txt
├──          ├── common.txt [1]    ← First occurrence
├── 📁 sub1  ├── feature.txt
│   └──      └── common.txt [1]    ← Same number for repeated file
└── 📁 sub2  └── other.txt
```
