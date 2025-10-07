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
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Globalization;

// =============================================================
// SQLite Journal Mode Tutorial - WAL vs DELETE Mode
// -------------------------------------------------------------
// This educational tutorial explains SQLite journal modes:
//  1) What are SQLite journal modes and why they matter
//  2) DELETE mode (traditional) - how it works
//  3) WAL mode (Write-Ahead Logging) - modern approach
//  4) Performance comparison in different scenarios
//  5) Concurrency benefits and limitations
//  6) File system behavior and auxiliary files
//  7) Production recommendations and trade-offs
// Perfect for understanding SQLite behavior in GeoPackages!
// =============================================================

Console.WriteLine("=== SQLite Journal Mode Tutorial - WAL vs DELETE ===");
Console.WriteLine("Understanding how SQLite manages transactions and concurrent access");
Console.WriteLine();

const int SRID = 3006; // SWEREF99 TM (Sweden)
const int TEST_RECORDS = 1000; // Manageable size for demonstration

// File paths
string deleteModePath = Path.Combine(Environment.CurrentDirectory, "DeleteMode_Example.gpkg");
string walModePath = Path.Combine(Environment.CurrentDirectory, "WALMode_Example.gpkg");

// Clean up any existing files
CleanupFiles(deleteModePath, walModePath);

Console.WriteLine("=== EDUCATIONAL OVERVIEW ===");
Console.WriteLine("SQLite supports different journal modes that control how transactions are handled:");
Console.WriteLine("• DELETE mode: Traditional rollback journaling (default)");
Console.WriteLine("• WAL mode: Write-Ahead Logging for better concurrency");
Console.WriteLine("• Each mode has different performance and concurrency characteristics");
Console.WriteLine();

try
{
    // =================================================================
    // SECTION 1: Understanding DELETE Mode (Traditional)
    // =================================================================
    Console.WriteLine("SECTION 1: DELETE Mode (Traditional Rollback Journal)");
    Console.WriteLine("This is SQLite's default mode, used in most applications");
    Console.WriteLine();

    Console.WriteLine("DELETE Mode Characteristics:");
    Console.WriteLine("• Creates temporary journal files during transactions (.gpkg-journal)");
    Console.WriteLine("• Only one writer allowed at a time");
    Console.WriteLine("• Readers block writers and vice versa");
    Console.WriteLine("• Smaller disk footprint");
    Console.WriteLine("• Simpler file management");
    Console.WriteLine();

    Console.WriteLine("Creating GeoPackage in DELETE mode...");
    await CreateAndPopulateGeoPackage(deleteModePath, "DELETE", TEST_RECORDS);
    
    await DemonstrateJournalMode(deleteModePath, "DELETE");
    Console.WriteLine();

    // =================================================================
    // SECTION 2: Understanding WAL Mode (Write-Ahead Logging)
    // =================================================================
    Console.WriteLine("SECTION 2: WAL Mode (Write-Ahead Logging)");
    Console.WriteLine("Modern SQLite mode designed for better concurrency");
    Console.WriteLine();

    Console.WriteLine("WAL Mode Characteristics:");
    Console.WriteLine("• Creates persistent WAL and SHM files (.gpkg-wal, .gpkg-shm)");
    Console.WriteLine("• Multiple readers can run concurrently with writers");
    Console.WriteLine("• Better performance for read-heavy workloads");
    Console.WriteLine("• Larger disk footprint due to auxiliary files");
    Console.WriteLine("• More complex file management");
    Console.WriteLine();

    Console.WriteLine("Creating GeoPackage in WAL mode...");
    await CreateAndPopulateGeoPackage(walModePath, "WAL", TEST_RECORDS);
    
    await DemonstrateJournalMode(walModePath, "WAL");
    Console.WriteLine();

    // =================================================================
    // SECTION 3: Performance Comparison
    // =================================================================
    Console.WriteLine("SECTION 3: Performance Comparison");
    Console.WriteLine("Comparing write and read performance between modes");
    Console.WriteLine();

    await PerformWritePerformanceTest(deleteModePath, walModePath);
    await PerformReadPerformanceTest(deleteModePath, walModePath);

    // =================================================================
    // SECTION 4: File System Behavior
    // =================================================================
    Console.WriteLine("SECTION 4: File System Behavior");
    Console.WriteLine("Examining auxiliary files created by each mode");
    Console.WriteLine();

    AnalyzeFileSystemBehavior(deleteModePath, walModePath);

    // =================================================================
    // SECTION 5: Concurrency Demonstration
    // =================================================================
    Console.WriteLine("SECTION 5: Concurrency Capabilities");
    Console.WriteLine("Demonstrating concurrent access patterns");
    Console.WriteLine();

    await DemonstrateConcurrency(deleteModePath, walModePath);

    // =================================================================
    // SECTION 6: Production Recommendations
    // =================================================================
    Console.WriteLine("SECTION 6: Production Recommendations");
    Console.WriteLine("When to use each mode in real applications");
    Console.WriteLine();

    DisplayProductionGuidance();

    // =================================================================
    // SUMMARY
    // =================================================================
    Console.WriteLine("=== TUTORIAL SUMMARY ===");
    Console.WriteLine("Key takeaways about SQLite journal modes:");
    Console.WriteLine();
    Console.WriteLine("DELETE Mode (Traditional):");
    Console.WriteLine("SUCCESS: Simple, reliable, smaller disk footprint");
    Console.WriteLine("SUCCESS: Perfect for single-user applications");
    Console.WriteLine("LIMITATION: Limited concurrency, readers block writers");
    Console.WriteLine();
    Console.WriteLine("WAL Mode (Write-Ahead Logging):");
    Console.WriteLine("SUCCESS: Better concurrency, readers don't block writers");
    Console.WriteLine("SUCCESS: Excellent for read-heavy multi-user scenarios");
    Console.WriteLine("LIMITATION: More complex, additional auxiliary files");
    Console.WriteLine();
    Console.WriteLine("Files created for examination:");
    Console.WriteLine($"• {Path.GetFileName(deleteModePath)} (DELETE mode)");
    Console.WriteLine($"• {Path.GetFileName(walModePath)} (WAL mode + auxiliary files)");
    Console.WriteLine();
    Console.WriteLine("You can examine these files to see the differences!");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Details: {ex.InnerException.Message}");
    }
}

