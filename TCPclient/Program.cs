using System.Net.Sockets;
using System.Text;

using TcpClient client = new TcpClient();
try
{
    await client.ConnectAsync("127.0.0.1", 5000);

    Console.WriteLine("Успешное подключение к серверу");
    Console.WriteLine("Введите заказ в формате:");
    Console.WriteLine("ORDER;Название товара;Количество;Цена");
    Console.WriteLine("Для отключения введите EXIT");

    NetworkStream stream = client.GetStream();
    using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
    using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8){AutoFlush = true};

    while (true)
    {
        Console.Write("Введите команду: ");
        string? message = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("Ошибка: команда не может быть пустой");
            continue;
        }

        await writer.WriteLineAsync(message);
        if (message.Trim().Equals("EXIT",StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Отключение от сервера");
            break;
        }

        string? response = await reader.ReadLineAsync();
        if (response == null)
        {
            Console.WriteLine("Сервер разорвал соединение");
            break;
        }

        Console.WriteLine($"{response}");
    }
}
catch (SocketException)
{
    Console.WriteLine("Ошибка: не удалось подключиться к серверу");
}

Console.WriteLine("Работа клиента завершена");