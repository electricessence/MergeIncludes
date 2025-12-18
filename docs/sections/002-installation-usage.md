## 📦 Installation

*Coming soon as a .NET global tool.*

Currently, you can run it directly from source using the .NET SDK:

```sh
dotnet run --project MergeIncludes
```

### Installing from Source as a Global Tool

If you have the source code, you can install MergeIncludes as a global .NET tool on your machine:

```bash
# Clone the repository (if you haven't already)
git clone https://github.com/electricessence/MergeIncludes.git
cd MergeIncludes

# Pack the project as a tool (without modifying the .csproj)
dotnet pack -c Release -p:PackAsTool=true -p:ToolCommandName=mergeincludes -p:PackageIcon="" MergeIncludes/MergeIncludes.csproj

# Install globally from the local package
dotnet tool install --global --add-source ./MergeIncludes/bin/Release MergeIncludes --version 3.1.0
```

After installation, you can use `mergeincludes` from anywhere:

```bash
mergeincludes --help
```

To uninstall:

```bash
dotnet tool uninstall --global mergeincludes
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
