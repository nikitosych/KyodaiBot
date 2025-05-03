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
}
