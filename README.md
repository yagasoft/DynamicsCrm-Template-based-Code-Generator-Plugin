# DynamicsCrm-Template-based-Code-Generator-Plugin

[![Join the chat at https://gitter.im/yagasoft/DynamicsCrm-TemplateBasedCodeGeneratorPlugin](https://badges.gitter.im/yagasoft/DynamicsCrm-TemplateBasedCodeGeneratorPlugin.svg)](https://gitter.im/yagasoft/DynamicsCrm-TemplateBasedCodeGeneratorPlugin?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### Version: 2.1.0.1
---

An XrmToolBox plugin that can be used to generate Early-bound code from a CRM Schema using a customisable T4 Template.

## Features

  + Preserved the original CrmSvcUtil structure and logic.
  + Customize the way the code is generated.
    + You get a default T4 template for the code that is generated, with a multitude more features than the official tool (features below).
    + You can rewrite the whole template if you wish for any possible requirements.
  + Replaced the SDK types with .NET types.
  + Generate only what's needed
    + Only choose the entities required.
    + Only the fields required.
  + Additional control
    + Option to use display names of entities and fields as variable names instead of logical names.
    + Override field names inside the tool's UI.
    + Ability to Lock variable names to avoid code errors on regeneration.
  + Support for strongly-typed alternate keys, for entities and Entity References.
  + Add annotations for model validation.
  + Generate metadata.
    + Field logical and schema names.
    + Localised labels.
  + Automatically limit attributes retrieved from CRM on any entity in a LINQ to the ones choosen (filtered) in the tool (check new entity constructors).
  + Many options to optimise generated code size even further.
  + Generate concrete classes for CRM Actions.
  + Support bulk relation loading.
    + Support filtering on relation loading.

## Usage

Install ([here](https://www.xrmtoolbox.com/plugins/plugininfo/?id=45abdb43-f0e5-ea11-bf21-281878877ebf)).

+ Load data
+ Choose entities
+ Optionally edit the default T4 Template
+ Generate
+ Save output CS code

A more sophisticated Visual Studio Extension can be found at [VS Marketplace](https://marketplace.visualstudio.com/items?itemName=Yagasoft.CrmCodeGenerator).

The engine for this Plugin and the VS Extension is the same. The settings and T4 Templates can be used for both (with minor modifications).

You can read a quick overview of the tool and its functionality [here](http://blog.yagasoft.com/2020/09/dynamics-template-based-code-generator-supercharged).

## Changes

#### _v2.1.0.1 (2020-09-28)_
+ Added: recent settings list (load history)
+ Added: reset option for the template text
+ Added: toast notification for clearer status
+ Improved: load and save logic
+ Fixed: fixed cancel button
+ Fixed: issues
#### _v2.0.0.1 (2020-09-26)_
+ Added: all missing features from VS extension (click on 'Quick Guide' for more info), except Contracts
+ Added: keep track of paths (settings, template, and code) used in previous sessions and the links between them
+ Fixed: layout issues
#### _v1.0.0.5 (2020-08-28)_
+ Fixed: assembly issues
#### _v1.0.0.3 (2020-08-25)_
+ Changed: updated package name
#### _v1.0.0.1 (2020-08-24)_
+ Initial release

---
**Copyright &copy; by Ahmed Elsawalhy ([Yagasoft](http://yagasoft.com))** -- _GPL v3 Licence_
