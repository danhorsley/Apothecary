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
        // This is the fixed UpdateExploration method for Game1.cs

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
                        return;
                }

                if (_player.Gold >= cost)
                {
                    _player.Gold -= cost;
                    _player.AddIngredient(new Ingredient { Name = name, Type = type, Rarity = rarity });
                }
            }
        }
    }
}