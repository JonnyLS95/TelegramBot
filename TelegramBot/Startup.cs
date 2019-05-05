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
        public static readonly List<string> Insultos = new List<string>()
        {
            "idiota", "gilipollas", "tonto", "capullo", "cabron", "imbecil", "lerdo"
        };
        public static Dictionary<string, string> FrasesAprendidas = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            Bot.OnMessage += OnMessage;
            FrasesAprendidas = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(System.IO.Directory.GetCurrentDirectory() + "\\FrasesAprendidas.json"));

            Bot.StartReceiving();
            Console.ReadLine();

            System.IO.File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "\\FrasesAprendidas.json", JsonConvert.SerializeObject(FrasesAprendidas));
            Bot.StopReceiving();
        }

        private static async void OnMessage(object sender, MessageEventArgs e)
        {
            if(e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text && e.Message.Date.AddMinutes(1) < DateTime.Now)
            {
                string messageText = e.Message.Text.ToLower();
                if (messageText.Contains("patatuela"))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Has dicho patatuela? Esa es mi amiga!");
                }
                if (messageText.ContainsAny(Insultos))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Como diría mufasa... eso tiene intencionalidad agresiva");
                }
                if (messageText.Contains("/remove"))
                {
                    int position = e.Message.Text.IndexOf("/remove") + 8;
                    string keyToRemove = messageText.Substring(position);
                    if (FrasesAprendidas.ContainsKey(keyToRemove))
                    {
                        FrasesAprendidas.Remove(keyToRemove);
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Borrado!");
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "No he encontrado esa frase para borrar");
                    }
                }
                if (messageText.ContainsAny(FrasesAprendidas.Keys))
                {
                    foreach (var key in messageText.GetAllContainingKeysIn(FrasesAprendidas.Keys.ToList()))
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, FrasesAprendidas[key]);
                    }
                }
                if (messageText.ContainsAll(new List<string>() { "amigo", "aprende a contestar" }))
                {
                    int position = e.Message.Text.IndexOf("aprende a contestar") + 20;
                    List<string> nuevaFrase = new List<string>();
                    foreach (Match match in Regex.Matches(messageText, "\"([^\"]*)\""))
                        nuevaFrase.Add(match.ToString());

                    if (nuevaFrase.Count == 2)
                    {
                        string key = nuevaFrase.Last().ToLower().Replace("\"", string.Empty);
                        string value = nuevaFrase.First().ToLower().Replace("\"", string.Empty);
                        if (key.Length >= 3)
                        {
                            if (FrasesAprendidas.ContainsKey(key))
                            {
                                FrasesAprendidas[key] = value;
                            }
                            else
                            {
                                FrasesAprendidas.Add(key, value);
                            }
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Aprendido!");
                        }
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Eso no lo he podido aprender...");
                    }
                }
                else if (messageText.ContainsAll(new List<string>() { "amigo", "di" }))
                {
                    int position = e.Message.Text.IndexOf("di") + 2;
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Text.Substring(position));
                }
                
                if (messageText.Contains("mierda de bot"))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Para mierda tú, cabesahuevo");
                }
                if (messageText.Contains("tu ano"))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "El culo tuyo");
                }
                if (messageText.Contains("cabesahuevo"))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Amigoooo amigooo amigoooooo amigoo");
                }
                if (messageText.Contains("patataspa"))
                {
                    if (e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group)
                    {
                        await SpamAdmins(e);
                    }
                }
                if (messageText.Contains("/start"))
                {
                    if (e.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group)
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Quereis empezar una partida? Eso merece un buen patataspam!!");
                        await SpamAdmins(e);
                    }
                }
                if (messageText.Contains("indepe"))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Visca la republica catalana! Visca puigdemont!");
                }
                if (messageText.Contains("/suicidio"))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "ALLAHU AKBAAAAAR!");
                }
            }
        }

        private static async System.Threading.Tasks.Task SpamAdmins(MessageEventArgs e)
        {
            ChatMember[] members = await Bot.GetChatAdministratorsAsync(e.Message.Chat.Id);
            string spamMsg = "";
            foreach (var member in members)
            {
                if (!string.IsNullOrEmpty(member.User.Username))
                    spamMsg += $"@{member.User.Username} ";
            }
            await Bot.SendTextMessageAsync(e.Message.Chat.Id, spamMsg);
        }
    }
}
