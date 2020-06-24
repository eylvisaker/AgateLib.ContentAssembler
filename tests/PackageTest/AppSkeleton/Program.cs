using System;

namespace App
{
    public static class Program
    {
        public static int ExitCode = 1;

        [STAThread]
        static int Main()
        {
            Console.WriteLine("Starting test app...");
            
            using (var game = new Game1())
                game.Run();

            return ExitCode;
        }
    }
}
