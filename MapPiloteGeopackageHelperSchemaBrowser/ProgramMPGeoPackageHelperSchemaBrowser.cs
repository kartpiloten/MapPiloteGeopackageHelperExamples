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
using System.Globalization;

// =============================================================
// GeoPackage Schema Inspector Tutorial
// -------------------------------------------------------------
// This tutorial demonstrates how to explore unknown GeoPackage files:
//  1) Opening and inspecting existing GeoPackage files
//  2) Discovering layers and their schemas
//  3) Understanding coordinate systems and spatial extents
//  4) Examining column definitions and data types
//  5) Generating C# code for working with the data
//  6) Browsing sample data to understand content
// Perfect for understanding the structure of unfamiliar GeoPackages!
// =============================================================

Console.WriteLine("=== GeoPackage Schema Inspector Tutorial ===");
Console.WriteLine("Learn to explore and understand GeoPackage file structures");
Console.WriteLine();

// Locate the sample GeoPackage file
string workDir = Environment.CurrentDirectory;
string sampleGpkg = Path.Combine(workDir, "Data", "AdmBordersSweden.gpkg");

Console.WriteLine("=== TUTORIAL OVERVIEW ===");
Console.WriteLine("This tutorial will teach you to:");
Console.WriteLine("• Inspect GeoPackage metadata and structure");
Console.WriteLine("• Understand coordinate systems and spatial extents");  
Console.WriteLine("• Examine column schemas and data types");
Console.WriteLine("• Generate C# code for working with the data");
Console.WriteLine("• Browse sample records to understand content");
Console.WriteLine();

// Check if sample file exists
if (!File.Exists(sampleGpkg))
{
    Console.WriteLine("ERROR: SAMPLE FILE NOT FOUND");
    Console.WriteLine($"Expected location: {sampleGpkg}");
    Console.WriteLine();
    Console.WriteLine("This tutorial requires the sample data file to demonstrate schema inspection.");
    Console.WriteLine("Please ensure the AdmBordersSweden.gpkg file is in the Data folder.");
    Console.WriteLine();
    Console.WriteLine("Alternative: Create your own GeoPackage using other examples");
    Console.WriteLine("   and replace the file path above to inspect your own data!");
    return;
}

Console.WriteLine("INSPECTING GEOPACKAGE");
Console.WriteLine($"File: {Path.GetFileName(sampleGpkg)}");
Console.WriteLine($"Size: {new FileInfo(sampleGpkg).Length / 1024.0:F1} KB");
Console.WriteLine();

