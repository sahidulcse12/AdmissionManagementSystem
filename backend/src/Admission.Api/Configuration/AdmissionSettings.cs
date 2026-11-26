namespace Admission.Api.Configuration;

/// <summary>
/// Runtime settings read from environment variables (see .env.example).
/// </summary>
public sealed class AdmissionSettings
{
    public const int DefaultHoldDurationSeconds = 600;
    public const int DefaultExpirySweepSeconds = 15;

    public required string ConnectionString { get; init; }
    public int HoldDurationSeconds { get; init; } = DefaultHoldDurationSeconds;
    public int ExpirySweepSeconds { get; init; } = DefaultExpirySweepSeconds;
    public string[] CorsOrigins { get; init; } = ["*"];

    public static AdmissionSettings FromConfiguration(IConfiguration config)
    {
        var connection = config["DATABASE_URL"]
            ?? config.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=admission;Username=admission;Password=admission";

        return new AdmissionSettings
        {
            ConnectionString = connection,
            HoldDurationSeconds = ReadPositiveInt(config, "HOLD_DURATION_SECONDS", DefaultHoldDurationSeconds),
            ExpirySweepSeconds = ReadPositiveInt(config, "EXPIRY_SWEEP_SECONDS", DefaultExpirySweepSeconds),
            CorsOrigins = (config["CORS_ORIGINS"] ?? "*")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
        };
    }

    private static int ReadPositiveInt(IConfiguration config, string key, int fallback)
    {
        var raw = config[key];
        if (string.IsNullOrWhiteSpace(raw)) return fallback;
        if (int.TryParse(raw, out var value) && value > 0) return value;
        throw new InvalidOperationException($"Environment variable {key} must be a positive integer, got '{raw}'.");
    }
}
