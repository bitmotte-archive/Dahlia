using NetCord.Rest;
using SQLite;

namespace Dahlia.Leveling;

public class LevelsDatabaseHandler {

    private SQLiteConnection _db;
    
    public LevelsDatabaseHandler() {
        
        _db = new SQLiteConnection("Databases/levels.db");
        _db.CreateTable<Levels>();	
    }

    public void AddUser(ulong user)
    {
        List<Levels> result = _db.Query<Levels>("SELECT * FROM levels WHERE user_id = ?;", user);
        var alreadyExists = result.Count > 0;

        if(!alreadyExists)
        {
            Console.WriteLine("User doesn't exist, registering . . .");
            _db.Query<Levels>("INSERT INTO levels VALUES (?, 0, 0);", user);
        }
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
        Levels result = _db.FindWithQuery<Levels>("SELECT * FROM levels WHERE user_id = ?;", id);
        return result;
    }

    public void UpdateUserLevel(ulong id, int level, int exp)
    {
        _db.Query<Levels>("UPDATE levels SET level = ? WHERE user_id = ?;", level, id);
        _db.Query<Levels>("UPDATE levels SET exp = ? WHERE user_id = ?;", exp, id);
    }
}	