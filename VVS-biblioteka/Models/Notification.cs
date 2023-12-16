using VVS_biblioteka.Models;

public class Notification
{
    public int Id { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
}

