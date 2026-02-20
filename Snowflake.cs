using System;
using System.Collections.Generic;
using System.Text;

namespace Screen_saver
{
    /// <summary>
    /// Структура для хранения параметров снежинки (инкапсуляция как в эталоне)
    /// </summary>
    public struct Snowflake
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Size { get; set; }
        public float Speed { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр структуры Snowflake c заданными параметрами.
        /// </summary>
        /// <param name="x">Позиция по оси X</param>
        /// <param name="y">Позиция по оси Y</param>
        /// <param name="size">Размер снежинки</param>
        /// <param name="speed">Скорость движения снежинки</param>
        public Snowflake(int x, int y, int size, int speed)
        {
            X = x;
            Y = y;
            Size = size;
            Speed = speed;
        }
    }
}
