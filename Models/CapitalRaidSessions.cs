// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
namespace KyodaiBot.Models;
public class Attack
{
    public Attacker attacker { get; set; }
    public int destructionPercent { get; set; }
    public int stars { get; set; }
}

public class Attacker
{
    public string tag { get; set; }
    public string name { get; set; }
    public int level { get; set; }
    public BadgeUrls badgeUrls { get; set; }
}

public class AttackLog
{
    public Defender defender { get; set; }
    public int attackCount { get; set; }
    public int districtCount { get; set; }
    public int districtsDestroyed { get; set; }
    public List<District> districts { get; set; }
}

public partial class BadgeUrls
{
}

public partial class Cursors
{
}

public class Defender
{
    public string tag { get; set; }
    public string name { get; set; }
    public int level { get; set; }
    public BadgeUrls badgeUrls { get; set; }
}

public class DefenseLog
{
    public Attacker attacker { get; set; }
    public int attackCount { get; set; }
    public int districtCount { get; set; }
    public int districtsDestroyed { get; set; }
    public List<District> districts { get; set; }
}

public partial class District
{
    public int id { get; set; }
    public string name { get; set; }
    public int districtHallLevel { get; set; }
    public int destructionPercent { get; set; }
    public int stars { get; set; }
    public int attackCount { get; set; }
    public int totalLooted { get; set; }
    public List<Attack> attacks { get; set; }
}

public partial class Item
{
    public string state { get; set; }
    public string startTime { get; set; }
    public string endTime { get; set; }
    public int capitalTotalLoot { get; set; }
    public int raidsCompleted { get; set; }
    public int totalAttacks { get; set; }
    public int enemyDistrictsDestroyed { get; set; }
    public int offensiveReward { get; set; }
    public int defensiveReward { get; set; }
    public List<Member> members { get; set; }
    public List<AttackLog> attackLog { get; set; }
    public List<DefenseLog> defenseLog { get; set; }
}

public class Member
{
    public string tag { get; set; }
    public string name { get; set; }
    public int attacks { get; set; }
    public int attackLimit { get; set; }
    public int bonusAttackLimit { get; set; }
    public int capitalResourcesLooted { get; set; }
}

public partial class Paging
{
    public Cursors cursors { get; set; }
}

public class CapitalRaidSessions : Response
{
    public List<Item> items { get; set; }
    public Paging paging { get; set; }
}
