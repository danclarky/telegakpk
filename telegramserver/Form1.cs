using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.DirectoryServices;
using System.Management;
using System.Threading;
using Npgsql;
using Telegram.Bot;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using MihaZupan;

namespace telegramserver
{
    [System.Runtime.InteropServices.GuidAttribute("CC9C1C8C-513B-4A1B-9CC7-D39C5EE6BE06")]
    public partial class Form1 : Form
    {
        BackgroundWorker bw;
        public Form1()
        {
            InitializeComponent();
            this.bw = new BackgroundWorker();
            this.bw.DoWork += this.bw_DoWork;
            this.bw.RunWorkerAsync();

        }
        string databasepath = "Host=ip;Port=port;Username=admin;Password=pass;Database=Base;Command Timeout=0;";
        static readonly Random rndGen = new Random();

        string fotowork = "";



        Int64 countpret = 0; //количество договоров претензии


        static string GetRandomPassword(string ch, int pwdLength)
        {
            char[] pwd = new char[pwdLength];
            for (int i = 0; i < pwd.Length; i++)
                pwd[i] = ch[rndGen.Next(ch.Length)];
            return new string(pwd);
        }

        private static async void DownloadFile(string fileId, string name)
        {
            WebProxy proxyObject = new WebProxy("http://142.93.58.158:8080/", true);
            var key = "423941311:AAEe-MEhc1gO9EbUrBQEhqgNLIyHuVVt3UA";
            var Bot = new Telegram.Bot.TelegramBotClient(key, proxyObject);

            try
            {
                var file = await Bot.GetFileAsync(fileId);
                using (var saveImageStream = new FileStream("\\\\ip\\программа\\" + name + ".jpg", FileMode.Create))
                {
                    // await file.FilePath.CopyTo(saveImageStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading: " + ex.Message);
            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            string procName = Console.ReadLine();
            if (this.bw.IsBusy != true)
            {
                this.bw.RunWorkerAsync();
            }
        }

        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            string data = "";
            var worker = sender as BackgroundWorker;
            var key = "4239";
            var keygetadress = "AIzaSyC";
            var keydistancematrix = "AIz";
            try
            {
                WebProxy proxyObject = new WebProxy("http://142.93.58.158:8080/", true);
                var proxy = new HttpToSocks5Proxy("97.74.230.16", 44951);
                proxy.ResolveHostnamesLocally = true;
                var Bot = new Telegram.Bot.TelegramBotClient(key, proxy);
         
                await Bot.SetWebhookAsync("");

                Bot.OnUpdate += async (object su, Telegram.Bot.Args.UpdateEventArgs evu) =>
                {

                    if (evu.Update.CallbackQuery != null || evu.Update.InlineQuery != null) return;
                    var update = evu.Update;
                    var message = update.Message;
                    if (message == null) return;
                    if (message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
                    {
                        string path = "", name = "";
                        const string rc = "йцукенгшщзхъфывапролджэячсмитьabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ0123456789";
                        for (int i = 0; i < 15; i++)
                        {
                            name = GetRandomPassword(rc, i);
                        }
                        fotowork = "\\\\ip\\программа\\" + name + ".jpg";
                        DownloadFile(message.Photo[message.Photo.Length - 1].FileId, name);
                    }
                    if (message.Type == Telegram.Bot.Types.Enums.MessageType.Location)
                    {
                        string adress = "";
                        HttpWebRequest request = WebRequest.Create("https://maps.googleapis.com/maps/api/geocode/json?latlng=" + message.Location.Latitude.ToString().Replace(",", ".") + "," + message.Location.Longitude.ToString().Replace(",", ".") + "&key=" + keygetadress) as HttpWebRequest;
                        WebResponse response = request.GetResponse();
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                            Newtonsoft.Json.Linq.JObject jobject = Newtonsoft.Json.Linq.JObject.Parse(reader.ReadToEnd());
                            StringBuilder res = new StringBuilder();

                            res.Append(jobject["results"][0]["formatted_address"]);
                            adress = res.Replace(",", "").Replace(" ", "+").Replace("'", "").ToString();
                            using (var conn = new NpgsqlConnection(databasepath))
                            {
                                conn.Open();
                                using (var cmd = new NpgsqlCommand())
                                {
                                    cmd.Connection = conn;

                                    cmd.CommandText = "update userstelegramm set adress='" + adress + "' where name = '" + message.Chat.Id + "'";
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            await Bot.SendTextMessageAsync(message.Chat.Id, "Я запомнил Ваш адрес");
                        }
                    }
                    if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        // выйти из юзера
                        if (message.Text.ToLower() == "выход" || message.Text.ToLower() == "/logout")
                        {
                            using (var conn = new NpgsqlConnection(databasepath))
                            {

                                conn.Open();
                                using (var cmd = new NpgsqlCommand())
                                {

                                    cmd.Connection = conn;
                                    cmd.CommandText = "delete from userstelegramm where name = '" + message.Chat.Id + "'";
                                    Console.WriteLine(cmd.CommandText);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                        }

                        using (var conn = new NpgsqlConnection(databasepath))
                        {

                            conn.Open();
                            using (var cmd = new NpgsqlCommand())
                            {
                                cmd.Connection = conn;
                                //запись того, что юзер вошел первый раз
                                string count = "0";
                                cmd.CommandText = "select count(code) as coun from userstelegramm where name = '" + message.Chat.Id + "'";
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        count = reader["coun"].ToString();
                                    }
                                }
                                if (count == "0")
                                {
                                    cmd.CommandText = "insert into userstelegramm(name,data,date) values ('" + message.Chat.Id + "','0','" + DateTime.Now + "') ";
                                }
                                cmd.ExecuteNonQuery();

                                // берем его и инфу
                                cmd.CommandText = "select fio,data from userstelegramm where name = '" + message.Chat.Id + "'";
                                Console.WriteLine(cmd.CommandText);
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {

                                        data = reader["data"].ToString();

                                    }
                                }

                            }
                        }




                        var keyboardnorm = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardRemove();



                        Console.WriteLine(message.Text);


                        if (data == "0" || message.Text == "/enter")
                        {
                            string count = "0";
                            using (var conn = new NpgsqlConnection(databasepath))
                            {

                                conn.Open();
                                using (var cmd = new NpgsqlCommand())
                                {
                                    cmd.Connection = conn;

                                    cmd.CommandText = "update userstelegramm set data='1',date='" + DateTime.Now + "' where name = '" + message.Chat.Id + "'";
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            await Bot.SendTextMessageAsync(message.Chat.Id, "Введите логин и пароль через пробел.", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardnorm);
                        }



                        else if (data == "1" && message.Text != "/enter")
                        {
                            bool has = false;
                            String[] words = message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (words.Length > 1)
                            {
                                Console.WriteLine();
                                using (var conn = new NpgsqlConnection(databasepath))
                                {
                                    string fio = "";

                                    conn.Open();
                                    using (var cmd = new NpgsqlCommand())
                                    {
                                        cmd.Connection = conn;
                                        cmd.CommandText = "select Код,ФИО from Пользователи where lower(Логин) ='" + words[0].ToLower() + "' and lower(Пароль)='" + words[1].ToLower() + "'";
                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {

                                                    fio = reader["ФИО"].ToString();
                                                    has = true;
                                                }
                                            }
                                        }
                                        if (has)
                                        {

                                            cmd.CommandText = "update userstelegramm set fio='" + fio + "', data='2',date='" + DateTime.Now + "' where name = '" + message.Chat.Id + "'";
                                            cmd.ExecuteNonQuery();
                                            var keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                                            {
                                                Keyboard = new[] {
                                                new[]
                                                {
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Выход")
                                                },
                                                new[]
                                                {
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Должники")
                                                },
                                                new[]
                                                {
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("76 счет")
                                                },
                                            },
                                                ResizeKeyboard = true
                                            };
                                            await Bot.SendTextMessageAsync(message.Chat.Id, "Привет, " + fio + ". Если хочешь, отправь мне свое местоположение.", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboard);
                                        }
                                        else
                                        {
                                            await Bot.SendTextMessageAsync(message.Chat.Id, "Неверно", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboardnorm);
                                        }
                                    }

                                }
                            }

                        }

                        else if (data == "2")
                        {
                            if (message.Text == "76 счет")
                            {
                                pretchoose(Bot, message);
                            }
                        }
                        //76 счет
                        else if (data == "3")
                        {
                            string level = "";
                            string typed = "";
                            using (var conn = new NpgsqlConnection(databasepath))
                            {

                                conn.Open();
                                using (var cmd = new NpgsqlCommand())
                                {
                                    cmd.Connection = conn;
                                    cmd.CommandText = "select leveldolg from userstelegramm where name = '" + message.Chat.Id + "'";
                                    Console.WriteLine(cmd.CommandText);
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            level = reader["leveldolg"].ToString();
                                        }
                                    }
                                }
                            }
                            if (level == "0")
                            {

                                if (message.Text.ToLower() == "назад")//Общее вернуться на первый уровень
                                {
                                    var keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                                    {
                                        Keyboard = new[] {
                                        new[]
                                                {
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Выход")
                                                },
                                                new[]
                                                {
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Должники")
                                                },
                                                new[]
                                                {
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("76 счет")
                                                },
                                            },
                                        ResizeKeyboard = true
                                    };
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Выберите. Если хочешь выйти, нажми /logout", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboard);
                                    using (var conn = new NpgsqlConnection(databasepath))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand())
                                        {
                                            cmd.Connection = conn;

                                            cmd.CommandText = "update userstelegramm set data='2',date='" + DateTime.Now + "' where name = '" + message.Chat.Id + "'";
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                                else
                                {
                                    using (var conn = new NpgsqlConnection(databasepath))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand())
                                        {
                                            cmd.Connection = conn;

                                            cmd.CommandText = "update userstelegramm set typesort='" + message.Text + "',date='" + DateTime.Now + "' where name = '" + message.Chat.Id + "'";
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    pret(Bot, message);
                                }
                            }

                            if (level == "1")
                            {
                                if (message.Text.ToLower() == "назад")
                                {
                                    pretchoose(Bot, message);
                                    using (var conn = new NpgsqlConnection(databasepath))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand())
                                        {
                                            cmd.Connection = conn;

                                            cmd.CommandText = "update userstelegramm set leveldolg='0',date='" + DateTime.Now + "' where name = '" + message.Chat.Id + "'";
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                else
                                {
                                    string fiodolg = "";
                                    string dogovor = "";
                                    string info = "";
                                    string codedolg = "";
                                    String[] words = message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (words.Length > 2)
                                    {

                                        using (var conn = new NpgsqlConnection(databasepath))
                                        {

                                            conn.Open();
                                            using (var cmd = new NpgsqlCommand())
                                            {
                                                cmd.Connection = conn;
                                                string adress = "";
                                                cmd.CommandText = "select adress from userstelegramm where name ='" + message.Chat.Id + "'";
                                                using (var reader = cmd.ExecuteReader())
                                                {

                                                    while (reader.Read())
                                                    {
                                                        adress = reader["adress"].ToString();
                                                    }
                                                }
                                                cmd.CommandText = "select Код,ДатаРешения,ФотоЗаемщика,АдресЗаемщика,ФактАдресЗаемщика,ДатаНачалаЗайма,Договор,ФИОЗаемщика,ТелефонЗаемщика,Контакты,РаботаЗаемщика,Оплатил,ДатаПлатежа,СуммаПлатежа,ВидОбеспечения,ФИОПоручителя,АдресПоручителя,ТелефонПоручителя,РаботаПоручителя,ОбъектЗалога,ДолгСуд from семьшесть where replace(ФИОЗаемщика,' ','')='" + words[0] + words[1] + words[2] + "'";
                                                using (var reader = cmd.ExecuteReader())
                                                {
                                                    int q = 0;
                                                    while (reader.Read())
                                                    {
                                                        dogovor = reader["Договор"].ToString();
                                                        codedolg = reader["Код"].ToString();
                                                        fiodolg = reader["ФИОЗаемщика"].ToString();
                                                        string qq = "";
                                                        if (reader["ФотоЗаемщика"].ToString() != "")
                                                        {
                                                            String[] words1 = reader["ФотоЗаемщика"].ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                            if (words.Length > 0)
                                                            {
                                                                qq = words1[words1.Length - 1];
                                                                if (qq.Contains("10.0.0.1"))
                                                                {
                                                                    qq = qq.Replace("10.0.0.1", "192.168.1.222");
                                                                }
                                                                else if (qq.Contains("78.69.157.1"))
                                                                {
                                                                    qq = qq.Replace("78.69.157.1", "192.168.1.222");
                                                                }
                                                                try
                                                                {
                                                                    using (var stream = System.IO.File.Open(qq, FileMode.Open))
                                                                    {
                                                                        // InputTelegramFile fts = new InputTelegramFile();
                                                                        //fts.Content = stream;
                                                                        //fts.Filename = qq.Split('\\').Last();
                                                                        //var test = await Bot.SendPhotoAsync(message.Chat.Id, fts);
                                                                    }
                                                                }
                                                                catch { }
                                                            }
                                                        }
                                                        else { info += "Нет фото" + Environment.NewLine; }

                                                        info += "Прописан " + reader["АдресЗаемщика"].ToString() + Environment.NewLine;
                                                        HttpWebRequest request = WebRequest.Create("https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + adress + "&destinations=" + reader["АдресЗаемщика"].ToString().Replace(",", " ").Replace(" ", "+") + "&key=" + keydistancematrix) as HttpWebRequest;
                                                        WebResponse response = request.GetResponse();
                                                        using (Stream responseStream = response.GetResponseStream())
                                                        {
                                                            StreamReader reader1 = new StreamReader(responseStream, Encoding.UTF8);
                                                            Newtonsoft.Json.Linq.JObject jobject = Newtonsoft.Json.Linq.JObject.Parse(reader1.ReadToEnd());
                                                            StringBuilder res = new StringBuilder();

                                                            try
                                                            {
                                                                res.Append(jobject["rows"][0]["elements"][0]["distance"]["text"]);
                                                                info += "Расстояние " + res.ToString() + Environment.NewLine + "https://www.google.ru/maps/place/" + reader["АдресЗаемщика"].ToString().Replace(",", " ").Replace(" ", "+") + Environment.NewLine;
                                                            }
                                                            catch { }
                                                        }
                                                        info += "Живет " + reader["ФактАдресЗаемщика"].ToString() + Environment.NewLine;
                                                        request = WebRequest.Create("https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + adress + "&destinations=" + reader["ФактАдресЗаемщика"].ToString().Replace(",", "").Replace(" ", "+") + "&key=" + keydistancematrix) as HttpWebRequest;
                                                        response = request.GetResponse();
                                                        using (Stream responseStream = response.GetResponseStream())
                                                        {
                                                            StreamReader reader1 = new StreamReader(responseStream, Encoding.UTF8);
                                                            Newtonsoft.Json.Linq.JObject jobject = Newtonsoft.Json.Linq.JObject.Parse(reader1.ReadToEnd());
                                                            StringBuilder res = new StringBuilder();
                                                            try
                                                            {
                                                                res.Append(jobject["rows"][0]["elements"][0]["distance"]["text"]);
                                                                info += "Расстояние " + res.ToString() + Environment.NewLine + "https://www.google.ru/maps/place/" + reader["ФактАдресЗаемщика"].ToString().Replace(",", "").Replace(" ", "+") + Environment.NewLine;
                                                            }
                                                            catch { }
                                                        }
                                                        info += "Контакты " + reader["ТелефонЗаемщика"].ToString() + "," + reader["Контакты"].ToString() + Environment.NewLine;
                                                        if (reader["РаботаЗаемщика"].ToString() != "")
                                                        {
                                                            info += "Место работы " + reader["РаботаЗаемщика"].ToString() + Environment.NewLine;
                                                        }

                                                        info += "Дата начала займа " + reader["ДатаНачалаЗайма"].ToString() + Environment.NewLine;
                                                        info += "Дата решения  " + reader["ДатаРешения"].ToString() + Environment.NewLine;
                                                        info += "Должен " + reader["ДолгСуд"].ToString() + " рублей" + Environment.NewLine;
                                                        if (reader["ДатаПлатежа"].ToString() != "")
                                                        {
                                                            info += "Всего оплатил " + reader["Оплатил"].ToString() + " рублей, платил в последний раз " + reader["ДатаПлатежа"].ToString() + ", сумму " + reader["СуммаПлатежа"].ToString() + Environment.NewLine;
                                                        }
                                                        if (reader["ВидОбеспечения"].ToString() == "Поручительство")
                                                        {
                                                            info += "Поручитель " + reader["ФИОПоручителя"].ToString() + Environment.NewLine;
                                                            info += "По адресу " + reader["АдресПоручителя"].ToString() + Environment.NewLine;
                                                            info += "Телефон " + reader["АдресПоручителя"].ToString() + Environment.NewLine;
                                                            if (reader["РаботаПоручителя"].ToString() != "")
                                                            {
                                                                info += "Место работы " + reader["РаботаПоручителя"].ToString() + Environment.NewLine;
                                                            }
                                                        }
                                                        if (reader["ОбъектЗалога"].ToString() != "")
                                                        {
                                                            if (reader["ФИОПоручителя"].ToString() != reader["ФИОЗаемщика"].ToString())
                                                            {
                                                                info += "Залогодатель " + reader["ФИОПоручителя"].ToString() + Environment.NewLine;

                                                                info += "По адресу " + reader["АдресПоручителя"].ToString() + Environment.NewLine;
                                                                info += "Телефон " + reader["ТелефонПоручителя"].ToString() + Environment.NewLine;
                                                                if (reader["РаботаПоручителя"].ToString() != "")
                                                                {
                                                                    info += "Место работы " + reader["РаботаПоручителя"].ToString() + Environment.NewLine;
                                                                }
                                                            }
                                                            info += "Объект залога " + reader["ОбъектЗалога"].ToString();
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                        await Bot.SendTextMessageAsync(message.Chat.Id, info);
                                        await Bot.SendTextMessageAsync(message.Chat.Id, "Если хочешь выйти, нажми /logout");
                                        using (var conn = new NpgsqlConnection(databasepath))
                                        {

                                            conn.Open();
                                            using (var cmd = new NpgsqlCommand())
                                            {
                                                cmd.Connection = conn;
                                                cmd.CommandText = "update userstelegramm set leveldolg='3',date='" + DateTime.Now + "',fiodolg='" + fiodolg + "',codedolg='" + codedolg + "',dogovor='" + dogovor + "' where name = '" + message.Chat.Id + "'";
                                                cmd.ExecuteNonQuery();
                                            }
                                        }
                                        pretclientopen(Bot, message);
                                    }
                                }
                            }
                            else if (level == "2")
                            {
                                string dogovor = "";
                                string fiodolg = "";
                                using (var conn = new NpgsqlConnection(databasepath))
                                {

                                    conn.Open();
                                    using (var cmd = new NpgsqlCommand())
                                    {
                                        cmd.Connection = conn;
                                        cmd.CommandText = "select fiodolg,dogovor from userstelegramm where name = '" + message.Chat.Id + "'";
                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            int q = 0;
                                            while (reader.Read())
                                            {
                                                fiodolg = reader["fiodolg"].ToString();
                                                dogovor = reader["dogovor"].ToString();
                                            }
                                        }
                                    }
                                }
                                if (message.Text == "Назад")
                                {
                                    using (var conn = new NpgsqlConnection(databasepath))
                                    {

                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand())
                                        {
                                            cmd.Connection = conn;
                                            cmd.CommandText = "update userstelegramm set leveldolg='1',date='" + DateTime.Now + "' where name = '" + message.Chat.Id + "'";
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    pret(Bot, message);
                                }
                                if (message.Text == "Работа по договору")
                                {

                                    using (var conn = new NpgsqlConnection(databasepath))
                                    {
                                        string txt = "";
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand())
                                        {
                                            cmd.Connection = conn;
                                            cmd.Connection = conn;
                                            cmd.CommandText = "select Тип,Результат,Дата,Путь,Запись,Ответственный from РаботаДолг where Договор = '" + dogovor + "' and ФИО = '" + fiodolg + "'  order by ДатаРаботы";
                                            using (var reader = cmd.ExecuteReader())
                                            {
                                                int q = 0;
                                                while (reader.Read())
                                                {
                                                    txt += reader["Дата"].ToString() + " " + reader["Результат"].ToString() + " " + reader["Ответственный"].ToString() + Environment.NewLine;
                                                    //if (reader["Путь"].ToString() != "")
                                                    //{

                                                    //    String[] words = reader["Путь"].ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                    //    for (int i = 0; i != words.Length; i++)
                                                    //    {
                                                    //        if (words[i].Contains("10.0.0.1"))
                                                    //        {
                                                    //            words[i] = words[i].Replace("10.0.0.1", "78.69.157.1");
                                                    //        }
                                                    //        else if (words[i].Contains("78.69.157.1"))
                                                    //        {
                                                    //            words[i] = words[i].Replace("78.69.157.1", "78.69.157.1");
                                                    //        }
                                                    //        using (var stream = System.IO.File.Open(words[i], FileMode.Open))
                                                    //        {

                                                    //            FileToSend fts = new FileToSend();
                                                    //            fts.Content = stream;
                                                    //            fts.Filename = q.ToString() + "." + words[i].Split('\\').Last().Split('.').Last();
                                                    //            Console.WriteLine(fts.Filename);
                                                    //            await Bot.SendDocumentAsync(message.Chat.Id, fts);
                                                    //        }
                                                    //    }
                                                    //}
                                                    q++;
                                                }
                                                await Bot.SendTextMessageAsync(message.Chat.Id, txt);

                                            }
                                        }
                                    }

                                }
                                if (message.Text == "История расчетов")
                                {

                                    //using (var conn = new NpgsqlConnection(databasepath))
                                    //{

                                    //    conn.Open();
                                    //    using (var cmd = new NpgsqlCommand())
                                    //    {
                                    //        cmd.Connection = conn;
                                    //        cmd.Connection = conn;
                                    //        cmd.CommandText = "select Дата,Основной,Проценты, Членские,Штрафы from ИсторияРасчетов where Договор = '" + dogovor + "' and (Основной<>'0' or Проценты<>'0' or Членские<>'0' or Штрафы<>'0') order by Дата";
                                    //        using (var reader = cmd.ExecuteReader())
                                    //        {
                                    //            int q = 0;
                                    //            while (reader.Read())
                                    //            {
                                    //                await Bot.SendTextMessageAsync(message.Chat.Id, reader["Дата"].ToString() + " " + reader["Основной"].ToString() + " " + reader["Проценты"].ToString() + " " + reader["Членские"].ToString() + " " + reader["Штрафы"].ToString());

                                    //            }
                                    //        }
                                    //    }
                                    //}

                                }
                                string lvlvvod = "";
                                using (var conn = new NpgsqlConnection(databasepath))
                                {

                                    conn.Open();
                                    using (var cmd = new NpgsqlCommand())
                                    {
                                        cmd.Connection = conn;
                                        cmd.CommandText = "select levelvvod from userstelegramm where name = '" + message.Chat.Id + "'";
                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            int q = 0;
                                            while (reader.Read())
                                            {
                                                lvlvvod = reader["levelvvod"].ToString();
                                            }
                                        }
                                    }
                                }
                                if (message.Text == "Ввести информацию")
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Введите тип работы");
                                    using (var conn = new NpgsqlConnection(databasepath))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand())
                                        {
                                            cmd.Connection = conn;
                                            cmd.CommandText = "update userstelegramm set levelvvod='1' where name = '" + message.Chat.Id + "'";
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }

                                if (lvlvvod == "1")
                                {

                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Введите результат работы");
                                    using (var conn = new NpgsqlConnection(databasepath))
                                    {

                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand())
                                        {
                                            cmd.Connection = conn;
                                            cmd.CommandText = "update userstelegramm set levelvvod='2',work1='" + message.Text + "' where name = '" + message.Chat.Id + "'";
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                                else if (lvlvvod == "2")
                                {

                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Приложите фото и нажмите /ok");
                                    using (var conn = new NpgsqlConnection(databasepath))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand())
                                        {
                                            cmd.Connection = conn;
                                            cmd.CommandText = "update userstelegramm set levelvvod='3',work2='" + message.Text + "' where name = '" + message.Chat.Id + "'";
                                            cmd.ExecuteNonQuery();
                                        }
                                    }

                                }
                                else if (lvlvvod == "3")
                                {

                                    using (var conn = new NpgsqlConnection(databasepath))
                                    {
                                        conn.Open();
                                        using (var cmd = new NpgsqlCommand())
                                        {
                                            cmd.Connection = conn;
                                            string resultwork = "";
                                            string typework = "";
                                            string codedolg = "";
                                            cmd.CommandText = "select work2,work1,codedolg from userstelegramm where name = '" + message.Chat.Id + "'";
                                            using (var reader = cmd.ExecuteReader())
                                            {
                                                while (reader.Read())
                                                {
                                                    typework = reader["work1"].ToString();
                                                    resultwork = reader["work2"].ToString();
                                                    codedolg = reader["codedolg"].ToString();
                                                }
                                            }
                                            cmd.CommandText = "insert into РаботаДолг(Договор,ФИО,Тип,Результат,Дата,Путь,Имя,Ответственный,ДатаРаботы) values ('" + dogovor + "','" + fiodolg + "','" + typework + "','" + resultwork + "','" + DateTime.Now.ToShortDateString() + "',';" + fotowork + "','фото','telegramm','" + DateTime.Now + "') ";
                                            Console.WriteLine(cmd.CommandText);
                                            cmd.ExecuteNonQuery();
                                            cmd.CommandText = "update userstelegramm set levelvvod='0',work1='',work2='' where name = '" + message.Chat.Id + "'";
                                            cmd.ExecuteNonQuery();
                                            cmd.CommandText = "update семьшесть set Пропуск = '" + DateTime.Now.ToShortDateString() + "' where Код = '" + codedolg + "'";
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Я все запомнил.");

                                }
                            }
                        }
                    }
                };
                Bot.StartReceiving();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        async void pretclientopen(Telegram.Bot.TelegramBotClient Bot, Telegram.Bot.Types.Message message)
        {
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
            {
                Keyboard = new[] {
                                                new[]
                                                {
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Назад")
                                                },
                                                 new[]
                                                {
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Работа по договору"),
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("История расчетов")
                                                },
                                                new[]
                                                {
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Ввести информацию")
                                                },
                                            },
                ResizeKeyboard = true
            };
            await Bot.SendTextMessageAsync(message.Chat.Id, "Выберите", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboard);
            using (var conn = new NpgsqlConnection(databasepath))
            {

                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "update userstelegramm set leveldolg='2' where name = '" + message.Chat.Id + "'";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        async void pretchoose(Telegram.Bot.TelegramBotClient Bot, Telegram.Bot.Types.Message message)
        {



           

            var keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
            {

                Keyboard = new[] {
                                    new[]
                                      {
                                                    //Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Поиск по ФИО"),
                                                    //Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Назад"),
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("По алфавиту")
                                                },
                                                 new[]
                                                {
                                                    //Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Без оплаты"),
                                                    //Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContactn("По дате платежа"),
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("По сумме долга")
                                                },
                                                new[]
                                                {
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("По дате решения"),
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("По дате начала"),
                                                    Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Ближайшие")
                                                },
                                            },
                ResizeKeyboard = true
            };
            await Bot.SendTextMessageAsync(message.Chat.Id, "Выберите", Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboard);
            using (var conn = new NpgsqlConnection(databasepath))
            {

                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "update userstelegramm set data='3',leveldolg='0' where name = '" + message.Chat.Id + "'";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        async void pret(Telegram.Bot.TelegramBotClient Bot, Telegram.Bot.Types.Message message)
        {
            int q = 0;
            Telegram.Bot.Types.ReplyMarkups.KeyboardButton[][] arr1;
            using (var conn = new NpgsqlConnection(databasepath))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    string secondfield = "";
                    string type = "";
                    cmd.Connection = conn;
                    cmd.CommandText = "select typesort from userstelegramm where name='" + message.Chat.Id + "'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            type = reader["typesort"].ToString();
                        }

                    }



                    if (type == "По алфавиту")
                    {
                        cmd.CommandText = "select count(Код) from семьшесть where Подразделение='Основное подразделение'";
                        countpret = (Int64)cmd.ExecuteScalar();
                        cmd.CommandText = "select ФИОЗаемщика,ДолгСуд from семьшесть where Подразделение='Основное подразделение' order by ФИОЗаемщика";
                        secondfield = "ДолгСуд";
                    }
                    else if (type == "Без оплаты")
                    {
                        cmd.CommandText = "select count(Код) from семьшесть where Подразделение='Основное подразделение' and ДатаПлатежа =''";
                        countpret = (Int64)cmd.ExecuteScalar();
                        cmd.CommandText = "select ФИОЗаемщика, ДолгСуд from семьшесть where Подразделение='Основное подразделение' and ДатаПлатежа ='' order by ФИОЗаемщика";

                        secondfield = "ДолгСуд";
                    }
                    else if (type == "По дате платежа")
                    {
                        cmd.CommandText = "select count(Код) from семьшесть where Подразделение='Основное подразделение' and ДатаПлатежа <>''";
                        countpret = (Int64)cmd.ExecuteScalar();
                        cmd.CommandText = "select ФИОЗаемщика,ДатаПлатежа from семьшесть where Подразделение='Основное подразделение' and ДатаПлатежа <>'' order by cast(ДатаПлатежа as timestamp without time zone)";
                        secondfield = "ДатаПлатежа";
                    }
                    else if (type == "По сумме долга")
                    {
                        cmd.CommandText = "select count(Код) from семьшесть where Подразделение='Основное подразделение'";
                        countpret = (Int64)cmd.ExecuteScalar();
                        cmd.CommandText = "select ФИОЗаемщика,ДолгСуд from семьшесть where Подразделение='Основное подразделение' order by cast(replace(replace(ДолгСуд,',','.'),' ','') as float) desc";
                        secondfield = "ДолгСуд";
                    }
                    else if (type == "По дате решения")
                    {
                        cmd.CommandText = "select count(Код) from семьшесть where Подразделение='Основное подразделение'";
                        countpret = (Int64)cmd.ExecuteScalar();
                        cmd.CommandText = "select ФИОЗаемщика,ДатаРешения from семьшесть where Подразделение='Основное подразделение' order by cast(ДатаРешения as timestamp without time zone)";
                        secondfield = "ДатаРешения";
                    }
                    else if (type == "По дате начала")
                    {
                        cmd.CommandText = "select count(Код) from семьшесть where Подразделение='Основное подразделение'";
                        countpret = (Int64)cmd.ExecuteScalar();
                        cmd.CommandText = "select ФИОЗаемщика,ДатаНачалаЗайма from семьшесть where Подразделение='Основное подразделение' order by cast(ДатаНачалаЗайма as timestamp without time zone)";
                        secondfield = "ДатаНачалаЗайма";
                    }

                    arr1 = new Telegram.Bot.Types.ReplyMarkups.KeyboardButton[countpret + 1][];

                    arr1[0] = new[]
                    {
                        Telegram.Bot.Types.ReplyMarkups.KeyboardButton.WithRequestContact("Назад")
                    };


                    using (var reader = cmd.ExecuteReader())
                    {
                        q = 1;


                        while (reader.Read())
                        {
                            arr1[q] = new Telegram.Bot.Types.ReplyMarkups.KeyboardButton[1];
                            arr1[q][0] = reader["ФИОЗаемщика"].ToString() + "  " + reader[secondfield].ToString();
                            q++;
                        }

                    }
                }
            }

            var keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
            {
                Keyboard = arr1,
                ResizeKeyboard = true
            };
            await Bot.SendTextMessageAsync(message.Chat.Id, "Всего: " + countpret.ToString(), Telegram.Bot.Types.Enums.ParseMode.Default, false, false, 0, keyboard);
            using (var conn = new NpgsqlConnection(databasepath))
            {

                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "update userstelegramm set leveldolg='1' where name = '" + message.Chat.Id + "'";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

    }
}
