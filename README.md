# DynamicsCrm-Template-based-Code-Generator-Plugin

[![Join the chat at https://gitter.im/yagasoft/DynamicsCrm-TemplateBasedCodeGeneratorPlugin](https://badges.gitter.im/yagasoft/DynamicsCrm-TemplateBasedCodeGeneratorPlugin.svg)](https://gitter.im/yagasoft/DynamicsCrm-TemplateBasedCodeGeneratorPlugin?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### Version: 1.0.0.5
---

An XrmToolBox plugin that can be used to generate Early-bound code from a CRM Schema using a customisable T4 Template.

## Usage

+ Load data
+ Choose entities
+ Optionally edit the default T4 Template
+ Generate
+ Save output CS code

A more sophisticated Visual Studio Extension can be found at [VS Extension](https://marketplace.visualstudio.com/items?itemName=Yagasoft.CrmCodeGenerator).

The engine for this Plugin and the VS Extension is the same. The settings and T4 Templates can be used for both; however, the T4 Template from the Extension uses C# v6, which is not supported by this Plugin.

## Credits

  + VS Extension base code:
	+ Eric Labashosky
	+ https://github.com/xairrick/CrmCodeGenerator
  + My work:
	+ Completely reworked the screens
	+ Greatly enhanced the generation and regeneration speed
	+ Added the features that don't exist in the official tool

## Changes

#### _v1.0.0.5 (2020-08-28)_
+ Fixed: assembly issues
#### _v1.0.0.3 (2020-08-25)_
+ Changed: updated package name
#### _v1.0.0.1 (2020-08-24)_
+ Initial release

---
**Copyright &copy; by Ahmed Elsawalhy ([Yagasoft](http://yagasoft.com))** -- _GPL v3 Licence_
