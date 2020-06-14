![Project Icon](icon.png) SimpleTasks
=====================================

[![NuGet](https://img.shields.io/nuget/v/SimpleTasks.svg)](https://www.nuget.org/packages/SimpleTasks/)

A simple task runner, e.g. for use in build scripts.
Supports task dependencies, running multiple tasks at once, and per-task command-line options.

Installation
------------

[Install from NuGet](https://nuget.org/packages/SimpleTasks)


Usage
-----

```cs
using static SimpleTasks.SimpleTask;

public static int Main(string[] args)
{
    CreateTask("build", "Build the project").Run((string version) => Console.WriteLine($"Building version {version}..."));
    CreateTask("test").DependsOn("build").Run(() => Console.WriteLine("Testing..."));
    CreateTask("package").Run(() => Console.WriteLine("Packaging..."));

    CreateTask("default").DependsOn("build", "package");

    return InvokeTask(args);
}
```

Then:

```
$ MyProject.exe --version 1.2.3
$ MyProject.exe build package --version 1.2.3
```

Define tasks using `SimpleTask.CreateTask`, passing a name and optional description.
Declare dependencies using `DependsOn`. 
Define a delegate which is invoked using `Run`.

If the user doesn't specify the name of a task to run, a task called `default` is run if specified.

Delegates passed to `Run` can take parameters: these are translated into command-line options.
Boolean don't take a value, all other types do.
Parameters can be of any type which `TypeDescriptor.GetConverter` supports.
Parameters are optional if:

 - They are a nullable value type, or
 - They have a default value, or
 - Their name ends in `Opt`

You can give options aliases and descriptions using the `Option` attribute:

```cs
private static void Build([Option("version|v", "Version number to use")] string version)
{
}

CreateTask("build").Run<string>(Build);
```

You must pass all options which are required by any of the tasks specified, and all of their dependencies.
If multiple tasks take an option with the same name, the single value specified is passed to all such tasks.

Advanced
--------

You can create explicit sets of tasks:

```cs
var set = new SimpleTaskSet();
set.Create("build").Run(...);
set.Invoke(args);
```

If you don't want it to print anything to `Console`, use `InvokeAdvanced` instead of `Invoke`.
This throws a subclass of `SimpleTaskException` if a problem is encountered.
