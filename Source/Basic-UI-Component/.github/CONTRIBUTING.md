# How to contribute

At Innoactive we want to thank you for being part of this community and helping us to improve the Creator.

There are many ways to contribute, it could be by suggesting new features and ideas, reporting bugs, extending and modifying functionalities, and even from just using the Creator and developing amazing VR training applications.

In this guide, we summarize how you can do all those actions.

### Table of Contents:

1. [Getting Started](#getting-started)
1. [Code of Conduct](#code-of-conduct)
1. [Feature Request](#feature-request)
1. [Bug Report](#bug-report)
1. [Submitting Changes](#submitting-changes)
1. [Maintainers](#maintainers)
1. [Coding Conventions](#coding-conventions)
1. [Source Control Commit Guidelines](#source-control-commit-guidelines)
1. [Contributors](#contributors)

## Getting Started

The easiest and simplest way to get started and try out the Creator is by downloading the latest version from our [resources site](http://developers.innoactive.de/creator/).

Make sure to read the [documentation](http://developers.innoactive.de/documentation/creator/) for a deeper understanding of how the tool works.

## Code of Conduct

This project and everyone participating in it are governed by the [Innoactive Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to our [maintainers](#maintainers).

## Feature Request

We are keen to listen to your feature requests.

To request a feature, please go to the `Issues` section of this repository and create a new issue using the `Feature request` template. Be as detailed as possible, if possible, try to include references, and explain what is the benefit of the feature.

Make sure your idea is unique using the search functionality. If a similar feature request already exists, try providing additional information instead of creating a new issue. 

In the case that a similar feature request already exits but is closed without being resolved, you are welcome to create a new one if you think it makes sense.

## Bug Report

If you find an error in the source code, or experience one when using the Creator, you can help us by submitting a bug report issue to our GitHub issue tracker. 

Before submitting a new bug report, make sure there has not been reported already by using the search functionality in the issues section of this repository.

To report a new bug, please go to the `Issues` section of this repository and create a new issue using the `Bug report` template. Be as detailed as possible, explain how to reproduce it, if possible, try to include references and rate of reproducibility.

You can also [submit a Pull Request](#submitting-changes) with a fix!

Creating comprehensive bug reports helps the community to quickly understand the problem and the severity.

If you find a security vulnerability, do NOT open an issue. Contact a [maintainer](#maintainers) instead.

Alternatively, customers can contact us directly using our [support channel](https://jira.innoactive.de/servicedesk/customer/portal/3).

## Submitting Changes

By default, this repository is protected, the only way to submit changes is by merging via [creating a pull request from a fork](https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/creating-a-pull-request-from-a-fork), this is only intending to provide the best quality and add a security layer, we could detect and prevent new bugs, breaking functionalities and avoid violations to our [code convention](#coding-conventions).

Use the [pull request template](PULL_REQUEST_TEMPLATE.md) for detail as much as possible all the changes included in the pull request, this way it is easier for everyone to review.

Make sure to also follow the [Source Control Commit Guidelines](#source-control-commit-guidelines).

See more about [Pull Requests](https://help.github.com/en/github/collaborating-with-issues-and-pull-requests/about-pull-requests) and [how to fork a repository](https://help.github.com/en/github/getting-started-with-github/fork-a-repo).

## Maintainers

Meet our maintainers:

[<img alt="tomwim" src="https://github.com/tomwim.png" width="110">](https://github.com/tomwim) | [<img alt="SimonTheSourcerer" src="https://github.com/SimonTheSourcerer.png" width="110">](https://github.com/SimonTheSourcerer) | 
:---: | :---: |
[Thomas W.](mailto:thomas.wimmer@innoactive.de) | [Simon L](mailto:simon.lerch@innoactive.de)

Maintainers are responsible for this repository and its community.

## Coding Conventions

By encouraging coding conventions we ensure:

* The code to having a consistent look, so that readers can focus on content, not layout.
* Enabling readers to understand the code more quickly by making assumptions based on previous experience.
* Facilitating copying, changing, and maintaining the code.
* Sticking to C# best practices.

Please follow our [Coding Conventions](CODING_CONVENTIONS.md).

## Source Control Commit Guidelines

When committing to an Innoactive git project, please adhere to the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary). This leads to more readable messages that are easy to follow when looking through the project history. But also, we use the git commit messages to generate automated release notes.

### Commit Message Format
Every commit message consists of a **header** (mandatory), a **body**, and a **footer**.  
The header has a special format that includes a **type** and **subject**:

```
<type>: <subject>

[optional body]

[optional footer(s)]
```

Any line of the commit message cannot be longer than 100 characters! This allows the message to be easier
to read on GitHub as well as in various git tools.

The footer should contain a [closing reference to an issue](https://help.github.com/articles/closing-issues-via-commit-messages/) if any.


### Revert
If the commit reverts a previous commit, it should begin with `revert: `, followed by the header of the reverted commit. In the body it should say: `This reverts commit <hash>.`, where the hash is the SHA of the commit being reverted.

### Type
Must be one of the following:

* **build**: Changes that affect the build system or external dependencies (example scopes: gulp, broccoli, npm)
* **ci**: Changes to our CI configuration files and scripts (example scopes: Circle, BrowserStack, SauceLabs)
* **docs**: Documentation only changes
* **feat**: A new feature
* **fix**: A bug fix
* **perf**: A code change that improves performance
* **refactor**: A code change that neither fixes a bug nor adds a feature
* **style**: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
* **test**: Adding missing tests or correcting existing tests

### Subject
The subject contains a succinct description of the change:

* use the imperative, present tense: "change" not "changed" nor "changes"
* don't capitalize the first letter
* no dot (.) at the end

### Body
Just as in the **subject**, use the imperative, present tense: "change" not "changed" nor "changes".
The body should include the motivation for the change and contrast this with previous behavior.

### Footer
The footer should contain any information about **Breaking Changes** and is also the place to
reference GitHub issues that this commit **Closes**.

**Breaking Changes** should start with the word `BREAKING CHANGE:` with a space or two newlines. The rest of the commit message is then used for this.

### Examples:

#### Commit message with no body

```
docs: update changelog to beta.5
```

#### Commit message with both
```
fix: need to depend on latest rxjs and zone.js

The version in our package.json gets copied to the one we publish, and users need the latest of these.
```

#### Commit message with multi-paragraph body and multiple footers
```
fix: correct minor typos in code

see the issue for details

on typos fixed.

Reviewed-by: Z
Refs #133
```

See more [Conventional Commits Examples](https://www.conventionalcommits.org/en/v1.0.0/#examples)

## Contributors

Thank you all for your help, we appreciate all your [contributions](https://github.com/Innoactive/Creator/graphs/contributors).
