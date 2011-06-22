﻿using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using XNAHelpers;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace TrainerPlugin
{
    /// <summary>
    /// F7 = Show trainer form
    /// </summary>
    [APIVersion(1, 5)]
    public class TrainerPlugin : TerrariaPlugin
    {
        public override string Name
        {
            get { return "Trainer"; }
        }

        public override Version Version
        {
            get { return new Version(1, 0); }
        }

        public override string Author
        {
            get { return "High / Jaex"; }
        }

        public override string Description
        {
            get { return "Just a simple 'trainer'"; }
        }

        private InputManager input;
        private TrainerForm trainerform;
        private TrainerSettings trainerSettings;
        private TrainerSettings defaultSettings;
        private Texture2D gridtexture;

        private Player me
        {
            get { return Main.player[Main.myPlayer]; }
        }

        public TrainerPlugin(Main game)
            : base(game)
        {
            input = new InputManager();
            trainerSettings = new TrainerSettings();
            defaultSettings = new TrainerSettings();
            trainerform = new TrainerForm(trainerSettings);
        }

        public override void Initialize()
        {
            GameHooks.LoadContent += GameHooks_LoadContent;
            GameHooks.Update += TerrariaHooks_Update;
            PlayerHooks.UpdatePhysics += PlayerHooks_UpdatePhysics;
            DrawHooks.DrawInterface += DrawHooks_DrawInterface;
        }

        private void GameHooks_LoadContent(ContentManager obj)
        {
            gridtexture = TrainerHelper.CreateGrid(Game.GraphicsDevice);
        }

        public override void DeInitialize()
        {
            GameHooks.LoadContent -= GameHooks_LoadContent;
            GameHooks.Update -= TerrariaHooks_Update;
            PlayerHooks.UpdatePhysics -= PlayerHooks_UpdatePhysics;
            DrawHooks.DrawInterface -= DrawHooks_DrawInterface;
        }

        private void TerrariaHooks_Update(GameTime gameTime)
        {
            if (Game.IsActive && trainerSettings != null)
            {
                input.Update(gameTime);

                if (input.IsKeyPressed(Keys.F7))
                {
                    trainerform.Visible = !trainerform.Visible;
                }
                else if (input.IsControlKeyDown && input.IsKeyPressed(Keys.B) && trainerSettings.AllowBankOpen)
                {
                    TrainerHelper.OpenBank();
                }
                else if (input.IsControlKeyDown && input.IsKeyDown(Keys.Z, 250) && trainerSettings.CreateWater)
                {
                    TrainerHelper.AddLiquidToCursor(true);
                }
                else if (input.IsControlKeyDown && input.IsKeyDown(Keys.X, 250) && trainerSettings.CreateLava)
                {
                    TrainerHelper.AddLiquidToCursor(false);
                }

                if (input.IsMouseButtonDown(MouseButtons.Right) && trainerSettings.CreateTile)
                {
                    Item item = me.inventory[me.selectedItem];

                    if (item.active && item.createTile > -1)
                    {
                        TrainerHelper.AddTileToCursor(item.createTile, trainerSettings.BigBrushSize);
                    }
                }
                else if (input.IsMouseButtonDown(MouseButtons.Middle))
                {
                    if (trainerSettings.DestroyTile)
                    {
                        TrainerHelper.DestroyTileFromCursor(false, trainerSettings.BigBrushSize);
                    }

                    if (trainerSettings.DestroyWall)
                    {
                        TrainerHelper.DestroyTileFromCursor(true, trainerSettings.BigBrushSize);
                    }
                }
            }
        }

        private void PlayerHooks_UpdatePhysics(Player obj)
        {
            if (Game.IsActive && trainerSettings != null)
            {
                if (trainerSettings.EnableTrainer)
                {
                    ApplySettings(trainerSettings);
                }
                else
                {
                    ApplySettings(defaultSettings);
                }
            }
        }

        private void ApplySettings(TrainerSettings settings)
        {
            if (settings.InfiniteHealth)
            {
                me.statLife = me.statLifeMax;
            }

            if (settings.InfiniteMana)
            {
                me.statMana = me.statManaMax;
            }

            if (settings.InfiniteBreath)
            {
                me.breath = me.breathMax;
            }

            if (settings.InfiniteAmmo)
            {
                foreach (Item item in me.inventory)
                {
                    if (item.ammo > 0)
                    {
                        item.stack = item.maxStack - 1;
                    }
                }
            }

            if (settings.NoFallDamage)
            {
                me.noFallDmg = true;
            }

            if (settings.NoKnockback)
            {
                me.noKnockback = true;
            }

            if (settings.JumpBoost)
            {
                me.jumpBoost = true;
            }

            if (settings.DoubleJump)
            {
                me.doubleJump = true;
            }

            if (settings.InfiniteJump)
            {
                me.jumpAgain = true;
            }

            if (settings.RocketBoots)
            {
                me.rocketBoots = true;
            }

            if (settings.UseFlipper)
            {
                me.accFlipper = true;
            }

            if (settings.FireWalk)
            {
                me.fireWalk = true;
            }

            if (settings.SpawnMax)
            {
                me.spawnMax = true;
            }

            if (settings.InstantRespawn)
            {
                me.respawnTimer = 0;
            }

            if (settings.NoPotionCooldown)
            {
                me.potionDelay = 0;
            }

            if (settings.NoManaCost)
            {
                me.manaCost = 0;
            }

            Main.lightTiles = settings.LightTiles;

            if (settings.LightYourCharacter)
            {
                TrainerHelper.LightCharacter();
            }

            if (settings.LightCursor)
            {
                TrainerHelper.LightCursor();
            }

            Main.grabSun = settings.GrabSun;
            Main.stopSpawns = settings.StopSpawns;
        }

        private void DrawHooks_DrawInterface(SpriteBatch sb, HandledEventArgs e)
        {
            if (trainerSettings.DrawGrid)
            {
                int offx = (int)(Main.screenPosition.X) % 16;
                int offy = (int)(Main.screenPosition.Y) % 16;

                for (int y = -offy; y < Main.screenHeight + 16; y += 16)
                {
                    for (int x = -offx; x < Main.screenWidth + 16; x += 16)
                    {
                        sb.Draw(gridtexture, new Vector2(x, y), Color.White);
                    }
                }
            }
        }
    }
}