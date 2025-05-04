using KyodaiBot.Models.Base;

namespace KyodaiBot.Models;

// WarLog myDeserializedClass = JsonConvert.DeserializeObject<WarLog>(myJsonResponse);
public partial class BadgeUrls
{
}

public partial class Clan
{
    public string tag { get; set; }
    public string name { get; set; }
    public BadgeUrls badgeUrls { get; set; }
    public int clanLevel { get; set; }
    public int attacks { get; set; }
    public int stars { get; set; }
    public double destructionPercentage { get; set; }
    public int expEarned { get; set; }
}

public partial class Cursors
{
}

public partial class Item
{
    public string result { get; set; }
    public int teamSize { get; set; }
    public int attacksPerMember { get; set; }
    public string battleModifier { get; set; }
    public Clan clan { get; set; }
    public Opponent opponent { get; set; }
}

public class Opponent
{
    public string tag { get; set; }
    public string name { get; set; }
    public BadgeUrls badgeUrls { get; set; }
    public int clanLevel { get; set; }
    public int stars { get; set; }
    public double destructionPercentage { get; set; }
}

public partial class Paging
{
}

public class WarLog : Response
{
    public List<Item> items { get; set; }
    public Paging paging { get; set; }
}

