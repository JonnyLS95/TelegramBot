using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using System.Configuration;

namespace TelegramBot
{
    public class Startup
    {
        public static readonly TelegramBotClient Bot = new TelegramBotClient(ConfigurationManager.AppSettings["TelegramBotKey"]);
        public static readonly List<string> Insultos = new List<string>()
        {
            "idiota", "gilipollas", "tonto", "capullo", "cabron", "imbecil", "lerdo"
        };
        static void Main(string[] args)
        {
            Bot.OnMessage += OnMessage;

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void OnMessage(object sender, MessageEventArgs e)
        {
            if(e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
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
                if (messageText.ContainsAll(new List<string>() { "amigo", "di" }))
                {
                    int position = e.Message.Text.IndexOf("di");
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Text.Substring(position + 2));
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
                    await SpamAdmins(e);
                }
                if (messageText.Contains("/start"))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Quereis empezar una partida? Eso merece un buen patataspam!!");
                    await SpamAdmins(e);
                }
                if (messageText.Contains("indepe"))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Visca la republica catalana! Visca puigdemont!");
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
