using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;

namespace TelegramBot
{
    public class Startup
    {
        public static readonly TelegramBotClient Bot = new TelegramBotClient(ConfigurationManager.AppSettings["TelegramBotKey"]);
        public static Dictionary<string, string> LearnedPhrases = new Dictionary<string, string>();
        public static string LearnedPhrasesPath;

        static void Main(string[] args)
        {
            LearnedPhrasesPath = System.IO.Directory.GetCurrentDirectory() + "\\LearnedPhrases.json";

            Bot.OnMessage += OnMessage;

            if (System.IO.File.Exists(LearnedPhrasesPath))
            LearnedPhrases = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(LearnedPhrasesPath));

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void OnMessage(object sender, MessageEventArgs e)
        {
            if(e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text && e.Message.Date.AddMinutes(1) > DateTime.UtcNow)
            {
                string messageText = e.Message.Text.ToLower();
                long chatId = e.Message.Chat.Id;

                // LearnedPhrases
                if (messageText.Contains("/remove"))
                {
                    int position = e.Message.Text.IndexOf("/remove") + 8;
                    string keyToRemove = messageText.Substring(position);
                    if (LearnedPhrases.ContainsKey(keyToRemove))
                    {
                        LearnedPhrases.Remove(keyToRemove);
                        await Bot.SendTextMessageAsync(chatId, "Borrado!");
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(chatId, "No he encontrado esa frase para borrar");
                    }
                }
                else if (messageText.ContainsAny(LearnedPhrases.Keys))
                {
                    foreach (var key in messageText.GetAllContainingKeysIn(LearnedPhrases.Keys.ToList()))
                    {
                        await Bot.SendTextMessageAsync(chatId, LearnedPhrases[key]);
                    }
                }
                else if (messageText.ContainsAll(new List<string>() { "amigo", "aprende a contestar" }))
                {
                    int position = e.Message.Text.IndexOf("aprende a contestar") + 20;
                    List<string> nuevaFrase = new List<string>();
                    bool learned = false;

                    foreach (Match match in Regex.Matches(messageText, "\"([^\"]*)\""))
                        nuevaFrase.Add(match.ToString());

                    if (nuevaFrase.Count == 2)
                    {
                        string key = nuevaFrase.Last().ToLower().Replace("\"", string.Empty);
                        string value = nuevaFrase.First().Replace("\"", string.Empty);

                        if (!string.IsNullOrEmpty(key) && 
                            !string.IsNullOrEmpty(value) && 
                            key.Length >= 3)
                        {
                            if (LearnedPhrases.ContainsKey(key))
                            {
                                LearnedPhrases[key] = value;
                            }
                            else
                            {
                                LearnedPhrases.Add(key, value);
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
                else if (messageText.ContainsAll(new List<string>() { "amigo", "di" }))
                {
                    int position = e.Message.Text.IndexOf("di") + 2;
                    await Bot.SendTextMessageAsync(chatId, e.Message.Text.Substring(position));
                }

                // SPAM
                if (messageText.Contains("/start") &&
                    e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group)
                {
                    await Bot.SendTextMessageAsync(chatId, "Quereis empezar una partida? Eso merece un buen patataspam!!");
                    await SpamAdmins(chatId);
                }
                else if (messageText.Contains("patataspa") &&
                    e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group)
                {
                    await SpamAdmins(chatId);
                }
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
