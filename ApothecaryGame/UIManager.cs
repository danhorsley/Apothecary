using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using FontStashSharp;
using System;

namespace ApothecaryGame
{
    public class UIManager
    {
        private Desktop _desktop = null!;
        private Panel _mainPanel = null!;
        private Label _stateLabel = null!;
        private Label _goldLabel = null!;
        private Label _healthLabel = null!;

        private Game1 _game;
        private SpriteFontBase _defaultFont = null!;

        public UIManager(Game1 game)
        {
            _game = game;

            // Initialize Myra
            MyraEnvironment.Game = game;
        }

        public void LoadContent()
        {
            try
            {
                // Create a default FontSystem for Myra
                var fontSystem = new FontSystem();
                fontSystem.AddFont(TitleContainer.OpenStream("Content/Font.ttf"));
                _defaultFont = fontSystem.GetFont(16);

                // Create UI
                CreateUI();

                Console.WriteLine("UI Manager loaded content successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading UI content: {ex.Message}");
                // Create a minimal UI anyway
                CreateMinimalUI();
            }
        }

        private void CreateMinimalUI()
        {
            // Create main panel
            _mainPanel = new Panel
            {
                Width = 800,
                Height = 600
            };

            // Create desktop
            _desktop = new Desktop();
            _desktop.Root = _mainPanel;

            Console.WriteLine("Created minimal UI");
        }

        private void CreateUI()
        {
            // Create main panel
            _mainPanel = new Panel
            {
                Width = 800,
                Height = 600
            };

            // Create labels for player stats
            _stateLabel = new Label
            {
                Text = "Current State: Shop",
                Font = _defaultFont,
                Left = 10,
                Top = 10
            };

            _goldLabel = new Label
            {
                Text = "Gold: 100",
                Font = _defaultFont,
                Left = 10,
                Top = 40,
                TextColor = Color.Yellow
            };

            _healthLabel = new Label
            {
                Text = "Health: 100",
                Font = _defaultFont,
                Left = 10,
                Top = 70,
                TextColor = Color.Red
            };

            // Add labels to panel
            _mainPanel.Widgets.Add(_stateLabel);
            _mainPanel.Widgets.Add(_goldLabel);
            _mainPanel.Widgets.Add(_healthLabel);

            // Create desktop
            _desktop = new Desktop();
            _desktop.Root = _mainPanel;
        }

        public void UpdateUI(GameState currentState, Player player)
        {
            try
            {
                if (_stateLabel != null)
                    _stateLabel.Text = $"Current State: {currentState}";

                if (_goldLabel != null)
                    _goldLabel.Text = $"Gold: {player.Gold}";

                if (_healthLabel != null)
                    _healthLabel.Text = $"Health: {player.Health}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating UI: {ex.Message}");
            }
        }

        public void Draw()
        {
            try
            {
                _desktop.Render();
            }
            catch (Exception ex)
            {
                // Don't log every frame to avoid console spam
                // Console.WriteLine($"Error rendering UI: {ex.Message}");
            }
        }

        // Add shop UI
        public void CreateShopUI(Customer customer)
        {
            try
            {
                // Clear existing widgets except for the basic stats
                while (_mainPanel.Widgets.Count > 3)
                {_mainPanel.Widgets.RemoveAt(3);
                                    }

                                    // Customer info
                                    var customerLabel = new Label
                                    {
                                        Text = $"Customer: {customer.Name} ({customer.Type})",
                                        Font = _defaultFont,
                                        Left = 200,
                                        Top = 150
                                    };

                                    var needLabel = new Label
                                    {
                                        Text = $"Needs: {customer.Need} potion",
                                        Font = _defaultFont,
                                        Left = 200,
                                        Top = 180
                                    };

                                    var rewardLabel = new Label
                                    {
                                        Text = $"Reward: {customer.Reward} gold",
                                        Font = _defaultFont,
                                        Left = 200,
                                        Top = 210,
                                        TextColor = Color.Yellow
                                    };

                                    // Add to panel
                                    _mainPanel.Widgets.Add(customerLabel);
                                    _mainPanel.Widgets.Add(needLabel);
                                    _mainPanel.Widgets.Add(rewardLabel);

                                    // Buttons for state changes (fixing the Content property instead of Text)
                                    var mixButton = new Button
                                    {
                                        Content = new Label { Text = "Go to Mixing", Font = _defaultFont },
                                        Left = 200,
                                        Top = 300,
                                        Width = 150
                                    };
                                    mixButton.Click += (s, e) => _game.ChangeState(GameState.Mixing);

                                    var exploreButton = new Button
                                    {
                                        Content = new Label { Text = "Go Exploring", Font = _defaultFont },
                                        Left = 400,
                                        Top = 300,
                                        Width = 150
                                    };
                                    exploreButton.Click += (s, e) => _game.ChangeState(GameState.Exploration);

                                    _mainPanel.Widgets.Add(mixButton);
                                    _mainPanel.Widgets.Add(exploreButton);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error creating shop UI: {ex.Message}");
                                }
                            }

