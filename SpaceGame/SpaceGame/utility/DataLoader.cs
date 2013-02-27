using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Xml;

using SpaceGame.graphics;
using SpaceGame.units;
using SpaceGame.equipment;
using SpaceGame.states;

using System.Xml.Linq;
using System.Diagnostics;

namespace SpaceGame.utility
{
    /// <summary>
    /// for loading data from XML files
    /// </summary>
    static class DataLoader
    {
        const string PARTICLE_TEXTURE_DIRECTORY = "particles/";
        const string UNIT_DATA_PATH = "data/UnitData.xml";
        const string LEVEL_DIRECTORY = "data/LevelData.xml";
        /// <summary>
        /// get a dict mapping sprite names to spritedata. 
        /// Run in Game.Initialize and assign to Sprite.DataDict
        /// </summary>
        /// <param name="pathToXML"></param>
        /// <param name="theContent"></param>
        /// <returns></returns>
        public static Dictionary<string, SpriteData> LoadSpriteData(string pathToXML, ContentManager theContent)
        {
            string spriteFolderPath = "spritesheets/";
            return (from sd in XElement.Load(pathToXML).Descendants("SpriteData")
                           select new SpriteData
                           {
                               Name = (string)sd.Attribute("Name"),
                               Texture = theContent.Load<Texture2D>(spriteFolderPath + (string)sd.Attribute("AssetName")),
                               FrameWidth = (int)sd.Attribute("FrameWidth"),
                               FrameHeight = (int)sd.Attribute("FrameHeight"),
                               NumFrames = (int)sd.Attribute("NumFrames"),
                               NumStates = (int)sd.Attribute("NumStates"),
                               DefaultScale = (float)sd.Attribute("DefaultScale"),
                               AnimationRate = TimeSpan.FromSeconds((double)sd.Attribute("SecondsPerAnimation")),
                               ZLayer = (float)sd.Attribute("ZLayer")
                           }).ToDictionary(t => t.Name);
        }

        public static PhysicalData LoadPhysicalData(XElement unitElement)
        {
                   return new PhysicalData
                           {
                               Name = (string)unitElement.Attribute("Name"),
                               MovementParticleEffectName = (string)unitElement.Attribute("MovementParticleEffect"),
                               Mass = (float)unitElement.Attribute("Mass"),
                               MoveForce = (float)unitElement.Attribute("MoveForce"),
                               MaxSpeed = (float)unitElement.Attribute("MaxSpeed"),
                               DecelerationFactor = (float)unitElement.Attribute("DecelerationFactor"),
                               Health = (int)unitElement.Attribute("Health")
                           };
        }

        public static PhysicalData LoadAstronautData()
        {
            XElement element = XElement.Load(UNIT_DATA_PATH).Descendants("AstronautData").Single();
            return LoadPhysicalData(element);
        }

        public static Dictionary<string, EnemyData> LoadEnemyData()
        {
            return (from enemy in XElement.Load(UNIT_DATA_PATH).Descendants("EnemyData")
                           select new EnemyData
                           {
                               Name = (string)enemy.Attribute("Name"),
                               PhysicalData = LoadPhysicalData(enemy),
                               MeleeWeaponName = (string)enemy.Attribute("MeleeWeapon")
                           }).ToDictionary(t => t.Name);
        }

        public static Dictionary<string, ParticleEffectData> LoadParticleEffectData(string pathToXML, ContentManager content)
        {
            return (from sd in XElement.Load(pathToXML).Descendants("ParticleEffectData")
                           select new ParticleEffectData
                           {
                               Name = (string)sd.Attribute("Name"),
                               Speed = (float)sd.Attribute("Speed"),
                               SpeedVariance = (float)sd.Attribute("SpeedVariance"),
                               DecelerationFactor = (float)sd.Attribute("DecelerationFactor"),
                               ParticleRotation = (float)sd.Attribute("ParticleRotation"),
                               StartScale = (float)sd.Attribute("StartScale"),
                               ScaleVariance = (float)sd.Attribute("ScaleVariance"),
                               EndScale = (float)sd.Attribute("EndScale"),
                               SpawnArc = (float)sd.Attribute("SpawnArc"),
                               SpawnRate = (int)sd.Attribute("SpawnRate"),
                               ParticleLife = TimeSpan.FromSeconds((double)sd.Attribute("ParticleLife")),
                               ParticleLifeVariance = (float)sd.Attribute("ParticleLifeVariance"),
                               Reversed = (bool)sd.Attribute("Reversed"),
                               StartColor = parseColor((string)sd.Attribute("StartColor")),
                               EndColor = parseColor((string)sd.Attribute("EndColor")),
                               UniqueParticle = ((string)sd.Attribute("UniqueParticle") == null ? null : content.Load<Texture2D>(PARTICLE_TEXTURE_DIRECTORY + (string)sd.Attribute("UniqueParticle")))
                           }).ToDictionary(t => t.Name);
        }

        private static Color parseColor(string colorValues)
        {
			string[] nums = colorValues.Split(',');
			Debug.Assert(nums.Length == 4);
			return new Color(byte.Parse(nums[1]), byte.Parse(nums[2]), byte.Parse(nums[3]), byte.Parse(nums[0]));
		}

