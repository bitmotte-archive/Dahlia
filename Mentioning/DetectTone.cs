using System.Text.RegularExpressions;

namespace Dahlia.Mentioning;

public static partial class DahliaRegex
{
  [GeneratedRegex(@"[^a-z]", RegexOptions.IgnoreCase)]
  public static partial Regex ToneRegex();
}

public class DetectTone(ToneStore store) : IService
{
    public ulong[] recentUsers = [];
    public int exhaustion = 0;
    public DateTimeOffset mostRecent = new();

    private readonly ToneStore _store = store;

    private T DeduceTone<T>(Dictionary<T, int> toneValues, T defaultValue, bool includeDefault = true) where T: struct, Enum
    {
        var finalTone = defaultValue;
        var minValue = int.MaxValue;
        var maxValue = 0;
        var allValues = Enum.GetValues<T>();
        foreach (var key in allValues)
        {
            if (key.Equals(defaultValue) && !includeDefault) continue;
            var val = toneValues.GetValueOrDefault(key);
            minValue = Math.Min(val, minValue);
            maxValue = Math.Max(val, maxValue);
            Console.Out.WriteLine($"{key}: {val}");
            if (val > 0 && maxValue == val) finalTone = key;
        }

        if (minValue == maxValue) return defaultValue;
        return finalTone;
    }

    public string Predict(string message)
    {
        var messageSplit = message.ToLower().Split(' ');

        // sentiments
        var tones = new Dictionary<Tone, int>();
        var addressing = new Dictionary<Addressing, int>();
        var statementType = new Dictionary<StatementType, int>();
        foreach (string split in messageSplit)
        {
            var replaced = DahliaRegex.ToneRegex().Replace(split, "");

            var tone = _store.MatchWordTone(replaced);
            if (tone != null)
            {
                var target = tones.GetValueOrDefault((Tone)tone) + 1;
                tones[(Tone)tone] = target;
            }

            var mAddressing = _store.MatchWordAddressing(replaced);
            if (mAddressing != null)
            {
                var target = addressing.GetValueOrDefault((Addressing)mAddressing) + 1;
                addressing[(Addressing)mAddressing] = target;
            }

            var mStatementType = _store.MatchWordTone(replaced);
            if (mStatementType != null)
            {
                var target = statementType.GetValueOrDefault((StatementType)mStatementType) + 1;
                statementType[(StatementType)mStatementType] = target;
            }
        }

        var finalTone = DeduceTone(tones, Tone.Neutral, false);
        var finalAddressing = DeduceTone(addressing, Addressing.Dahlia);
        var finalType = DeduceTone(statementType, StatementType.Statement);

        Console.Out.WriteLine($"Deduced tone: {finalTone}");
        Console.Out.WriteLine($"Deduced addressing: {finalAddressing}");
        Console.Out.WriteLine($"Deduced type: {finalType}");

        return SelectResponse(finalTone, finalAddressing, finalType);
    }

    public string SelectResponse(Tone tone, Addressing addressing, StatementType type)
    {
        var rng = new Random();
        var responses = _store.GetResponsesFor(tone, addressing, type);
        return responses[rng.Next(0,responses.Length)];
    }

    public void Init() {}

    public enum Tone
    {
        Neutral = 0,
        Lovely,
        Aggressive
    }
    
    public enum Addressing
    {
        Dahlia = 0,
        User
    }

    public enum StatementType
    {
        Statement = 0,
        Question
    }
}