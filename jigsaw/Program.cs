namespace jigsaw
{
    class Program
    {
        static void Main()
        {
            var board = new Board();
            var solver = new Solver(board);

            solver.Solve();
            board.Render();
            board.RenderFull();
        }
    }
}
