using System;
using System.Collections.Generic;
using System.Linq;

namespace jigsaw
{
    class Board
    {
        private int version;
        
        internal IReadOnlyCollection<Cell> Cells { get; }

        internal Board()
        {
            Cells = Create();
            VerifySectors();
        }

        internal void SetValue(Cell cell, int value)
        {
            if (GetRow(cell.Row).Any(c => c.HasValue && c.Value == value))
            {
                RenderFull();
                throw new ApplicationException();
            }
            
            if (GetColumn(cell.Column).Any(c => c.HasValue && c.Value == value))
            {
                RenderFull();
                throw new ApplicationException();
            }
            
            if (GetSector(cell.Sector).Any(c => c.HasValue && c.Value == value))
            {
                RenderFull();
                throw new ApplicationException();
            }
            
            cell.SetValue(value, version++);
        }

        internal void RemoveOption(Cell cell, int option)
        {
            cell.SetOptions(cell.Options.Where(o => o != option).ToList(), version++);
        }

        internal IEnumerable<Cell> GetColumn(int column)
        {
            return Cells.Where(c => c.Column == column);
        }

        internal IEnumerable<Cell> GetRow(int row)
        {
            return Cells.Where(c => c.Row == row);
        }

        internal IEnumerable<Cell> GetSector(int sector)
        {
            return Cells
                .Where(c => c.Sector == sector);
        }
        
        private void VerifySectors()
        {
            if (Cells.Count != 81)
            {
                throw new ApplicationException("Not 81 cells.");
            }

            for (int i = 1; i <= 9; i++)
            {
                if (Cells.Count(c => c.Sector == i) != 9)
                {
                    throw new ApplicationException($"Not exactly 9 cells for sector {i}.");
                }
            }
        }

