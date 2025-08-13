All files in here are under the Unity Companion License: [http://www.unity3d.com/legal/licenses/Unity\_Companion\_License](http://www.unity3d.com/legal/licenses/Unity_Companion_License)

Files in this folder are copies of files and folder structure from samples of Unity packages.
There are two exceptions: 
- `XR Origin (XR Rig) - (Added Vignett & Hand Visual)` is a modified version of `XR Origin (XR Rig)`
- `ClimbTeleportDestinationIndicator.cs` is in a aditional folder with an assembly definition refrence to `VRBuilder.Unity.XR.Interaction.Toolkit.Samples.StarterAssets` as it needs acces to the XRI internal method `ComponentLocatorUtility`.

The reason for copying is to remove the dependency on XRI samples.
When updating XRI to another version, these files might need to be updated as well.