using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApothecaryGame
{
    // Player class to track inventory, gold, health, etc.
    public class Player
    {
        public List<Ingredient> Inventory { get; private set; } = new List<Ingredient>();
        public List<Potion> Potions { get; private set; } = new List<Potion>();
        public int Gold { get; set; } = 100; // Starting gold
        public int Health { get; set; } = 100; // Max health
        public int Reputation { get; set; } = 50; // Shop reputation 
        public int InventorySize { get; set; } = 10; // Maximum inventory slots

        public void AddIngredient(Ingredient ingredient)
        {
            if (Inventory.Count < InventorySize)
            {
                Inventory.Add(ingredient);
            }
        }

        public void RemoveIngredient(int index)
        {
            if (index >= 0 && index < Inventory.Count)
            {
                Inventory.RemoveAt(index);
            }
        }

        public void AddPotion(Potion potion)
        {
            Potions.Add(potion);
        }

        public void RemovePotion(int index)
        {
            if (index >= 0 && index < Potions.Count)
            {
                Potions.RemoveAt(index);
            }
        }

        public bool CanAddIngredient()
        {
            return Inventory.Count < InventorySize;
        }
    }

    // Ingredient class with name, type and rarity
    public class Ingredient
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Rarity { get; set; }
    }

    // Potion class with effects and value
    public class Potion
    {
        public string Effect { get; set; }
        public int Value { get; set; }

        // Default constructor for JSON deserialization
        public Potion() { }

        // Constructor to create potion from ingredients
        public Potion(Ingredient a, Ingredient b)
        {
            GenerateEffect(a, b);
            CalculateValue(a, b);
        }

        private void GenerateEffect(Ingredient a, Ingredient b)
        {
            // Simple rule-based effect generation
            // This can be expanded with more complex rules
            if (a.Type == "Herb" && b.Type == "Crystal") Effect = "Heal";
            else if (a.Type == "Herb" && b.Type == "Herb") Effect = "Poison";
            else if (a.Type == "Crystal" && b.Type == "Crystal") Effect = "Explosion";
            else if (a.Type == "Mushroom" && b.Type == "Herb") Effect = "Strength";
            else if (a.Type == "Mushroom" && b.Type == "Crystal") Effect = "Invisibility";
            else Effect = "Unknown"; // Default effect
        }

        private void CalculateValue(Ingredient a, Ingredient b)
        {
            // Base value from rarity
            Value = (a.Rarity + b.Rarity) * 10;

            // Adjust based on effect
            switch (Effect)
            {
                case "Heal":
                    Value += 20;
                    break;
                case "Poison":
                    Value += 15;
                    break;
                case "Explosion":
                    Value += 30;
                    break;
                case "Strength":
                    Value += 25;
                    break;
                case "Invisibility":
                    Value += 40;
                    break;
                default:
                    Value += 5;
                    break;
            }
        }
    }

    // Recipe book to track discovered potion recipes
    public class RecipeBook
    {
        public List<Potion> KnownRecipes { get; set; } = new List<Potion>();

        public void AddRecipe(Potion potion)
        {
            // Only add if not already known
            if (!KnownRecipes.Exists(p => p.Effect == potion.Effect))
            {
                KnownRecipes.Add(potion);
            }
        }

        // Save recipe book to JSON
        public string SaveToJson()
        {
            return JsonConvert.SerializeObject(KnownRecipes, Formatting.Indented);
        }

        // Load recipe book from JSON
        public void LoadFromJson(string json)
        {
            KnownRecipes = JsonConvert.DeserializeObject<List<Potion>>(json) ?? new List<Potion>();
        }
    }

    // Customer class for shop interactions
    public class Customer
    {
        public string Name { get; set; }
        public string Type { get; set; } // e.g., Adventurer, Merchant, Scholar
        public string Need { get; set; } // Required potion effect
        public int Reward { get; set; } // Gold reward for correct potion

        private static Random random = new Random();

        // Create a random customer
        public static Customer CreateRandom()
        {
            string[] types = { "Adventurer", "Merchant", "Scholar" };
            string type = types[random.Next(types.Length)];

            string need;
            int reward;

            // Different customer types have different needs and rewards
            switch (type)
            {
                case "Adventurer":
                    string[] adventurerNeeds = { "Heal", "Strength", "Invisibility" };
                    need = adventurerNeeds[random.Next(adventurerNeeds.Length)];
                    reward = need == "Heal" ? 50 : (need == "Strength" ? 60 : 80);
                    break;
                case "Merchant":
                    string[] merchantNeeds = { "Heal", "Poison" };
                    need = merchantNeeds[random.Next(merchantNeeds.Length)];
                    reward = need == "Heal" ? 40 : 70;
                    break;
                case "Scholar":
                    string[] scholarNeeds = { "Explosion", "Invisibility", "Unknown" };
                    need = scholarNeeds[random.Next(scholarNeeds.Length)];
                    reward = need == "Unknown" ? 100 : (need == "Explosion" ? 90 : 70);
                    break;
                default:
                    need = "Heal";
                    reward = 50;
                    break;
            }

            return new Customer
            {
                Name = GenerateRandomName(),
                Type = type,
                Need = need,
                Reward = reward
            };
        }

        private static string GenerateRandomName()
        {
            string[] firstNames = { "Thrond", "Elara", "Gorn", "Sybil", "Markus", "Brianne", "Dorian", "Faye" };
            string[] lastNames = { "Stonefist", "Brightwood", "Darkmoon", "Silverblade", "Frostbeard", "Swiftfoot" };

            return $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}";
        }
    }

    // Forest tile for exploration mode
    public class Tile
    {
        public enum TileType
        {
            Empty,
            Ingredient,
            Hazard
        }

        public enum HazardType
        {
            None,
            Thorns,
            WeakEnemy,
            Trap
        }

        public TileType Type { get; set; } = TileType.Empty;
        public HazardType Hazard { get; set; } = HazardType.None;
        public Ingredient Ingredient { get; set; }
        public bool Explored { get; set; } = false;

        public int GetHazardDamage()
        {
            switch (Hazard)
            {
                case HazardType.Thorns:
                    return 10;
                case HazardType.WeakEnemy:
                    return 20;
                case HazardType.Trap:
                    return 5;
                default:
                    return 0;
            }
        }
    }

    // Forest class for exploration mode
    public class Forest
    {
        public Tile[,] Grid { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private static Random random = new Random();

        public Forest(int width = 10, int height = 10)
        {
            Width = width;
            Height = height;
            Grid = new Tile[width, height];

            GenerateForest();
        }

        private void GenerateForest()
        {
            // Initialize all tiles
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Grid[x, y] = new Tile();

                    // 70% chance for ingredient
                    if (random.NextDouble() < 0.7)
                    {
                        Grid[x, y].Type = Tile.TileType.Ingredient;
                        Grid[x, y].Ingredient = GenerateRandomIngredient();
                    }
                    // 20% chance for hazard
                    else if (random.NextDouble() < 0.2)
                    {
                        Grid[x, y].Type = Tile.TileType.Hazard;
                        Grid[x, y].Hazard = GenerateRandomHazard();
                    }
                    // Otherwise empty
                }
            }
        }

        private Ingredient GenerateRandomIngredient()
        {
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

            return new Ingredient
            {
                Name = name,
                Type = type,
                Rarity = rarity
            };
        }

        private Tile.HazardType GenerateRandomHazard()
        {
            Tile.HazardType[] hazards = {
                Tile.HazardType.Thorns,
                Tile.HazardType.WeakEnemy,
                Tile.HazardType.Trap
            };

            return hazards[random.Next(hazards.Length)];
        }
    }
}