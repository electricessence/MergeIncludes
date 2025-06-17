namespace Spectre.Console.Extensions;

/// <summary>
/// Utilities for detecting terminal-specific capabilities
/// </summary>
public static class TerminalCapabilities
{   /// <summary>
	/// Gets a value indicating whether the current terminal supports clickable hyper-links.
	/// Cached for performance since environment variables don't change during execution.
	/// </summary>
	public static bool Links { get; } = DetectHyperlinkSupport();

	/// <summary>
	/// Gets a value indicating whether we're running in Windows Terminal specifically.
	/// Useful for Windows Terminal-specific features beyond just hyper-links.
	/// </summary>
	public static bool IsWindowsTerminal { get; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION"));

	/// <summary>
	/// Determines if hyper-links should be created based on terminal capabilities and user preference
	/// </summary>
	/// <param name="forceCreation">If true, always create links regardless of terminal capabilities</param>
	/// <returns>True if links should be created</returns>
	public static bool ShouldCreateHyperlinks(bool forceCreation = false) => forceCreation || Links;
	/// <summary>
	/// Detects if the current terminal supports clickable hyper-links
	/// </summary>
	private static bool DetectHyperlinkSupport()
	{
		// Windows Terminal
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION")))
			return true;

		// VS Code integrated terminal - currently has display corruption issues with hyperlinks
		// Commented out until VS Code fixes hyperlink rendering
		// if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_INJECTION")))
		//     return true;

		// iTerm2 (macOS)
		var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
		if (string.Equals(termProgram, "iTerm.app", StringComparison.OrdinalIgnoreCase))
			return true;

		// Terminal.app on macOS (newer versions)
		if (string.Equals(termProgram, "Apple_Terminal", StringComparison.OrdinalIgnoreCase))
		{
			// Could add version detection here if needed
			return true;
		}

		// GNOME Terminal and other VTE-based terminals
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VTE_VERSION")))
			return true;

		// Kitty terminal
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KITTY_WINDOW_ID")))
			return true;

		// Hyper terminal
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HYPER")))
			return true;

		// Future: Could add more terminal detection here
		// For now, default to false for unknown terminals
		return false;
	}
}