        internal void RenderFull()
        {
            Console.WriteLine("█████████████████████████████████████████████████████████████████████████");
            for (int y = 0; y < 27; y++)
            {
                var cellY = (y / 3) + 1;

                for (int x = 0; x < 27; x++)
                {
                    var cellX = (x / 3) + 1;
                    var option = ((y % 3) * 3) + (x % 3) + 1;
                    var cell = Cells.Single(c => c.Row == cellX && c.Column == cellY);
                    var nextCell = Cells.SingleOrDefault(c => c.Row == cellX + 1 && c.Column == cellY);

                    var value = cell.Options.Contains(option)
                        ? option.ToString()
                        : " ";
                    
                    var delimiter = option % 3 == 0
                        ? nextCell == null || nextCell.Sector != cell.Sector
                            ? " █ "
                            : " | "
                        : " ";

                    if (option == 5 && cell.HasValue)
                    {
                        value = cell.Value.ToString();
                        Console.ForegroundColor = ConsoleColor.Green;
                    }

                    var initialBorder = x == 0
                        ? "█ "
                        : string.Empty;
                    
                    Console.Write($"{initialBorder}{value}{delimiter}");
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.WriteLine();
                if (y % 3 == 2)
                {
                    Console.Write("-");
                    for (int x = 1; x <= 9; x++)
                    {
                        var cell = Cells.Single(c => c.Row == x && c.Column == cellY);
                        var nextCell = Cells.SingleOrDefault(c => c.Row == x && c.Column == cellY + 1);

                        var delimiter = nextCell == null || nextCell.Sector != cell.Sector
                            ? "███████ "
                            : "------- ";
                        
                        Console.Write(delimiter);
                    }
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine($"Version: {version}");
        }

        internal void Render()
        {
            for (int y = 1; y <= 9; y++)
            {
                for (int x = 1; x <= 9; x++)
                {
                    var cell = Cells.Single(c => c.Row == x && c.Column == y);
                    var value = cell.HasValue
                        ? cell.Value.ToString()
                        : " ";
                    Console.Write($"{value} ");
                }
                
                Console.WriteLine();
            }
        }
        
        private static IReadOnlyCollection<Cell> Create()
        {
            return new[]
            {
                new Cell(1, 1, 1, 4),
                new Cell(2, 1, 1),
                new Cell(3, 1, 1),
                new Cell(4, 1, 2, 2),
                new Cell(5, 1, 3, 7),
                new Cell(6, 1, 3),
                new Cell(7, 1, 3),
                new Cell(8, 1, 3),
                new Cell(9, 1, 3),
                new Cell(1, 2, 1),
                new Cell(2, 2, 1),
                new Cell(3, 2, 1),
                new Cell(4, 2, 2),
                new Cell(5, 2, 2, 3),
                new Cell(6, 2, 2),
                new Cell(7, 2, 3),
                new Cell(8, 2, 4),
                new Cell(9, 2, 3),
                new Cell(1, 3, 1),
                new Cell(2, 3, 5),
                new Cell(3, 3, 2, 4),
                new Cell(4, 3, 2),
                new Cell(5, 3, 6, 6),
                new Cell(6, 3, 6, 2),
                new Cell(7, 3, 3, 9),
                new Cell(8, 3, 4),
                new Cell(9, 3, 4, 3),
                new Cell(1, 4, 1),
                new Cell(2, 4, 5, 1),
                new Cell(3, 4, 5, 8),
                new Cell(4, 4, 2, 6),
                new Cell(5, 4, 6),
                new Cell(6, 4, 7),
                new Cell(7, 4, 3, 3),
                new Cell(8, 4, 4),
                new Cell(9, 4, 4),
                new Cell(1, 5, 1),
                new Cell(2, 5, 5, 4),
                new Cell(3, 5, 2),
                new Cell(4, 5, 2, 8),
                new Cell(5, 5, 6),
                new Cell(6, 5, 7, 3),
                new Cell(7, 5, 4),
                new Cell(8, 5, 4, 9),
                new Cell(9, 5, 4),
                new Cell(1, 6, 5),
                new Cell(2, 6, 5),
                new Cell(3, 6, 5),
                new Cell(4, 6, 6, 3),
                new Cell(5, 6, 6, 1),
                new Cell(6, 6, 7, 8),
                new Cell(7, 6, 7),
                new Cell(8, 6, 9),
                new Cell(9, 6, 4),
                new Cell(1, 7, 8),
                new Cell(2, 7, 8, 6),
                new Cell(3, 7, 5),
                new Cell(4, 7, 6, 4),
                new Cell(5, 7, 7, 2),
                new Cell(6, 7, 7),
                new Cell(7, 7, 7),
                new Cell(8, 7, 9),
                new Cell(9, 7, 9),
                new Cell(1, 8, 8),
                new Cell(2, 8, 8),
                new Cell(3, 8, 5),
                new Cell(4, 8, 6),
                new Cell(5, 8, 6),
                new Cell(6, 8, 7),
                new Cell(7, 8, 7),
                new Cell(8, 8, 9),
                new Cell(9, 8, 9),
                new Cell(1, 9, 8),
                new Cell(2, 9, 8),
                new Cell(3, 9, 8, 7),
                new Cell(4, 9, 8),
                new Cell(5, 9, 8),
                new Cell(6, 9, 9),
                new Cell(7, 9, 9),
                new Cell(8, 9, 9),
                new Cell(9, 9, 9)
            };
        }
        
        private static IReadOnlyCollection<Cell> CreateOld()
        {
            return new[]
            {
                new Cell(1, 1, 1),
                new Cell(2, 1, 1),
                new Cell(3, 1, 1, 3),
                new Cell(4, 1, 1),
                new Cell(5, 1, 2),
                new Cell(6, 1, 3),
                new Cell(7, 1, 3),
                new Cell(8, 1, 3),
                new Cell(9, 1, 3),
                new Cell(1, 2, 1),
                new Cell(2, 2, 1, 6),
                new Cell(3, 2, 1, 5),
                new Cell(4, 2, 2, 3),
                new Cell(5, 2, 2),
                new Cell(6, 2, 3, 7),
                new Cell(7, 2, 3),
                new Cell(8, 2, 4, 1),
                new Cell(9, 2, 4, 9),
                new Cell(1, 3, 1),
                new Cell(2, 3, 1),
                new Cell(3, 3, 2, 6),
                new Cell(4, 3, 2),
                new Cell(5, 3, 2),
                new Cell(6, 3, 3, 2),
                new Cell(7, 3, 4),
                new Cell(8, 3, 4),
                new Cell(9, 3, 4),
                new Cell(1, 4, 5, 1),
                new Cell(2, 4, 5),
                new Cell(3, 4, 5),
                new Cell(4, 4, 2),
                new Cell(5, 4, 3),
                new Cell(6, 4, 3, 9),
                new Cell(7, 4, 4, 6),
                new Cell(8, 4, 4),
                new Cell(9, 4, 4),
                new Cell(1, 5, 5),
                new Cell(2, 5, 5),
                new Cell(3, 5, 5),
                new Cell(4, 5, 2),
                new Cell(5, 5, 6),
                new Cell(6, 5, 6, 6),
                new Cell(7, 5, 6),
                new Cell(8, 5, 6),
                new Cell(9, 5, 4),
                new Cell(1, 6, 5),
                new Cell(2, 6, 5),
                new Cell(3, 6, 7, 7),
                new Cell(4, 6, 2, 1),
                new Cell(5, 6, 6, 4),
                new Cell(6, 6, 6),
                new Cell(7, 6, 6),
                new Cell(8, 6, 6),
                new Cell(9, 6, 8),
                new Cell(1, 7, 5),
                new Cell(2, 7, 7),
                new Cell(3, 7, 7, 9),
                new Cell(4, 7, 9),
                new Cell(5, 7, 9, 8),
                new Cell(6, 7, 9),
                new Cell(7, 7, 6),
                new Cell(8, 7, 8, 3),
                new Cell(9, 7, 8),
                new Cell(1, 8, 7),
                new Cell(2, 8, 7, 5),
                new Cell(3, 8, 7),
                new Cell(4, 8, 7),
                new Cell(5, 8, 9),
                new Cell(6, 8, 9, 3),
                new Cell(7, 8, 8),
                new Cell(8, 8, 8),
                new Cell(9, 8, 8),
                new Cell(1, 9, 7),
                new Cell(2, 9, 7),
                new Cell(3, 9, 9),
                new Cell(4, 9, 9, 9),
                new Cell(5, 9, 9),
                new Cell(6, 9, 9),
                new Cell(7, 9, 8, 5),
                new Cell(8, 9, 8),
                new Cell(9, 9, 8),
            };
        }
    }
}