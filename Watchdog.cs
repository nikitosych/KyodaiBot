using System.Timers;
using KyodaiBot.Models.CurrentWar;
using KyodaiBot.Models.LeagueGroup;

namespace KyodaiBot
{
    public class Watchdog
    {
        private readonly ClashApi _clash;
        private readonly System.Timers.Timer _timer;
        private readonly string _banFile = "cwbanlist.txt";
        private DateTime _lastRaidEventDate = DateTime.MinValue;
        private readonly string _clanTag;

        public Watchdog(ref ClashApi clash, string clanTag, double intervalMs = 60000)
        {
            _clash = clash;
            _clanTag = clanTag;
            _timer = new System.Timers.Timer(intervalMs);

            _timer.Elapsed += CheckBans;
            _timer.Elapsed += CheckWarPreparation;
            _timer.Elapsed += CheckRaid;

            _timer.AutoReset = true;
        }

        public void Start()
        {
            Console.WriteLine("[Watchdog] Старт мониторинга банов...");
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private async void CheckCWL(object? sender, ElapsedEventArgs? e)
        {
            try
            {
                // ReSharper disable once StringLiteralTypo
                const string filename = "currentcwl.txt";
                var leagueGroup = await _clash.GetLeagueGroup(_clanTag);

                var cached = Saver.Load<LeagueGroup>(filename);

                if (leagueGroup != null && leagueGroup.clans[0].tag == cached.clans[0].tag)
                    return;
                

                if (leagueGroup == null)
                {
                    Console.WriteLine("[Watchdog] Ошибка при получении информации о Лиге Кланов.");
                    return;
                }
                if (leagueGroup.state == LeagueState.PREPARATION)
                {
                    Console.WriteLine("[Watchdog] Началась подготовка к ЛВК!");
                }
                if (leagueGroup.state == LeagueState.WAR)
                {
                    Console.WriteLine("[Watchdog] Началась Лига Кланов!");
                    WatchdogEvents.OnCWLStarted(leagueGroup);
                }

                Saver.Clear(filename);
                Saver.Save(leagueGroup, filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Watchdog] Ошибка при проверке Лиги Кланов: {ex.Message}");
            }
        }

        private async void CheckWarPreparation(object? sender, ElapsedEventArgs? e)
        {
            try
            {
                // ReSharper disable once StringLiteralTypo
                const string filename = "currentwar.txt";
                var war = await _clash.GetCurrentWar(_clanTag);

                var cached = Saver.Load<CurrentWar>(filename);

                if (cached != null && cached.opponent.tag == war?.opponent.tag)
                    return;
                Saver.Clear(filename);
                Saver.Save(war, filename);

                if (war == null)
                {
                    Console.WriteLine("[Watchdog] Ошибка при получении информации о войне.");
                    return;
                }
                if (war.state == WarState.PREPARATION)
                {
                    Console.WriteLine("[Watchdog] Началась подготовка к войне!");
                    WatchdogEvents.OnWarPreparationStarted(war);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Watchdog] Ошибка при проверке войны: {ex.Message}");
            }
        }

        private void CheckRaid(object? sender, ElapsedEventArgs? e)
        {
            DateTime utcNow = DateTime.UtcNow;

            if (utcNow.DayOfWeek == DayOfWeek.Friday &&
                utcNow.Hour == 7 && utcNow.Minute == 0 &&
                _lastRaidEventDate.Date != utcNow.Date)
            {
                _lastRaidEventDate = utcNow.Date;
                Console.WriteLine($"[Watchdog] Открыта сессия рейдов! (Последний раз было: {_lastRaidEventDate:yy-MMM-dd ddd})");
                WatchdogEvents.OnCapitalRaidStarted();
            }
        }

        private void CheckBans(object? sender, ElapsedEventArgs? e)
        {
            try
            {
                var banlist = Saver.Load<List<Ban>>(_banFile);

                if (banlist == null || banlist.Count == 0)
                    return;

                var now = DateTime.UtcNow;
                var expired = banlist.Where(b => b.Duration <= now).ToList();

                if (expired.Count > 0)
                {
                    foreach (var ban in expired)
                    {
                        Console.WriteLine($"[Watchdog] Снятие бана: {ban.Player.name} ({ban.Player.tag}) — срок истёк.");
                        banlist.Remove(ban);
                    }

                    Saver.Save(banlist, _banFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Watchdog] Ошибка при проверке банов: {ex.Message}");
            }
        }
    }

    static class WatchdogEvents
    {
        public delegate void CapitalRaidStarted();
        public static event CapitalRaidStarted? CapitalRaidStartedEvent;
        public static void OnCapitalRaidStarted()
        {
            CapitalRaidStartedEvent?.Invoke();
        }

        public delegate void WarPreparationStarted(CurrentWar war);
        public static event WarPreparationStarted? WarPreparationStartedEvent;
        public static void OnWarPreparationStarted(CurrentWar war)
        {
            WarPreparationStartedEvent?.Invoke(war);
        }

        public delegate void CWLStarted(LeagueGroup leagueGroup);
        public static event CWLStarted? CWLStartedEvent;
        public static void OnCWLStarted(LeagueGroup leagueGroup)
        {
            CWLStartedEvent?.Invoke(leagueGroup);
        }
    }
}