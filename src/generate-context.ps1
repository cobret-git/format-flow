<#
.SYNOPSIS
    Universal project context generator for AI assistants
.DESCRIPTION
    Generates a comprehensive markdown file containing project structure and source code
    for use with AI coding assistants like Claude. Works with any project type.
.PARAMETER OutputDirName
    Name of the output directory (default: .claude)
.PARAMETER OutputFileName
    Name of the output file (default: project-context.md)
.PARAMETER FindRoot
    Automatically find repository root by looking for .git directory
#>

param(
    [string]$OutputDirName = ".claude",
    [string]$OutputFileName = "project-context.md",
    [switch]$FindRoot = $true
)

$ErrorActionPreference = "Stop"

# ------------------------------------------------------------------
# Helper Functions
# ------------------------------------------------------------------

function Find-RepositoryRoot {
    param([string]$StartPath)
    
    $current = $StartPath
    while ($current) {
        if (Test-Path (Join-Path $current ".git")) {
            return $current
        }
        $parent = Split-Path -Parent $current
        if ($parent -eq $current) { break }
        $current = $parent
    }
    return $StartPath
}

function Get-LanguageFromExtension {
    param([string]$Extension)
    
    $langMap = @{
        # .NET / C-family
        ".cs"     = "csharp"
        ".vb"     = "vbnet"
        ".fs"     = "fsharp"
        ".cpp"    = "cpp"
        ".c"      = "c"
        ".h"      = "c"
        ".hpp"    = "cpp"
        ".cc"     = "cpp"
        
        # Web
        ".js"     = "javascript"
        ".jsx"    = "jsx"
        ".ts"     = "typescript"
        ".tsx"    = "tsx"
        ".html"   = "html"
        ".htm"    = "html"
        ".css"    = "css"
        ".scss"   = "scss"
        ".sass"   = "sass"
        ".less"   = "less"
        ".vue"    = "vue"
        ".svelte" = "svelte"
        
        # Scripting
        ".py"     = "python"
        ".rb"     = "ruby"
        ".php"    = "php"
        ".pl"     = "perl"
        ".sh"     = "bash"
        ".bash"   = "bash"
        ".zsh"    = "zsh"
        ".ps1"    = "powershell"
        
        # Mobile
        ".swift"  = "swift"
        ".kt"     = "kotlin"
        ".java"   = "java"
        ".dart"   = "dart"
        
        # Data/Config
        ".json"   = "json"
        ".yaml"   = "yaml"
        ".yml"    = "yaml"
        ".xml"    = "xml"
        ".toml"   = "toml"
        ".ini"    = "ini"
        ".conf"   = "conf"
        
        # Markup
        ".xaml"   = "xml"
        ".resx"   = "xml"
        ".md"     = "markdown"
        ".rst"    = "rst"
        
        # Build/Project
        ".csproj" = "xml"
        ".vbproj" = "xml"
        ".fsproj" = "xml"
        ".sln"    = "text"
        ".gradle" = "groovy"
        ".cmake"  = "cmake"
        
        # Other
        ".sql"    = "sql"
        ".go"     = "go"
        ".rs"     = "rust"
        ".lua"    = "lua"
        ".r"      = "r"
    }
    
    return $langMap[$Extension.ToLower()]
}

function Test-BinaryFile {
    param([string]$Path)
    
    # Known binary extensions
    $binaryExtensions = @(
        # Executables & Libraries
        ".exe", ".dll", ".so", ".dylib", ".a", ".lib", ".o", ".obj",
        # Archives
        ".zip", ".tar", ".gz", ".bz2", ".7z", ".rar", ".tgz",
        # Images
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico", ".webp", ".svg", ".tiff", ".tif",
        # Video
        ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm",
        # Audio
        ".mp3", ".wav", ".ogg", ".flac", ".aac", ".m4a", ".wma",
        # Fonts
        ".ttf", ".otf", ".woff", ".woff2", ".eot",
        # Documents (binary formats)
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        # Databases
        ".db", ".sqlite", ".mdb",
        # Other
        ".pyc", ".pyo", ".class", ".jar", ".war", ".ear",
        ".swf", ".fla", ".psd", ".ai", ".sketch"
    )
    
    $ext = [System.IO.Path]::GetExtension($Path).ToLower()
    if ($binaryExtensions -contains $ext) {
        return $true
    }
    
    # Check file content for binary data (first 8KB)
    try {
        $bytes = [System.IO.File]::ReadAllBytes($Path)
        $sampleSize = [Math]::Min(8192, $bytes.Length)
        $nullBytes = 0
        
        for ($i = 0; $i -lt $sampleSize; $i++) {
            if ($bytes[$i] -eq 0) {
                $nullBytes++
                if ($nullBytes -gt 3) { return $true }
            }
        }
    }
    catch {
        return $true
    }
    
    return $false
}

function Get-GitignorePatterns {
    param([string]$RootPath)
    
    $patterns = @()
    $gitignorePath = Join-Path $RootPath ".gitignore"
    
    if (Test-Path $gitignorePath) {
        $lines = Get-Content $gitignorePath -ErrorAction SilentlyContinue
        foreach ($line in $lines) {
            $trimmed = $line.Trim()
            if ($trimmed -and -not $trimmed.StartsWith("#")) {
                $patterns += $trimmed
            }
        }
    }
    
    # Add universal ignore patterns
    $patterns += @(
        "node_modules/",
        "bin/",
        "obj/",
        ".vs/",
        ".vscode/",
        ".idea/",
        "*.user",
        "*.suo",
        ".DS_Store",
        "Thumbs.db",
        "packages/",
        "dist/",
        "build/",
        ".git/",
        "__pycache__/",
        "*.pyc",
        ".pytest_cache/",
        "vendor/",
        ".gradle/",
        "target/"
    )
    
    return $patterns
}

