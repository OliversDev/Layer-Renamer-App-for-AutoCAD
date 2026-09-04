# Repository Cleanup Audit

## Removed

- `Layer Renamer App for AutoCAD/LI-In-Bug.png` — exact duplicate of the referenced file under `Resources`.
- `Layer Renamer App for AutoCAD/github-mark-white.png` — exact duplicate of the referenced file under `Resources`.
- `Layer Renamer App for AutoCAD/Logo_BW-NOBG.png` — exact duplicate of the referenced file under `Resources`.
- `Layer Renamer App for AutoCAD/Logo_BW_Small.ico` — not referenced by the project, application, icon generator, or Marketplace package.
- Unused WPF project configuration from the WinForms-only project.
- Stale `backgroundWorker1` designer metadata from `MainForm.resx`.
- References to ignored `dazzleicons` build inputs that were absent from the repository. The existing compiled resource images are now themed directly.

## Intentionally retained

- `Layer Renamer App for AutoCAD/Resources/LI-In-Bug.png`, `github-mark-white.png`, and `Logo_BW-NOBG.png` are referenced by `Properties/Resources.resx` and used by the footer.
- The project and Marketplace copies of `LayerRenamerIcon.ico` are identical but required in separate source and submission locations.
- Root `LICENSE.html` and bundled `Contents/Help/license.html` are identical by design; the first is the repository source and the second keeps the bundle template complete.
- `LICENSE.txt` is retained as the conventional plain-text repository license; the application and Marketplace bundle open the HTML version.
- The partial CUI files under `AppStore/Ribbon/LayerRenamer` are all required by `build-app-store.ps1` when constructing the ribbon CUIX.

After these removals, no other unintentional byte-for-byte duplicate files or unresolved project resource references were found.
