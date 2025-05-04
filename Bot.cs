using System.Text;
using KyodaiBot.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace KyodaiBot;

public class Bot
{
    public readonly User Me;
    public readonly CancellationTokenSource Cts;
    public readonly TelegramBotClient Client;

    private readonly ClashApi _clash;

    private readonly long _allowedGroupId;
    private readonly string _defaultClanTag;

    private readonly Dictionary<long, string> _authStartHashes;
    private readonly Dictionary<long, Player> _authPlayersStorage;
    private readonly Dictionary<long, int> _authSeqSteps;

    private delegate Task CwBanHandler(ChatId chatId);

    private event CwBanHandler CwBanned;

    public async Task ListBans(ChatId chatId)
    {
        var banlist = Saver.Load<List<Ban>>("cwbanlist.txt");
        if (banlist == null) return;

        if (banlist.Count == 0)
        {
            await Client.SendMessage(chatId, "✅ Игроков в бане нету.");
            return;
        }

        var msg = "‼️ Этих игроков <strong>не брать</strong> на КВ ‼️\n";

        for (var i = 0; i < banlist.Count; i++)
        {
            var ban = banlist[i];
            msg += $"""
                    {i + 1}.
                    <strong>Имя:</strong> {ban.Player.name} ({ban.Player.tag}),
                    <strong>Причина:</strong> {ban.Reason},
                    <strong>До:</strong> {ban.Duration:dd.MM.yyyy}

                    """;
        }

        await Client.SendMessage(
            chatId,
            msg,
            Telegram.Bot.Types.Enums.ParseMode.Html
        );
    }

    public async Task Warn(ChatId chat, long authorId, string tag, string reason)
    {
        if (Saver.TryGetUser(authorId, out var author) == false)
        {
            await Client.SendMessage(chat, "❌ Ошибка: Вы не верифицированы. Пройдите верификацию командой /verify.");
            return;
        }

        var clan = await _clash.GetClan(_defaultClanTag);
        if (clan == null)
        {
            await Client.SendMessage(chat, "❌ Ошибка вынесения палки на этапе обработки участников клана.");
            return;
        }

        var authorRole = clan.memberList.Find(m => m.tag == author!.Player.tag)!.role;
        if (authorRole is not (Models.Roles.LEADER or Models.Roles.COLEADER))
        {
            await Client.SendMessage(chat, "❌ Ошибка: Вы не имеете прав на вынесение палки. Вам нужна роль \"Глава\" или \"Соруководитель\".");
            return;
        }
        if (string.IsNullOrEmpty(tag) || tag.Length > 11)
        {
            await Client.SendMessage(chat, "❌ Ошибка вынесения палки на этапе обработки тега игрока. Тег не может быть пустым или превышать 10 символов");
            return;
        }
        if (tag[0] != '#')
        {
            tag = "#" + tag;
        }
        var player = await _clash.GetPlayer(tag);
        if (player == null)
        {
            await Client.SendMessage(chat, "❌ Ошибка: Не удалось обработать профиль игрока.");
            return;
        }
        Saver.AddWarn(player, reason);
        await Client.SendMessage(chat, "✅ Успешно вынесена палка.");
    }
    public async Task Unwarn(ChatId chat, long authorId, string tag)
    {
        if (Saver.TryGetUser(authorId, out var author) == false)
        {
            await Client.SendMessage(chat, "❌ Ошибка: Вы не верифицированы. Пройдите верификацию командой /verify.");
            return;
        }
        var clan = await _clash.GetClan(_defaultClanTag);
        if (clan == null)
        {
            await Client.SendMessage(chat, "❌ Ошибка снятия палки на этапе обработки участников клана.");
            return;
        }
        var authorRole = clan.memberList.Find(m => m.tag == author!.Player.tag)!.role;
        if (authorRole is not (Models.Roles.LEADER or Models.Roles.COLEADER))
        {
            await Client.SendMessage(chat, "❌ Ошибка: Вы не имеете прав на снятие палки. Вам нужна роль \"Глава\" или \"Соруководитель\".");
            return;
        }
        if (string.IsNullOrEmpty(tag) || tag.Length > 11)
        {
            await Client.SendMessage(chat, "❌ Ошибка снятия палки на этапе обработки тега игрока. Тег не может быть пустым или превышать 10 символов");
            return;
        }
        if (tag[0] != '#')
        {
            tag = "#" + tag;
        }
        var player = await _clash.GetPlayer(tag);
        if (player == null)
        {
            await Client.SendMessage(chat, "❌ Ошибка: Не удалось обработать профиль игрока.");
            return;
        }
        Saver.RemoveWarn(player);
        await Client.SendMessage(chat, "✅ Палка успешно снята.");
    }

