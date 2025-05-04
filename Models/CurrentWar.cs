namespace KyodaiBot.Models.CurrentWar; // приходится юзать отдельный неймспейс так как api возвращает Clan.members разных типов в зависимости от запроса

public class BadgeUrls
{
    public string small { get; set; }
    public string large { get; set; }
    public string medium { get; set; }
}

public class Clan
{
    public string tag { get; set; }
    public string name { get; set; }
    public BadgeUrls badgeUrls { get; set; }
    public int? clanLevel { get; set; }
    public int? attacks { get; set; }
    public int? stars { get; set; }
    public int? destructionPercentage { get; set; }
    public List<Member> members { get; set; }
}

public class Member
{
    public string tag { get; set; }
    public string name { get; set; }
    public int? townhallLevel { get; set; }
    public int? mapPosition { get; set; }
    public int? opponentAttacks { get; set; }
}

public class Opponent
{
    public string tag { get; set; }
    public string name { get; set; }
    public BadgeUrls badgeUrls { get; set; }
    public int? clanLevel { get; set; }
    public int? attacks { get; set; }
    public int? stars { get; set; }
    public int? destructionPercentage { get; set; }
    public List<Member> members { get; set; }
}

public class CurrentWar
{
    public WarState state { get; set; }
    public int? teamSize { get; set; }
    public int? attacksPerMember { get; set; }
    public string battleModifier { get; set; }
    public string preparationStartTime { get; set; }
    public string startTime { get; set; }
    public string endTime { get; set; }
    public Clan clan { get; set; }
    public Opponent opponent { get; set; }
}

public enum WarState
{
    CLAN_NOT_FOUND, 
    ACCESS_DENIED, 
    NOT_IN_WAR, 
    IN_MATCHMAKING, 
    ENTER_WAR, 
    MATCHED, 
    PREPARATION, 
    WAR, 
    IN_WAR, 
    ENDED
}