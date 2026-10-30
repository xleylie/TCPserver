using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener listener = new TcpListener(IPAddress.Any, 5000);
int acceptedOrders = 0;
decimal totalRevenue = 0;
object _lock = new object();

listener.Start();
while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    Console.WriteLine("Клиент подключился");
    _ = ClientAsync(client);
}

async Task ClientAsync(TcpClient client)
{
    using (client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true){AutoFlush = true};

            while (true)
            {
                string? message = await reader.ReadLineAsync();
                if (message == null){ break;}
                Console.WriteLine($"Получено: {message}");
                if (message.Trim().Equals("EXIT",StringComparison.OrdinalIgnoreCase)){ break;}

                string response = ProcessOrder(message);
                await writer.WriteLineAsync(response);
                Console.WriteLine($"Отправлено: {response}");
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Ошибка сокета: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Клиент отключился");
        }
    }
}

string ProcessOrder(string message)
{
    string[] parts = message.Split(';');

    if (parts.Length != 4)
    {
        return "Неверный формат заказа";
    }
    if (!parts[0].Trim().Equals("ORDER", StringComparison.OrdinalIgnoreCase))
    {
        return "Неизвестная команда";
    }

    string product = parts[1].Trim();
    if (string.IsNullOrWhiteSpace(product))
    {
        return "Имя продукта не может быть пустым";
    }
    if (!int.TryParse(parts[2].Trim(), out int quantity))
    {
        return "Неверный формат количества";
    }
    if (quantity <= 0)
    {
        return "Ошибка: количество должно быть положительным";
    }
    if (!decimal.TryParse(parts[3].Trim(), out decimal price))
    {
        return "Неверный формат цены";
    }
    if (price < 0)
    {
        return "Цена не может быть отрицательной";
    }

    try
    {
        decimal orderTotal = quantity * price;
        lock (_lock)
        {
            totalRevenue = totalRevenue + orderTotal;
            acceptedOrders++;
        }

        Console.WriteLine($"Заказов принято: {acceptedOrders}");
        Console.WriteLine($"Общий доход: {totalRevenue}");

        return $"Заказ принят: {product}, итого: {orderTotal}";
    }
    catch (OverflowException)
    {
        return "Ошибка: сумма заказа слишком велика";
    }
}
