using System;
using System.Runtime.CompilerServices;
using VerifyTests;

namespace MergeIncludes.Tests;

public static class VerifyConfig
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Disable unicode scrubbing in Verify
        VerifierSettings.DisableRequireUniquePrefix();
    }
}