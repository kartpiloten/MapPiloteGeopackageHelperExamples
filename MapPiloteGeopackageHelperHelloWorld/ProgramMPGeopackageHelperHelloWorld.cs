/* Licence...
 * MIT License
 *
 * Copyright (c) 2025 Anders Dahlgren
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights 
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */
using MapPiloteGeopackageHelper;
using NetTopologySuite.Geometries;

// =============================================================
// Hello World Tutorial for MapPiloteGeopackageHelper
// -------------------------------------------------------------
// Perfect first introduction to GeoPackage creation with:
//  1) Creating your first GeoPackage file
//  2) Defining schemas and creating layers
//  3) Adding individual features with geometry and attributes
//  4) Bulk inserting multiple features efficiently  
//  5) Reading and displaying data
// Each step is simple, clear, and ready to copy into your projects!
// =============================================================

Console.WriteLine("=== MapPilote GeoPackage Helper - Hello World Tutorial ===");
Console.WriteLine("Your first steps into GeoPackage creation with .NET");
Console.WriteLine();

Console.WriteLine("WHAT YOU'LL LEARN:");
Console.WriteLine("• Create your first GeoPackage file");
Console.WriteLine("• Define table schemas with geometry and attributes");  
Console.WriteLine("• Insert points with coordinate data and properties");
Console.WriteLine("• Use bulk operations for better performance");
Console.WriteLine("• Read your data back and display it");
Console.WriteLine();

// =================================================================
// SETUP: File paths and coordinate system
// =================================================================
Console.WriteLine("STEP 1: Setting up file paths and coordinate system");

string workingDirectory = Environment.CurrentDirectory;
string geoPackageFile = Path.Combine(workingDirectory, "MyFirstGeoPackage.gpkg");
string layerName = "sample_locations";
const int coordinateSystem = 3006; // SWEREF99 TM - Swedish national grid

Console.WriteLine($"  Working directory: {workingDirectory}");
Console.WriteLine($"  GeoPackage file: {Path.GetFileName(geoPackageFile)}");
Console.WriteLine($"  Layer name: {layerName}");
Console.WriteLine($"  Coordinate system: SWEREF99 TM (SRID {coordinateSystem})");
Console.WriteLine();

// Clean start - remove any existing file
TryDeleteFile(geoPackageFile);

// =================================================================
// STEP 2: Create empty GeoPackage
// =================================================================
Console.WriteLine("STEP 2: Creating an empty GeoPackage database");
Console.WriteLine("  GeoPackages are SQLite databases with spatial extensions");

CMPGeopackageCreateHelper.CreateGeoPackage(geoPackageFile, coordinateSystem);
Console.WriteLine("  SUCCESS: Empty GeoPackage created with required metadata tables");
Console.WriteLine();

// =================================================================
// STEP 3: Define schema and create layer
// =================================================================
Console.WriteLine("STEP 3: Defining table schema and creating a spatial layer");
Console.WriteLine("  Schemas define what attributes (columns) each feature will have");

// Define what information we want to store about each location
var tableSchema = new Dictionary<string, string>(StringComparer.Ordinal)
{
    ["location_name"] = "TEXT",      // Name of the place
    ["category"] = "TEXT",           // Type of location  
    ["population"] = "INTEGER",      // Number of people
    ["area_hectares"] = "REAL",      // Size in hectares
    ["description"] = "TEXT"         // Additional notes
};

Console.WriteLine("  Schema defined with these attribute columns:");
foreach (var column in tableSchema)
{
    Console.WriteLine($"    • {column.Key}: {column.Value}");
}
Console.WriteLine("  Note: Geometry column and ID column are added automatically");

// Create the spatial layer (table) with this schema
GeopackageLayerCreateHelper.CreateGeopackageLayer(
    geoPackageFile,
    layerName, 
    tableSchema,
    geometryType: "POINT",
    srid: coordinateSystem);

Console.WriteLine($"  SUCCESS: Spatial layer '{layerName}' created successfully");
Console.WriteLine();

// =================================================================
// STEP 4: Insert individual features
// =================================================================
Console.WriteLine("STEP 4: Adding individual features (one at a time)");
Console.WriteLine("  Each feature combines a geographic location with attribute data");

// Create a point in Stockholm, Sweden (SWEREF99 TM coordinates)
var stockholmPoint = new Point(683527, 6579433);
var stockholmAttributes = new[] { "Stockholm", "City", "975551", "18800", "Capital of Sweden" };

Console.WriteLine("  Adding Stockholm:");
Console.WriteLine($"    Coordinates: ({stockholmPoint.X:F0}, {stockholmPoint.Y:F0})");
Console.WriteLine($"    Name: {stockholmAttributes[0]}");
Console.WriteLine($"    Population: {stockholmAttributes[2]}");

CGeopackageAddDataHelper.AddPointToGeoPackage(geoPackageFile, layerName, stockholmPoint, stockholmAttributes);
Console.WriteLine("  SUCCESS: Stockholm added successfully");

// Add another point
var goteborgPoint = new Point(317773, 6394498);
var goteborgAttributes = new[] { "Göteborg", "City", "579281", "20360", "Sweden's second largest city" };

Console.WriteLine("  Adding Göteborg:");
Console.WriteLine($"    Coordinates: ({goteborgPoint.X:F0}, {goteborgPoint.Y:F0})");
Console.WriteLine($"    Population: {goteborgAttributes[2]}");

