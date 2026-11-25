namespace RealmStudioX.WorldGeneration.LandformGeneration;

using SkiaSharp;

/// <summary>
/// Contains the generated geometry and its generation metadata.
/// The result owns its Geometry path and must be disposed.
/// </summary>
public sealed class LandformGenerationResult : IDisposable
{
    public SKPath Geometry { get; }

    public SKRect GenerationBounds { get; }

    public int Seed { get; }

    public int PointCount { get; }

    internal LandformGenerationResult(
        SKPath geometry,
        SKRect generationBounds,
        int seed,
        int pointCount)
    {
        Geometry = geometry;
        GenerationBounds = generationBounds;
        Seed = seed;
        PointCount = pointCount;
    }

    public void Dispose()
    {
        Geometry.Dispose();
    }
}
