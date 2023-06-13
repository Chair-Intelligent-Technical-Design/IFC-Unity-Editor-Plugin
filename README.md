# IFC-Unity-Editor-Plugin
Load IFC files during design time into unity.

##Features
* Load geometry of IFC files
* Prepare the geometry for texture usage (done on loading and starting of a project including the package)
* Load and link semantic information of IFC files
* provide access to semantic IFC information on runtime and design time
* Generate Unity material based on the material definitions in the IFC file
* Automatic assignment of Unity materials to model (does not work for layered materials)
* Assign colliders based on the IFC classes: IfcWall, IfcRoof, IfcBeam, IfcColumn, IfcSlab, IfcCurtainWall, IfcPlate, IfcStair, IfcStairFlight

A video tutorial may be found at https://vimeo.com/821292560. All binaries are included in the release section. The source code does not contain IfcConvert. If you want to build the plugin on your own, you have to download IfcConvert on your own and include it in your project.

Download of the binaries in the [release section](https://github.com/Chair-Intelligent-Technical-Design/IFC-Unity-Editor-Plugin/releases).

For this software, the [xBIM Toolkit](https://xbim.net/open-toolkit/) and [IfcConvert](https://github.com/IfcOpenShell/IfcOpenShell) have been used.

