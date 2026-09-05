using System.Text;

namespace telemetry_tracker.Features.Setups;

public sealed record SetupSettingChange(string Name, string Value);
public sealed record AppliedSetupSettingChange(string Section, string Name, string PreviousValue, string Value, string Comment);
public sealed record SetupModificationResult(byte[]? Content, IReadOnlyList<AppliedSetupSettingChange> Changes, string? Error);

public static class BmwM4SetupModifier
{
    public const string CarIdentifier = "BMW_M4_LMGT3 GT3 WEC2025";

    private sealed record SettingContract(string Section, IReadOnlyDictionary<string, string> Values);

    private static readonly IReadOnlyDictionary<string, SettingContract> Contracts =
        new Dictionary<string, SettingContract>(StringComparer.Ordinal)
        {
            ["RWSetting"] = new("REARWING", Values(("6", "1.4 deg"), ("11", "4.6 deg"))),
            ["RearAntiSwaySetting"] = new("SUSPENSION", Values(("0", "Detached"), ("1", "P1 (soft)"))),
            ["RearBrakeSetting"] = new("CONTROLS", Values(("35", "48.3:51.7"), ("37", "47.8:52.2"))),
            ["TractionControlMapSetting"] = new("CONTROLS", Values(("2", "2"), ("4", "4"))),
            ["TCPowerCutMapSetting"] = new("CONTROLS", Values(("4", "4"), ("6", "6"))),
            ["TCSlipAngleMapSetting"] = new("CONTROLS", Values(("6", "6"), ("8", "8")))
        };

    public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> SupportedValues { get; } =
        Contracts.ToDictionary(
            item => item.Key,
            item => (IReadOnlyCollection<string>)item.Value.Values.Keys.ToArray(),
            StringComparer.Ordinal);

    public static SetupModificationResult Modify(byte[] source, IReadOnlyCollection<SetupSettingChange> requestedChanges)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(requestedChanges);

        var document = SvmSetupDocument.Parse(source);
        if (!string.Equals(document.VehicleClassSetting, CarIdentifier, StringComparison.Ordinal))
        {
            return Failure($"Unsupported car '{document.VehicleClassSetting ?? "<missing>"}'. Expected exact identifier '{CarIdentifier}'.");
        }

        if (requestedChanges.Count == 0)
        {
            return Failure("At least one setting change is required.");
        }

        var duplicates = requestedChanges.GroupBy(change => change.Name, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
        if (duplicates is not null)
        {
            return Failure($"Setting '{duplicates.Key}' was specified more than once.");
        }

        var sourceText = Encoding.Latin1.GetString(source);
        var lines = GetLines(sourceText);
        var replacements = new List<(int LineNumber, string Line)>();
        var applied = new List<AppliedSetupSettingChange>();
        foreach (var change in requestedChanges)
        {
            if (!Contracts.TryGetValue(change.Name, out var contract))
            {
                return Failure($"Setting '{change.Name}' is not supported for {CarIdentifier}.");
            }

            if (!contract.Values.TryGetValue(change.Value, out var newComment))
            {
                return Failure($"Value '{change.Value}' is not supported for {change.Name}. Supported values: {string.Join(", ", contract.Values.Keys)}.");
            }

            var matches = document.Settings.Where(setting =>
                string.Equals(setting.Section, contract.Section, StringComparison.Ordinal) &&
                string.Equals(setting.Name, change.Name, StringComparison.Ordinal)).ToList();
            if (matches.Count != 1)
            {
                return Failure($"Setup must contain exactly one active [{contract.Section}] {change.Name} setting.");
            }

            var current = matches[0];
            if (!contract.Values.TryGetValue(current.Value, out var expectedComment) ||
                !string.Equals(current.Comment, expectedComment, StringComparison.Ordinal))
            {
                return Failure($"The source encoding for {change.Name} has not been validated.");
            }

            var sourceLine = lines[current.LineNumber - 1];
            var validatedLine = $"{change.Name}={current.Value}//{expectedComment}";
            if (!sourceText.AsSpan(sourceLine.Start, sourceLine.Length).SequenceEqual(validatedLine))
            {
                return Failure($"The source line format for {change.Name} has not been validated.");
            }

            if (string.Equals(current.Value, change.Value, StringComparison.Ordinal))
            {
                return Failure($"Setting '{change.Name}' already has value '{change.Value}'.");
            }

            replacements.Add((current.LineNumber, $"{change.Name}={change.Value}//{newComment}"));
            applied.Add(new(contract.Section, change.Name, current.Value, change.Value, newComment));
        }

        var output = new StringBuilder(sourceText);
        foreach (var replacement in replacements.OrderByDescending(item => lines[item.LineNumber - 1].Start))
        {
            var line = lines[replacement.LineNumber - 1];
            output.Remove(line.Start, line.Length);
            output.Insert(line.Start, replacement.Line);
        }

        var content = Encoding.Latin1.GetBytes(output.ToString());
        var reparsed = SvmSetupDocument.Parse(content);
        if (!string.Equals(reparsed.VehicleClassSetting, CarIdentifier, StringComparison.Ordinal))
        {
            return Failure("Modified setup failed exact car-identity validation.");
        }

        return new(content, applied, null);
    }

    private static Dictionary<string, string> Values(params (string Value, string Comment)[] values) =>
        values.ToDictionary(item => item.Value, item => item.Comment, StringComparer.Ordinal);

    private static SetupModificationResult Failure(string error) => new(null, [], error);

    private static List<(int Start, int Length)> GetLines(string source)
    {
        var lines = new List<(int Start, int Length)>();
        var start = 0;
        for (var index = 0; index < source.Length; index++)
        {
            if (source[index] != '\r' && source[index] != '\n') continue;
            lines.Add((start, index - start));
            if (source[index] == '\r' && index + 1 < source.Length && source[index + 1] == '\n') index++;
            start = index + 1;
        }

        if (start <= source.Length) lines.Add((start, source.Length - start));
        return lines;
    }
}
