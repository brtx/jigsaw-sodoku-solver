using System.Linq;

namespace jigsaw
{
    internal class Solver
    {
        private readonly Board board;
        private bool hasChanges;

        internal Solver(Board board)
        {
            this.board = board;
        }

        internal void Solve()
        {
            foreach (var cell in board.Cells.Where(c => c.Value.HasValue).ToList())
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
        
        private void UpdateAffectedCells(Cell cell)
        {
            var affectedCells = board.Cells
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

        /// <summary>
        /// Checks all rows / columns / sectors if only one cell is left with a specific option.  
        /// </summary>
        private void CheckForUniqueOptions()
        {
            for (int option = 0; option <= 9; option++)
            {
                for (int x = 1; x <= 9; x++)
                {
                    var cellsWithOption = board.Cells
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
                    var cellsWithOption = board.Cells
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
                    var cellsWithOption = board.Cells
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
                    var cellsWithOption = board.GetSector(sector)
                        .Where(c => c.Options.Contains(option))
                        .ToList();

                    if (cellsWithOption.Count > 1)
                    {
                        var cell = cellsWithOption.First();

                        if (cellsWithOption.All(c => c.Row == cell.Row))
                        {
                            var cellsWithOptionToRemove = board.GetRow(cell.Row)
                                .Where(c => c.Sector != sector && c.Options.Contains(option))
                                .ToList();

                            foreach (var cellWithOptionToRemove in cellsWithOptionToRemove)
                            {
                                RemoveOption(cellWithOptionToRemove, option);
                            }
                        }
                        
                        if (cellsWithOption.All(c => c.Column == cell.Column))
                        {
                            var cellsWithOptionToRemove = board.GetColumn(cell.Column)
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
                    var cellsWithOption = board.GetSector(sector)
                        .Where(c => c.Options.Contains(option))
                        .ToList();

                    var rows = cellsWithOption
                        .Select(c => c.Row)
                        .Distinct();

                    foreach (var row in rows)
                    {
                        if (cellsWithOption.Count(c => c.Row == row) > 1)
                        {
                            var cellsInRowOutsideSectorWithOption = board.GetRow(row)
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
                            var cellsInColumnOutsideSectorWithOption = board.GetColumn(column)
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

        /// <summary>
        /// If an option available in two sectors in exactly two columns / rows respectively,
        /// than it cannot be present in a third sector in the same column / row.  
        /// </summary>
        private void CheckSectorCombinations()
        {
            var sectors = board.Cells
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

                    var sector1CellsWithOption = board.GetSector(sector1)
                        .Where(c => c.Options.Contains(option))
                        .ToList();

                    var sector2CellsWithOption = board.GetSector(sector2)
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
                            .SelectMany(board.GetRow)
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
                            .SelectMany(board.GetColumn)
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
    }
}