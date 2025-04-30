using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Myra;

namespace ApothecaryGame
{
    public enum GameState
    {
        Shop,
        Mixing,
        Exploration
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch = null!;

        // Game state
        private GameState _currentState;

        // Player and game data
        private Player _player;
        private RecipeBook _recipeBook;
        private Customer _currentCustomer;
        private Forest _forest;

        // UI Manager
        private UIManager _uiManager = null!;

        // Selected ingredients for mixing
        private int _selectedIngredient1 = -1;
        private int _selectedIngredient2 = -1;

        // Resources
        private SpriteFont _font = null!;
        private SpriteManager _spriteManager = null!;

        // Mouse and keyboard states
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;

        // Save file paths
        private const string SaveDirectory = "Saves";
        private const string RecipeBookFile = "recipebook.json";
        private const string PlayerFile = "player.json";

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            
            // Headless configuration
            _graphics.HardwareModeSwitch = false;
            _graphics.PreferHalfPixelOffset = true;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.IsFullScreen = false;
            _graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.PreferredDepthStencilFormat = DepthFormat.None;
            _graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
            _graphics.GraphicsProfile = GraphicsProfile.Reach;
            _graphics.PreferMultiSampling = false;
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set initial game state
            _currentState = GameState.Shop;

            // Initialize player and recipe book
            _player = new Player();
            _recipeBook = new RecipeBook();

            // Create save directory if it doesn't exist
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            // Load saved data if exists
            LoadGameData();

            // Add some starter ingredients to player inventory (for testing)
            if (_player.Inventory.Count == 0)
            {
                _player.AddIngredient(new Ingredient { Name = "Red Herb", Type = "Herb", Rarity = 1 });
                _player.AddIngredient(new Ingredient { Name = "Blue Crystal", Type = "Crystal", Rarity = 2 });
                _player.AddIngredient(new Ingredient { Name = "Spotted Cap", Type = "Mushroom", Rarity = 1 });
                _player.AddIngredient(new Ingredient { Name = "Green Herb", Type = "Herb", Rarity = 1 });
            }

            // Generate a customer
            _currentCustomer = Customer.CreateRandom();

            // Generate forest
            _forest = new Forest();
        }

        protected override void Initialize()
        {
            // Set window size
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load a font for text display
            _font = Content.Load<SpriteFont>("Font");

            // Initialize sprite manager and load all sprites
            _spriteManager = new SpriteManager(this);
            _spriteManager.LoadContent();

            // Initialize UI manager
            _uiManager = new UIManager(this);
            _uiManager.LoadContent(); // Add this to load fonts

            // Set up UI for current state
            UpdateUIForCurrentState();
        }

        public void ChangeState(GameState newState)
        {
            // Save current state data if needed
            SaveGameData();

            // Set new state
            _currentState = newState;

            // Update UI for new state
            UpdateUIForCurrentState();

            // Additional state transition logic if needed
            if (newState == GameState.Exploration)
            {
                // Generate new forest when entering exploration mode
                _forest = new Forest();
            }
            else if (newState == GameState.Shop)
            {
                // Generate new customer when entering shop
                _currentCustomer = Customer.CreateRandom();
            }
        }

        private void UpdateUIForCurrentState()
        {
            switch (_currentState)
            {
                case GameState.Shop:
                    _uiManager.CreateShopUI(_currentCustomer);
                    break;
                case GameState.Mixing:
                    _uiManager.CreateMixingUI(_player, _recipeBook);
                    break;
                case GameState.Exploration:
                    _uiManager.CreateExplorationUI(_forest, _player);
                    break;
            }

            _uiManager.UpdateUI(_currentState, _player);
        }