        public static Dictionary<string, ProjectileWeaponData> LoadProjectileWeaponData(string pathToXML)
        {
            return (from wd in XElement.Load(pathToXML).Descendants("ProjectileWeapon")
                    select new ProjectileWeaponData
                    {
                        Name = (string)wd.Attribute("Name"),
                        ProjectileSpriteName = (string)wd.Attribute("ProjectileSpriteName"),
                        FireRate = (float)wd.Attribute("FireRate"),
                        MaxAmmo = (int)wd.Attribute("MaxAmmo"),
                        AmmoConsumption = (int)wd.Attribute("AmmoConsumption"),
                        ProjectilesPerFire = (int)wd.Attribute("ProjectilesPerFire"),
                        Damage = (int)wd.Attribute("Damage"),
                        ProjectileForce = (float)wd.Attribute("ProjectileForce"),
                        Recoil = (float)wd.Attribute("Recoil"),
                        ProjectileSpeed = (float)wd.Attribute("ProjectileSpeed"),
                        ProjectileAcceleration = (float)wd.Attribute("ProjectileAcceleration"),
                        ProjectileSpread = (float)wd.Attribute("ProjectileSpread"),
                        DissipateOnHit = (bool)wd.Attribute("DissipateOnHit"),
                        MaxProjectiles = (int)wd.Attribute("MaxProjectiles"),
                        ProjectileLife = TimeSpan.FromSeconds((double)wd.Attribute("ProjectileLife")),

                        SplashDamage = (int)wd.Attribute("SplashDamage"),
                        SplashRadius = (float)wd.Attribute("SplashRadius"),
                        SplashForce = (float)wd.Attribute("SplashForce"),

                        MovementParticleEffect = (string)wd.Attribute("MovementParticleEffect"),
                        SplashParticleEffect = (string)wd.Attribute("SplashParticleEffect")
                    }).ToDictionary(t => t.Name);
        }

        public static Dictionary<string, MeleeWeapon.MeleeWeaponData> LoadMeleeWeaponData(string pathToXML)
        {
            return (from wd in XElement.Load(pathToXML).Descendants("MeleeWeapon")
                    select new MeleeWeapon.MeleeWeaponData
                    {
                        Name = (string)wd.Attribute("Name"),
                        FireRate = (float)wd.Attribute("FireRate"),
                        Range = (float)wd.Attribute("Range"),
                        Recoil = (float)wd.Attribute("Recoil"),
                        HitArc = MathHelper.ToRadians((float)wd.Attribute("HitArc")),
                        MaxAmmo = (int)wd.Attribute("MaxAmmo"),
                        AmmoConsumption = (int)wd.Attribute("AmmoConsumption"),
                        Damage = (int)wd.Attribute("Damage"),
                        Force = (int)wd.Attribute("Impact"),
                        AttackParticleEffect = (string)wd.Attribute("AttackParticleEffect"),
                        HitParticleEffect = (string)wd.Attribute("HitParticleEffect")
                    }).ToDictionary(t => t.Name);
        }

        public static Level.LevelData LoadLevel(int levelNumber)
        {
            return (from level in XElement.Load(LEVEL_DIRECTORY).Descendants("Level")
                    where (int)level.Attribute("LevelNumber") == levelNumber
                    select new Level.LevelData
                    {
                        PlayerStartLocation = parseVector(level.Element("Player")),
                        BlackHole = parseBlackHole(level.Element("BlackHole")),
                        TrickleWaveData = (from wave in level.Descendants("TrickleWave")
                                    select new Wave.WaveData
                                    {
                                        EnemyNames = (from enemy in wave.Descendants("Enemy")
                                                   select (string)enemy.Attribute("Name")).ToArray<string>(),
                                        SpawnInterval = TimeSpan.FromSeconds((float)wave.Attribute("SpawnInterval")),
                                        StartTime = TimeSpan.FromSeconds((float)wave.Attribute("StartTime")),
                                    }).ToArray<Wave.WaveData>(),
                        BurstWaveData = (from wave in level.Descendants("BurstWave")
                                    select new Wave.WaveData
                                    {
                                        EnemyNames = (from enemy in wave.Descendants("Enemy")
                                                   select (string)enemy.Attribute("Name")).ToArray<string>(),
                                        SpawnInterval = TimeSpan.FromSeconds((float)wave.Attribute("SpawnInterval")),
                                        StartTime = TimeSpan.FromSeconds((float)wave.Attribute("StartTime")),
                                    }).ToArray<Wave.WaveData>()
                    }).Single<Level.LevelData>();
        }

        private static BlackHole parseBlackHole(XElement e)
        {
            return new BlackHole(parseVector(e),
                (float)e.Attribute("Gravity"), (float)e.Attribute("Radius"),
                (float)e.Attribute("Capacity"));
        }

        private static Vector2 parseVector(XElement e)
        {
            return new Vector2((float)e.Attribute("X"), (float)e.Attribute("Y"));
        }

    }
}
