## 📦 Installation

*Coming soon as a .NET global tool.*

Currently, you can run it directly from source using the .NET SDK:

```sh
dotnet run --project MergeIncludes
```

## 🛠️ Usage

```sh
mergeincludes <entry-file> [--output <output-file>]
```

* `entry-file`: The root file that contains include directives.
* `--output`: (Optional) Destination file for merged output. Defaults to `entryFile.merged`.

### Example

Say you have a file like:

```txt
##require ./intro.txt
##require ./details.txt
```

Running:

```sh
mergeincludes main.txt --output final.txt
```

...produces a single `final.txt` with all dependencies flattened inline.
