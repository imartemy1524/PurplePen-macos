# PurplePen

PurplePen is a desktop course-setting application for orienteering races. It is
being ported from the original Windows WinForms application to a cross-platform
Avalonia application.

This README focuses on building and running the cross-platform app on Linux and
macOS.

## Current Status

The cross-platform application is:

```text
src/AvPurplePen/AvPurplePen.csproj
```

Use this project on Linux and macOS. The legacy WinForms project under
`src/PurplePen/` is Windows-specific and is not the target for cross-platform
builds.

The Avalonia app currently builds for `net10.0` and is compiled with the
`PORTING` symbol. Some legacy UI workflows are still being ported.

## Repository Layout

Important paths:

```text
README.md                                This file
src/PPen.slnx                            Solution file
src/AvPurplePen/                         Avalonia desktop app
src/PurplePenViewModels/                 Avalonia ViewModels
src/PurplePenCore/                       Shared application/core logic
src/MapModel/                            Map model and rendering backends
src/MapModel/PDFsharp/                   PDFsharp git submodule
src/PdfConverter/                        PDF-to-bitmap helper tool
src/PurplePenViewModels.Tests/           NUnit ViewModel tests
```

## Prerequisites

Required:

- Git
- .NET SDK capable of building `net10.0`
- A graphical desktop session for running the app

Recommended:

- Use the local SDK path `~/.dotnet/dotnet` if that is how .NET is installed on
  your machine.
- Keep `~/.dotnet` first in `PATH` so child `dotnet` invocations use the same
  SDK.

Check the SDK:

```bash
~/.dotnet/dotnet --info
```

Or, if `dotnet` is installed globally:

```bash
dotnet --info
```

### Linux Native Packages

Most desktop Linux installs already have the required GUI libraries. Minimal
systems and containers often do not.

On Debian or Ubuntu, install the usual desktop runtime libraries:

```bash
sudo apt-get update
sudo apt-get install -y \
  libfontconfig1 \
  libfreetype6 \
  libx11-6 \
  libxext6 \
  libxrender1 \
  libxrandr2 \
  libxi6 \
  libxcursor1 \
  libxinerama1 \
  libgl1
```

On Fedora:

```bash
sudo dnf install -y \
  fontconfig \
  freetype \
  libX11 \
  libXext \
  libXrender \
  libXrandr \
  libXi \
  libXcursor \
  libXinerama \
  mesa-libGL
```

For headless Linux, you need an X11 or Wayland display. Building works headless;
running the GUI does not.

### macOS Notes

Use a normal Terminal session inside a logged-in desktop. No extra native
packages are normally required beyond the .NET SDK.

On Apple Silicon, `osx-arm64` is the native publish runtime. On Intel Macs, use
`osx-x64`.

## Clone and Initialize Submodules

Clone the repository:

```bash
git clone <repo-url> PurplePen
cd PurplePen
```

Initialize submodules:

```bash
git submodule update --init --recursive
```

The PDFsharp submodule must be present at:

```text
src/MapModel/PDFsharp
```

This repository currently expects PDFsharp at:

```text
4faa3276fc3d052aa2c4dcb5836c95289896710e
```

You can verify it with:

```bash
git -C src/MapModel/PDFsharp rev-parse HEAD
```

## Choose the dotnet Command

For the examples below, set a shell variable once:

```bash
export DOTNET="$HOME/.dotnet/dotnet"
export PATH="$HOME/.dotnet:$PATH"
```

If you use a system-wide SDK instead:

```bash
export DOTNET="dotnet"
```

All commands below assume you are at the repository root.

## Restore

Restore packages for the Avalonia app:

```bash
$DOTNET restore src/AvPurplePen/AvPurplePen.csproj
```

The restore downloads NuGet packages for Avalonia, SkiaSharp, PDFiumCore,
ImageSharp, NUnit test dependencies, and the other managed dependencies.

## Build

Debug build:

```bash
$DOTNET build src/AvPurplePen/AvPurplePen.csproj \
  -f net10.0 \
  -v:minimal \
  -m:1
```

Release build:

```bash
$DOTNET build src/AvPurplePen/AvPurplePen.csproj \
  -c Release \
  -f net10.0 \
  -v:minimal \
  -m:1
```

The `AvPurplePen` project also builds `src/PdfConverter/PdfConverter.csproj`
for `net10.0` and copies its output next to the app. This is required for PDF
map support.

Expected debug output:

```text
src/AvPurplePen/bin/Debug/net10.0/AvPurplePen.dll
src/AvPurplePen/bin/Debug/net10.0/PdfConverter.dll
```

On Unix-like systems, the SDK may also create apphost executables:

```text
src/AvPurplePen/bin/Debug/net10.0/AvPurplePen
src/AvPurplePen/bin/Debug/net10.0/PdfConverter
```

## Run

Run the debug build via the DLL:

```bash
PATH="$HOME/.dotnet:$PATH" \
  $DOTNET src/AvPurplePen/bin/Debug/net10.0/AvPurplePen.dll
```

