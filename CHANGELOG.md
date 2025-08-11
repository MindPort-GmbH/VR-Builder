# Changelog - VR Builder

**v5.3.2 (2025/08/07 - Current)**

*[Added]*
- Updated the default rig to match the XRI Toolkit 3.1.2 rig.
- Added tunneling vignette option to the default rig.
- Added animated hands option to the default rig.

*[Fixed]*
- Fixed exception in WebGL mode.
- Fixed API break related to LockOnParentObjectLock.

**v5.3.1 (2025/07/01)**

*[Added]*
- Added generic control conditions and property interfaces to basic interaction component.

**v5.3.0 (2025/06/18)**

*[Added]*
- Lockable properties now include a Is Always Unlocked toggle. When checked, the VR Builder process will never lock the object, useful for sandbox environments.
- The Release Objects condition now includes a toggle to keep the objects unlocked after completion, like the Grab Objects condition already does.

*[Changed]*
- Maximum zoom scale changed to 300% in the Process Editor. This allows high DPI display to still display the graph at a readable size.
- Lock on Parent Object Lock on lockable properties renamed to Inherit Scene Object Lock State, to make its function clearer.

*[Fixed]*
- Fixed usable properties not correctly inheriting parent object lock state.

**v5.2.0 (2025/05/19)**

*[Added]*
- It is now possible to define a scene object implementation as the default implementation via the `DefaultSceneObjectProperty` attribute. This implementation will be used for auto-configuring game objects when multiple implementations of the same interface are available.

*[Changed]*
- When multiple `RUNTIME_CONFIGURATOR` objects are found in the scene, a warning is displayed instead of an error.
- Methods returning values in the `ISceneObjectManager` interface have been made void with the option to pass a callback to execute on completion. This lets them support async logic in implementations.

**v5.1.0 (2025/03/17)**

*[Added]*
- Added "all objects" option to Used and Touched conditions.

*[Changed]*
- Step description in the Step Inspector now supports multiple lines.
- VR Builder is again compatible with Unity 2022. Please note that this version still requires XRI 3+.
- Obsoleted RuntimeConfiguration `LocalUser` property. Please use `User` instead.
- Custom scene setups in the Scene Setup Wizard now can parent created objects. 
- Custom scene setups can create a scene from a scene template.
- Improved editor performance of Play Audio behavior inspector.
- Scene objects in scenes without a runtime configurator now throw warnings instead of errors.
- Made `LockableProperty` more extensible.

*[Fixed]*
- Added some missing textures to the controller.

**v5.0.1 (2024/12/20)**

*[Changed]*
- `GrabbableProperty` no longer requires a `TouchableProperty` on the same game object. Since touch is now handled with poking and requires different component, a grabbable object is no longer automatically touchable as well.

*[Fixed]*
- Fixed the wrong orientation of the left controller of the XRI rig (demo scene and prefab).
- Fixed filtering for label groups.

**v5.0.0 (2024/11/11)**

*[Changed]*
- VR Builder now supports Unity 6 out of the box. Unity 2022 and previous versions are not officially supported by VR Builder 5.
- Since Unity 6 uses URP by default, we now consider URP as our default render pipeline as well, and updated our content accordingly.
- The default interaction component is now based on XRI 3. This entails a completely new rig (based on the XRI default rig) and some updated interactions.
- The default raycast and interaction layers for teleportation are now named "Teleport" instead of "XR Teleport" and set to layer 31 if possible.
- Teleportation anchors now have two more options: Add Snap Volume and Add Interaction Affordance, respectively auto configuring the anchor so that the teleportation ray snaps to it and so that it provides affordance when hovered.
- Changed and tidied up namespaces. This can result in API breaks on custom content, but it should be a simple case of pointing to the new namespace.
- Merged Text-to-Speech and Basic Interaction assemblies in the Core assembly, in order to simplify the architecture. Custom content that referenced these assemblies now needs to reference the Core assembly.
- Removed obsolete code.

