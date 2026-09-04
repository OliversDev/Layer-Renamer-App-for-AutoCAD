# Autodesk Marketplace release checklist

## Build and package

- Install the .NET 8 and .NET 10 SDKs.
- Confirm AutoCAD 2026 and AutoCAD 2027 managed API files are installed.
- Run `build-app-store.ps1` from the repository root.
- Confirm both release DLLs and the generated CUIX are present in `AppStore\artifacts\LayerRenamer.bundle`.
- Confirm the generated Marketplace ZIP opens without archive errors.

## Functional testing

- Test AutoCAD 2026 and AutoCAD 2027 on clean Windows profiles.
- Test the Plug-Ins ribbon button and `OW:LayerRenamer` command.
- Test Windows light, dark, and high-contrast modes at 100%, 125%, and 150% display scaling.
- Test partial filtering, `*` wildcards, Ctrl/Shift selection, and selection persistence.
- Test prefix, suffix, find/replace, exact-name rename, exact and `*` wildcard filtering, persistent selection, manual script editing, comment handling, duplicate destination warnings, repeated source warnings, copy, save, confirmation, and AutoCAD undo.
- Verify rejection of invalid, empty, duplicate, conflicting, and over-length names.
- Verify layer 0, Defpoints, and dependent xref layers cannot be selected.
- Verify Help, License, Privacy, GitHub, and LinkedIn links.

## Submission content

- Review `STORE-LISTING.md` and replace the Marketplace support contact as required in the publisher portal.
- Publish `PRIVACY.md` at the URL stated in the listing before submission.
- Upload the 120 × 120 transparent icon and any requested screenshots.
- Use the generated `LayerRenamer-2.0.0-Autodesk-Marketplace.zip` as the app package.
- Digitally sign release binaries if required by the publisher workflow.
- Confirm the ProductCode remains unique and the UpgradeCode remains stable for future updates.
- Verify all claimed AutoCAD versions match the tested package before submitting.
