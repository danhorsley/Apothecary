using System;
using System.IO;

namespace ApothecaryGame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Ensure the Content directory exists
                if (!Directory.Exists("Content"))
                {
                    Directory.CreateDirectory("Content");
                    Console.WriteLine("Created Content directory");
                }

                // Check if Font.ttf file exists, if not provide guidance
                if (!File.Exists("Content/Font.ttf"))
                {
                    Console.WriteLine("Font.ttf file is missing from the Content directory!");
                    Console.WriteLine("Please add a TTF font file to the Content directory and name it Font.ttf");
                    Console.WriteLine("You can download one using this command in the Shell:");
                    Console.WriteLine("curl -o Content/Font.ttf https://github.com/google/fonts/raw/main/apache/roboto/static/Roboto-Regular.ttf");

                    // Wait for user to acknowledge
                    Console.WriteLine("\nPress Enter to continue anyway (the game may crash without a font)...");
                    Console.ReadLine();
                }

                // Ensure the Saves directory exists
                if (!Directory.Exists("Saves"))
                {
                    Directory.CreateDirectory("Saves");
                    Console.WriteLine("Created Saves directory");
                }

                // Set environment variables for headless/software rendering
                Environment.SetEnvironmentVariable("DOTNET_SYSTEM_DRAWING_ENABLEUNIXGDIPLUS", "1");
                Environment.SetEnvironmentVariable("SDL_VIDEODRIVER", "dummy");
                Environment.SetEnvironmentVariable("OPENAL_ENABLE_CAPTURE", "0");
                
                // Create and run the game
                using (var game = new Game1())
                {
                    game.Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("\nPress Enter to exit...");
                Console.ReadLine();
            }
        }
    }
}