    public async Task ListWarns(ChatId chat)
    {
        var warns = Saver.GetWarnings();
        if (warns == null) return;
        if (warns.Count == 0)
        {
            await Client.SendMessage(chat, "✅ Игроков с предупреждениями нету.");
            return;
        }
        var msg = "‼️ Этим игрокам вынесены палки\n<i>(3 палки -> до свидания)</i>\n";
        for (var i = 0; i < warns.Count; i++)
        {
            var warn = warns[i];
            msg += $"""
                    {i + 1}. {warn.Player.name} ({warn.Player.tag}),
                    <strong>Палки:</strong>
                    
                    """;
            for (var j = 0; j < warn.Warnings.Count; j++)
            {
                var w = warn.Warnings[j];
                msg += $"""
                        {j + 1}. {w.Reason} ({w.Date:dd.MM.yyyy})
                        
                        """;
            }
            if (warn.Warnings.Count >= 3)
            {
                msg += "\n🚪 <i>Этого игрока можно исключать из клана</i>";
            }
        }
        await Client.SendMessage(
            chat,
            msg,
            Telegram.Bot.Types.Enums.ParseMode.Html
        );
    }
    public async Task CwBan(ChatId chat, long authorId, string tag, int? days, bool unban = false, string? reason = null)
    {
        if (Saver.TryGetUser(authorId, out var author) == false)
        {
            await Client.SendMessage(chat, "❌ Ошибка: Вы не верифицированы. Пройдите верификацию командой /verify.");
            return;
        }

        var clan = await _clash.GetClan(_defaultClanTag);
        if (clan == null)
        {
            await Client.SendMessage(chat, "❌ Ошибка введения бана на этапе обработки участников клана.");
            return;
        }

        var authorRole = clan.memberList.Find(m => m.tag == author!.Player.tag)!.role;
        if (authorRole is not (Models.Roles.LEADER or Models.Roles.COLEADER))
        {
            await Client.SendMessage(chat, "❌ Ошибка: Вы не имеете прав на введение бана. Вам нужна роль \"Глава\" или \"Соруководитель\".");
            return;
        }

        if (string.IsNullOrEmpty(tag) || tag.Length > 11)
        {
            await Client.SendMessage(chat, "❌ Ошибка введения бана на этапе обработки тега игрока. Тег не может быть пустым или превышать 10 символов");
            return;
        }
        if (tag[0] != '#')
        {
            tag = "#" + tag;
        }
        if (unban && days != null)
        {
            await Client.SendMessage(chat, "❌ Ошибка: дни не могут быть указаны при разбане.");
            return;
        }
        if (!unban && days == null)
        {
            await Client.SendMessage(chat, "❌ Ошибка: дни должны быть указаны при бане.");
            return;
        }
        if (!unban && !int.TryParse(days.ToString(), out _))
        {
            await Client.SendMessage(chat, "❌ Ошибка: дни должны быть числом.");
            return;
        }
        if (!unban && reason == null)
        {
            await Client.SendMessage(chat, "❌ Ошибка: причина должна быть указана при бане.");
            return;
        }
        if (days != null && (int)days < 0)
        {
            await Client.SendMessage(chat, "❌ Ошибка: дни не могут быть отрицательными.");
            return;
        }
        if (days != null && (int)days > 14)
        {
            await Client.SendMessage(chat, "❌ Ошибка: дни бана не могут превышать 14.");
            return;
        }


        var player = await _clash.GetPlayer(tag);
        var file = "cwbanlist.txt";

        if (player == null)
        {
            await Client.SendMessage(chat, "❌ Ошибка введения бана на этапе обработки профиля игрока.");
            return;
        }

        var member = clan.memberList.Find(m => m.tag == tag);
        var pname = member?.name;

        var banlist = Saver.Load<List<Ban>>(file);

        // разбан
        if (unban)
        {
            if (banlist == null)
            {
                Console.WriteLine($"[unban] {file} does not exist or is empty. Returning.");
                return;
            }

            var i = banlist.FindIndex(b => b.Player.tag == tag);

            if (i == -1)
            {
                Console.WriteLine($"[unban] Cannot find \"{tag}\" in {file}. Returning");

                await Client.SendMessage(chat, $"❌ Игрока {pname ?? tag} нету в списке банов");

                return;
            }

            banlist.RemoveAt(i);

            Saver.Save(banlist, file);

            Console.WriteLine($"[unban] Saved {file}");
            await Client.SendMessage(chat, $"✅ Игрок был удален из КВ банов.");
            await CwBanned.Invoke(chat);
            return;
        }

        // бан
        if (banlist == null)
        {
            banlist = new List<Ban>();
        }

        if (banlist.Exists(b => b.Player.tag == tag))
        {
            await Client.SendMessage(chat, $"❌ Игрок уже в бане.");
            return;
        }

        banlist.Add(new Ban(player, (int)days!, reason!));

        Saver.Save(banlist, file);
        Console.WriteLine($"[ban] Saved {file}");

        await Client.SendMessage(chat, $"✅ Игрок {pname} был добавлен в КВ баны на {days} дней.\nПричина: {reason}");

        await CwBanned.Invoke(chat);


        return;
    }
    private static readonly Dictionary<Roles, string> Roles = new()
    {
        { Models.Roles.LEADER, "Глава" },
        { Models.Roles.COLEADER, "Соруководитель" },
        { Models.Roles.ADMIN, "Старейшина" },
        { Models.Roles.MEMBER, "Соклановец" },
        { Models.Roles.NOT_MEMBER, "???" }
    };
    public async Task SendMemberProfile(ChatId chat, User user)
    {
        if (!Saver.TryGetUser(user.Id, out var player))
        {
            await Client.SendMessage(chat, $"""
                                           Упс, @{user.Username}! А тебя то нету в моей базе. 
                                           Попробуй сначала пройти верификацию командой /verify
                                           """);
            return;
        }


        var p = await _clash.GetPlayer(player!.Player.tag);
        var ms = await _clash.GetMembers(_defaultClanTag);

        if (p == null)
        {
            await Client.SendMessage(chat, "❌ Ошибка. По какой-то причине мне не удалось обработать твой профиль.");
        }

        await Client.SendMessage(chat, $"""
                                         @{user.Username}, вот твоя игровая информация:
                                         <strong>Имя:</strong> {p.name} ({p.tag}),
                                         <strong>Подтвержден:</strong> {(Saver.LoadUsers().Exists(u => u.Player.tag == p.tag) ? "✅" : "❌")},
                                         <strong>Клан:</strong> {p.clan.name} {(p.clan.tag == _defaultClanTag ? "🏰" : "(Чужой)")},
                                         {(ms != null && ms.items.Exists(m => m.tag == p.tag) ? $"<strong>Роль:</strong> {Roles[ms.items.Find(m => m.tag == p.tag)!.role]}" : "")},
                                          <strong>ТХ:</strong> {p.townHallLevel}, <strong>Уровень:</strong> {p.expLevel}, 
                                          <strong>Кубки:</strong> {p.trophies} 🏆
                                         """, ParseMode.Html);
    }
    public async Task SendClanMembers(ChatId chat)
    {
        List<Item> GetSortedMembers(List<Item> items)
        {
            var roleOrder = new List<Roles> { Models.Roles.LEADER, Models.Roles.COLEADER, Models.Roles.ADMIN, Models.Roles.MEMBER };

            return items
                .Where(m => roleOrder.Contains(m.role))
                .OrderByDescending(m => m.trophies)
                .ToList();
        }

        var members = await _clash.GetMembers(_defaultClanTag);

        if (members == null || members.items == null)
        {
            await Client.SendMessage(chat, "❌ Не удалось получить список участников клана.");
            return;
        }

        int pageSize = 5;
        int totalPages = (int)Math.Ceiling(members.items.Count / (double)pageSize);
        int currentPage = 1;

        var sortedMembers = GetSortedMembers(members.items);
        string message = FormatMembersPage(sortedMembers, currentPage, pageSize);

        var keyboard = GeneratePageKeyboard(currentPage, totalPages);

        await Client.SendMessage(chat, message, parseMode: ParseMode.Html, replyMarkup: keyboard);
    }
    string FormatMembersPage(List<Item> members, int page, int pageSize)
    {
        int start = (page - 1) * pageSize;
        var pageItems = members.Skip(start).Take(pageSize).ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"🏰 Список наших соклановцев: <strong>{members.Count}</strong>\nСтраница <strong>{page}</strong>\n");

