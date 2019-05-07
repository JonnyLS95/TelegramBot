using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public class Startup
    {
        public static readonly TelegramBotClient Bot = new TelegramBotClient(ConfigurationManager.AppSettings["TelegramBotKey"]);
        public static Dictionary<string, List<string>> LearnedPhrases = new Dictionary<string, List<string>>();
        public static string LearnedPhrasesPath;
        private static Random random;


        static void Main(string[] args)
        {
            LearnedPhrasesPath = System.IO.Directory.GetCurrentDirectory() + "\\LearnedPhrases.json";
            random = new Random(DateTime.Now.Millisecond);

            Bot.OnMessage += OnMessage;
            Bot.OnCallbackQuery += OnCallbackQueryReceived;


            if (System.IO.File.Exists(LearnedPhrasesPath))
                LearnedPhrases = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(System.IO.File.ReadAllText(LearnedPhrasesPath));

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void OnMessage(object sender, MessageEventArgs e)
        {
            if(e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text && e.Message.Date.AddMinutes(1) > DateTime.UtcNow)
            {
                string messageText = e.Message.Text;
                string lowerMessageText = messageText.ToLower();
                long chatId = e.Message.Chat.Id;

                // LearnedPhrases
                if (lowerMessageText.Contains("/remove"))
                {
                    int position = e.Message.Text.IndexOf("/remove") + 8;
                    string keyToRemove = lowerMessageText.Substring(position);
                    if (LearnedPhrases.ContainsKey(keyToRemove))
                    {
                        var valuesList = LearnedPhrases[keyToRemove];
                        if (valuesList.Count == 1)
                        {
                            LearnedPhrases.Remove(keyToRemove);
                            await Bot.SendTextMessageAsync(chatId, "Borrado!");
                        }
                        else
                        {
                            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[valuesList.Count][];
                            for (int i = 0; i < valuesList.Count; i++)
                            {
                                buttons[i] = new InlineKeyboardButton[] {
                                    InlineKeyboardButton.WithCallbackData(
                                        valuesList[i], 
                                        string.Format("{0};#!{1}", keyToRemove, valuesList[i])
                                    )
                                };
                            }
                            var keyboardMarkup = new InlineKeyboardMarkup(buttons);

                            await Bot.SendTextMessageAsync(chatId, "Cual quieres eliminar?", replyMarkup: keyboardMarkup);
                        }
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(chatId, "No he encontrado esa frase para borrar");
                    }
                }
                else if (lowerMessageText.ContainsAll(new List<string>() { "amigo", "aprende a contestar" }))
                {
                    int position = e.Message.Text.IndexOf("aprende a contestar") + 20;
                    List<string> nuevaFrase = new List<string>();
                    bool learned = false;

                    foreach (Match match in Regex.Matches(messageText, "\"([^\"]*)\""))
                        nuevaFrase.Add(match.ToString());

                    if (nuevaFrase.Count == 2)
                    {
                        string key = nuevaFrase.Last().ToLower().Replace("\"", string.Empty).Trim();
                        string value = nuevaFrase.First().Replace("\"", string.Empty).Trim();

                        if (!string.IsNullOrEmpty(key) && 
                            !string.IsNullOrEmpty(value) && 
                            key.Length >= 3)
                        {
                            if (LearnedPhrases.ContainsKey(key))
                            {
                                if(LearnedPhrases[key].Contains(value))
                                {
                                    await Bot.SendTextMessageAsync(chatId, "Esta ya me la sabía!");
                                }
                                else
                                {
                                    LearnedPhrases[key].Add(value);
                                }
                            }
                            else
                            {
                                LearnedPhrases.Add(key, new List<string>() { value });
                            }
                            await Bot.SendTextMessageAsync(chatId, "Aprendido!");
                            System.IO.File.WriteAllText(LearnedPhrasesPath, JsonConvert.SerializeObject(LearnedPhrases));

                            learned = true;
                        }
                    }
                    
                    if (!learned)
                    {
                        await Bot.SendTextMessageAsync(chatId, "Eso no lo he podido aprender...");
                    }
                }
                else if (lowerMessageText.ContainsAll(new List<string>() { "amigo", "di" }))
                {
                    int position = e.Message.Text.IndexOf("di") + 3;
                    await Bot.SendTextMessageAsync(chatId, e.Message.Text.Substring(position));
                }
                else if (lowerMessageText.ContainsAny(LearnedPhrases.Keys))
                {
                    foreach (var key in lowerMessageText.GetAllContainingKeysIn(LearnedPhrases.Keys.ToList()))
                    {
                        var valuesList = LearnedPhrases[key];
                        int randomPosition = random.Next(valuesList.Count);

                        await Bot.SendTextMessageAsync(chatId, valuesList[randomPosition]);
                    }
                }

                // SPAM
                if (lowerMessageText.Contains("/start") &&
                    (e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group ||
                    e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup))
                {
                    await Bot.SendTextMessageAsync(chatId, "Quereis empezar una partida? Eso merece un buen patataspam!!");
                    await SpamAdmins(chatId);
                }
                else if (lowerMessageText.Contains("patataspa") &&
                    (e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group ||
                    e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup))
                {
                    await SpamAdmins(chatId);
                }
            }
        }

        private static async void OnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            long chatId = e.CallbackQuery.Message.Chat.Id;
            int separatorPosition = e.CallbackQuery.Data.IndexOf(";#!");

            string keyToRemove = e.CallbackQuery.Data.Substring(0, separatorPosition);
            string valueToRemove = e.CallbackQuery.Data.Substring(separatorPosition + 3);

            var valuesList = LearnedPhrases[keyToRemove];

            if (valuesList.Contains(valueToRemove))
            {
                valuesList.Remove(valueToRemove);
                await Bot.SendTextMessageAsync(chatId, string.Format("Eliminada la opción {0} - {1}", keyToRemove, valueToRemove));

                System.IO.File.WriteAllText(LearnedPhrasesPath, JsonConvert.SerializeObject(LearnedPhrases));
            }
            else
            {
                await Bot.SendTextMessageAsync(chatId, "No he podido eliminar esta opción, tal vez ya la haya eliminado otro!");
            }
        }

        private static async System.Threading.Tasks.Task SpamAdmins(long chatId)
        {
            ChatMember[] members = await Bot.GetChatAdministratorsAsync(chatId);
            string spamMsg = "";
            foreach (var member in members)
            {
                if (!string.IsNullOrEmpty(member.User.Username))
                    spamMsg += $"@{member.User.Username} ";
            }
            await Bot.SendTextMessageAsync(chatId, spamMsg);
        }
    }
}
