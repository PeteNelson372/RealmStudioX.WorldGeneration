namespace RealmStudioX.WorldGeneration.LandformGeneration.Islands;

using RealmStudioX.WorldGeneration.LandformGeneration;
using SkiaSharp;

/// <summary>
/// Generates irregular island outlines in map coordinates.
/// This class has no dependencies on the map model or WPF.
/// </summary>
public sealed class IslandGeometryGenerator
{
    private const float MaximumShapeVariation = 0.28f;
    private const float MaximumCoastlineVariation = 0.10f;

    private const int MinimumPointCount = 64;
    private const int MaximumPointCount = 8192;

    private const int ShapeHarmonicCount = 4;
    private const int DetailHarmonicCount = 8;

    /// <summary>
    /// Generates an island outline within the supplied rectangular bounds.
    /// </summary>
    public LandformGenerationResult Generate(
        SKRect generationBounds,
        LandformGenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ValidateBounds(generationBounds);
        ValidateOptions(options);

        var random = new DeterministicRandom(options.Seed);

        float centerX =
            (generationBounds.Left + generationBounds.Right) * 0.5f;

        float centerY =
            (generationBounds.Top + generationBounds.Bottom) * 0.5f;

        float radiusX =
            generationBounds.Width * 0.5f * options.Size;

        float radiusY =
            generationBounds.Height * 0.5f * options.Size;

        // Generate independent harmonic sets for the broad shape
        // and the smaller coastline variations.

        Harmonic[] shapeHarmonics = CreateHarmonics(
            random,
            minimumFrequency: 2,
            maximumFrequency: 5,
            count: ShapeHarmonicCount);

        Harmonic[] detailHarmonics = CreateHarmonics(
            random,
            minimumFrequency: 7,
            maximumFrequency: 24,
            count: DetailHarmonicCount);

        int pointCount = options.CoastlinePointCount;

        var points = new SKPoint[pointCount];
        var radiusFactors = new float[pointCount];

        float maximumRadiusFactor = 0f;

        // -------------------------------------------------
        // Calculate radial variations around the island.
        // -------------------------------------------------

        for (int i = 0; i < pointCount; i++)
        {
            double angle =
                (Math.PI * 2.0 * i) / pointCount;

            float shapeVariation = EvaluateHarmonics(
                shapeHarmonics,
                angle);

            float detailVariation = EvaluateHarmonics(
                detailHarmonics,
                angle);

            float radiusFactor =
                1f
                + MaximumShapeVariation *
                    options.ShapeIrregularity * shapeVariation
                + MaximumCoastlineVariation *
                    options.CoastlineDetail * detailVariation;

            radiusFactors[i] = radiusFactor;

            maximumRadiusFactor = Math.Max(
                maximumRadiusFactor,
                radiusFactor);
        }

        // -------------------------------------------------
        // Construct the island perimeter.
        // -------------------------------------------------

        using var pathBuilder = new SKPathBuilder
        {
            FillType = SKPathFillType.EvenOdd
        };

        for (int i = 0; i < pointCount; i++)
        {
            double angle =
                (Math.PI * 2.0 * i) / pointCount;

            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            // Distance from the center to the elliptical
            // boundary in this direction.

            float ellipseRadius = 1f /
                MathF.Sqrt(
                    (cos * cos) / (radiusX * radiusX)
                    + (sin * sin) / (radiusY * radiusY));

            // Normalize variations so the island remains
            // within the requested generation bounds.

            float radius =
                ellipseRadius *
                radiusFactors[i] /
                maximumRadiusFactor;

            float x = centerX + cos * radius;
            float y = centerY + sin * radius;

            points[i] = new SKPoint(x, y);
        }

        pathBuilder.MoveTo(points[0]);

        for (int i = 1; i < pointCount; i++)
        {
            pathBuilder.LineTo(points[i]);
        }

        pathBuilder.Close();

        SKPath path = pathBuilder.Snapshot();

        return new LandformGenerationResult(
            path,
            generationBounds,
            options.Seed,
            pointCount);
    }

    // -------------------------------------------------
    // Harmonic generation
    // -------------------------------------------------

    private static Harmonic[] CreateHarmonics(
        DeterministicRandom random,
        int minimumFrequency,
        int maximumFrequency,
        int count)
    {
        var harmonics = new Harmonic[count];

        for (int i = 0; i < count; i++)
        {
            int frequency = random.Next(
                minimumFrequency,
                maximumFrequency + 1);

            double phase =
                random.NextDouble() * Math.PI * 2.0;

            double amplitude =
                0.5 + random.NextDouble() * 0.5;

            harmonics[i] = new Harmonic(
                frequency,
                phase,
                amplitude);
        }

        return harmonics;
    }

    private static float EvaluateHarmonics(
        Harmonic[] harmonics,
        double angle)
    {
        double value = 0.0;
        double totalAmplitude = 0.0;

        foreach (var harmonic in harmonics)
        {
            value += harmonic.Amplitude *
                     Math.Sin(
                         harmonic.Frequency * angle +
                         harmonic.Phase);

            totalAmplitude += harmonic.Amplitude;
        }

        if (totalAmplitude <= 0.0)
            return 0f;

        return (float)(value / totalAmplitude);
    }

    private readonly record struct Harmonic(
        int Frequency,
        double Phase,
        double Amplitude);

    // -------------------------------------------------
    // Validation
    // -------------------------------------------------

    private static void ValidateBounds(SKRect bounds)
    {
        if (!float.IsFinite(bounds.Left) ||
            !float.IsFinite(bounds.Top) ||
            !float.IsFinite(bounds.Right) ||
            !float.IsFinite(bounds.Bottom) ||
            bounds.Width <= 0f ||
            bounds.Height <= 0f)
        {
            throw new ArgumentException(
                "Generation bounds must be finite and have positive dimensions.",
                nameof(bounds));
        }
    }

    private static void ValidateOptions(
        LandformGenerationOptions options)
    {
        if (!float.IsFinite(options.Size) ||
            options.Size <= 0f ||
            options.Size > 1f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.Size),
                "Size must be greater than zero and no greater than one.");
        }

        if (!float.IsFinite(options.ShapeIrregularity) ||
            options.ShapeIrregularity < 0f ||
            options.ShapeIrregularity > 1f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.ShapeIrregularity));
        }

        if (!float.IsFinite(options.CoastlineDetail) ||
            options.CoastlineDetail < 0f ||
            options.CoastlineDetail > 1f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.CoastlineDetail));
        }

        if (options.CoastlinePointCount < MinimumPointCount ||
            options.CoastlinePointCount > MaximumPointCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.CoastlinePointCount),
                $"Point count must be between {MinimumPointCount} and {MaximumPointCount}.");
        }
    }

    // -------------------------------------------------
    // Deterministic pseudo-random number generator
    // -------------------------------------------------

    /// <summary>
    /// Small deterministic generator so island geometry does not
    /// depend on the runtime implementation of System.Random.
    /// </summary>
    private sealed class DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(int seed)
        {
            _state = unchecked((uint)seed);

            // Xorshift32 cannot operate correctly with a zero state.
            if (_state == 0)
                _state = 0x6D2B79F5;
        }

        private uint NextUInt()
        {
            uint value = _state;

            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;

            _state = value;

            return value;
        }

        public int Next(int minimum, int maximum)
        {
            uint range = (uint)(maximum - minimum);

            return minimum +
                   (int)(NextUInt() % range);
        }

        public double NextDouble()
        {
            return NextUInt() / 4294967296.0;
        }
    }
}
