[![](https://img.shields.io/nuget/v/soenneker.quark.gen.simpleicons.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.quark.gen.simpleicons/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.quark.gen.simpleicons/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.quark.gen.simpleicons/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.quark.gen.simpleicons.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.quark.gen.simpleicons/)

# Soenneker.Quark.Gen.SimpleIcons

Defines the simple icons write runner contract.

## Install

```bash
dotnet add package Soenneker.Quark.Gen.SimpleIcons
```

## Quick start

```csharp
using Soenneker.Quark.Gen.SimpleIcons.BuildTasks.Abstract;

ISimpleIconsWriteRunner simpleIconsWriteRunner = /* resolve from DI */;
var result = await simpleIconsWriteRunner.Run("value", default);
```

Runs simple Icons Write Runner for the Simple Icons Write Runner.

## What you get

- `ISimpleIconsWriteRunner` — Defines the simple icons write runner contract.
- `Startup` — Represents the startup.
- `BuildTasksCommandLineArgs` — Represents the build tasks command line args.
- `ConsoleHostedService` — Represents the console hosted service.
- `Program` — Represents the program.
- `SimpleIconsGenerator` — Represents the simple icons generator.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `BuildTasksCommandLineArgs.Args` | Gets args. | Gets args. |
| `ConsoleHostedService.StartAsync(cancellationToken)` | Starts the Console Hosted Service and begins its background work. | A task that completes after the Console Hosted Service has started. |
| `ConsoleHostedService.StopAsync(cancellationToken)` | Stops the Console Hosted Service and waits for its background work to finish. | A task that completes after the Console Hosted Service has stopped. |
| `Program.Main(args)` | Runs the application using the supplied command-line arguments. | A task that completes when the application exits. |
| `SimpleIconsGenerator.Initialize(context)` | Initializes the Simple Icons Generator so it is ready for use. | Returns no value; the requested change is complete when the method returns. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
