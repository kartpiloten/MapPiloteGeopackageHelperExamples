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

## Example Projects

| Project | Description | API Style | Complexity |
|---------|-------------|-----------|------------|
| **[HelloWorld](MapPiloteGeopackageHelperHelloWorld/)** | Step-by-step tutorial showing basic operations | Traditional |  Beginner |
| **[FluentApi](MapPiloteFluentApiExample/)** | Comprehensive modern API demonstration | Modern |  Intermediate |
| **[SchemaBrowser](MapPiloteGeopackageHelperSchemaBrowser/)** | Inspect existing GeoPackage files and metadata | Analysis |  Intermediate |
| **[BulkLoadTester](MapPiloteBulkLoadPerformaceTester/)** | Performance comparison between insertion methods | Benchmarks |  Advanced |
| **[LargeDataset](MapPiloteLargeDatasetUploadExample/)** | Spatial index performance with large datasets | Performance |  Advanced |

### Project Details

####  MapPiloteGeopackageHelperHelloWorld
Perfect starting point! Shows traditional API usage with:
- Creating GeoPackages and layers
- Inserting individual points
- Bulk operations
- Reading data back

####  MapPiloteFluentApiExample  
Modern async/await patterns featuring:
- Fluent API chains with `GeoPackage.OpenAsync()`
- Progress reporting during bulk operations
- Streaming queries with `IAsyncEnumerable`
- CRUD operations (Create, Read, Update, Delete)
- Real Swedish cities dataset

#### MapPiloteGeopackageHelperSchemaBrowser
Essential for working with unknown GeoPackage files:
- Layer discovery and metadata extraction
- Column schema inspection
- Spatial reference system details
- Sample data browsing
- Includes sample Swedish administrative borders

#### MapPiloteBulkLoadPerformaceTester
Performance comparison demonstrating:
- Single-row vs bulk insert speed differences
- Timing measurements and throughput analysis
- File size comparisons
- Configurable dataset generation

####  MapPiloteLargeDatasetUploadExample
Advanced spatial operations with:
- Large dataset generation (100k+ points)
- Spatial index creation and performance testing
- Buffer queries and spatial filtering
- Real-world performance scenarios across Sweden

## API Comparison

### Modern API (Recommended)
```csharp
// Fluent, async, with progress reporting
using var gp = await GeoPackage.OpenAsync("data.gpkg");
var layer = await gp.EnsureLayerAsync("places", schema);
await layer.BulkInsertAsync(features, options, progress);
var count = await layer.CountAsync("population > 50000");
```

### Traditional API (Still Supported)
```csharp
// Step-by-step approach
CMPGeopackageCreateHelper.CreateGeoPackage(path, srid);
GeopackageLayerCreateHelper.CreateGeopackageLayer(path, name, schema);
CGeopackageAddDataHelper.BulkInsertFeatures(path, name, features);
```

## Library Features

| Feature | Description | Example |
|---------|-------------|---------|
| **Async/Await** | Modern async support with CancellationToken | `await layer.BulkInsertAsync(...)` |
| **Fluent API** | Chain operations naturally | `GeoPackage.OpenAsync().EnsureLayerAsync()` |
| **Progress Reporting** | Track long-running operations | `IProgress<BulkProgress>` |
| **Options Objects** | Clean configuration patterns | `BulkInsertOptions(BatchSize: 1000)` |
| **Streaming** | `IAsyncEnumerable` for large datasets | `await foreach (var item in ...)` |
| **Rich Queries** | WHERE, LIMIT, ORDER BY support | `ReadOptions(WhereClause: "pop > 1000")` |
| **Conflict Handling** | Insert policies (Abort/Ignore/Replace) | `ConflictPolicy.Ignore` |
| **CRUD Operations** | Count, Delete with conditions | `await layer.DeleteAsync("status = 'old'")` |
| **Spatial Indexing** | Optimized spatial queries | `CreateSpatialIndex: true` |

## Running the Examples

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- The examples will automatically install **MapPiloteGeopackageHelper** from NuGet

### Quick Start
```bash
# Clone the repository
git clone https://github.com/kartpiloten/MapPiloteGeopackageHelperExamples
cd MapPiloteGeopackageHelperExamples

# Run the hello world example
cd MapPiloteGeopackageHelperHelloWorld
dotnet run

# Run the modern fluent API example  
cd ../MapPiloteFluentApiExample
dotnet run

# Inspect sample data
cd ../MapPiloteGeopackageHelperSchemaBrowser
dotnet run
```

### Building All Projects
```bash
# Build entire solution
dotnet build

# Run performance tests
cd MapPiloteBulkLoadPerformaceTester
dotnet run

# Test with large datasets
cd ../MapPiloteLargeDatasetUploadExample  
dotnet run
```

## Sample Data

The repository includes sample GeoPackage files:
- **AdmBordersSweden.gpkg** - Swedish administrative boundaries
- **SwedenBorder.gpkg** - National boundary for spatial filtering
- Generated files from examples (cities, pollution data, etc.)

All generated `.gpkg` files can be opened in:
- **QGIS** (recommended)
- **ArcGIS**
- **FME**
- Any OGC GeoPackage-compatible software

## Reference Links

### GeoPackage Specification
- **[GeoPackage Encoding Standard](https://www.geopackage.org/spec/)** - Official OGC specification
- **[OGC Standard Page](https://www.ogc.org/standard/geopackage/)** - Standards organization
- **Core Tables:**
  - [gpkg_contents](https://www.geopackage.org/spec/#_contents) - Layer metadata
  - [gpkg_spatial_ref_sys](https://www.geopackage.org/spec/#_spatial_ref_sys) - Coordinate systems
  - [gpkg_geometry_columns](https://www.geopackage.org/spec/#_geometry_columns) - Spatial columns
- **[Binary Geometry Format](https://www.geopackage.org/spec/#gpb_format)** - Spatial data encoding

### Library Resources
- **[NuGet Package](https://www.nuget.org/packages/MapPiloteGeopackageHelper/)** - Official package
- **[API Documentation](https://github.com/kartpiloten/MapPiloteGeopackageHelper/wiki)** - Full reference
- **[NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite)** - Geometry library

## What This Library Does

- ? **Creates** GeoPackages with required OGC core tables  
- ? **Manages** layers (tables) with geometry + custom attribute columns  
- ? **Bulk writes** features with validation and progress tracking  
- ? **Streams** features back with filtering, paging, and sorting  
- ? **Provides** modern async patterns with cancellation support  
- ? **Enables** schema inspection and validation  
- ? **Optimizes** spatial queries with R-tree indexing

## System Requirements

- **.NET 9.0** or later
- **Windows, macOS, or Linux**
- **SQLite** (included via Microsoft.Data.Sqlite)
- **NetTopologySuite** for geometry operations

## Contributing

This is an examples repository. For library issues or feature requests, please visit the main [MapPiloteGeopackageHelper repository](https://github.com/kartpiloten/MapPiloteGeopackageHelper).

## License

MIT License - see individual project files for full license text.

---

**Ready to get started?** Begin with the **HelloWorld** example and work your way up to the advanced spatial indexing examples!

