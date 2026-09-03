# Layer Renamer for AutoCAD

## Short description

Safely preview and rename multiple AutoCAD layers using prefix, suffix, and find-and-replace rules.

## Description

Layer Renamer is a focused AutoCAD productivity tool for renaming multiple drawing layers. Select layers, enter a prefix or suffix, optionally find and replace part of each name, and review the resulting names before adding them to the read-only script.

The app checks every proposed name before adding it to the script. It prevents invalid characters, empty names, names longer than AutoCAD's limit, duplicate results, and conflicts with existing layers. Selections are preserved while filtering. The generated script can be copied, saved, or run in the active drawing.

The interface follows the Windows light or dark application theme and keeps the workflow intentionally simple.

## General usage instructions

Open **Layer Renamer** from the **Plug-Ins** ribbon tab or enter `OW:LayerRenamer`. Select one or more layers in the grid. Enter a prefix and/or suffix, and optionally enter **Find** and **Replace** text. Review the **Proposed Layer Name** preview and select **Add To Script List**. Repeat as needed, then copy or save the read-only script or select **Rename** to run it. Layer 0, Defpoints, and externally dependent layers are excluded.

## Installation

Download and run the installer from the Autodesk Design and Make Marketplace. The installer registers Layer Renamer for supported AutoCAD versions. Restart AutoCAD after installation. Open the **Plug-Ins** tab and select **Layer Renamer**, or enter `OW:LayerRenamer` at the command line.

## Uninstallation

Close AutoCAD. Open **Windows Settings > Apps > Installed apps**, locate **Layer Renamer**, select the available options menu, and choose **Uninstall**. Follow the Windows prompts. Restart AutoCAD if it was open during removal.

## Support information

For product support, usage questions, or defect reports, use the support contact or support link on this Autodesk Marketplace listing. Include the AutoCAD version, Layer Renamer version, a description of the issue, and reproducible steps. Do not include confidential drawings or layer information unless specifically required and appropriately sanitized.

## Additional information and known issues

Layer Renamer supports AutoCAD 2026 and AutoCAD 2027 on 64-bit Windows. It does not rename layer 0, Defpoints, or externally dependent layers. AutoCAD prevents some layer changes based on drawing state or object dependencies. Always maintain current drawing backups and review the preview column before renaming production layers. No known unresolved application defects are included with version 2.0.0.

## Commands

- `OW:LayerRenamer` — opens Layer Renamer.

## Privacy

Layer Renamer does not collect or transmit personal information, drawing information, layer names, or usage analytics. Processing occurs locally.

Privacy policy: https://github.com/OliversDev/Layer-Renamer-App-for-AutoCAD/blob/master/PRIVACY.md

## Submission artwork

- `Assets/LayerRenamerIcon-100.png` — transparent 100 × 100 icon
- `Assets/LayerRenamerIcon-120.png` — transparent 120 × 120 Marketplace icon
- `Assets/LayerRenamerIcon-1024.png` — transparent high-resolution icon
- `Assets/LayerRenamerIcon.ico` — 16, 24, 32, 48, 64, 128, and 256 pixel Windows icon
- `Assets/LayerRenamerIcon.svg` — editable application icon source
- `Assets/LayerRenamerRibbon.svg` — simplified ribbon source
