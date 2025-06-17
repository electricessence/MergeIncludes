using System.Text;

namespace MergeIncludes.Tests;

[UsesVerify]
public static class ExtensionsTests
{
	[Fact]
	public static async Task MergeIncludesAsync()
	{
		var root = new FileInfo(@"..\..\..\TestScenarios\08_CommentedIncludes\root.txt");
		var sb = new StringBuilder();
		await foreach (var line in root.MergeIncludesAsync())
		{
			sb.AppendLine(line);
		}

		await Verify(sb.ToString());
	}
	[Fact]
	public static async Task MergeIncludesWildCardAsync()
	{
		var root = new FileInfo(@"..\..\..\TestScenarios\07_WildcardIncludes\root.txt");
		var sb = new StringBuilder();
		await foreach (var line in root.MergeIncludesAsync())
		{
			sb.AppendLine(line);
		}

		await Verify(sb.ToString());
	}
	[Fact]
	public static Task FailRecursionAsync()
		=> Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			var root = new FileInfo(@"..\..\..\TestScenarios\03_CircularReferences\simple-recursive.txt");
			await foreach (var line in root.MergeIncludesAsync())
			{
			}
		});
}