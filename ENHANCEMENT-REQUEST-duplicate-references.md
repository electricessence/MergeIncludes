// Enhancement Request for MergeIncludes Tool // Submitted by: User // Date:
June 16, 2025

## Duplicate Reference Visualization Enhancement

### Current Behavior:

- Tool filters duplicate references automatically
- Tree shows each file only once
- No visual indication of how many times a file is referenced

### Requested Enhancement:

#### 1. New Command Line Flag:

```bash
--show-duplicates (or -D)    Show all file references including duplicates
```

#### 2. Visual Indicators for Duplicates:

**First Reference (Original):**

- Color: Yellow/Orange (different from default Cyan1)
- Format: `filename.pine [1]`
- Shows: Full dependency tree as normal

**Subsequent References (Duplicates):**

- Color: Gray/Dimmed
- Format: `filename.pine [1]` (same ID number)
- Shows: No children (collapsed) with note "→ See reference [1]"

#### 3. Example Output:

```
📁 project / root.pine
├── shared-utility.pine [1]                    # Yellow/Orange - first reference
│   └── common-functions.pine [2]              # Yellow/Orange - first reference
├── module-a.pine
│   └── shared-utility.pine [1]                # Gray - duplicate, no children
├── module-b.pine
│   ├── shared-utility.pine [1]                # Gray - duplicate, no children
│   └── common-functions.pine [2]              # Gray - duplicate, no children
└── shared-utility.pine [1]                    # Gray - duplicate, no children
```

#### 4. Implementation Details:

**Data Structure Changes:**

- Track reference count per file path
- Store first occurrence location
- Maintain reference ID mapping

**Display Logic:**

- Assign sequential IDs [1], [2], [3] to unique files
- Use different colors for first vs duplicate references
- Collapse duplicate subtrees with reference note

**Backward Compatibility:**

- Default behavior unchanged (filter duplicates)
- New flag enables duplicate visualization
- Existing scripts continue to work

#### 5. Code Changes Required:

**Settings.cs:**

```csharp
[Description("Show all file references including duplicates with visual indicators")]
[CommandOption("-D|--show-duplicates")]
public bool ShowDuplicates { get; set; } = false;
```

**Command.Display.cs:**

- Modify `BuildReferenceTreeRecursive` to track duplicates
- Add reference counting logic
- Implement color-coded display for duplicates

#### 6. Use Cases:

1. **Dependency Analysis:** See how many times a utility is referenced
2. **Optimization:** Identify frequently referenced files for potential
   optimization
3. **Debugging:** Understand complex dependency trees with multiple paths
4. **Refactoring:** Visualize impact of moving shared utilities

#### 7. Testing Scenarios:

```pine
// Test Case 1: Direct duplicates
// #require ./shared.pine
// #require ./shared.pine     # Should show as [1] duplicate

// Test Case 2: Indirect duplicates
// #require ./module-a.pine   # Contains shared.pine
// #require ./module-b.pine   # Also contains shared.pine

// Test Case 3: Mixed include types
// #require ./shared.pine
// #include ./shared.pine     # Different directive, same file
```

This enhancement will make Pine Script dependency analysis much more powerful
and transparent!
