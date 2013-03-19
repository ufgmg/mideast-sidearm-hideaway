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
        const string PARTICLE_EFFECT_PATH = "data/ParticleEffectData.xml";
        const string SPRITE_PATH = "data/SpriteData.xml";
        const string UNIT_DATA_PATH = "data/UnitData.xml";
        const string PROJECTILE_WEAPON_PATH = "data/WeaponData.xml";
        const string MELEE_WEAPON_PATH = "data/WeaponData.xml";
        const string LEVEL_DIRECTORY = "data/LevelData.xml";
        /// <summary>
        /// get a dict mapping sprite names to spritedata. 
        /// Run in Game.Initialize and assign to Sprite.DataDict
        /// </summary>
        /// <param name="pathToXML"></param>
        /// <param name="theContent"></param>
        /// <returns></returns>
        public static Dictionary<string, SpriteData> LoadSpriteData(ContentManager theContent)
        {
            string spriteFolderPath = "spritesheets/";
            return (from sd in XElement.Load(SPRITE_PATH).Descendants("SpriteData")
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

        public static PhysicalData LoadFoodCartData()
        {
            XElement element = XElement.Load(UNIT_DATA_PATH).Descendants("FoodCartData").Single();
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

        public static Dictionary<string, ParticleGeneratorData> LoadParticleGeneratorData(ContentManager content)
        {
            return (from el in XElement.Load(PARTICLE_EFFECT_PATH).Descendants("ParticleGeneratorData")
                           select parseGeneratorElement(el, content)).ToDictionary(t => t.Name);
        }

        private static ParticleGeneratorData parseGeneratorElement(XElement el, ContentManager content)
        {
            return new ParticleGeneratorData
            {
                Name = (string)el.Attribute("Name"),
                Speed = (float)el.Attribute("Speed"),
                SpeedVariance = (float)el.Attribute("SpeedVariance"),
                DecelerationFactor = (float)el.Attribute("DecelerationFactor"),
                ParticleRotation = (float)el.Attribute("ParticleRotation"),
                StartScale = (float)el.Attribute("StartScale"),
                ScaleVariance = (float)el.Attribute("ScaleVariance"),
                EndScale = (float)el.Attribute("EndScale"),
                SpawnArc = (float)el.Attribute("SpawnArc"),
                SpawnRate = (int)el.Attribute("SpawnRate"),
                ParticleLife = TimeSpan.FromSeconds((double)el.Attribute("ParticleLife")),
                ParticleLifeVariance = (float)el.Attribute("ParticleLifeVariance"),
                Reversed = (bool)el.Attribute("Reversed"),
                StartColor = parseColor((string)el.Attribute("StartColor")),
                EndColor = parseColor((string)el.Attribute("EndColor")),
                UniqueParticle = ((string)el.Attribute("UniqueParticle") == null ? null : content.Load<Texture2D>(PARTICLE_TEXTURE_DIRECTORY + (string)el.Attribute("UniqueParticle"))),
                Offset = (el.Attribute("Offset") == null ? 0 : (float)el.Attribute("Offset"))
            };
        }


        public static Dictionary<string, ParticleEffectData> LoadParticleEffectData(ContentManager content)
        {
            return (from sd in XElement.Load(PARTICLE_EFFECT_PATH).Descendants("ParticleEffectData")
                           select new ParticleEffectData
                           {
                               Name = (string)sd.Attribute("Name"),
                               ParticleGenerators = (from gen in sd.Elements("ParticleGenerator")
                                                     select parseGeneratorInstance(gen, content)).ToArray<ParticleGeneratorData>(),
                           }).ToDictionary(t => t.Name);
        }

        private static ParticleGeneratorData parseGeneratorInstance(XElement el, ContentManager content)
        {
            XElement originalElement = (from template in XElement.Load(PARTICLE_EFFECT_PATH).Descendants("ParticleGeneratorData")
                                        where (string)template.Attribute("Name") == (string)el.Attribute("Name")
                                            select template).Single<XElement>();                     
                    
            foreach (XAttribute at in el.Attributes())
            {
                XName atName = at.Name.LocalName;
                if (originalElement.Attribute(atName) != null)
                    originalElement.Attribute(atName).SetValue(at.Value);
                else
                    originalElement.Add(new XAttribute(at.Name.LocalName, at.Value));
            }

            return parseGeneratorElement(originalElement, content);
        }

        private static Color parseColor(string colorValues)
        {
			string[] nums = colorValues.Split(',');
			Debug.Assert(nums.Length == 4);
			return new Color(byte.Parse(nums[1]), byte.Parse(nums[2]), byte.Parse(nums[3]), byte.Parse(nums[0]));
		}

        public static Dictionary<string, ProjectileWeaponData> LoadProjectileWeaponData()
        {
            return (from wd in XElement.Load(PROJECTILE_WEAPON_PATH).Descendants("ProjectileWeapon")
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

                        FireParticleEffect = (string)wd.Attribute("FireParticleEffect"),
                        MovementParticleEffect = (string)wd.Attribute("MovementParticleEffect"),
                        SplashParticleEffect = (string)wd.Attribute("SplashParticleEffect")
                    }).ToDictionary(t => t.Name);
        }

        public static Dictionary<string, MeleeWeapon.MeleeWeaponData> LoadMeleeWeaponData()
        {
            return (from wd in XElement.Load(MELEE_WEAPON_PATH).Descendants("MeleeWeapon")
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
                        Width = (int)level.Attribute("LevelWidth"),
                        Height = (int)level.Attribute("LevelHeight"),
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
                                    }).ToArray<Wave.WaveData>(),
                        Unicorns = (from unicorn in level.Descendants("Unicorn")
                                    select new UnicornData
                                    {
                                        StartTime = (float)unicorn.Attribute("StartTime"),
                                        EndTime = (float)unicorn.Attribute("EndTime"),
                                        SpawnTime = (float)unicorn.Attribute("SpawnTime"),
                                    }).ToArray<UnicornData>(),
                                    FoodCarts = (from cart in level.Descendants("FoodCart")
                                                 select buildFoodCart(cart)).ToArray<FoodCart>()

                    }).Single<Level.LevelData>();

        }

        private static FoodCart buildFoodCart(XElement el)
        {
            return new FoodCart(TimeSpan.FromSeconds((double)el.Attribute("StartTime")),
                TimeSpan.FromSeconds((double)el.Attribute("Duration")));
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
