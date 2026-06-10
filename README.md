<h1 align="center">VR Builder</h1>

<p align="center">
    <img src="https://github.com/user-attachments/assets/c8d8ae7e-8369-4d00-9ab1-16159dd1bd6c" alt="vr builder logo" height="200px"/>
    <br>
    <em><span><b>VR</b> <b>Builder</b> is an open source toolkit for creating <b>VR</b> applications without prior VR development experience.
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

VR Builder helps you create interactive VR applications better and faster.
It combines a graphical workflow editor with ready-to-use behaviors, conditions and scene setup tools.
Typical use cases are VR training, guided simulations, product demonstrations and interactive walkthroughs.

A VR Builder application is driven by a *process*. A process is a sequence of *steps* connected by *transitions*.
Steps can run *behaviors*, such as moving an object or playing audio.
Transitions wait for *conditions*, such as the user grabbing an object, before the process continues.

Processes are edited in VR Builder's Workflow Editor, a node editor built for authoring and maintaining these flows.
Behaviors and conditions are the main building blocks.
Several of them are provided in this vr builder core version already.
Additional behaviors and conditions are available in our [pro package](https://www.mindport.co/vr-builder/get-vr-builder) or [paid partner add-ons](https://www.mindport.co/vr-builder#partner-add-ons).
**Since VR Builder is open source, you can always write your own behaviors and conditions as well.**

Behaviors and conditions can interact only with *process scene objects*.
These are Unity game objects with a `Process Scene Object` component.
Their capabilities are defined by *scene object properties*.
For example, adding a `Grabbable Property` lets VR Builder know that the object can be grabbed and when that happens (it still needs to have a collider and a mesh, of course).

In most cases, properties do not need to be added manually.
When an object is assigned to a behavior or condition, VR Builder can configure the required components with a single click.

This makes it very easy to start from some generic assets and build a fully interactive scene.

## Requirements

VR Builder 5 is currently supported on Unity 6 or later. It works with any headset that is compatible with Unity's XR Interaction Toolkit, as well as desktop and web-based platforms.

## Installation
<a href="https://openupm.com/packages/co.mindport.vrbuilder.core/"><img alt="OpenUPM Badge" src="https://img.shields.io/npm/v/co.mindport.vrbuilder.core?label=openupm&amp;registry_uri=https://package.openupm.com"/></a>
<a href="https://github.com/MindPort-GmbH/VR-Builder/releases" target="_blank"><img alt="Static download badge" src="https://img.shields.io/github/downloads/MindPort-GmbH/VR-Builder/total.svg"></a>
<br><br>

Install VR Builder from [OpenUPM](https://openupm.com/packages/co.mindport.vrbuilder.core/), the [Unity Asset Store](https://u3d.as/3pUD), or download the latest Unity package from [Releases](https://github.com/MindPort-GmbH/VR-Builder/releases).
(If you download a `.unitypackage`, import it by double-clicking the file or dragging it into the Unity Project window, see [Unity Docs](https://docs.unity3d.com/6000.3/Documentation/Manual/AssetPackagesImport.html) for more information.)

**Importing will take some time as VR Builder also imports the necessary dependencies.**
Once the import is complete, the Project Setup Wizard opens and guides you through the basic project setup.

Quick start:

1. Install VR Builder in a Unity project that matches the requirements above.
2. Let the Project Setup Wizard configure the required project settings.
3. (Optional) Import `Demo - Scene` from the Scene setup wizard.
4. Click Finish to close the Project Setup Wizard. If 'Demo - Scene' is checked, the scene will load and automatically open the `Process Editor` view.

Included samples:

- `Demo - Core Features`: shows the main free behaviors and conditions in a working process with (3D) assets.
- `Demo - Hands Interaction`: shows hand tracking with VR Builder and the XR Interaction Toolkit. Importing this sample also installs its required Unity packages and XRI samples.

If you work from the Git repository instead of a packaged release, clone the repository with its submodules:

```bash
git clone --recurse-submodules https://github.com/MindPort-GmbH/VR-Builder.git
```

If the repository was already cloned, initialize the submodules afterwards:

```bash
git submodule update --init --recursive
```

VR Builder depends on the `Source/CoreRuntime` submodule.
VR Builder CoreRuntime contains the runtime process architecture used by VR Builder: processes, steps, transitions, behaviors, conditions, scene object references and related runtime services.
This repository adds the Unity package around it, including the editor, setup workflow, samples and XR Interaction Toolkit integration.
A source checkout without the submodule is incomplete and will not compile.

If Unity reports missing `VRBuilder.Core` types after opening a source checkout, check that `Source/CoreRuntime` is populated and run the submodule command again.
Packaged installs from OpenUPM, the Asset Store or GitHub Releases do not require manual submodule setup.

## Documentation

You can find the manual in [Documentation~](/Documentation~/VR-Builder-Manual.pdf), or read the documentation [online](http://documentation.mindport.co).

Useful resources:

- [VR Builder setup](https://www.mindport.co/vr-builder-tutorials/vr-builder-setup)
- [Process Editor tutorial](https://www.mindport.co/vr-builder-tutorials/process-editor)
- [VR Builder tutorials](https://www.mindport.co/vr-builder/tutorials)

## Support Us
<a href="https://u3d.as/3pUD" target="_blank"><img alt="Static Badge" src="https://img.shields.io/badge/Unity Asset Store-v5-Blue?logo=unity"></a><br><br>

VR Builder Core is free and open source and our goal is to make VR Builder accessible for everyone.
Buying it from the [Unity Asset Store](https://u3d.as/3pUD) or [over our website](https://www.mindport.co/vr-builder/get-vr-builder) supports us on ongoing maintenance and development.

The Asset Store version contains all the content from the VR Builder Core (this repository), as well as the [Pro features](https://mindport-gmbh.github.io/VR-Builder-Documentation/articles/pro/introduction.html)
that are not publicly available or included in VR Builder Core such as guidance features, feedback, reports and data, as well as randomisation capabilities.
**Plus you can sleep safe knowing that Unity has officially reviewed and approved the package!**

**Make sure to review VR Builder on the [Unity Asset Store](https://u3d.as/3pUD#reviews) if you like it!**

## Acknowledgements

VR Builder is based on the open source edition of the Innoactive Creator, which is now discontinued. We adopted it to provide value for creators who want to streamline VR development.

Like Innoactive, we believe in open source. Contributions to the [VR Builder GitHub repositories](https://github.com/MindPort-GmbH) are welcome.

## Contact and Support
<a href="https://discord.com/invite/aUdwRRPgrK" target="_blank"><img src="https://img.shields.io/discord/861482616539578378" alt="Discord conversation"></a><br><br>

Join our official [Discord server](https://discord.com/invite/aUdwRRPgrK) for quick support from the developers and community. Suggest and vote on new ideas to influence the future of the VR Builder.

For other questions, contact [contact@mindport.co](mailto:contact@mindport.co).
Let's build something extraordinary!

You can also visit our website at [MindPort.co](https://www.mindport.co/).
