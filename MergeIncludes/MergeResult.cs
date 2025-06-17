namespace MergeIncludes;

/// <summary>
/// Result of a merge operation, containing both the merged content and metadata
/// </summary>
public class MergeResult
{	/// <summary>
	/// Whether the merge was successful
	/// </summary>
	public bool IsSuccess { get; init; }

	/// <summary>
	/// The merged content as a string (null if merge failed)
	/// </summary>
	public string? MergedContent { get; init; }

	/// <summary>
	/// List of all files that were processed during the merge
	/// </summary>
	public List<FileInfo> ProcessedFiles { get; init; } = [];

	/// <summary>
	/// Dictionary mapping parent files to their included children
	/// </summary>
	public Dictionary<string, List<string>> FileRelationships { get; init; } = [];

	/// <summary>
	/// Error message if the merge failed (null if successful)
	/// </summary>
	public string? ErrorMessage { get; init; }

	/// <summary>
	/// Create a successful merge result
	/// </summary>
	public static MergeResult Success(string mergedContent, List<FileInfo> processedFiles, Dictionary<string, List<string>> fileRelationships)
	{		return new MergeResult
		{
			IsSuccess = true,
			MergedContent = mergedContent,
			ProcessedFiles = processedFiles,
			FileRelationships = fileRelationships
		};
	}

	/// <summary>
	/// Create a failed merge result
	/// </summary>
	public static MergeResult Failure(string errorMessage, List<FileInfo> processedFiles, Dictionary<string, List<string>> fileRelationships)
	{		return new MergeResult
		{
			IsSuccess = false,
			ErrorMessage = errorMessage,
			ProcessedFiles = processedFiles,
			FileRelationships = fileRelationships
		};
	}
}
