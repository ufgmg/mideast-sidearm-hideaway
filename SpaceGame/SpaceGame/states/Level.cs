using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.graphics.hud;
using SpaceGame.utility;
using SpaceGame.units;
using SpaceGame.equipment;

namespace SpaceGame.states
{
    class Level : Gamestate
    {
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

        #region fields
        Spaceman _player;
        BlackHole _blackHole;
        Weapon _primaryWeapon, _secondaryWeapon;
        Gadget _primaryGadget, _secondaryGadget;
        Wave[] _waves;
        Unicorn[] _unicorns;
        FoodCart[] _foodCarts;
        Rectangle _levelBounds;
        #endregion

        #region constructor
        public Level (int levelNumber)
            : base(false)
        {
            LevelData data = DataLoader.LoadLevel(levelNumber);
            _levelBounds = new Rectangle(0, 0, data.Width, data.Height);
            _player = new Spaceman(data.PlayerStartLocation);
            _blackHole = data.BlackHole;
            _waves = new Wave[data.TrickleWaveData.Length + data.BurstWaveData.Length];
            //construct waves
            for (int i = 0; i < data.TrickleWaveData.Length; i++)
            { 
                _waves[i] = new Wave(data.TrickleWaveData[i], true, _levelBounds);
            }
            for (int i = 0; i < data.BurstWaveData.Length; i++)
            {
                _waves[i + data.TrickleWaveData.Length] = new Wave(data.BurstWaveData[i], false, _levelBounds);
            }

            _unicorns = new Unicorn[data.Unicorns.Length];
            for (int j = 0; j < data.Unicorns.Length; j++)
            {
                _unicorns[j] = new Unicorn(data.Unicorns[j]);
            }

            _foodCarts = data.FoodCarts;

            _primaryWeapon = new ProjectileWeapon("Rocket", _player, _levelBounds);
            _secondaryWeapon = new ProjectileWeapon("Flamethrower", _player, _levelBounds);
            //_secondaryWeapon = new HookShot(_player, _levelBounds);
            _primaryGadget = new Gadget(new Gadget.GadgetData { MaxEnergy = 1000 });
        }

        #endregion

        #region methods
        public override void Update(GameTime gameTime, InputManager input)
        {
            handleInput(input);

            if (_primaryGadget.Active)
                gameTime = new GameTime(gameTime.TotalGameTime, 
                    TimeSpan.FromSeconds((float)gameTime.ElapsedGameTime.TotalSeconds / 2));
            
            _blackHole.ApplyToUnit(_player, gameTime);
            _player.Update(gameTime, _levelBounds);
            _primaryGadget.Update(gameTime);
            _blackHole.Update(gameTime);

            for (int i = 0; i < _waves.Length; i++)
            {
                _waves[i].Update(gameTime, _player, _blackHole, _primaryWeapon, _secondaryWeapon, _unicorns);
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
                for (int j = 0; j < _foodCarts.Length; j++)
                {
                    _unicorns[i].CheckAndApplyCollision(_player, gameTime);
                }
            }

            for (int i = 0; i < _foodCarts.Length; i++)
            {
                _foodCarts[i].Update(gameTime, _levelBounds, _blackHole.Position);
                _primaryWeapon.CheckAndApplyCollision(_foodCarts[i]);
                _secondaryWeapon.CheckAndApplyCollision(_foodCarts[i]);
                _blackHole.ApplyToUnit(_foodCarts[i], gameTime);
            }

            _primaryWeapon.Update(gameTime);
            _secondaryWeapon.Update(gameTime);
        }

        private void handleInput(InputManager input)
        { 
            if (input.Exit)
                this.PopState = true;

            _player.MoveDirection = input.MoveDirection;
            _player.LookDirection = XnaHelper.DirectionBetween(_player.Center, input.MouseLocation);

            if (input.FirePrimary && _player.UnitLifeState == PhysicalUnit.LifeState.Living)
            {
                _primaryWeapon.Trigger(_player.Position, input.MouseLocation);
            }
            if (input.FireSecondary && _player.UnitLifeState == PhysicalUnit.LifeState.Living)
            {
                _secondaryWeapon.Trigger(_player.Position, input.MouseLocation);
            }

            if (input.TriggerGadget1)
            {
                _primaryGadget.Trigger();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _blackHole.Draw(spriteBatch);
            _player.Draw(spriteBatch);
            _primaryWeapon.Draw(spriteBatch);
            _secondaryWeapon.Draw(spriteBatch);
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
        }
        #endregion
    }
}
