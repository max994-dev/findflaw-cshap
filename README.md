# FindFlaw

A WPF desktop application for viewing 3D models and line markers. Use it to visualize CAD/engineering models with annotated measurement lines or flaw markers loaded from JSON.

## Features

- **3D model loading** — Supports OBJ and STL formats
- **Line markers** — Load line annotations from JSON files and display them in the 3D viewport
- **Interactive 3D view** — Pan, zoom, and orbit using the mouse or toolbar controls
- **View presets** — Top-down or bottom-up initial camera view
- **Customization** — Model color (Blue/Grey), emissive glow strength
- **Line management** — Select lines in the viewport or list, edit labels, highlight with color spheres

## Requirements

- .NET 8.0
- Windows (WPF)

## Build & Run

```bash
# Restore packages
dotnet restore

# Build
dotnet build FindFlaw.sln

# Run
dotnet run --project FindFlaw
```

Or open `FindFlaw.sln` in Visual Studio 2022 and press F5.

## Usage

1. **Open Model** — Load an OBJ or STL 3D model
2. **Load Lines** — Load a JSON file containing line marker definitions
3. **Navigate** — Use the View pad (top-right) or mouse to pan/zoom; double-click a line to focus the camera on it
4. **Select** — Click lines in the viewport or in the Lines list (left overlay) to select and edit labels

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl + Arrow keys | Move camera position (X/Y) |
| Ctrl + +/- | Move camera position (Z) |
| Ctrl + PageUp/PageDown | Change field of view |
| Shift + Arrow keys | Move look-at target |
| Shift + +/- | Move look-at target (Z) |

## Line JSON Format

```json
[
  {
    "id": 1,
    "label": "Flaw A",
    "StartX": 0.0,
    "StartY": 0.0,
    "StartZ": 0.0,
    "EndX": 1.0,
    "EndY": 0.0,
    "EndZ": 0.0,
    "ColorArgb": -65536
  }
]
```

`ColorArgb` is an ARGB integer (e.g. `-65536` = red).

## Project Structure

```
FindFlaw/
├── MainWindow.xaml         # Main UI layout
├── MainWindow.xaml.cs      # Window logic, event handlers
├── Managers/
│   ├── LineManager.cs      # Line loading, selection, highlight
│   ├── ModelManager.cs     # 3D model loading
│   ├── ViewportInteraction.cs
│   └── UIStateManager.cs
├── Models/
│   └── LineMarker.cs       # Line marker model & visuals
└── Assets/
    └── findflaw.ico
```

## Dependencies

- [HelixToolkit.Wpf](https://github.com/helix-toolkit/helix-toolkit) — 3D visualization

## License

See [LICENSE](LICENSE) for details.
