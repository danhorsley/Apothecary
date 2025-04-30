using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ApothecaryGame
{
    public class SpriteManager
    {
        private Game1 _game;
        private Dictionary<string, Texture2D> _sprites = new Dictionary<string, Texture2D>();

        // Constants for sprite sizes
        public const int IngredientSize = 32;
        public const int PotionSize = 32;
        public const int TileSize = 32;

        public SpriteManager(Game1 game)
        {
            _game = game;
        }

        public void LoadContent()
        {
            // Load all sprites
            _sprites["pixel"] = CreatePixelTexture();

            // Create placeholder textures for different game elements
            _sprites["herb"] = CreateColoredTexture(IngredientSize, Color.LightGreen);
            _sprites["crystal"] = CreateColoredTexture(IngredientSize, Color.LightBlue);
            _sprites["mushroom"] = CreateColoredTexture(IngredientSize, Color.LightPink);

            _sprites["potion_heal"] = CreatePotionTexture(Color.Red);
            _sprites["potion_poison"] = CreatePotionTexture(Color.Green);
            _sprites["potion_explosion"] = CreatePotionTexture(Color.Orange);
            _sprites["potion_strength"] = CreatePotionTexture(Color.Purple);
            _sprites["potion_invisibility"] = CreatePotionTexture(Color.LightBlue);
            _sprites["potion_unknown"] = CreatePotionTexture(Color.Gray);

            _sprites["tile_empty"] = CreateColoredTexture(TileSize, Color.ForestGreen);
            _sprites["tile_hazard"] = CreateColoredTexture(TileSize, Color.Red);
            _sprites["tile_unexplored"] = CreateColoredTexture(TileSize, Color.DarkGray);

            _sprites["player"] = CreatePlayerTexture();
            _sprites["cauldron"] = CreateCauldronTexture();

            _sprites["customer_adventurer"] = CreateCustomerTexture(Color.Brown);
            _sprites["customer_merchant"] = CreateCustomerTexture(Color.Gold);
            _sprites["customer_scholar"] = CreateCustomerTexture(Color.Blue);
        }

        public Texture2D GetSprite(string key)
        {
            if (_sprites.TryGetValue(key, out var texture))
            {
                return texture;
            }

            // Return pixel texture as fallback
            return _sprites["pixel"];
        }

        public Texture2D GetIngredientSprite(Ingredient ingredient)
        {
            string key = ingredient.Type.ToLower();
            return GetSprite(key);
        }

        public Texture2D GetPotionSprite(Potion potion)
        {
            string key = "potion_" + potion.Effect.ToLower();

            if (_sprites.ContainsKey(key))
            {
                return _sprites[key];
            }

            return _sprites["potion_unknown"];
        }

        public Texture2D GetTileSprite(Tile tile)
        {
            if (!tile.Explored)
            {
                return _sprites["tile_unexplored"];
            }

            switch (tile.Type)
            {
                case Tile.TileType.Empty:
                    return _sprites["tile_empty"];
                case Tile.TileType.Ingredient:
                    return tile.Ingredient != null ? GetIngredientSprite(tile.Ingredient) : _sprites["tile_empty"];
                case Tile.TileType.Hazard:
                    return _sprites["tile_hazard"];
                default:
                    return _sprites["tile_empty"];
            }
        }

        public Texture2D GetCustomerSprite(Customer customer)
        {
            string key = "customer_" + customer.Type.ToLower();

            if (_sprites.ContainsKey(key))
            {
                return _sprites[key];
            }

            return _sprites["customer_adventurer"];
        }

        // Create a 1x1 white pixel texture
        private Texture2D CreatePixelTexture()
        {
            Texture2D texture = new Texture2D(_game.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        // Create a colored square texture
        private Texture2D CreateColoredTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(_game.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }

            texture.SetData(data);
            return texture;
        }

        // Create a simple potion texture (colored circle)
        private Texture2D CreatePotionTexture(Color color)
        {
            int size = PotionSize;
            Texture2D texture = new Texture2D(_game.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            int radius = size / 2;
            int centerX = size / 2;
            int centerY = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;

                    // Calculate distance from center
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

                    if (distance <= radius)
                    {
                        // Inside circle - potion color
                        data[index] = color;
                    }
                    else
                    {
                        // Outside circle - transparent
                        data[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        // Create a simple player texture (blue triangle)
        private Texture2D CreatePlayerTexture()
        {
            int size = 32;
            Texture2D texture = new Texture2D(_game.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            // Fill with transparent
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }

            // Draw a triangle
            int centerX = size / 2;
            int top = 5;
            int bottom = size - 5;
            int left = 5;
            int right = size - 5;

            // Draw top to bottom left
            DrawLine(data, size, centerX, top, left, bottom, Color.Blue);

            // Draw bottom left to bottom right
            DrawLine(data, size, left, bottom, right, bottom, Color.Blue);

            // Draw bottom right to top
            DrawLine(data, size, right, bottom, centerX, top, Color.Blue);

            texture.SetData(data);
            return texture;
        }

        // Create a simple cauldron texture (black pot with bubbles)
        private Texture2D CreateCauldronTexture()
        {
            int size = 64;
            Texture2D texture = new Texture2D(_game.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            // Fill with transparent
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }

            int centerX = size / 2;
            int top = 10;
            int bottom = size - 10;

            // Draw cauldron outline
            for (int y = top; y < bottom; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;

                    // Calculate distance from center horizontally
                    float distanceX = Math.Abs(x - centerX);

                    // Calculate maximum width at this height (ellipse shape)
                    float heightRatio = (float)(y - top) / (bottom - top);
                    float widthAtHeight = (size / 2 - 5) * (1.0f - 0.3f * (1.0f - heightRatio));

                    if (distanceX <= widthAtHeight)
                    {
                        // Inside cauldron - dark gray
                        if (y == top || distanceX >= widthAtHeight - 1 || y >= bottom - 1)
                        {
                            // Edge - black
                            data[index] = Color.Black;
                        }
                        else
                        {
                            // Inside - dark gray with bubbles
                            float bubble = (float)Math.Sin(x * 0.5f + y * 0.3f) * 0.5f + 0.5f;

                            if (bubble > 0.8f && y < bottom - 10)
                            {
                                // Bubble - light green
                                data[index] = Color.LightGreen;
                            }
                            else
                            {
                                // Potion liquid - dark green
                                data[index] = Color.DarkGreen;
                            }
                        }
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        // Create a simple customer texture (colored person shape)
        private Texture2D CreateCustomerTexture(Color color)
        {
            int size = 48;
            Texture2D texture = new Texture2D(_game.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];

            // Fill with transparent
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }

            int centerX = size / 2;
            int headRadius = size / 6;
            int headCenterY = headRadius + 5;

            // Draw head (circle)
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;

                    // Calculate distance from head center
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, headCenterY));

                    if (distance <= headRadius)
                    {
                        // Inside head circle
                        data[index] = color;
                    }
                }
            }

            // Draw body (rectangle)
            int bodyTop = headCenterY + headRadius;
            int bodyBottom = size - 5;
            int bodyWidth = headRadius * 2;

            for (int y = bodyTop; y < bodyBottom; y++)
            {
                for (int x = centerX - bodyWidth / 2; x < centerX + bodyWidth / 2; x++)
                {
                    int index = y * size + x;
                    data[index] = color;
                }
            }

            texture.SetData(data);
            return texture;
        }

        // Helper method to draw a line in a texture
        private void DrawLine(Color[] data, int textureWidth, int x1, int y1, int x2, int y2, Color color)
        {
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                // Set pixel if within bounds
                if (x1 >= 0 && x1 < textureWidth && y1 >= 0 && y1 < textureWidth)
                {
                    int index = y1 * textureWidth + x1;
                    data[index] = color;
                }

                // Check if we've reached the end point
                if (x1 == x2 && y1 == y2)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }
    }
}