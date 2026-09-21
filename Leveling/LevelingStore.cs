using NetCord.Rest;
using SQLite;

namespace Dahlia.Leveling;

public interface ILevelingStore : IService
{
    public void EnsureUser(ulong user);

    public void StartTx();
    public void CommitTx();
    public void RollbackTx();

    public Levels GetLevelByUser(ulong id);
    public void UpdateUserLevel(ulong id, int level, int exp);
}

public class LevelingStore : ILevelingStore {

    private SQLiteConnection _db;
    
    public LevelingStore()
    {
        _db = new SQLiteConnection("Databases/levels.db");
        _db.CreateTable<Levels>();	
    }

    public void EnsureUser(ulong user)
    {
        _db.Query<Levels>("INSERT INTO levels VALUES (?, 0, 0) ON CONFLICT (user_id) DO NOTHING;", user);
    }

    public void StartTx()
    {
        _db.BeginTransaction();
    }

    public void CommitTx()
    {
        _db.Commit();
    }

    public void RollbackTx()
    {
        _db.Rollback();
    }

    public Levels GetLevelByUser(ulong id)
    {
        return _db.FindWithQuery<Levels>("SELECT * FROM levels WHERE user_id = ?;", id);
    }

    public void UpdateUserLevel(ulong id, int level, int exp)
    {
        _db.Query<Levels>("UPDATE levels SET level = ?, exp = ? WHERE user_id = ?;", level, exp, id);
    }

    public void Init() {}
}	