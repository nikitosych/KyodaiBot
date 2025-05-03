namespace KyodaiBot.Models;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public partial class BuilderBaseLeague
{
    public int id { get; set; }
    public string name { get; set; }
}

public partial class Cursors
{
}

public partial class Element
{
    public string type { get; set; }
    public int id { get; set; }
}

public partial class IconUrls
{
    public string small { get; set; }
    public string tiny { get; set; }
    public string medium { get; set; }
}

public partial class Item
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

public enum Roles
{
    NOT_MEMBER,
    MEMBER,
    LEADER,
    ADMIN,
    COLEADER
}

public partial class League
{
    public int id { get; set; }
    public string name { get; set; }
    public IconUrls iconUrls { get; set; }
}

public partial class Paging
{

}

public partial class PlayerHouse
{
    public List<Element> elements { get; set; }
}

public class Members : Response
{
    public List<Item> items { get; set; }
    public Paging paging { get; set; }
}

