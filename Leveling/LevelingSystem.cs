using NetCord.Rest;

namespace Dahlia.Leveling;

public interface ILevelingSystem : IService
{
    public Task ProcessUser(RestClient client, ulong userid);
}

public class LevelingSystem(ILevelingStore store) : ILevelingSystem
{
    private readonly ILevelingStore store = store;

    public void Init() {}

    public async Task ProcessUser(RestClient client, ulong userid)
    {
        store.EnsureUser(userid);
        store.StartTx();

        try
        {
            Levels userLevels = store.GetLevelByUser(userid);

            userLevels.Exp += 1;
            int maxExp = 30 + (userLevels.Level * 3);
            
            if (userLevels.Exp > maxExp)
            {
                userLevels.Level++;
                userLevels.Exp = 0;

                await client.SendMessageAsync(1504214992034070693, $"<@{userid}> Has reached level {userLevels.Level} !!");
            }

            store.UpdateUserLevel(userLevels.Id, userLevels.Level, userLevels.Exp);
            store.CommitTx();
        } catch(Exception e)
        {
            Console.WriteLine(e);
            store.RollbackTx();
        }
    }
}