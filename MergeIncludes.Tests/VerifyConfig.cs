using System.Runtime.CompilerServices;

namespace MergeIncludes.Tests;

public static class VerifyConfig
{
	[ModuleInitializer]
	public static void Initialize()
	{
		// Disable Unicode scrubbing in Verify
		VerifierSettings.DisableRequireUniquePrefix();

		// Detect if we're running in Visual Studio or development environment
		var isVisualStudio = Environment.GetEnvironmentVariable("VisualStudioVersion") != null ||
						   Environment.GetEnvironmentVariable("DevEnvDir") != null ||
						   Environment.GetEnvironmentVariable("VS_CommonTools") != null;

		var isInteractive
			= Environment.UserInteractive
		   && !Environment.GetEnvironmentVariable("CI")?
				.ToLower().Contains("true", StringComparison.CurrentCultureIgnoreCase) == true;
		// Always disable auto-verify to ensure tests fail when output doesn't match expected
		// This prevents automatic overwriting of verified files with incorrect output
		Console.WriteLine("Verify: Auto-verify disabled - manual approval required for all changes");

		// Configure better console output for failed verifications
		VerifierSettings.OnVerifyMismatch((filePair, message) =>
		{
			Console.WriteLine($"\nVerify mismatch detected:");
			Console.WriteLine($"Expected: {filePair.VerifiedPath}");
			Console.WriteLine($"Received: {filePair.ReceivedPath}");
			Console.WriteLine($"Message: {message}");

			// In automated mode, show the content differences
			if (!isVisualStudio && !isInteractive)
			{
				Console.WriteLine("\nTo approve this change, copy the .received.txt file to .verified.txt");

				// Try to show the diff in console if files exist
				if (File.Exists(filePair.VerifiedPath) && File.Exists(filePair.ReceivedPath))
				{
					Console.WriteLine("\n=== EXPECTED CONTENT ===");
					Console.WriteLine(File.ReadAllText(filePair.VerifiedPath));
					Console.WriteLine("\n=== RECEIVED CONTENT ===");
					Console.WriteLine(File.ReadAllText(filePair.ReceivedPath));
					Console.WriteLine("=== END COMPARISON ===\n");
				}
			}

			return Task.CompletedTask;
		});
	}
}