using KyodaiBot.Models;
using KyodaiBot.Models.Base;

namespace KyodaiBot.Models;

// Clan myDeserializedClass = JsonConvert.DeserializeObject<Clan>(myJsonResponse);
public partial class BadgeUrls
{
}

public partial class BuilderBaseLeague
{
}

public class CapitalLeague
{
    public int id { get; set; }
    public string name { get; set; }
}

public class ChatLanguage
{
    public int id { get; set; }
    public string name { get; set; }
    public string languageCode { get; set; }
}

public class ClanCapital
{
    public int capitalHallLevel { get; set; }
    public List<District> districts { get; set; }
}

public partial class District
{
}

public partial class Element
{
}

public partial class IconUrls
{
}

public partial class Label
{
    public int id { get; set; }
    public string name { get; set; }
    public IconUrls iconUrls { get; set; }
}

public partial class League
{
}

public class MemberList
{
    public string tag { get; set; }
    public string name { get; set; }
    public Roles role { get; set; }
    public int townHallLevel { get; set; }
    public int expLevel { get; set; }
    public League league { get; set; }
    public int trophies { get; set; }
    public int builderBaseTrophies { get; set; }
    public int clanRank { get; set; }
    public int previousClanRank { get; set; }
    public int donations { get; set; }
    public int donationsReceived { get; set; }
    public PlayerHouse playerHouse { get; set; }
    public BuilderBaseLeague builderBaseLeague { get; set; }
}

public partial class PlayerHouse
{
}

public partial class Clan : Response
{
    public string type { get; set; }
    public string description { get; set; }
    public bool isFamilyFriendly { get; set; }
    public int clanPoints { get; set; }
    public int clanBuilderBasePoints { get; set; }
    public int clanCapitalPoints { get; set; }
    public CapitalLeague capitalLeague { get; set; }
    public int requiredTrophies { get; set; }
    public string warFrequency { get; set; }
    public int warWinStreak { get; set; }
    public int warWins { get; set; }
    public int warTies { get; set; }
    public int warLosses { get; set; }
    public bool isWarLogPublic { get; set; }
    public WarLeague warLeague { get; set; }
    public int members { get; set; }
    public List<MemberList> memberList { get; set; }
    public List<Label> labels { get; set; }
    public int requiredBuilderBaseTrophies { get; set; }
    public int requiredTownhallLevel { get; set; }
    public ClanCapital clanCapital { get; set; }
    public ChatLanguage chatLanguage { get; set; }
}

public class WarLeague : Response
{
    public int id { get; set; }
    public string name { get; set; }
}

