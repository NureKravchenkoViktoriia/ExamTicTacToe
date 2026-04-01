using System.Net.Http.Json;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== Клієнт гри Хрестики-Нолики ===");
Console.Write("Оберіть, ким ви будете грати (X або O): ");
char player = char.ToUpper(Console.ReadKey().KeyChar);
Console.WriteLine();

string serverUrl = "https://localhost:7172/api/game";

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
};
using HttpClient client = new HttpClient(handler);

Console.WriteLine($"Ви граєте за: {player}. Підключення до сервера...");

while (true)
{
    try
    {
        string statusJson = await client.GetStringAsync($"{serverUrl}/status");

        Console.WriteLine($"\n[Сервер]: {statusJson}");

        if (statusJson.Contains("завершена"))
        {
            Console.WriteLine("Гру закінчено. Натисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
            break;
        }

        if (statusJson.Contains($"черга гравця: {player}"))
        {
            Console.Write("Введіть координату X (0-99): ");
            int x = int.Parse(Console.ReadLine()!);

            Console.Write("Введіть координату Y (0-99): ");
            int y = int.Parse(Console.ReadLine()!);

            var move = new { X = x, Y = y, Player = player };

            var response = await client.PostAsJsonAsync($"{serverUrl}/move", move);
            string resultText = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[Відповідь сервера]: {resultText}");
        }
        else
        {
            Console.WriteLine("Чекаємо на хід суперника...");
            await Task.Delay(2000);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Помилка зв'язку з сервером: {ex.Message}");
        await Task.Delay(3000); // Чекаємо 3 секунди і пробуємо знову
    }
}