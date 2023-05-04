## Template for [Sideloader](https://github.com/Neoshrimp/LBoL-Entity-Sideloader/tree/master) mod.

Use is highly encouraged as it has dependencies, publicizer and common boilerplate setup.


Instructions:
- Copy `LBoL Sideloader Template.zip` to `<User>\Documents\Visual Studio 2022\Templates\ProjectTemplates` (no need to extract).

- Create a new project, search for LBoL Sideloader template.

- Change _GameFolder_ in .csproj file to target the game installation folder.

For first time setup:
- Download [Sideloader.dll](https://github.com/Neoshrimp/LBoL-Entity-Sideloader/blob/master/src/LBoL-Entity-Sideloader/LBoL-Entity-Sideloader.dll) and put it in `BepInEx/plugins` folder. It will be used as reference in the project.
- Download [scriptengine.dll](https://github.com/Neoshrimp/BepInEx.Debug/blob/master/src/ScriptEngine/ScriptEngine.dll), put in plugins folder. Create `BepInEx/plugins/scripts` directory. Technically, this is optional but workflow without scriptengine is 10 times slower.

Change post-build command to copy to plugins folder instead of scripts if ScriptEngine is not used.

`https://nuget.bepinex.dev/v3/index.json` might need to be added as a source for nuget manager for BepInEx packages to be installed correctly.
