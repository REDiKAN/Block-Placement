# Script Combiner

Professional Unity Editor tool for combining C# scripts into a single file for AI analysis or documentation.

## Features

- **Smart Consolidation**: Automatically groups and deduplicates `using` statements
- **Code Cleanup**: Remove comments, empty lines, and regions
- **File Exclusions**: Filter out test files and temporary scripts
- **Multiple Encodings**: Support for UTF-8, ANSI, and Win-1251
- **Detailed Statistics**: Track code lines, comments, classes, and methods
- **Drag & Drop**: Easy file and folder selection
- **Preview Mode**: View combined output before saving
- **Clipboard Support**: Quick copy to clipboard

## Installation

1. Import the package into your Unity project
2. Navigate to **Tools > Script Combiner** in the menu bar
3. The editor window will open

## Usage

### Basic Workflow

1. **Select Files**: Drag and drop `.cs` files or folders into the drop area, or use the "Add Selected" button
2. **Configure Options**: Adjust encoding, exclusions, and cleanup settings
3. **Preview**: Switch to the Preview tab and click "Regenerate Preview"
4. **Export**: Click "Save Combined Scripts To File..." or "Copy to Clipboard"

### Configuration Options

#### Generation Settings
- **Target Encoding**: Choose between UTF-8, ANSI, or Win-1251
- **Consolidate Usings**: Group all using statements at the top
- **Enable Exclusions**: Filter files by name patterns
- **Code Cleanup**: Remove comments, empty lines, and regions
- **Detailed Statistics**: Show detailed line-by-line analysis

#### File Selection
- Drag and drop files or folders directly
- Use "Add Selected" to add currently selected assets
- Use "Add Folder" to browse for folders
- Remove individual files with the ✖ button

### Output Format

The combined file includes:
- Header with generation metadata
- Consolidated using statements (if enabled)
- All script contents with file path headers
- Statistics summary

## Requirements

- Unity 2020.3 or later
- .NET Standard 2.1 compatible

## Support

For issues, feature requests, or questions, please contact support.

## License

See LICENSE file for details.