# VR Builder

https://github.com/MindPort-GmbH/VR-Builder/assets/247111/ca755abb-23fa-4742-a66c-2785bff4e80f

## Introduction

VR Builder helps you create interactive VR applications better and faster. By setting up a Unity scene for VR Builder, you will pair it with a VR Builder *process*. Through the VR Builder process, you can define a sequence of actions the user can take in the scene and the resulting consequences.

You can easily edit a process without coding through VR Builder's Workflow Editor. The Workflow Editor is a node editor where the user can arrange and connect the *steps* of the process. Each step is a different node and can include any number of *behaviors*, which make things happen in the scene. Likewise, a step will have at least one *transition* leading to another step. Every transition can list several *conditions* which have to be completed for the transition to trigger. For example, step B can be reached only after the user has grabbed the object specified in step A.

Behaviors and conditions are the "building blocks" of VR Builder. Several of them are provided in the free version already. Additional behaviors and conditions are available in our paid add-ons. Since VR Builder is open source, you can always write your own behaviors and conditions as well.

Behaviors and conditions can interact only with *process scene objects*. These are game objects in the scene which have a `Process Scene Object` component on them.

The interaction capabilities of a process scene object can be increased by adding *scene object properties* to it. For example, adding a `Grabbable Property` component to a game object will let VR Builder know that the object is grabbable, and when it is grabbed.

Normally it is not necessary to add properties manually to an object. When an object is dragged in the inspector of a condition or behavior, the user has the option to automatically configure it with a single click.

Where possible, properties try to add and configure required components by themselves. If you add a `Grabbable Property` to a game object, this will automatically be made grabbable in VR (it still needs to have a collider and a mesh, of course).

This makes it very easy to start from some generic assets and build a fully interactive scene.

## Requirements

VR Builder is supported on Unity 2021.3 or later.

VR Builder works out of the box with any headset compatible with Unity's XR Interaction Toolkit.

## Installation

The GitHub repository should be cloned in a Unity project's Assets folder. The recommended subfolder path is `Assets/MindPort/VR Builder/Core`. UnityPackages and Asset Store package will automatically place the files in the aforementioned subfolder.

After importing, please refer to the [user manual](/Documentation/vr-builder-manual.md#installation) for details on the VR Builder import process.

## Documentation

You can find comprehensive documentation in the [Documentation](/Documentation/vr-builder-manual.md) folder, or [online](documentation.mindport.co).

## Support Us

Our goal is to make VR Builder accessible for everyone - it is free and open source, and we want to keep things that way. To be able to maintain and extend it, we rely on your support!

If you wish to support us, you can buy VR Builder from the [Unity Asset Store](https://u3d.as/2F4c). Doing so will help us keep the lights on and ultimately deliver a better product.

While the Asset Store version is identical in content, it provides some added convenience as it will be listed with your other assets (and VR Builder add-ons). Plus you can sleep safe knowing that Unity has officially reviewed and approved the package!

We also sell a number of VR Builder add-ons on the Unity Asset Store. These expand the capabilities of VR Builder by providing more behaviors, conditions and general functionality. They work both with the Asset Store and the GitHub version of VR Builder, so make sure to check them out!

## Acknowledgements

VR Builder is based on the open source edition of the [Innoactive Creator](https://www.innoactive.io/creator). While Innoactive helps enterprises to scale VR training, we adopted this tool to provide value for content creators looking to streamline their VR development processes. 

Like Innoactive, we believe in the value of open source and will continue to support this approach together with them and the open source community. 
This means you are welcome to contribute to the [VR Builder GitHub repositories](https://github.com/MindPort-GmbH).

## Contact and Support

Join our official [Discord server](http://community.mindport.co) for quick support from the developer and fellow users. Suggest and vote on new ideas to influence the future of the VR Builder.

Make sure to review VR Builder on the [Unity Asset Store](https://assetstore.unity.com/packages/tools/visual-scripting/vr-builder-201913) if you like it. This will help us sustain the development of VR Builder.

If you have any issues, please contact [contact@mindport.co](mailto:contact@mindport.co). We'd love to get your feedback, both positive and constructive. By sharing your feedback you help us improve - thank you in advance!
Let's build something extraordinary!

You can also visit our website at [mindport.co](http://www.mindport.co).