*[Known issues]*
- There is no automated upgrade path from VR Builder 4 processes. It's technically still possible by manually editing the JSONs, but we recommend not upgrading a project in progress.
  
**v4.4.0 (2024/10/31)**

*[Added]*
- The `Object in Collider` condition now supports multiple object references and lets the user specify a number of objects that needs to be in the collider.
- Every text-to-speech provider implementation can now have custom configuration fields. This makes it easier to implement custom TTS providers.
- `UserSceneObject` now has a `Base` property corresponding to the position of the user's feet.


*[Changed]*
- Standardized component configuration sections in Project Settings (like the SnapZone settings) to make it easier to create more.
  
*[Fixed]*
- Fixed automatically focusing the Step Inspector window when recompiling.
- Play Audio behavior now stops playing before aborting.
- If a condition is a lockable property provider, lockable properties are retrieved using the condition's API instead of a helper method. This makes it easier to implement custom conditions that return a specific set of lockable properties.

**v4.3.0 (2024/09/12)**

*[Added]*
- The Parallel Execution node now lets you create optional paths. You can make a path optional by clicking the new button next to its name. An optional path will be interrupted when all non-optional paths have completed, and execution will immediately proceed to the next node. This can be useful to create background looping animations, recurring hints and so on. An optional path can even be an endless loop and it will still be interrupted.
- Added advanced user settings to `Project Settings > VR Builder`. Advanced users can, for example, skip the "adding Process Scene Object" dialog or automatically add properties when an object is dragged in the step inspector, effectively auto-pressing the "Fix it" button.
- VR Builder components now have a custom icon instead of the Unity script icon.
- Added API to abort entities to entity lifecycle. Calling the `Abort()` method on an entity lifecycle will enter a short Aborting stage where all child entities are aborted as well, then will revert the entity to Inactive. Note that while every lifecycle can be individually aborted, it mostly makes sense to abort entire processes or chapters.

*[Changed]*
- The `Set Next Chapter` node has been renamed to `End Chapter` node. It now actively ends the current chapter (by aborting it) instead of just setting the following one. This means it now also works reliably in nested nodes, so we made it possible to create this node inside step groups or parallel execution nodes, where it was previously greyed out.
  
*[Fixed]*
- Fixed warning in Unity 2022+ when using the Move Object behavior to move a kinematic object.

**v4.2.1 (2024/07/29)**

*[Added]*
- Added resource drawer for animation clips, in order to support the new Play Animation Clip behavior in the Animations add-on version 2.1.0.
  
**v4.2.0 (2024/07/05)**

*[Added]*
- Added support for copy/pasting behaviors and conditions in the Step Inspector. The Delete button has been replaced with a Menu button which allows to delete, copy and paste behaviors and conditions, similar to the Unity inspector. Additionally, a Paste button has been added next to the New Behavior/Condition button to allow pasting into an empty inspector.
- New Load Scene behavior letting you load scenes with options to load additively and/or asynchronously.
- Generic dropdown drawer that can be used for your custom behaviors/conditions: your class should extend `DropdownDrawer` and should be passed a string in the attribute `[UsesSpecificProcessDrawer("YourDrawerName")]` above the property you want to select via dropdown.

*[Changed]*
- The Process Editor now displays a message when it is open in a scene without a process. This also displays warnings in case the process has been moved/deleted.
- The Runtime Configurator now handles a moved/deleted process by displaying a message in the dropdown instead of silently defaulting to the first selectable process.
- Improved the UI of the dropdowns showing scene object groups.
- Improved error handling with having multiple instances of type RuntimeConfigurator in all loaded scenes.

*[Fixed]*
- Black text on the "drop object here" box of scene object references in Step Inspector.
- Fixed exception when attempting to create snap zone highlight from multiple large meshes.
- Fixed a build error.
- Small editor performance improvements in behaviors/conditions that let you choose between a data property and a constant value.

