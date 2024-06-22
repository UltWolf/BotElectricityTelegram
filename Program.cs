// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Text.RegularExpressions;
using TL;

namespace ElecticityBot
{



    public class Program
    {
        private static string sourceChannelUsername = "pat_cherkasyoblenergo";
        private static string destChannelUsername = "turn_off_light_3_che";
        // private static readonly long SourceChannelId = -1001893927522;
        //private static readonly long SourceChannelId = -1002149421685;
        //   private static readonly long DestinationChannelId = -1002149421685;
        public static string FilenameForLastId = "nameLastId";
        public static int lastMessageId = 0;

        public static string botToken = Environment.GetEnvironmentVariable("telegramelectricybot_BotToken");
        public static string channelUsername = Environment.GetEnvironmentVariable("telegramelectricybot_ChannelUsername");
        public static long SourceChannelId = long.Parse(Environment.GetEnvironmentVariable("telegramelectricybot_SourceChannelId"));
        public static long DestinationChannelId = long.Parse(Environment.GetEnvironmentVariable("telegramelectricybot_DestinationChannelId"));

        static string Config(string what)
        {
            return what switch
            {
                "api_id" => Environment.GetEnvironmentVariable("telegramelectricybot_ApiId"),
                "api_hash" => Environment.GetEnvironmentVariable("telegramelectricybot_ApiHash"),
                "phone_number" => Environment.GetEnvironmentVariable("telegramelectricybot_PhoneNumber"),
                "session_pathname" => Environment.GetEnvironmentVariable("telegramelectricybot_SessionPathname"),
                _ => null
            };
        }


        static async Task Main(string[] args)
        {
            //using var connection = new Microsoft.Data.Sqlite.SqliteConnection(@"Data Source=WTelegramBot.sqlite");
            //StreamWriter WTelegramLogs = new StreamWriter("WTelegramBot.log", true, Encoding.UTF8) { AutoFlush = true };
            //WTelegram.Helpers.Log = (lvl, str) => WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");

            if (File.Exists(FilenameForLastId))
            {
                int.TryParse(File.ReadAllText(FilenameForLastId), out lastMessageId);
            }
            //using var bot = new WTelegram.Bot(Token, apiId, apiHash, connection);
            using var client = new WTelegram.Client(Config);

            var user = await client.LoginUserIfNeeded();
            var resolvedDest = await client.Contacts_ResolveUsername(destChannelUsername);
            Console.WriteLine("Bot is up and running.");
            using (var cts = new CancellationTokenSource())
            {
                while (!cts.IsCancellationRequested)
                {

                    var resolved = await client.Contacts_ResolveUsername(sourceChannelUsername);
                    var messages = await client.Messages_GetHistory(resolved, limit: 1);
                    Console.CancelKeyPress += (sender, eventArgs) =>
                    {
                        eventArgs.Cancel = true;
                        cts.Cancel();
                    };

                    AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
                    {
                        cts.Cancel();
                    };




                    var message = messages.Messages[0];


                    if (message is TL.Message && message.ID > lastMessageId)
                    {
                        lastMessageId = message.ID;
                        File.WriteAllText(FilenameForLastId, lastMessageId.ToString());
                        var messageText = message as TL.Message;
                        var parsedMessage = GetTimeRangesForShift(messageText.message, "3");

                        string dateString = ExtractDateString(messageText.message);
                        await client.Messages_SendMessage(resolvedDest, "Orig Message :" + GenerateLink(message.ID), new Random().Next(int.MinValue, int.MaxValue));
                        var messageForSend = "Виключення  " + dateString + " будуть об : \n" + String.Join(",\n", parsedMessage);
                        await client.Messages_SendMessage(resolvedDest, messageForSend, new Random().Next(int.MinValue, int.MaxValue));
                    }
                    await Task.Delay(1000 * 60 * 30);
                }
            }
            //channelPeer = resolved.peer;
            // var messages = await client.Messages_GetHistory(channelPeer, limit: 10);
            //var channel = await client.(SourceChannelId.ToString());
            //if (channel.Chat is TL.Channel)
            //{
            //    var messages = await client.Messages_GetHistory(channel, limit: 10);
            //    foreach (var message in messages.Messages)
            //    {
            //        if (message is TL.Message)
            //        {
            //            var msg = (TL.Message)message;
            //            Console.WriteLine($"{msg.Date}: {msg.message}");
            //        }
            //    }
            //}
            //   var messages = await bot.GetMessagesById(SourceChannelId, Enumerable.Range(1904, 5));




            //Console.WriteLine("___________________________________________________\n");
            //Console.WriteLine("I'm listening now. Send me a command in private or in a group where I am... Or press Escape to exit");
            //bot.WantUnknownTLUpdates = true;
            //for (int offset = 0; ;)
            //{
            //    var updates = await bot.GetUpdates(offset, 100, 1, WTelegram.Bot.AllUpdateTypes);
            //    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape) break;

            //    await bot.SendTextMessage(DestinationChannelId, "hello userbot");


            //    Console.WriteLine("Exiting...");

            //    using (var cts = new CancellationTokenSource())
            //    {
            //        var receiverOptions = new Telegram.Bot.Polling.ReceiverOptions
            //        {
            //            AllowedUpdates = Array.Empty<Telegram.Bot.Types.Enums.UpdateType>()
            //        };
            //        Console.CancelKeyPress += (sender, eventArgs) =>
            //        {
            //            eventArgs.Cancel = true;
            //            cts.Cancel();
            //        };

            //        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            //        {
            //            cts.Cancel();
            //        };

            //        Console.WriteLine("Bot is up and running.");

            //        //try
            //        //{
            //        //    FetchLastMessages();
            //        //    BotClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);
            //        //    while (!cts.Token.IsCancellationRequested)
            //        //    {
            //        //        await Task.Delay(1000, cts.Token);
            //        //    }
            //        //}
            //        //catch (OperationCanceledException)
            //        //{
            //        //    Console.WriteLine("Bot is shutting down.");
            //        //}
            //    }
            //}
        }

