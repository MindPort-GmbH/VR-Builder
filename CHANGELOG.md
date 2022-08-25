# Changelog - VR Builder

**v2.4.0 (2022-08-25 - Current)**

*[Added]*
- Added new rig with animated hands in place of controllers. The new rig is the default for newly created scenes, but the old one is still available as a prefab. Note that this will not automatically update the rig in a previously created scene. To do so, delete the existing rig then respawn it by selecting Tools > VR Builder > Setup Scene for VR Process.

*[Changed]*
- The Setup Wizard has been split in Project Setup Wizard and Scene Setup Wizard. The Project Setup Wizard opens after importing VR Builder and allows to configure the project. The Scene Setup Wizard should be called to create or configure a new VR Builder scene. Both wizards are available from the menu. This will allow us to extend and specialize them in the future.
- Updated behavior and condition help links. Clicking on the question mark on a behavior/condition's header in the Step Inspector will open the relevant VR Builder documentation.

**v2.3.2 (2022-08-04)**

*[Added]*
- Added utility functions to some data properties, useful to change a data property value e.g. through a Unity event. There is a function that increases a number data property and one that inverts the value of a boolean data property.

*[Changed]*
- Data properties now use Unity events, therefore it is possible to bind functions in the inspector that are executed when the value of the property changes or is reset.

*[Fixed]*
- Fixed issue where teleporting to an anchor did not always succeed.
- Fixed issue where Unity 2021+ got stuck while importing VR Builder for the first time.
- Fixed progress bar for VR Builder dependency import.

**v2.3.1 (2022-07-20)**

*[Changed]*
- Updates related to the Menus add-on.
- A warning will be displayed when using custom meshes as snap zone highlights. The meshes need to be set readable in order for the highlights to be displayed in builds.

**v2.3.0 (2022-07-06)**

*[Added]*
- Added support for displaying a 0-1 slider in the step inspector.
- Added type value to step metadata, in order to support different step visualization strategies.
- Entities can now retrieve their parent.

**v2.2.1 (2022-06-14)**

*[Fixed]*
- Fixed build error due to incorrectly set assembly platforms in the VRBuilder.Editor.PackageManager.XRInteraction assembly.


**v2.2.0 (2022-06-10)**

*[Added]*
- Streamlined scene setup by spawning a default rig in the scene instead of rig loader and dummy user prefabs. This makes it easier and more intuitive to customize or replace the rig. VR Builder remains compatible with scenes created with previous versions, and it is still possible to manually add the rig loader for advanced use cases. Note: if using Unity 2019 and OpenVR, the default rig needs to be manually replaced with the [XR_Setup_Device_Based] prefab.

*[Fixed]*
- Fixed bug with Object in Collider condition completing instantly when it should not in some cases.
- Fixed incorrect snap zone materials when using URP.
- Minor improvements to the node editor UI.


**v2.1.0 (2022-05-11)**

*[Added]*
- First iteration of a new node editor based on Unity’s GraphView API. This more flexible system will allow us to more easily make improvements to the node editor. Benefits from the switch include the ability to zoom the graph, select and drag multiple nodes and to rename a node by double clicking on its name. Processes created with previous versions are compatible but nodes may need rearranging.

*[Changed]*
- VR Builder does not include anymore a Newtonsoft JSON plugin, but instead downloads and uses the version from Unity’s Package Manager. This should offer better compatibility and allows to build for ARM64 with IL2CPP, which is relevant for Android headsets like the Oculus Quest devices. In Unity 2021+, Manage Stripping Level in the Player Settings needs to be raised to Low in order for the build to succeed.

*[Known Issues]*
- Double clicking to rename a node does not work on Unity 2019.


**v2.0.0 (2022-04-01)**

*[Added]*
- Unsnap Object behavior to unsnap objects from snap zones in the process logic.
- New Process Wizard is visible on iOS devices.

*[Changed]*
- External downloads are no longer required: VR Builder now includes everything in the Unity Asset Store package.
- The XR Interaction Component can be disabled in VR Builder’s project settings if needed, e.g. if you want to use another interaction framework.
- XR Interaction Component is now based on XR Interaction Toolkit v2.0.0.


**v1.3.0 (2022-03-07)**

*[Added]*
- Improved logging: Option to log data property changes in the project settings so that you can easily debug data properties.
- Technical refactoring for States & Data add-on.

*[Changed]*
- Data property operations work in the States & Data add-on


**v1.2.1 (2022-02-23)**

*[Added]*
- Better accessibility to helpful information: Links to Add-ons & Integrations overview page and Interhaptics integration added in menu and documentation.


**v1.2.0 (2022-02-11)**

*[Added]*
- Technical refactoring for Track & Measure add-on.

*[Changed]*
- Renaming menu items for improved naming convention.
- Users are warned to make a back-up when adding VR Builder to existing projects.
- Choice between "required" and "latest" version in the VR Builder downloader.
- Renamed "Create New Process..." to “New Process Wizard”.

*[Fixed]*
- Error when dragging Process Scene Object prefabs into the scene


**v1.1.0 (2022-01-21)**

*[Added]*
- New "Set Parent" behavior: Allows to change the parent of a scene object in the hierarchy.
- Improved usability for Bezier spline paths: Option to approximate a linear progression on the curve to better control the speed of movements.

**v1.0.0 (2022-01-10)**

*[Changed]*
- The XR interaction component has been modularized so that you can use other interaction components instead.
- Renaming training-related terminology in the code: "training/course" became "process", "trainee" became "user".
