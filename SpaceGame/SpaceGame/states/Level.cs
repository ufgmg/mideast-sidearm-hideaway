﻿using System;
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
            public BlackHole BlackHole;
            public Vector2 PlayerStartLocation;
        }
        #endregion

        #region fields
        Spaceman _player;
        BlackHole _blackHole;
        Weapon _primaryWeapon, _secondaryWeapon;
        Gadget _primaryGadget, _secondaryGadget;
        Wave[] _trickleWaves;
        Wave[] _burstWaves;
        int _waveNumber;
        //debug
        #endregion

        #region constructor
        public Level (int levelNumber)
            : base(false)
        {
            LevelData data = DataLoader.LoadLevel(levelNumber);
            _player = new Spaceman(data.PlayerStartLocation);
            _blackHole = data.BlackHole;
            //construct waves
            _trickleWaves = new Wave[data.TrickleWaveData.Length];
            for (int i = 0; i < _trickleWaves.Length; i++)
            { 
                _trickleWaves[i] = new Wave(data.TrickleWaveData[i], true);
            }
            _burstWaves = new Wave[data.BurstWaveData.Length];
            for (int i = 0; i < _burstWaves.Length; i++)
            { 
                _burstWaves[i] = new Wave(data.BurstWaveData[i], false);
            }

            _primaryWeapon = new ProjectileWeapon("Rocket", _player);
            _secondaryWeapon = new ProjectileWeapon("Swarmer", _player);
            _primaryGadget = new Gadget(new Gadget.GadgetData { MaxEnergy = 1000 });
            _waveNumber = 0;

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
            _player.Update(gameTime);
            _primaryWeapon.Update(gameTime);
            _secondaryWeapon.Update(gameTime);
            _primaryGadget.Update(gameTime);
            _blackHole.Update(gameTime);

            _trickleWaves[_waveNumber].Update(gameTime, _player,
                _blackHole, _primaryWeapon, _secondaryWeapon);
            _burstWaves[_waveNumber].Update(gameTime, _player,
                _blackHole, _primaryWeapon, _secondaryWeapon);
        }

        private void handleInput(InputManager input)
        { 
            if (input.Exit)
                this.PopState = true;

            _player.MoveDirection = input.MoveDirection;
            _player.LookDirection = XnaHelper.DirectionBetween(_player.Center, input.MouseLocation);

            if (input.FirePrimary)
            {
                _primaryWeapon.Trigger(_player.Position, input.MouseLocation);
            }
            if (input.FireSecondary)
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
            _trickleWaves[0].Draw(spriteBatch);
            _burstWaves[0].Draw(spriteBatch);
        }
        #endregion
    }
}
