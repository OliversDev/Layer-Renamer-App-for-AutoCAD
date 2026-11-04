# Autodesk Marketplace release checklist

## Build and package

- Close AutoCAD 2026 and 2027.
- Install the .NET 8 and .NET 10 SDKs and both AutoCAD managed API sets.
- Run `Apply-Build-Install-Renamer-2026-2027.bat`.
- Confirm `AppStore\artifacts\Renamer-1.0.0-Autodesk-Marketplace.zip` opens correctly.

## Functional testing

- Confirm the Renamer panel appears only on AutoCAD's standard Add-ins tab.
- Hover over the ribbon button and press F1; confirm Renamer help opens.
- Test the ribbon button and `OW:Renamer` in AutoCAD 2026 and 2027.
- Test all tabs: Layers, Blocks, Dimension Styles, Linetypes, Table Styles, and Text Styles.
- Test exact and `*` filters, independent tab selections, prefix, suffix, find/replace, exact single-object naming, mixed-category staging, manual edits, comments, copy, save, clear, execution, and Undo.
- Verify same-category collision warnings do not incorrectly flag identical names in different categories.
- Verify layer `0`, Defpoints, reserved linetypes, the Standard table style, layout/anonymous blocks, and dependent xref records are excluded.
- Test Windows light/dark themes and 100%, 125%, and 150% scaling.
- Verify Help, License, Privacy, GitHub, and LinkedIn links.

## Submission

- Rename the GitHub repository and confirm all new URLs resolve.
- Publish `PRIVACY.md` and enable the GitHub Issues support page.
- Add the final publisher support email to `CompanyDetails` and the Marketplace form.
- Upload the 120 × 120 icon and requested screenshots.
- Submit `Renamer-1.0.0-Autodesk-Marketplace.zip`.
- Keep the UpgradeCode stable for future updates.
