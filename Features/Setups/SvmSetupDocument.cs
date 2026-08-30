using System.Security.Cryptography;
using System.Text;

namespace telemetry_tracker.Features.Setups;

public sealed record SvmSetting(string? Section, string Name, string Value, string? Comment, int LineNumber);

public sealed class SvmSetupDocument
{
    private SvmSetupDocument(string sourceText, string? vehicleClassSetting, IReadOnlyList<SvmSetting> settings)
    {
        SourceText = sourceText;
        VehicleClassSetting = vehicleClassSetting;
        Settings = settings;
    }

    public string SourceText { get; }
    public string? VehicleClassSetting { get; }
    public IReadOnlyList<SvmSetting> Settings { get; }
    public string FingerprintSha256 => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(SourceText)));

    // This deliberately returns the original text. Formatting and unknown content are part of an LMU setup artifact.
    public string WriteUnchanged() => SourceText;

    public static SvmSetupDocument Parse(string sourceText)
    {
        ArgumentNullException.ThrowIfNull(sourceText);

        var settings = new List<SvmSetting>();
        string? section = null;
        string? vehicleClassSetting = null;
        using var reader = new StringReader(sourceText);
        string? line;
        var lineNumber = 0;

        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith("//", StringComparison.Ordinal)) continue;

            if (trimmed.StartsWith('[') && trimmed.EndsWith(']') && trimmed.Length > 2)
            {
                section = trimmed[1..^1];
                continue;
            }

            var equalsIndex = trimmed.IndexOf('=');
            if (equalsIndex <= 0) continue;

            var name = trimmed[..equalsIndex].Trim();
            var valueAndComment = trimmed[(equalsIndex + 1)..];
            var commentIndex = valueAndComment.IndexOf("//", StringComparison.Ordinal);
            var value = (commentIndex < 0 ? valueAndComment : valueAndComment[..commentIndex]).Trim();
            var comment = commentIndex < 0 ? null : valueAndComment[(commentIndex + 2)..].Trim();
            settings.Add(new SvmSetting(section, name, value, comment, lineNumber));

            if (section is null && name.Equals("VehicleClassSetting", StringComparison.Ordinal))
            {
                vehicleClassSetting = value.Trim('"');
            }
        }

        return new SvmSetupDocument(sourceText, vehicleClassSetting, settings);
    }

    public static async Task<SvmSetupDocument> ReadAsync(string filePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("A setup file path is required.", nameof(filePath));
        var sourceText = await File.ReadAllTextAsync(filePath, cancellationToken);
        return Parse(sourceText);
    }
}

public sealed record SetupFileCandidate(string FilePath, string TrackName, string? VehicleClassSetting, string FingerprintSha256, int SettingCount);

public static class SvmSetupDiscovery
{
    public static async Task<IReadOnlyList<SetupFileCandidate>> DiscoverAsync(string settingsRoot, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settingsRoot)) throw new ArgumentException("A settings root is required.", nameof(settingsRoot));
        if (!Directory.Exists(settingsRoot)) throw new DirectoryNotFoundException($"LMU settings directory was not found: {settingsRoot}");

        var candidates = new List<SetupFileCandidate>();
        foreach (var filePath in Directory.EnumerateFiles(settingsRoot, "*.svm", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = await SvmSetupDocument.ReadAsync(filePath, cancellationToken);
            var relativeDirectory = Path.GetRelativePath(settingsRoot, Path.GetDirectoryName(filePath)!);
            candidates.Add(new SetupFileCandidate(filePath, relativeDirectory, document.VehicleClassSetting, document.FingerprintSha256, document.Settings.Count));
        }

        return candidates.OrderBy(candidate => candidate.TrackName).ThenBy(candidate => candidate.FilePath).ToList();
    }
}
