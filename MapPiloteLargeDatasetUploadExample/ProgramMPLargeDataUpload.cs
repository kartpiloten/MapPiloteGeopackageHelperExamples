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

// =============================================================
// Large Dataset Spatial Index Performance Example
// -------------------------------------------------------------
// This example demonstrates the performance benefits of spatial indexing:
//  1) Generate a large dataset of points within Sweden
//  2) Create GeoPackages with and without spatial indexes
//  3) Compare query performance using spatial filtering
//  4) Show the dramatic performance improvement with indexing
// Perfect for understanding when and why to use spatial indexes!
// =============================================================

Console.WriteLine("=== Large Dataset Spatial Index Performance Example ===");
Console.WriteLine("Demonstrating spatial index benefits with MapPiloteGeopackageHelper");
Console.WriteLine();

const int POINT_COUNT = 100000; // Adjustable for your system
const int SRID = 3006; // SWEREF99 TM (Sweden)
const double BUFFER_DISTANCE = 50000.0; // 50km buffer for queries

string workDir = Environment.CurrentDirectory;
string gpkgWithoutIndex = Path.Combine(workDir, "LargeDataset_NoIndex.gpkg");
string gpkgWithIndex = Path.Combine(workDir, "LargeDataset_WithIndex.gpkg");

// Clean up existing files
TryDelete(gpkgWithoutIndex);
TryDelete(gpkgWithIndex);

// Generate realistic test data
Console.WriteLine($"1. Generating {POINT_COUNT:N0} random monitoring stations across Sweden...");
var monitoringStations = GenerateMonitoringStations(POINT_COUNT);
Console.WriteLine($"   Generated {monitoringStations.Count:N0} stations");

// Define schema for air quality monitoring stations
var schema = new Dictionary<string, string>
{
    ["station_id"] = "TEXT",
    ["station_name"] = "TEXT", 
    ["measurement_type"] = "TEXT",
    ["value"] = "REAL"
};

Console.WriteLine("\n2. Creating GeoPackage WITHOUT spatial index...");
var stopwatch = Stopwatch.StartNew();
using (var geoPackage = await GeoPackage.OpenAsync(gpkgWithoutIndex, SRID))
{
    var layer = await geoPackage.EnsureLayerAsync("monitoring_stations", schema, SRID);
    
    // Bulk insert WITHOUT spatial index
    await layer.BulkInsertAsync(
        monitoringStations,
        new BulkInsertOptions(BatchSize: 1000, CreateSpatialIndex: false),
        new Progress<BulkProgress>(p => {
            if (p.Processed % 2000 == 0 || p.IsComplete)
                Console.WriteLine($"   Progress: {p.Processed:N0}/{p.Total:N0} ({p.PercentComplete:F1}%)");
        }));
}
stopwatch.Stop();
Console.WriteLine($"   Completed in {stopwatch.ElapsedMilliseconds:N0} ms");

Console.WriteLine("\n3. Creating GeoPackage WITH spatial index...");
stopwatch.Restart();
using (var geoPackage = await GeoPackage.OpenAsync(gpkgWithIndex, SRID))
{
    var layer = await geoPackage.EnsureLayerAsync("monitoring_stations", schema, SRID);
    
    // Bulk insert WITH spatial index - MapPiloteGeopackageHelper handles the indexing
    await layer.BulkInsertAsync(
        monitoringStations,
        new BulkInsertOptions(BatchSize: 1000, CreateSpatialIndex: true),
        new Progress<BulkProgress>(p => {
            if (p.Processed % 2000 == 0 || p.IsComplete)
                Console.WriteLine($"   Progress: {p.Processed:N0}/{p.Total:N0} ({p.PercentComplete:F1}%)");
        }));
}
stopwatch.Stop();
Console.WriteLine($"   Completed in {stopwatch.ElapsedMilliseconds:N0} ms (includes index creation)");

// Performance test: spatial query around Stockholm
Console.WriteLine("\n4. Performance Test: Finding stations within 50km of Stockholm...");
var stockholmCenter = new Point(683527, 6579433); // Stockholm coordinates
var queryBuffer = stockholmCenter.Buffer(BUFFER_DISTANCE);

Console.WriteLine($"   Query area: {BUFFER_DISTANCE/1000:F0}km radius around Stockholm");
Console.WriteLine($"   Buffer covers approximately {queryBuffer.Area / 1000000:F0} km²");

