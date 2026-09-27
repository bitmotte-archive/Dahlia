using NetCord.Rest;
using SQLite;

namespace Dahlia.Mentioning;

public interface IAngerStore : IService
{
    public void EnsureUser(ulong user);

    public void StartTx();
    public void CommitTx();
    public void RollbackTx();

    public Anger GetAngerByUser(ulong id);
    public void UpdateAngerLevel(ulong id, int anger);
}

public class AngerStore : IAngerStore {

    private SQLiteConnection _db;
    
    public AngerStore()
    {
        _db = new SQLiteConnection("Databases/Anger.db");
        _db.CreateTable<Anger>();	
    }

    public void EnsureUser(ulong user)
    {
        _db.Query<Anger>("INSERT INTO anger VALUES (?, 0) ON CONFLICT (user_id) DO NOTHING;", user);
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

    public Anger GetAngerByUser(ulong id)
    {
        return _db.FindWithQuery<Anger>("SELECT * FROM anger WHERE user_id = ?;", id);
    }

    public void UpdateAngerLevel(ulong id, int anger)
    {
        _db.Query<Anger>("UPDATE anger SET sentiment = ? WHERE user_id = ?;", anger, id);
    }

    public void Init() {}
}	