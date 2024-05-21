using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

class Program
{
    private static readonly List<string> MessageHistory = new List<string>();
    
    public static void StartListening(int port)
    {
        // Разрешение сетевых имён

        // Привязываем сокет ко всем интерфейсам на текущей машинe
        IPAddress ipAddress = IPAddress.Any;
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        // CREATE
        Socket listener = new Socket(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            // BIND
            listener.Bind(localEndPoint);

            // LISTEN
            listener.Listen(10);

            while (true)
            {
                Console.WriteLine("Ожидание соединения клиента...");
                // ACCEPT
                Socket handler = listener.Accept();

                Console.WriteLine("Получение данных...");
                byte[] buf = new byte[1024];
                string data = null;
                while (true)
                {
                    // RECEIVE
                    int bytesRec = handler.Receive(buf);

                    data += Encoding.UTF8.GetString(buf, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                Console.WriteLine("Полученный текст: {0}", data);

                // Отправляем текст обратно клиенту
                MessageHistory.Add(data.Replace("<EOF>", ""));
                string response = "";
                MessageHistory.ForEach(message => response += $"{message}\n");
                response += "<EOF>";
                byte[] msg = Encoding.UTF8.GetBytes(response);

                // SEND
                handler.Send(msg);

                // RELEASE
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void Main(string[] args)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out int port))
        {
            Console.WriteLine("Invalid parameters");
            Console.WriteLine("Usage: dotnet run <port>");
            return;
        }
        
        Console.WriteLine("Запуск сервера...");
        StartListening(port);

        Console.WriteLine("\nНажмите ENTER чтобы выйти...");
        Console.Read();
    }
}