CGeopackageAddDataHelper.AddPointToGeoPackage(geoPackageFile, layerName, goteborgPoint, goteborgAttributes);
Console.WriteLine("  SUCCESS: Göteborg added successfully");
Console.WriteLine();

// =================================================================  
// STEP 5: Bulk insert multiple features
// =================================================================
Console.WriteLine("STEP 5: Bulk inserting multiple features (efficient method)");
Console.WriteLine("  For multiple features, bulk operations are much faster");

// Prepare several features using FeatureRecord objects
var bulkFeatures = new List<FeatureRecord>
{
    new FeatureRecord(
        Geometry: new Point(375040, 6163000), // Malmö
        Attributes: new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["location_name"] = "Malmö",
            ["category"] = "City", 
            ["population"] = "350963",
            ["area_hectares"] = "15840",
            ["description"] = "Southern Sweden's largest city"
        }),
    
    new FeatureRecord(
        Geometry: new Point(646138, 6636722), // Uppsala
        Attributes: new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["location_name"] = "Uppsala",
            ["category"] = "City",
            ["population"] = "230767", 
            ["area_hectares"] = "4880",
            ["description"] = "Historic university city"
        }),
        
    new FeatureRecord(
        Geometry: new Point(511954, 6569151), // Örebro
        Attributes: new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["location_name"] = "Örebro",
            ["category"] = "City",
            ["population"] = "126009",
            ["area_hectares"] = "5820", 
            ["description"] = "Central Sweden regional center"
        })
};

Console.WriteLine($"  Prepared {bulkFeatures.Count} features for bulk insert:");
foreach (var feature in bulkFeatures)
{
    Console.WriteLine($"    • {feature.Attributes["location_name"]} (pop: {feature.Attributes["population"]})");
}

// Perform the bulk insert
CGeopackageAddDataHelper.BulkInsertFeatures(
    geoPackagePath: geoPackageFile,
    layerName: layerName,
    features: bulkFeatures,
    srid: coordinateSystem,
    batchSize: 100);  // Process in batches for optimal performance

Console.WriteLine($"  SUCCESS: Bulk inserted {bulkFeatures.Count} cities successfully");
Console.WriteLine();

// =================================================================
// STEP 6: Read data back and display
// =================================================================  
Console.WriteLine("STEP 6: Reading all data back from the GeoPackage");
Console.WriteLine("  Verify our data was stored correctly");

var allFeatures = CMPGeopackageReadDataHelper.ReadFeatures(
    geoPackageFilePath: geoPackageFile,
    tableName: layerName,
    geometryColumn: "geom",  // Standard geometry column name
    includeGeometry: true);   // Include spatial coordinates

Console.WriteLine("  ALL LOCATIONS IN DATABASE:");
Console.WriteLine();

int displayCount = 0;
foreach (var feature in allFeatures)
{
    displayCount++;
    
    // Extract coordinate information
    var point = feature.Geometry as Point;
    var coordinates = point != null ? $"({point.X:F0}, {point.Y:F0})" : "<no coordinates>";
    
    // Extract attribute information
    var name = feature.Attributes.GetValueOrDefault("location_name") ?? "<unknown>";
    var category = feature.Attributes.GetValueOrDefault("category") ?? "<unknown>";
    var population = feature.Attributes.GetValueOrDefault("population") ?? "0";
    var description = feature.Attributes.GetValueOrDefault("description") ?? "<no description>";
    
    // Display the information
    Console.WriteLine($"  {displayCount}. {name} ({category})");
    Console.WriteLine($"     Population: {int.Parse(population):N0}");
    Console.WriteLine($"     Location: {coordinates}");
    Console.WriteLine($"     Notes: {description}");
    Console.WriteLine();
}

// =================================================================
// SUCCESS SUMMARY
// =================================================================
Console.WriteLine("CONGRATULATIONS! Tutorial completed successfully!");
Console.WriteLine(new string('=', 60));
Console.WriteLine($"SUCCESS: Created GeoPackage: {Path.GetFileName(geoPackageFile)}");
Console.WriteLine($"SUCCESS: Added {displayCount} Swedish cities with complete data");
Console.WriteLine($"SUCCESS: File location: {Path.GetFullPath(geoPackageFile)}");
Console.WriteLine();

Console.WriteLine("WHAT YOU LEARNED:");
Console.WriteLine("• How to create GeoPackages with MapPiloteGeopackageHelper");
Console.WriteLine("• Defining table schemas for spatial data");
Console.WriteLine("• Adding features individually and in bulk");
Console.WriteLine("• Reading spatial data back from storage");
Console.WriteLine("• Working with Swedish coordinate system (SWEREF99 TM)");
Console.WriteLine();

Console.WriteLine("NEXT STEPS:");
Console.WriteLine("• Open your GeoPackage file in QGIS to see it visually");
Console.WriteLine("• Try the 'FluentApiExample' for more advanced features");
Console.WriteLine("• Explore 'SchemaBrowser' to inspect existing files");
Console.WriteLine("• Check 'BulkLoadPerformaceTester' for performance insights");
Console.WriteLine();

Console.WriteLine("TIP: GeoPackage files are standard and work with any GIS software!");

// =================================================================
// Helper method for clean file management
// =================================================================
static void TryDeleteFile(string filePath)
{
    try 
    { 
        if (File.Exists(filePath)) 
        {
            File.Delete(filePath);
        }
    } 
    catch 
    { 
        // Ignore file deletion errors - not critical for tutorial
    }
}
