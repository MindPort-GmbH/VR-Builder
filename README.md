<h1 align="center">VR Builder</h1>

<p align="center">
    <img src="https://github.com/user-attachments/assets/c8d8ae7e-8369-4d00-9ab1-16159dd1bd6c" alt="vr builder logo" height="200px"/>
    <br>
    <em><span><b>VR</b> <b>Builder</b> is an open source toolkit that lets you create <b>VR</b> applications without prior experience.
    <br>
    It supports all major <b>VR</b> headsets, has a graphical workflow editor, and offers various integrations.</span></em>
    <br>
</p>

<p align="center">
    <a href="https://www.mindport.co/vr-builder"><strong>VR Builder</strong></a>
    <br>
</p>

<p align="center">
    <a href="https://github.com/MindPort-GmbH/VR-Builder/issues?q=">Submit an Issue</a>
    ·
    <a href="https://www.mindport.co/blog">Blog</a>
    <br>
    <br>
</p>

<p align="center">
    <a href="https://openupm.com/packages/co.mindport.vrbuilder.core/" title="Download VR Builder over GitHub OpenUPM" target="_blank"><img alt="OpenUPM Badge" src="https://img.shields.io/npm/v/co.mindport.vrbuilder.core?label=openupm&amp;registry_uri=https://package.openupm.com"/></a>
    <a href="https://github.com/MindPort-GmbH/VR-Builder/releases" title="Download VR Builder over GitHub" target="_blank"><img alt="Static download badge" src="https://img.shields.io/github/downloads/MindPort-GmbH/VR-Builder/total.svg"></a>
    <a href="https://github.com/MindPort-GmbH/VR-Builder/issues?q=is%3Aopen" title="Show issues for VR Builder" target="_blank"><img alt="GitHub Issues or Pull Requests" src="https://img.shields.io/github/issues/mindport-gmbh/vr-builder?style=flat&label=open%20issues&color=232EA043"></a>
    <a href="https://www.codefactor.io/repository/github/mindport-gmbh/vr-builder" title="Code quality of VR Builder" target="_blank"><img alt="CodeFactor badge" src="https://www.codefactor.io/repository/github/mindport-gmbh/vr-builder/badge"></a>
    <a href="https://discord.com/invite/aUdwRRPgrK" title="Join the community of VR Builder" target="_blank"><img alt="Discord conversation badge" src="https://img.shields.io/discord/861482616539578378"></a>
</p>


## Introduction

<video src="https://github.com/MindPort-GmbH/VR-Builder/assets/247111/ca755abb-23fa-4742-a66c-2785bff4e80f" width="300"></video>

VR Builder helps you create interactive VR applications better and faster. By setting up a Unity scene for VR Builder, you will pair it with a VR Builder *process*. Through the VR Builder process, you can define a sequence of actions the user can take in the scene and the resulting consequences.

You can easily edit a process without coding through VR Builder's Workflow Editor. The Workflow Editor is a node editor where the user can arrange and connect the *steps* of the process. Each step is a different node and can include any number of *behaviors*, which make things happen in the scene. Likewise, a step will have at least one *transition* leading to another step. Every transition can list several *conditions* which have to be completed for the transition to trigger. For example, step B can be reached only after the user has grabbed the object specified in step A.

Behaviors and conditions are the "building blocks" of VR Builder. Several of them are provided in the free version already. Additional behaviors and conditions are available in our paid add-ons. Since VR Builder is open source, you can always write your own behaviors and conditions as well.

Behaviors and conditions can interact only with *process scene objects*. These are game objects in the scene which have a `Process Scene Object` component on them.

The interaction capabilities of a process scene object can be increased by adding *scene object properties* to it. For example, adding a `Grabbable Property` component to a game object will let VR Builder know that the object is grabbable, and when it is grabbed.

Normally it is not necessary to add properties manually to an object. When an object is dragged in the inspector of a condition or behavior, the user has the option to automatically configure it with a single click.