// Test WITHOUT spatial index
Console.WriteLine("\n   Testing WITHOUT spatial index...");
var resultsWithoutIndex = await QueryNearbyStations(gpkgWithoutIndex, queryBuffer);

// Test WITH spatial index  
Console.WriteLine("\n   Testing WITH spatial index...");
var resultsWithIndex = await QueryNearbyStations(gpkgWithIndex, queryBuffer);

// Compare results
Console.WriteLine("\n=== PERFORMANCE COMPARISON ===");
Console.WriteLine($"Stations found (no index):  {resultsWithoutIndex.Count:N0} in {resultsWithoutIndex.QueryTime:N0} ms");
Console.WriteLine($"Stations found (with index): {resultsWithIndex.Count:N0} in {resultsWithIndex.QueryTime:N0} ms");

if (resultsWithoutIndex.QueryTime > 0 && resultsWithIndex.QueryTime > 0)
{
    var speedup = (double)resultsWithoutIndex.QueryTime / resultsWithIndex.QueryTime;
    Console.WriteLine($"Performance improvement:     {speedup:F1}x faster with spatial index");
}

var validation = resultsWithoutIndex.Count == resultsWithIndex.Count ? "PASSED" : "FAILED";
Console.WriteLine($"Result validation:           {validation}");

Console.WriteLine($"\nFiles created:");
Console.WriteLine($"  - {Path.GetFileName(gpkgWithoutIndex)} (no spatial index)");
Console.WriteLine($"  - {Path.GetFileName(gpkgWithIndex)} (with spatial index)");
Console.WriteLine("\nKey takeaway: Spatial indexes dramatically improve query performance on large datasets!");
Console.WriteLine("Use CreateSpatialIndex: true in BulkInsertOptions for better performance.");

// =============================================================
// Helper Methods
// =============================================================

static List<FeatureRecord> GenerateMonitoringStations(int count)
{
    var random = new Random(42); // Fixed seed for reproducible results
    var stations = new List<FeatureRecord>();
    
    // Swedish national grid extent (SWEREF99 TM)
    const double minX = 200000.0, minY = 6100000.0;
    const double maxX = 900000.0, maxY = 7600000.0;
    
    var measurementTypes = new[] { "PM2.5", "NO2", "O3", "SO2", "CO" };
    var prefixes = new[] { "Urban", "Rural", "Industrial", "Traffic", "Background" };
    
    for (int i = 0; i < count; i++)
    {
        var x = minX + random.NextDouble() * (maxX - minX);
        var y = minY + random.NextDouble() * (maxY - minY);
        var point = new Point(x, y);
        
        var stationId = $"SE{i+1:D5}";
        var prefix = prefixes[random.Next(prefixes.Length)];
        var stationName = $"{prefix} Station {i+1}";
        var measurementType = measurementTypes[random.Next(measurementTypes.Length)];
        var value = Math.Round(random.NextDouble() * 100, 2);
        
        var attributes = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["station_id"] = stationId,
            ["station_name"] = stationName,
            ["measurement_type"] = measurementType,
            ["value"] = value.ToString("F2")
        };
        
        stations.Add(new FeatureRecord(point, attributes));
    }
    
    return stations;
}

static async Task<(int Count, long QueryTime)> QueryNearbyStations(string geoPackagePath, Geometry queryArea)
{
    var stopwatch = Stopwatch.StartNew();
    var results = new List<FeatureRecord>();
    
    // Use MapPiloteGeopackageHelper's built-in spatial query capabilities
    var allStations = CMPGeopackageReadDataHelper.ReadFeatures(
        geoPackageFilePath: geoPackagePath, 
        tableName: "monitoring_stations",
        includeGeometry: true);
    
    foreach (var station in allStations)
    {
        if (station.Geometry != null && queryArea.Contains(station.Geometry))
        {
            results.Add(station);
        }
    }
    
    stopwatch.Stop();
    
    Console.WriteLine($"     Found {results.Count:N0} stations in {stopwatch.ElapsedMilliseconds:N0} ms");
    
    return (results.Count, stopwatch.ElapsedMilliseconds);
}

static void TryDelete(string path)
{
    try { if (File.Exists(path)) File.Delete(path); } catch { }
}