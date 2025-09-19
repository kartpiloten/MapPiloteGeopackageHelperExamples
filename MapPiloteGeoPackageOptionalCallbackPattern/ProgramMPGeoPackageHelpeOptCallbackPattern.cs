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
// Optional Callback Pattern Example for MapPiloteGeopackageHelper
// -------------------------------------------------------------
// This pedagogical example demonstrates the Optional Callback Pattern:
//  1) What is the Optional Callback Pattern?
//  2) How MapPiloteGeopackageHelper implements it with IProgress<T>
//  3) Different ways to use optional callbacks
//  4) When to use callbacks vs when to omit them
//  5) Custom callback implementations and scenarios
// Perfect for understanding modern .NET callback design patterns!
// =============================================================

Console.WriteLine("=== Optional Callback Pattern Tutorial ===");
Console.WriteLine("Understanding IProgress<T> and optional callbacks in MapPiloteGeopackageHelper");
Console.WriteLine();

// Clean up any existing files
const string gpkgPath = "OptionalCallbackExample.gpkg";
const int srid = 3006;
TryDelete(gpkgPath);

Console.WriteLine("## What is the Optional Callback Pattern?");
Console.WriteLine("The Optional Callback Pattern allows methods to accept an optional");
Console.WriteLine("callback parameter that callers can provide to receive notifications");
Console.WriteLine("about progress, status, or intermediate results during long operations.");
Console.WriteLine();
Console.WriteLine("Benefits:");
Console.WriteLine("- Callers can choose to monitor progress or ignore it");
Console.WriteLine("- No performance penalty when callbacks aren't provided");
Console.WriteLine("- Enables responsive UI updates during long operations");
Console.WriteLine("- Follows the .NET IProgress<T> standard pattern");
Console.WriteLine();

