# Ollama Model Manager by ObviousIdea

A lightweight Windows desktop app to inspect and manage local Ollama models without using the terminal.

## Features

- List installed Ollama models
- Show size and freshness
- Detect vision-capable models from Ollama capabilities
- Re-pull a model
- Delete a model with confirmation
- Light and dark theme support based on Windows

## Stack

- WPF
- .NET 10

## Requirements

- Windows
- Ollama installed locally

## Build

```powershell
dotnet build
```

## Publish standalone

```powershell
dotnet publish -c Release
```

## Installer

The project includes an Inno Setup script:

```text
installer.iss
```
