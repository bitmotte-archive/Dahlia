using SQLite;

namespace Dahlia.Mentioning;

[Table("anger")]	 
public class Anger		
{		
    [PrimaryKey]
    [Column("user_id")]		
    public ulong Id { get; set; }	

    [Column("sentiment")]
    public int Sentiment { get; set; }	
}	