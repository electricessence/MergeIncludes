using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Throw;

namespace MergeIncludes;
public static partial class Extensions
{
    const string INCLUDE = "include";
    const string REQUIRE = "require";
    const string EXACT = "exact";
    const string METHOD = "method";
    const string FILE = "file";
    const string CommentPatternText = "##.+";
    const string IncludePatternText = @$"#(?<{METHOD}>{INCLUDE}|{REQUIRE})(?<{EXACT}>-{EXACT})?\s+(?<{FILE}>.+)";

    [GeneratedRegex(
        @$"^(//\s*)?{IncludePatternText}|^(<!--\s*){IncludePatternText}(\s*-->)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex GetIncludePattern();

    [GeneratedRegex(
        @$"^(//\s*)?{CommentPatternText}|^(<!--\s*){CommentPatternText}(\s*-->)",
        RegexOptions.Compiled)]
    private static partial Regex GetCommentPattern();

    public static IAsyncEnumerable<string> MergeIncludesAsync(
        this FileInfo root,
        MergeOptions? options = null,
        Action<FileInfo>? onFileAccessed = null)
    {
        root.ThrowIfNull();
        Contract.EndContractBlock();

        Dictionary<string, FileInfo> registry = new();
        Register(root.FullName, registry, onFileAccessed);
        return MergeIncludesAsync(root.FullName, registry, options, onFileAccessed);
    }

    static IEnumerable<FileInfo> Register(
        string filePath,
        Dictionary<string, FileInfo> registry,
        Action<FileInfo>? onFileAccessed = null,
        bool newOnly = false)
    {
        filePath.ThrowIfNull().OnlyInDebug();
        registry.ThrowIfNull().OnlyInDebug();
        Contract.EndContractBlock();

        if (registry.TryGetValue(filePath, out var file))
        {
            if(!newOnly) yield return file;
            yield break;
        }

        var dir = Path.GetDirectoryName(filePath)!;
        var fileName = Path.GetFileName(filePath);
        var files = Directory.GetFiles(dir, fileName);

        if (files is null || files.Length == 0)
            throw new FileNotFoundException(filePath);

        foreach (var fp in files)
        {
            if (registry.TryGetValue(fp, out var fi))
            {
                if (!newOnly) yield return fi;
                continue;
            }

            fi = new FileInfo(fp);
            registry.Add(fp, fi);
            onFileAccessed?.Invoke(fi);
            yield return fi;
        }
    }

    static async IAsyncEnumerable<string> MergeIncludesAsync(
        string rootFileName,
        Dictionary<string, FileInfo> registry,
        MergeOptions? options = null,
        Action<FileInfo>? onFileAccessed = null,
        HashSet<string>? active = null,
        bool newOnly = false)
    {
        rootFileName.ThrowIfNull().OnlyInDebug();
        registry.ThrowIfNull().OnlyInDebug();
        Contract.EndContractBlock();

        var files = Register(rootFileName, registry, onFileAccessed, newOnly);

        active ??= new();

        foreach (var root in files)
        {
            await foreach (var e in MergeIncludesAsyncCore(root, registry, options, active, onFileAccessed))
            {
                yield return e;
            }
        }

        static async IAsyncEnumerable<string> MergeIncludesAsyncCore(
            FileInfo root,
            Dictionary<string, FileInfo> registry,
            MergeOptions? options,
            HashSet<string> active,
            Action<FileInfo>? onFileAccessed)
        {
            var rootFileName = root.FullName;
            if (!active.Add(rootFileName))
                throw new UnreachableException();

            var commentPattern = GetCommentPattern();
            var includePattern = GetIncludePattern();
            using var file = root.OpenRead();
            using var reader = new StreamReader(file);
            Lazy<string> path = new(() => root.Directory?.FullName.ThrowIfNull()!);
            var trimming = options?.Trim == true;
            var trimLeading = trimming;
            var lineNumber = 0;
            List<string>? whiteSpace = trimming ? new List<string>() : null;

        more:
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (line is null)
            {
                var pad = options?.Padding ?? 0;
                for (var i = 0; i < pad; i++)
                    yield return string.Empty;

                active.Remove(rootFileName);
                yield break;
            }

            lineNumber++;

            // Trimming enabled?  Track the whitespace.
            if (trimming && string.IsNullOrWhiteSpace(line))
            {
                whiteSpace!.Add(line);
                goto more;
            }

            if (trimLeading)
            {
                trimLeading = false;
                whiteSpace!.Clear();
            }

            if (trimming && whiteSpace!.Count != 0)
            {
                foreach (var w in whiteSpace)
                    yield return w;

                whiteSpace.Clear();
            }

            if (commentPattern.IsMatch(line))
            {
                goto more;
            }

            var include = includePattern.Match(line);
            if (!include.Success)
            {
                yield return line;
                goto more;
            }

            var includePath = Path.GetFullPath(Path.Combine(path.Value, include.Groups[FILE].Value));
            if (active.Contains(includePath))
                throw new InvalidOperationException($"Detected recursive reference to {includePath}.");

            var require = include.Groups[METHOD].ValueSpan.Equals(REQUIRE, StringComparison.OrdinalIgnoreCase);
            // Require means to only include it once.
            if (require && registry.ContainsKey(includePath))
                goto more;

            var exact = include.Groups[EXACT].Success;

            IAsyncEnumerable<string> included;
            try
            {
                included = MergeIncludesAsync(includePath, registry, exact ? null : options, onFileAccessed, active, require);
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException($"Could not find include file on line {lineNumber} in {root.FullName}:{lineNumber}", includePath, ex);
            }

            await foreach (var n in included)
            {
                yield return n;
            }

            trimLeading = trimming;

            goto more;
        }
    }
}
