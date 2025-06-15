using Spectre.Console;

namespace MergeIncludes.Renderables;

/// <summary>
/// Configuration for TextPath that retains the original path.
/// </summary>
public readonly record struct TextPathConfig
{
	/// <summary>
	/// Constructs a new instance of <see cref="TextPathConfig"/> with the specified path.
	/// </summary>
	/// <param name="path">The file system path to be represented.</param>
	/// <exception cref="ArgumentNullException">If the <paramref name="path"/> is null.</exception>
	public TextPathConfig(string path)
	{
		Path = path ?? throw new ArgumentNullException(nameof(path));
		TextPath = new TextPath(path);
	}

	/// <summary>
	/// Gets the original path provided.
	/// </summary>
	public string Path { get; }

	/// <summary>
	/// The TextPath instance that represents the file system path.
	/// </summary>
	public TextPath TextPath { get; }

	/// <summary>
	/// Implicitly converts a <see cref="TextPathConfig"/> to a <see cref="TextPath"/>.
	/// </summary>
	public static implicit operator TextPath(TextPathConfig config) => config.TextPath;
}
