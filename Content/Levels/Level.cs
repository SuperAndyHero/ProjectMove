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
            LevelInternalNames = new List<string>();

            List<Type> TypeList = Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.Namespace == "ProjectMove.Content.Levels.LevelTypes" && t.IsSubclassOf(typeof(LevelBase)))
                      .ToList();

            foreach (Type type in TypeList)
            {
                Bases.Add((LevelBase)Activator.CreateInstance(type));
                LevelInternalNames.Add(type.Name);
            }
        }

        public static ushort LevelIdByName(string name)
        {
            ushort index = 0;
            foreach (string str in LevelInternalNames)
            {
                if (str == name)
                    return index;
                index++;
            }
            return 0;
        }
    }

    public abstract class LevelBase
    {
        public Level level;

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
        public virtual void ExtraUpdate() { }

        /// <summary>
        /// A draw hook for per-level extra vfx
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void ExtraDraw(SpriteBatch spriteBatch) { }

        public virtual Point Size() => new Point(4);
    }

    public class Level//TODO level object can be eleminated, and the world object can take it's place
    {
        public Level(ushort levelType)//this stuff is done via the ctor, the npc doesn't do this
        {
            type = levelType;
            levelBase = LevelHandler.Bases[type];
            levelBase.level = this;
            levelBase.Initialize();
            size = levelBase.Size();//fallback size
        }

        public LevelBase levelBase;

        public readonly ushort type;

        public Point size;

        #region dummy methods
        public void Worldgen(World world) => levelBase.Worldgen(world);

        public void Setup(World world) => levelBase.Setup(world);

        public void Update() => levelBase.ExtraUpdate();

        public void Draw(SpriteBatch spriteBatch) => levelBase.ExtraDraw(spriteBatch);
        #endregion    
    }
}