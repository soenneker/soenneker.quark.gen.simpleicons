[![](https://img.shields.io/nuget/v/soenneker.quark.gen.simpleicons.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.quark.gen.simpleicons/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.quark.gen.simpleicons/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.quark.gen.simpleicons/actions/workflows/publish-package.yml)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.quark.gen.simpleicons/build-and-test.yml?label=Build&style=for-the-badge)](https://github.com/soenneker/soenneker.quark.gen.simpleicons/actions/workflows/build-and-test.yml)
[![](https://img.shields.io/nuget/dt/soenneker.quark.gen.simpleicons.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.quark.gen.simpleicons/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.quark.gen.simpleicons/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.quark.gen.simpleicons/actions/workflows/codeql.yml)

# Soenneker.Quark.Gen.SimpleIcons

Build-time generation of a trimmed Simple Icons SVG provider for Quark and Razor projects.

## Install

```bash
dotnet add package Soenneker.Quark.Gen.SimpleIcons
dotnet add package Soenneker.SimpleIcons.Enums.Icons
dotnet add package Soenneker.SimpleIcons.Icons
```

The enum package supplies `SimpleIcon` values. The icons package supplies the SVG resources used during generation.

## Usage

Reference icons directly in C# or Razor so the build can discover them:

```razor
<SimpleIcon Name="Github" />
<SimpleIcon Name="@SimpleIcon.OpenAi" />
```

Register the generated provider with dependency injection:

```csharp
using Soenneker.Quark.Gen.SimpleIcons.Generated;

services.AddSimpleIconsAsScoped();
```

At build time, the package finds `SimpleIcon.Name` references and literal `<SimpleIcon Name="Name" />` values in `.cs` and `.razor` files. It embeds only those SVGs in the consuming assembly.

Icon names created only through reflection, concatenation, configuration, or other dynamic logic cannot be discovered. Add a direct `SimpleIcon.Name` reference for every icon that must be included.

## Build options

Disable generation for a project:

```xml
<PropertyGroup>
  <SimpleIconsGeneratorBuildEnabled>false</SimpleIconsGeneratorBuildEnabled>
</PropertyGroup>
```

Override the generated map path when needed:

```xml
<PropertyGroup>
  <SimpleIconsSvgMapOutput>$(IntermediateOutputPath)Generated\SimpleIconSvgMap.g.cs</SimpleIconsSvgMapOutput>
</PropertyGroup>
```

The generated map and provider are implementation details. Consume them through `ISimpleIconsSvgProvider` or Quark’s `SimpleIcon` component.
