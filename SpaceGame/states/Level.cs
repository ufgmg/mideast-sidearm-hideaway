using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using SpaceGame.graphics;
using SpaceGame.graphics.hud;
using SpaceGame.utility;
using SpaceGame.units;
using SpaceGame.equipment;
using Microsoft.Xna.Framework.Content;

namespace SpaceGame.states
{
    //each gadget is associated with an action that affects the game
    public delegate void GadgetAction(bool active);

    class Level : Gamestate
    {
		public static Texture2D s_CursorTexture;

        #region classes
        public struct LevelData
        {
            public Wave.WaveData[] TrickleWaveData;
            public Wave.WaveData[] BurstWaveData;
            public UnicornData[] Unicorns;
            public FoodCart[] FoodCarts;
            public BlackHole BlackHole;
            public Vector2 PlayerStartLocation;
            public int Width, Height;
        }
        #endregion

        #region constants
        const float c_timeSlowFactor = 0.5f;
        #endregion

        #region fields
        Spaceman _player;
        InventoryManager _inventoryManager;
        BlackHole _blackHole;
        Weapon _primaryWeapon, _secondaryWeapon;
        Gadget _primaryGadget, _secondaryGadget;
        Wave[] _waves;
        Unicorn[] _unicorns;
        FoodCart[] _foodCarts;
        Rectangle _levelBounds;
        Vector2 _mousePos;

        GUI userInterface;
        Rectangle _cameraLock;
        Camera2D _camera;

        bool _timeSlowed;

		Vector2 _cursorTextureCenter;

        TimeSpan _gameOverTimer = TimeSpan.FromSeconds(3.0);
        #endregion

        #region constructor
        public Level (ContentManager content, int levelNumber, InventoryManager im)
            : base(content, false)
        {
            LevelData data = DataLoader.LoadLevel(levelNumber);
            _levelBounds = new Rectangle(0, 0, data.Width, data.Height);
            _player = new Spaceman(data.PlayerStartLocation);
            _blackHole = data.BlackHole;
            _waves = new Wave[data.TrickleWaveData.Length + data.BurstWaveData.Length];
            _camera = new Camera2D(_player.Position, _levelBounds.Width, _levelBounds.Height);
            //construct waves
            for (int i = 0; i < data.TrickleWaveData.Length; i++)
            { 
                _waves[i] = new Wave(data.TrickleWaveData[i], true, _levelBounds);
            }
            for (int i = 0; i < data.BurstWaveData.Length; i++)
            {
                _waves[i + data.TrickleWaveData.Length] = new Wave(data.BurstWaveData[i], false, _levelBounds);
            }
            //Test code to set weapons 1-6 to created weapons
            im.setPrimaryWeapon(new ProjectileWeapon("Rocket", _player));
            im.setSecondaryWeapon(new ThrowableWeapon("Cryonade", _player));
            im.setPrimaryGadget(new Gadget("Teleporter", this));
            im.setSecondaryGadget(new Gadget("Stopwatch", this));
            im.setSlot(1, new ThrowableWeapon("Cryonade", _player));

            //Set Weapon holders in level
            _primaryWeapon = im.getPrimaryWeapon();
            _secondaryWeapon = im.getSecondaryWeapon();

            _unicorns = new Unicorn[data.Unicorns.Length];
            for (int j = 0; j < data.Unicorns.Length; j++)
            {
                _unicorns[j] = new Unicorn(data.Unicorns[j]);
            }

            _foodCarts = data.FoodCarts;

            _primaryGadget = im.getPrimaryGadget();
            _secondaryGadget = im.getSecondaryGadget();
            _inventoryManager = im;
            
            userInterface = new GUI(_player, _blackHole);

			_cursorTextureCenter = new Vector2(s_CursorTexture.Width / 2 , s_CursorTexture.Height / 2);
            selectRandomWeapons();
            Song song = content.Load<Song>("music/gravitational_conflict");
            MediaPlayer.Play(song);
        }

        void selectRandomWeapons()
        {
            Random rand = new Random();
            int rand1 = rand.Next(0, 4);
            int rand2;
            do
            {
                rand2 = rand.Next(0, 4);
            } while (rand2 == rand1);
            ProjectileWeapon[] weapons = new ProjectileWeapon[]
            {
                new ProjectileWeapon("Shotgun", _player),
                new ProjectileWeapon("Gatling", _player),
                new ProjectileWeapon("Flamethrower", _player),
                new ProjectileWeapon("Rocket", _player),
            };
            _primaryWeapon = weapons[rand1];
            _secondaryWeapon = weapons[rand2];
        }
        #endregion

