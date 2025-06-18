## Circular Reference Detection

Prevents infinite include loops by detecting when files include each other.

### What Are Circular References?

When files include each other, creating a loop:

```
file-a.txt  →  includes file-b.txt
file-b.txt  →  includes file-a.txt  ← Circular!
```

### How It Works

- **Detection:** MergeIncludes tracks the include chain and stops when a file tries to include itself again
- **Result:** Merge operation fails with an error message
- **No visual indicators:** Currently just prevents the merge from completing

### Example Error

When a circular reference is detected, the merge fails and you get an error message indicating the problem.

### Self-References

Files that include themselves are also detected:

```
# In self-ref.txt
This file includes itself:
#include ./self-ref.txt  ← Fails immediately
```
