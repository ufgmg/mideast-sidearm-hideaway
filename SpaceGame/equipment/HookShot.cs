using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceGame.graphics;
using SpaceGame.units;
using SpaceGame.utility;

namespace SpaceGame.equipment
{
    class HookShot : Weapon
    {
        //spacing between chain links to draw between handle and hook
        const float DISTANCE_PER_LINK = 15.0f;
        const float MAX_RANGE = 1000.0f;
        const float HOOK_SPEED = 20.0f;
        const float HOOK_FORCE = 12000.0f;
        const float FIRE_DELAY = 0.5f;

        #region fields
        enum HookState
        {
            Idle,
            Fired,
            Attached,
            Retracting
        }

        PhysicalUnit _hookedUnit;
        HookState _hookState;

        Vector2 _hookPosition, _hookVelocity;
        float _hookAngle;

        Rectangle _hookHitRect;

        Sprite _hookSprite, _chainSprite;
        #endregion

        #region constructor
        public HookShot(PhysicalUnit owner)
            :base(TimeSpan.FromSeconds(FIRE_DELAY), owner)
        {
            _hookState = HookState.Idle;
            _hookSprite = new Sprite("HookClaw", Sprite.SpriteType.Projectile);
            _chainSprite = new Sprite("HookChain", Sprite.SpriteType.Projectile);
            _hookHitRect = new Rectangle(0, 0, (int)_hookSprite.Width, (int)_hookSprite.Height);
        }
        #endregion

        #region methods
        protected override void UpdateWeapon(GameTime gameTime)
        {
            _hookAngle = XnaHelper.RadiansFromVector(_hookPosition - _owner.Position);
            switch (_hookState)
            {
                case (HookState.Idle):
                    {
                        if (_firing)
                        {
                            _hookState = HookState.Fired;
                            _hookPosition = _owner.Position;
                            _hookVelocity = _fireDirection * HOOK_SPEED;
                        }
                        break;
                    }
                case (HookState.Fired):
                    {
                        if (!(_firing) && Vector2.Distance(_hookPosition, _owner.Position) < MAX_RANGE)
                        {
                            _hookPosition += _hookVelocity;
                            _hookHitRect.X = (int)_hookPosition.X;
                            _hookHitRect.Y = (int)_hookPosition.Y;
                        }
                        else
                        {
                            _hookState = HookState.Retracting;
                        }

                        break;
                    }
                case (HookState.Retracting):
                    {
                        _hookPosition -= HOOK_SPEED * Vector2.Normalize(_hookPosition - _owner.Center);
                        if (XnaHelper.PointInRect(_hookPosition, _owner.HitRect))
                            _hookState = HookState.Idle;
                        break;
                    }
                case (HookState.Attached):
                    {
                        _hookPosition = _hookedUnit.Center;
                        Vector2 pullForce = HOOK_FORCE * XnaHelper.DirectionBetween(_owner.Position, _hookedUnit.Position);
                        _owner.ApplyForce(pullForce);
                        _hookedUnit.ApplyForce(Vector2.Negate(pullForce));

                        if (_firing || _hookedUnit.UnitLifeState == PhysicalUnit.LifeState.Destroyed)
                        {
                            _hookState = HookState.Idle;
                        }
                        break;
                    }
            }

        }

        public override void CheckAndApplyCollision(PhysicalUnit unit, TimeSpan time)
        {
            if (_hookState == HookState.Fired || _hookState == HookState.Retracting)
            { 
                if (XnaHelper.RectsCollide(_hookHitRect, unit.HitRect))
                {
                    _hookedUnit = unit;
                    _hookState = HookState.Attached;
                }
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            if (_hookState != HookState.Idle)
            {
                _hookSprite.Angle = _hookAngle;
                _hookSprite.Draw(sb, _hookPosition);
                Vector2 nextLinkPosition = _owner.Center;
                Vector2 chainDirection = Vector2.Normalize(_hookPosition - _owner.Center);
                int numLinksToDraw = (int)((_hookPosition - _owner.Center).Length() / DISTANCE_PER_LINK);
                for (int i = 0; i < numLinksToDraw; i++)
                {
                    nextLinkPosition += chainDirection * DISTANCE_PER_LINK;
                    _chainSprite.Draw(sb, nextLinkPosition, MathHelper.PiOver2 + _hookAngle);
                }
            }
        }
        #endregion



    }
}
