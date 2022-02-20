namespace jigsaw
{
    class Program
    {
        static void Main()
        {
            var board = new Board();

            board.Solve();
            board.Render();
            board.RenderFull();
        }
    }
}
