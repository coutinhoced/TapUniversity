using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TapUniversity
{
    public static class Title
    {
        private static string _title;
        private static int _titleWidth;
        private static int _titleStartColumn;
        private static int _windowWidth;
        private static  int _windowHeight;
        private static int CursorTop;
        private static int CursorLeft;

        static Title()
        {
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);      
            _windowWidth = Console.WindowWidth;
            _windowHeight = Console.WindowHeight;
        }
        public static void SetTitle(string title)
        {
            _title = title;
            _titleWidth = _title.Length;
            var temp = _windowWidth - _titleWidth;
            temp /= 2;
            _titleStartColumn = temp;
            SetCursorPosition(1, temp);
            Console.WriteLine(_title, Console.ForegroundColor = ConsoleColor.Red);
            Console.ForegroundColor = ConsoleColor.White;
        }



        private static void SetCursorPosition(int row, int column)
        {
            if (row >= 0 && row <= _windowHeight)
            {
                CursorTop = row;
            }

            if (column >= 0 && column <= _windowWidth)
            {
                CursorLeft = column;
            }
            Console.SetCursorPosition(CursorLeft, CursorTop);
        }
    }
}
