using System;
using System.Collections.Generic;
using System.Linq;

namespace jigsaw
{
    class Board
    {
        private bool hasChanges;

        private IReadOnlyCollection<Cell> Cells { get; }

        public Board()
        {
            Cells = Create();
            VerifySectors();
        }

        public void Solve()
        {
            foreach (var cell in Cells.Where(c => c.Value.HasValue).ToList())
            {
                UpdateAffectedCells(cell);
            }

            do
            {
                hasChanges = false;
            
                CheckForUniqueOptions();
                CheckForOptionsMustBeInSector();
                CheckForOptionsMustBeInRowOrColumn();
                CheckSectorCombinations();
            } while (hasChanges);
        }

        /// <summary>
        /// If an option available in two sectors in exactly two columns / rows respectively,
        /// than it cannot be present in a third sector in the same column / row.  
        /// </summary>
        private void CheckSectorCombinations()
        {
            var sectors = Cells
                .Select(c => c.Sector)
                .Distinct()
                .ToList();

            var sectorCombinations = (from sector1 in sectors
                from sector2 in sectors
                select new { sector1, sector2 }).ToList();

            for (int option = 1; option <= 9; option++)
            {
                foreach (var sectorCombination in sectorCombinations)
                {
                    var sector1 = sectorCombination.sector1;
                    var sector2 = sectorCombination.sector2;

                    var sector1CellsWithOption = GetSector(sector1)
                        .Where(c => c.Options.Contains(option))
                        .ToList();

                    var sector2CellsWithOption = GetSector(sector2)
                        .Where(c => c.Options.Contains(option))
                        .ToList();

                    var possibleRowsSector1 = sector1CellsWithOption
                        .Select(c => c.Row)
                        .Distinct()
                        .ToList();

                    var possibleRowsSector2 = sector2CellsWithOption
                        .Select(c => c.Row)
                        .Distinct()
                        .ToList();

                    if (possibleRowsSector1.Count == 2 && possibleRowsSector2.Count == 2)
                    {
                        var cellsWithOptionToRemove = possibleRowsSector1
                            .SelectMany(GetRow)
                            .Where(c => c.Sector != sector1 && c.Sector != sector2 && c.Options.Contains(option))
                            .ToList();

                        foreach (var cellWithOptionToRemove in cellsWithOptionToRemove)
                        {
                            RemoveOption(cellWithOptionToRemove, option);
                        }
                    }
                    
                    var possibleColumnsSector1 = sector1CellsWithOption
                        .Select(c => c.Column)
                        .Distinct()
                        .ToList();

                    var possibleColumnsSector2 = sector2CellsWithOption
                        .Select(c => c.Column)
                        .Distinct()
                        .ToList();

                    if (possibleColumnsSector1.Count == 2 && possibleColumnsSector2.Count == 2)
                    {
                        var cellsWithOptionToRemove = possibleColumnsSector1
                            .SelectMany(GetColumn)
                            .Where(c => c.Sector != sector1 && c.Sector != sector2 && c.Options.Contains(option))
                            .ToList();

                        foreach (var cellWithOptionToRemove in cellsWithOptionToRemove)
                        {
                            RemoveOption(cellWithOptionToRemove, option);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If an option in one sector is only available in one row / column respectively,
        /// than it cannot be available in another sector in the same row / column.
        /// </summary>
        private void CheckForOptionsMustBeInSector()
        {
            for (int option = 0; option <= 9; option++)
            {
                for (int sector = 1; sector <= 9; sector++)
                {
                    var cellsWithOption = GetSector(sector)
                        .Where(c => c.Options.Contains(option))
                        .ToList();

                    if (cellsWithOption.Count > 1)
                    {
                        var cell = cellsWithOption.First();

                        if (cellsWithOption.All(c => c.Row == cell.Row))
                        {
                            var cellsWithOptionToRemove = GetRow(cell.Row)
                                .Where(c => c.Sector != sector && c.Options.Contains(option))
                                .ToList();

                            foreach (var cellWithOptionToRemove in cellsWithOptionToRemove)
                            {
                                RemoveOption(cellWithOptionToRemove, option);
                            }
                        }
                        
                        if (cellsWithOption.All(c => c.Column == cell.Column))
                        {
                            var cellsWithOptionToRemove = GetColumn(cell.Column)
                                .Where(c => c.Sector != sector && c.Options.Contains(option))
                                .ToList();

                            foreach (var cellWithOptionToRemove in cellsWithOptionToRemove)
                            {
                                RemoveOption(cellWithOptionToRemove, option);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If an option in a row / column respectively is only present within in one sector,
        /// it cannot, this option can be eliminated from all other rows / columns within that sector.  
        /// </summary>
        private void CheckForOptionsMustBeInRowOrColumn()
        {
            for (int option = 0; option <= 9; option++)
            {
                for (int sector = 1; sector <= 9; sector++)
                {
                    var cellsWithOption = GetSector(sector)
                        .Where(c => c.Options.Contains(option))
                        .ToList();

                    var rows = cellsWithOption
                        .Select(c => c.Row)
                        .Distinct();

                    foreach (var row in rows)
                    {
                        if (cellsWithOption.Count(c => c.Row == row) > 1)
                        {
                            var cellsInRowOutsideSectorWithOption = GetRow(row)
                                .Where(c => c.Sector != sector && c.Options.Contains(option))
                                .ToList();
                            
                            if (!cellsInRowOutsideSectorWithOption.Any())
                            {
                                var cellsWithOptionToRemove = cellsWithOption
                                    .Where(c => c.Row != row)
                                    .ToList();

                                foreach (var cellWithOptionToRemove in cellsWithOptionToRemove)
                                {
                                    RemoveOption(cellWithOptionToRemove, option);
                                }
                            }
                        }
                    }
                    
                    var columns = cellsWithOption
                        .Select(c => c.Row)
                        .Distinct();

                    foreach (var column in columns)
                    {
                        if (cellsWithOption.Count(c => c.Column == column) > 1)
                        {
                            var cellsInColumnOutsideSectorWithOption = GetColumn(column)
                                .Where(c => c.Sector != sector && c.Options.Contains(option))
                                .ToList();
                            
                            if (!cellsInColumnOutsideSectorWithOption.Any())
                            {
                                var cellsWithOptionToRemove = cellsWithOption
                                    .Where(c => c.Column != column)
                                    .ToList();

                                foreach (var cellWithOptionToRemove in cellsWithOptionToRemove)
                                {
                                    RemoveOption(cellWithOptionToRemove, option);
                                }
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<Cell> GetColumn(int column)
        {
            return Cells.Where(c => c.Column == column);
        }

        private IEnumerable<Cell> GetRow(int row)
        {
            return Cells.Where(c => c.Row == row);
        }

        private IEnumerable<Cell> GetSector(int sector)
        {
            return Cells
                .Where(c => c.Sector == sector);
        }

        /// <summary>
        /// Checks all rows / columns / sectors if only one cell is left with a specific option.  
        /// </summary>
        private void CheckForUniqueOptions()
        {
            for (int option = 0; option <= 9; option++)
            {
                for (int x = 1; x <= 9; x++)
                {
                    var cellsWithOption = Cells
                        .Where(c => !c.Value.HasValue
                                              && c.Row == x
                                              && c.Options.Contains(option))
                        .ToList();

                    if (cellsWithOption.Count == 1)
                    {
                        var uniqueCellWithOption = cellsWithOption.Single();
                        uniqueCellWithOption.Value = option;
                        hasChanges = true;
                        
                        UpdateAffectedCells(uniqueCellWithOption);
                    }
                }
                
                for (int y = 1; y <= 9; y++)
                {
                    var cellsWithOption = Cells
                        .Where(c => !c.Value.HasValue
                                    && c.Column == y
                                    && c.Options.Contains(option))
                        .ToList();

                    if (cellsWithOption.Count == 1)
                    {
                        var uniqueCellWithOption = cellsWithOption.Single();
                        uniqueCellWithOption.Value = option;
                        hasChanges = true;
                        
                        UpdateAffectedCells(uniqueCellWithOption);
                    }
                }
                
                for (int sector = 1; sector <= 9; sector++)
                {
                    var cellsWithOption = Cells
                        .Where(c => !c.Value.HasValue
                                    && c.Sector == sector
                                    && c.Options.Contains(option))
                        .ToList();

                    if (cellsWithOption.Count == 1)
                    {
                        var uniqueCellWithOption = cellsWithOption.Single();
                        uniqueCellWithOption.Value = option;
                        hasChanges = true;
                        
                        UpdateAffectedCells(uniqueCellWithOption);
                    }
                }
            }
        }

        private void UpdateAffectedCells(Cell cell)
        {
            var affectedCells = Cells
                .Where(c => c.Row == cell.Row || c.Column == cell.Column || c.Sector == cell.Sector);

            var value = cell.Value.Value;

            foreach (var affectedCell in affectedCells)
            {
                RemoveOption(affectedCell, value);
            }
        }

        private void RemoveOption(Cell affectedCell, int value)
        {
            affectedCell.Options.Remove(value);
            hasChanges = true;

            if (!affectedCell.Value.HasValue && affectedCell.Options.Count == 1)
            {
                affectedCell.Value = affectedCell.Options.Single();

                UpdateAffectedCells(affectedCell);
            }
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

        public void RenderFull()
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

                    if (option == 5 && cell.Value.HasValue)
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
        }

        public void Render()
        {
            for (int y = 1; y <= 9; y++)
            {
                for (int x = 1; x <= 9; x++)
                {
                    var cell = Cells.Single(c => c.Row == x && c.Column == y);
                    var value = cell.Value.HasValue
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
                new Cell { Row = 1, Column = 1, Sector = 1, Value = 4, IsInitial = true },
                new Cell { Row = 2, Column = 1, Sector = 1 },
                new Cell { Row = 3, Column = 1, Sector = 1 },
                new Cell { Row = 4, Column = 1, Sector = 2, Value = 2, IsInitial = true },
                new Cell { Row = 5, Column = 1, Sector = 3, Value = 7, IsInitial = true },
                new Cell { Row = 6, Column = 1, Sector = 3 },
                new Cell { Row = 7, Column = 1, Sector = 3 },
                new Cell { Row = 8, Column = 1, Sector = 3 },
                new Cell { Row = 9, Column = 1, Sector = 3 },
                new Cell { Row = 1, Column = 2, Sector = 1 },
                new Cell { Row = 2, Column = 2, Sector = 1 },
                new Cell { Row = 3, Column = 2, Sector = 1 },
                new Cell { Row = 4, Column = 2, Sector = 2 },
                new Cell { Row = 5, Column = 2, Sector = 2, Value = 3, IsInitial = true },
                new Cell { Row = 6, Column = 2, Sector = 2 },
                new Cell { Row = 7, Column = 2, Sector = 3 },
                new Cell { Row = 8, Column = 2, Sector = 4 },
                new Cell { Row = 9, Column = 2, Sector = 3 },
                new Cell { Row = 1, Column = 3, Sector = 1 },
                new Cell { Row = 2, Column = 3, Sector = 5 },
                new Cell { Row = 3, Column = 3, Sector = 2, Value = 4, IsInitial = true },
                new Cell { Row = 4, Column = 3, Sector = 2 },
                new Cell { Row = 5, Column = 3, Sector = 6, Value = 6, IsInitial = true },
                new Cell { Row = 6, Column = 3, Sector = 6, Value = 2, IsInitial = true },
                new Cell { Row = 7, Column = 3, Sector = 3, Value = 9, IsInitial = true },
                new Cell { Row = 8, Column = 3, Sector = 4 },
                new Cell { Row = 9, Column = 3, Sector = 4, Value = 3, IsInitial = true },
                new Cell { Row = 1, Column = 4, Sector = 1 },
                new Cell { Row = 2, Column = 4, Sector = 5, Value = 1, IsInitial = true },
                new Cell { Row = 3, Column = 4, Sector = 5, Value = 8, IsInitial = true },
                new Cell { Row = 4, Column = 4, Sector = 2, Value = 6, IsInitial = true },
                new Cell { Row = 5, Column = 4, Sector = 6 },
                new Cell { Row = 6, Column = 4, Sector = 7 },
                new Cell { Row = 7, Column = 4, Sector = 3, Value = 3, IsInitial = true },
                new Cell { Row = 8, Column = 4, Sector = 4 },
                new Cell { Row = 9, Column = 4, Sector = 4 },
                new Cell { Row = 1, Column = 5, Sector = 1 },
                new Cell { Row = 2, Column = 5, Sector = 5, Value = 4, IsInitial = true },
                new Cell { Row = 3, Column = 5, Sector = 2 },
                new Cell { Row = 4, Column = 5, Sector = 2, Value = 8, IsInitial = true },
                new Cell { Row = 5, Column = 5, Sector = 6 },
                new Cell { Row = 6, Column = 5, Sector = 7, Value = 3, IsInitial = true },
                new Cell { Row = 7, Column = 5, Sector = 4 },
                new Cell { Row = 8, Column = 5, Sector = 4, Value = 9, IsInitial = true },
                new Cell { Row = 9, Column = 5, Sector = 4 },
                new Cell { Row = 1, Column = 6, Sector = 5 },
                new Cell { Row = 2, Column = 6, Sector = 5 },
                new Cell { Row = 3, Column = 6, Sector = 5 },
                new Cell { Row = 4, Column = 6, Sector = 6, Value = 3, IsInitial = true },
                new Cell { Row = 5, Column = 6, Sector = 6, Value = 1, IsInitial = true },
                new Cell { Row = 6, Column = 6, Sector = 7, Value = 8, IsInitial = true },
                new Cell { Row = 7, Column = 6, Sector = 7 },
                new Cell { Row = 8, Column = 6, Sector = 9 },
                new Cell { Row = 9, Column = 6, Sector = 4 },
                new Cell { Row = 1, Column = 7, Sector = 8 },
                new Cell { Row = 2, Column = 7, Sector = 8, Value = 6, IsInitial = true },
                new Cell { Row = 3, Column = 7, Sector = 5 },
                new Cell { Row = 4, Column = 7, Sector = 6, Value = 4, IsInitial = true },
                new Cell { Row = 5, Column = 7, Sector = 7, Value = 2, IsInitial = true },
                new Cell { Row = 6, Column = 7, Sector = 7 },
                new Cell { Row = 7, Column = 7, Sector = 7 },
                new Cell { Row = 8, Column = 7, Sector = 9 },
                new Cell { Row = 9, Column = 7, Sector = 9 },
                new Cell { Row = 1, Column = 8, Sector = 8 },
                new Cell { Row = 2, Column = 8, Sector = 8 },
                new Cell { Row = 3, Column = 8, Sector = 5 },
                new Cell { Row = 4, Column = 8, Sector = 6 },
                new Cell { Row = 5, Column = 8, Sector = 6 },
                new Cell { Row = 6, Column = 8, Sector = 7 },
                new Cell { Row = 7, Column = 8, Sector = 7 },
                new Cell { Row = 8, Column = 8, Sector = 9 },
                new Cell { Row = 9, Column = 8, Sector = 9 },
                new Cell { Row = 1, Column = 9, Sector = 8 },
                new Cell { Row = 2, Column = 9, Sector = 8 },
                new Cell { Row = 3, Column = 9, Sector = 8, Value = 7, IsInitial = true },
                new Cell { Row = 4, Column = 9, Sector = 8 },
                new Cell { Row = 5, Column = 9, Sector = 8 },
                new Cell { Row = 6, Column = 9, Sector = 9 },
                new Cell { Row = 7, Column = 9, Sector = 9 },
                new Cell { Row = 8, Column = 9, Sector = 9 },
                new Cell { Row = 9, Column = 9, Sector = 9 }
            };
        }
        
        private static IReadOnlyCollection<Cell> CreateOld()
        {
            return new[]
            {
                new Cell { Row = 1, Column = 1, Sector = 1 },
                new Cell { Row = 2, Column = 1, Sector = 1 },
                new Cell { Row = 3, Column = 1, Sector = 1, Value = 3, IsInitial = true },
                new Cell { Row = 4, Column = 1, Sector = 1 },
                new Cell { Row = 5, Column = 1, Sector = 2 },
                new Cell { Row = 6, Column = 1, Sector = 3 },
                new Cell { Row = 7, Column = 1, Sector = 3 },
                new Cell { Row = 8, Column = 1, Sector = 3 },
                new Cell { Row = 9, Column = 1, Sector = 3 },
                new Cell { Row = 1, Column = 2, Sector = 1 },
                new Cell { Row = 2, Column = 2, Sector = 1, Value = 6, IsInitial = true },
                new Cell { Row = 3, Column = 2, Sector = 1, Value = 5, IsInitial = true },
                new Cell { Row = 4, Column = 2, Sector = 2, Value = 3, IsInitial = true },
                new Cell { Row = 5, Column = 2, Sector = 2 },
                new Cell { Row = 6, Column = 2, Sector = 3, Value = 7, IsInitial = true },
                new Cell { Row = 7, Column = 2, Sector = 3 },
                new Cell { Row = 8, Column = 2, Sector = 4, Value = 1, IsInitial = true },
                new Cell { Row = 9, Column = 2, Sector = 4, Value = 9, IsInitial = true },
                new Cell { Row = 1, Column = 3, Sector = 1 },
                new Cell { Row = 2, Column = 3, Sector = 1 },
                new Cell { Row = 3, Column = 3, Sector = 2, Value = 6, IsInitial = true },
                new Cell { Row = 4, Column = 3, Sector = 2 },
                new Cell { Row = 5, Column = 3, Sector = 2 },
                new Cell { Row = 6, Column = 3, Sector = 3, Value = 2, IsInitial = true },
                new Cell { Row = 7, Column = 3, Sector = 4 },
                new Cell { Row = 8, Column = 3, Sector = 4 },
                new Cell { Row = 9, Column = 3, Sector = 4 },
                new Cell { Row = 1, Column = 4, Sector = 5, Value = 1, IsInitial = true },
                new Cell { Row = 2, Column = 4, Sector = 5 },
                new Cell { Row = 3, Column = 4, Sector = 5 },
                new Cell { Row = 4, Column = 4, Sector = 2 },
                new Cell { Row = 5, Column = 4, Sector = 3 },
                new Cell { Row = 6, Column = 4, Sector = 3, Value = 9, IsInitial = true },
                new Cell { Row = 7, Column = 4, Sector = 4, Value = 6, IsInitial = true },
                new Cell { Row = 8, Column = 4, Sector = 4 },
                new Cell { Row = 9, Column = 4, Sector = 4 },
                new Cell { Row = 1, Column = 5, Sector = 5 },
                new Cell { Row = 2, Column = 5, Sector = 5 },
                new Cell { Row = 3, Column = 5, Sector = 5 },
                new Cell { Row = 4, Column = 5, Sector = 2 },
                new Cell { Row = 5, Column = 5, Sector = 6 },
                new Cell { Row = 6, Column = 5, Sector = 6, Value = 6, IsInitial = true },
                new Cell { Row = 7, Column = 5, Sector = 6 },
                new Cell { Row = 8, Column = 5, Sector = 6 },
                new Cell { Row = 9, Column = 5, Sector = 4 },
                new Cell { Row = 1, Column = 6, Sector = 5 },
                new Cell { Row = 2, Column = 6, Sector = 5 },
                new Cell { Row = 3, Column = 6, Sector = 7, Value = 7, IsInitial = true },
                new Cell { Row = 4, Column = 6, Sector = 2, Value = 1, IsInitial = true },
                new Cell { Row = 5, Column = 6, Sector = 6, Value = 4, IsInitial = true },
                new Cell { Row = 6, Column = 6, Sector = 6 },
                new Cell { Row = 7, Column = 6, Sector = 6 },
                new Cell { Row = 8, Column = 6, Sector = 6 },
                new Cell { Row = 9, Column = 6, Sector = 8 },
                new Cell { Row = 1, Column = 7, Sector = 5 },
                new Cell { Row = 2, Column = 7, Sector = 7 },
                new Cell { Row = 3, Column = 7, Sector = 7, Value = 9, IsInitial = true },
                new Cell { Row = 4, Column = 7, Sector = 9 },
                new Cell { Row = 5, Column = 7, Sector = 9, Value = 8, IsInitial = true },
                new Cell { Row = 6, Column = 7, Sector = 9 },
                new Cell { Row = 7, Column = 7, Sector = 6 },
                new Cell { Row = 8, Column = 7, Sector = 8, Value = 3, IsInitial = true },
                new Cell { Row = 9, Column = 7, Sector = 8 },
                new Cell { Row = 1, Column = 8, Sector = 7 },
                new Cell { Row = 2, Column = 8, Sector = 7, Value = 5, IsInitial = true },
                new Cell { Row = 3, Column = 8, Sector = 7 },
                new Cell { Row = 4, Column = 8, Sector = 7 },
                new Cell { Row = 5, Column = 8, Sector = 9 },
                new Cell { Row = 6, Column = 8, Sector = 9, Value = 3, IsInitial = true },
                new Cell { Row = 7, Column = 8, Sector = 8 },
                new Cell { Row = 8, Column = 8, Sector = 8 },
                new Cell { Row = 9, Column = 8, Sector = 8 },
                new Cell { Row = 1, Column = 9, Sector = 7 },
                new Cell { Row = 2, Column = 9, Sector = 7 },
                new Cell { Row = 3, Column = 9, Sector = 9 },
                new Cell { Row = 4, Column = 9, Sector = 9, Value = 9, IsInitial = true },
                new Cell { Row = 5, Column = 9, Sector = 9 },
                new Cell { Row = 6, Column = 9, Sector = 9 },
                new Cell { Row = 7, Column = 9, Sector = 8, Value = 5, IsInitial = true },
                new Cell { Row = 8, Column = 9, Sector = 8 },
                new Cell { Row = 9, Column = 9, Sector = 8 }
            };
        }
    }
}