try
{
    // =================================================================
    // TASK 1: Getting GeoPackage Overview
    // =================================================================
    Console.WriteLine("TASK 1: Getting GeoPackage metadata overview");
    Console.WriteLine("GetGeopackageInfo() provides complete structural information");
    Console.WriteLine();

    var geoPackageInfo = CMPGeopackageReadDataHelper.GetGeopackageInfo(sampleGpkg);
    
    Console.WriteLine($"GEOPACKAGE SUMMARY:");
    Console.WriteLine($"   Total layers: {geoPackageInfo.Layers.Count}");
    Console.WriteLine($"   File format: GeoPackage (SQLite with spatial extensions)");
    Console.WriteLine();

    // =================================================================
    // TASK 2: Examining Each Layer in Detail
    // =================================================================
    Console.WriteLine("TASK 2: Detailed layer examination");
    Console.WriteLine("Each layer contains geometry, attributes, and metadata");
    Console.WriteLine();

    int layerNumber = 1;
    foreach (var layer in geoPackageInfo.Layers)
    {
        Console.WriteLine($"LAYER {layerNumber}: {layer.TableName}");
        Console.WriteLine(new string('─', 50));

        // Basic layer information
        Console.WriteLine($"BASIC INFORMATION:");
        Console.WriteLine($"   Data type: {layer.DataType}");
        Console.WriteLine($"   Geometry type: {layer.GeometryType ?? "<none>"}");
        Console.WriteLine($"   Geometry column: {layer.GeometryColumn ?? "<none>"}");
        Console.WriteLine($"   Coordinate system (SRID): {layer.Srid?.ToString() ?? "<unknown>"}");
        Console.WriteLine();

        // Spatial extent information
        if (layer.MinX.HasValue && layer.MaxX.HasValue)
        {
            Console.WriteLine($"SPATIAL EXTENT:");
            Console.WriteLine($"   Southwest corner: ({layer.MinX:F2}, {layer.MinY:F2})");
            Console.WriteLine($"   Northeast corner: ({layer.MaxX:F2}, {layer.MaxY:F2})");
            
            var width = layer.MaxX - layer.MinX;
            var height = layer.MaxY - layer.MinY;
            Console.WriteLine($"   Bounding box size: {width:F0} × {height:F0} coordinate units");
            Console.WriteLine();
        }

        // Column schema details
        Console.WriteLine($"COLUMN SCHEMA ({layer.Columns.Count} columns):");
        foreach (var column in layer.Columns)
        {
            var isPrimaryKey = column.IsPrimaryKey ? " [PRIMARY KEY]" : "";
            var isNotNull = column.NotNull ? " [NOT NULL]" : "";
            var isGeometry = column.Name.Equals(layer.GeometryColumn, StringComparison.OrdinalIgnoreCase) ? " [GEOMETRY]" : "";
            
            Console.WriteLine($"   • {column.Name}: {column.Type}{isPrimaryKey}{isNotNull}{isGeometry}");
        }
        Console.WriteLine();

        // =================================================================
        // TASK 3: Generating C# Code Templates
        // =================================================================
        Console.WriteLine($"SUGGESTED C# RECORD (copy-paste ready):");
        Console.WriteLine($"   Use this record to work with {layer.TableName} data in your C# code:");
        Console.WriteLine();

        // Generate C# record for attributes (excluding geometry and primary key)
        var attributeColumns = layer.AttributeColumns;
        if (attributeColumns.Count > 0)
        {
            Console.WriteLine($"   public sealed record {ToPascalCase(layer.TableName)}Attributes(");
            for (int i = 0; i < attributeColumns.Count; i++)
            {
                var col = attributeColumns[i];
                var clrType = MapSqliteTypeToClr(col.Type, col.NotNull);
                var propertyName = ToPascalCase(col.Name);
                var comma = i == attributeColumns.Count - 1 ? "" : ",";
                
                Console.WriteLine($"       {clrType} {propertyName}{comma}");
            }
            Console.WriteLine($"   );");
        }
        else
        {
            Console.WriteLine($"   // No attribute columns (geometry-only layer)");
        }
        Console.WriteLine();

        // =================================================================
        // TASK 4: Code Examples for Reading Data
        // =================================================================
        Console.WriteLine($"EXAMPLE CODE TO READ THIS LAYER:");
        Console.WriteLine($"   Copy this code to read {layer.TableName} in your applications:");
        Console.WriteLine();
        
        Console.WriteLine($"   // Basic reading");
        Console.WriteLine($"   var features = CMPGeopackageReadDataHelper.ReadFeatures(");
        Console.WriteLine($"       geoPackageFilePath: \"{Path.GetFileName(sampleGpkg)}\",");
        Console.WriteLine($"       tableName: \"{layer.TableName}\",");
        Console.WriteLine($"       includeGeometry: true);");
        Console.WriteLine();
        
        Console.WriteLine($"   // Process each feature");
        Console.WriteLine($"   foreach (var feature in features)");
        Console.WriteLine($"   {{");
        Console.WriteLine($"       var geometry = feature.Geometry; // NetTopologySuite geometry");
        Console.WriteLine($"       var attributes = feature.Attributes; // Dictionary<string, string?>");
        Console.WriteLine();
        if (attributeColumns.Count > 0)
        {
            var firstCol = attributeColumns[0];
            Console.WriteLine($"       // Access attribute example:");
            Console.WriteLine($"       var {firstCol.Name} = attributes[\"{firstCol.Name}\"];");
        }
        Console.WriteLine($"   }}");
        Console.WriteLine();

        // =================================================================
        // TASK 5: Sample Data Preview
        // =================================================================
        Console.WriteLine($"SAMPLE DATA (first 3 records):");
        Console.WriteLine($"   Preview of actual data in this layer:");
        Console.WriteLine();

        try
        {
            var includeGeometry = !string.IsNullOrEmpty(layer.GeometryColumn);
            var geometryColumn = layer.GeometryColumn ?? "geom";
            
            int recordCount = 0;
            foreach (var feature in CMPGeopackageReadDataHelper.ReadFeatures(
                sampleGpkg, layer.TableName, geometryColumn, includeGeometry))
            {
                recordCount++;
                
                // Format geometry summary
                string geometryInfo = feature.Geometry switch
                {
                    NetTopologySuite.Geometries.Point pt => 
                        $"POINT({pt.X:F1}, {pt.Y:F1})",
                    null => 
                        "<no geometry>",
                    _ => 
                        $"{feature.Geometry.GeometryType} ({feature.Geometry.NumPoints} points)"
                };

                // Format attributes  
                var attributeInfo = string.Join(", ", 
                    feature.Attributes.Take(3).Select(kvp => 
                        $"{kvp.Key}={kvp.Value ?? "<null>"}"));

                Console.WriteLine($"   Record {recordCount}:");
                Console.WriteLine($"     Geometry: {geometryInfo}");
                Console.WriteLine($"     Attributes: {attributeInfo}");
                if (feature.Attributes.Count > 3)
                {
                    Console.WriteLine($"     (+ {feature.Attributes.Count - 3} more attributes...)");
                }
                Console.WriteLine();

                if (recordCount >= 3) break; // Show only first 3 records
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   WARNING: Could not read sample data: {ex.Message}");
            Console.WriteLine();
        }

        Console.WriteLine(new string('═', 60));
        Console.WriteLine();
        layerNumber++;
    }

    // =================================================================
    // TUTORIAL SUMMARY
    // =================================================================
    Console.WriteLine("TUTORIAL COMPLETED!");
    Console.WriteLine("You've learned to inspect GeoPackage structure and generate working code.");
    Console.WriteLine();
    Console.WriteLine("Key skills acquired:");
    Console.WriteLine("SUCCESS: Extract metadata from unknown GeoPackage files");
    Console.WriteLine("SUCCESS: Understand coordinate systems and spatial extents");
    Console.WriteLine("SUCCESS: Analyze column schemas and data types");
    Console.WriteLine("SUCCESS: Generate C# records for strongly-typed data access");
    Console.WriteLine("SUCCESS: Write code to read and process GeoPackage data");
    Console.WriteLine();
    Console.WriteLine("Next steps:");
    Console.WriteLine("• Use the generated C# records in your applications");
    Console.WriteLine("• Try inspecting other GeoPackage files");
    Console.WriteLine("• Combine with other examples to create complete workflows");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR INSPECTING GEOPACKAGE: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine("This demonstrates error handling when working with");
    Console.WriteLine("potentially corrupted or inaccessible GeoPackage files.");
}

// =================================================================
// Helper Methods for Code Generation
// =================================================================

static string ToPascalCase(string input)
{
    if (string.IsNullOrEmpty(input)) return input;
    
    // Split on common separators and capitalize each part
    var parts = input.Split(new[] { '_', ' ', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
    return string.Concat(parts.Select(part => 
        char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()));
}

static string MapSqliteTypeToClr(string sqliteType, bool isNotNull)
{
    var type = (sqliteType ?? "").Trim().ToUpperInvariant();
    
    string clrType = type switch
    {
        "INTEGER" or "INT" => "long",
        "REAL" or "FLOAT" or "DOUBLE" => "double", 
        "TEXT" or "VARCHAR" or "CHAR" => "string",
        "BLOB" => "byte[]",
        _ => "string" // Default for unknown types
    };

    // Handle nullability
    if (clrType == "string" || clrType == "byte[]")
    {
        // Reference types - add ? if nullable
        return isNotNull ? clrType : clrType + "?";
    }
    else
    {
        // Value types - add ? if nullable  
        return isNotNull ? clrType : clrType + "?";
    }
}
