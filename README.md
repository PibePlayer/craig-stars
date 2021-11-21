# craig-stars

![screenshot](docs/screenshots/screenshot3.png)

A clone of the 4X game Stars!

[![Build and Release Latest](https://github.com/sirgwain/craig-stars/actions/workflows/build-and-release-latest.yaml/badge.svg)](https://github.com/sirgwain/craig-stars/actions/workflows/build-and-release-latest.yaml)

# Download

Download the latest release from [release](https://github.com/sirgwain/craig-stars/releases) from github.

### Mac OS Image

**Note**: The macOS dmg file is not signed and will report that it is damaged if you download it and run it. To fix this, open up a terminal and execute `xattr -r -d com.apple.quarantine ~/Downloads/path-to-craig-stars-image.dmg`. This will hopefully be fixed by a future release of
godot ([issue #51550](https://github.com/godotengine/godot/pull/51550)).

# Development

This clone is done using the [Godot](https://godotengine.org) game engine. To launch this project, install Godot Mono 3.3 or greater (and [the required .net sdks](https://docs.godotengine.org/en/latest/tutorials/scripting/c_sharp/c_sharp_basics.html#setting-up-c-for-godot)), open project.godot in the editor and click the Play button.

To run unit Core.Tests, install [.NET Core 5](https://dotnet.microsoft.com/download).

## VS Code Extensions:

-   [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
-   [Mono Debug](https://marketplace.visualstudio.com/items?itemName=ms-vscode.mono-debug)
-   [C# Tools for Godot](https://marketplace.visualstudio.com/items?itemName=neikeq.godot-csharp-vscode)
-   [godot-tools](https://marketplace.visualstudio.com/items?itemName=geequlim.godot-tools)
-   [Test Explorer UI](https://marketplace.visualstudio.com/items?itemName=hbenl.vscode-test-explorer)
-   [.Net Core Test Explorer](https://marketplace.visualstudio.com/items?itemName=derivitec-ltd.vscode-dotnet-adapter)
-   [Prettier - Code formatter](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode)
-   [C# Xml Documentation Comments](https://marketplace.visualstudio.com/items?itemName=k--kato.docomment)
-   C# FixFormat Fixed

_... More to come on setting up a dev environment_

# Credits

-   The artwork comes from ForceUser on the [Home World Forum](https://starsautohost.org/sahforum2/index.php?t=index&rid=479)
-   The calculations for various formulas come from [The Stars! FAQ](http://starsfaq.com), FreeStars, Nova, and the Home World Forum.
-   Help setting up a multi-module c#+godot project came from [van800](https://github.com/van800/godot-demo-projects/tree/nunit/mono). Thanks!
-   Wormhole urchin graphics and portal icon are from [Hansjörg Malthaner](http://opengameart.org/users/varkalandar)
-   Mineral Packet graphics (and probably more to come) are from [Kenney](https://www.kenney.nl/)

# License

The source code is licensed under the permissive MIT license. Feel free to copy and use it for whatever. The artwork and assets were created by a different team and licensed under GPL.
