# LAYER RENAMER APP for AutoCAD®
## Overview
The Layer Renamer App is a Windows Forms application built for AutoCAD that provides users with an easy-to-use interface for renaming multiple layers in a drawing. It allows you to apply prefixes and suffixes to selected layers and includes features like filtering layers, managing layer properties, and a dark theme for a modern user experience.

## Features
- Layer Filtering: Easily search for layers using the filter box to match layer names with a wildcard search.
- Layer Renaming: Append custom prefixes and suffixes to selected layers.
- Layer Management: Displays layer properties such as color, linetype, frozen status, lock status, and lineweight with sorting capability.
- Persistent Layer Selection: The app maintains your selected layers even after filtering and clearing the filter.
- AutoCAD Integration: Fully integrated with AutoCAD's document and layer management system.
- Dark Theme: A visually appealing dark mode is applied across the application.

## System Requirements
- Operating System: Windows 7 or later
- Autodesk AutoCAD 2013 or later
- .NET Framework 4.7.2 or later

## Installation
Download the latest release from the GitHub repository.
Extract the zip file to a location of your choice.
Run the LayerRenamerApp.exe file to launch the application.

# Usage Guide
1. Load Layers
Once the app is opened, it automatically loads all layers from the active AutoCAD drawing. These layers are displayed in a grid, showing details such as:
- Layer Name
- Color
- Linetype
- Frozen/Locked Status
- Lineweight

2. Filter Layers
- Use the Filter text box to search for layers based on name. The search is case-insensitive and supports partial matches.
- Example: Typing "Wall" in the filter box will display all layers with "Wall" in their name.

3. Select Layers
- Click on any row in the table to select the corresponding layer. You can select multiple layers by holding the Ctrl or Shift keys while clicking.

4. Apply Prefix/Suffix
- Enter a Prefix or Suffix in their respective text boxes.
- Click Apply. The app will rename all selected layers with the provided prefix and suffix.
- If no layers are selected, a warning will appear.

5. Persistent Selection
- The application saves your selected layers even after filtering or clearing the filter. This ensures that your selected layers remain intact during the entire renaming process.

6. Clear Filter
- Click the Clear Filter button or erase the text in the filter box to reset the grid to show all layers.

7. Layers "0" and "Defpoints"
- These layers cannot and should not be renamed. Therefore, they are not included in the layer table by design. Layer table will be blank if you open a new drawing with no user defined layers and run the application.

9. Dark Theme
- The app comes with a pre-configured dark theme that applies to all components, providing a sleek and modern look. The background is a dark gray while the text is displayed in white.

## Error Handling
The app provides detailed error messages if something goes wrong, such as:
- Invalid characters in prefix/suffix.
- Issues when renaming layers.
- Problems loading layers from AutoCAD.
</br>
The following characters are not allowed in layer names:
<, >, /, \, ", :, ;, ?, *, |, ,, =

## License
This project is licensed under the MIT License - see the LICENSE.txt file for details: https://github.com/OliversDev/Layer-Renamer-App-for-AutoCAD/blob/master/LICENSE.txt

## Help and Support
For bug reports or feature requests, feel free to open an issue on the GitHub Issues Page.
You can also reach out to me on LinkedIn: Oliver Wackenreuther https://ca.linkedin.com/in/oliverwackenreuther

## Contributing
Contributions are welcome! If you'd like to contribute to this project, please fork the repository and submit a pull request with your proposed changes.

