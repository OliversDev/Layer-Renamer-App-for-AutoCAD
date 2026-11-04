# Renamer Autodesk Marketplace compliance

## Package and compatibility

- Product name, namespace, product/upgrade codes, assembly, bundle, command, help, CUIX, icon, and documentation use Renamer identifiers.
- AutoCAD 2026 uses .NET 8 and exact series `R25.1`; AutoCAD 2027 uses .NET 10 and exact series `R26.0`.
- `OW:Renamer` is demand-loaded and mapped to local contextual F1 help.

## User experience

- The partial CUIX merges only into `ID_ADDINSTAB` using `AddToWorkspace` and `MergeTabOnly`.
- No custom ribbon tab is created and no command name is injected into the tooltip.
- Bundled Help, Privacy, License, and third-party notices work offline.
- Six object tabs share one consistent selection, filtering, rename, validation, script, and execution workflow.

## Privacy and dependencies

- Processing is local; no telemetry, analytics, cloud processing, advertising, tracking, or application logs are used.
- No third-party executable DLL is included. Runtime dependencies are Autodesk's managed AutoCAD API, Microsoft .NET, and Windows system libraries used for UI theming.
- The application reads the Windows app-theme registry value but does not create, change, or delete registry values.

## Build safeguards

- `build-app-store.ps1` validates both AutoCAD API installations, ribbon merge and F1 configuration, manifest ranges, command registration, required files, and final archive contents.
- `Apply-Build-Install-Renamer-2026-2027.bat` renames the repository folder, replaces source files without backups, builds both targets, removes obsolete bundles, installs the current user-level test bundle, verifies hashes, and preserves all output in a visible CMD window.

## Publisher actions

- Rename the GitHub repository to `Renamer App for AutoCAD` so its URL becomes `Renamer-App-for-AutoCAD`.
- Publish the privacy policy and support page before submission.
- Add the publisher support email required by the Marketplace portal/manifest when the final support address is chosen.
- Test both supported AutoCAD versions and contextual F1 help before submission.
