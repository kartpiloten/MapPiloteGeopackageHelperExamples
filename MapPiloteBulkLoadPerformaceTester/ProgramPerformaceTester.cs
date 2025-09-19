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
using System.Diagnostics;
using System.Globalization;

// =============================================================
// Bulk Insert Performance Comparison Tutorial
// -------------------------------------------------------------
// This tutorial demonstrates performance differences between:
//  1) Single-row inserts (traditional approach)
//  2) Bulk inserts (modern efficient approach)
// Learn when and why to use bulk operations for better performance!
// =============================================================

Console.WriteLine("=== Bulk Insert Performance Tutorial ===");
Console.WriteLine("Comparing single-row vs bulk insert performance with MapPiloteGeopackageHelper");
Console.WriteLine();

const int TEST_COUNT = 2000; // Manageable size for demonstration
const int SRID = 3006; // SWEREF99 TM (Sweden)
const string LAYER_NAME = "test_points";

// Output paths
string singleInsertPath = Path.Combine(Environment.CurrentDirectory, "SingleInsert_Performance.gpkg");
string bulkInsertPath = Path.Combine(Environment.CurrentDirectory, "BulkInsert_Performance.gpkg");

// Clean up any existing test files
TryDelete(singleInsertPath);
TryDelete(bulkInsertPath);

Console.WriteLine("=== TUTORIAL OVERVIEW ===");
Console.WriteLine("We'll create identical datasets using two different approaches:");
Console.WriteLine($"• Dataset size: {TEST_COUNT:N0} points");
Console.WriteLine($"• Coordinate system: SWEREF99 TM (SRID {SRID})");
Console.WriteLine($"• Attributes: name, category, value, notes");
Console.WriteLine();

// Define schema for test data
var schema = new Dictionary<string, string>(StringComparer.Ordinal)
{
    ["name"] = "TEXT",
    ["category"] = "TEXT", 
    ["value"] = "REAL",
    ["notes"] = "TEXT"
};

// Generate test dataset
Console.WriteLine("1. GENERATING TEST DATA");
Console.WriteLine($"   Creating {TEST_COUNT:N0} random points across Sweden...");
var testFeatures = GenerateTestFeatures(TEST_COUNT);
Console.WriteLine($"   SUCCESS: Generated {testFeatures.Count:N0} features");
Console.WriteLine();

// =================================================================
// METHOD 1: SINGLE-ROW INSERTS (Traditional Approach)
// =================================================================
Console.WriteLine("2. METHOD 1: SINGLE-ROW INSERTS");
Console.WriteLine("   Traditional approach - inserting one feature at a time");
Console.WriteLine("   Each insert is a separate database transaction");
Console.WriteLine();

CreateEmptyGeoPackage(singleInsertPath, LAYER_NAME, schema);

var stopwatch = Stopwatch.StartNew();
int progress = 0;

Console.WriteLine("   Inserting features (one by one):");
foreach (var feature in testFeatures)
{
    // Extract attributes in the correct order for the traditional API
    var point = feature.Geometry as Point;
    var attributeValues = new[]
    {
        feature.Attributes.GetValueOrDefault("name") ?? "",
        feature.Attributes.GetValueOrDefault("category") ?? "",
        feature.Attributes.GetValueOrDefault("value") ?? "",
        feature.Attributes.GetValueOrDefault("notes") ?? ""
    };

    // Single insert - one database transaction per feature
    CGeopackageAddDataHelper.AddPointToGeoPackage(singleInsertPath, LAYER_NAME, point!, attributeValues);

    progress++;
    if (progress % 400 == 0)
    {
        var percentage = (double)progress / TEST_COUNT * 100;
        Console.Write($"\r     Progress: {progress:N0}/{TEST_COUNT:N0} ({percentage:F0}%) ");
    }
}
stopwatch.Stop();
Console.WriteLine($"\r     SUCCESS: Completed {TEST_COUNT:N0} single inserts");

var singleInsertTime = stopwatch.ElapsedMilliseconds;
var singleInsertRate = TEST_COUNT / Math.Max(stopwatch.Elapsed.TotalSeconds, 0.001);
var singleInsertSize = new FileInfo(singleInsertPath).Length;

Console.WriteLine($"   Results:");
Console.WriteLine($"     Time: {singleInsertTime:N0} ms");
Console.WriteLine($"     Rate: {singleInsertRate:F0} inserts/second");
Console.WriteLine($"     File size: {singleInsertSize / 1024.0:F1} KB");
Console.WriteLine();

// =================================================================  
// METHOD 2: BULK INSERTS (Modern Efficient Approach)
// =================================================================
Console.WriteLine("3. METHOD 2: BULK INSERTS");
Console.WriteLine("   Modern approach - inserting many features in batches");
Console.WriteLine("   Optimized with transactions and prepared statements");
Console.WriteLine();

CreateEmptyGeoPackage(bulkInsertPath, LAYER_NAME, schema);

Console.WriteLine("   Bulk inserting all features at once:");
stopwatch.Restart();

// Bulk insert - optimized for performance
CGeopackageAddDataHelper.BulkInsertFeatures(
    geoPackagePath: bulkInsertPath,
    layerName: LAYER_NAME,
    features: testFeatures,
    srid: SRID,
    batchSize: 500  // Process in batches of 500 for optimal performance
);

