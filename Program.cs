namespace KyodaiBot;

internal class Program
{
    public static async Task<int> Main(string[] args) // ← Обязательно async Task<int>
    {
        string ourClanTag = "#2JYQJYVJ8"; // Kyodai

        string? explicitTgToken = null;
        string? explicitCocToken = null;

        var actualTgToken = explicitTgToken ?? Environment.GetEnvironmentVariable("TELEGRAM_API_TOKEN")!;
        var actualCocToken = explicitCocToken ?? Environment.GetEnvironmentVariable("CLASH_API_TOKEN")!;

        var clash = new ClashApi(actualCocToken);
        var bot = new Bot(actualTgToken, actualCocToken, ref clash, ourClanTag);
        var watchdog = new Watchdog(ref clash, ourClanTag, 90000);

        Console.WriteLine($"@{bot.Me.Username} is running... Press Ctrl+C to terminate");

        watchdog.Start();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            bot.Cts.Cancel();
        };

        try
        {
            await Task.Delay(-1, bot.Cts.Token); // Block indefinitely until cancellation
        }
        catch (TaskCanceledException)
        { } // Ignore

        watchdog.Stop();
        return 0;
    }
}