        protected override void Update(GameTime gameTime)
        {
            // Update input states
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || _currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // Simple state switching for testing (using number keys)
            if (_currentKeyboardState.IsKeyDown(Keys.D1) && !_previousKeyboardState.IsKeyDown(Keys.D1))
                ChangeState(GameState.Shop);

            if (_currentKeyboardState.IsKeyDown(Keys.D2) && !_previousKeyboardState.IsKeyDown(Keys.D2))
                ChangeState(GameState.Mixing);

            if (_currentKeyboardState.IsKeyDown(Keys.D3) && !_previousKeyboardState.IsKeyDown(Keys.D3))
                ChangeState(GameState.Exploration);

            // Update based on current state
            switch (_currentState)
            {
                case GameState.Shop:
                    UpdateShop(gameTime);
                    break;
                case GameState.Mixing:
                    UpdateMixing(gameTime);
                    break;
                case GameState.Exploration:
                    UpdateExploration(gameTime);
                    break;
            }

            // Update UI
            _uiManager.UpdateUI(_currentState, _player);

            base.Update(gameTime);
        }

        private void UpdateShop(GameTime gameTime)
        {
            // Process selling potions - example using keyboard for now
            // S key to sell first potion to current customer
            if (_currentKeyboardState.IsKeyDown(Keys.S) && !_previousKeyboardState.IsKeyDown(Keys.S))
            {
                if (_player.Potions.Count > 0)
                {
                    SellPotion(0);
                }
            }

            // N key to get a new customer
            if (_currentKeyboardState.IsKeyDown(Keys.N) && !_previousKeyboardState.IsKeyDown(Keys.N))
            {
                _currentCustomer = Customer.CreateRandom();
                UpdateUIForCurrentState();
            }

            // B key to buy a random ingredient
            if (_currentKeyboardState.IsKeyDown(Keys.B) && !_previousKeyboardState.IsKeyDown(Keys.B))
            {
                BuyIngredient();
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

                    // Remove sold potion
                    _player.RemovePotion(potionIndex);

                    // Get a new customer
                    _currentCustomer = Customer.CreateRandom();
                    UpdateUIForCurrentState();
                }
                else
                {
                    // Failed sale
                    _player.Reputation -= 10;

                    // Remove used potion
                    _player.RemovePotion(potionIndex);
                }
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

                System.Random random = new System.Random();
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

                    // Update UI
                    UpdateUIForCurrentState();
                }
            }
        }

        private void UpdateMixing(GameTime gameTime)
        {
            // Test mixing with M key
            if (_currentKeyboardState.IsKeyDown(Keys.M) && !_previousKeyboardState.IsKeyDown(Keys.M))
            {
                MixPotion();
            }
        }

        public void SelectIngredient(int index)
        {
            if (index >= 0 && index < _player.Inventory.Count)
            {
                if (_selectedIngredient1 == -1)
                {
                    _selectedIngredient1 = index;
                }
                else if (_selectedIngredient2 == -1)
                {
                    // Don't select the same ingredient twice
                    if (index != _selectedIngredient1)
                    {
                        _selectedIngredient2 = index;

                        // Automatically mix when both ingredients are selected
                        MixPotion();
                    }
                }
                else
                {
                    // Reset selection
                    _selectedIngredient1 = index;
                    _selectedIngredient2 = -1;
                }
            }
        }

        private void MixPotion()
        {
            // If we have two selected ingredients, mix them
            if (_selectedIngredient1 >= 0 && _selectedIngredient2 >= 0)
            {
                // Make sure indices are still valid
                if (_selectedIngredient1 < _player.Inventory.Count && _selectedIngredient2 < _player.Inventory.Count)
                {
                    // Get the ingredients (make copies since we'll be removing them)
                    var ingredient1 = _player.Inventory[_selectedIngredient1];
                    var ingredient2 = _player.Inventory[_selectedIngredient2];

                    // Create the potion
                    var potion = new Potion(ingredient1, ingredient2);

                    // Add to recipe book and player's potions
                    _recipeBook.AddRecipe(potion);
                    _player.AddPotion(potion);

                    // Remove the used ingredients (be careful with indices)
                    // Remove the higher index first to avoid shifting problems
                    if (_selectedIngredient1 > _selectedIngredient2)
                    {
                        _player.RemoveIngredient(_selectedIngredient1);
                        _player.RemoveIngredient(_selectedIngredient2);
                    }
                    else
                    {
                        _player.RemoveIngredient(_selectedIngredient2);
                        _player.RemoveIngredient(_selectedIngredient1);
                    }

                    // Reset selected ingredients
                    _selectedIngredient1 = -1;
                    _selectedIngredient2 = -1;

                    // Update UI
                    UpdateUIForCurrentState();
                }
            }
            // If we have two ingredients but not selected, use the first two
            else if (_player.Inventory.Count >= 2)
            {
                var potion = new Potion(_player.Inventory[0], _player.Inventory[1]);
                _recipeBook.AddRecipe(potion);
                _player.AddPotion(potion);

                // Remove the used ingredients
                _player.RemoveIngredient(0);
                _player.RemoveIngredient(0); // Index 0 again since the first one was already removed

                // Update UI
                UpdateUIForCurrentState();
            }
        }

        private void UpdateExploration(GameTime gameTime)
        {
            // Simple exploration using keyboard for now
            // WASD to move in the grid
            bool moved = false;

            // Player position
            int playerX = 0;
            int playerY = 0;

            // Movement controls
            if (_currentKeyboardState.IsKeyDown(Keys.W) && !_previousKeyboardState.IsKeyDown(Keys.W))
            {
                playerY = Math.Max(0, playerY - 1);
                moved = true;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.S) && !_previousKeyboardState.IsKeyDown(Keys.S))
            {
                playerY = Math.Min(_forest.Height - 1, playerY + 1);
                moved = true;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.A) && !_previousKeyboardState.IsKeyDown(Keys.A))
            {
                playerX = Math.Max(0, playerX - 1);
                moved = true;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.D) && !_previousKeyboardState.IsKeyDown(Keys.D))
            {
                playerX = Math.Min(_forest.Width - 1, playerX + 1);
                moved = true;
            }

            // Process tile if player moved
            if (moved)
            {
                var tile = _forest.Grid[playerX, playerY];

                // Mark as explored
                tile.Explored = true;

                // Process tile based on type
                if (tile.Type == Tile.TileType.Ingredient && tile.Ingredient != null)
                {
                    // Collect ingredient if inventory has space
                    if (_player.CanAddIngredient())
                    {
                        _player.AddIngredient(tile.Ingredient);

                        // Clear the ingredient from the tile
                        tile.Ingredient = null;
                        tile.Type = Tile.TileType.Empty;
                    }
                }
                else if (tile.Type == Tile.TileType.Hazard)
                {
                    // Take damage from hazard
                    int damage = tile.GetHazardDamage();
                    _player.Health -= damage;

                    // Check if player died
                    if (_player.Health <= 0)
                    {
                        // Game over - return to shop with consequences
                        _player.Health = 100; // Restore health
                        _player.Gold = Math.Max(0, _player.Gold - 50); // Lose some gold
                        _player.Inventory.Clear(); // Lose all ingredients

                        // Return to shop
                        ChangeState(GameState.Shop);
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Draw state-specific UI
            switch (_currentState)
            {
                case GameState.Shop:
                    DrawShop();
                    break;
                case GameState.Mixing:
                    DrawMixing();
                    break;
                case GameState.Exploration:
                    DrawExploration();
                    break;
            }

            _spriteBatch.End();

            // Draw Myra UI
            _uiManager.Draw();

            base.Draw(gameTime);
        }

        private void DrawShop()
        {
            // Draw customer
            int customerSize = 64;
            int centerX = 400;
            int customerY = 180;

            _spriteBatch.Draw(_spriteManager.GetCustomerSprite(_currentCustomer),
                new Rectangle(centerX - customerSize/2, customerY, customerSize, customerSize),
                Color.White);

            // Draw customer info
            int infoX = centerX - 100;
            int infoY = customerY + customerSize + 20;

            _spriteBatch.DrawString(_font, $"{_currentCustomer.Name} ({_currentCustomer.Type})",
                new Vector2(infoX, infoY), Color.White);

            _spriteBatch.DrawString(_font, $"Wants: {_currentCustomer.Need} potion",
                new Vector2(infoX, infoY + 25), Color.White);

            _spriteBatch.DrawString(_font, $"Reward: {_currentCustomer.Reward} gold",
                new Vector2(infoX, infoY + 50), Color.Yellow);

            // Draw player's potions (for selling)
            int potionStartX = 200;
            int potionY = 350;

            _spriteBatch.DrawString(_font, "Your Potions to Sell:", new Vector2(potionStartX, potionY - 30), Color.White);

            if (_player.Potions.Count == 0)
            {
                _spriteBatch.DrawString(_font, "You don't have any potions to sell.",
                    new Vector2(potionStartX, potionY), Color.Gray);
            }
            else
            {
                for (int i = 0; i < Math.Min(_player.Potions.Count, 10); i++)
                {
                    var potion = _player.Potions[i];
                    Texture2D sprite = _spriteManager.GetPotionSprite(potion);

                    // Draw potion
                    _spriteBatch.Draw(sprite,
                        new Rectangle(potionStartX + i * 50, potionY, SpriteManager.PotionSize, SpriteManager.PotionSize),
                        Color.White);

                    // Draw effect below
                    _spriteBatch.DrawString(_font, potion.Effect,
                        new Vector2(potionStartX + i * 50, potionY + SpriteManager.PotionSize + 5),
                        potion.Effect == _currentCustomer.Need ? Color.LightGreen : Color.White);
                }
            }

            // Draw shop inventory (available ingredients to buy)
            int shopX = 50;
            int shopY = 150;

            _spriteBatch.DrawString(_font, "Shop Inventory (Press B to buy random ingredient):", 
                new Vector2(shopX, shopY - 30), Color.White);

            // Draw herb
            _spriteBatch.Draw(_spriteManager.GetSprite("herb"),
                new Rectangle(shopX, shopY, SpriteManager.IngredientSize, SpriteManager.IngredientSize),
                Color.White);
            _spriteBatch.DrawString(_font, "Herbs - 20 gold",
                new Vector2(shopX + SpriteManager.IngredientSize + 10, shopY + 8),
                _player.Gold >= 20 ? Color.White : Color.Gray);

            // Draw crystal
            _spriteBatch.Draw(_spriteManager.GetSprite("crystal"),
                new Rectangle(shopX, shopY + 40, SpriteManager.IngredientSize, SpriteManager.IngredientSize),
                Color.White);
            _spriteBatch.DrawString(_font, "Crystals - 30 gold",
                new Vector2(shopX + SpriteManager.IngredientSize + 10, shopY + 40 + 8),
                _player.Gold >= 30 ? Color.White : Color.Gray);

            // Draw mushroom
            _spriteBatch.Draw(_spriteManager.GetSprite("mushroom"),
                new Rectangle(shopX, shopY + 80, SpriteManager.IngredientSize, SpriteManager.IngredientSize),
                Color.White);
            _spriteBatch.DrawString(_font, "Mushrooms - 25 gold",
                new Vector2(shopX + SpriteManager.IngredientSize + 10, shopY + 80 + 8),
                _player.Gold >= 25 ? Color.White : Color.Gray);

            // Draw instructions
            _spriteBatch.DrawString(_font, "Press S to sell first potion",
                new Vector2(200, 450), Color.LightGray);

            _spriteBatch.DrawString(_font, "Press N for a new customer",
                new Vector2(200, 470), Color.LightGray);

            _spriteBatch.DrawString(_font, "Press B to buy a random ingredient",
                new Vector2(200, 490), Color.LightGray);
        }

        private void DrawMixing()
        {
            // Draw cauldron in the center
            int cauldronSize = 64;
            int centerX = 400;
            int centerY = 250;

            _spriteBatch.Draw(_spriteManager.GetSprite("cauldron"),
                new Rectangle(centerX - cauldronSize/2, centerY - cauldronSize/2, cauldronSize, cauldronSize),
                Color.White);

            // Draw selected ingredients (if any)
            if (_selectedIngredient1 >= 0 && _selectedIngredient1 < _player.Inventory.Count)
            {
                var ingredient = _player.Inventory[_selectedIngredient1];
                Texture2D sprite = _spriteManager.GetIngredientSprite(ingredient);

                // Draw to the left of the cauldron
                _spriteBatch.Draw(sprite,
                    new Rectangle(centerX - cauldronSize/2 - 40, centerY, SpriteManager.IngredientSize, SpriteManager.IngredientSize),
                    Color.White);

                // Draw name below
                _spriteBatch.DrawString(_font, ingredient.Name,
                    new Vector2(centerX - cauldronSize/2 - 40 - ingredient.Name.Length * 3, centerY + SpriteManager.IngredientSize + 5),
                    Color.White);
            }

            if (_selectedIngredient2 >= 0 && _selectedIngredient2 < _player.Inventory.Count)
            {
                var ingredient = _player.Inventory[_selectedIngredient2];
                Texture2D sprite = _spriteManager.GetIngredientSprite(ingredient);

                // Draw to the right of the cauldron
                _spriteBatch.Draw(sprite,
                    new Rectangle(centerX + cauldronSize/2 + 10, centerY, SpriteManager.IngredientSize, SpriteManager.IngredientSize),
                    Color.White);

                // Draw name below
                _spriteBatch.DrawString(_font, ingredient.Name,
                    new Vector2(centerX + cauldronSize/2 + 10, centerY + SpriteManager.IngredientSize + 5),
                    Color.White);
            }

            // Draw inventory on the left side
            int inventoryX = 50;
            int inventoryY = 150;

            _spriteBatch.DrawString(_font, "Ingredients:", new Vector2(inventoryX, inventoryY - 30), Color.White);

            for (int i = 0; i < _player.Inventory.Count; i++)
            {
                var ingredient = _player.Inventory[i];
                Texture2D sprite = _spriteManager.GetIngredientSprite(ingredient);

                // Highlight if selected
                Color color = (i == _selectedIngredient1 || i == _selectedIngredient2) ? Color.Yellow : Color.White;

                // Draw ingredient sprite
                _spriteBatch.Draw(sprite,
                    new Rectangle(inventoryX, inventoryY + i * 40, SpriteManager.IngredientSize, SpriteManager.IngredientSize),
                    color);

                // Draw ingredient info
                _spriteBatch.DrawString(_font, $"{ingredient.Name} ({ingredient.Type}, Rarity: {ingredient.Rarity})",
                    new Vector2(inventoryX + SpriteManager.IngredientSize + 10, inventoryY + i * 40 + 8),
                    color);
            }

            // Draw known potions on the right side
            int potionsX = 550;
            int potionsY = 150;

            _spriteBatch.DrawString(_font, "Recipe Book:", new Vector2(potionsX, potionsY - 30), Color.White);

            for (int i = 0; i < _recipeBook.KnownRecipes.Count; i++)
            {
                var potion = _recipeBook.KnownRecipes[i];
                Texture2D sprite = _spriteManager.GetPotionSprite(potion);

                // Draw potion sprite
                _spriteBatch.Draw(sprite,
                    new Rectangle(potionsX, potionsY + i * 40, SpriteManager.PotionSize, SpriteManager.PotionSize),
                    Color.White);

                // Draw potion info
                _spriteBatch.DrawString(_font, $"{potion.Effect} (Value: {potion.Value})",
                    new Vector2(potionsX + SpriteManager.PotionSize + 10, potionsY + i * 40 + 8),
                    Color.White);
            }

            // Draw created potions at the bottom
            int createdX = 200;
            int createdY = 400;

            _spriteBatch.DrawString(_font, "Your Potions:", new Vector2(createdX, createdY - 30), Color.White);

            for (int i = 0; i < Math.Min(_player.Potions.Count, 8); i++)
            {
                var potion = _player.Potions[i];
                Texture2D sprite = _spriteManager.GetPotionSprite(potion);

                // Draw in a row
                _spriteBatch.Draw(sprite,
                    new Rectangle(createdX + i * 50, createdY, SpriteManager.PotionSize, SpriteManager.PotionSize),
                    Color.White);
            }

            // Draw instructions
            _spriteBatch.DrawString(_font, "Click on ingredients to select them for mixing.",
                new Vector2(200, 500), Color.LightGray);

            _spriteBatch.DrawString(_font, "Press M to mix selected ingredients.",
                new Vector2(200, 520), Color.LightGray);
        }

        private void DrawExploration()
        {
            // Draw the forest grid
            int tileSize = SpriteManager.TileSize;
            int gridOffsetX = 200;
            int gridOffsetY = 150;

            // Draw grid background
            _spriteBatch.Draw(_spriteManager.GetSprite("pixel"), 
                new Rectangle(gridOffsetX, gridOffsetY, _forest.Width * tileSize, _forest.Height * tileSize), 
                Color.DarkOliveGreen);

            // Draw tiles
            for (int x = 0; x < _forest.Width; x++)
            {
                for (int y = 0; y < _forest.Height; y++)
                {
                    var tile = _forest.Grid[x, y];

                    // Get the appropriate sprite for this tile
                    Texture2D tileSprite = _spriteManager.GetTileSprite(tile);

                    // Draw tile
                    _spriteBatch.Draw(tileSprite, 
                        new Rectangle(gridOffsetX + x * tileSize, gridOffsetY + y * tileSize, tileSize, tileSize), 
                        Color.White);

                    // If the tile contains an ingredient and is explored, draw the ingredient
                    if (tile.Explored && tile.Type == Tile.TileType.Ingredient && tile.Ingredient != null)
                    {
                        Texture2D ingredientSprite = _spriteManager.GetIngredientSprite(tile.Ingredient);
                        _spriteBatch.Draw(ingredientSprite,
                            new Rectangle(gridOffsetX + x * tileSize, gridOffsetY + y * tileSize, tileSize, tileSize),
                            Color.White);
                    }

                    // Draw grid lines
                    _spriteBatch.Draw(_spriteManager.GetSprite("pixel"), 
                        new Rectangle(gridOffsetX + x * tileSize, gridOffsetY + y * tileSize, tileSize, 1), 
                        Color.Black * 0.5f);
                    _spriteBatch.Draw(_spriteManager.GetSprite("pixel"), 
                        new Rectangle(gridOffsetX + x * tileSize, gridOffsetY + y * tileSize, 1, tileSize), 
                        Color.Black * 0.5f);
                }
            }

            // Draw grid bottom and right borders
            _spriteBatch.Draw(_spriteManager.GetSprite("pixel"), 
                new Rectangle(gridOffsetX, gridOffsetY + _forest.Height * tileSize, _forest.Width * tileSize, 1), 
                Color.Black);
            _spriteBatch.Draw(_spriteManager.GetSprite("pixel"), 
                new Rectangle(gridOffsetX + _forest.Width * tileSize, gridOffsetY, 1, _forest.Height * tileSize), 
                Color.Black);

            // Draw player (always in the center for now)
            int playerX = _forest.Width / 2;
            int playerY = _forest.Height / 2;
            _spriteBatch.Draw(_spriteManager.GetSprite("player"),
                new Rectangle(gridOffsetX + playerX * tileSize, gridOffsetY + playerY * tileSize, tileSize, tileSize),
                Color.White);

            // Draw legend
            int legendX = gridOffsetX + _forest.Width * tileSize + 20;
            int legendY = gridOffsetY;

            _spriteBatch.DrawString(_font, "Legend:", new Vector2(legendX, legendY), Color.White);
            legendY += 30;

            // Unexplored tile
            _spriteBatch.Draw(_spriteManager.GetSprite("tile_unexplored"),
                new Rectangle(legendX, legendY, tileSize, tileSize), Color.White);
            _spriteBatch.DrawString(_font, "Unexplored", new Vector2(legendX + tileSize + 10, legendY + 8), Color.White);
            legendY += tileSize + 10;

            // Empty tile
            _spriteBatch.Draw(_spriteManager.GetSprite("tile_empty"),
                new Rectangle(legendX, legendY, tileSize, tileSize), Color.White);
            _spriteBatch.DrawString(_font, "Empty", new Vector2(legendX + tileSize + 10, legendY + 8), Color.White);
            legendY += tileSize + 10;

            // Ingredient tiles
            _spriteBatch.Draw(_spriteManager.GetSprite("herb"),
                new Rectangle(legendX, legendY, tileSize, tileSize), Color.White);
            _spriteBatch.DrawString(_font, "Herb", new Vector2(legendX + tileSize + 10, legendY + 8), Color.White);
            legendY += tileSize + 10;

            _spriteBatch.Draw(_spriteManager.GetSprite("crystal"),
                new Rectangle(legendX, legendY, tileSize, tileSize), Color.White);
            _spriteBatch.DrawString(_font, "Crystal", new Vector2(legendX + tileSize + 10, legendY + 8), Color.White);
            legendY += tileSize + 10;

            _spriteBatch.Draw(_spriteManager.GetSprite("mushroom"),
                new Rectangle(legendX, legendY, tileSize, tileSize), Color.White);
            _spriteBatch.DrawString(_font, "Mushroom", new Vector2(legendX + tileSize + 10, legendY + 8), Color.White);
            legendY += tileSize + 10;

            // Hazard tile
            _spriteBatch.Draw(_spriteManager.GetSprite("tile_hazard"),
                new Rectangle(legendX, legendY, tileSize, tileSize), Color.White);
            _spriteBatch.DrawString(_font, "Hazard", new Vector2(legendX + tileSize + 10, legendY + 8), Color.White);
            legendY += tileSize + 10;

            // Player
            _spriteBatch.Draw(_spriteManager.GetSprite("player"),
                new Rectangle(legendX, legendY, tileSize, tileSize), Color.White);
            _spriteBatch.DrawString(_font, "You", new Vector2(legendX + tileSize + 10, legendY + 8), Color.White);
        }

        // SpriteManager now handles all texture creation and management

        // Save game data to files
        private void SaveGameData()
        {
            try
            {
                // Save recipe book
                string recipeBookJson = _recipeBook.SaveToJson();
                File.WriteAllText(Path.Combine(SaveDirectory, RecipeBookFile), recipeBookJson);

                // Save player data
                string playerJson = JsonConvert.SerializeObject(_player, Formatting.Indented);
                File.WriteAllText(Path.Combine(SaveDirectory, PlayerFile), playerJson);
            }
            catch (Exception ex)
            {
                // Log error but continue game
                System.Diagnostics.Debug.WriteLine($"Error saving game data: {ex.Message}");
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
                    _player = JsonConvert.DeserializeObject<Player>(playerJson) ?? new Player();
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with new game
                System.Diagnostics.Debug.WriteLine($"Error loading game data: {ex.Message}");

                // Initialize new data
                _player = new Player();
                _recipeBook = new RecipeBook();
            }
        }
    }
}