                            // Add potion mixing UI
                            public void CreateMixingUI(Player player, RecipeBook recipeBook)
                            {
                                try
                                {
                                    // Clear existing widgets except for the basic stats
                                    while (_mainPanel.Widgets.Count > 3)
                                    {
                                        _mainPanel.Widgets.RemoveAt(3);
                                    }

                                    // Title
                                    var titleLabel = new Label
                                    {
                                        Text = "Potion Mixing",
                                        Font = _defaultFont,
                                        Left = 350,
                                        Top = 120
                                    };

                                    _mainPanel.Widgets.Add(titleLabel);

                                    // Ingredient list
                                    var inventoryLabel = new Label
                                    {
                                        Text = "Your Ingredients:",
                                        Font = _defaultFont,
                                        Left = 200,
                                        Top = 160
                                    };

                                    _mainPanel.Widgets.Add(inventoryLabel);

                                    int y = 190;
                                    for (int i = 0; i < player.Inventory.Count; i++)
                                    {
                                        var ingredient = player.Inventory[i];
                                        var ingredientButton = new Button
                                        {
                                            Content = new Label { Text = $"{ingredient.Name} ({ingredient.Type}, Rarity: {ingredient.Rarity})", Font = _defaultFont },
                                            Left = 200,
                                            Top = y,
                                            Width = 250
                                        };

                                        int index = i; // Capture for lambda
                                        ingredientButton.Click += (s, e) => _game.SelectIngredient(index);

                                        _mainPanel.Widgets.Add(ingredientButton);
                                        y += 30;
                                    }

                                    // Recipe book
                                    var recipeLabel = new Label
                                    {
                                        Text = "Known Recipes:",
                                        Font = _defaultFont,
                                        Left = 500,
                                        Top = 160
                                    };

                                    _mainPanel.Widgets.Add(recipeLabel);

                                    y = 190;
                                    foreach (var recipe in recipeBook.KnownRecipes)
                                    {
                                        var recipeInfo = new Label
                                        {
                                            Text = $"{recipe.Effect} (Value: {recipe.Value})",
                                            Font = _defaultFont,
                                            Left = 500,
                                            Top = y
                                        };

                                        _mainPanel.Widgets.Add(recipeInfo);
                                        y += 30;
                                    }

                                    // Navigation buttons
                                    var shopButton = new Button
                                    {
                                        Content = new Label { Text = "Return to Shop", Font = _defaultFont },
                                        Left = 200,
                                        Top = 500,
                                        Width = 150
                                    };
                                    shopButton.Click += (s, e) => _game.ChangeState(GameState.Shop);

                                    var exploreButton = new Button
                                    {
                                        Content = new Label { Text = "Go Exploring", Font = _defaultFont },
                                        Left = 400,
                                        Top = 500,
                                        Width = 150
                                    };
                                    exploreButton.Click += (s, e) => _game.ChangeState(GameState.Exploration);

                                    _mainPanel.Widgets.Add(shopButton);
                                    _mainPanel.Widgets.Add(exploreButton);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error creating mixing UI: {ex.Message}");
                                }
                            }

                            // Create exploration UI
                            public void CreateExplorationUI(Forest forest, Player player)
                            {
                                try
                                {
                                    // Clear existing widgets except for the basic stats
                                    while (_mainPanel.Widgets.Count > 3)
                                    {
                                        _mainPanel.Widgets.RemoveAt(3);
                                    }

                                    // Title
                                    var titleLabel = new Label
                                    {
                                        Text = "Forest Exploration",
                                        Font = _defaultFont,
                                        Left = 350,
                                        Top = 120
                                    };

                                    _mainPanel.Widgets.Add(titleLabel);

                                    // Return button
                                    var returnButton = new Button
                                    {
                                        Content = new Label { Text = "Return to Shop", Font = _defaultFont },
                                        Left = 350,
                                        Top = 500,
                                        Width = 150
                                    };
                                    returnButton.Click += (s, e) => _game.ChangeState(GameState.Shop);

                                    _mainPanel.Widgets.Add(returnButton);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error creating exploration UI: {ex.Message}");
                                }
                            }
                        }
                    }