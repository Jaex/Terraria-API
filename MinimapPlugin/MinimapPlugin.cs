﻿using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MinimapPlugin
{
    /// <summary>
    /// F5 = Show/Hide minimap
    /// F6 = Show minimap settings form
    /// </summary>
    public class MinimapPlugin : TerrariaPlugin
    {
        public override string Name
        {
            get { return "Minimap"; }
        }

        public override Version Version
        {
            get { return new Version(2, 0); }
        }

        public override Version APIVersion
        {
            get { return new Version(1, 1); }
        }

        public override string Author
        {
            get { return "High / Jaex"; }
        }

        public override string Description
        {
            get { return "Its a minimap, what do you think?"; }
        }

        public const string SettingsFilename = "MinimapSettings.xml";

        private WorldRenderer rend;
        private InputManager input;
        private Texture2D minimap;
        // private Texture2D chest;
        private Thread renderthread;
        private MinimapSettings settings;
        private MinimapForm settingsForm;

        public MinimapPlugin(Main main)
            : base(main)
        {
            input = new InputManager();
        }

        public override void Initialize()
        {
            Application.EnableVisualStyles();
            // GameHooks.OnLoadContent += GameHooks_OnLoadContent;
            GameHooks.OnUpdate += GameHooks_OnUpdate;
            DrawHooks.OnEndDraw += DrawHooks_OnEndDraw;
            renderthread = new Thread(RenderMap);
            renderthread.Start();

            ThreadPool.QueueUserWorkItem(state => settings = TerrariaAPI.SettingsHelper.Load<MinimapSettings>(SettingsFilename));
        }

        public override void DeInitialize()
        {
            renderthread = null;
            // GameHooks.OnLoadContent -= GameHooks_OnLoadContent;
            GameHooks.OnUpdate -= GameHooks_OnUpdate;
            DrawHooks.OnEndDraw -= DrawHooks_OnEndDraw;

            if (settings != null)
            {
                TerrariaAPI.SettingsHelper.Save(settings, SettingsFilename);
            }
        }

        private void GameHooks_OnLoadContent(ContentManager obj)
        {
            // chest = BitmapToTexture(Game.GraphicsDevice, Properties.Resources.chest);
        }

        private void GameHooks_OnUpdate(GameTime obj)
        {
            if (Game.IsActive && settings != null)
            {
                input.Update();

                if (input.IsKeyUp(Keys.F5, true))
                {
                    if (rend == null)
                    {
                        rend = new WorldRenderer(Main.tile, Main.maxTilesX, Main.maxTilesY);
                    }
                    else
                    {
                        rend = null;
                    }
                }
                else if (input.IsKeyUp(Keys.F6, true))
                {
                    if (settingsForm == null || settingsForm.IsDisposed)
                    {
                        settingsForm = new MinimapForm(settings);
                    }

                    settingsForm.Show();
                    settingsForm.BringToFront();
                }
            }
        }

        private void DrawHooks_OnEndDraw(SpriteBatch arg1)
        {
            if (Game.IsActive && settings != null && rend != null && minimap != null && !Main.playerInventory)
            {
                Vector2 position;

                if (settings.MinimapPosition == MinimapPosition.RightBottom)
                {
                    position = new Vector2(Main.screenWidth - minimap.Width - settings.MinimapPositionOffset,
                        Main.screenHeight - minimap.Height - settings.MinimapPositionOffset);
                }
                else
                {
                    position = new Vector2(settings.MinimapPositionOffset, Main.screenHeight - minimap.Height - settings.MinimapPositionOffset);
                }

                Game.spriteBatch.Draw(minimap, position, new Color(1, 1, 1, settings.MinimapTransparency));
                // DrawPlayers();
            }
        }

        private void RenderMap()
        {
            while (renderthread != null)
            {
                if (settings != null && rend != null)
                {
                    int curx = (int)(Main.player[Main.myPlayer].position.X / 16) + settings.PositionOffsetX;
                    int cury = (int)(Main.player[Main.myPlayer].position.Y / 16) + settings.PositionOffsetY;
                    int width = settings.MinimapWidth;
                    int height = settings.MinimapHeight;

                    int[,] img = rend.GenerateMinimap(curx, cury, width, height, (int)Main.worldSurface, settings.MinimapZoom,
                        settings.ShowSky, settings.ShowBorder, settings.ShowCrosshair);

                    minimap = MinimapHelper.IntsToTexture(Game.GraphicsDevice, img, width, height);
                }

                Thread.Sleep(100);
            }
        }

        /*private void DrawPlayers()
        {
            for (int i = 0; i < Main.player.Length; i++)
            {
                if (Main.player[i].active)
                {
                    int mex = (int)(Main.player[Main.myPlayer].position.X / 16);
                    int mey = (int)(Main.player[Main.myPlayer].position.Y / 16);
                    int targetx = (int)(Main.player[i].position.X / 16);
                    int targety = (int)(Main.player[i].position.Y / 16);

                    if (targetx < mex - 100)
                        continue;
                    if (targetx > mex + 100)
                        continue;
                    if (targety < mey - 100)
                        continue;
                    if (targety > mey + 100)
                        continue;

                    targetx = targetx - mex + 100;
                    targety = targety - mey + 100;

                    targetx -= Main.player[i].width / 2;
                    targety -= Main.player[i].height;

                    Game.spriteBatch.Draw(chest, new Vector2(Main.screenWidth - minimap.Width + targetx, Main.screenHeight - minimap.Height + targety), Color.White);
                }
            }
        }*/
    }
}