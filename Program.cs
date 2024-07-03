using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Threading;
using Gtk;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using Cairo;

namespace TestGtkApp
{
    public class Messages(string IP)
    {
        public (int, int)? Services { get; set; } // Key -> размер в битах одной части, value -> количество частей.
        public string IP { get; set; } = IP;
    }


    public static class Program
    {
        public static List<(string, Messages)> messages = []; // Журнал
        public static List<string> comands = ["/name", "/err", "/messageCL", "/updateCL", "/messageSERV", "/getIPCL", "/check", "/fileServices", "/get_ip_SERV_CL", "/corr", "/updateSERV", "/yourName", "/corrOnline"]; // Список команд сервера
        public const string IPSERV = "192.168.1.253"; // SERVER IP
        public static string MyName = ""; // MyName 
        public static bool Correct_Name = false; // Наше имя корректно?
        public static bool if_in_Group_Chat = false; // Находимся ли сейчас в общем чате
        public static OwnerChat Chats = null; // Для обновления чата в реальном времени
        public static string MyDirectory = Directory.GetCurrentDirectory();
        public static string GroupChat = $"{MyDirectory}/Data/GroupChat.txt"; // Name:IP message
        public static bool ifOnline = false;
        public static string tempNameCL = "";
        public static string nameCL = "";
        public static Connection connection = null;

        public static async Task CommandUpdateCLAsync()
        {
            byte[] data = Encoding.UTF8.GetBytes($"/updateCL ");
            await connection.SendMessageAsync(data);
        }

        public static async Task<bool> CommandNameAsync(string mes)
        {
            byte[] data = Encoding.UTF8.GetBytes($"/name {mes}");
            await connection.SendMessageAsync(data);
            // отправляем данные
            Thread.Sleep(100);
            if (Correct_Name)
            {
                MyName = mes;
            }
            return Correct_Name;
        }

        public static async Task CommandMessageCLAsync(string mes)
        {
           // Console.WriteLine(mes);
            byte[] data = Encoding.UTF8.GetBytes($"/messageCL {MyName}: {mes}");
            // отправляем данные
            await connection.SendMessageAsync(data);
        }

        public static async Task<bool> CommandGetIPCLAsync(string mes)
        {
            //Console.WriteLine(mes);
            byte[] data = Encoding.UTF8.GetBytes($"/getIPCL {mes}");
            // отправляем данные
            await connection.SendMessageAsync(data);
            tempNameCL = mes;
            Thread.Sleep(200);

            return ifOnline;
        }
        public static async Task CommandFileServicesAsync(string path)
        {
            string filename = "";
            if (path.Contains('/'))
            {
                filename = path.Split("/")[^1];
            }
            else if (path.Contains('\\'))
            {
                filename = path.Split("\\")[^1];
            }
            byte[] file = File.ReadAllBytes(path);

            filename = filename.Replace(' ', '_');
            byte[] fileServices = Encoding.UTF8.GetBytes($"/fileServices {nameCL} {filename} <END>");
            byte[] rv = new byte[fileServices.Length + file.Length];
            Buffer.BlockCopy(fileServices, 0, rv, 0, fileServices.Length);
            Buffer.BlockCopy(file, 0, rv, fileServices.Length, file.Length);
            await connection.SendMessageAsync(rv);
        }


        static void InitAsync() // Correct Work
        {
            int port = 11000;
            Console.WriteLine("Запуск клиента....");
            try
            {
                using TcpClient tcpClient = new TcpClient("192.168.1.253", port);
                using Connection connection = new Connection(tcpClient);
                Program.connection = connection;
                Console.WriteLine($"Подключен к серверу: {port}");
                while (true)
                {
                    string input = Console.ReadLine();
                    if (input.Length == 0)
                        break;
                    Task t = connection.SendMessageAsync(Encoding.UTF8.GetBytes(input));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey(true);
        }

        static void Main()
        {
            new Thread(InitAsync).Start();
            Thread.Sleep(200);
            if (!Correct_Name)
            {
                Application.Init();
                MainWindow mainWindow = [];
                mainWindow.ShowAll();
                Application.Run();
            }
            else
            {
                Application.Init();
                Chats mainWindow = [];
                mainWindow.ShowAll();
                Application.Run();
            }
        }
    }
}