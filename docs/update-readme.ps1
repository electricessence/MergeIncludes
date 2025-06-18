#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Updates the root README.md from the modular documentation template.

.DESCRIPTION
    This script builds the README.md file from docs/README-template.md using MergeIncludes.
    It demonstrates the tool's capabilities by using it to generate its own documentation.

.PARAMETER Watch
    Enable watch mode to automatically rebuild when any documentation file changes.

.EXAMPLE
    .\update-readme.ps1
    Builds the README.md once from the template.

.EXAMPLE
    .\update-readme.ps1 -Watch
    Builds the README.md and watches for changes to auto-rebuild.
#>

[CmdletBinding()]
param(
    [switch]$Watch
)

# Get the script directory and project root
$ScriptDir = $PSScriptRoot
$ProjectRoot = Split-Path $ScriptDir -Parent
$TemplatePath = Join-Path $ScriptDir "README-template.md"
$OutputPath = Join-Path $ProjectRoot "README.md"
$MergeIncludesProject = Join-Path $ProjectRoot "MergeIncludes\MergeIncludes.csproj"

Write-Host "🔄 MergeIncludes Documentation Builder" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if template exists
if (-not (Test-Path $TemplatePath)) {
    Write-Error "❌ Template not found: $TemplatePath"
    exit 1
}

# Check if MergeIncludes project exists
if (-not (Test-Path $MergeIncludesProject)) {
    Write-Error "❌ MergeIncludes project not found: $MergeIncludesProject"
    exit 1
}

Write-Host "📁 Template: " -NoNewline -ForegroundColor Yellow
Write-Host (Resolve-Path $TemplatePath -Relative)
Write-Host "📄 Output:   " -NoNewline -ForegroundColor Yellow  
Write-Host (Resolve-Path $OutputPath -Relative)
Write-Host ""

try {
    # Build the command arguments
    $arguments = @(
        "run"
        "--project"
        $MergeIncludesProject
        "--"
        $TemplatePath
        "-o"
        $OutputPath
    )
    
    if ($Watch) {
        $arguments += "--watch"
        Write-Host "👀 Watch mode enabled - press Ctrl+C to stop" -ForegroundColor Green
        Write-Host ""
    }

    # Change to project root directory
    Push-Location $ProjectRoot
    
    Write-Host "🔧 Building README.md..." -ForegroundColor Blue
    
    # Run MergeIncludes
    & dotnet @arguments
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ README.md updated successfully!" -ForegroundColor Green
        
        if (-not $Watch) {
            Write-Host ""
            Write-Host "📖 View the result:" -ForegroundColor Cyan
            Write-Host "   code README.md" -ForegroundColor Gray
            Write-Host ""
            Write-Host "🔄 To watch for changes:" -ForegroundColor Cyan
            Write-Host "   .\docs\update-readme.ps1 -Watch" -ForegroundColor Gray
        }
    } else {
        Write-Error "❌ Failed to build README.md (exit code: $LASTEXITCODE)"
        exit $LASTEXITCODE
    }
} catch {
    Write-Error "❌ Error: $_"
    exit 1
} finally {
    Pop-Location
}
