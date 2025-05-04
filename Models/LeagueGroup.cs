

using KyodaiBot.Models.Base;

namespace KyodaiBot.Models.LeagueGroup;

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
    public int? clanLevel { get; set; }
    public BadgeUrls badgeUrls { get; set; }
    public List<Member> members { get; set; }
}

public class Member
{
    public string tag { get; set; }
    public string name { get; set; }
    public int? townHallLevel { get; set; }
}

public class LeagueGroup : Response
{
    public LeagueState state { get; set; }
    public string season { get; set; }
    public List<Clan> clans { get; set; }
    public List<Round> rounds { get; set; }
}

public class Round
{
    public List<string> warTags { get; set; }
}

public enum LeagueState
{
    GROUP_NOT_FOUND, 
    NOT_IN_WAR, 
    PREPARATION, 
    WAR, 
    ENDED
}