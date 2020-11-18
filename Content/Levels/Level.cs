using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using ProjectMove.Content.Npcs.NpcTypes;
using ProjectMove;
using static ProjectMove.GameID;

namespace ProjectMove.Content.Levels
{
    //for seperate areas / worlds, for seperating into seperate files and setting things up when swapping easier
    public static class LevelHandler
    {
        public static List<LevelBase> Bases;//static list of npc bases, copied(?) from when each npc is created
        public static List<string> LevelInternalNames;

        public static void Initialize()
        {
            Bases = new List<LevelBase>();

            LevelID = new Dictionary<Type, ushort>();

            List<Type> TypeList = Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(LevelBase)) && t.Namespace == "ProjectMove.Content.Levels.LevelTypes")
                      .ToList();

            for (ushort i = 0; i < TypeList.Count; i++)
            {
                Type type = TypeList[i];

                Bases.Add((LevelBase)Activator.CreateInstance(type));
                LevelID.Add(type, i);
            }
        }

        public static void LoadLevelTextures()
        {

        }

        //public static ushort LevelIdByName(string name)
        //{
        //    ushort index = 0;
        //    foreach (string str in LevelInternalNames)
        //    {
        //        if (str == name)
        //            return index;
        //        index++;
        //    }
        //    return 0;
        //}
    }

    public abstract class LevelBase//there is no level object, just the base which is directly used by the world
    {
        /// <summary>
        /// Called before everything, as this is created
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Called during worldgen, after the world has been cleared and before the player is created
        /// </summary>
        /// <param name="world"></param>
        public virtual void Worldgen(World world) { }

        /// <summary>
        /// Called after worldgen and after the player is created
        /// </summary>
        /// <param name="world"></param>
        public virtual void Setup(World world) { }

        /// <summary>
        /// A update hook for per-level extra effects
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// A draw hook for per-level extra vfx
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(SpriteBatch spriteBatch) { }

        public virtual Point Size() => new Point(4);
    }
}