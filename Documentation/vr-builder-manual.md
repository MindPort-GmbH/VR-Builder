# VR Builder Manual

## Table of Contents

1. [Introduction](#introduction)
1. [Requirements](#requirements)
1. [Installation](#installation)
1. [Quick Start](#quick-start)
    - [Demo Scene Overview](#demo-scene-overview)
    - [Demo Scene Hierarchy](#demo-scene-hierarchy)
    - [Workflow Editor](#workflow-editor)
1. [Default Behaviors](#default-behaviors)
    - [Play Audio File](#guidanceplay-audio-file)
    - [Play TextToSpeech Audio](#guidanceplay-texttospeech-audio)
    - [Hightlight Object](#guidancehightlight-object)
    - [Audio Hint](#guidanceaudio-hint)
    - [Spawn Confetti](#guidancespawn-confetti)
    - [Behavior Sequence](#utilitybehavior-sequence)
    - [Delay](#utilitydelay)
    - [Set Parent](#utilityset-parent)
    - [Disable Object](#environmentdisable-object)
    - [Enable Object](#environmentenable-object)
    - [Unsnap Object](#environmentunsnap-object)
    - [Move Object](#animationmove-object)
1. [Default Conditions](#default-conditions)
    - [Move Object into Collider](#environmentmove-object-in-collider)
    - [Object Nearby](#environmentobject-nearby)
    - [Grab Object](#interactiongrab-object)
    - [Release Object](#interactionrelease-object)
    - [Snap Object](#interactionsnap-object)
    - [Touch Object](#interactiontouch-object)
    - [Use Object](#interactionuse-object)
    - [Timeout](#utilitytimeout)
    - [Teleport](#vr-userteleport)
1. [Online Documentation](#online-documentation)
1. [Acknowledgements](#acknowledgements)
1. [Contact](#contact)

## Introduction

## Requirements

VR Builder works on Unity 2019.4 or later.

## Installation

Download and import the VR Builder package from the package manager. VR Builder will compile and import some dependencies. If Unity's new input system is not selected in the player settings, the following window will appear prompting the user to automatically switch.

![Restart Unity](images/installation-restart-input-system.png)

Please click `Yes` in order to restart the editor and enable the new input system. Note that the new input system is required by VR Builder, but you can enable both the new and the legacy one if it makes sense for you to do so.

Then you will be presented by another dialog, this time from Unity's XR Interaction Component.

![Update Interaction Layermask](images/installation-xrit-layermask-update.png)

VR Builder should work with either choice, so select the option that better suits your existing project, or just go ahead if starting from a blank project.

After the automated restart, you should be presented with the New Process Wizard.

![Wizard Welcome Page](images/installation-wizard-welcome.png)

Click `Next` to proceed to the process setup page.

![Wizard Process Page](images/installation-wizard-process.png)

Here you can choose whether to create a VR Builder process in the current scene, create a new process scene, or open the included demo scene. Click `Next` afterwards.

Finally, if it's not configured already, you will be able to configure your project to work with your VR hardware.
Select one of the provided options to install the relevant packages from the Package Manager.

![Wizard Hardware Page](images/installation-wizard-hardware.png)

Note that further steps might be required to get your hardware fully functional - for example, if you select OpenXR you will need to manually select a controller profile.
After that, make sure that your VR SDK of choice is selected in the project settings, then VR Builder is good to go!

## Quick Start

You can have a first look around by opening the provided demo scene. This simple scene contains a pre-built process that showcases some of the interactions provided in VR Builder.

To open the demo scene, select the relevant entry in the wizard as described above, or use the shortcut in `Tools > VR Builder > Demo Scenes > Core`. Note that, while the scene can be found and opened from disk, it is necessary to use one of the above methods at least once in order to copy the process file to the StreamingAssets folder, where VR Builder processes are saved.

### Demo Scene Overview

The demo scene showcases how it is possible to assemble a process with the building blocks included in the core VR Builder. More building blocks and features will be made available as separate addons.

These building blocks are either conditions or behaviors. Conditions check if the user or the world is in a certain state, and behaviors change the world state when called.

The scene includes three stations. The user can teleport from the starting point to any station and back, and can choose to try the different stations in any order this way.

Snap station: showcases the grab/release conditions, and the snap condition which can be used in conjunction with snap zones.

![Snap station](images/station-snap.png)

Touch station: showcases the touch condition. Touching the button will trigger a simple behavior, confetti.

![Touch station](images/station-touch.png)

Tool station: showcases how object can be made usable. After grabbing the light sword, press the trigger to extend it.

![Tool station](images/station-tool.png)

### Demo Scene Hierarchy

Let's have a look at the hierarchy. 

![Hierarchy](images/hierarchy.png)

The four game objects in parentheses are automatically added to every VR Builder scene.

- `[PROCESS_CONFIGURATION]` allows to select the process for the current scene from the ones saved in the project.
- `[INTERACTION_RIG_LOADER]` allows to arrange the priority of the available interaction rigs. There should not be need to touch it except for using the simulator and playing the scene without a VR headset.
- `[USER]` is a dummy game object that defines the initial position of the user in the scene. On play, it will be replaced by the VR rig prefab.
- `[PROCESS_CONTROLLER]` defines some parameters for processes in this scene.

By looking at the other objects in the scene, we can see that some have a `Process Scene Object` component and possibly some "property" component. A `Process Scene Object` is an object with an unique name which can be seen by the process logic. Properties define how the process can interact with the object. For example, a `Grabbable Property` will let VR Builder recognize if an object is being grabbed, and adding a `Grabbable Property` to an object will automatically make it a `Process Scene Object` and add a few components to make the object grabbable in VR.

If these properties are not added manually the user will usually be prompted to add them automatically as needed while building the process.

### Workflow Editor

Now let's see how the process in the demo scene is built. You can open the Workflow Editor from `Tools > VR Builder > Open Workflow Editor` or `Window > VR Builder > Workflow Editor`. The window should look like this.

![Workflow editor](images/workflow-editor.png)

On the left, there is a list of chapters. The demo scene only has one chapter, but processes can have more. Every chapter is a separate section of the process. Chapters are useful to separate a process in its logical steps and avoid too much clutter in a single graph.

On the right, there is a graphical representation of the current chapter. Every node is called a `Step`, and can include a number of `Behaviors` which can happen when the node is triggered or before leaving it. In this example, those are mostly text to speech instructions. A step can have as many exit points, called `Transitions`, as needed. Every transition can list a number of `Conditions` which determine if it can be chosen.

Select, for example, the "Tool grabbed" node. This will open the Step Inspector and the window should look like the following.

![Step inspector behaviors](images/step-inspector-behavior.png)

The only behavior is a text to speech instruction that will be triggered when the node is entered. Click on the "Transitions" tab.

![Step inspector transitions](images/step-inspector-transitions.png)

Note there are two transitions, each with its own list of conditions, and more can be added. Each will lead to a different node, depending on which condition is satisfied first.

The first transition will trigger if the object is used (by pressing the trigger on the controller).

The second one if the object is dropped without being used.

Feel free to investigate the other nodes to understand how the demo scene is built.

## Default Behaviors

This section lists the default behaviors included in the base VR Builder package.

------

## Guidance/Play Audio File

### Description

This Behavior plays an audio clip loaded from the `Resources` folder in your project’s asset folder. VR Builder supports all audio file formats supported by Unity, which are:

- aif
- wav
- mp3
- ogg

### Configuration

- **Resources path**

    Relative file path from the Resources folder. Omit the file extension (see example).

    #### Example
     
    File to be played: `Assets/.../Resources/Sounds/click-sound.ogg`  
    Default resource path: `Sounds/click-sound`  

- **Execution stages**

    By default, steps execute behaviors in the beginning, in their activation stage. This can be changed with the `Execution stages` dropdown menu:

    - `Before Step Execution`: The step invokes the behavior during its activation.
    - `After Step Execution`: Once a transition to another step has been selected and the current step starts deactivating, the behavior is invoked.
    - `Before and After Step Execution`: Execution at activation and deactivation of a step.

- **Wait for completion**

    By default, the step waits for the audio file to finish. If you want the step to interrupt the audio in case the step is completed, uncheck this option. 
    
    Note: this might lead to an audio file not even being started.

------

## Guidance/Play TextToSpeech Audio

### Description

This behavior uses a synthesized voice to read text.

### Configuration

The default Text-to-Speech language is set to ‘English’. Check out our online [tutorial](https://www.mindport.co/vr-builder-learning-path/how-to-add-and-customize-verbal-instructions-in-unity) to learn how to configure the Text-to-Speech Engine (TTS).

- **Text**

    Here you can input the text to be synthesized by the TTS engine.

- **Execution stages**

    By default, steps execute behaviors in the beginning, in their activation stage. This can be changed with the `Execution stages` dropdown menu:

    - `Before Step Execution`: The step invokes the behavior during its activation.
    - `After Step Execution`: Once a transition to another step has been selected and the current step starts deactivating, the behavior is invoked.
    - `Before and After Step Execution`: Execution at activation and deactivation of a step.

- **Wait for completion**

    By default, the step waits for the audio file to finish. If you want the step to interrupt the audio in case the trainee completes the conditions, uncheck this option. 
    
    Note: this might lead to an audio file not even being started.

------

## Guidance/Hightlight Object

### Description

This behavior visually highlights the selected object until the end of a step.

Select the highlighted `Object` in the Unity Hierarchy and open the Unity Inspector. Search for the *Interactable Highlighter Script*.

[![Interactable Highlighter Script](images/interactable-highlighter-script.png)](../images/default-behaviors/interactable-highlighter-script.png)

You can define the Color and Material for *On Touch Highlight*, *On Grab Highlight*, and *On Use Highlight*. The object will show the highlight color configured in the Highlight behavior by default, as soon as the object is touched it will change to the color configured in *On Touch Highlight*. The same happens when the object is grabbed or used. It will display the configured color in ‘On Grab Highlight’ or ‘On Use Highlight’. 

### Configuration

- **Color**

    Color in which the target object will be highlighted. Colors are defined in the RGBA or HSV color channel. By configuring the alpha (A) value, highlights can be translucent.

- **Object**

    the `Process Scene Object` which should be highlighted.

------

## Guidance/Audio Hint

### Description

This composite behavior plays an audio file after a set time, for example to give the user some delayed hints.

### Configuration

This behavior is a sequence combining a Delay and a Play Audio File behavior. Please refer to the documentation for the [Behavior Sequence](#utilitybehavior-sequence), the [Delay behavior](#utilitydelay) and the [Play Audio File behavior](#guidanceplay-audio-file).

------

## Guidance/Spawn Confetti

### Description

This behavior causes confetti to fall above the selected `Object`. It can be useful as visual feedback or celebration for completing something.

### Configuration

- **Spawn Above User**

If checked, the spawn position will be above the user rather than on the specified `Process Scene Object`.

- **Position Provider**

Specifies where the confetti should spawn if not set to spawn above the user.

- **Confetti Machine Path**

Path to the confetti machine prefab, relative to a `Resources` folder. Use the default one or point to your custom confetti machine.

- **Area Radius**

Radius around the position provider in which confetti will be spawned.

- **Duration**

Duration of the visual effect in seconds.

- **Execution stages**

    By default, steps execute behaviors in the beginning, in their activation stage. This can be changed with the `Execution stages` dropdown menu:

    - `Before Step Execution`: The step invokes the behavior during its activation.
    - `After Step Execution`: Once a transition to another step has been selected and the current step starts deactivating, the behavior is invoked.
    - `Before and After Step Execution`: Execution at activation and deactivation of a step.

------

## Utility/Behavior Sequence

### Description

This behavior contains a list of child behaviors which will be activated one after another. A child behavior in the list will not be activated until the previous child behavior has finished its life cycle.

### Configuration

- **Repeat**

    If checked, the behavior sequence restarts from the top of the child behavior list as soon as the life cycle of the last child behavior in the list has finished.

- **Child behaviors**

    List of all queued behaviors. Add behaviors to the list using the *"Add Behavior"* button.

- **Wait for completion**

    if checked, the behavior sequence will finish the life cycle of each child behavior in the list before it transitions to another step. Even when the *"Repeat"* option is enabled, the execution will transition to the next step after the child behavior list has been completed. 
    Uncheck this option, If you want to interrupt the sequence as soon as all conditions of a transition are fulfilled.

------

## Utility/Delay

### Description

This behavior completes after the specified amount of time. Even when the user fulfills the required conditions to transition to the next step, this step will wait for the duration configured in `Delay (in seconds)`.  

### Configuration

- **Delay (in seconds)**

    configure the behavior’s delay duration in seconds.

    #### Example

    Delay (in seconds) = 1.3

------

## Utility/Set Parent

### Description

This behavior parents an `Object` to another one in the Unity hierarchy.

### Configuration

- **Target**

The `Process Scene Object` to be parented.

- **Parent**

The new parent for the target object. Note this can be null, in which case the object will be unparented.

- **Snap to parent transform**

If checked, the target object will snap to the same position and rotation as the parent object.

------

## Environment/Disable Object

### Description

This behavior makes the selected `Object` invisible and non-interactive until it specifically is set back to *"enabled"* in a future step.
Put into Unity terms, it deactivates the selected Game Object.

If you would like to make an object non-interactive while being visible, consider locking the object instead.

### Configuration

- **Object**

    the `Process Scene Object` to be disabled.

------

## Environment/Enable Object

### Description

This behavior makes the selected `Object` visible and interactive until it is specifically set back to *"disabled"* in a future step.
Put into Unity terms, it activates the selected Game Object.

### Configuration

- **Object**

    the `Process Scene Object` to be enabled.

------

## Environment/Unsnap Object

### Description

This behavior unsnap a snapped object from a snap zone. This can be useful in case the object needs to be further manipulated by the process, for example.

### Configuration

Either the object or the snap zone can be left null. This will result in either the object unsnapping from any snap zone it is in, or in the unsnapping of whatever object is snapped to the specified snap zone.

If both are specified, the unsnap will occur only if the specified object is snapped to the specified snap zone.

- **Object to unsnap**

The `Process Scene Object` to unsnap.

- **Snap zone to unsnap**

The `Snap Zone` the object will be unsnapped from.

------

## Animation/Move Object

### Description

This behavior animates the `Object` to move and rotate (no scaling) to the position and rotation of the `Final Position Provider` in the time in seconds specified in `Duration (in seconds)`.
 
Note: If `Object` was affected by gravity before, it will continue to be affected after this behavior. 

### Configuration

- **Object**

    the `Process Scene Object` to be moved and rotated (no scaling).

- **Final position provider**

    the `Process Scene Object` that is being used as position provider object which should be placed at the exact position and rotation where you want to move and rotate your `object` to.

- **Animation duration (in seconds)**

    time in seconds the animation takes to move and rotate `Object` to the `Final position provider`.

    #### Example
    
    Duration (in seconds) = 1.3

------

## Default Conditions

Conditions need to be active in order to be fulfilled. As soon as a step is active, all containing Conditions are active as well.

------

## Environment/Move Object in Collider

### Description

This condition is fulfilled when the `Object` is within the specified `Collider` for the required amount of time (`Required seconds inside`) while this condition is active.

### Configuration

- **Object**

    The `Process Scene Object` to move. If the object needs to be grabbed, it needs to have the `Grabbable Property` and a collider component configured. The collider defines the area where the trainee can grab this object.

- **Collider**

    The `Process Scene Object` with the collider you want to move the `Object` to. Make sure that it has a collider added and that the option `Is Trigger` is enabled.

- **Required seconds inside**

    Set the time in seconds that the `Object` should stay inside the `Collider`.

------

## Environment/Object Nearby

### Description

This condition is fulfilled when the `Object` is within the specified `Range` of a `Reference object`.

### Configuration

- **Object**

    The `Process Scene Object` that should be in the radius of the `Reference Object`. Make sure you add at least the `Process Scene Object` component to this game object in the Unity Inspector. 
    
- **Reference Object**

   The `Process Scene Object` you want to measure the distance from.

- **Range**

    In this field, you can set the maximum distance between the *Object* and the *Reference object* required to fulfill this condition. The distance is calculated as the Euclidean norm between the transform’s positions of Object and Reference Object.

- **Required seconds inside**

    In this field, you can set the time in seconds the `Object` should stay within the radius `Range` of the `Reference Object`.

------

## Interaction/Grab Object

### Description

This condition is fulfilled when the user grabs the `Object`. 
The condition is also fulfilled if the user already grabbed the Object before the step was activated, that is, if the user is already holding
the specified object.

### Configuration

- **Object**

    The `Process Scene Object` to grab. The object needs to have the `Grabbable Property` and a collider component configured. The collider defines the area where the user can grab this object.

------

## Interaction/Release Object

### Description

This condition is fulfilled when the `Object` is released by the user's controller. If the user is not already holding the specified object in hand while this condition is active, it is fulfilled immediately.

### Configuration

- **Object**

    The `Process Scene Object` to release. The object needs to have the `Grabbable Property` and a collider component configured. 

------

## Interaction/Snap Object

### Description

This condition is fulfilled when the `Object` is released into the `Zone to snap into`, which means the collider of the Object and collider of the Zone overlap. Adapt the collider size of the snap zone to increase or decrease the area where the user can release the `Object`. Increasing the collider size of the snap zone decreases the required *snap* precision and simplifies the user's interaction in VR. 
After the user releases the `Object`, this is moved to the snap zone's SnapPoint. To adjust this position, change the position of the SnapPoint child object of the `Zone to snap into` object.

#### Snap Zone Generator
For any snappable object you can generate a snap zone that can snap exactly this object and makes can be used as a `Zone to snap into`. To do so, navigate to the `Snappable Property` in Unity's Inspector and click on the button `Create Snap Zone`. 

![Snap Zone Generator](images/snapzonegenerator.png)

#### Manual Snap Zone Creation
Instead of the automatic generation as described above, you can do those steps also manually. Please refer to available documention on the `XRSocketInteractor` from Unity or related sources. You can also make changes to the automatically created snap zone to adapt it to your needs. Please note that these changes might impact the process logic. Do so on your own risk.

#### Feed Forward for Snap Zones

Snap zones are restricted to which objects can be snapped to them, which means any object can be valid (i.e. it can be snapped to this zone) or invalid (it can not be snapped to this zone) for a snap zone. In case you are moving a valid object into a zone (c.f. above, colliders and stuff), the snap zone color changes to ‘Validation Color’ (green), giving the trainee a positive feedback. In case you are moving an invalid object into a zone, the snap zone color changes to ‘Invalid Color’ (red), giving the trainee the feedback that this is the wrong object for this zone. 
Which colors and which materials are to be used can be changed in the Snap Zones parameters and settings.

#### Snap Zone Parameters and Settings
To change the highlight color or validation hover material of a dedicated snap zone, navigate to the snap zone object in the Unity inspector. In the script `Snap Zone` you will find these parameters among others. 

![Snap Zone Parameters](images/snapzoneparameters.png)

To change the colors and materials of all snap zones in the scene, select them in the VR Builder snap zone settings and press 'Apply settings in current scene'.

![Snap Zone Settings](images/snapzonesettings.png)]

The snap zone settings can be found in the project settings in tab `VR Builder > Settings > Snap Zones`.

### Configuration

- **Object**

    The `Process Scene Object` to place (snap). The object needs to have the `Snappable Property` and a collider component configured. 

- **Zone to snap into**

    This field contains the `Process Scene Object` where the `Object` is required to be snapped. Make sure you added the `Snap Zone Property` component to the snap zone game object in the Unity Inspector. Besides, the object must have a collider component with the `Is Trigger` property *enabled*.

------

## Interaction/Touch Object

### Description

This condition is fulfilled when the `Object` is touched by the user's controller.  If a trainee is already touching the specified object while this condition is active, it is fulfilled immediately.

### Configuration

- **Object**

    The `Process Scene Object` to be touched. The object needs to have the `Touchable Property` and a collider component configured. 

------

## Interaction/Use Object

### Description

This condition is fulfilled when the `Object` is used by pressing the *Use* button of the controller while being touched or grabbed.

### Configuration

- **Object**

    The `Process Scene Object` that is required to be used.The `Object` needs to have the `Usable Property` and a collider component configured.

------

## Utility/Timeout

### Description

This condition is fulfilled when the time specified in `Wait (in seconds)` has elapsed.

### Configuration

- **Wait (in seconds)**

    Set the time in seconds that should elapse before this condition is fulfilled.

------

## VR User/Teleport

### Description

This condition is fulfilled when the user teleports to the referenced `Teleportation Point`. Previous teleportation actions made into the `Teleportation Point` are not considered.

The provided `Teleportation Property` is based on the Unity XR Interaction Toolkit's `Teleportation Anchor`. For further reference, please check out the XR Interaction Toolkit  [documentation](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.0/api/UnityEngine.XR.Interaction.Toolkit.TeleportationProvider.html).

#### Configuring the Teleportation Point

The `Teleportation Property` can be set as a **Default Teleportation Anchor** by clicking on the `Set Default Teleportation Anchor` button. You can find it when selecting the `Teleportation Point` and viewing it in the Unity Inspector.

![Teleportation Property](images/teleportationproperty.PNG)

This will configure the attached `Teleportation Anchor`. It will provide a visual element in the Unity Editor that helps placing the `Teleportation Point` in the scene. This visual element will also be shown in the virtual world during training execution to guide the user.

### Configuration

- **Teleportation Point**

    The `Teleportation Property` is used as the location point for the trainee to teleport to.

## Online Documentation

We offer a constantly expanding list of [guides and tutorials](https://www.mindport.co/tutorials-unity-vr-development) on our website. Feel free to check them out to improve your VR Builder skills.

If this is your first time with VR Builder, you should start from the [Workflow Editor](https://www.mindport.co/vr-builder-learning-path/how-to-define-the-process-of-vr-applications-in-unity) and [Step Inspector](https://www.mindport.co/vr-builder-learning-path/how-to-define-steps-of-vr-applications-in-unity) tutorials, which explain the basics of working with VR Builder.

If that's your target, you might also want to check out the guides on how to build standalone VR Builder apps on the [Oculus Quest](https://www.mindport.co/vr-builder-learning-path/how-to-run-vr-builder-apps-on-oculus-quest-devices) or [Pico Neo 3](https://www.mindport.co/vr-builder-learning-path/how-to-run-vr-builder-apps-on-pico-neo-devices).

You can also check out some guides on the most advanced interactions, like the [series on snap zones](https://www.mindport.co/vr-builder-learning-path/pick-and-place-introduction-to-snap-zones).

Lastly, there are some [step-by-step tutorials](https://www.mindport.co/vr-builder-learning-path/how-to-create-a-vr-ball-game-with-track-and-measure-add-on-for-vr-builder) explaining how to work with our latest paid add-ons. Even if you don't intend to buy the relevant content, they can provide a good overview on how to build a functional process with VR Builder from scratch. 

## Acknowledgements

VR Builder is based on the open source edition of the [Innoactive Creator](https://www.innoactive.io/creator). While Innoactive helps enterprises to scale VR training, we adopted this tool to provide value for smaller content creators looking to streamline their processes. 

Like Innoactive, we believe in the value of open source and will continue to support this approach together with them and the open source community.

As VR Builder shares the same DNA as the Innoactive Creator, it can be useful check the [documentation for the Innoactive Creator](https://developers.innoactive.de/documentation/creator/v2.11.1/), the majority of which might be applicable to VR Builder as well.

## Contact

Join our official [Discord server](http://community.mindport.co) for quick support from the developer and fellow users. Suggest and vote on new ideas to influence the future of the VR Builder.

Make sure to review [VR Builder](https://assetstore.unity.com/packages/tools/visual-scripting/vr-builder-201913) if you like it. It will help us immensely.

If you have any issues, please contact [contact@mindport.co](mailto:contact@mindport.co). We'd love to get your feedback, both positive and constructive. By sharing your feedback you help us improve - thank you in advance!
Let's build something extraordinary!

You can also visit our website at [mindport.co](http://www.mindport.co).