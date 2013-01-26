using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

using SpaceGame.graphics;
using SpaceGame.units;
using SpaceGame.utility;
using SpaceGame.equipment;
using SpaceGame.states;


namespace SpaceGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        InputManager _inputManager = new InputManager();

        List<Gamestate> _stateStack = new List<Gamestate>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //set resolution        TODO: Move this to an XML Configuration File
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            //TODO: replace with custom cursor
            this.IsMouseVisible = true;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //load data from xml
            Sprite.Data = DataLoader.LoadSpriteData("data/SpriteData.xml", Content);
            PhysicalUnit.Data = DataLoader.LoadPhysicalData("data/PhysicalData.xml");
            ParticleEffect.Data = DataLoader.LoadParticleEffectData("data/ParticleEffectData.xml");
            ProjectileWeapon.ProjectileWeaponData = DataLoader.LoadProjectileWeaponData("data/WeaponData.xml");

            _inputManager = new InputManager();

            //Set so units stay in screen bounds
            PhysicalUnit.ScreenWidth = graphics.GraphicsDevice.Viewport.Width;
            PhysicalUnit.ScreenHeight = graphics.GraphicsDevice.Viewport.Height;
            Weapon.ScreenBounds = graphics.GraphicsDevice.Viewport.Bounds;

            _stateStack.Add(new Level(1));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Initialize blank particle texture as a single pixel
            ParticleEffect.ParticleTexture = new Texture2D(GraphicsDevice, 1, 1);
            ParticleEffect.ParticleTexture.SetData<Color>(new Color[] {Color.White});

            XnaHelper.PixelTexture = new Texture2D(GraphicsDevice, 1, 1);
            XnaHelper.PixelTexture.SetData<Color>(new Color[] {Color.White});

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (_inputManager.Exit)
                this.Exit();

            _inputManager.Update();

            _stateStack.Last().Update(gameTime, _inputManager);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            _stateStack.Last().Draw(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
