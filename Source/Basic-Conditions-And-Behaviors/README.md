# Basic Behaviors, Conditions and Properties 

This [Innoactive Creator](https://github.com/Innoactive/Creator) component contains a set of basic behaviors, conditions and properties which will find a use in almost every training course.

### Included Behaviors

- Behavior Sequence
- Delay Behavior
- Enable GameObject Behavior
- Disable GameObject Behavior
- Highlight Object Behavior
- Lock Object Behavior
- Unlock Object Behavior
- Move Object Behavior
- Play Audio Behavior

### Included Conditions

- Timeout Condition
- Object In Collider Condition
- Object In Range Condition
- Object In Target Condition

## Getting Started

Refer to our [Getting Started](http://developers.innoactive.de/documentation/creator/latest/articles/getting-started/index.html) guide.

As a developer, you might want to [submodule](https://git-scm.com/book/en/v2/Git-Tools-Submodules) our repositories instead of importing Unity packages. Given that you have set up a repository for your project and have launched Git BASH inside the `Assets` folder of it, execute the following commands:

### [Creator Core](https://github.com/Innoactive/Creator)

Every project in this list depends on the Creator Core. It contains fundamental definitions, generally applicable logic, and the visual editor.

```
git submodule add git@github.com:Innoactive/Creator.git Innoactive/Creator/Core
```

### [Basic Conditions and Behaviors Component](https://github.com/Innoactive/Basic-Conditions-And-Behaviors)

*You are here.*

This component contains very simple conditions and behaviors that involve no interaction with trainees. For example, a condition on a timer, or a behavior that moves its target object. 

```
git submodule add git@github.com:Innoactive/Basic-Conditions-And-Behaviors.git Innoactive/Creator/Components/Basic-Conditions-And-Behaviors-Component
```

### [Basic Interaction Component](https://github.com/Innoactive/Basic-Interaction-Component)

This component is an abstraction layer between the Creator Core and a component that would implement user interactions.

```
git submodule add git@github.com:Innoactive/Basic-Interaction-Component.git Innoactive/Creator/Components/Basic-Interaction 
```

### [Unity XR Interaction Component](https://github.com/Innoactive/XR-Interaction-Component)

This component implements user interactions in VR by using the Unity XR framework. Include the Basic Interaction component along this repository to your project to let designers create training applications for VR.

```
git submodule add git@github.com:Innoactive/XR-Interaction-Component.git Innoactive/Creator/Components/XR-Interaction-Component
```

### [Text-To-Speech Component](https://github.com/Innoactive/TextToSpeech-Component)

This component uses text-to-speech engines so designers could generate audio instructions for trainees.

```
git submodule add git@github.com:Innoactive/TextToSpeech-Component.git Innoactive/Creator/Components/TextToSpeech-Component
```

### [Base Template](https://github.com/Innoactive/IA-Training-Template)

This template makes an initial setup of the Creator, and serves both as example and as a starting point for creating new templates. Every VR training application project includes one template.

## [Examples](https://github.com/Innoactive/XR-Creator-Examples)

This repository includes everything above, a configured template, and examples for training designers. Execute this command inside the folder you want to have the Unity project:

```
git clone --recurse-submodules -j8 git@github.com:Innoactive/XR-Creator-Examples.git
```

## Documentation

Start with [this page](http://developers.innoactive.de/documentation/creator/latest/articles/getting-started/index.html) and then proceed with our [developer's guide](http://developers.innoactive.de/documentation/creator/latest/articles/developer/index.html). This way you will get familiar with our tool and will know how to configure and extend it.

## Contributing

See our [contributor's guide](.github/CONTRIBUTING.md).

## Maintainers

You can find contacts of current maintainers in the [Maintainers](.github/CONTRIBUTING.md#maintainers) section of our contributing guidelines.

## Community

Join our [spectrum community](https://spectrum.chat/innoactive-creator?tab=posts)!

## License

This repository is licensed under the Apache License, Version 2.0. See the [LICENSE](LICENSE) file for the full text.

## Acknowledgements

We have referenced every 3rd party work we use in this repository in the [NOTICE](NOTICE) file.

We list all contributors to this repository in the [Contributors](.github/CONTRIBUTING.md#contributors) section of our contributing guidelines.
