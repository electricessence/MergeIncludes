namespace Spectre.Console.Extensions;

/// <summary>
/// Utilities for terminal capability detection
/// </summary>
internal static class TerminalCapabilities
{
	/// <summary>
	/// Gets a value indicating whether we're running in Windows Terminal.
	/// Cached for performance since environment variables don't change during execution.
	/// </summary>
	public static bool IsWindowsTerminal { get; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION"));

	/// <summary>
	/// Determines if hyperlinks should be created based on terminal capabilities and user preference
	/// </summary>
	/// <param name="forceCreation">If true, always create links regardless of terminal</param>
	/// <returns>True if links should be created</returns>
	public static bool ShouldCreateHyperlinks(bool forceCreation = false) => forceCreation || IsWindowsTerminal;
}
