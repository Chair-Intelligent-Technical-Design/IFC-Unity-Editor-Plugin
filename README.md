# IFC-Unity-Editor-Plugin
Load IFC files during design time into unity.

## Features
1. Load geometry of IFC files
2. Prepare the geometry for texture usage (done on loading and starting of a project including the package)
3. Load and link semantic information of IFC files
4. provide access to semantic IFC information on runtime and design time
5. Generate Unity material based on the material definitions in the IFC file
6. Automatic assignment of Unity materials to model
7. Assign colliders based on the IFC classes: IfcWall, IfcRoof, IfcBeam, IfcColumn, IfcSlab, IfcCurtainWall, IfcPlate, IfcStair, IfcStairFlight, IfcRamp, IfcRampFlight
8. To ease interactions, all GameObjects are assigned to one of three layers automatically based on their IFC type.
	* IfcWall, IfcWallStandardCase, IfcBeam, IfcColumn, IfcCurtainWall, IfcWindow and IfcPlate are added to layer 29 'Walls'
	* IfcRoof, IfcSlab, IfcStair, IfcStairFlight, IfcSite, IfcRamp and IfcRampFlight are added to layer 30 'Grounds'
	* IfcDoor and IfcDoorStandardCase are added to layer 31 'Doors'
9. Enable/Disable automatic generation of texture maps to save time during development

## Download
Download of the binaries in the [release section](https://github.com/Chair-Intelligent-Technical-Design/IFC-Unity-Editor-Plugin/releases).

## Tutorial
A video tutorial may be found at https://vimeo.com/821292560. All binaries are included in the release section. The source code does not contain IfcConvert. If you want to build the plugin on your own, you have to download IfcConvert on your own and include it in your project.

## Acknowledgements
For this software, the [xBIM Toolkit](https://xbim.net/open-toolkit/) and [IfcConvert](https://github.com/IfcOpenShell/IfcOpenShell) have been used.

## Changes
### 1.0
* Initial version with feature 1 to 6. 6 Not for layered or constitued materials

### 1.1
* Adding feature 7

### 1.2
* Extending Feature 5 to also cover layered and constitued materials
* Adding feature 8
* Adding feature 9

### 1.3
* Bugfix: the collider builder doesn't hinder the build process anymore