**v4.1.1 (2024/06/07)**

*[Changed]*
- Significantly improved editor performance when deleting a step in a chapter with many steps.
- The user's position in the graph is now persisted between sessions.
- The selected tab in the step inspector (Behaviors, Transitions or Unlocked Objects) is now stored globally, so it will stay the same when cycling through steps.
- HasGuidValidation no longer accepts any object if no guids are specified, but will accept no object as is logical instead. While this can result in an unusable snapzone, it makes it possible to potentially add valid objects at runtime.

*[Fixed]*
- The progress bar doesn't get stuck when the TTS provider fails to generate a clip, an error message is displayed in the console.
- Fixed language settings not being saved correctly in certain cases.
- Fixed process controller prefab so that unnecessary components are not present when Standard is selected.
- Groups created from a Process Scene Object are persisted correctly.
- Fix for occasional ArgumentOutOfRangeException causing the wizard not to display correctly when importing VR Builder for the first time.
- Fixed the bug where an outdated process is executed when pressing play.

**v4.1.0 (2024/05/17)**

*[Added]*
- Significantly improved runtime performance for processes with many steps. Now the number of steps in a process has no impact on the performance.
- New multiline text drawer contributed by LEFX - thank you! The drawer is currently used in the Text To Speech behavior but can be manually set for fields in other behaviors/conditions.
- Added `ForceLockControllerState` and `UnlockControllerState` public methods to `ActionBasedControllerManager`. These can be used to externally lock the controllers e.g. in UI mode or teleport mode, which can be useful if the user is required to perform a specific action.

*[Changed]*
- The `UserSceneObject` component does not inherit from `ProcessSceneObject` anymore. This allows you to use the rig in a scene without scene object registry without getting errors - for example when loading additively scenes with processes in them. As a result, `BaseRuntimeConfiguration.User` does not work anymore. You can still use `BaseRuntimeConfiguration.LocalUser` which provides the same functionality while returning a `UserSceneObject` component. Note that nothing stops you from manually adding `ProcessSceneObject` components to parts of the rig you want to interact with the process.

*[Fixed]*
- A `link.xml` file is now automatically created before builds in order to prevent managed stripping from removing behavior/condition code which is actually in use, and thus resulting in a non-functional process in a standalone headset.
- Fixed tags on game objects not being converted to groups when upgrading from version 3 to 4.
- Fixed bug in the automatic updater of the Enable/Disable Object by Tag behaviors. Now these should be replaced by a correctly configured non-obsolete behavior.
- Scene property extensions will not be added multiple times when selecting the `Add Scene Property Extension` menu entry in a scene where they have already been added.

**v4.0.0 (2024/04/18)**

*[Added]*
- Added upgrade tool for updating processes to the new referencing system (see below). If you open an old process in VR Builder 4.0.0, all object references will be null because the system has changed. You can attempt to update the process by opening its scene, then selecting Tools > VR Builder > Developer > Update Process in Scene. Note it's important that the correct scene is loaded, as the updater will have to search for the correct game objects in order to update the references.
If you have custom behaviors or conditions in your project, you'll need to update them to the new referencing system. Afterwards they will be supported by the automatic updater.
- Added support for TTS when using an async provider, courtesy of LEFX.

