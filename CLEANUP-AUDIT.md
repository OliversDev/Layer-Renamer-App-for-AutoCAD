# Repository Cleanup Audit

## Removed

- `Layer Renamer App for AutoCAD/LI-In-Bug.png` — exact duplicate of the referenced file under `Resources`.
- `Layer Renamer App for AutoCAD/github-mark-white.png` — exact duplicate of the referenced file under `Resources`.
- `Layer Renamer App for AutoCAD/Logo_BW-NOBG.png` — exact duplicate of the referenced file under `Resources`.
- `Layer Renamer App for AutoCAD/Logo_BW_Small.ico` — not referenced by the project, application, icon generator, or Marketplace package.
- Stale `backgroundWorker1` designer metadata from `MainForm.resx`.
- Reintroduced, unreferenced `dazzleicons` source files and their stale ignored build inputs. The footer uses the images referenced by `Properties/Resources.resx`, which are themed directly.

## Intentionally retained

- `Layer Renamer App for AutoCAD/Resources/LI-In-Bug.png`, `github-mark-white.png`, and `Logo_BW-NOBG.png` are referenced by `Properties/Resources.resx` and used by the footer.
- The project and Marketplace copies of `LayerRenamerIcon.ico` are identical but required in separate source and submission locations.
- Root `LICENSE.html` and bundled `Contents/Help/license.html` are identical by design; the first is the repository source and the second keeps the bundle template complete.
- `LICENSE.txt` is retained as the conventional plain-text repository license; the application and Marketplace bundle open the HTML version.
- The partial CUI files under `AppStore/Ribbon/LayerRenamer` are all required by `build-app-store.ps1` when constructing the ribbon CUIX.
- `UseWPF` is enabled for both targets because the AutoCAD 2026 and 2027 managed assemblies depend on their matching Windows Desktop `WindowsBase` reference sets. The application UI remains WinForms.

After these removals, no other unintentional byte-for-byte duplicate files or unresolved project resource references were found.
