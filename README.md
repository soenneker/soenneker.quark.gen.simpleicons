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

The enum package supplies `SimpleIconEnum` values. The icons package supplies the SVG resources used during generation.

## Usage

Use literal Razor icon names so the build can discover them:

```razor
@using Soenneker.Quark
@using static Soenneker.SimpleIcons.Enums.Icons.SimpleIconEnum

<SimpleIcon Name="Github" />
<SimpleIcon Name="React" aria-label="React" />
```

The static import brings enum members such as `Github` into scope for the component's enum parameter, and the generator reads the literal names directly. Keep this import in the page that uses it. No enum alias is needed.

Register the generated provider with dependency injection:

```csharp
using Soenneker.Quark.Gen.SimpleIcons.Generated;

services.AddSimpleIconsAsScoped();
```

Reference the generator directly in the application project and build once to produce this namespace and registration extension. Quark's own private build dependencies do not configure the consuming application's provider. The [Quark installation guide](https://quark.soenneker.com/installation) covers the rest of the component setup.

At build time, the package scans `.cs` and `.razor` files and embeds only the discovered SVGs in the consuming assembly. Its scanner recognizes literal `<SimpleIcon Name="Name" />` values and C# references spelled `SimpleIcon.Name`.

The public enum is currently named `SimpleIconEnum`; do not assume arbitrary enum expressions or aliases are discovered by that textual scanner. Use literal Razor `Name` values for reliable discovery. If runtime logic selects an icon, ensure each permitted icon also appears as a literal `Name` in source. Names created only through reflection, concatenation, or configuration cannot add SVGs to the build.

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

`SimpleIconsIncludeSvgProvider` and `SimpleIconsIncludeServiceCollectionExtensions` default to `true`. Library authors can disable these outputs when the final application supplies the provider and registration. Leave them enabled for a normal app.

If an icon is missing, check its source-visible name, the SVG catalog reference, generated files under `obj/.../Generated`, and the DI registration. See the [generator guide](https://quark.soenneker.com/generators) for how brand icons, Lucide, CSS, and themes fit together.
