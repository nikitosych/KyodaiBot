using System.Timers;
using KyodaiBot.Models;
using KyodaiBot.Models.CurrentWar;

namespace KyodaiBot
{
    public class Watchdog
    {
        private readonly System.Timers.Timer _timer;
        private readonly string _banFile = "cwbanlist.txt";
        private DateTime _lastRaidEventDate = DateTime.MinValue;
        private ClashApi _clash;

        private string _clanTag;

        public Watchdog(ref ClashApi clash, string clanTag, double intervalMs = 60000)
        {
            _clash = clash;
            _clanTag = clanTag;
            _timer = new System.Timers.Timer(intervalMs);

            _timer.Elapsed += CheckBans;
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

        private async Task CheckWarPreparation(object? sender, ElapsedEventArgs? e)
        {
            // ReSharper disable once StringLiteralTypo
            var war = await _clash.Get<CurrentWar>($"/clans/{_clanTag}/currentwar");

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
    }
}