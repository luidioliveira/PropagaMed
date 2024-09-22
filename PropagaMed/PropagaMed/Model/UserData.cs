using SQLite;

public class UserData
{
    [PrimaryKey, AutoIncrement, NotNull]
    public int Id { get; set; }
    public string Mail { get; set; }
    public string Pin { get; set; }
    public int CountLogin { get; set; }
}