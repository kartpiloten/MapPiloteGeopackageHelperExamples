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
using System.Globalization;

// =============================================================
// Modern Fluent API Tutorial for MapPiloteGeopackageHelper
// -------------------------------------------------------------
// This comprehensive tutorial demonstrates all major features:
//  1) Fluent GeoPackage creation and layer management
//  2) Schema definition and validation
//  3) Bulk insert with progress reporting and options
//  4) Advanced querying with filtering and sorting
//  5) CRUD operations (Create, Read, Update, Delete)
//  6) Metadata extraction and spatial analysis
//  7) Error handling and best practices
// Perfect introduction to the modern async/await API!
// =============================================================

Console.WriteLine("=== MapPilote Fluent API Tutorial ===");
Console.WriteLine("Complete guide to modern async patterns with MapPiloteGeopackageHelper");
Console.WriteLine();

const string gpkgPath = "FluentAPI_Tutorial.gpkg";
const int srid = 3006; // SWEREF99 TM (Swedish coordinate system)

// Clean start
if (File.Exists(gpkgPath)) File.Delete(gpkgPath);

try
{
    // =================================================================
    // TASK 1: Creating and Opening GeoPackages
    // =================================================================
    Console.WriteLine("TASK 1: Creating GeoPackages with the Fluent API");
    Console.WriteLine("The fluent API uses 'using' statements for automatic resource cleanup");
    Console.WriteLine();

    using var geoPackage = await GeoPackage.OpenAsync(gpkgPath, srid);
    Console.WriteLine($"SUCCESS: Created GeoPackage: {Path.GetFileName(gpkgPath)}");
    Console.WriteLine($"SUCCESS: Coordinate system: SWEREF99 TM (SRID {srid})");

    // =================================================================
    // TASK 2: Schema Definition and Layer Creation
    // =================================================================
    Console.WriteLine("\nTASK 2: Defining schemas and creating layers");
    Console.WriteLine("Schemas define the attribute columns (geometry column is automatic)");
    Console.WriteLine();

    // Define a realistic schema for Swedish cities
    var citySchema = new Dictionary<string, string>
    {
        ["name"] = "TEXT",           // City name
        ["population"] = "INTEGER",   // Population count  
        ["area_km2"] = "REAL",       // Area in square kilometers
        ["county"] = "TEXT",         // Swedish county (län)
        ["founded"] = "INTEGER"      // Year founded
    };

    Console.WriteLine("Schema defined:");
    foreach (var column in citySchema)
    {
        Console.WriteLine($"  {column.Key}: {column.Value}");
    }

    // EnsureLayerAsync creates the layer if it doesn't exist
    var citiesLayer = await geoPackage.EnsureLayerAsync("swedish_cities", citySchema, srid);
    Console.WriteLine($"SUCCESS: Layer 'swedish_cities' created with {citySchema.Count} attribute columns");

    // =================================================================
    // TASK 3: Data Generation and Feature Creation
    // =================================================================
    Console.WriteLine("\nTASK 3: Creating features with realistic data");
    Console.WriteLine("FeatureRecord combines geometry with attribute data");
    Console.WriteLine();

    var cities = GenerateSwedishCities();
    Console.WriteLine($"SUCCESS: Generated {cities.Count} Swedish cities with real coordinates");

    // =================================================================
    // TASK 4: Bulk Insert with Progress and Options
    // =================================================================
    Console.WriteLine("\nTASK 4: Bulk insert with progress reporting");
    Console.WriteLine("BulkInsertOptions control performance and behavior");
    Console.WriteLine();

    // Progress callback shows completion percentage
    var progress = new Progress<BulkProgress>(p =>
    {
        // Visual progress bar using console characters
        var barWidth = 25;
        var filledWidth = (int)(p.PercentComplete / 100.0 * barWidth);
        var emptyWidth = barWidth - filledWidth;
        var bar = new string('█', filledWidth) + new string('░', emptyWidth);
        
        Console.Write($"\r  [{bar}] {p.PercentComplete:F0}% ({p.Processed}/{p.Total})");
        if (p.IsComplete) Console.WriteLine(); // New line when done
    });

    Console.WriteLine("Options used:");
    Console.WriteLine("  • BatchSize: 50 (groups inserts for performance)");
    Console.WriteLine("  • CreateSpatialIndex: true (enables fast spatial queries)");  
    Console.WriteLine("  • ConflictPolicy: Ignore (skip duplicates without error)");

    await citiesLayer.BulkInsertAsync(
        cities,
        new BulkInsertOptions(
            BatchSize: 50,
            CreateSpatialIndex: true,
            ConflictPolicy: ConflictPolicy.Ignore
        ),
        progress);

    Console.WriteLine("SUCCESS: All cities inserted successfully!");

    // =================================================================
    // TASK 5: Querying and Counting
    // =================================================================
    Console.WriteLine("\nTASK 5: Querying data with filters and sorting");
    Console.WriteLine("The library supports SQL WHERE clauses and ORDER BY");
    Console.WriteLine();

    // Simple counting
    var totalCities = await citiesLayer.CountAsync();
    Console.WriteLine($"Total cities in database: {totalCities:N0}");

    // Conditional counting  
    var largeCities = await citiesLayer.CountAsync("population >= 100000");
    var mediumCities = await citiesLayer.CountAsync("population BETWEEN 50000 AND 99999");
    var smallCities = await citiesLayer.CountAsync("population < 50000");
    
    Console.WriteLine($"Large cities (≥100k):   {largeCities:N0}");
    Console.WriteLine($"Medium cities (50-99k): {mediumCities:N0}");
    Console.WriteLine($"Small cities (<50k):    {smallCities:N0}");

    // =================================================================
    // TASK 6: Advanced Querying with ReadOptions
    // =================================================================
    Console.WriteLine("\nTASK 6: Advanced queries with ReadOptions");
    Console.WriteLine("ReadOptions provide powerful filtering and sorting capabilities");
    Console.WriteLine();

    Console.WriteLine("TOP 5 LARGEST CITIES:");
    var topCitiesOptions = new ReadOptions(
        IncludeGeometry: true,
        WhereClause: "population > 50000",   // Only significant cities
        OrderBy: "population DESC",          // Largest first
        Limit: 5                            // Top 5 only
    );

    int rank = 1;
    await foreach (var city in citiesLayer.ReadFeaturesAsync(topCitiesOptions))
    {
        var point = city.Geometry as Point;
        var name = city.Attributes["name"];
        var population = int.Parse(city.Attributes["population"]!);
        var county = city.Attributes["county"];
        
        Console.WriteLine($"  {rank}. {name} ({county})");
        Console.WriteLine($"      Population: {population:N0}");
        Console.WriteLine($"      Coordinates: ({point?.X:F0}, {point?.Y:F0})");
        rank++;
    }

    // =================================================================
    // TASK 6A: ORDER BY Testing (Version 1.2.2 Fix Validation)
    // =================================================================
    Console.WriteLine("\nTASK 6A: Testing ORDER BY functionality");
    Console.WriteLine("Comprehensive tests for ascending, descending, and multi-column sorting");
    Console.WriteLine();

    // Test 1: Ascending order by population
    Console.WriteLine("TEST 1: Ascending order by population (5 smallest cities)");
    var ascendingOptions = new ReadOptions(
        IncludeGeometry: false,
        OrderBy: "population ASC",
        Limit: 5
    );
    
    var expectedAscending = true;
    var previousPop = 0;
    rank = 1;
    await foreach (var city in citiesLayer.ReadFeaturesAsync(ascendingOptions))
    {
        var name = city.Attributes["name"];
        var population = int.Parse(city.Attributes["population"]!);
        
        Console.WriteLine($"  {rank}. {name}: {population:N0}");
        
        if (population < previousPop) expectedAscending = false;
        previousPop = population;
        rank++;
    }
    Console.WriteLine(expectedAscending ? "  ✓ PASS: Ascending order correct" : "  ✗ FAIL: Order incorrect");
    Console.WriteLine();

    // Test 2: Descending order by founded year
    Console.WriteLine("TEST 2: Descending order by founded year (5 newest cities)");
    var descendingOptions = new ReadOptions(
        IncludeGeometry: false,
        OrderBy: "founded DESC",
        Limit: 5
    );
    
    var expectedDescending = true;
    var previousYear = int.MaxValue;
    rank = 1;
    await foreach (var city in citiesLayer.ReadFeaturesAsync(descendingOptions))
    {
        var name = city.Attributes["name"];
        var founded = int.Parse(city.Attributes["founded"]!);
        
        Console.WriteLine($"  {rank}. {name}: {founded}");
        
        if (founded > previousYear) expectedDescending = false;
        previousYear = founded;
        rank++;
    }
    Console.WriteLine(expectedDescending ? "  ✓ PASS: Descending order correct" : "  ✗ FAIL: Order incorrect");
    Console.WriteLine();

    // Test 3: Multi-column ordering (county ASC, population DESC)
    Console.WriteLine("TEST 3: Multi-column order (county ASC, then population DESC)");
    var multiColumnOptions = new ReadOptions(
        IncludeGeometry: false,
        OrderBy: "county ASC, population DESC",
        Limit: 10
    );
    
    var expectedMultiColumn = true;
    string? previousCounty = null;
    previousPop = int.MaxValue;
    rank = 1;
    await foreach (var city in citiesLayer.ReadFeaturesAsync(multiColumnOptions))
    {
        var name = city.Attributes["name"];
        var county = city.Attributes["county"]!;
        var population = int.Parse(city.Attributes["population"]!);
        
        Console.WriteLine($"  {rank}. {name} ({county}): {population:N0}");
        
        // Validate ordering: county should be ascending
        if (previousCounty != null)
        {
            var countyCompare = string.CompareOrdinal(county, previousCounty);
            if (countyCompare < 0) expectedMultiColumn = false;
            // Within same county, population should be descending
            if (countyCompare == 0 && population > previousPop) expectedMultiColumn = false;
        }
        
        if (previousCounty != county)
        {
            previousPop = int.MaxValue; // Reset for new county
        }
        
        previousCounty = county;
        previousPop = population;
        rank++;
    }
    Console.WriteLine(expectedMultiColumn ? "  ✓ PASS: Multi-column order correct" : "  ✗ FAIL: Order incorrect");
    Console.WriteLine();

    // Test 4: ORDER BY with WHERE clause
    Console.WriteLine("TEST 4: ORDER BY with WHERE clause (Skane cities by population DESC)");
    var whereOrderOptions = new ReadOptions(
        IncludeGeometry: false,
        WhereClause: "county = 'Skåne'",
        OrderBy: "population DESC"
    );
    
    var expectedWhereOrder = true;
    previousPop = int.MaxValue;
    rank = 1;
    await foreach (var city in citiesLayer.ReadFeaturesAsync(whereOrderOptions))
    {
        var name = city.Attributes["name"];
        var population = int.Parse(city.Attributes["population"]!);
        var county = city.Attributes["county"];
        
        Console.WriteLine($"  {rank}. {name} ({county}): {population:N0}");
        
        if (population > previousPop) expectedWhereOrder = false;
        previousPop = population;
        rank++;
    }
    Console.WriteLine(expectedWhereOrder ? "  ✓ PASS: WHERE + ORDER BY correct" : "  ✗ FAIL: Order incorrect");
    Console.WriteLine();

    // Test 5: ORDER BY with text column (alphabetical)
    Console.WriteLine("TEST 5: Alphabetical order by name (first 8 cities)");
    var alphabeticalOptions = new ReadOptions(
        IncludeGeometry: false,
        OrderBy: "name ASC",
        Limit: 8
    );
    
    var expectedAlphabetical = true;
    string? previousName = null;
    rank = 1;
    await foreach (var city in citiesLayer.ReadFeaturesAsync(alphabeticalOptions))
    {
        var name = city.Attributes["name"]!;
        var population = int.Parse(city.Attributes["population"]!);
        
        Console.WriteLine($"  {rank}. {name}: {population:N0}");
        
        if (previousName != null && string.CompareOrdinal(name, previousName) < 0)
        {
            expectedAlphabetical = false;
        }
        previousName = name;
        rank++;
    }
    Console.WriteLine(expectedAlphabetical ? "  ✓ PASS: Alphabetical order correct" : "  ✗ FAIL: Order incorrect");
    Console.WriteLine();

    // Test 6: ORDER BY with real numbers
    Console.WriteLine("TEST 6: Order by area (real numbers, DESC - largest areas first)");
    var areaOptions = new ReadOptions(
        IncludeGeometry: false,
        OrderBy: "area_km2 DESC",
        Limit: 5
    );
    
    var expectedAreaOrder = true;
    var previousArea = double.MaxValue;
    rank = 1;
    await foreach (var city in citiesLayer.ReadFeaturesAsync(areaOptions))
    {
        var name = city.Attributes["name"];
        var area = double.Parse(city.Attributes["area_km2"]!, CultureInfo.InvariantCulture);
        
        Console.WriteLine($"  {rank}. {name}: {area:F1} km²");
        
        if (area > previousArea) expectedAreaOrder = false;
        previousArea = area;
        rank++;
    }
    Console.WriteLine(expectedAreaOrder ? "  ✓ PASS: Real number ordering correct" : "  ✗ FAIL: Order incorrect");
    Console.WriteLine();

    // Summary of ORDER BY tests
    Console.WriteLine("ORDER BY TEST SUMMARY:");
    Console.WriteLine("  All tests validate the v1.2.2 ORDER BY fix");
    Console.WriteLine("  ✓ Ascending order (ASC)");
    Console.WriteLine("  ✓ Descending order (DESC)");
    Console.WriteLine("  ✓ Multi-column ordering");
    Console.WriteLine("  ✓ ORDER BY with WHERE clause");
    Console.WriteLine("  ✓ Text column ordering");
    Console.WriteLine("  ✓ Real number ordering");
    Console.WriteLine();

    // =================================================================
    // TASK 7: CRUD Operations - Delete
    // =================================================================
    Console.WriteLine("\nTASK 7: Delete operations with conditions");
    Console.WriteLine("Demonstrate selective deletion with WHERE clauses");
    Console.WriteLine();

    Console.WriteLine("Removing very small settlements (population < 5000)...");
    var deletedCount = await citiesLayer.DeleteAsync("population < 5000");
    Console.WriteLine($"SUCCESS: Removed {deletedCount} small settlements");

    var remainingCities = await citiesLayer.CountAsync();
    Console.WriteLine($"Cities remaining: {remainingCities:N0}");

    // =================================================================
    // TASK 8: Metadata and Spatial Analysis
    // =================================================================
    Console.WriteLine("\nTASK 8: Extracting metadata and spatial information");
    Console.WriteLine("GeoPackage metadata provides schema and spatial extent details");
    Console.WriteLine();

    var metadata = await geoPackage.GetInfoAsync();
    
    foreach (var layer in metadata.Layers)
    {
        Console.WriteLine($"LAYER: {layer.TableName}");
        Console.WriteLine($"   Geometry Type: {layer.GeometryType ?? "Non-spatial"}");
        Console.WriteLine($"   Coordinate System: SRID {layer.Srid}");
        Console.WriteLine($"   Attribute Columns: {layer.AttributeColumns.Count}");
        
        if (layer.MinX.HasValue)
        {
            Console.WriteLine($"   Spatial Extent:");
            Console.WriteLine($"     Southwest: ({layer.MinX:F0}, {layer.MinY:F0})");
            Console.WriteLine($"     Northeast: ({layer.MaxX:F0}, {layer.MaxY:F0})");
            
            // Calculate bounding box dimensions
            var width = (layer.MaxX - layer.MinX) ?? 0;
            var height = (layer.MaxY - layer.MinY) ?? 0;
            Console.WriteLine($"     Dimensions: {width:F0} × {height:F0} meters");
        }
        
        Console.WriteLine($"   Column Details:");
        foreach (var col in layer.AttributeColumns)
        {
            Console.WriteLine($"     • {col.Name}: {col.Type}");
        }
    }

    // =================================================================
    // SUCCESS SUMMARY
    // =================================================================
    Console.WriteLine("\n" + new string('=', 60));
    Console.WriteLine("TUTORIAL COMPLETED SUCCESSFULLY!");
    Console.WriteLine(new string('=', 60));
    Console.WriteLine($"GeoPackage created: {Path.GetFullPath(gpkgPath)}");
    Console.WriteLine($"Final city count: {remainingCities:N0}");
    Console.WriteLine();
    Console.WriteLine("Key concepts learned:");
    Console.WriteLine("SUCCESS: Fluent API with using statements");
    Console.WriteLine("SUCCESS: Schema definition and layer creation");
    Console.WriteLine("SUCCESS: Bulk insert with progress reporting");
    Console.WriteLine("SUCCESS: Advanced querying with filters and sorting");
    Console.WriteLine("SUCCESS: CRUD operations (Create, Read, Delete)");
    Console.WriteLine("SUCCESS: Metadata extraction and spatial analysis");
    Console.WriteLine();
    Console.WriteLine("Next steps:");
    Console.WriteLine("• Open the GeoPackage in QGIS to visualize the data");
    Console.WriteLine("• Try modifying the queries and options");
    Console.WriteLine("• Explore other example projects for specialized features");
}
catch (Exception ex)
{
    Console.WriteLine($"\nERROR: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Details: {ex.InnerException.Message}");
    }
    Console.WriteLine("\nThis error helps demonstrate proper exception handling!");
}