try
{
    using var geoPackage = await GeoPackage.OpenAsync(gpkgPath, srid);
    var schema = new Dictionary<string, string>
    {
        ["name"] = "TEXT",
        ["category"] = "TEXT",
        ["value"] = "REAL"
    };

    // ========================================================================
    // SCENARIO 1: No Callback (Silent Operation)
    // ========================================================================
    Console.WriteLine("## Scenario 1: No Progress Callback (Silent Operation)");
    Console.WriteLine("Many operations work fine without progress monitoring:");
    Console.WriteLine();

    var layer1 = await geoPackage.EnsureLayerAsync("scenario1_points", schema, srid);
    var smallDataset = GenerateSampleData(100);

    Console.WriteLine("Inserting 100 points without progress callback...");
    
    // Note: progress parameter is null - this is the Optional Callback Pattern in action!
    await layer1.BulkInsertAsync(
        smallDataset,
        new BulkInsertOptions(BatchSize: 50),
        progress: null  // <-- Optional callback omitted
    );
    
    Console.WriteLine("SUCCESS: Completed silently - no progress updates needed for small datasets");
    Console.WriteLine();

    // ========================================================================
    // SCENARIO 2: Simple Progress Callback
    // ========================================================================
    Console.WriteLine("## Scenario 2: Simple Progress Callback");
    Console.WriteLine("For longer operations, a simple progress callback helps:");
    Console.WriteLine();

    var layer2 = await geoPackage.EnsureLayerAsync("scenario2_points", schema, srid);
    var mediumDataset = GenerateSampleData(500);

    Console.WriteLine("Inserting 500 points with simple progress callback...");
    
    // Simple callback that just shows percentage
    var simpleProgress = new Progress<BulkProgress>(p => 
        Console.WriteLine($"  Progress: {p.PercentComplete:F1}% ({p.Processed}/{p.Total})")
    );

    await layer2.BulkInsertAsync(
        mediumDataset,
        new BulkInsertOptions(BatchSize: 100),
        simpleProgress  // <-- Simple callback provided
    );
    
    Console.WriteLine("SUCCESS: Completed with basic progress tracking");
    Console.WriteLine();

    // ========================================================================
    // SCENARIO 3: Advanced Progress Callback with Visual Indicator
    // ========================================================================
    Console.WriteLine("## Scenario 3: Advanced Progress Callback with Visual Progress Bar");
    Console.WriteLine("For large operations, rich progress visualization improves UX:");
    Console.WriteLine();

    var layer3 = await geoPackage.EnsureLayerAsync("scenario3_points", schema, srid);
    var largeDataset = GenerateSampleData(100000);
    
    Console.WriteLine("Inserting 100000 points with visual progress bar...");
    
    // Advanced callback with progress bar and ETA
    var startTime = DateTime.Now;
    var advancedProgress = new Progress<BulkProgress>(p =>
    {
        // Create visual progress bar
        var barWidth = 30;
        var filledWidth = (int)(p.PercentComplete / 100.0 * barWidth);
        var emptyWidth = barWidth - filledWidth;
        var bar = new string('#', filledWidth) + new string('.', emptyWidth);
        
        // Calculate ETA
        var elapsed = DateTime.Now - startTime;
        var eta = p.PercentComplete > 0 
            ? TimeSpan.FromMilliseconds(elapsed.TotalMilliseconds * (100.0 - p.PercentComplete) / p.PercentComplete)
            : TimeSpan.Zero;
        
        // Rich progress display (carriage return overwrites the line)
        Console.Write($"\r  [{bar}] {p.PercentComplete:F1}% ({p.Processed:N0}/{p.Total:N0}) ETA: {eta:mm\\:ss}");
        
        // Newline when complete
        if (p.IsComplete)
        {
            Console.WriteLine();
        }
    });

    await layer3.BulkInsertAsync(
        largeDataset,
        new BulkInsertOptions(BatchSize: 250, CreateSpatialIndex: true),
        advancedProgress  // <-- Rich callback provided
    );
    
    Console.WriteLine("SUCCESS: Completed with rich progress visualization");
    Console.WriteLine();

    // ========================================================================
    // SCENARIO 4: Custom Callback with Business Logic
    // ========================================================================
    Console.WriteLine("## Scenario 4: Custom Callback with Business Logic");
    Console.WriteLine("Callbacks can trigger business logic, not just UI updates:");
    Console.WriteLine();

    var layer4 = await geoPackage.EnsureLayerAsync("scenario4_points", schema, srid);
    var finalDataset = GenerateSampleData(75000);
    
    Console.WriteLine("Inserting 750 points with custom business logic callback...");
    
    var milestones = new[] { 25, 50, 75, 90, 100 };
    var achievedMilestones = new HashSet<int>();
    
    var businessLogicProgress = new Progress<BulkProgress>(p =>
    {
        // Check for milestone achievements - this now correctly tracks the current batch only
        foreach (var milestone in milestones)
        {
            if (p.PercentComplete >= milestone && !achievedMilestones.Contains(milestone))
            {
                achievedMilestones.Add(milestone);
                Console.WriteLine($"  *** MILESTONE: {milestone}% completed! ({p.Processed:N0} of {p.Total:N0} records processed) ***");
                
                // Custom business logic could happen here:
                // - Send notifications
                // - Update databases  
                // - Trigger other processes
                // - Log to monitoring systems
            }
        }
        
        // Regular progress update - only show key milestones to reduce noise
        if (p.Processed % 150 == 0 || p.IsComplete)
        {
            Console.WriteLine($"  Status: {p.Processed:N0}/{p.Total:N0} records ({p.PercentComplete:F1}%)");
        }
    });

    await layer4.BulkInsertAsync(
        finalDataset,
        new BulkInsertOptions(BatchSize: 150),
        businessLogicProgress  // <-- Business logic callback
    );
    
    Console.WriteLine("SUCCESS: Completed with milestone tracking and business logic");
    Console.WriteLine();

    // ========================================================================
    // SCENARIO 5: Conditional Callback (Runtime Decision)
    // ========================================================================
    Console.WriteLine("## Scenario 5: Conditional Callback (Runtime Decision)");
    Console.WriteLine("Sometimes you decide at runtime whether to use a callback:");
    Console.WriteLine();

    var layer5 = await geoPackage.EnsureLayerAsync("scenario5_points", schema, srid);
    var conditionalDataset = GenerateSampleData(300);
    
    // Simulate runtime decision (e.g., based on user preferences, dataset size, etc.)
    bool enableProgressTracking = conditionalDataset.Count > 250; // Decision based on size
    
    Console.WriteLine($"Dataset size: {conditionalDataset.Count} records");
    Console.WriteLine($"Progress tracking: {(enableProgressTracking ? "ENABLED" : "DISABLED")}");
    Console.WriteLine();
    
    // Optional callback pattern: pass null or actual callback based on condition
    IProgress<BulkProgress>? conditionalProgress = enableProgressTracking 
        ? new Progress<BulkProgress>(p => Console.WriteLine($"  Conditional progress: {p.PercentComplete:F0}% ({p.Processed}/{p.Total})"))
        : null;

    Console.WriteLine("Inserting with conditional progress callback...");
    
    await layer5.BulkInsertAsync(
        conditionalDataset,
        new BulkInsertOptions(BatchSize: 100),
        conditionalProgress  // <-- null or callback based on runtime condition
    );
    
    Console.WriteLine("SUCCESS: Completed with conditional callback logic");
    Console.WriteLine();

    // ========================================================================
    // SUMMARY: Benefits of Optional Callback Pattern
    // ========================================================================
    Console.WriteLine("## Summary: Benefits of Optional Callback Pattern");
    Console.WriteLine();
    Console.WriteLine("FLEXIBILITY: Callers choose their level of monitoring");
    Console.WriteLine("PERFORMANCE: No overhead when callbacks aren't needed");
    Console.WriteLine("USABILITY: Simple operations stay simple, complex ones get rich features");
    Console.WriteLine("STANDARDS: Follows .NET IProgress<T> conventions");
    Console.WriteLine("TESTING: Easy to test with mock callbacks or null");
    Console.WriteLine("UI RESPONSIVE: Enables responsive UI during long operations");
    Console.WriteLine();
    
    // Show final statistics across all layers
    var totalCount1 = await layer1.CountAsync();
    var totalCount2 = await layer2.CountAsync();
    var totalCount3 = await layer3.CountAsync();
    var totalCount4 = await layer4.CountAsync();
    var totalCount5 = await layer5.CountAsync();
    var grandTotal = totalCount1 + totalCount2 + totalCount3 + totalCount4 + totalCount5;
    
    Console.WriteLine($"Records inserted per scenario:");
    Console.WriteLine($"  Scenario 1 (silent):     {totalCount1:N0} records");
    Console.WriteLine($"  Scenario 2 (simple):     {totalCount2:N0} records");
    Console.WriteLine($"  Scenario 3 (visual):     {totalCount3:N0} records");
    Console.WriteLine($"  Scenario 4 (business):   {totalCount4:N0} records");
    Console.WriteLine($"  Scenario 5 (conditional): {totalCount5:N0} records");
    Console.WriteLine($"  TOTAL across all scenarios: {grandTotal:N0} records");
    Console.WriteLine();
    Console.WriteLine($"GeoPackage saved to: {Path.GetFullPath(gpkgPath)}");
    Console.WriteLine();
    Console.WriteLine("Tutorial completed! You now understand the Optional Callback Pattern.");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
    }
}

// =============================================================
// HELPER METHODS
// =============================================================

static List<FeatureRecord> GenerateSampleData(int count)
{
    var random = new Random(42); // Fixed seed for reproducible results
    var categories = new[] { "School", "Hospital", "Park", "Shop", "Office" };
    var features = new List<FeatureRecord>();

    // Generate points in a grid pattern around Stockholm
    const double baseX = 650000; // SWEREF99 TM coordinates around Stockholm
    const double baseY = 6580000;
    const double spread = 50000; // 50km spread

    for (int i = 0; i < count; i++)
    {
        var x = baseX + random.NextDouble() * spread - spread / 2;
        var y = baseY + random.NextDouble() * spread - spread / 2;
        var point = new Point(x, y);

        var attributes = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["name"] = $"Location_{i + 1:D4}",
            ["category"] = categories[random.Next(categories.Length)],
            ["value"] = (random.NextDouble() * 100).ToString("F2", CultureInfo.InvariantCulture)
        };

        features.Add(new FeatureRecord(point, attributes));
    }

    return features;
}

static void TryDelete(string path)
{
    try 
    { 
        if (File.Exists(path)) 
            File.Delete(path); 
    } 
    catch 
    { 
        // Ignore deletion errors
    }
}
