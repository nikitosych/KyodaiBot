using System.Security.Cryptography.X509Certificates;
using KyodaiBot.Models;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace KyodaiBot
{
    public static class Saver
    {
        private static readonly string StoragePath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
            "Storage"
        );

        private static readonly string UsersFileName = "authedUsers.json";

        public static void Save<TModel>(TModel json, string modelFileName)
        {
            string parsed = JsonConvert.SerializeObject(json, Formatting.Indented);
            string fullPath = Path.Combine(StoragePath, modelFileName);

            Directory.CreateDirectory(StoragePath);
            File.WriteAllText(fullPath, parsed);
        }

        public static TModel? Load<TModel>(string modelFileName)
        {
            string fullPath = Path.Combine(StoragePath, modelFileName);

            if (!File.Exists(fullPath))
                return default;

            string json = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<TModel>(json);
        }

        public static void Clear(string modelFileName)
        {
            string fullPath = Path.Combine(StoragePath, modelFileName);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public static void Empty(string modelFileName)
        {
            string fullPath = Path.Combine(StoragePath, modelFileName);
            if (File.Exists(fullPath))
                File.WriteAllText(fullPath, string.Empty);
        }

        public static void SaveUsers(List<VerifiedUser> users)
        {
            Save(users, UsersFileName);
        }

        public static List<VerifiedUser> LoadUsers()
        {
            return Load<List<VerifiedUser>>(UsersFileName) ?? new List<VerifiedUser>();
        }

        public static void AddUser(VerifiedUser newUser)
        {
            var users = LoadUsers();

            var existingIndex = users.FindIndex(u => u.User.Id == newUser.User.Id);
            if (existingIndex >= 0)
                users[existingIndex] = newUser;
            else
                users.Add(newUser);

            SaveUsers(users);
        }

        public static bool TryGetUser(long userId, out VerifiedUser? user)
        {
            var users = LoadUsers();
            user = users.FirstOrDefault(u => u.User.Id == userId);
            return user != null;
        }

        public static void AddWarn(Player player, string reason)
        {
            // ReSharper disable once StringLiteralTypo
            var filename = "playerwarnings.json";
            var file = Load<List<PlayerWarnings>>(filename);

            if (file == null)
            {
                List<PlayerWarnings> pwl = [new PlayerWarnings(player)];
                pwl[0].Warnings.Add(new Warning(reason, DateTime.UtcNow));
                Save(pwl, filename);
            }
            else
            {
                var pw = file.Find(el => el.Player.tag == player.tag);

                if (pw == null)
                {
                    pw = new PlayerWarnings(player);
                    pw.Warnings.Add(new Warning(reason, DateTime.UtcNow));
                    file.Add(pw);
                }
                else
                {
                    pw.Warnings.Add(new Warning(reason, DateTime.UtcNow));
                }

                Save(file, filename);
            }

        }

        public static bool RemoveWarn(Player player)
        {
            // ReSharper disable once StringLiteralTypo
            var filename = "playerwarnings.json";
            var file = Load<List<PlayerWarnings>>(filename);
            if (file == null)
                return false;
            var pw = file.Find(el => el.Player.tag == player.tag);
            if (pw == null)
                return false;
            if (pw.Warnings.Count > 0)
                pw.Warnings.RemoveAt(pw.Warnings.Count - 1);
            else
                file.Remove(pw);
            Save(file, filename);
            return true;
        }

        public static List<PlayerWarnings> GetWarnings()
        {
            // ReSharper disable once StringLiteralTypo
            var filename = "playerwarnings.json";
            var file = Load<List<PlayerWarnings>>(filename);
            if (file == null)
                return new List<PlayerWarnings>();
            return file;
        }
    }

    public class VerifiedUser(User user, Player player)
    {
        public User User = user;
        public Player Player = player;
    }
    public class Ban(Player player, int durationDays, string reason)
    {
        public Player Player = player;
        public DateTime Duration = DateTime.UtcNow + TimeSpan.FromDays(durationDays);
        public string Reason = reason;
    }
    public class PlayerWarnings(Player player)
    {
        public Player Player = player;
        public List<Warning> Warnings = [];
    }
    public class Warning(string reason, DateTime date)
    {
        public string Reason = reason;
        public DateTime Date = date;
    }
}
