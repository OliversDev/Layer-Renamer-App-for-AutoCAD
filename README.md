# Layer Renamer for AutoCAD

Layer Renamer is a focused AutoCAD add-in for renaming drawing layers through generated or manually edited AutoCAD script commands. It supports prefix, suffix, find-and-replace, and exact-name methods with validation before generated commands are added.

## Features

- Rename multiple selected layers in one operation.
- Add a prefix, suffix, or both.
- Find and replace text without requiring another option to be enabled.
- Rename one selected layer directly to an exact name.
- Filter layers by exact name or `*` wildcard patterns while preserving selection.
- Prevent empty names, invalid characters, names longer than 255 characters, duplicate results, and collisions with existing layers.
- Build an editable script with one direct AutoCAD rename command per selected layer.
- Enter or revise script commands manually before running them.
- Warn before running when active layer-rename commands reuse a destination name or rename the same source layer more than once.
- Copy or save the generated script, or run it in the active drawing.
- Exclude layer `0`, `Defpoints`, and externally dependent layers.
- Display layer colour, linetype, frozen state, locked state, and lineweight.
- Follow the Windows light or dark application theme.

## Requirements

- AutoCAD 2026 or AutoCAD 2027 for Windows, 64-bit

## Installation

The Autodesk Design and Make Marketplace installer registers the application automatically.

For local bundle testing, copy `LayerRenamer.bundle` to:

```text
C:\Program Files\Autodesk\ApplicationPlugins
```

Restart AutoCAD. Open the **Plug-Ins** ribbon tab and select **Layer Renamer**, or enter:

```text
OW:LayerRenamer
```

## Usage

1. Select one or more layers in the grid.
2. Enter a prefix and/or suffix, or optionally enter **Find** and **Replace** text.
3. To assign an exact name, select one layer and enter the result in **Rename To (single layer)**. This overrides the other rename fields.
4. Select **Add To Script List** and repeat for other layer selections as needed.
5. Review or manually edit the script.
6. Copy or save the script, or select **Rename** and confirm to run it. Lines beginning with `;` are treated as comments. Unrelated manual commands are not included in the layer-conflict analysis.

Before execution, Layer Renamer examines active commands that match its generated layer-rename format. If multiple source layers use the same destination name, or one source layer is renamed more than once, the app warns that the script may fail and lets the user cancel or continue.

Each rename command can be reversed through AutoCAD's normal undo workflow while the drawing remains open. Maintain current backups and review the preview before renaming production drawings.

## Uninstallation

Use **Settings > Apps > Installed apps** in Windows, select **Layer Renamer**, and choose **Uninstall**. For a manually installed test bundle, close AutoCAD and remove `LayerRenamer.bundle` from the Autodesk `ApplicationPlugins` folder.

## Privacy

Layer Renamer does not collect or transmit personal information, drawing information, layer names, or usage analytics. Processing occurs locally. See [PRIVACY.md](PRIVACY.md).

## Development and Marketplace packaging

The solution builds AutoCAD 2026 with .NET 8 and AutoCAD 2027 with .NET 10. Install the corresponding .NET SDKs and AutoCAD managed APIs, then run from a PowerShell developer prompt:

The AutoCAD 2027 target imports the .NET 10 WPF reference set because Autodesk's managed API depends on its `WindowsBase` assembly. The application itself continues to use Windows Forms.

```powershell
.\build-app-store.ps1
```

The script validates both AutoCAD API installations, builds release-specific DLLs, creates the partial CUIX ribbon, stages the complete bundle, copies Help/License/Privacy resources, and writes:

```text
AppStore\artifacts\LayerRenamer-2.0.0-Autodesk-Marketplace.zip
```

If AutoCAD is installed elsewhere, pass `-AutoCAD2026Dir` and `-AutoCAD2027Dir`. If the source was downloaded as a ZIP, unblock it in Windows before extracting or use `Unblock-File` on a trusted extracted copy.

Marketplace artwork is maintained under `AppStore\Assets`, including exact 100, 120, and 1024 pixel transparent PNG files, a multi-resolution ICO, and editable SVG sources. The ribbon has separate 16 and 32 pixel assets.

## License and third-party notices

See [LICENSE.html](LICENSE.html) for the risk disclaimer and MIT terms. The application includes licensed Dazzle icon assets; see [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt).
