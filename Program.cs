// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ElecticityBot
{
    public class Program
    {


        private static readonly TelegramBotClient BotClient = new TelegramBotClient(Token);
        private static string channelUsername = "@pat_cherkasyoblenergo";
        private static string sourceChannelUsername = "pat_cherkasyoblenergo";
        private static readonly long SourceChannelId = -1001893927522;
        //private static readonly long SourceChannelId = -1002149421685;
        private static readonly long DestinationChannelId = -1002149421685;
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            BotClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);

            Console.WriteLine("Bot is up and running. Press any key to exit.");
            Console.ReadKey();

            cts.Cancel();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.ChannelPost && update.ChannelPost!.Type == MessageType.Text)
            {
                var chatId = update.ChannelPost.Chat.Id;
                var messageText = update.ChannelPost.Text;

                if (chatId == SourceChannelId)
                {
                    var parsedMessage = GetTimeRangesForShift(messageText, "3");
                    foreach (var timeRange in parsedMessage)
                    {
                        Console.WriteLine(timeRange);
                    }
                    string dateString = ExtractDateString(messageText);
                    await BotClient.SendTextMessageAsync(DestinationChannelId, "Orig Message :" + GenerateLink(update.ChannelPost.MessageId) + "\nВиключення  " + dateString + " будуть об : \n" + String.Join(",\n", parsedMessage), cancellationToken: cancellationToken);
                }
            }
        }
        static string ExtractDateString(string text)
        {
            // Regular expression to find the date in format "dd month"
            string pattern = @"\b(\d{1,2}) (січня|лютого|березня|квітня|травня|червня|липня|серпня|вересня|жовтня|листопада|грудня)\b";
            Match match = Regex.Match(text, pattern);
            if (match.Success)
            {
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
            string pattern = $@"(\d{{2}}:\d{{2}})-(\d{{2}}:\d{{2}})\s+{Regex.Escape(shift)}";

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
        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
            return Task.CompletedTask;
        }
    }
}