param(
    [string]$AutoCAD2026Dir = "C:\Program Files\Autodesk\AutoCAD 2026",
    [string]$AutoCAD2027Dir = "C:\Program Files\Autodesk\AutoCAD 2027"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$repositoryRoot = $PSScriptRoot
$project = Join-Path $repositoryRoot "Renamer App for AutoCAD\Renamer App for AutoCAD.csproj"
$bundleTemplate = Join-Path $repositoryRoot "AppStore\Renamer.bundle"
$artifactsRoot = Join-Path $repositoryRoot "AppStore\artifacts"
$bundle = Join-Path $artifactsRoot "Renamer.bundle"
$ribbonSource = Join-Path $repositoryRoot "AppStore\Ribbon\Renamer"
$outputZip = Join-Path $artifactsRoot "Renamer-1.0.0-Autodesk-Marketplace.zip"

function Assert-AutoCADApi {
    param(
        [string]$Version,
        [string]$Directory
    )

    foreach ($fileName in @("AcCoreMgd.dll", "AcDbMgd.dll", "AcMgd.dll")) {
        $apiFile = Join-Path $Directory $fileName
        if (-not (Test-Path -LiteralPath $apiFile -PathType Leaf)) {
            throw "AutoCAD $Version API file was not found: '$apiFile'."
        }
    }
}

function Assert-DotNetSdk {
    param([string]$MajorVersion)

    $installedSdks = @(& dotnet --list-sdks 2>$null)
    if ($LASTEXITCODE -ne 0 -or -not ($installedSdks | Where-Object { $_ -match "^$([regex]::Escape($MajorVersion))\." })) {
        throw ".NET $MajorVersion SDK is required. Install it, reopen this CMD window, and run the BAT again."
    }
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "The dotnet command was not found. Install the .NET 8 and .NET 10 SDKs."
}

Assert-DotNetSdk -MajorVersion "8"
Assert-DotNetSdk -MajorVersion "10"

Assert-AutoCADApi -Version "2026" -Directory $AutoCAD2026Dir
Assert-AutoCADApi -Version "2027" -Directory $AutoCAD2027Dir

$storeListing = Join-Path $repositoryRoot "AppStore\STORE-LISTING.md"
if (-not (Test-Path -LiteralPath $storeListing -PathType Leaf) -or
    (Get-Content -LiteralPath $storeListing -Raw) -notmatch [regex]::Escape("https://github.com/OliversDev/Renamer-App-for-AutoCAD/issues")) {
    throw "STORE-LISTING.md must contain the explicit Renamer support page."
}

if (-not (Test-Path -LiteralPath (Join-Path $bundleTemplate "PackageContents.xml") -PathType Leaf)) {
    throw "The bundle template is incomplete: '$bundleTemplate'."
}

New-Item -ItemType Directory -Path $artifactsRoot -Force | Out-Null

if (Test-Path -LiteralPath $bundle) {
    Remove-Item -LiteralPath $bundle -Recurse -Force
}

Copy-Item -LiteralPath $bundleTemplate -Destination $bundle -Recurse -Force

dotnet build $project `
    --configuration Release `
    --framework net8.0-windows `
    --property:AutoCAD2026Dir="$AutoCAD2026Dir"

if ($LASTEXITCODE -ne 0) {
    throw "The AutoCAD 2026 / .NET 8 build failed."
}

dotnet build $project `
    --configuration Release `
    --framework net10.0-windows `
    --property:AutoCAD2027Dir="$AutoCAD2027Dir"

if ($LASTEXITCODE -ne 0) {
    throw "The AutoCAD 2027 / .NET 10 build failed."
}

foreach ($version in @("2026", "2027")) {
    $sourceDll = Join-Path $repositoryRoot "Renamer App for AutoCAD\bin\Release\AutoCAD$version\Renamer App for AutoCAD.dll"
    if (-not (Test-Path -LiteralPath $sourceDll -PathType Leaf)) {
        throw "The AutoCAD $version build did not produce the expected DLL: '$sourceDll'."
    }
    $destinationDir = Join-Path $bundle "Contents\$version"
    New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
    Copy-Item -LiteralPath $sourceDll -Destination (Join-Path $destinationDir "Renamer App for AutoCAD.dll") -Force
}

$resourcesDir = Join-Path $bundle "Contents\Resources"
$helpDir = Join-Path $bundle "Contents\Help"
New-Item -ItemType Directory -Path $resourcesDir -Force | Out-Null
New-Item -ItemType Directory -Path $helpDir -Force | Out-Null

$requiredRibbonFiles = @(
    "[Content_Types].xml",
    "_rels\.rels",
    "Header.cui",
    "MenuGroup.cui",
    "Menu_Package_Info.xml",
    "RibbonRoot.cui",
    "AcceleratorRoot.cui",
    "DigitizerButtonRoot.cui",
    "DoubleClickRoot.cui",
    "ImageMenuRoot.cui",
    "MouseButtonRoot.cui",
    "OverrideRoot.cui",
    "PopMenuRoot.cui",
    "QuickAccessToolbarRoot.cui",
    "QuickPropertiesRoot.cui",
    "RolloverTooltipRoot.cui",
    "ScreenMenuRoot.cui",
    "TabletMenuRoot.cui",
    "ToolbarRoot.cui",
    "WorkspaceRoot.cui",
    "Renamer16.png",
    "Renamer32.png"
)

foreach ($fileName in $requiredRibbonFiles) {
    $ribbonFile = Join-Path $ribbonSource $fileName
    if (-not (Test-Path -LiteralPath $ribbonFile -PathType Leaf)) {
        throw "Required ribbon source file was not found: '$ribbonFile'."
    }
}

# Autodesk review requires the partial ribbon to merge into the built-in
# Add-ins tab and expose contextual F1 help. Keep these acceptance checks
# here so a future CUI edit cannot accidentally recreate a product-specific tab
# or remove Renamer help.
[xml]$ribbonDefinition = Get-Content -LiteralPath (Join-Path $ribbonSource "RibbonRoot.cui") -Raw
$tabDefinition = $ribbonDefinition.RibbonRoot.RibbonTabSourceCollection.RibbonTabSource
if ($tabDefinition.UID -ne "ID_ADDINSTAB" -or
    $tabDefinition.Alias -ne "ID_ADDINSTAB" -or
    $tabDefinition.DefaultDisplay -ne "AddToWorkspace" -or
    $tabDefinition.WorkspaceBehavior -ne "MergeTabOnly") {
    throw "Renamer must use and merge into ID_ADDINSTAB with AddToWorkspace and MergeTabOnly."
}

[xml]$macroDefinition = Get-Content -LiteralPath (Join-Path $ribbonSource "MenuGroup.cui") -Raw
$macroNode = $macroDefinition.MenuGroup.MacroGroup.MenuMacro.Macro
$tooltipDefinition = $macroNode.ToolTip
if ($null -ne $macroNode.SelectSingleNode("CLICommand") -or
    $tooltipDefinition.HasAttribute("CommandName") -or
    $tooltipDefinition.EnableHelp -ne "true" -or
    $tooltipDefinition.HelpSource -ne "Renamer.html#renamer") {
    throw "Renamer ribbon must omit CLICommand and tooltip CommandName while retaining contextual F1 help."
}

[xml]$manifestDefinition = Get-Content -LiteralPath (Join-Path $bundle "PackageContents.xml") -Raw
$componentSets = @($manifestDefinition.ApplicationPackage.Components)
if ($componentSets.Count -ne 2) {
    throw "PackageContents.xml must contain exactly the AutoCAD 2026 and AutoCAD 2027 component sets."
}

$expectedSeries = @{
    "AutoCAD 2026" = "R25.1"
    "AutoCAD 2027" = "R26.0"
}
foreach ($componentSet in $componentSets) {
    if (-not $expectedSeries.ContainsKey([string]$componentSet.Description)) {
        throw "Unexpected component set in PackageContents.xml: '$($componentSet.Description)'."
    }
    $expected = $expectedSeries[[string]$componentSet.Description]
    if ($componentSet.RuntimeRequirements.SeriesMin -ne $expected -or
        $componentSet.RuntimeRequirements.SeriesMax -ne $expected) {
        throw "$($componentSet.Description) must use SeriesMin and SeriesMax '$expected'."
    }
    if ($componentSet.RuntimeRequirements.SupportPath -ne "./Contents/Help") {
        throw "Every supported AutoCAD version must register ./Contents/Help as a support path."
    }

    $managedEntry = @($componentSet.ComponentEntry) | Where-Object { $_.ModuleName -like "*.dll" }
    $command = $managedEntry.Commands.Command
    if ($command.Global -ne "OW:Renamer" -or $command.HelpTopic -ne "#renamer") {
        throw "The Renamer command or contextual help topic is not registered correctly."
    }
}

$namedHelp = Join-Path $bundle "Contents\Help\Renamer.html"
if (-not (Test-Path -LiteralPath $namedHelp -PathType Leaf)) {
    throw "The app-specific Renamer.html help page is missing."
}

$ribbonWorkRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("RenamerCuix_" + [Guid]::NewGuid().ToString("N").Substring(0, 8))
$ribbonPackageRoot = Join-Path $ribbonWorkRoot "Package"
$ribbonZip = Join-Path $ribbonWorkRoot "Renamer.zip"
$ribbonCuix = Join-Path $resourcesDir "Renamer.cuix"

try {
    New-Item -ItemType Directory -Path $ribbonPackageRoot -Force | Out-Null

    foreach ($fileName in $requiredRibbonFiles) {
        $sourceFile = Join-Path $ribbonSource $fileName
        $destinationFile = Join-Path $ribbonPackageRoot $fileName
        $destinationFolder = Split-Path -Parent $destinationFile
        New-Item -ItemType Directory -Path $destinationFolder -Force | Out-Null
        Copy-Item -LiteralPath $sourceFile -Destination $destinationFile -Force
    }

    Compress-Archive -Path (Join-Path $ribbonPackageRoot "*") -DestinationPath $ribbonZip -CompressionLevel Optimal
    Move-Item -LiteralPath $ribbonZip -Destination $ribbonCuix -Force
}
finally {
    if (Test-Path -LiteralPath $ribbonWorkRoot) {
        Remove-Item -LiteralPath $ribbonWorkRoot -Recurse -Force
    }
}

Copy-Item `
    -LiteralPath (Join-Path $repositoryRoot "Renamer App for AutoCAD\RenamerIcon.ico") `
    -Destination (Join-Path $resourcesDir "Renamer.ico") `
    -Force

Copy-Item `
    -LiteralPath (Join-Path $repositoryRoot "AppStore\Renamer.bundle\Contents\Help\Renamer.html") `
    -Destination (Join-Path $helpDir "Renamer.html") `
    -Force

Copy-Item `
    -LiteralPath (Join-Path $repositoryRoot "LICENSE.html") `
    -Destination (Join-Path $helpDir "license.html") `
    -Force

Copy-Item `
    -LiteralPath (Join-Path $repositoryRoot "THIRD-PARTY-NOTICES.txt") `
    -Destination (Join-Path $helpDir "THIRD-PARTY-NOTICES.txt") `
    -Force

$requiredBundleFiles = @(
    "PackageContents.xml",
    "Contents\2026\Renamer App for AutoCAD.dll",
    "Contents\2027\Renamer App for AutoCAD.dll",
    "Contents\Resources\Renamer.cuix",
    "Contents\Resources\Renamer.ico",
    "Contents\Help\Renamer.html",
    "Contents\Help\index.html",
    "Contents\Help\privacy.html",
    "Contents\Help\license.html",
    "Contents\Help\THIRD-PARTY-NOTICES.txt"
)

foreach ($relativePath in $requiredBundleFiles) {
    $requiredBundleFile = Join-Path $bundle $relativePath
    if (-not (Test-Path -LiteralPath $requiredBundleFile -PathType Leaf)) {
        throw "The Marketplace bundle is missing required file: '$relativePath'."
    }
}

[xml](Get-Content -LiteralPath (Join-Path $bundle "PackageContents.xml") -Raw) | Out-Null

if (Test-Path -LiteralPath $outputZip) {
    Remove-Item -LiteralPath $outputZip -Force
}

Compress-Archive -Path $bundle -DestinationPath $outputZip -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($outputZip)
try {
    $archiveNames = @($archive.Entries | ForEach-Object { $_.FullName.Replace('/', '\') })
    foreach ($relativePath in $requiredBundleFiles) {
        $archivePath = "Renamer.bundle\$relativePath"
        if ($archiveNames -notcontains $archivePath) {
            throw "The generated Marketplace ZIP is missing: '$archivePath'."
        }
    }
}
finally {
    $archive.Dispose()
}

$zipHash = (Get-FileHash -LiteralPath $outputZip -Algorithm SHA256).Hash
Write-Host "Created $outputZip"
Write-Host "SHA256 $zipHash"
