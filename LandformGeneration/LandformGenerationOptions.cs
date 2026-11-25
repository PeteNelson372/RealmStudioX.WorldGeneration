namespace RealmStudioX.WorldGeneration.LandformGeneration;

/// <summary>
/// Controls the generation of an island outline.
/// </summary>
public sealed class LandformGenerationOptions
{
    /// <summary>
    /// Seed used to produce reproducible island geometry.
    /// </summary>
    public int Seed { get; set; } = 12345;

    /// <summary>
    /// Island size relative to the supplied generation bounds.
    /// 1.0 uses the full available width and height.
    /// </summary>
    public float Size { get; set; } = 0.85f;

    /// <summary>
    /// Controls large-scale variations in the island outline.
    /// Range: 0.0 (elliptical) to 1.0 (highly irregular).
    /// </summary>
    public float ShapeIrregularity { get; set; } = 0.25f;

    /// <summary>
    /// Controls smaller-scale coastline variations.
    /// Range: 0.0 (smooth) to 1.0 (highly detailed).
    /// </summary>
    public float CoastlineDetail { get; set; } = 0.25f;

    /// <summary>
    /// Number of points used to construct the coastline.
    /// Higher values produce smoother, more detailed outlines.
    /// </summary>
    public int CoastlinePointCount { get; set; } = 256;
}
