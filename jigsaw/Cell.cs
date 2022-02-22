using System.Collections.Generic;

namespace jigsaw
{
    class Cell
    {
        public Cell(int row, int column, int sector, int? value = null)
        {
            Row = row;
            Column = column;
            Sector = sector;
            this.value = value;
        }

        private int? value;
        public int Row { get; }
        public int Column { get; }

        public int Sector { get; }

        public int? Value
        {
            get => value;
            set
            {
                this.value = value;
                this.Options.Clear();
            }
        }

        public bool IsInitial { get; init; }
        public List<int> Options { get; } = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    }
}