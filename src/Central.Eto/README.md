# Eto.Forms Proof of Concept

This directory contains an Eto.Forms implementation proof of concept for Zero Install dialogs.

## Overview

This proof of concept demonstrates converting simple WinForms dialogs to Eto.Forms while retaining resx-based localization.

## What was converted

Two simple dialogs were converted from WinForms to Eto.Forms:

1. **PortableCreatorDialog** - A dialog for creating portable Zero Install installations
2. **SelectCommandDialog** - A dialog for selecting command options (simplified version)

## Key Features Demonstrated

### ✅ Eto.Forms Integration
- Successfully integrated Eto.Forms 2.10.2 with the existing .NET Framework 4.7.2 codebase
- Created cross-platform compatible dialogs using Eto's abstraction layer
- Used Eto.Platform.Windows for Windows-specific rendering

### ✅ Resx-based Localization Retained
- All resx resource files were copied and remain functional
- Resources are loaded using the standard `ResourceManager` class
- Localization for multiple languages (17 languages) is preserved:
  - English, German, Greek, Spanish, French, Indonesian, Italian, Japanese, Korean, Dutch, Polish, Portuguese (BR and PT), Romanian, Russian, Turkish, Chinese

### ✅ UI Elements Converted
- Labels, TextBoxes, Buttons, ComboBoxes, CheckBoxes
- GroupBox containers
- TableLayout and StackLayout for flexible, responsive layouts
- Dialog base class with standard OK/Cancel button patterns

### ✅ Event Handling
- Button click events
- TextChanged events
- CheckedChanged events for checkboxes
- Proper async/await patterns maintained

## Project Structure

```
Central.Eto/
├── Central.Eto.csproj              # Main Eto.Forms library project
├── PortableCreatorDialog.cs         # Eto version of portable creator dialog
├── PortableCreatorDialog*.resx      # Localization resources (17 languages)
├── SelectCommandDialog.cs           # Eto version of select command dialog
├── SelectCommandDialog*.resx        # Localization resources (17 languages)
└── Properties/
    ├── Resources.Designer.cs        # Resource accessor class
    └── Resources*.resx              # Shared resources

Central.Eto.Demo/
├── Central.Eto.Demo.csproj         # Demo application project
└── Program.cs                       # Demo app showing both dialogs
```

## Building and Running

To build the Eto.Forms projects:

```powershell
cd src/Central.Eto
dotnet build
```

To run the demo application:

```powershell
cd src/Central.Eto.Demo
dotnet run
```

## Comparison: WinForms vs. Eto.Forms

### PortableCreatorDialog

**WinForms version:**
- 60 lines of code + 131 lines of designer code
- Uses InitializeComponent() with designer-generated code
- Platform-specific (Windows only)
- Uses WinForms-specific controls (FolderBrowserDialog, HintTextBox)

**Eto.Forms version:**
- 200 lines of code (no designer code needed)
- All UI construction in code (more maintainable)
- Cross-platform (Windows, Mac, Linux)
- Uses Eto abstractions (SelectFolderDialog, TextBox with PlaceholderText)

### Key Differences

1. **Layout System**:
   - WinForms: Absolute positioning with designer
   - Eto.Forms: Flexible TableLayout and StackLayout

2. **Resource Loading**:
   - WinForms: Automatic through designer using ComponentResourceManager
   - Eto.Forms: Manual using ResourceManager (more explicit, same mechanism)

3. **Platform Support**:
   - WinForms: Windows only
   - Eto.Forms: Windows, macOS, Linux (GTK)

4. **Code Maintainability**:
   - WinForms: Designer code is hard to review in PRs
   - Eto.Forms: All code is explicit and reviewable

## Findings and Recommendations

### ✅ Advantages of Eto.Forms

1. **Cross-platform support** - Can target Windows, macOS, and Linux from the same codebase
2. **Modern layout system** - Flexible layouts that scale well with DPI changes
3. **No designer dependency** - All UI code is explicit and reviewable
4. **Active development** - Eto.Forms is actively maintained
5. **Localization compatible** - Resx files work without modification

### ⚠️ Challenges

1. **Learning curve** - Team needs to learn Eto.Forms API
2. **Manual UI construction** - No visual designer (though this can be an advantage for code review)
3. **Different control model** - Some WinForms patterns need adaptation
4. **Platform-specific features** - Some Windows-specific features may require platform detection

### 📋 Recommendation

**Eto.Forms is a viable option for modernizing Zero Install's UI** with these caveats:

1. **Gradual migration** - Convert dialogs incrementally rather than all at once
2. **Focus on simple dialogs first** - Complex custom controls may require more effort
3. **Test on all platforms** - If cross-platform support is desired, test thoroughly
4. **Consider Avalonia** - As an alternative, Avalonia UI offers XAML-based development which may be more familiar to WPF developers

### Next Steps

If proceeding with Eto.Forms:

1. Convert more dialogs to validate the approach
2. Create reusable base classes and helper methods
3. Establish UI guidelines for consistent look and feel
4. Set up CI/CD for multi-platform testing
5. Document migration patterns for the team

If not proceeding:
- Keep this as a reference implementation
- Consider Avalonia UI or MAUI as alternatives
- Document decision for future reference

## Dependencies

- **Eto.Forms**: 2.10.2
- **Eto.Platform.Windows**: 2.10.2
- **System.Resources.Extensions**: 7.0.0
- **ZeroInstall.Commands**: 2.28.1

## Notes

- The demo application is Windows-specific due to using Eto.Platform.Windows
- For cross-platform deployment, use Eto.Platform.Wpf (Windows), Eto.Platform.Mac (macOS), or Eto.Platform.Gtk (Linux)
- Resource files are binary compatible between WinForms and Eto.Forms
- Some advanced features (like CommandUtils.RunAsync) are not fully implemented in the POC and would need proper implementation
