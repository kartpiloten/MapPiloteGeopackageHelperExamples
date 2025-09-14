# Optional Callback Pattern Tutorial

This educational example demonstrates the **Optional Callback Pattern** used throughout the MapPiloteGeopackageHelper library.

## What You'll Learn

- **What is the Optional Callback Pattern?** - Modern .NET design pattern for progress reporting
- **IProgress\<T\> interface** - Standard .NET callback mechanism
- **When to use callbacks** vs when to omit them
- **Different callback scenarios** - from simple to advanced
- **Best practices** for implementing progress callbacks

## Running the Example

```bash
cd MapPiloteGeoPackageOptionalCallbackPattern
dotnet run
```

## Five Scenarios Demonstrated

### 1. **No Callback (Silent Operation)**
```csharp
// For small operations, no callback needed
await layer.BulkInsertAsync(smallDataset, options, progress: null);
```

### 2. **Simple Progress Callback**
```csharp
// Basic progress percentage reporting
var progress = new Progress<BulkProgress>(p => 
    Console.WriteLine($"Progress: {p.PercentComplete:F1}%"));

await layer.BulkInsertAsync(dataset, options, progress);
```

### 3. **Advanced Visual Progress Bar**
```csharp
// Rich UI with progress bar and ETA
var progress = new Progress<BulkProgress>(p => {
    var bar = new string('#', filledWidth) + new string('.', emptyWidth);
    Console.Write($"\r[{bar}] {p.PercentComplete:F1}% ETA: {eta:mm\\:ss}");
});
```

### 4. **Custom Business Logic**
```csharp
// Trigger business actions at milestones
var progress = new Progress<BulkProgress>(p => {
    if (p.PercentComplete >= milestone) {
        Console.WriteLine("*** MILESTONE: Send notification! ***");
        // Custom business logic here
    }
});
```

### 5. **Conditional Callback**
```csharp
// Runtime decision whether to use callback
IProgress<BulkProgress>? progress = enableProgress 
    ? new Progress<BulkProgress>(p => /* callback */)
    : null;

await layer.BulkInsertAsync(dataset, options, progress);
```

## Key Benefits

**FLEXIBILITY** - Callers choose their monitoring level  
**PERFORMANCE** - No overhead when not needed  
**STANDARDS** - Follows .NET IProgress\<T\> conventions  
**UI RESPONSIVE** - Enables responsive interfaces  
**TESTABILITY** - Easy to test with mock callbacks  

## Pattern Usage in MapPiloteGeopackageHelper

The library uses this pattern in:
- `BulkInsertAsync()` methods for progress reporting
- Large dataset operations
- Spatial index creation
- Long-running queries

This makes the API both simple for basic use and powerful for advanced scenarios!