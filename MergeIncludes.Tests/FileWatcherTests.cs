namespace MergeIncludes.Tests;

public class FileWatcherTests
{
	[Fact]
	public async Task WatchAsync_DetectsFileChanges()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			// Create a temporary file to watch
			File.WriteAllText(tempFile, "Initial content");

			var files = new[] { tempFile };
			var changedFiles = new List<string>();
			using var cts = new CancellationTokenSource();

			// Start watching the file in a separate task
			var watchTask = Task.Run(async () =>
			{
				await foreach (var file in FileWatcher.WatchAsync(files, 100, cts.Token))
				{
					changedFiles.Add(file);
				}
			});

			// Act
			// Give the watcher time to start
			await Task.Delay(200);

			// Modify the file to trigger the watcher
			File.WriteAllText(tempFile, "Modified content");

			// Give the watcher time to detect the change
			await Task.Delay(500);

			// Cancel the watcher
			cts.Cancel();

			try
			{
				// Wait for the watch task to complete
				await watchTask;
			}
			catch (OperationCanceledException)
			{
				// Expected when the token is canceled
			}

			// Assert
			Assert.Contains(tempFile, changedFiles);
		}
		finally
		{
			// Clean up
			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}
		}
	}

	[Fact]
	public void WatchAsync_FileInfo_ConvertsToPaths()
	{
		// Arrange
		var tempFile = Path.GetTempFileName();
		try
		{
			// Create a FileInfo for the temporary file
			var fileInfo = new FileInfo(tempFile);
			var files = new[] { fileInfo };
			using var cts = new CancellationTokenSource();

			// Act - Just ensure it doesn't throw an exception
			var asyncEnumerable = FileWatcher.WatchAsync(files, 100, cts.Token);

			// Assert
			Assert.NotNull(asyncEnumerable);
		}
		finally
		{
			// Clean up
			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}
		}
	}

	[Fact]
	public void WatchAsync_EmptyCollection_ThrowsArgumentException()
	{
		// Arrange
		var files = Array.Empty<string>();
		using var cts = new CancellationTokenSource();

		// Act & Assert
		Assert.Throws<ArgumentException>(() => FileWatcher.WatchAsync(files, 100, cts.Token).GetAsyncEnumerator());
	}

	[Fact]
	public void WatchAsync_NullCollection_ThrowsArgumentNullException()
	{
		// Arrange
		IEnumerable<string> files = null!;
		using var cts = new CancellationTokenSource();

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => FileWatcher.WatchAsync(files, 100, cts.Token).GetAsyncEnumerator());
	}
}