        #region methods
        public override void Update(GameTime gameTime, InputManager input, InventoryManager im)
        {
            if (_player.UnitLifeState == PhysicalUnit.LifeState.Destroyed || _player.UnitLifeState == PhysicalUnit.LifeState.Disabled
                || _blackHole.capacityUsed > _blackHole.totalCapacity)
            {
                _gameOverTimer -= gameTime.ElapsedGameTime;
            }
            if (_gameOverTimer < TimeSpan.Zero)
            {
                ReplaceState = new Gamemenu(_content);
            }
            _mousePos = input.MouseLocation;
            input.SetCameraOffset(_camera.Position);
            handleInput(input);
            _camera.Update(gameTime, _player.Position);
            //if player is outside static area rectangle, call update on camera to update position of camera until
            //the player is in the static area rectangle or the camera reaches the _levelbounds, in which case,
            //the camera does not move in that direction (locks)

            /*
            if ((_player.HitRect.Bottom > _cameraLock.Bottom && _player.HitRect.Top < _cameraLock.Top &&
            _player.HitRect.Right < _cameraLock.Right && _player.HitRect.Left > _cameraLock.Left) && (player is in level bounds)
            {
             * _camera.Update(gameTime);
             * _cameraLock.X = (int)(_camera.position.X + (_camera.getViewportWidth() * 0.2));
             * _cameraLock.Y = (int)(_camera.position.Y + (_camera.getViewportHeight() * 0.2));
             * 
            }*/
           
            if (_timeSlowed)
                gameTime = new GameTime(gameTime.TotalGameTime, 
                    TimeSpan.FromSeconds((float)gameTime.ElapsedGameTime.TotalSeconds / 2));

            if (_blackHole.State == BlackHole.BlackHoleState.Pulling)
            {
                _blackHole.ApplyToUnit(_player, gameTime);
            }
            _player.Update(gameTime, _levelBounds);
            _primaryGadget.Update(gameTime);
            _secondaryGadget.Update(gameTime);
            _blackHole.Update(gameTime);


            if (_blackHole.State == BlackHole.BlackHoleState.Overdrive)
            {
                foreach (Wave w in _waves)
                {
                    w.SpawnEnable = false;
                }
                foreach (Unicorn u in _unicorns)
                {
                    u.SpawnEnable = false;
                }
            }
          
            for (int i = 0; i < _waves.Length; i++)
            {
                _waves[i].Update(gameTime, _player, _blackHole, _primaryWeapon, _secondaryWeapon, _inventoryManager, _unicorns);
                //check cross-wave collisions
                if (_waves[i].Active)
                {
                    for (int j = i + 1; j < _waves.Length; j++)
                    {
                        _waves[i].CheckAndApplyCollisions(_waves[j]);
                    }
                }
            }

            for (int i = 0; i < _unicorns.Length; i++)
            {
                _unicorns[i].Update(gameTime, _levelBounds, _blackHole.Position, _player.Position, _player.HitRect);
                _unicorns[i].CheckAndApplyCollision(_player, gameTime);
                _blackHole.TryEatUnicorn(_unicorns[i], gameTime);
                for (int j = 0; j < _foodCarts.Length; j++)
                {
                    _unicorns[i].CheckAndApplyCollision(_foodCarts[j], gameTime);
                }
            }

            for (int i = 0; i < _foodCarts.Length; i++)
            {
                _foodCarts[i].Update(gameTime, _levelBounds, _blackHole.Position);
                _primaryWeapon.CheckAndApplyCollision(_foodCarts[i], gameTime.ElapsedGameTime);
                _secondaryWeapon.CheckAndApplyCollision(_foodCarts[i], gameTime.ElapsedGameTime);
                _inventoryManager.CheckCollisions(gameTime, _foodCarts[i]);
                _blackHole.ApplyToUnit(_foodCarts[i], gameTime);
            }

            //Update Weapons 
            _primaryWeapon.Update(gameTime);
            _secondaryWeapon.Update(gameTime);
            //update all items
            _inventoryManager.Update(gameTime, input);
        }

        private void handleInput(InputManager input)
        { 
            if (input.Exit)
                this.PopState = true;

            if (_blackHole.State == BlackHole.BlackHoleState.Exhausted)
                return;

            _player.MoveDirection = input.MoveDirection;
            _player.LookDirection = XnaHelper.DirectionBetween(_player.Center, input.MouseLocation);

            if (_player.UnitLifeState == PhysicalUnit.LifeState.Living)
            {
                if (input.FirePrimary)
                {
                    _primaryWeapon.Trigger(_player.Position, input.MouseLocation);
                }
                else if (input.FireSecondary)
                {
                    _secondaryWeapon.Trigger(_player.Position, input.MouseLocation);
                }
                if (input.UseItem)
                {
                    _inventoryManager.CurrentItem.Use(input.MouseLocation);
                }
                if (input.TriggerGadget1)
                {
                    _primaryGadget.Trigger();
                }
                if (input.TriggerGadget2)
                {
                    _secondaryGadget.Trigger();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, _camera.TransformMatrix());
            
            _blackHole.Draw(spriteBatch);
            _player.Draw(spriteBatch);
            _primaryWeapon.Draw(spriteBatch);
            _secondaryWeapon.Draw(spriteBatch);
            if (_inventoryManager.CurrentItem != null)
            {
                _inventoryManager.CurrentItem.Draw(spriteBatch);
            }
			
            foreach (FoodCart cart in _foodCarts)
            {
                cart.Draw(spriteBatch);
            }

            foreach (Wave wave in _waves)
            {
                wave.Draw(spriteBatch);
            }
			
            foreach (Unicorn unicorn in _unicorns)
            {
                unicorn.Draw(spriteBatch);
            }
            spriteBatch.End();

            spriteBatch.Begin();
            userInterface.draw(spriteBatch);
			spriteBatch.Draw(s_CursorTexture, _mousePos - _cursorTextureCenter, Color.White);
            spriteBatch.End();

        }
        #endregion

        #region gadget actions
        public void TimeSlowAction(bool active)
        {
            _timeSlowed = active;
        }
        public void TeleportAction(bool active)
        {
            _player.Teleport(_mousePos);
        }
        #endregion
    }
}
