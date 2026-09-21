using NetCord;

public class DiscordCacheService : IService
{
    public User SelfUser {
        get
        {
            if (_selfUser == null) throw new Exception("not cached");
            return _selfUser;
        }
        set
        {
            _selfUser = value;
        } 
    }
    private User? _selfUser;

    public void Init()
    {
        
    }
}