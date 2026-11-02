# Layer Renamer Autodesk Marketplace compliance

Layer Renamer implements the Autodesk Marketplace packaging, ribbon, help, privacy, licensing, listing, artwork, build, and release requirements with product-specific names, commands, identifiers, help, and artwork.

## Package and compatibility

- Unique `ProductCode`, stable `UpgradeCode`, namespace, publisher, version, icon, local help, and online documentation are declared in `PackageContents.xml`.
- AutoCAD 2026 uses the .NET 8 build and AutoCAD 2027 uses the .NET 10 build.
- Each version has an exact `RuntimeRequirements` series range and registers the bundled Help directory.
- `OW:LayerRenamer` is demand-loaded and mapped to the `#layerrenamer` help topic.

## AutoCAD user experience

- The partial CUIX uses `ID_ADDINSTAB` as both its tab element ID and alias, then merges the Layer Renamer panel into AutoCAD's built-in Add-ins tab using `AddToWorkspace` and `MergeTabOnly`.
- No separate product ribbon tab is created.
- The ribbon button includes 16 px and 32 px images, a clean tooltip without the command-name line, and enabled contextual F1 help.
- Bundled HTML Help, Privacy, License/risk disclaimer, and third-party notices are available offline.

## Marketplace content

- Store description, usage, installation, uninstallation, support, commands, privacy statement, limitations, and artwork inventory are maintained in `STORE-LISTING.md`.
- Transparent 100 px, 120 px, and 1024 px artwork, editable SVG sources, a multi-resolution ICO, and ribbon images are provided.
- The privacy policy states what the application reads, what it stores, what it does not collect or transmit, optional external links, support communications, retention, security, changes, and contact route.
- The HTML license explains drawing-modification risks, backups, user validation duties, warranty exclusions, liability limitations, and MIT terms.

## Build safeguards

- `build-app-store.ps1` validates API installations, ribbon merge settings, contextual help, manifest support paths, command registration, and app-specific help before producing the dual-version submission ZIP.
- `Apply-Build-Install-Layer-Renamer-2026.bat` provides a repeatable AutoCAD 2026 update/build/install test loop that directly replaces the update files and installed test bundle.
- The release checklist includes clean-profile testing, ribbon placement, contextual F1, supported versions, theme/scaling, rename behavior, links, privacy publication, artwork, package integrity, and stable identifiers.

## Final publisher actions

- Replace the listing's generic support route with the final Marketplace support contact.
- Publish the privacy policy at the URL used in the Marketplace portal.
- When Autodesk assigns a Layer Renamer Marketplace app ID or exact HelpDoc URL, update the ribbon `HelpSource` to that Autodesk URL and repeat the F1 acceptance test.
- Build and test both targets on Windows with AutoCAD 2026 and 2027 before submission.
