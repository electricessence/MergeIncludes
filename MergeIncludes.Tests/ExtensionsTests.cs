using System.Text;

namespace MergeIncludes.Tests;

[UsesVerify]
public static class ExtensionsTests
{
	[Fact]
	public static async Task MergeIncludesAsync()
	{
		var root = new FileInfo(@".\sample.txt");
		var sb = new StringBuilder();
		await foreach(var line in root.MergeIncludesAsync())
		{
			sb.AppendLine(line);
		}

		await Verify(sb.ToString());
	}

	[Fact]
	public static Task FailRecursionAsync()
		=> Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			var root = new FileInfo(@".\sample-recursive.txt");
			await foreach (var line in root.MergeIncludesAsync())
			{
			}
		});
}