*[Changed]*
- Rebuilt the system for referencing scene objects in the process from the ground up. The new system is more robust and lets you use prefabs normally, without worrying about unique name conflicts. References in behaviors and conditions now can hold a mix of unique objects and object groups (formerly tags). They can be single or multiple references, meaning they will return one object or all viable objects. Make sure to check the documentation for more information.
- "By tag" behaviors and conditions have been removed. Now there is only one version of a given behavior/condition, but where it makes sense it will be possible to reference multiple objects, thus replicating the functionality.
- VR Builder is now a hybrid package. This means that while it's still an Asset Store .unitypackage, it will be imported in the Packages section of the project instead of your Assets folder. It's recommended to delete the MindPort/VR Builder/Core folder under Assets before updating to this version.
- Scene Object Tags have been renamed to Groups. Functionality is the same, but if updating from an old project you will notice that the Scene Object Groups settings page is empty. You can import your old tags by selecting Tools > VR Builder > Developer > Update Object Groups.
- Objects unlocked by conditions are now unlocked when the step is active instead of activating. This means, for example, that the teleportation anchor for a teleport condition will appear after all blocking behaviors have finished instead of the beginning of the step. It also means that it won't be possible to teleport there before the condition starts checking.
- The process won't stop executing because of entity lifecycle exceptions, instead, an informative error will be displayed in the console.

*[Fixed]*
- Fixed Set Next Chapter node exception when being fast forwarded.
- Fixed some errors when creating a new scene through the wizard.
- Fixed manually unlocked objects not locking again if they had been manually unlocked in multiple consecutive steps.
- Fixed unresponsive buttons in the Scene Object Groups settings page.

*[Removed]*
- Removed previously deprecated code. This includes the C# events on all properties, the old process editor, and more.

**v3.4.0 (2023/12/01)**

*[Added]*
- Experimental hand tracking rig based on the default XRI Hands rig. The prefab is named `XR_Setup_Action_Based_HandTracking` and can be used in place of the default rig. The rig supports both controllers and hand tracking. Note that there is no teleportation solution currently available for hand tracking, and some controls and behaviors are slightly different from the standard rig.
- Added dependency to the XR Hands package.
- The XR Teleport interaction layer is now automatically created when importing VR Builder.
- Added a check to ensure rig and teleportation areas/anchors are set to the correct raycast/interaction layers. The rig is automatically set up on rig creation, and you can check the entire scene manually by selecting `Tools > VR Builder > Developer > Configure Teleportation Layers`. The demo scene is automatically set up when opened from the menu or the wizard.
- Added animation curve functionality to the Move and Scale Object behaviors. Thanks LEFX!

*[Changed]*
- Updated XRI dependency to XRI 2.5.2
- Changed how the Project Setup Wizard decides whether to show the hardware selection page: now the page will show if none of the common XR SDKs (OpenXR, OculusXR, WMR) are installed.

*[Fixed]*
- Fixed having a full path stored in the runtime configuration instead of a relative one when renaming a process, which could cause issues in builds or when working across different computers. 

**v3.3.2 (2023/10/31)**

*[Added]*
- It is now possible to add proximity detection to VR Builder teleportation anchors. This means that the anchor will send a teleported event readable by VR Builder even if the user gets near it by continuous locomotion or walking, without teleporting. Click the "Add Teleportation Proximity Entry" button on the teleportation anchor to instantiate the necessary components.
- Added support for the Cognitive3D integration.
- Added drawer for selectable value between int and data property reference.

*[Changed]*
- The touchable property now recognizes touch from any direct interactor, not only from interactors parented to the user scene object.

*[Fixed]*
- Fixed issue when having punctuation in the name of a localization table.

**v3.3.1 (2023/09/28)**

*[Added]*
- Added auto-configuration options to VR Builder's custom Teleportation Area and Anchor components. You can now use the provided buttons to automatically configure the teleport interactable to work with the VR Builder rig, and, in the case of the Anchor, you have the option to set up the default anchor. Note that this functionality is no longer available on the Teleportation Property.
- Added error message when building audio with localization enabled but no localization table assigned to the process.
- Added PropertyExtensionExclusionList component, which can be added to the game object containing the SceneConfiguration in order to exclude specific property extension types.

*[Changed]*
- Changed the way transitions are named in the process editor. Instead of showing the step they lead to, they display the name of their first condition, followed by a number in case more conditions are present. This should be more informative and help understand the process at a glance.
- Moved user scene object on rig root instead of the main camera. RuntimeConfiguration.User is obsolete, use LocalUser instead. Transforms for head and hands can be accessed through the LocalUser property.