stopwatch.Stop();
Console.WriteLine($"   SUCCESS: Bulk insert completed");

var bulkInsertTime = stopwatch.ElapsedMilliseconds;
var bulkInsertRate = TEST_COUNT / Math.Max(stopwatch.Elapsed.TotalSeconds, 0.001);
var bulkInsertSize = new FileInfo(bulkInsertPath).Length;

Console.WriteLine($"   Results:");
Console.WriteLine($"     Time: {bulkInsertTime:N0} ms");
Console.WriteLine($"     Rate: {bulkInsertRate:F0} inserts/second");
Console.WriteLine($"     File size: {bulkInsertSize / 1024.0:F1} KB");
Console.WriteLine();

// =================================================================
// PERFORMANCE ANALYSIS
// =================================================================
Console.WriteLine("4. PERFORMANCE ANALYSIS");
Console.WriteLine("   Comparing the two approaches:");
Console.WriteLine();

Console.WriteLine($"DETAILED COMPARISON:");
Console.WriteLine($"   Single-row approach:");
Console.WriteLine($"     • Time: {singleInsertTime:N0} ms");
Console.WriteLine($"     • Rate: {singleInsertRate:F0} records/second");
Console.WriteLine($"     • File: {singleInsertSize / 1024.0:F1} KB");
Console.WriteLine();
Console.WriteLine($"   Bulk insert approach:");
Console.WriteLine($"     • Time: {bulkInsertTime:N0} ms");
Console.WriteLine($"     • Rate: {bulkInsertRate:F0} records/second");
Console.WriteLine($"     • File: {bulkInsertSize / 1024.0:F1} KB");
Console.WriteLine();

if (singleInsertTime > 0 && bulkInsertTime > 0)
{
    var timeImprovement = (double)singleInsertTime / bulkInsertTime;
    var rateImprovement = bulkInsertRate / singleInsertRate;
    
    Console.WriteLine($"PERFORMANCE IMPROVEMENT:");
    Console.WriteLine($"   • Time: {timeImprovement:F1}x faster");
    Console.WriteLine($"   • Throughput: {rateImprovement:F1}x higher rate");
    
    if (timeImprovement > 2.0)
        Console.WriteLine($"   • Result: Bulk insert is SIGNIFICANTLY faster!");
    else if (timeImprovement > 1.5)  
        Console.WriteLine($"   • Result: Bulk insert shows good improvement");
    else
        Console.WriteLine($"   • Result: Similar performance (dataset may be too small)");
}

Console.WriteLine();
Console.WriteLine("KEY TAKEAWAYS:");
Console.WriteLine("   • Use BulkInsertFeatures() for inserting multiple records");
Console.WriteLine("   • Bulk operations use database transactions efficiently");
Console.WriteLine("   • Performance gains increase with larger datasets");
Console.WriteLine("   • BatchSize parameter can be tuned for your system");
Console.WriteLine();

Console.WriteLine($"FILES CREATED:");
Console.WriteLine($"   • {Path.GetFileName(singleInsertPath)} (single-row method)");
Console.WriteLine($"   • {Path.GetFileName(bulkInsertPath)} (bulk insert method)");
Console.WriteLine();
Console.WriteLine("RECOMMENDATION: Always use bulk operations for multiple inserts!");

// =================================================================
// Helper Methods
// =================================================================

static List<FeatureRecord> GenerateTestFeatures(int count)
{
    var random = new Random(42); // Fixed seed for reproducible tests
    var features = new List<FeatureRecord>();
    
    // Swedish coordinate bounds (SWEREF99 TM)
    const double minX = 250000.0, minY = 6200000.0;
    const double maxX = 850000.0, maxY = 7400000.0;
    
    var categories = new[] { "Residential", "Commercial", "Industrial", "Agricultural", "Forest" };
    var notes = new[] { "High priority", "Standard", "Low priority", null }; // Include nulls
    
    for (int i = 0; i < count; i++)
    {
        // Random coordinates within Sweden
        var x = minX + random.NextDouble() * (maxX - minX);
        var y = minY + random.NextDouble() * (maxY - minY);
        var point = new Point(x, y);
        
        // Generate realistic attributes
        var name = $"Point_{i + 1:D5}";
        var category = categories[random.Next(categories.Length)];
        var value = Math.Round(random.NextDouble() * 1000, 2);
        var note = notes[random.Next(notes.Length)];
        
        var attributes = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["name"] = name,
            ["category"] = category,
            ["value"] = value.ToString("F2", CultureInfo.InvariantCulture),
            ["notes"] = note
        };
        
        features.Add(new FeatureRecord(point, attributes));
    }
    
    return features;
}

static void CreateEmptyGeoPackage(string path, string layerName, Dictionary<string, string> schema)
{
    // Create the GeoPackage file and layer structure
    CMPGeopackageCreateHelper.CreateGeoPackage(path, SRID);
    GeopackageLayerCreateHelper.CreateGeopackageLayer(path, layerName, schema, "POINT", SRID);
}

static void TryDelete(string path)
{
    try { if (File.Exists(path)) File.Delete(path); } catch { /* Ignore cleanup errors */ }
}