        for (int i = 0; i < pageItems.Count; i++)
        {
            var m = pageItems[i];
            sb.AppendLine($"""
                               {start + i + 1}.
                               <strong>Имя:</strong> {m.name} ({m.tag}),
                               <strong>Подтвержден:</strong> {(Saver.LoadUsers().Exists(u => u.Player.tag == m.tag) ? "✅" : "❌")},
                               <strong>Роль:</strong> {Roles[m.role]},
                               <strong>ТХ:</strong> 🛖{m.townHallLevel}, <strong>Уровень:</strong> ⭐{m.expLevel}, 
                               <strong>Кубки:</strong> {m.trophies} 🏆
                           """);
        }

        return sb.ToString();
    }
    InlineKeyboardMarkup GeneratePageKeyboard(int currentPage, int totalPages)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        var row = new List<InlineKeyboardButton>();
        for (int i = 1; i <= totalPages; i++)
        {
            row.Add(InlineKeyboardButton.WithCallbackData(i.ToString(), $"page_{i}<{currentPage}"));
        }

        buttons.Add(row.ToArray());
        return new InlineKeyboardMarkup(buttons);
    }

    async Task RequestAuth(ChatId chat, User user, bool welcome = false)
    {
        if (!_authStartHashes.ContainsKey(user.Id)) { _authStartHashes.Add(user.Id, GenerateRandomString(6)); }
        string message = welcome ?
            $"""
            Добро пожаловать, @{user.Username}!
            Пожалуйста, перейди по ссылке для верификации и нажми кнопку "Start".
            https://t.me/{Me.Username}?start={_authStartHashes[user.Id]}
            """ :
            $"""
            Пройдите по ссылке для верификации:
            https://t.me/{Me.Username}?start={_authStartHashes[user.Id]}
            """;

        await Client.SendMessage(chat, message);
    }

    async Task AuthSequence(User user, ChatId chatId, Message? replyMessage = null)
    {
        void rem(long id)
        {
            _authStartHashes.Remove(id);
            _authPlayersStorage.Remove(id);
        }

        if (!_authStartHashes.ContainsKey(user.Id))
            return;

        string[] messages = new[]
        {
            """
            Скинь мне тег своего профиля в игре в формате <code>#1234ABCD</code>.
            """,
            """
            Отлично!
            Скинь теперь свой API токен, чтобы я мог подтвердить, что профиль действительно твой.
            Не волнуйся, он одноразовый и годится только для верификации!
            
            Подробнее: https://developer.clashofclans.com/#/documentation
            """,
            """❤️ Спасибо! Внёс тебя в базу наших соклановцев."""
        };

        if (Saver.TryGetUser(user.Id, out var authedUser))
        {
            await Client.SendMessage(chatId, $"""
                                                  ❌ Ошибка! Данный пользователь уже есть в базе!

                                                  Тег: {authedUser.Player.tag}
                                                  Ник в игре: {authedUser.Player.name}
                                                  """);
            _authStartHashes.Remove(user.Id);
            return;
        }

        if (replyMessage == null)
        {
            _authSeqSteps.TryAdd(user.Id, 1);
            await Client.SendMessage(chatId, messages[0], ParseMode.Html);
            return;
        }

        if (_authSeqSteps.ContainsKey(user.Id) && _authSeqSteps[user.Id] == 1)
        {
            string gameTag = replyMessage.Text!;

            var player = await _clash.GetPlayer(gameTag);

            if (player == null)
            {
                rem(user.Id);
                await Client.SendMessage(chatId, "❌ Ошибка, не удалось обработать профиль игрока.");
                return;
            }
            if (player.reason != null)
            {
                await Client.SendMessage(chatId,
                    $"❌ Ошибка, не удалось обработать профиль игрока.\nПричина: <code>{player.reason}</code>",
                    ParseMode.Html);
                rem(user.Id);
                return;
            }

            if (player.clan.tag != _defaultClanTag)
            {
                await Client.SendMessage(chatId,
                    $"❌ Чтобы продолжить, вы должны быть в нашем клане (тег: #2JYQJYVJ8)\nТекущий клан: {player.clan.name}");
                rem(user.Id);
                return;
            }
            _authPlayersStorage.Add(user.Id, player);
            _authSeqSteps[user.Id] = 2;
            await Client.SendMessage(chatId, messages[1]);
        }
        else if (_authSeqSteps.ContainsKey(user.Id) && _authSeqSteps[user.Id] == 2)
        {
            string gameToken = replyMessage.Text!;

            var verified = await _clash.ValidatePlayer(_authPlayersStorage[user.Id].tag, gameToken);

            if (verified == null)
            {
                rem(user.Id);
                return;
            }
            if (verified.reason != null)
            {
                await Client.SendMessage(chatId,
                    $"❌ Ошибка, не удалось обработать профиль игрока.\nПричина: <code>{verified.reason}</code>", ParseMode.Html);
                rem(user.Id);
                return;
            }

            if (verified.status != "ok")
            {
                await Client.SendMessage(chatId,
                    $"❌ Ошибка, не удалось обработать профиль игрока.\nПричина: <code>{verified.status}</code>", ParseMode.Html);
                rem(user.Id);
                return;
            }

            await Client.SendMessage(chatId, messages[2]);
            Saver.AddUser(new VerifiedUser(user, _authPlayersStorage[user.Id]));

            Console.WriteLine($"[Auth] User {user.Username} ({user.Id}) verified with tag {_authPlayersStorage[user.Id].tag}");

            await Client.PromoteChatMember(_allowedGroupId, user.Id, false, false, false, false, false, false, false,
                false, false, false, false, false, true, false, false);

            Console.WriteLine($"[Auth] User {user.Username} ({user.Id}) promoted to member in group {_allowedGroupId}");


            var pn = authedUser?.Player.name;

            if (pn != null)
                await Client.SetChatAdministratorCustomTitle(_allowedGroupId, user.Id, pn);

            rem(user.Id);
        }
    }

    async Task OnTextMessage(Message msg) // received a text message that is not a command
    {
        Console.WriteLine($"Received text '{msg.Text}' in {msg.Chat} ({msg.Chat.Id})");
        //await OnCommand("/start", "", msg); // for now we redirect to command /start

        long userId = msg.From!.Id;

        if (msg.Text == "Бот" || msg.Text == "бот")
        {
            await Client.SendMessage(msg.Chat, "Меня звали?");
        }

        if (msg.Chat.Type == ChatType.Private)
        {
            await AuthSequence(msg.From, msg.Chat, msg);
        }
    }

    async Task OnCommand(string command, string args, Message msg)
    {
        Console.WriteLine($"Received command: {command} {args}");
        switch (command)
        {
            case "/start":
                if (msg.Chat.Type != ChatType.Private)
                    return;
                if (args == "")
                    break;
                if (_authStartHashes.TryGetValue(msg.From!.Id, out var hash) && hash == args)
                {
                    await AuthSequence(msg!.From!, msg.Chat);
                }
                else
                {
                    await Client.SendMessage(msg!.Chat!.Id!,
                        "❌ Неверный стартовый хеш. Запросите команду <code>/verify</code> в группе заново, или обратитесь к @borishyn.", parseMode: ParseMode.Html);
                }
                break;
            case "/help":
                await Client.SendMessage(
                    msg.Chat,
                    """
                    Вот что я умею:
                    /verify - Пройди подтверждение ника (может сбоить, пишите @borishyn если что)
                    /members - Вывести список игроков клана
                    /me - Вывести информацию о твоем профиле
                    /cwban - Забанить игрока от участия в КВ: Использование /cwban <тег> <дни> <причина>
                    /cwunban - Разбанить игрока от участия в КВ: Использование /cwunban <тег>
                    /banlist - Вывести список забаненных игроков от участия в КВ
                    /warn - Выдать предупреждение игроку: Использование /warn <тег> <причина>
                    /unwarn - Снять предупреждение игроку: Использование /unwarn <тег>
                    /listwarns - Вывести список игроков с предупреждениями
                    
                    Вот что я скоро буду уметь:
                    /rank - Система рейтингов и отслеживания активности игроков
                    /clan - Инфо о клане
                    /cw - Инфо о текущем КВ
                    
                    ... ну и что ещё придумаете)
                    """,
                    parseMode: ParseMode.Html
                    );
                break;
            case "/verify":
                await RequestAuth(msg.Chat, msg.From!);
                break;
            case "/members":
                await SendClanMembers(msg.Chat);
                break;
            case "/me":
                await SendMemberProfile(msg.Chat, msg.From!);
                break;
            case "/cwban":
                if (args == "")
                {
                    await Client.SendMessage(msg.Chat, "❌ Ошибка: Не указаны аргументы команды.");
                    return;
                }
                var argsSplit = args.Split(" ");
                if (argsSplit.Length < 2)
                {
                    await Client.SendMessage(msg.Chat, "❌ Ошибка: Не указаны аргументы команды.");
                    return;
                }
                var tag = argsSplit[0];
                int? days = int.TryParse(argsSplit[1], out var d) ? d : null;
                var reason = argsSplit.Length > 2 ? string.Join(" ", argsSplit.Skip(2)) : null;
                await CwBan(msg.Chat, msg.From!.Id, tag, days, reason: reason);
                break;
            case "/cwunban":
                if (args == "")
                {
                    await Client.SendMessage(msg.Chat, "❌ Ошибка: Не указаны аргументы команды.");
                    return;
                }
                var argsSplitUnban = args.Split(" ");
                if (argsSplitUnban.Length < 1)
                {
                    await Client.SendMessage(msg.Chat, "❌ Ошибка: Не указаны аргументы команды.");
                    return;
                }
                var tagUnban = argsSplitUnban[0];
                await CwBan(msg.Chat, msg.From!.Id, tagUnban, null, true);
                break;
            case "/banlist":
                await ListBans(msg.Chat);
                break;
            case "/warn":
                if (args == "")
                {
                    await Client.SendMessage(msg.Chat, "❌ Ошибка: Не указаны аргументы команды.");
                    return;
                }
                var argsSplitWarn = args.Split(" ");
                if (argsSplitWarn.Length < 2)
                {
                    await Client.SendMessage(msg.Chat, "❌ Ошибка: Не указаны аргументы команды.");
                    return;
                }
                var tagWarn = argsSplitWarn[0];
                var reasonWarn = string.Join(" ", argsSplitWarn.Skip(1));
                await Warn(msg.Chat, msg.From!.Id, tagWarn, reasonWarn);
                break;
            case "/unwarn":
                if (args == "")
                {
                    await Client.SendMessage(msg.Chat, "❌ Ошибка: Не указаны аргументы команды.");
                    return;
                }
                var argsSplitUnwarn = args.Split(" ");
                if (argsSplitUnwarn.Length < 1)
                {
                    await Client.SendMessage(msg.Chat, "❌ Ошибка: Не указаны аргументы команды.");
                    return;
                }
                var tagUnwarn = argsSplitUnwarn[0];
                await Unwarn(msg.Chat, msg.From!.Id, tagUnwarn);
                break;
            case "/listwarns":
                await ListWarns(msg.Chat);
                break;
        }
    }

    async Task OnUpdate(Update update)
    {
        Console.WriteLine($"Received update {update.Type}");

        if (update.Type == UpdateType.Message && update.Message!.Text != null)
        {
            var msg = update.Message;
            if (msg.Chat.Id != _allowedGroupId && msg.Chat.Type != ChatType.Private)
                return;

            Console.WriteLine($"Received a message of type {msg.Type}");

            if (msg.Text.StartsWith('/'))
            {
                var space = msg.Text.IndexOf(' ');
                if (space < 0) space = msg.Text.Length;

                var command = msg.Text[..space].ToLower();

                if (command.LastIndexOf('@') is > 0 and var at)
                {
                    if (command[(at + 1)..].Equals(Me.Username, StringComparison.OrdinalIgnoreCase))
                        command = command[..at];
                    else
                        return;
                }

                await OnCommand(command, msg.Text[space..].TrimStart(), msg);
            }
            else
            {
                await OnTextMessage(msg);
            }
        }

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery!.Data!.StartsWith("page_"))
        {
            var pageStr = update.CallbackQuery.Data.Replace("page_", "");
            var pageSwitch = pageStr.Split("<");

            if (int.TryParse(pageSwitch[0], out int target) && int.TryParse(pageSwitch[1], out int current) &&
                target != current)
            {
                var members = await _clash.GetMembers(_defaultClanTag);
                if (members == null) return;

                var newText = FormatMembersPage(members.items, target, 5);
                var newKeyboard = GeneratePageKeyboard(target, (int)Math.Ceiling(members.items.Count / 5.0));

                await Client.EditMessageText(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    messageId: update.CallbackQuery.Message.MessageId,
                    text: newText,
                    parseMode: ParseMode.Html,
                    replyMarkup: newKeyboard
                );

                await Client.AnswerCallbackQuery(update.CallbackQuery.Id);
            }
        }

        if (update.Type == UpdateType.Message && update.Message!.Text == null && update.Message!.NewChatMembers != null)
        {
            foreach (var user in update.Message!.NewChatMembers)
            {
                await RequestAuth(update.Message!.Chat, user, true);
            }
        }
    }

    private static string GenerateRandomString(int length)
    {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        StringBuilder res = new StringBuilder();
        Random rnd = new Random();
        while (0 < length--)
        {
            res.Append(valid[rnd.Next(valid.Length)]);
        }
        return res.ToString();
    }
    public Bot(string TgToken, string CocToken, ref ClashApi clashApi, string defaultClanTag)
    {
        _authPlayersStorage = new();
        _authStartHashes = new();
        _authSeqSteps = new();

        Cts = new CancellationTokenSource();
        Client = new TelegramBotClient(TgToken);
        _clash = clashApi;

        _defaultClanTag = defaultClanTag;

        Client.DeleteWebhook().Wait();
        Client.DropPendingUpdates().Wait();

        Me = Client.GetMe().Result;

        var groupChat = Client.GetChat("@KyodaiCoC").Result;
        _allowedGroupId = groupChat.Id;

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        Client.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: Cts.Token
        );

        // ПОДПИСКИ

        CwBanned += async chatId => await ListBans(chatId);

        WatchdogEvents.WarPreparationStartedEvent += async war =>
        {
            var msg = "📣 Началась подготовка к войне!\n";

            msg += $"""
                    ⚔️ <strong>Противник:</strong> {war.opponent.name} - {war.opponent.clanLevel} ур. (<span class="tg-spoiler"><code>{war.opponent.tag}</code></span>),
                    🟫 <strong>Поле:</strong> {war.teamSize}v{war.teamSize},
                    🪖 <strong>Манифест:</strong> <i>{war.attacksPerMember} атак на участника</i>,
                    
                    """;

            for (int i = 0; i < war.teamSize; i++)
            {
                var our = war.clan.members.FirstOrDefault(m => m.mapPosition == i);
                var their = war.opponent.members.FirstOrDefault(m => m.mapPosition == i);
                msg +=
                    $"{i + 1}. {our?.name} ({our?.townhallLevel} тх.) vs. {their?.name} ({their?.townhallLevel} тх.)\n";
            }

            msg += "🤜🤛 Удачи!";

            await Client.SendMessage(_allowedGroupId, msg, ParseMode.Html);
            Console.WriteLine($"Обработан ивент WarPreparationStartedEvent: {msg}");
        };

        WatchdogEvents.CapitalRaidStartedEvent += async () =>
        {
            await Client.SendMessage(_allowedGroupId, "⚔️ Пятница - открыта сессия рейдов!");
            Console.WriteLine($"Обработан ивент CapitalRaidStartedEvent");
        };
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Update received: {update.Type} at {DateTime.Now}");
        await OnUpdate(update);
        Console.WriteLine($"Update handled at {DateTime.Now}");
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Ошибка в Telegram API: {exception}");
        return Task.CompletedTask;
    }
}