If the apphost executable exists, you can also run:

```bash
src/AvPurplePen/bin/Debug/net10.0/AvPurplePen
```

For a release build:

```bash
PATH="$HOME/.dotnet:$PATH" \
  $DOTNET src/AvPurplePen/bin/Release/net10.0/AvPurplePen.dll
```

## Publish

Framework-dependent publish, using the installed .NET runtime:

```bash
$DOTNET publish src/AvPurplePen/AvPurplePen.csproj \
  -c Release \
  -f net10.0 \
  -o artifacts/AvPurplePen
```

Run it:

```bash
PATH="$HOME/.dotnet:$PATH" \
  $DOTNET artifacts/AvPurplePen/AvPurplePen.dll
```

Self-contained publish examples:

```bash
# macOS Apple Silicon
$DOTNET publish src/AvPurplePen/AvPurplePen.csproj \
  -c Release \
  -f net10.0 \
  -r osx-arm64 \
  --self-contained true \
  -o artifacts/AvPurplePen-osx-arm64

# macOS Intel
$DOTNET publish src/AvPurplePen/AvPurplePen.csproj \
  -c Release \
  -f net10.0 \
  -r osx-x64 \
  --self-contained true \
  -o artifacts/AvPurplePen-osx-x64

# Linux x64
$DOTNET publish src/AvPurplePen/AvPurplePen.csproj \
  -c Release \
  -f net10.0 \
  -r linux-x64 \
  --self-contained true \
  -o artifacts/AvPurplePen-linux-x64

# Linux arm64
$DOTNET publish src/AvPurplePen/AvPurplePen.csproj \
  -c Release \
  -f net10.0 \
  -r linux-arm64 \
  --self-contained true \
  -o artifacts/AvPurplePen-linux-arm64
```

Run a self-contained publish:

```bash
artifacts/AvPurplePen-linux-x64/AvPurplePen
```

or on macOS:

```bash
artifacts/AvPurplePen-osx-arm64/AvPurplePen
```

The publish target copies `PdfConverter` output into the publish directory
automatically.

## Tests

Run the ViewModel test suite:

```bash
$DOTNET test src/PurplePenViewModels.Tests/PurplePenViewModels.Tests.csproj \
  --logger "console;verbosity=normal" \
  -m:1
```

Run it without restoring again:

```bash
$DOTNET test src/PurplePenViewModels.Tests/PurplePenViewModels.Tests.csproj \
  --no-restore \
  --logger "console;verbosity=normal" \
  -m:1
```

Some older tests and WinForms-oriented projects are still Windows-oriented. For
Linux/macOS development, start with the Avalonia app and ViewModel tests above.

## PDF Map Support

PDF map support depends on two pieces:

- `src/MapModel/PDFsharp` submodule
- `src/PdfConverter`, built automatically by `AvPurplePen`

After building `AvPurplePen`, `PdfConverter.dll` should exist next to
`AvPurplePen.dll`.

You can manually test the converter with a sample PDF:

```bash
$DOTNET src/AvPurplePen/bin/Debug/net10.0/PdfConverter.dll \
  72 \
  src/TestFiles/pdfmaps/Potholes.pdf \
  /tmp/purplepen-potholes.png
```

If this command creates `/tmp/purplepen-potholes.png`, the PDF conversion helper
is available.

## Common Problems

### The build cannot find PDFsharp projects

Initialize submodules:

```bash
git submodule update --init --recursive
```

Then restore and build again.

### `File -> New Event` or PDF map loading fails with missing PdfConverter

Rebuild the Avalonia app:

```bash
$DOTNET build src/AvPurplePen/AvPurplePen.csproj -f net10.0 -v:minimal -m:1
```

Check that `PdfConverter.dll` is next to `AvPurplePen.dll`:

```bash
ls src/AvPurplePen/bin/Debug/net10.0/PdfConverter.dll
```

### The app does not start on Linux

Make sure you are in a graphical session:

```bash
echo "$DISPLAY"
echo "$WAYLAND_DISPLAY"
```

If both are empty, start a desktop session or run under a virtual display.

Also install the Linux native packages listed above.

### `dotnet` resolves to the wrong SDK

Use the local SDK explicitly:

```bash
export DOTNET="$HOME/.dotnet/dotnet"
export PATH="$HOME/.dotnet:$PATH"
$DOTNET --info
```

### NuGet warning NU1903 for `Tmds.DBus.Protocol`

The current dependency graph can emit a high-severity audit warning for
`Tmds.DBus.Protocol`. At the time this README was written, the app still builds
successfully with that warning.

## Development Notes

- Prefer `src/AvPurplePen/AvPurplePen.csproj` for Linux/macOS work.
- Keep ViewModels platform-neutral under `src/PurplePenViewModels`.
- Keep Avalonia views and platform dialogs under `src/AvPurplePen`.
- Map rendering is moving toward SkiaSharp through `Map_SkiaStd`.
- Do not use the old WinForms `src/PurplePen/PurplePen.csproj` as the
  cross-platform entry point.

