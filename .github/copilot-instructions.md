# GitHub Copilot Instructions for More Autosave Slots (Continued) RimWorld Mod

## Overview and Purpose
More Autosave Slots (Continued) is a RimWorld mod that provides additional options for periodic saving. It allows players to increase or decrease the number of autosave files, use a more descriptive naming scheme, and control the frequency of autosaves. The mod prevents accidental overwriting of autosaves and provides more control over file management.

## Key Features and Systems
More Autosave Slots (Continued) includes the following key features and systems:

- Autosave slots: Players can increase or decrease the number of autosave files used by the mod.
- Descriptive naming scheme: The mod uses a more descriptive naming scheme than "Autosave-X" for autosave files.
- Manual save naming: Players can use the autosave naming scheme for manual saving.
- Autosave frequency: Players can control the frequency of autosaves.

## Coding Patterns and Conventions
The More Autosave Slots (Continued) mod follows the following coding patterns and conventions:

- C# language: The mod is written in C#.
- Harmony patching: The mod uses Harmony patching to modify RimWorld's code.
- XML integration: The mod uses XML files for configuration and metadata.

## XML Integration
The mod includes the following XML files:

- **About.xml**: Contains information about the mod, including its dependencies.
- **Manifest.xml**: Contains information about the mod, including its package ID and version.

## Harmony Patching
The mod uses Harmony patching to modify RimWorld's code. The following Harmony patches are used:

- **Autosaver_AutosaverTick.cs**: Modifies the `Autosaver.AutosaverTick` method to control the frequency of autosaves.
- **Autosaver_NewAutosaveFileName.cs**: Modifies the `Autosaver.NewAutosaveFileName` method to use a more descriptive naming scheme.
- **GenText_IsValidFilename.cs**: Modifies the `GenText.IsValidFilename` method to allow the use of the autosave naming scheme.
- **Window_PostOpen.cs**: Modifies the `Window.PostOpen` method to provide options for manual saving.

## Suggestions for Copilot
When using Copilot to generate code for the More Autosave Slots (Continued) mod, consider the following suggestions:

- Ensure that the generated code follows the mod's coding patterns and conventions.
- Use Harmony patching to modify RimWorld's code.
- Integrate the generated code with the mod's XML files for configuration and metadata.
- Test the generated code thoroughly to ensure that it works as intended.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

