# MapPiloteGeopackageHelper Examples & Tests

This repository contains comprehensive examples, tutorials, and performance tests for the [MapPiloteGeopackageHelper](https://www.nuget.org/packages/MapPiloteGeopackageHelper/) .NET library.

[![NuGet](https://img.shields.io/nuget/v/MapPiloteGeopackageHelper.svg)](https://www.nuget.org/packages/MapPiloteGeopackageHelper/)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download)

## Quick Start

The **MapPiloteGeopackageHelper** library provides modern .NET APIs for creating, reading, and bulk-loading GeoPackage (GPKG) data using SQLite and NetTopologySuite.

### Installation
```bash
dotnet add package MapPiloteGeopackageHelper
```

### Modern Fluent API Example
```csharp
// Create/open GeoPackage with fluent API
using var geoPackage = await GeoPackage.OpenAsync("data.gpkg", srid: 3006);

// Create layer with schema
var layer = await geoPackage.EnsureLayerAsync("cities", new Dictionary<string, string>
{
    ["name"] = "TEXT",
    ["population"] = "INTEGER", 
    ["area_km2"] = "REAL"
});

// Bulk insert with progress
var progress = new Progress<BulkProgress>(p => 
    Console.WriteLine($"Progress: {p.PercentComplete:F1}%"));

await layer.BulkInsertAsync(features, 
    new BulkInsertOptions(BatchSize: 1000, CreateSpatialIndex: true),
    progress);

// Query with async streaming
await foreach (var city in layer.ReadFeaturesAsync(
    new ReadOptions(WhereClause: "population > 100000", Limit: 10)))
{
    Console.WriteLine($"City: {city.Attributes["name"]} - {city.Attributes["population"]} people");
}
```

## Learning Path - Example Projects

Each example is designed as a complete tutorial with detailed explanations, real-world data, and practical code you can copy into your projects.

| Project | Learning Focus | Difficulty | Key Concepts |
|---------|----------------|------------|--------------|
| **[HelloWorld](MapPiloteGeopackageHelperHelloWorld/)** | Your first GeoPackage | [Beginner] | File creation, basic schema, individual inserts |
| **[FluentApi](MapPiloteFluentApiExample/)** | Modern async patterns | [Intermediate] | Fluent API, progress reporting, CRUD operations |
| **[OptionalCallbackPattern](MapPiloteGeoPackageOptionalCallbackPattern/)** | Progress callbacks | [Intermediate] | IProgress<T>, callback patterns, UI responsiveness |
| **[SchemaBrowser](MapPiloteGeopackageHelperSchemaBrowser/)** | Exploring existing files | [Intermediate] | Metadata inspection, code generation, data analysis |
| **[BulkLoadTester](MapPiloteBulkLoadPerformaceTester/)** | Performance comparison | [Advanced] | Bulk vs single inserts, benchmarking, optimization |
| **[LargeDataset](MapPiloteLargeDatasetUploadExample/)** | Spatial indexing | [Advanced] | Large datasets, spatial queries, index performance |

### Recommended Learning Sequence

**START HERE: Complete Beginner**
1. **HelloWorld** - Learn the fundamentals step-by-step
2. **FluentApi** - Explore modern patterns and comprehensive features
3. **SchemaBrowser** - Understand how to work with existing data

**Performance & Optimization**  
4. **BulkLoadTester** - Learn performance best practices
5. **LargeDataset** - Master spatial indexing for large datasets

**Advanced Patterns**
6. **OptionalCallbackPattern** - Implement progress reporting and callbacks

## Detailed Project Descriptions

### HelloWorld Tutorial
**Perfect first introduction to GeoPackages**
- Step-by-step file creation with detailed explanations
- Schema definition for real Swedish cities data
- Individual feature insertion with coordinate examples
- Bulk operations demonstration with performance benefits
- Complete data verification and display
- Ready-to-copy code examples

**What you'll learn:**
- GeoPackage fundamentals and file structure
- Coordinate systems (SWEREF99 TM example)
- Traditional API usage patterns
- Basic error handling

### FluentApi Tutorial  
**Complete guide to modern async patterns**
- Structured as 8 comprehensive tasks
- Real Swedish cities with accurate coordinates and data
- Visual progress bars and completion tracking
- Advanced querying with filtering, sorting, and limiting
- CRUD operations with practical examples
- Metadata extraction and spatial analysis
- Professional error handling patterns

**What you'll learn:**
- Modern C# async/await patterns
- Fluent API design principles
- Progress reporting with IProgress<T>
- Complex queries and data filtering
- Spatial extent analysis

### SchemaBrowser Inspector
**Master GeoPackage exploration and analysis**
- Complete file structure analysis tutorial
- Automatic C# code generation from schemas
- Sample data preview with formatted output
- Column type mapping and nullability handling
- Copy-paste ready code examples
- Professional error handling for corrupted files

**What you'll learn:**
- GeoPackage metadata structure
- Schema analysis and documentation
- Code generation techniques
- Working with unknown data sources

### BulkLoadTester Benchmark
**Performance optimization masterclass**
- Side-by-side comparison of insertion methods
- Real-time performance metrics and analysis
- Visual progress indicators for both methods
- File size and throughput comparisons
- Detailed performance analysis with recommendations
- Configurable test parameters

**What you'll learn:**
- When to use bulk operations vs single inserts
- Database transaction optimization
- Performance measurement techniques
- Scaling considerations for large datasets

### LargeDataset Spatial Indexing
**Advanced spatial query optimization**
- Realistic large dataset generation (10k+ points)
- Spatial index creation and performance testing
- Buffer queries with geometric analysis
- Performance comparison with detailed metrics
- Swedish geographic boundary integration
- Production-ready optimization techniques

**What you'll learn:**
- Spatial indexing benefits and implementation
- Large dataset handling strategies
- Geometric operations and spatial queries
- Performance optimization for spatial data

### OptionalCallbackPattern Tutorial
**Progress reporting and callback design patterns**
- 5 comprehensive callback scenarios
- Silent operations vs progress monitoring
- Visual progress indicators and business logic integration
- Conditional callback usage patterns
- Modern .NET IProgress<T> implementation
- Console-safe formatting without emojis

**What you'll learn:**
- Optional callback pattern design
- Progress reporting best practices
- IProgress<T> interface usage
- Conditional callback implementation
- UI responsiveness techniques

## Running the Examples

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- Examples automatically install **MapPiloteGeopackageHelper** from NuGet
- No additional setup required

### Quick Start Commands
```bash
# Clone and explore
git clone https://github.com/kartpiloten/MapPiloteGeopackageHelperExamples
cd MapPiloteGeopackageHelperExamples

# Start with the basics
cd MapPiloteGeopackageHelperHelloWorld
dotnet run

# Explore modern patterns
cd ../MapPiloteFluentApiExample
dotnet run

# Learn to inspect files
cd ../MapPiloteGeopackageHelperSchemaBrowser
dotnet run

# Compare performance
cd ../MapPiloteBulkLoadPerformaceTester
dotnet run

# Master spatial indexing
cd ../MapPiloteLargeDatasetUploadExample
dotnet run

# Understand callbacks
cd ../MapPiloteGeoPackageOptionalCallbackPattern
dotnet run
```

### Build Everything
```bash
# Build entire solution
dotnet build

# Verify all examples work
dotnet test  # If tests are added later
```

## Sample Data & Visualization

All examples generate **real Swedish geographic data** using:
- **SWEREF99 TM coordinate system** (SRID 3006)
- **Actual city coordinates** and population data
- **Realistic attribute schemas** for practical learning

Generated `.gpkg` files work perfectly with:
- **QGIS** (recommended - free and powerful)
- **ArcGIS Pro/Desktop**
- **FME Workbench**
- **PostGIS** and other spatial databases
- Any **OGC GeoPackage-compatible** software

## Key Learning Outcomes

After completing these tutorials, you'll master:

**Core Skills:**
- Creating and managing GeoPackage files
- Working with coordinate systems and spatial data
- Understanding schema design for spatial databases
- Reading and writing feature data efficiently

**Performance Optimization:**
- Choosing appropriate insertion methods for your data size
- Implementing spatial indexing for query performance
- Measuring and optimizing spatial operations
- Scaling strategies for production applications

**Modern .NET Patterns:**
- Async/await patterns for I/O operations
- Progress reporting with IProgress<T>
- Fluent API design and usage
- Error handling for spatial data operations

**Production Readiness:**
- Schema inspection and validation
- Working with unknown data sources
- Performance benchmarking and optimization
- Integration with GIS software and workflows

## Reference Links

### GeoPackage Specification
- **[GeoPackage Encoding Standard](https://www.geopackage.org/spec/)** - Official OGC specification
- **[OGC Standard Page](https://www.ogc.org/standard/geopackage/)** - Standards organization
- **[Binary Geometry Format](https://www.geopackage.org/spec/#gpb_format)** - Spatial data encoding

### Library Resources  
- **[NuGet Package](https://www.nuget.org/packages/MapPiloteGeopackageHelper/)** - Official package
- **[NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite)** - Geometry library

## System Requirements

- **.NET 9.0** or later
- **Windows, macOS, or Linux**
- **SQLite** (included via Microsoft.Data.Sqlite)
- **NetTopologySuite** for geometry operations

## Contributing

This is an examples repository demonstrating MapPiloteGeopackageHelper usage. For library issues or feature requests, please visit the main [MapPiloteGeopackageHelper repository](https://github.com/kartpiloten/MapPiloteGeopackageHelper).

## License

MIT License - see individual project files for full license text.

---

**Ready to master GeoPackage development?** Start with **HelloWorld** and progress through each tutorial at your own pace!