function Test-ShouldIgnore {
    param(
        [string]$Path,
        [string]$RootPath,
        [array]$Patterns
    )
    
    $relativePath = $Path.Substring($RootPath.Length).TrimStart('\', '/').Replace('\', '/')
    
    foreach ($pattern in $Patterns) {
        $pattern = $pattern.Replace('\', '/')
        
        # Directory patterns
        if ($pattern.EndsWith("/")) {
            $dirPattern = $pattern.TrimEnd("/")
            if ($relativePath -like "*$dirPattern/*" -or $relativePath -eq $dirPattern) {
                return $true
            }
        }
        # File patterns with wildcards
        elseif ($pattern.Contains("*")) {
            if ($relativePath -like $pattern) {
                return $true
            }
            # Check if pattern matches any part of path
            $segments = $relativePath -split "/"
            foreach ($segment in $segments) {
                if ($segment -like $pattern) {
                    return $true
                }
            }
        }
        # Exact match
        else {
            if ($relativePath -like "*$pattern*") {
                return $true
            }
        }
    }
    
    return $false
}

# ------------------------------------------------------------------
# Main Script
# ------------------------------------------------------------------

Write-Host "[*] Initializing project context generator..." -ForegroundColor Cyan

# Determine root directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = if ($FindRoot) {
    $found = Find-RepositoryRoot $ScriptDir
    Write-Host "[*] Repository root: $found" -ForegroundColor Yellow
    $found
} else {
    $ScriptDir
}

# Setup output
$OutputDir = Join-Path $RootDir $OutputDirName
$OutputFile = Join-Path $OutputDir $OutputFileName

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    Write-Host "[+] Created output directory: $OutputDirName" -ForegroundColor Green
}

# Load ignore patterns
$IgnorePatterns = Get-GitignorePatterns -RootPath $RootDir
Write-Host "[*] Loaded $($IgnorePatterns.Count) ignore patterns" -ForegroundColor Yellow

# Collect files
Write-Host "[*] Scanning for source files..." -ForegroundColor Cyan

$Files = Get-ChildItem -Path $RootDir -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object {
        -not (Test-ShouldIgnore -Path $_.FullName -RootPath $RootDir -Patterns $IgnorePatterns) -and
        -not (Test-BinaryFile -Path $_.FullName) -and
        $_.Length -lt 1MB
    } | Sort-Object FullName

Write-Host "[+] Found $($Files.Count) files to include" -ForegroundColor Green

# Build markdown
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$content = @"
# Project Context

> Generated: $timestamp
> Root: $RootDir

## Overview

_TODO: Describe your project's purpose, architecture, key technologies, and any important constraints or conventions._

## File Index

"@

foreach ($file in $Files) {
    $rel = $file.FullName.Substring($RootDir.Length).TrimStart('\', '/').Replace('\', '/')
    if ($file.Length -lt 1KB) {
        $size = "$($file.Length) B"
    } elseif ($file.Length -lt 1MB) {
        $sizeKB = [math]::Round($file.Length / 1KB, 1)
        $size = "$sizeKB KB"
    } else {
        $sizeMB = [math]::Round($file.Length / 1MB, 1)
        $size = "$sizeMB MB"
    }
    $content += "`n- $rel ($size)"
}

$content += "`n`n## Source Files`n"

# Add file contents
$fileCount = 0
foreach ($file in $Files) {
    $fileCount++
    $rel = $file.FullName.Substring($RootDir.Length).TrimStart('\', '/').Replace('\', '/')
    $lang = Get-LanguageFromExtension -Extension $file.Extension
    if (-not $lang) { $lang = "" }
    
    $percentComplete = [math]::Round(($fileCount / $Files.Count) * 100)
    Write-Progress -Activity "Processing files" -Status $rel -PercentComplete $percentComplete
    
    try {
        $fileContent = Get-Content $file.FullName -Raw -Encoding UTF8 -ErrorAction SilentlyContinue
        if ($null -eq $fileContent) { $fileContent = "" }
        
        $content += "`n`n---`n`n### $rel`n`n``````$lang`n$fileContent`n``````"
    }
    catch {
        Write-Warning "Could not read file: $rel"
        $content += "`n`n---`n`n### $rel`n`n_[Could not read file]_"
    }
}

Write-Progress -Activity "Processing files" -Completed

# Write output
[System.IO.File]::WriteAllText($OutputFile, $content, [System.Text.UTF8Encoding]::new($false))

$outputFileSize = [math]::Round((Get-Item $OutputFile).Length / 1KB, 1)

Write-Host "`n[SUCCESS] Context file generated successfully!" -ForegroundColor Green
Write-Host "[*] Output: $OutputFile" -ForegroundColor Cyan
Write-Host "[*] Total files: $($Files.Count)" -ForegroundColor Cyan
Write-Host "[*] File size: $outputFileSize KB" -ForegroundColor Cyan