// =============================================================
// EDUCATIONAL HELPER METHODS
// =============================================================

static async Task CreateAndPopulateGeoPackage(string path, string mode, int recordCount)
{
    var schema = new Dictionary<string, string>
    {
        ["name"] = "TEXT",
        ["category"] = "TEXT",
        ["measurement"] = "REAL",
        ["timestamp"] = "TEXT"
    };

    // Create using traditional API to control journal mode
    CMPGeopackageCreateHelper.CreateGeoPackage(path, SRID);
    GeopackageLayerCreateHelper.CreateGeopackageLayer(path, "sensor_data", schema, "POINT", SRID);

    // Set journal mode using direct SQLite connection
    using var connection = new SqliteConnection($"Data Source={path}");
    await connection.OpenAsync();
    
    using var command = new SqliteCommand($"PRAGMA journal_mode = {mode}", connection);
    var result = await command.ExecuteScalarAsync();
    Console.WriteLine($"   Journal mode set to: {result}");

    // Generate and insert test data
    var testData = GenerateSensorData(recordCount);
    
    var stopwatch = Stopwatch.StartNew();
    CGeopackageAddDataHelper.BulkInsertFeatures(path, "sensor_data", testData, SRID, 100);
    stopwatch.Stop();
    
    Console.WriteLine($"   Inserted {recordCount:N0} records in {stopwatch.ElapsedMilliseconds:N0} ms");
}

static async Task DemonstrateJournalMode(string path, string expectedMode)
{
    using var connection = new SqliteConnection($"Data Source={path}");
    await connection.OpenAsync();
    
    // Check current journal mode
    using var modeCommand = new SqliteCommand("PRAGMA journal_mode", connection);
    var currentMode = await modeCommand.ExecuteScalarAsync();
    
    // Get database info
    using var sizeCommand = new SqliteCommand("PRAGMA page_count", connection);
    var pageCount = await sizeCommand.ExecuteScalarAsync();
    
    using var pageSizeCommand = new SqliteCommand("PRAGMA page_size", connection);
    var pageSize = await pageSizeCommand.ExecuteScalarAsync();
    
    var dbSize = Convert.ToInt64(pageCount) * Convert.ToInt64(pageSize);
    
    Console.WriteLine($"   Current journal mode: {currentMode}");
    Console.WriteLine($"   Database size: {dbSize / 1024.0:F1} KB ({pageCount} pages × {pageSize} bytes)");
    
    // Check for auxiliary files
    var walFile = path + "-wal";
    var shmFile = path + "-shm";
    var journalFile = path + "-journal";
    
    if (File.Exists(walFile))
    {
        var walSize = new FileInfo(walFile).Length;
        Console.WriteLine($"   WAL file size: {walSize / 1024.0:F1} KB");
    }
    
    if (File.Exists(shmFile))
    {
        var shmSize = new FileInfo(shmFile).Length;
        Console.WriteLine($"   SHM file size: {shmSize / 1024.0:F1} KB");
    }
    
    if (File.Exists(journalFile))
    {
        Console.WriteLine($"   Journal file: Present (created during transactions)");
    }
}

