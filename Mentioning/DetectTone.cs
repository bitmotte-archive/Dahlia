using System.Text.RegularExpressions;

namespace Dahlia.Mentioning;

[Serializable]
public class DetectToneLoadError : Exception
{
    public DetectToneLoadError ()
    {}

    public DetectToneLoadError (string message) 
        : base(message)
    {}

    public DetectToneLoadError (string message, Exception innerException)
        : base (message, innerException)
    {}    
}

static class DetectTone
{
    public static ulong[] recentUsers = [];
    public static int exhaustion = 0;
    public static DateTimeOffset mostRecent = new();

    const string BASE_DIR = "data/";

    private static Dictionary<Tone, Dictionary<StatementType, Dictionary<Addressing, string[]>>> responseKeys = [];
    private static Dictionary<Tone, HashSet<string>> toneKeys = [];
    private static Dictionary<Addressing, HashSet<string>> addressingKeys = [];
    private static Dictionary<StatementType, HashSet<string>> statementKeys = [];

    private static Dictionary<T, HashSet<string>> LoadEnumThingy<T>(string dirName) where T: struct, Enum
    {
        var tones = Enum.GetValues<T>();
        var list = new Dictionary<T, HashSet<string>>();

        foreach (T t in tones)
        {
            var toneName = t.ToString().ToLower();
            Console.Out.WriteLine($"Loading {toneName}");
            try
            {
                var keys = File.ReadAllLines(Path.Join(BASE_DIR, $"{dirName}/{toneName}"));
                var set = new HashSet<string>(keys);
                list.Add(t, set);
            } catch (FileNotFoundException)
            {
                Console.Out.WriteLine($"failed to load key {toneName}, not found :broken_heart:");
                throw new DetectToneLoadError(toneName);
            }
        }

        Console.Out.WriteLine($"Loaded {list.Count} enum entries");
        return list;
    }

    public static void Init()
    {
        Console.Out.WriteLine($"Initializing Dahlia DetectTone");

        toneKeys = LoadEnumThingy<Tone>("tone-keys");
        addressingKeys = LoadEnumThingy<Addressing>("addressing-keys");
        statementKeys = LoadEnumThingy<StatementType>("type-keys");

        var tones = Enum.GetValues<Tone>();

        // holy ugly nesting (if it works it works ig)
        foreach (Tone t in tones)
        {
            responseKeys.Add(t, []);
            var toneName = t.ToString().ToLower();
            var statementTypes = Enum.GetValues<StatementType>();
            foreach (StatementType s in statementTypes)
            {
                responseKeys[t].Add(s, []);
                var statementName = s.ToString().ToLower();
                var addressings = Enum.GetValues<Addressing>();
                foreach (Addressing a in addressings)
                {
                    responseKeys[t][s].Add(a, []);
                    var addressingName = a.ToString().ToLower();
                    var keys = File.ReadAllLines(Path.Join(BASE_DIR, $"responses/{toneName}/{statementName}/{addressingName}"));
                    responseKeys[t][s][a] = keys;
                }
            }
            
        }
    }

    private static void CheckTone<T>(string word, ref Dictionary<T, int> list, Dictionary<T, HashSet<string>> wordListDict) where T: struct, Enum
    {
        foreach (var item in wordListDict)
        {
            if (!item.Value.Contains(word)) continue;

            var newValue = list.GetValueOrDefault(item.Key) + 1;
            list.Add(item.Key, newValue);
        }
    }

    private static T DeduceTone<T>(Dictionary<T, int> toneValues, T defaultValue, bool includeDefault = true) where T: struct, Enum
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

    public static string Predict(string message)
    {
        var messageSplit = message.ToLower().Split(' ');

        // sentiments
        var tones = new Dictionary<Tone, int>();
        var addressing = new Dictionary<Addressing, int>();
        var statementType = new Dictionary<StatementType, int>();
        foreach (string split in messageSplit)
        {
            var replaced = Regex.Replace(split, @"[^a-z]", "");

            CheckTone(replaced, ref tones, toneKeys);
            CheckTone(replaced, ref addressing, addressingKeys);
            CheckTone(replaced, ref statementType, statementKeys);
        }

        var finalTone = DeduceTone(tones, Tone.Neutral, false);
        var finalAddressing = DeduceTone(addressing, Addressing.Dahlia);
        var finalType = DeduceTone(statementType, StatementType.Statement);

        Console.Out.WriteLine($"Deduced tone: {finalTone}");
        Console.Out.WriteLine($"Deduced addressing: {finalAddressing}");
        Console.Out.WriteLine($"Deduced type: {finalType}");

        return SelectResponse(finalTone, finalAddressing, finalType);
    }

    public static string SelectResponse(Tone tone, Addressing addressing, StatementType type)
    {
        var rng = new Random();

        var responseList = responseKeys[tone][type][addressing];
        return responseList[rng.Next(0,responseList.Length)];
    }

    public enum Tone
    {
        Lovely,
        Neutral,
        Aggressive
    }
    
    public enum Addressing
    {
        Dahlia,
        User
    }

    public enum StatementType
    {
        Question,
        Statement
    }
}