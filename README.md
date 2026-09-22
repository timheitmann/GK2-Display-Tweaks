# GK2 Ultrawide Fix

A small patcher that enables ultrawide resolutions in Graveyard Keeper 2.

Graveyard Keeper 2 already contains configuration for resolutions such as 5120×1440, but resolutions with an aspect ratio wider than 2:1 are filtered out by the game.

This patch removes that restriction and allows the game to expose supported ultrawide resolutions in its display settings.

## Tested Resolution

The patch has currently been tested with:

* 5120×1440
* 32:9 aspect ratio
* Linux using Steam/Proton

Windows support is included in the patcher, but should initially be considered less tested.

## How It Works

Graveyard Keeper 2 contains a method named:

`ResolutionConfig.IsUltraWide()`

The game uses this method while building the list of available display resolutions. Resolutions wider than a 2:1 aspect ratio are normally rejected.

The patcher changes this method so that it no longer identifies resolutions as unsupported ultrawide resolutions.

The game already contains a native configuration for 5120×1440, so no custom resolution is added by the patcher.

The patcher modifies:

`GraveyardKeeper2_Data/Managed/Assembly-CSharp.dll`

A backup of the original assembly is automatically created before any changes are made.

## Usage

### Automatic Steam Detection

Simply run:

Linux:

```text
chmod +x GK2-Ultrawide-Fix-linux-x64
./GK2-Ultrawide-Fix-linux-x64
```

Windows:

```text
GK2-Ultrawide-Fix-win-x64.exe
```

The patcher searches for the Graveyard Keeper 2 Steam installation automatically.

Steam libraries on additional drives are also detected through Steam's `libraryfolders.vdf`.

### Manual Game Directory

You can also specify the Graveyard Keeper 2 installation directory manually:

Linux:

```text
./GK2-Ultrawide-Fix-linux-x64 "/path/to/steamapps/common/Graveyard Keeper 2"
```

Windows:

```text
GK2-Ultrawide-Fix-win-x64.exe "C:\Program Files (x86)\Steam\steamapps\common\Graveyard Keeper 2"
```

## Restore Original Files

The patcher automatically creates:

```text
Assembly-CSharp.dll.ultrawide-backup
```

Linux:

To restore the original game assembly:

```text
./GK2-Ultrawide-Fix-linux-x64 --restore
```

A custom game directory can also be specified:

```text
./GK2-Ultrawide-Fix-linux-x64 --restore "/path/to/Graveyard Keeper 2"
```

Windows:

To restore the original game assembly:

```text
GK2-Ultrawide-Fix-win-x64.exe --restore
```

A custom game directory can also be specified:

```text
GK2-Ultrawide-Fix-win-x64.exe --restore "/path/to/Graveyard Keeper 2"
```

## Verbose Output

Technical information about Steam detection, method location, RVA and file offsets can be displayed with:

```text
./GK2-Ultrawide-Fix-linux-x64 --verbose
```

## Building From Source

Requirements:

* .NET SDK
* Mono.Cecil

Clone the repository and build:

```text
dotnet build -c Release
```

### Linux x64

```text
dotnet publish -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -o publish/linux-x64
```

### Windows x64

```text
dotnet publish -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:DebugType=None \
    -p:DebugSymbols=false \
    -o publish/win-x64
```

The resulting executables are self-contained and do not require a separate .NET installation.

## Safety

The patcher verifies the target method before modifying the game assembly.

If the implementation is unknown, for example after an incompatible game update, the patcher aborts without applying the patch.

The original assembly is backed up automatically and can be restored using `--restore`.

Steam's "Verify integrity of game files" feature can also be used to restore the original game files.

## Game Updates

Game updates may replace `Assembly-CSharp.dll` and therefore remove the patch.

After an update, simply run the patcher again.

If the game update changes the relevant code, the patcher may report that the installed game version is unsupported. In that case, do not attempt to manually force the patch.

## Disclaimer

This is an unofficial community modification and is not affiliated with or endorsed by Lazy Bear Games or the publisher of Graveyard Keeper 2.

Use it at your own risk.

No game files are distributed with this project. The patcher only modifies an existing local installation.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
