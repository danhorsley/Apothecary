using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;

namespace ApothecaryGame
{
    /// <summary>
    /// Helper class for creating headless-friendly graphics devices
    /// </summary>
    public static class HeadlessGraphicsHelper
    {
        /// <summary>
        /// Attempt to create a software-based GraphicsDevice that works in headless environments
        /// </summary>
        /// <param name="game">The Game instance</param>
        /// <param name="graphicsDeviceManager">The GraphicsDeviceManager to configure</param>
        public static void ConfigureForHeadless(Game game, GraphicsDeviceManager graphicsDeviceManager)
        {
            try
            {
                Console.WriteLine("Configuring for headless environment...");

                // Basic headless-friendly settings
                graphicsDeviceManager.IsFullScreen = false;
                graphicsDeviceManager.PreferredBackBufferWidth = 800;
                graphicsDeviceManager.PreferredBackBufferHeight = 600;
                graphicsDeviceManager.GraphicsProfile = GraphicsProfile.Reach;
                graphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                graphicsDeviceManager.PreferMultiSampling = false;
                graphicsDeviceManager.PreferredBackBufferFormat = SurfaceFormat.Color;
                graphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.None;
                graphicsDeviceManager.HardwareModeSwitch = false;

                // Try to make window borderless
                game.Window.AllowUserResizing = false;

                try
                {
                    game.Window.IsBorderless = true;
                }
                catch
                {
                    // Ignore if not supported
                }

                // Additional environment variables that might help
                Environment.SetEnvironmentVariable("SDL_VIDEO_GL_DRIVER", "libGL.so.1");
                Environment.SetEnvironmentVariable("MGFX_FORCE_SOFTRASTERIZER", "1");

                // Try to find and access protected/private fields via reflection if needed
                // This is a workaround approach when normal configuration fails
                try
                {
                    var managerType = graphicsDeviceManager.GetType();

                    // Try to set up software-only approach
                    var useReferenceFlag = managerType.GetField("_useReferenceDevice", 
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if (useReferenceFlag != null)
                    {
                        useReferenceFlag.SetValue(graphicsDeviceManager, true);
                        Console.WriteLine("Enabled reference device mode");
                    }

                    // Force a specific adapter
                    var adapterIndexField = managerType.GetField("_preferredBackBufferWidth", 
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if (adapterIndexField != null)
                    {
                        adapterIndexField.SetValue(graphicsDeviceManager, 0);
                        Console.WriteLine("Set adapter index to 0");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reflection configuration failed: {ex.Message}");
                }

                Console.WriteLine("Headless configuration complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in headless configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a simple 1x1 texture that can be used as a placeholder for testing
        /// </summary>
        public static Texture2D CreatePlaceholderTexture(GraphicsDevice graphicsDevice)
        {
            try
            {
                if (graphicsDevice == null)
                    throw new ArgumentNullException(nameof(graphicsDevice));

                Texture2D texture = new Texture2D(graphicsDevice, 1, 1);
                texture.SetData(new[] { Color.White });
                return texture;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create placeholder texture: {ex.Message}");
                return null;
            }
        }
    }
}