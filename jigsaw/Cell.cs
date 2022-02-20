using System.Collections.Generic;

namespace jigsaw
{
    class Cell
    {
        private int? value;
        public int Row { get; init; }
        public int Column { get; init; }

        public int? Value
        {
            get => value;
            set
            {
                this.value = value;
                this.Options.Clear();
            }
        }

        public bool IsInitial { get; set; }
        public int Sector { get; init; }
        public List<int> Options { get; } = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    }
}