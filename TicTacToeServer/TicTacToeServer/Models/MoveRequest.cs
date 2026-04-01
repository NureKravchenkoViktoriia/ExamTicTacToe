namespace TicTacToeServer.Models
{
    public class MoveRequest
    {
        public int X { get; set; } // Координата по горизонталі (0-99)
        public int Y { get; set; } // Координата по вертикалі (0-99)
        public char Player { get; set; } // Символ: 'X' або 'O'
    }
}