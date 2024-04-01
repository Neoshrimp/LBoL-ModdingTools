# LBoL-ModdingTools
Modding tools for Lost Branch of Legend

#### [Sideloader template](https://github.com/Neoshrimp/LBoL-ModdingTools/tree/master/src/SideloaderTemplate)

#### [DebugMode](https://github.com/Neoshrimp/LBoL-ModdingTools/blob/master/src/DebugMode/DebugMode.dll)

### Alpha branch is available with code `THRDBG20220728`
It offers most up to date debug menus.

# Press F5 in main menu to start a debug run!
#### F4 while in battle to opens combat log.

![image](https://user-images.githubusercontent.com/89428565/236315259-4339e8f8-0ede-42b5-ab68-7dac289baeed.png)

#### [Quick Loc Reload](https://github.com/Neoshrimp/LBoL-ModdingTools/tree/master/src/QuickReloadLoc)

#### [Add watermark](https://github.com/Neoshrimp/LBoL-ModdingTools/tree/master/src/LBoL-AddWatermark)

---

[LBoL Plugin.zip](https://github.com/Neoshrimp/LBoL-ModdingTools/blob/master/LBoL%20Plugin.zip) is BepInEx plugin template for Visual Studio. Includes assembly publicizer. Adjust `<GameFolder>` property in .csproj file.

Visual Studio templates should probably be placed at `<User>/Documents/Visual Studio 20xx/Templates/ProjectTemplates`

# Tool superlist

Injectors:
- [BePinEx](https://github.com/BepInEx/BepInEx). Code injector and plugin loader providing a starting point for any mod.

Code patchers
- [Harmony](https://github.com/pardeike/Harmony). The essential modding library allowing to manipulate source code at runtime. While technically BePinEx is using custom fork of Harmony, [HarmonyX](https://github.com/BepInEx/HarmonyX/wiki), the regular Harmony [documentation](https://harmony.pardeike.net/articles/intro.html) still applies. It's both a great beginner's guide and explanation of more advanced features.

Development plugins:
- [Unity Explorer](https://github.com/sinai-dev/UnityExplorer). GUI for exploring scene hierarchy, inspecting game objects, C# REPL console and more. Very powerful.
- [BePinEx debug tools](https://github.com/BepInEx/BepInEx.Debug). Collection of dev tools. `DemystifyExceptions` and `Scriptengine` are particularly useful ones. Although, scriptengine requires a slight modification to work with LBoL 100% correctly
- [Sharplab](https://sharplab.io/). Not a plugin but a tool to check how C# transforms to Intermediary Language (advanced, don't worry about it if you're starting out).

Decompilation:
- [dnSpyEx](https://github.com/dnSpyEx/dnSpy). Probably the most important modding tool. Super convenient source code explorer and debugger.
- [Patched mono](https://github.com/Neoshrimp/dnSpy-Unity-mono-unity2021.xx/tree/unity2021/builds/Release/unity-2021.3.18f1/win64). Patched mono for LBoL for attaching dnSpyEx debugger. Enables many debugging features during runtime. Apparently, the exposed `127.0.0.1:55555` port can be used to connect other debuggers, such as Ryder, as well. [More info](https://github.com/dnSpy/dnSpy/wiki/Debugging-Unity-Games#debugging-release-builds) on that.
- [ILSpy](https://github.com/icsharpcode/ILSpy). The OG C# decompiler. Not as shiny as dnSpy but sometimes offers better decompiled code quality.

Asset extraction:
- [AssetRipper](https://github.com/AssetRipper/AssetRipper). Actively developed and probably one of the best asset extractors out there
- [AssetStudio](https://github.com/Perfare/AssetStudio). No longer maintained and not as reliable as AssetRipper but a nicer gui for quick for exploring/extraction.

Performance profilers:
- \<to be added\> Although the game isn't very performance critical so profiling your mod might be an overkill.
