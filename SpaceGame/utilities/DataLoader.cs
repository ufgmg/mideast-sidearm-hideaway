using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

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
        public const string PARTICLE_EFFECT_PATH = "data/ParticleEffectData.xml";
        public const string SPRITE_PATH = "data/SpriteData.xml";
        public const string UNIT_DATA_PATH = "data/UnitData.xml";
        public const string GADGET_DATA_PATH = "data/GadgetData.xml";
        public const string WEAPON_DATA_PATH = "data/WeaponData.xml";
        public const string LEVEL_DIRECTORY = "data/LevelData.xml";

        /// <summary>
        /// Main XML data loading method. All game objects stored in XML should use this method to load.
        /// </summary>
        /// <typeparam name="T">Data type to construct</typeparam>
        /// <param name="xmlPath">Path to XML doc containing data to load</param>
        /// <param name="elementName">Name of XElements to load from doc</param>
        /// <returns>An enumeration of all data loaded elements with a matching name</returns>
        public static System.Collections.Generic.IEnumerable<T> CollectData<T>(
            string xmlPath, string elementName)
        {
            return (from el in XElement.Load(xmlPath).Descendants(elementName)
                    select ElementToData<T>(el)
                    ); 
        }

        /// <summary>
        /// Parse a single XElement into an object.
        /// Tries to parse and cast every XAttribute and assign to the correspondingly named field
        /// Calls ElementToData recursively on every nested element
        /// </summary>
        /// <typeparam name="T">Type of object to construct from XElement</typeparam>
        /// <param name="el">XElement to construct object from</param>
        /// <returns>An object of type T, whose fields have been assigned based on the XElements attributes and sub-elements</returns>
        public static T ElementToData<T>(XElement el)
        {
            Type dataType = typeof(T);
            T data = Activator.CreateInstance<T>();

            foreach (XAttribute at in el.Attributes())
            {
                string fieldName = at.Name.LocalName;
                System.Reflection.FieldInfo p = dataType.GetField(fieldName);
                dataType.GetField(fieldName).SetValue(data, Convert.ChangeType(at.Value, p.FieldType));
            }

            foreach (XElement subel in el.Elements())
            {
                string fieldName = subel.Name.LocalName;
                System.Reflection.FieldInfo p = dataType.GetField(fieldName);
                Type elType = p.FieldType;
                MethodInfo method = typeof(DataLoader).GetMethod("ElementToData");
                MethodInfo genericMethod = method.MakeGenericMethod(new Type[] {elType});
                var subData = genericMethod.Invoke(null, new Object[] {subel});
                dataType.GetField(fieldName).SetValue(data, Convert.ChangeType(subData, p.FieldType));
            }

            return data;
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

            return ElementToData<ParticleGeneratorData>(originalElement);
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
