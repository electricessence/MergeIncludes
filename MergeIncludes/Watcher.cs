using Spectre.Console;
using System.Diagnostics;
using System.Threading.Channels;

namespace MergeIncludes;

static class FileWatcher
{
	public static IAsyncEnumerable<string> WatchAsync(
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

		cancellationToken.ThrowIfCancellationRequested();

		return WatchAsyncCore(fileList, msDelay, cancellationToken);

		IAsyncEnumerable<string> WatchAsyncCore(
			IList<(string dirName, string fileName)> files,
			int msDelay,
			CancellationToken cancellationToken)
		{
			var changes = Channel.CreateUnbounded<string>();
			var changeReg = new HashSet<string>();

			_ = Task.Run(async () =>
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
				{
					timer.Change(msDelay, Timeout.Infinite);
					lock (changeReg)
					{
						if (!changeReg.Add(e.FullPath)) return;

						if (!changes.Writer.TryWrite(e.FullPath))
						{
							Debug.WriteLine($"{e.FullPath} added after complete was called.");
						}
					}
				}

				// Create a FileSystemWatcher for each file
				var watchers = new List<FileSystemWatcher>();
				foreach (var g in files.GroupBy(e => e.dirName))
				{
					var watcher = new FileSystemWatcher(g.Key)
					{
						NotifyFilter
							= NotifyFilters.Attributes
							| NotifyFilters.FileName
							| NotifyFilters.LastWrite
							| NotifyFilters.Size,
						IncludeSubdirectories = false,
						EnableRaisingEvents = true
					};

					foreach (var (_, fileName) in g)
						watcher.Filters.Add(fileName);

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
			}, cancellationToken)
			.ContinueWith(_ =>	changes.Writer.Complete());

			return changes.Reader.ReadAllAsync(cancellationToken);
		}
	}

	public static IAsyncEnumerable<string> WatchAsync(
		IEnumerable<FileInfo> files,
		int msDelay,
		CancellationToken cancellationToken)
		=> WatchAsync(files.Select(f => f.FullName), msDelay, cancellationToken);
}