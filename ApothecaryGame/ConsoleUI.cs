using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApothecaryGame
{
    /// <summary>
    /// Console user interface helper for Replit and headless environments
    /// This class provides a basic command-line UI that runs alongside the game
    /// to help with testing and interaction in environments where the graphical UI might not be visible
    /// </summary>
    public class ConsoleUI
    {
        private Game1 _game;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _consoleTask;

        public ConsoleUI(Game1 game)
        {
            _game = game;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            _consoleTask = Task.Run(RunConsoleUI, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            try
            {
                _consoleTask?.Wait(1000); // Wait for the task to complete
            }
            catch (OperationCanceledException)
            {
                // This is expected when canceling
            }
        }

        private void RunConsoleUI()
        {
            try
            {
                Console.WriteLine("\n=== Apothecary Game Console Interface ===");
                Console.WriteLine("This console interface allows you to interact with the game in headless environments.");
                PrintCommands();

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        ProcessKey(key.KeyChar);
                    }

                    Thread.Sleep(100); // Short delay to prevent high CPU usage
                }
            }
            catch (OperationCanceledException)
            {
                // This is expected when canceling
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Console UI error: {ex.Message}");
            }
        }

        private void PrintCommands()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  1 - Change to Shop mode");
            Console.WriteLine("  2 - Change to Mixing mode");
            Console.WriteLine("  3 - Change to Exploration mode");
            Console.WriteLine("  s - Sell potion");
            Console.WriteLine("  b - Buy ingredient");
            Console.WriteLine("  m - Mix potion");
            Console.WriteLine("  n - Get new customer");
            Console.WriteLine("  i - Show inventory");
            Console.WriteLine("  p - Show potions");
            Console.WriteLine("  r - Show recipe book");
            Console.WriteLine("  h - Show these commands");
            Console.WriteLine("  q - Quit game");
            Console.WriteLine();
        }

        private void ProcessKey(char key)
        {
            try
            {
                switch (key)
                {
                    case '1':
                        _game.ChangeState(GameState.Shop);
                        Console.WriteLine("Changed to Shop mode");
                        break;
                    case '2':
                        _game.ChangeState(GameState.Mixing);
                        Console.WriteLine("Changed to Mixing mode");
                        break;
                    case '3':
                        _game.ChangeState(GameState.Exploration);
                        Console.WriteLine("Changed to Exploration mode");
                        break;
                    case 's':
                        SellPotion();
                        break;
                    case 'b':
                        BuyIngredient();
                        break;
                    case 'm':
                        MixPotion();
                        break;
                    case 'n':
                        GetNewCustomer();
                        break;
                    case 'i':
                        ShowInventory();
                        break;
                    case 'p':
                        ShowPotions();
                        break;
                    case 'r':
                        ShowRecipes();
                        break;
                    case 'h':
                        PrintCommands();
                        break;
                    case 'q':
                        Console.WriteLine("Exiting game...");
                        _game.Exit();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing command: {ex.Message}");
            }
        }

        private void SellPotion()
        {
            try
            {
                _game.SellFirstPotion();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selling potion: {ex.Message}");
            }
        }

        private void BuyIngredient()
        {
            try
            {
                _game.BuyRandomIngredient();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error buying ingredient: {ex.Message}");
            }
        }

        private void MixPotion()
        {
            try
            {
                _game.MixRandomPotion();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mixing potion: {ex.Message}");
            }
        }

        private void GetNewCustomer()
        {
            try
            {
                _game.GetNewCustomer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting new customer: {ex.Message}");
            }
        }

        private void ShowInventory()
        {
            try
            {
                string inventory = _game.GetPlayerInventoryString();
                Console.WriteLine("\nPlayer inventory:");
                Console.WriteLine(inventory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing inventory: {ex.Message}");
            }
        }

        private void ShowPotions()
        {
            try
            {
                string potions = _game.GetPlayerPotionsString();
                Console.WriteLine("\nPlayer potions:");
                Console.WriteLine(potions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing potions: {ex.Message}");
            }
        }

        private void ShowRecipes()
        {
            try
            {
                string recipes = _game.GetRecipeBookString();
                Console.WriteLine("\nKnown recipes:");
                Console.WriteLine(recipes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing recipes: {ex.Message}");
            }
        }
    }
}