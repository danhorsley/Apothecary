using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ApothecaryGame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Console.WriteLine("Starting Apothecary Game (Console-Only Version)...");

                // Ensure the Content directory exists
                if (!Directory.Exists("Content"))
                {
                    Directory.CreateDirectory("Content");
                    Console.WriteLine("Created Content directory");
                }

                // Ensure the Saves directory exists
                if (!Directory.Exists("Saves"))
                {
                    Directory.CreateDirectory("Saves");
                    Console.WriteLine("Created Saves directory");
                }

                // Check if we're running in Replit or another headless environment
                bool isHeadlessEnvironment = 
                    Environment.GetEnvironmentVariable("REPL_ID") != null || // Replit
                    Environment.GetEnvironmentVariable("CI") != null;       // CI environment

                if (isHeadlessEnvironment)
                {
                    Console.WriteLine("Detected headless environment. Running in console-only mode.");
                    RunConsoleOnlyMode();
                }
                else
                {
                    // Set environment variables for headless rendering
                    Console.WriteLine("Setting up environment for graphics mode...");

                    // These settings help MonoGame run in challenging environments
                    Environment.SetEnvironmentVariable("DISPLAY", ":99");
                    Environment.SetEnvironmentVariable("SDL_VIDEODRIVER", "dummy");
                    Environment.SetEnvironmentVariable("LIBGL_ALWAYS_SOFTWARE", "1");
                    Environment.SetEnvironmentVariable("MESA_GL_VERSION_OVERRIDE", "3.3");
                    Environment.SetEnvironmentVariable("SDL_AUDIODRIVER", "dummy");
                    Environment.SetEnvironmentVariable("MGFX_PREFER_HLSL_SHADER", "0");

                    // Try to run the game
                    Console.WriteLine("Creating game instance...");

                    // Create and run the game
                    using (var game = new Game1())
                    {
                        Console.WriteLine("Game instance created, calling Run()");
                        game.Run();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                // Fall back to console mode if we encountered a graphics error
                if (ex.Message.Contains("graphics device") || ex.Message.Contains("GraphicsDevice"))
                {
                    Console.WriteLine("\nFalling back to console-only mode...");
                    RunConsoleOnlyMode();
                }
                else
                {
                    Console.WriteLine("\nPress Enter to exit...");
                    Console.ReadLine();
                }
            }
        }

        static void RunConsoleOnlyMode()
        {
            // Create a simple text-based version of the game
            var consoleGame = new ConsoleOnlyGame();
            consoleGame.Start();
        }
    }

    /// <summary>
    /// A simplified console-only version of the Apothecary game for headless environments
    /// </summary>
    public class ConsoleOnlyGame
    {
        // Core game data
        private Player _player;
        private RecipeBook _recipeBook;
        private Customer _currentCustomer;
        private Forest _forest;
        private GameState _currentState;

        // Cancellation for the game loop
        private CancellationTokenSource _cancellationSource;

        // Save file paths
        private const string SaveDirectory = "Saves";
        private const string RecipeBookFile = "recipebook.json";
        private const string PlayerFile = "player.json";

        public ConsoleOnlyGame()
        {
            // Initialize player and recipe book
            _player = new Player();
            _recipeBook = new RecipeBook();
            _currentState = GameState.Shop;
            _cancellationSource = new CancellationTokenSource();

            try
            {
                // Try to load saved data
                LoadGameData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading saved data: {ex.Message}");

                // Add some starter ingredients if we couldn't load data
                if (_player.Inventory.Count == 0)
                {
                    _player.AddIngredient(new Ingredient { Name = "Red Herb", Type = "Herb", Rarity = 1 });
                    _player.AddIngredient(new Ingredient { Name = "Blue Crystal", Type = "Crystal", Rarity = 2 });
                    _player.AddIngredient(new Ingredient { Name = "Spotted Cap", Type = "Mushroom", Rarity = 1 });
                    _player.AddIngredient(new Ingredient { Name = "Green Herb", Type = "Herb", Rarity = 1 });
                }
            }

            // Generate a customer
            _currentCustomer = Customer.CreateRandom();

            // Generate forest
            _forest = new Forest();
        }

        public void Start()
        {
            try
            {
                Console.Clear();
                PrintWelcome();
                PrintCommands();

                while (!_cancellationSource.IsCancellationRequested)
                {
                    // Display the current game state
                    PrintGameState();

                    // Wait for input
                    Console.Write("\nCommand: ");
                    string input = Console.ReadLine().Trim().ToLower();

                    // Process the command
                    ProcessCommand(input);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Game error: {ex.Message}");
            }
        }

        private void PrintWelcome()
        {
            Console.WriteLine("===============================================");
            Console.WriteLine("  APOTHECARY - Console Edition");
            Console.WriteLine("  Mix potions, sell to customers, explore!");
            Console.WriteLine("===============================================");
            Console.WriteLine();
        }

        private void PrintGameState()
        {
            Console.WriteLine($"\n--- {_currentState} ---");
            Console.WriteLine($"Gold: {_player.Gold}  |  Health: {_player.Health}  |  Reputation: {_player.Reputation}");

            switch (_currentState)
            {
                case GameState.Shop:
                    Console.WriteLine($"\nCustomer: {_currentCustomer.Name} ({_currentCustomer.Type})");
                    Console.WriteLine($"Needs: {_currentCustomer.Need} potion");
                    Console.WriteLine($"Reward: {_currentCustomer.Reward} gold");
                    break;

                case GameState.Mixing:
                    Console.WriteLine("\nReady to mix potions!");
                    break;

                case GameState.Exploration:
                    Console.WriteLine("\nExploring the forest...");
                    Console.WriteLine("You might find ingredients or face hazards!");
                    break;
            }
        }

        private void PrintCommands()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  shop - Go to Shop mode");
            Console.WriteLine("  mix - Go to Mixing mode");
            Console.WriteLine("  explore - Go to Exploration mode");
            Console.WriteLine("  inv - Show your inventory");
            Console.WriteLine("  potions - Show your potions");
            Console.WriteLine("  recipes - Show your recipe book");
            Console.WriteLine("  buy - Buy a random ingredient");
            Console.WriteLine("  sell # - Sell the potion with given number");
            Console.WriteLine("  mix # # - Mix two ingredients by number");
            Console.WriteLine("  gather - Gather an ingredient while exploring");
            Console.WriteLine("  customer - Get a new customer");
            Console.WriteLine("  save - Save your game");
            Console.WriteLine("  help - Show these commands");
            Console.WriteLine("  exit - Quit the game");
        }

        private void ProcessCommand(string input)
        {
            try
            {
                // Split the input into command and parameters
                var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var command = parts.Length > 0 ? parts[0] : "";

                switch (command)
                {
                    case "shop":
                        _currentState = GameState.Shop;
                        break;

                    case "mix":
                        // Check if this is a mixing command with ingredients
                        if (parts.Length > 2 && 
                            int.TryParse(parts[1], out int ing1) && 
                            int.TryParse(parts[2], out int ing2))
                        {
                            // It's a mix ingredients command
                            if (_currentState == GameState.Mixing)
                                MixPotion(ing1 - 1, ing2 - 1); // Convert to 0-based
                            else
                                Console.WriteLine("You can only mix potions in the mixing area!");
                        }
                        else
                        {
                            // It's just changing to mix mode
                            _currentState = GameState.Mixing;
                        }
                        break;

                    case "explore":
                        _currentState = GameState.Exploration;
                        break;

                    case "inv":
                        ShowInventory();
                        break;

                    case "potions":
                        ShowPotions();
                        break;

                    case "recipes":
                        ShowRecipes();
                        break;

                    case "buy":
                        if (_currentState == GameState.Shop)
                            BuyIngredient();
                        else
                            Console.WriteLine("You can only buy ingredients in the shop!");
                        break;

                    case "sell":
                        if (_currentState == GameState.Shop)
                        {
                            if (parts.Length > 1 && int.TryParse(parts[1], out int potionIndex))
                                SellPotion(potionIndex - 1); // Convert to 0-based
                            else
                                Console.WriteLine("Please specify which potion to sell (e.g., 'sell 1')");
                        }
                        else
                            Console.WriteLine("You can only sell potions in the shop!");
                        break;

                    case "gather":
                        if (_currentState == GameState.Exploration)
                            GatherIngredient();
                        else
                            Console.WriteLine("You can only gather ingredients while exploring!");
                        break;

                    case "customer":
                        if (_currentState == GameState.Shop)
                        {
                            _currentCustomer = Customer.CreateRandom();
                            Console.WriteLine($"New customer: {_currentCustomer.Name} ({_currentCustomer.Type})");
                        }
                        else
                            Console.WriteLine("You can only get new customers in the shop!");
                        break;

                    case "save":
                        SaveGameData();
                        Console.WriteLine("Game saved!");
                        break;

                    case "help":
                        PrintCommands();
                        break;

                    case "exit":
                    case "quit":
                        SaveGameData();
                        _cancellationSource.Cancel();
                        Console.WriteLine("Thank you for playing! Game data saved.");
                        break;

                    default:
                        Console.WriteLine("Unknown command. Type 'help' for available commands.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing command: {ex.Message}");
            }
        }

        private void ShowInventory()
        {
            Console.WriteLine("\n--- Ingredients ---");

            if (_player.Inventory.Count == 0)
            {
                Console.WriteLine("Your inventory is empty.");
                return;
            }

            for (int i = 0; i < _player.Inventory.Count; i++)
            {
                var ingredient = _player.Inventory[i];
                Console.WriteLine($"{i+1}. {ingredient.Name} ({ingredient.Type}, Rarity: {ingredient.Rarity})");
            }
        }

        private void ShowPotions()
        {
            Console.WriteLine("\n--- Potions ---");

            if (_player.Potions.Count == 0)
            {
                Console.WriteLine("You don't have any potions.");
                return;
            }

            for (int i = 0; i < _player.Potions.Count; i++)
            {
                var potion = _player.Potions[i];
                bool matchesNeed = _currentState == GameState.Shop && potion.Effect == _currentCustomer.Need;

                if (matchesNeed)
                    Console.WriteLine($"{i+1}. {potion.Effect} potion (Value: {potion.Value}) [MATCHES CUSTOMER NEED!]");
                else
                    Console.WriteLine($"{i+1}. {potion.Effect} potion (Value: {potion.Value})");
            }
        }

        private void ShowRecipes()
        {
            Console.WriteLine("\n--- Recipe Book ---");

            if (_recipeBook.KnownRecipes.Count == 0)
            {
                Console.WriteLine("Your recipe book is empty. Mix potions to discover recipes!");
                return;
            }

            for (int i = 0; i < _recipeBook.KnownRecipes.Count; i++)
            {
                var recipe = _recipeBook.KnownRecipes[i];
                Console.WriteLine($"{i+1}. {recipe.Effect} potion (Value: {recipe.Value})");
            }
        }

        private void BuyIngredient()
        {
            // Simple random ingredient purchasing
            if (_player.Gold >= 20 && _player.CanAddIngredient())
            {
                // Random ingredient type
                string[] types = { "Herb", "Crystal", "Mushroom" };
                string[] herbNames = { "Red Herb", "Blue Herb", "Green Herb", "Yellow Herb" };
                string[] crystalNames = { "Blue Crystal", "Red Crystal", "Clear Crystal" };
                string[] mushroomNames = { "Spotted Cap", "Glowing Fungus", "Shadow Shroom" };

                Random random = new Random();
                string type = types[random.Next(types.Length)];
                string name;
                int rarity;
                int cost;

                switch (type)
                {
                    case "Herb":
                        name = herbNames[random.Next(herbNames.Length)];
                        rarity = random.Next(1, 3); // 1-2 rarity
                        cost = 20;
                        break;
                    case "Crystal":
                        name = crystalNames[random.Next(crystalNames.Length)];
                        rarity = random.Next(2, 4); // 2-3 rarity
                        cost = 30;
                        break;
                    case "Mushroom":
                        name = mushroomNames[random.Next(mushroomNames.Length)];
                        rarity = random.Next(1, 4); // 1-3 rarity
                        cost = 25;
                        break;
                    default:
                        name = "Unknown";
                        rarity = 1;
                        cost = 10;
                        break;
                }

                // Check if player has enough gold
                if (_player.Gold >= cost)
                {
                    _player.Gold -= cost;
                    _player.AddIngredient(new Ingredient
                    {
                        Name = name,
                        Type = type,
                        Rarity = rarity
                    });

                    Console.WriteLine($"Bought {name} for {cost} gold (Rarity: {rarity})");
                }
                else
                {
                    Console.WriteLine($"Not enough gold! You need {cost} gold.");
                }
            }
            else if (_player.Gold < 20)
            {
                Console.WriteLine("Not enough gold! The cheapest ingredients cost 20 gold.");
            }
            else
            {
                Console.WriteLine("Your inventory is full! Sell potions or use ingredients first.");
            }
        }

        private void SellPotion(int potionIndex)
        {
            if (potionIndex >= 0 && potionIndex < _player.Potions.Count)
            {
                var potion = _player.Potions[potionIndex];

                // Check if potion matches customer need
                if (potion.Effect == _currentCustomer.Need)
                {
                    // Successful sale
                    _player.Gold += _currentCustomer.Reward;
                    _player.Reputation += 5;

                    Console.WriteLine($"Success! You sold a {potion.Effect} potion to {_currentCustomer.Name} for {_currentCustomer.Reward} gold!");
                    Console.WriteLine($"Your reputation increased by 5.");

                    // Remove sold potion
                    _player.RemovePotion(potionIndex);

                    // Get a new customer
                    _currentCustomer = Customer.CreateRandom();
                    Console.WriteLine($"New customer arrived: {_currentCustomer.Name} ({_currentCustomer.Type})");
                    Console.WriteLine($"Needs: {_currentCustomer.Need} potion, Reward: {_currentCustomer.Reward} gold");
                }
                else
                {
                    // Failed sale
                    _player.Reputation -= 10;

                    Console.WriteLine($"Failed! {_currentCustomer.Name} wanted a {_currentCustomer.Need} potion, but you gave them a {potion.Effect} potion.");
                    Console.WriteLine("Your reputation decreased by 10.");

                    // Remove used potion
                    _player.RemovePotion(potionIndex);
                }
            }
            else
            {
                Console.WriteLine("Invalid potion number! Check your potions with 'potions' command.");
            }
        }

        private void MixPotion(int index1, int index2)
        {
            if (index1 < 0 || index1 >= _player.Inventory.Count ||
                index2 < 0 || index2 >= _player.Inventory.Count)
            {
                Console.WriteLine("Invalid ingredient numbers! Check your inventory with 'inv' command.");
                return;
            }

            if (index1 == index2)
            {
                Console.WriteLine("You need to select two different ingredients!");
                return;
            }

            // Get the ingredients (make copies since we'll be removing them)
            var ingredient1 = _player.Inventory[index1];
            var ingredient2 = _player.Inventory[index2];

            // Create the potion
            var potion = new Potion(ingredient1, ingredient2);

            Console.WriteLine($"Mixing {ingredient1.Name} and {ingredient2.Name}...");
            Console.WriteLine($"Created a {potion.Effect} potion (Value: {potion.Value})!");

            // Add to recipe book and player's potions
            _recipeBook.AddRecipe(potion);
            _player.AddPotion(potion);

            // Remove the used ingredients (remove higher index first)
            if (index1 > index2)
            {
                _player.RemoveIngredient(index1);
                _player.RemoveIngredient(index2);
            }
            else
            {
                _player.RemoveIngredient(index2);
                _player.RemoveIngredient(index1);
            }
        }

        private void GatherIngredient()
        {
            Random random = new Random();

            // 70% chance to find ingredient, 20% chance to find hazard, 10% chance to find nothing
            double roll = random.NextDouble();

            if (roll < 0.7)
            {
                // Found an ingredient
                if (_player.CanAddIngredient())
                {
                    // Generate a random ingredient
                    string[] types = { "Herb", "Crystal", "Mushroom" };
                    string[] herbNames = { "Red Herb", "Blue Herb", "Green Herb", "Yellow Herb" };
                    string[] crystalNames = { "Blue Crystal", "Red Crystal", "Clear Crystal" };
                    string[] mushroomNames = { "Spotted Cap", "Glowing Fungus", "Shadow Shroom" };

                    string type = types[random.Next(types.Length)];
                    string name;
                    int rarity;

                    switch (type)
                    {
                        case "Herb":
                            name = herbNames[random.Next(herbNames.Length)];
                            rarity = random.Next(1, 3); // 1-2 rarity
                            break;
                        case "Crystal":
                            name = crystalNames[random.Next(crystalNames.Length)];
                            rarity = random.Next(2, 4); // 2-3 rarity
                            break;
                        case "Mushroom":
                            name = mushroomNames[random.Next(mushroomNames.Length)];
                            rarity = random.Next(1, 4); // 1-3 rarity
                            break;
                        default:
                            name = "Unknown";
                            rarity = 1;
                            break;
                    }

                    var ingredient = new Ingredient
                    {
                        Name = name,
                        Type = type,
                        Rarity = rarity
                    };

                    _player.AddIngredient(ingredient);
                    Console.WriteLine($"You found a {name} (Rarity: {rarity})!");
                }
                else
                {
                    Console.WriteLine("You found an ingredient, but your inventory is full!");
                }
            }
            else if (roll < 0.9)
            {
                // Encountered a hazard
                string[] hazardTypes = { "Thorns", "Weak Enemy", "Trap" };
                string hazardType = hazardTypes[random.Next(hazardTypes.Length)];
                int damage;

                switch (hazardType)
                {
                    case "Thorns":
                        damage = 10;
                        break;
                    case "Weak Enemy":
                        damage = 20;
                        break;
                    case "Trap":
                        damage = 15;
                        break;
                    default:
                        damage = 5;
                        break;
                }

                _player.Health -= damage;
                Console.WriteLine($"You encountered {hazardType} and took {damage} damage!");

                if (_player.Health <= 0)
                {
                    Console.WriteLine("You died! Returning to the shop...");
                    _player.Health = 100;
                    _player.Gold = Math.Max(0, _player.Gold - 50);
                    _player.Inventory.Clear();
                    _currentState = GameState.Shop;
                }
                else
                {
                    Console.WriteLine($"Your health: {_player.Health}");
                }
            }
            else
            {
                // Found nothing
                Console.WriteLine("You searched but found nothing of interest.");
            }
        }

        // Save game data to files
        private void SaveGameData()
        {
            try
            {
                // Create save directory if it doesn't exist
                if (!Directory.Exists(SaveDirectory))
                {
                    Directory.CreateDirectory(SaveDirectory);
                }

                // Save recipe book
                string recipeBookJson = _recipeBook.SaveToJson();
                File.WriteAllText(Path.Combine(SaveDirectory, RecipeBookFile), recipeBookJson);

                // Save player data
                string playerJson = Newtonsoft.Json.JsonConvert.SerializeObject(_player, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(Path.Combine(SaveDirectory, PlayerFile), playerJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving game data: {ex.Message}");
            }
        }

        // Load game data from files
        private void LoadGameData()
        {
            try
            {
                // Load recipe book if file exists
                string recipeBookPath = Path.Combine(SaveDirectory, RecipeBookFile);
                if (File.Exists(recipeBookPath))
                {
                    string recipeBookJson = File.ReadAllText(recipeBookPath);
                    _recipeBook.LoadFromJson(recipeBookJson);
                }

                // Load player data if file exists
                string playerPath = Path.Combine(SaveDirectory, PlayerFile);
                if (File.Exists(playerPath))
                {
                    string playerJson = File.ReadAllText(playerPath);
                    _player = Newtonsoft.Json.JsonConvert.DeserializeObject<Player>(playerJson) ?? new Player();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game data: {ex.Message}");
                throw; // Re-throw to let the caller handle it
            }
        }
    }
}