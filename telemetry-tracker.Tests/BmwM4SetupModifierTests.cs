using System.Text;
using telemetry_tracker.Features.Setups;

namespace telemetry_tracker.Tests;

public sealed class BmwM4SetupModifierTests
{
    private static readonly string FixtureRoot = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "references", "lmu-setups", "bmw-m4-lmgt3"));

    [Fact]
    public void Modify_ChangesValidatedSettingsAndPreservesEveryOtherByte()
    {
        var source = File.ReadAllBytes(Path.Combine(FixtureRoot, "monza-mid-df.svm"));
        var changes = new[]
        {
            new SetupSettingChange("RWSetting", "11"),
            new SetupSettingChange("RearAntiSwaySetting", "0"),
            new SetupSettingChange("RearBrakeSetting", "37"),
            new SetupSettingChange("TractionControlMapSetting", "4"),
            new SetupSettingChange("TCPowerCutMapSetting", "6"),
            new SetupSettingChange("TCSlipAngleMapSetting", "8")
        };

        var result = BmwM4SetupModifier.Modify(source, changes);

        Assert.Null(result.Error);
        Assert.NotNull(result.Content);
        Assert.Equal(BmwM4SetupModifier.CarIdentifier, SvmSetupDocument.Parse(result.Content!).VehicleClassSetting);
        Assert.Contains("RWSetting=11//4.6 deg", Encoding.Latin1.GetString(result.Content));
        Assert.Contains("RearAntiSwaySetting=0//Detached", Encoding.Latin1.GetString(result.Content));
        Assert.Equal(6, result.Changes.Count);
        Assert.Equal(RemoveChangedLines(source, changes), RemoveChangedLines(result.Content, changes));
    }

    [Fact]
    public void Modify_PreservesOriginalNonUtf8Bytes()
    {
        var source = File.ReadAllBytes(Path.Combine(FixtureRoot, "monza-mid-df.svm"));
        Assert.Contains((byte)0xB0, source);
        Assert.Contains('\u00b0', SvmSetupDocument.Parse(source).SourceText);

        var result = BmwM4SetupModifier.Modify(source, [new("RWSetting", "11")]);

        Assert.Null(result.Error);
        Assert.Equal(source.Count(value => value == 0xB0), result.Content!.Count(value => value == 0xB0));
    }

    [Theory]
    [InlineData("FWSetting", "1", "not supported")]
    [InlineData("RWSetting", "7", "Supported values: 6, 11")]
    public void Modify_RejectsUnsupportedChangesWithoutContent(string name, string value, string expectedError)
    {
        var source = File.ReadAllBytes(Path.Combine(FixtureRoot, "monza-mid-df.svm"));

        var result = BmwM4SetupModifier.Modify(source, [new(name, value)]);

        Assert.Null(result.Content);
        Assert.Contains(expectedError, result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Modify_RejectsAnotherCarWithoutContent()
    {
        var source = Encoding.ASCII.GetBytes("VehicleClassSetting=\"Ferrari_296_LMGT3\"\r\n[REARWING]\r\nRWSetting=6//1.4 deg\r\n");

        var result = BmwM4SetupModifier.Modify(source, [new("RWSetting", "11")]);

        Assert.Null(result.Content);
        Assert.Contains("Unsupported car", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Modify_RejectsNoOpWithoutContent()
    {
        var source = File.ReadAllBytes(Path.Combine(FixtureRoot, "monza-mid-df.svm"));

        var result = BmwM4SetupModifier.Modify(source, [new("RWSetting", "6")]);

        Assert.Null(result.Content);
        Assert.Contains("already has value", result.Error, StringComparison.Ordinal);
    }

    private static byte[] RemoveChangedLines(byte[] content, IEnumerable<SetupSettingChange> changes)
    {
        var names = changes.Select(change => change.Name).ToHashSet(StringComparer.Ordinal);
        var text = Encoding.Latin1.GetString(content);
        var retained = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !names.Any(name => line.StartsWith($"{name}=", StringComparison.Ordinal)));
        return Encoding.Latin1.GetBytes(string.Join('\n', retained));
    }
}
