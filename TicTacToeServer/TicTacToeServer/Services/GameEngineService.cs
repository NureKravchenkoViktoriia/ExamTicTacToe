using System.Threading.Channels;
using TicTacToeServer.Models;

namespace TicTacToeServer.Services
{
    // Успадковуємось від BackgroundService, щоб процес гри йшов у фоні
    public class GameEngineService : BackgroundService
    {
        // Ігрове поле 100х100
        private readonly char[,] _board = new char[100, 100];

        // Потокобезпечна черга, суворо обмежена 50 запитами
        private readonly Channel<MoveRequest> _moveQueue;

        // Стан гри
        private char _expectedPlayer = 'X'; // Завжди починає хрестик
        private string _winner = string.Empty;


        public GameEngineService()
        {
            // Налаштування черги на 50 елементів
            var options = new BoundedChannelOptions(50)
            {
                FullMode = BoundedChannelFullMode.Wait // Запити чекають, якщо черга повна
            };
            _moveQueue = Channel.CreateBounded<MoveRequest>(options);
        }


        // Метод для контролера: додає запит від клієнта в чергу
        public async ValueTask<bool> QueueMoveAsync(MoveRequest move)
        {
            if (!string.IsNullOrEmpty(_winner)) return false; // Якщо вже є переможець, ходити не можна

            return await _moveQueue.Writer.WaitToWriteAsync() && _moveQueue.Writer.TryWrite(move);
        }

        public string GetStatus()
        {
            if (!string.IsNullOrEmpty(_winner)) return $"Гра завершена! Переможець: {_winner}";
            return $"Гра триває. Зараз черга гравця: {_expectedPlayer}";
        }

        // Фоновий процес обробки черги
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Послідовно забираємо ходи з черги
            await foreach (var move in _moveQueue.Reader.ReadAllAsync(stoppingToken))
            {
                if (!string.IsNullOrEmpty(_winner)) continue;

                // Якщо прийшов хід не того гравця (наприклад, два 'X' підряд), 
                // ми його просто ігноруємо і чекаємо правильного.
                if (char.ToUpper(move.Player) != _expectedPlayer) continue;

                // Перевіряємо, чи координати в межах поля 100х100 і чи клітинка вільна
                if (move.X < 0 || move.X >= 100 || move.Y < 0 || move.Y >= 100 || _board[move.X, move.Y] != '\0')
                    continue;

                // Записуємо хід на дошку
                _board[move.X, move.Y] = char.ToUpper(move.Player);

                // Перевіряємо, чи не виграв гравець цим ходом
                if (CheckWin(move.X, move.Y, char.ToUpper(move.Player)))
                {
                    _winner = move.Player.ToString().ToUpper();
                }
                else
                {
                    // Передаємо право ходу іншому
                    _expectedPlayer = _expectedPlayer == 'X' ? 'O' : 'X';
                }
            }
        }

        // Алгоритм пошуку 5 однакових символів
        private bool CheckWin(int x, int y, char player)
        {
            // Напрямки для перевірки: Горизонталь, Вертикаль, Діагональ \, Діагональ /
            int[,] directions = { { 1, 0 }, { 0, 1 }, { 1, 1 }, { 1, -1 } };

            for (int i = 0; i < 4; i++)
            {
                int dx = directions[i, 0];
                int dy = directions[i, 1];

                // Рахуємо скільки символів підряд (починаючи з поточного = 1)
                int count = 1
                            + CountConsecutive(x, y, dx, dy, player)
                            + CountConsecutive(x, y, -dx, -dy, player);

                if (count >= 5) return true;
            }
            return false;
        }

        // Допоміжний метод, який йде по дошці в одному напрямку і рахує символи
        private int CountConsecutive(int x, int y, int dx, int dy, char player)
        {
            int count = 0;
            int nx = x + dx;
            int ny = y + dy;

            while (nx >= 0 && nx < 100 && ny >= 0 && ny < 100 && _board[nx, ny] == player)
            {
                count++;
                nx += dx;
                ny += dy;
            }
            return count;
        }

        public List<MoveRequest> GetActiveBoard()
        {
            var activeMoves = new List<MoveRequest>();

            // Проходимось по всьому полю
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    // Якщо клітинка не порожня, додаємо її в результат
                    if (_board[x, y] != '\0')
                    {
                        activeMoves.Add(new MoveRequest
                        {
                            X = x,
                            Y = y,
                            Player = _board[x, y]
                        });
                    }
                }
            }

            return activeMoves;
        }
    }
}