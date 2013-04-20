using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceGame.units
{
    public struct PhysicalData
    {
        //stat effect decrease per second
        public const float DEFAULT_STAT_RESIST = 30;

        public String Name;
        public String MovementParticleEffectName;
        public float Mass;
        public float MoveForce;
        public float MaxSpeed;
        public float DecelerationFactor;
        public float Health;
        public float FireResist;
        public float ShockResist;
        public float CryoResist;
    }
}
