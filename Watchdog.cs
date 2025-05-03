using System.Timers;

namespace KyodaiBot
{
    public class Watchdog
    {
        private readonly System.Timers.Timer _timer;
        private readonly string _banFile = "cwbanlist.txt";

        public Watchdog(double intervalMs = 60000) // по умолчанию проверка каждую минуту
        {
            _timer = new System.Timers.Timer(intervalMs);
            _timer.Elapsed += OnCheck;
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

        private void OnCheck(object? sender, ElapsedEventArgs e)
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

                    // Можно также вызвать обновление Telegram-сообщения о банах
                    // Например: await CwBanned?.Invoke(...); если в асинхронном контексте
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Watchdog] Ошибка при проверке банов: {ex.Message}");
            }
        }
    }
}