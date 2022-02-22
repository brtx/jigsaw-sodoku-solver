using System;
using System.Collections.Generic;
using System.Linq;

namespace jigsaw
{
    class Cell
    {
        private readonly IDictionary<int, int> values = new Dictionary<int, int>();
        private readonly IDictionary<int, IReadOnlyCollection<int>> optionss = new Dictionary<int, IReadOnlyCollection<int>>();

        internal Cell(int row, int column, int sector, int? value = null)
        {
            Row = row;
            Column = column;
            Sector = sector;
            this.optionss[0] = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            if (value.HasValue)
            {
                this.SetValue(value.Value, 0);
            }
        }

        internal int Row { get; }

        internal int Column { get; }

        internal int Sector { get; }

        internal bool HasValue => values.Any();

        internal int Value => values.Last().Value;

        internal IReadOnlyCollection<int> Options => optionss.Last().Value;

        internal void SetValue(int value, int version)
        {
            this.values[version] = value;
            this.optionss[version] = Array.Empty<int>();
        }

        internal void SetOptions(IReadOnlyCollection<int> options, int version)
        {
            this.optionss[version] = options;
        }
    }
}