static async Task PerformWritePerformanceTest(string deletePath, string walPath)
{
    Console.WriteLine("Testing write performance (1000 additional records):");
    
    var testData = GenerateSensorData(1000);
    
    // Test DELETE mode
    var stopwatch = Stopwatch.StartNew();
    CGeopackageAddDataHelper.BulkInsertFeatures(deletePath, "sensor_data", testData, SRID, 100);
    stopwatch.Stop();
    var deleteWriteTime = stopwatch.ElapsedMilliseconds;
    
    // Test WAL mode
    stopwatch.Restart();
    CGeopackageAddDataHelper.BulkInsertFeatures(walPath, "sensor_data", testData, SRID, 100);
    stopwatch.Stop();
    var walWriteTime = stopwatch.ElapsedMilliseconds;
    
    Console.WriteLine($"   DELETE mode: {deleteWriteTime:N0} ms");
    Console.WriteLine($"   WAL mode:    {walWriteTime:N0} ms");
    
    if (deleteWriteTime > 0 && walWriteTime > 0)
    {
        var ratio = (double)deleteWriteTime / walWriteTime;
        if (ratio > 1.1)
            Console.WriteLine($"   Result: WAL mode {ratio:F1}x faster for writes");
        else if (ratio < 0.9)
            Console.WriteLine($"   Result: DELETE mode {1/ratio:F1}x faster for writes");
        else
            Console.WriteLine($"   Result: Similar write performance");
    }
}

static async Task PerformReadPerformanceTest(string deletePath, string walPath)
{
    Console.WriteLine("Testing read performance (spatial query):");
    
    // Define a spatial query area
    var queryPoint = new Point(500000, 6500000); // Center of Sweden
    var bufferDistance = 100000; // 100km radius
    
    // Test DELETE mode
    var stopwatch = Stopwatch.StartNew();
    var deleteResults = CMPGeopackageReadDataHelper.ReadFeatures(deletePath, "sensor_data", "geom", true);
    var deleteCount = deleteResults.Count(f => f.Geometry != null && 
        queryPoint.Distance(f.Geometry) <= bufferDistance);
    stopwatch.Stop();
    var deleteReadTime = stopwatch.ElapsedMilliseconds;
    
    // Test WAL mode
    stopwatch.Restart();
    var walResults = CMPGeopackageReadDataHelper.ReadFeatures(walPath, "sensor_data", "geom", true);
    var walCount = walResults.Count(f => f.Geometry != null && 
        queryPoint.Distance(f.Geometry) <= bufferDistance);
    stopwatch.Stop();
    var walReadTime = stopwatch.ElapsedMilliseconds;
    
    Console.WriteLine($"   DELETE mode: {deleteReadTime:N0} ms ({deleteCount:N0} features found)");
    Console.WriteLine($"   WAL mode:    {walReadTime:N0} ms ({walCount:N0} features found)");
    
    if (deleteReadTime > 0 && walReadTime > 0)
    {
        var ratio = (double)deleteReadTime / walReadTime;
        if (ratio > 1.1)
            Console.WriteLine($"   Result: WAL mode {ratio:F1}x faster for reads");
        else if (ratio < 0.9)
            Console.WriteLine($"   Result: DELETE mode {1/ratio:F1}x faster for reads");
        else
            Console.WriteLine($"   Result: Similar read performance");
    }
}

static void AnalyzeFileSystemBehavior(string deletePath, string walPath)
{
    Console.WriteLine("DELETE Mode Files:");
    AnalyzeFiles(deletePath, "DELETE");
    
    Console.WriteLine("WAL Mode Files:");
    AnalyzeFiles(walPath, "WAL");
}

static void AnalyzeFiles(string basePath, string mode)
{
    var mainFile = new FileInfo(basePath);
    Console.WriteLine($"   Main file: {mainFile.Name} ({mainFile.Length / 1024.0:F1} KB)");
    
    var walFile = basePath + "-wal";
    var shmFile = basePath + "-shm";
    var journalFile = basePath + "-journal";
    
    if (File.Exists(walFile))
    {
        var walInfo = new FileInfo(walFile);
        Console.WriteLine($"   WAL file:  {walInfo.Name} ({walInfo.Length / 1024.0:F1} KB)");
    }
    
    if (File.Exists(shmFile))
    {
        var shmInfo = new FileInfo(shmFile);
        Console.WriteLine($"   SHM file:  {shmInfo.Name} ({shmInfo.Length / 1024.0:F1} KB)");
    }
    
    if (File.Exists(journalFile))
    {
        Console.WriteLine($"   Journal:   {Path.GetFileName(journalFile)} (temporary during transactions)");
    }
    
    Console.WriteLine($"   File count: {(File.Exists(walFile) ? 1 : 0) + (File.Exists(shmFile) ? 1 : 0) + 1} files total");
    Console.WriteLine();
}

