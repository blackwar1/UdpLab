using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UdpLab;

internal static class Program
{
    private static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("       UDP/TCP Лабораторная работа      ");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Задание 1 (UDP Echo: сервер и клиент)");
            Console.WriteLine("2. Задание 2 (Границы сообщений: TCP и UDP)");
            Console.WriteLine("3. Задание 3 (Размер датаграммы)");
            Console.WriteLine("0. Выход");
            Console.WriteLine("========================================");
            Console.Write("Выберите задание: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1": MenuTask1(); break;
                case "2": MenuTask2(); break;
                case "3": MenuTask3(); break;
                case "0": return;
            }
        }
    }

    // ==========================================
    // ЗАДАНИЕ 1
    // ==========================================
    private static void MenuTask1()
    {
        Console.Clear();
        Console.WriteLine("--- ЗАДАНИЕ 1 ---");
        Console.WriteLine("1. Запустить Сервер");
        Console.WriteLine("2. Запустить Клиент");
        Console.Write("Выбор: ");
        string? c = Console.ReadLine();

        if (c == "1") RunEchoServer();
        else if (c == "2") RunEchoClient();
    }

    private static void RunEchoServer()
    {
        using var server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 8000));
        Console.WriteLine("UDP-сервер ждёт на 127.0.0.1:8000...");
        while (true)
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = server.Receive(ref remote);
            string text = Encoding.UTF8.GetString(data);
            Console.WriteLine($"От {remote}: {text}");
            string reply = $"Получено {data.Length} байт: {text}";
            byte[] replyBytes = Encoding.UTF8.GetBytes(reply);
            server.Send(replyBytes, replyBytes.Length, remote);
        }
    }

    private static void RunEchoClient()
    {
        using var client = new UdpClient();
        client.Client.ReceiveTimeout = 3000; // 3 сек

        try
        {
            byte[] msg = Encoding.UTF8.GetBytes("Привет, UDP!");
            client.Send(msg, msg.Length, new IPEndPoint(IPAddress.Loopback, 8000));
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = client.Receive(ref remote);
            Console.WriteLine($"Ответ от {remote}: {Encoding.UTF8.GetString(data)}");
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
        {
            Console.WriteLine("Сервер не ответил за 3 секунды (тайм-аут).");
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
        {
            Console.WriteLine("Порт назначения закрыт (ConnectionReset).");
        }
        Console.WriteLine("\nНажмите Enter для возврата в меню...");
        Console.ReadLine();
    }

    // ==========================================
    // ЗАДАНИЕ 2
    // ==========================================
    private static void MenuTask2()
    {
        Console.Clear();
        Console.WriteLine("--- ЗАДАНИЕ 2 ---");
        Console.WriteLine("1. TCP Сервер (границы)");
        Console.WriteLine("2. TCP Клиент");
        Console.WriteLine("3. UDP Сервер (границы)");
        Console.WriteLine("4. UDP Клиент");
        Console.Write("Выбор: ");
        string? c = Console.ReadLine();

        if (c == "1") RunTcpBoundServer();
        else if (c == "2") RunTcpBoundClient();
        else if (c == "3") RunUdpBoundServer();
        else if (c == "4") RunUdpBoundClient();
    }

    private static void RunTcpBoundServer()
    {
        var srv = new TcpListener(IPAddress.Loopback, 8000);
        srv.Start();
        Console.WriteLine("TCP-сервер ждёт...");
        using var conn = srv.AcceptTcpClient();
        Thread.Sleep(1000);
        var stream = conn.GetStream();
        byte[] buf = new byte[1024];
        int n = stream.Read(buf, 0, buf.Length);
        string s = Encoding.UTF8.GetString(buf, 0, n);
        Console.WriteLine($"TCP: один Read вернул {n} байт: \"{s}\"");
        srv.Stop();
        Console.WriteLine("\nНажмите Enter для возврата в меню...");
        Console.ReadLine();
    }

    private static void RunTcpBoundClient()
    {
        using var c = new TcpClient();
        c.Connect(IPAddress.Loopback, 8000);
        var stream = c.GetStream();
        foreach (var word in new[] { "one", "two", "three" })
        {
            byte[] b = Encoding.UTF8.GetBytes(word);
            stream.Write(b, 0, b.Length);
        }
        Console.WriteLine("TCP клиент отправил три сообщения.");
        Console.WriteLine("\nНажмите Enter для возврата в меню...");
        Console.ReadLine();
    }

    private static void RunUdpBoundServer()
    {
        using var server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 8000));
        Thread.Sleep(1000);
        for (int i = 0; i < 3; i++)
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = server.Receive(ref remote);
            Console.WriteLine($"UDP #{i + 1}: {Encoding.UTF8.GetString(data)}");
        }
        Console.WriteLine("\nНажмите Enter для возврата в меню...");
        Console.ReadLine();
    }

    private static void RunUdpBoundClient()
    {
        using var c = new UdpClient();
        var dst = new IPEndPoint(IPAddress.Loopback, 8000);
        foreach (var word in new[] { "one", "two", "three" })
        {
            byte[] b = Encoding.UTF8.GetBytes(word);
            c.Send(b, b.Length, dst);
        }
        Console.WriteLine("UDP клиент отправил три сообщения.");
        Console.WriteLine("\nНажмите Enter для возврата в меню...");
        Console.ReadLine();
    }

    // ==========================================
    // ЗАДАНИЕ 3
    // ==========================================
    private static void MenuTask3()
    {
        Console.Clear();
        Console.WriteLine("--- ЗАДАНИЕ 3 ---");
        Console.WriteLine("1. Сервер размеров");
        Console.WriteLine("2. Клиент размеров");
        Console.Write("Выбор: ");
        string? c = Console.ReadLine();

        if (c == "1") RunSizeServer();
        else if (c == "2") RunSizeClient();
    }

    private static void RunSizeServer()
    {
        const int BUFSIZE = 65535;
        using var server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 8000));
        server.Client.ReceiveBufferSize = BUFSIZE;
        Console.WriteLine($"Сервер ждёт... (BUFSIZE={BUFSIZE})");
        while (true)
        {
            try
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = server.Receive(ref remote);
                Console.WriteLine($"Получено {data.Length} байт от {remote}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка: {ex.SocketErrorCode}");
            }
        }
    }

    private static void RunSizeClient()
    {
        using var c = new UdpClient();
        var dst = new IPEndPoint(IPAddress.Loopback, 8000);
        int[] sizes = { 100, 1000, 1472, 60000, 70000 };

        foreach (int size in sizes)
        {
            byte[] payload = new byte[size];
            Array.Fill(payload, (byte)'A');
            try
            {
                c.Send(payload, payload.Length, dst);
                Console.WriteLine($"Отправлено {size} байт: OK");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Отправлено {size} байт: ОШИБКА ({ex.SocketErrorCode})");
            }
        }
        Console.WriteLine("\nНажмите Enter для возврата в меню...");
        Console.ReadLine();
    }
}