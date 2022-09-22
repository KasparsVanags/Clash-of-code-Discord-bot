namespace clash_of_code_bot;

public class Player
{
    public int codingamerId { get; set; }
    public string codingamerNickname { get; set; }
    public long codingamerAvatarId { get; set; }
    public int duration { get; set; }
    public string status { get; set; }
}

public class Lobby
{
    public int nbPlayersMin { get; set; }
    public int nbPlayersMax { get; set; }
    public string publicHandle { get; set; }
    public string clashDurationTypeId { get; set; }
    public string creationTime { get; set; }
    public string startTime { get; set; }
    public long startTimestamp { get; set; }
    public int msBeforeStart { get; set; }
    public bool finished { get; set; }
    public bool started { get; set; }
    public bool publicClash { get; set; }
    public List<Player> players { get; set; }
    public List<string> programmingLanguages { get; set; }
    public List<string> modes { get; set; }
    public string type { get; set; }
}