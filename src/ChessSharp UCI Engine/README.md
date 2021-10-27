# ChessSharp UCI Engine

[UCI](https://en.wikipedia.org/wiki/Universal_Chess_Interface) compatible executable.

## Publishing a single output exe for Windows

Publishing a _standalone exe_ file requires [specific build arguments](https://stackoverflow.com/a/65418065/8931881). Using the [dotnet CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/) which would look something like:

```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true
```

The equivalent [VSCode Task](https://code.visualstudio.com/docs/editor/tasks) task would look something like:

```
{
    "label": "publish",
    "command": "dotnet",
    "type": "process",
    "args": [
        "publish",
        "${workspaceFolder}/ChessSharp UCI Engine.csproj",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary",
        "-c",
        "Release",
        "--self-contained",
        "true",
        "-p:PublishSingleFile=true",
        "-r",
        "win-x64",
        "-p:IncludeAllContentForSelfExtract=true"
    ],
    "problemMatcher": "$msCompile"
}
```   

These arguments could also be supplied in the `.csproj`:

```
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <IncludeAllContentForSelfExtract>true</IncludeAllContentForSelfExtract>
```

See [this StackOverflow answer](https://stackoverflow.com/a/65418065/8931881) for more.