        //private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        //{
        //    if (update.Type == UpdateType.ChannelPost && update.ChannelPost!.Type == MessageType.Text)
        //    {
        //        var chatId = update.ChannelPost.Chat.Id;
        //        var messageText = update.ChannelPost.Text;

        //        if (chatId == SourceChannelId)
        //        {
        //            var parsedMessage = GetTimeRangesForShift(messageText, "3");
        //            foreach (var timeRange in parsedMessage)
        //            {
        //                Console.WriteLine(timeRange);
        //            }
        //            string dateString = ExtractDateString(messageText);
        //            await BotClient.SendTextMessageAsync(DestinationChannelId, "Orig Message :" + GenerateLink(update.ChannelPost.MessageId) + "\nВиключення  " + dateString + " будуть об : \n" + String.Join(",\n", parsedMessage), cancellationToken: cancellationToken);
        //        }
        //    }
        //}
        static string ExtractDateString(string text)
        {
            // Regular expression to find the date in format "dd month"
            string pattern = @"\b(\d{1,2}) (січня|лютого|березня|квітня|травня|червня|липня|серпня|вересня|жовтня|листопада|грудня)\b";
            Match match = Regex.Match(text, pattern);

            if (match.Success)
            {
                string numbers = Regex.Replace(match.Value, @"\D", "");
                if (int.TryParse(numbers, out int number))
                {
                    if (DateTime.Now.Day == number)
                    {
                        return "Сьогодні " + match.Value;
                    }
                }
                return match.Value;
            }
            return null;
        }
        private static string GenerateLink(int messageId)
        {
            return $"https://t.me/{sourceChannelUsername}/{messageId}";
        }
        static DateTime ParseUkrainianDate(string dateString)
        {
            string[] ukMonths = { "січня", "лютого", "березня", "квітня", "травня", "червня",
                              "липня", "серпня", "вересня", "жовтня", "листопада", "грудня" };
            for (int i = 0; i < ukMonths.Length; i++)
            {
                if (dateString.Contains(ukMonths[i]))
                {
                    dateString = dateString.Replace(ukMonths[i], (i + 1).ToString());
                    break;
                }
            }
            return DateTime.ParseExact(dateString, "d M", CultureInfo.InvariantCulture);
        }
        private static string ParseMessage(string message)
        {
            return message;
        }
        public static List<string> GetTimeRangesForShift(string inputText, string shift)
        {
            var timeRanges = new List<(TimeSpan Start, TimeSpan End)>();
            string pattern = $@"(\d{{2}}:\d{{2}})-(\d{{2}}:\d{{2}})\s+.*\b{Regex.Escape(shift)}\b";

            var matches = Regex.Matches(inputText, pattern);
            foreach (Match match in matches)
            {
                var start = TimeSpan.Parse(match.Groups[1].Value);
                var end = TimeSpan.Parse(match.Groups[2].Value);
                timeRanges.Add((start, end));
            }

            timeRanges.Sort((a, b) => a.Start.CompareTo(b.Start));
            var mergedRanges = MergeTimeRanges(timeRanges);
            var result = mergedRanges.Select(range => $"{range.Start:hh\\:mm}-{range.End:hh\\:mm}").ToList();
            return result;
        }

        private static List<(TimeSpan Start, TimeSpan End)> MergeTimeRanges(List<(TimeSpan Start, TimeSpan End)> timeRanges)
        {
            if (timeRanges.Count == 0)
                return new List<(TimeSpan Start, TimeSpan End)>();

            var mergedRanges = new List<(TimeSpan Start, TimeSpan End)>();
            var currentRange = timeRanges[0];

            for (int i = 1; i < timeRanges.Count; i++)
            {
                var nextRange = timeRanges[i];

                if (currentRange.End >= nextRange.Start)
                {
                    currentRange = (currentRange.Start, nextRange.End);
                }
                else
                {
                    mergedRanges.Add(currentRange);
                    currentRange = nextRange;
                }
            }

            mergedRanges.Add(currentRange);
            return mergedRanges;
        }
        //private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        //{
        //    Console.WriteLine(exception);
        //    return Task.CompletedTask;
        //}
    }
}