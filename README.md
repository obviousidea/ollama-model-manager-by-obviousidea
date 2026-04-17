# Ollama Model Manager by ObviousIdea

A Windows desktop app to inspect and manage local Ollama models without going back to the terminal.

![Ollama Model Manager screenshot](docs/omm-screenshot.jpg)

## Why this app

Ollama Model Manager gives you a quick local view of your installed models, their size, freshness, and capabilities, with safe cleanup actions in a simple desktop UI.

## Features

- List installed local Ollama models
- Show model size and last update freshness
- Detect vision-capable models from Ollama capabilities
- Re-pull a model directly from the UI
- Delete a model with confirmation
- Filter the local library with search
- Follow the Windows light or dark theme automatically

## Download

Download the latest installer from the GitHub Releases page:

- [Latest release](https://github.com/obviousidea/ollama-model-manager-by-obviousidea/releases)

## Requirements

- Windows
- Ollama installed locally

## Tech stack

- WPF
- .NET 10
- Inno Setup for the installer

## Build

```powershell
dotnet build
```

## Publish standalone

```powershell
dotnet publish -c Release
```

## Installer

The repository includes an Inno Setup script for packaging:

```text
installer.iss
```
