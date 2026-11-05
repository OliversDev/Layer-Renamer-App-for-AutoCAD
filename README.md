# Renamer for AutoCAD

Renamer is an AutoCAD add-in for batch-renaming named drawing objects through a common, reviewable workflow.

## Supported objects

- Layers
- Blocks
- Dimension styles
- Linetypes
- Table styles
- Text styles

## Features

- Switch object categories using a modern theme-aware tab strip while keeping separate selections and filters for each category.
- Rename selected objects with a prefix, suffix, case-insensitive find/replace, or an exact name for one object.
- Filter by exact name or `*` wildcard patterns while preserving selection.
- Reject empty, invalid, over-length, duplicate, and conflicting generated names.
- Exclude protected, anonymous, layout, xref-dependent, and other non-renamable AutoCAD records.
- Show the original detailed layer columns for colour, linetype, frozen state, and locked state while other tabs use Name and Details.
- Resize every visible grid column and scroll horizontally when the selected widths exceed the available space.
- Sort the Layers grid by name, colour, linetype, frozen state, or locked state.
- Build one editable script containing commands from multiple object categories.
- Warn when the same source is renamed more than once or same-category targets collide.
- Copy, save, clear, manually edit, or run the script in the active drawing.
- Follow the Windows light or dark application theme.

## Requirements

- AutoCAD 2026 or AutoCAD 2027 for 64-bit Windows
- .NET 8 runtime supplied for AutoCAD 2026
- .NET 10 runtime supplied for AutoCAD 2027

## Installation and use

The Autodesk Design and Make Marketplace installer registers the application automatically. For local testing, copy `Renamer.bundle` to `%APPDATA%\Autodesk\ApplicationPlugins`, restart AutoCAD, and open **Renamer** from the standard **Add-ins** ribbon tab or enter `OW:Renamer`.

1. Choose an object-category tab.
2. Select one or more objects.
3. Enter a prefix, suffix, or optional **Find** and **Replace** values. To set an exact name, select one object and use **Rename To (single object)**.
4. Select **Add To Script List**. Switch tabs and repeat as required.
5. Review or edit the combined script, then copy, save, or run it. Lines beginning with `;` are comments.

Renaming uses AutoCAD's standard `-RENAME` command and remains available to AutoCAD's normal Undo workflow while the drawing is open. Manually entered script lines run as written and should be reviewed carefully.

The protected Standard table style is intentionally excluded because AutoCAD does not permit it to be renamed.

## Development and packaging

The solution builds AutoCAD 2026 with .NET 8 and AutoCAD 2027 with .NET 10. Run `build-app-store.ps1` to validate, build, stage, and create:

```text
AppStore\artifacts\Renamer-1.0.0-Autodesk-Marketplace.zip
```

The update package includes `Apply-Build-Install-Renamer-2026-2027.bat`. It renames the local repository folder, replaces the source files without creating backups, builds both targets, removes old Renamer and former product bundles, installs `%APPDATA%\Autodesk\ApplicationPlugins\Renamer.bundle`, and leaves the CMD window open with the result.

## Privacy and support

Renamer does not collect or transmit drawing information, object names, personal information, telemetry, or analytics. See [PRIVACY.md](PRIVACY.md).

Report issues at [Renamer support](https://github.com/OliversDev/Renamer-App-for-AutoCAD/issues). Do not attach confidential drawings or object names.

## License

See [LICENSE.html](LICENSE.html) and [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt).
