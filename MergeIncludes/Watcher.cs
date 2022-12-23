namespace MergeIncludes;

static class FileWatcher
{
	public static Task WatchAsync(
		IEnumerable<string> files,
		int msDelay,
		CancellationToken cancellationToken)
	{
		if (files is null)
			throw new ArgumentNullException(nameof(files));

		var fileList = files
			.Select(path =>
			{
				if (path is null)
					throw new ArgumentException("File paths cannot be null.", nameof(files));
				if (string.IsNullOrWhiteSpace(path))
					throw new ArgumentException("File paths cannot be blank.", nameof(files));

				return (
					Path.GetDirectoryName(path) ?? throw new ArgumentException($"Cannot parse directory for: {path}", nameof(files)),
					Path.GetFileName(path));
			})
			.ToArray();

		if (fileList.Length == 0)
			throw new ArgumentException("No files provided.", nameof(files));

		if (cancellationToken.IsCancellationRequested)
			return Task.FromCanceled(cancellationToken);

		return WatchAsyncCore(fileList, msDelay, cancellationToken);

		async Task WatchAsyncCore(
			IList<(string, string)> files,
			int msDelay,
			CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource();
			using var ctr = cancellationToken
				.Register(() => tcs.TrySetCanceled(cancellationToken));

			// Create a timer that will complete after the specified delay
			// if no more changes have occurred
			using var timer = new Timer(
				(_) => tcs.TrySetResult(),
				null,
				Timeout.Infinite,
				Timeout.Infinite);

			// Reset the timer anytime a file changes
			void OnChanged(object sender, FileSystemEventArgs e)
				=> timer.Change(msDelay, Timeout.Infinite);

			// Create a FileSystemWatcher for each file
			var watchers = new List<FileSystemWatcher>();
			foreach (var (dirName, fileName) in files)
			{
				var watcher = new FileSystemWatcher(dirName, fileName)
				{
					NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
					EnableRaisingEvents = true,
				};

				watcher.Changed += OnChanged;
				watchers.Add(watcher);
			}

			try
			{
				await tcs.Task.ConfigureAwait(false);
				cancellationToken.ThrowIfCancellationRequested();
			}
			finally
			{
				timer.Change(Timeout.Infinite, Timeout.Infinite);
				// Clean up the FileSystemWatchers and the timer
				foreach (var watcher in watchers)
				{
					watcher.Changed -= OnChanged;
					watcher.Dispose();
				}
			}
		}
	}

	public static Task WatchAsync(
		IEnumerable<FileInfo> files,
		int msDelay,
		CancellationToken cancellationToken)
		=> WatchAsync(files.Select(f => f.FullName), msDelay, cancellationToken);
}