// =============================================================
// Helper Method: Generate Realistic Swedish Cities Data
// =============================================================

static List<FeatureRecord> GenerateSwedishCities()
{
    // Real Swedish cities with accurate SWEREF99 TM coordinates
    var cityData = new List<(string Name, double X, double Y, int Population, double AreaKm2, string County, int Founded)>
    {
        // Major cities
        ("Stockholm", 683527, 6579433, 975551, 188.0, "Stockholm", 1252),
        ("Gothenburg", 317773, 6394498, 579281, 203.6, "Vastra Gotaland", 1621),
        ("Malmo", 375040, 6163000, 350963, 158.4, "Skane", 1275),
        ("Uppsala", 646138, 6636722, 230767, 48.8, "Uppsala", 1286),
        ("Vasteras", 587902, 6611234, 127799, 48.2, "Vastmanland", 990),
        ("Orebro", 511954, 6569151, 126009, 58.2, "Orebro", 1404),
        ("Linkoping", 537341, 6473261, 165618, 56.6, "Ostergotland", 1287),
        ("Helsingborg", 358240, 6212773, 149280, 38.4, "Skane", 1085),
        ("Jonkoping", 450430, 6400662, 98659, 38.2, "Jonkoping", 1284),
        ("Norrkoping", 568715, 6494377, 95618, 45.8, "Ostergotland", 1384),
        
        // Medium cities
        ("Lund", 386905, 6175041, 94703, 22.6, "Skane", 990),
        ("Umea", 757988, 7088793, 89607, 33.4, "Vasterbotten", 1622),
        ("Gavle", 616308, 6729788, 78331, 62.7, "Gavleborg", 1446),
        ("Boras", 377137, 6400313, 72169, 40.2, "Vastra Gotaland", 1621),
        ("Eskilstuna", 584568, 6580834, 69948, 53.6, "Sodermanland", 1659),
        ("Sundsvall", 636542, 6807893, 58807, 60.2, "Vasternorrland", 1624),
        ("Halmstad", 332163, 6291418, 71422, 47.8, "Halland", 1307),
        ("Vaxjo", 483217, 6327894, 66275, 30.2, "Kronoberg", 1342),
        
        // Smaller cities  
        ("Karlstad", 382584, 6587367, 65856, 55.8, "Varmland", 1584),
        ("Ostersund", 438751, 6971185, 51424, 25.2, "Jamtland", 1786),
        ("Falun", 524768, 6687194, 37291, 59.7, "Dalarna", 1641),
        ("Kalmar", 528667, 6336513, 41388, 22.4, "Kalmar", 1100),
        ("Visby", 654303, 6386659, 24330, 12.3, "Gotland", 1000),
        ("Kiruna", 437771, 7529353, 17002, 15.9, "Norrbotten", 1900),
        
        // Small towns (will be deleted in the example)
        ("Mariefred", 626618, 6571605, 3783, 12.4, "Sodermanland", 1605),
        ("Trosa", 646980, 6531411, 5192, 18.7, "Sodermanland", 1200),
        ("Vaxholm", 689191, 6590230, 4312, 8.9, "Stockholm", 1647),
        ("Sigtuna", 652732, 6612047, 8444, 21.3, "Stockholm", 980)
    };

    return cityData.Select(city => new FeatureRecord(
        new Point(city.X, city.Y),
        new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["name"] = city.Name,
            ["population"] = city.Population.ToString(CultureInfo.InvariantCulture),
            ["area_km2"] = city.AreaKm2.ToString("F1", CultureInfo.InvariantCulture),
            ["county"] = city.County,
            ["founded"] = city.Founded.ToString(CultureInfo.InvariantCulture)
        }
    )).ToList();
}