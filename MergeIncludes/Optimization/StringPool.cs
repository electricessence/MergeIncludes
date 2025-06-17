using System.Buffers;

namespace MergeIncludes.Optimization;

/// <summary>
/// Provides pooled string operations to reduce GC pressure during file processing
/// </summary>
internal static class StringPool
{
	private static readonly ArrayPool<char> CharPool = ArrayPool<char>.Shared;

	/// <summary>
	/// Efficiently combines path parts using pooled memory
	/// </summary>
	public static string CombinePath(ReadOnlySpan<char> basePath, ReadOnlySpan<char> relativePath)
	{
		var totalLength = basePath.Length + 1 + relativePath.Length; // +1 for separator
		var buffer = CharPool.Rent(totalLength);

		try
		{
			var span = buffer.AsSpan(0, totalLength);
			basePath.CopyTo(span);
			span[basePath.Length] = Path.DirectorySeparatorChar;
			relativePath.CopyTo(span[(basePath.Length + 1)..]);

			return new string(span);
		}
		finally
		{
			CharPool.Return(buffer);
		}
	}

	/// <summary>
	/// Efficiently builds tree indent strings using pooled memory
	/// </summary>
	public static string BuildTreeIndent(int depth, ReadOnlySpan<char> branchChar, ReadOnlySpan<char> continueChar)
	{
		const int CharsPerLevel = 4; // "│   " or "├── "
		var totalLength = depth * CharsPerLevel;
		var buffer = CharPool.Rent(totalLength);

		try
		{
			var span = buffer.AsSpan(0, totalLength);
			for (int i = 0; i < depth; i++)
			{
				var levelSpan = span.Slice(i * CharsPerLevel, CharsPerLevel);
				if (i == depth - 1)
				{
					branchChar.CopyTo(levelSpan);
				}
				else
				{
					continueChar.CopyTo(levelSpan);
				}
			}

			return new string(span);
		}
		finally
		{
			CharPool.Return(buffer);
		}
	}
}