Where possible, properties try to add and configure required components by themselves. If you add a `Grabbable Property` to a game object, this will automatically be made grabbable in VR (it still needs to have a collider and a mesh, of course).

This makes it very easy to start from some generic assets and build a fully interactive scene.

## Requirements

VR Builder is currently supported on Unity 6 or later. The default interaction system is Unity XR Interaction Toolkit 3 or later. If you intend to use a older Unity or XRI version, you can do so by using version 4.x, which is optimized for Unity 2021/2022 and XRI 2.

VR Builder works out of the box with any headset compatible with Unity's XR Interaction Toolkit.

## Installation
<a href="https://openupm.com/packages/co.mindport.vrbuilder.core/"><img alt="OpenUPM Badge" src="https://img.shields.io/npm/v/co.mindport.vrbuilder.core?label=openupm&amp;registry_uri=https://package.openupm.com"/></a>
<a href="https://github.com/MindPort-GmbH/VR-Builder/releases" target="_blank"><img alt="Static download badge" src="https://img.shields.io/github/downloads/MindPort-GmbH/VR-Builder/total.svg"></a>
<br><br>

Download the latest Unity package from [Releases](https://github.com/MindPort-GmbH/VR-Builder/releases). You can import the package in your Unity project by double clicking on it or dragging it in your Assets window.

Importing will take some time as VR Builder also imports the necessary dependencies. Once the process is completed, the Project Setup Wizard should appear, letting you configure some basic settings before opening the demo scene or starting your own project.

## Documentation

You can find comprehensive documentation in the [Documentation](/Documentation/vr-builder-manual.pdf) folder, or [online](documentation.mindport.co).

## Support Us
<a href="https://u3d.as/3pUD" target="_blank"><img alt="Static Badge" src="https://img.shields.io/badge/Unity Asset Store-v5-Blue?logo=unity"></a><br><br>

Our goal is to make VR Builder accessible for everyone - it is free and open source, and we want to keep things that way. To be able to maintain and extend it, we rely on your support!

If you wish to support us, you can buy VR Builder from the [Unity Asset Store](https://u3d.as/3pUD). Doing so will help us keep the lights on and ultimately deliver a better product.

While the Asset Store version is identical in content, it provides some added convenience as it will be listed with your other assets (and VR Builder add-ons). Plus you can sleep safe knowing that Unity has officially reviewed and approved the package!

We also sell more VR Builder features over VR Builder Pro on the Unity Asset Store. These expand the capabilities of VR Builder by providing more behaviors, conditions and general functionality. They work both with the Asset Store and the GitHub version of VR Builder, so make sure to check them out!

## Acknowledgements

VR Builder is based on the open source edition of the Innoactive Creator (now discontinued). While Innoactive helps enterprises to scale VR training, we adopted this tool to provide value for content creators looking to streamline their VR development processes. 

Like Innoactive, we believe in the value of open source and will continue to support this approach together with them and the open source community. 
This means you are welcome to contribute to the [VR Builder GitHub repositories](https://github.com/MindPort-GmbH).

## Contact and Support
<a href="https://discord.com/invite/aUdwRRPgrK" target="_blank"><img src="https://img.shields.io/discord/861482616539578378" alt="Discord conversation"></a><br><br>

Join our official [Discord server](https://discord.com/invite/aUdwRRPgrK) for quick support from the developer and fellow users. Suggest and vote on new ideas to influence the future of the VR Builder.

Make sure to review VR Builder on the [Unity Asset Store](https://u3d.as/3pUD) if you like it. This will help us sustain the development of VR Builder.

If you have any issues, please contact [contact@mindport.co](mailto:contact@mindport.co). We'd love to get your feedback, both positive and constructive. By sharing your feedback you help us improve - thank you in advance!
Let's build something extraordinary!

You can also visit our website at [MindPort.co](https://www.mindport.co/).