*[Fixed]*
- The default teleportation anchor is now compatible with URP.
- Fixed potential cause of corruption of the JSON file.

**v3.3.0 (2023/08/29)**

*[Added]*
- Added support for Unity Localization based on a contribution by LEFX (https://www.lefx.de/en/). It is now optionally possible to use the Localization package in Play Audio and Play Text-to-Speech behaviors. Users need to set up localization and create a localization table which needs to be assigned to the process on the PROCESS_CONFIGURATION game object. It is then possible to type keys in the behaviors and add corresponding localized text in the localization table. The Project Wizard provides a complete list of steps for setting this up.
- Added a Start/Stop Particle Emission behavior which can control a particle emitter more naturally than just enabling or disabling the game object or component.

*[Fixed]*
- Fix for lockable objects without LockOnParentObjectLock being impossible to unlock after being force-locked on scene start.
- Fixed incorrect links when pasting steps from the same clipboard multiple times.

**v3.2.0 (2023/08/04)**

*[Changed]*
- Updated minimum XRI version to 2.4.1.
- Improved the project setup wizard: now it is possible to select more than one device at once.
- The default SDK for Meta Quests is now OpenXR. Oculus XR can still be selected via the Legacy option.
- Improved JSON serialization: now there is no theoretical limit on having step groups within groups; removed the limitation of creating groups only in the root chapter.
- Added support for different implementations of how to save the process asset.

*[Fixed]*
- Now VR Builder automatically closes the Process Editor before building in order to avoid accidental process corruption.

**v3.1.0 (2023/07/12)**

*[Added]*
- Parallel Execution node: this node works similarly to the Step Group node but lets you create multiple paths which are independent from one another and executed at the same time. The Parallel Execution node completes when all paths have ended.

*[Changed]*
- When the process file is changed externally (e.g. because of source control) and the Process Editor is open, a dialog will appear asking which data to use.

*[Fixed]*
- Fixed tags not saved when created from the inspector.
- Replaced FindObjectsByType call with FindObjectsOfType for better backwards compatibility.
- Fixed confetti machine not working in demo scene.
- Removed a few instances where the process file was saved unnecessarily.
- Fix for chapter started and step started events in Process Runner being called repeatedly.
- Fix for editor icon not found error.

**v3.0.1 (2023/06/05)**

*[Added]*
- Added menu entry that directly links to the roadmap.

*[Changed]*
- Snap zones now require to select a material for the base highlight instead of a color. This adds flexibility and is consistent with the valid/invalid materials.
- When pasting nodes in the process editor, those are now pasted on the mouse cursor's position instead of the same position as the copied nodes.

*[Fixed]*
- Fixed process editor losing focus when step is selected.
- Fixed issue with copied object references using the Duplicate Chapter button.
- Fixed hand animations not working as intended.
- Snap zone preview mesh is updated when the highlight settings are changed.

**v3.0.0 (2023/04/24)**

*[Added]*
- Added custom overrides `Teleportation Anchor (VR Builder)` and `Teleportation Area (VR Builder)`. Those should be used instead of the XRI defaults and will provide automated configuration and other useful features in the future.
- Added Dummy Text-to-speech provider. This provider generates blank files and can be used as a fallback on hardware that does not support the default Microsoft SAPI provider.
- Added support for property extensions, which are components that are automatically added along with a certain scene object property. To create your extensions, override `ISceneObjectPropertyExtension<TProperty>` and ensure the relevant assembly is listed in the scene configuration (see below).
- The PROCESS_CONFIGURATION object now includes an additional `Scene Configuration` component. This stores configuration pertinent to the scene, but not necessarily the whole project. At the moment, the configuration defines which assembly should be checked for property extensions and the default confetti prefab.

*[Changed]*
- VR Builder now requires and supports XRI 2.3.1 and later.
- All properties now use Unity events instead of C# events. This allows users to more easily use them with their own logic in the Unity inspector.
- Behaviors and conditions now are dynamically named. Names are more informative and are not stored in the process JSON anymore.
- Spawning objects and enabling/disabling objects and components now has an additional abstraction layer between the relevant behaviors and the Unity logic. This allows to handle these things differently in custom implementations, e.g. our upcoming multiuser support. Custom behaviors interacting with gameobject in these ways should do so through the `SceneObjectManager` found in the runtime configuration.
- Obsoleted the `InstructionPlayer` audio source reference in the runtime configuration. The Play Audio behavior and other behaviors playing audio should use the abstracted `ProcessAudioPlayer` instead.

*[Fixed]*
- Fixed Step Inspector window occasionally duplicating itself on recompile.
- Fixed tags not saved properly on prefab objects.

**v2.8.0 (2023/03/10)**

*[Added]*
- New variant of the *Snap Object* behavior that allows to snap any object with a given tag in a specified snap zone.
- Added *Duplicate Chapter* button to the chapter list in the step inspector. It can be used to create a copy of the currently selected chapter.

*[Changed]*
- The *Snap Object by Reference* condition now supports leaving one object reference field empty. This way, the condition will complete either when the user snaps a specific object in any snap zone, or when any object is snapped in a specific snap zone. Note that manual unlocking of objects or snap zones may be required, and fast forwarding will not automatically snap anything if a field is empty.

*[Fixed]*
- Copy/paste should now work as expected.
- Automated setup for OpenXR-based headsets should now work as expected.
- Change the way the step inspector focuses when a step is selected. This should help with the step inspector disappearing on macOS.

**v2.7.1 (2023-02-01)**

*[Changed]*
- Changed the way text-to-speech audio works: it is no longer possible to generate TTS audio at runtime. This will ensure a build works consistently regardless of which machine is running on, as audio for builds will always be synthesized and stored in advance. Missing/changed audio is automatically generated when creating the build. Buttons to manually generate/flush files have been added in Project Settings > VR Builder > Language.
- Refactored the TTS system to make it easier to add new TTS providers modularly, without the need to edit VR Builder files.
- Improvements to the drawer of the Play Audio/TTS behavior in the Step Inspector.
- Removed support for Google (v1) and Watson TTS providers.
- It is now possible to build a VR Builder project on WebGL. Note that WebGL does not support VR, but this can be useful for some advanced, custom use cases.

*[Fixed]*
- It is now possible to build for Android using IL2CPP with managed stripping set to "Minimal" - before, this had to be set to "Low" as a workaround.
- Grouping many steps in step groups no longer breaks the process JSON.

*[Known Issues]*
- TTS might not work properly on WebGL builds.

**v2.7.0 (2023-01-17)**

*[Added]*
- New "Step Group" node which can be used to group together a cluster of nodes and improve graph organization. Existing nodes can be grouped from the context menu, or an empty group can be created and populated. Caveats: currently the group node only has one input and one output. End Chapter nodes are not supported in step groups. Nested step groups are not recommended.

*[Changed]*
- Changed teleportation logic and updated rigs accordingly to improve reliability. Anchors and teleportation areas now need their Teleport Trigger to be set to "On Deactivated". This is taken care of automatically when autoconfiguring teleportation anchors, but already configured anchors/areas will not work as intended with the new rig and viceversa. Note that you will need to delete the rig and re-perform setup to create an up-to-date one in an existing scene.

*[Fixed]*
- Fixed process not loading correctly on some IL2CPP Android builds.
- Fixed scene object reference to a child in a prefab becoming invalid when the prefab is edited.
- Fixed process scene objects in additively loaded scene not registering.
- Fixed process scene objects in additively loaded scene requesting a new unique name on load.

**v2.6.0 (2022-12-01)**

*[Added]*
- New "End Chapter" node which can be used as an exit node and lets you specify which chapter will start next. This makes it possible to build non-linear processes where some chapters are skipped or you return back to previous ones!
- Scene Object Tag system: now a Process Scene Object can have a number of tags in addition to its unique name. This allows for manipulating scene objects in bulk instead of always relying on a unique reference.
- Scene Object Tag variation for the following behaviors/conditions: Enable/Disable Object, Enable/Disable Component, Grab Object. This makes it possible, for example, to disable all object with a given tag or to have a grab condition for an unspecified object with the relevant tag.
- Enable/Disable Component behaviors, which enable or disable all components of a specified type on the target object.

*[Changed]*
- Renamed the Workflow Editor to Process Editor.
- New node creation is now nested under the "New" context menu.

*[Fixed]*
- Fixed the Step Inspector taking a long time to open for the first time after loading the project. The time-consuming operations are now performed in the background when the project is loaded or recompiles.
- Fixed a build error when attempting to build while the Process Editor is open.
- Various other fixes and improvements.

*[Known Issues]*
- When importing in an empty project in Unity 2021.3.14, the editor can crash instead of restarting. If that's the case, just restart the editor manually and let the process finish.

**v2.5.1 (2022-10-18)**

*[Changed]*
- Renamed and reprioritized the process controllers. The Default process controller is now called Spectator, and the Standalone is now called Standard. The Standard process controller is now selected by default. After this update, you might have to re-select the Spectator process controller if you were using it in existing scenes!
- Overhauled the inspector for the Bezier Spline component. It is now possible to delete the last curve added and to select and edit the individual control points from the inspector, in order to avoid situations where a point would become unselectable due to overlapping.

**v2.5.0 (2022-09-30)**

*[Added]*
- Rebuilt the demo scene from scratch. Along with much improved graphics, the new scene includes a more complex linear process that better showcases the possibilities of the core VR Builder. Try it out!
- The workflow editor window now remembers the zoom and panning of the different chapters as long as it stays open. Closing and reopening the window will show the graph at its default position.
- Added option to open the demo scene right after the project setup wizard.

*[Changed]*
- The hardware setup wizard now allows to choose from a list of devices instead of APIs. This should make initial setup more user-friendly.
- Removed the blocking dialogs when installing hardware-related packages after the setup wizard has been closed.
- Removed the blocking dialog when opening the demo scene from the menu.
- Changed the default position of the start node in the workflow editor window.

*[Fixed]*
- Fixed a couple issues leaving hanging connections in the workflow editor.
- The touchable property now recognizes being touched only by interactors that are part of the VR rig. This avoids object being accidentally "touched" by random snap zones.
- The Move Object behavior resets object velocity before and after the behavior. This avoid an object resuming its momentum after the behavior has ended.
- XR loaders should now stay selected after automated hardware setup.

*[Known Issues]*
- Copying a group of nodes in the workflow editor will disconnect the original nodes from the first non-copied node.
- Deleting a pasted node can cause connection on the original node to be deleted.

**v2.4.0 (2022-08-25)**

*[Added]*
- Added new rig with animated hands in place of controllers. The new rig is the default for newly created scenes, but the old one is still available as a prefab. Note that this will not automatically update the rig in a previously created scene. To do so, delete the existing rig then respawn it by selecting Tools > VR Builder > Setup Scene for VR Process.

*[Changed]*
- The Setup Wizard has been split in Project Setup Wizard and Scene Setup Wizard. The Project Setup Wizard opens after importing VR Builder and allows to configure the project. The Scene Setup Wizard should be called to create or configure a new VR Builder scene. Both wizards are available from the menu. This will allow us to extend and specialize them in the future.
- Updated behavior and condition help links. Clicking on the question mark on a behavior/condition's header in the Step Inspector will open the relevant VR Builder documentation.
- Updated XR Interaction Toolkit dependency to version 2.1.1.

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