static async Task DemonstrateConcurrency(string deletePath, string walPath)
{
    Console.WriteLine("Simulating concurrent read access:");
    Console.WriteLine("(In real scenarios, WAL mode allows multiple readers simultaneously)");
    Console.WriteLine();
    
    // Simulate multiple readers
    var readTasks = new List<Task<int>>();
    
    Console.WriteLine("Testing DELETE mode concurrent reads...");
    var deleteStopwatch = Stopwatch.StartNew();
    for (int i = 0; i < 3; i++)
    {
        readTasks.Add(Task.Run(() => SimulateRead(deletePath)));
    }
    await Task.WhenAll(readTasks);
    deleteStopwatch.Stop();
    
    Console.WriteLine($"   DELETE mode: 3 readers completed in {deleteStopwatch.ElapsedMilliseconds:N0} ms");
    
    readTasks.Clear();
    
    Console.WriteLine("Testing WAL mode concurrent reads...");
    var walStopwatch = Stopwatch.StartNew();
    for (int i = 0; i < 3; i++)
    {
        readTasks.Add(Task.Run(() => SimulateRead(walPath)));
    }
    await Task.WhenAll(readTasks);
    walStopwatch.Stop();
    
    Console.WriteLine($"   WAL mode:    3 readers completed in {walStopwatch.ElapsedMilliseconds:N0} ms");
    Console.WriteLine();
    Console.WriteLine("Note: WAL mode typically shows better concurrency with real simultaneous access");
}

static int SimulateRead(string path)
{
    var features = CMPGeopackageReadDataHelper.ReadFeatures(path, "sensor_data", "geom", false);
    return features.Count();
}

static void DisplayProductionGuidance()
{
    Console.WriteLine("WHEN TO USE DELETE MODE:");
    Console.WriteLine("• Single-user desktop applications");
    Console.WriteLine("• Simple embedded scenarios");
    Console.WriteLine("• When file portability is critical");
    Console.WriteLine("• Applications with minimal concurrent access");
    Console.WriteLine("• When auxiliary files are problematic");
    Console.WriteLine();
    
    Console.WriteLine("WHEN TO USE WAL MODE:");
    Console.WriteLine("• Multi-user web applications");
    Console.WriteLine("• Read-heavy workloads with occasional writes");
    Console.WriteLine("• Applications requiring better concurrency");
    Console.WriteLine("• Server environments with multiple connections");
    Console.WriteLine("• When performance is more important than simplicity");
    Console.WriteLine();
    
    Console.WriteLine("IMPORTANT CONSIDERATIONS:");
    Console.WriteLine("• WAL files must be on the same filesystem as main database");
    Console.WriteLine("• Network filesystems may not support WAL mode properly");
    Console.WriteLine("• Backup strategies differ between modes");
    Console.WriteLine("• Some older SQLite tools may not handle WAL mode");
}

static List<FeatureRecord> GenerateSensorData(int count)
{
    var random = new Random(42); // Fixed seed for reproducible results
    var features = new List<FeatureRecord>();
    
    // Swedish coordinate bounds (SWEREF99 TM)
    const double minX = 300000.0, minY = 6200000.0;
    const double maxX = 800000.0, maxY = 7400000.0;
    
    var categories = new[] { "Temperature", "Humidity", "Pressure", "Wind", "Rain" };
    
    for (int i = 0; i < count; i++)
    {
        var x = minX + random.NextDouble() * (maxX - minX);
        var y = minY + random.NextDouble() * (maxY - minY);
        var point = new Point(x, y);
        
        var name = $"Sensor_{i + 1:D5}";
        var category = categories[random.Next(categories.Length)];
        var measurement = Math.Round(random.NextDouble() * 100, 2);
        var timestamp = DateTime.Now.AddMinutes(-random.Next(10080)).ToString("yyyy-MM-dd HH:mm:ss");
        
        var attributes = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["name"] = name,
            ["category"] = category,
            ["measurement"] = measurement.ToString("F2", CultureInfo.InvariantCulture),
            ["timestamp"] = timestamp
        };
        
        features.Add(new FeatureRecord(point, attributes));
    }
    
    return features;
}

static void CleanupFiles(string deletePath, string walPath)
{
    TryDelete(deletePath);
    TryDelete(deletePath + "-journal");
    TryDelete(walPath);
    TryDelete(walPath + "-wal");
    TryDelete(walPath + "-shm");
}

static void TryDelete(string path)
{
    try { if (File.Exists(path)) File.Delete(path); } catch { /* Ignore cleanup errors */ }
}