using static Dahlia.Mentioning.DetectTone;

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

public interface IToneStore : IService
{
    public string[] GetResponsesFor(Tone tone, Addressing addressing, StatementType type);
}

public class ToneStore : IToneStore
{
    const string BASE_DIR = "data/";

    private Dictionary<Tone, Dictionary<StatementType, Dictionary<Addressing, string[]>>> _responseKeys = [];
    private Dictionary<Tone, HashSet<string>> _toneKeys = [];
    private Dictionary<Addressing, HashSet<string>> _addressingKeys = [];
    private Dictionary<StatementType, HashSet<string>> _statementKeys = [];

    private string[] irritatedKeys = [];
    private string[] overjoyedKeys = [];

    private Dictionary<T, HashSet<string>> LoadEnumThingy<T>(string dirName) where T: struct, Enum
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
                list[t] = set;
            } catch (FileNotFoundException)
            {
                Console.Out.WriteLine($"failed to load key {toneName}, not found :broken_heart:");
                throw new DetectToneLoadError(toneName);
            }
        }

        Console.Out.WriteLine($"Loaded {list.Count} enum entries");
        return list;
    }

    public void Init()
    {
        Console.Out.WriteLine($"Initializing Dahlia DetectTone");

        _toneKeys = LoadEnumThingy<Tone>("tone-keys");
        _addressingKeys = LoadEnumThingy<Addressing>("addressing-keys");
        _statementKeys = LoadEnumThingy<StatementType>("type-keys");

        var tones = Enum.GetValues<Tone>();

        // holy ugly nesting (if it works it works ig)
        foreach (Tone t in tones)
        {
            _responseKeys[t] = [];
            var toneName = t.ToString().ToLower();
            var statementTypes = Enum.GetValues<StatementType>();
            foreach (StatementType s in statementTypes)
            {
                _responseKeys[t][s] = [];
                var statementName = s.ToString().ToLower();
                var addressings = Enum.GetValues<Addressing>();
                foreach (Addressing a in addressings)
                {
                    _responseKeys[t][s][a] = [];
                    var addressingName = a.ToString().ToLower();
                    var keys = File.ReadAllLines(Path.Join(BASE_DIR, $"responses/{toneName}/{statementName}/{addressingName}"));
                    _responseKeys[t][s][a] = keys;
                }
            }
            
        }

        irritatedKeys = File.ReadAllLines(Path.Join(BASE_DIR, $"responses/irritated"));
        overjoyedKeys = File.ReadAllLines(Path.Join(BASE_DIR, $"responses/overjoyed"));
    }

    public string[] GetResponsesFor(Tone tone, Addressing addressing, StatementType type)
    {
        var responseList = _responseKeys[tone][type][addressing];
        return responseList;
    }

    public string[] GetStrongResponsesFor(Strong strong)
    {
        if(strong == Strong.Irritated) return irritatedKeys;
        if(strong == Strong.Overjoyed) return overjoyedKeys;
        return ["shit went wrong man fuck god damnit hey <@1414406914669805662> hey <@1414406914669805662> your bot is broken go fix it"];
    }

    public Tone? MatchWordTone(string word)
    {
        foreach (var item in _toneKeys)
        {
            if (item.Value.Contains(word)) return item.Key;
        }

        return null;
    }

    public Addressing? MatchWordAddressing(string word)
    {
        foreach (var item in _addressingKeys)
        {
            if (item.Value.Contains(word)) return item.Key;
        }

        return null;
    }

    public StatementType? MatchWordStatementType(string word)
    {
        foreach (var item in _statementKeys)
        {
            if (item.Value.Contains(word)) return item.Key;
        }

        return null;
    }
}