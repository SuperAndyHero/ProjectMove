using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;
using ProjectMove.Content.Player;
using ProjectMove.Content.Npcs;
using ProjectMove.Content.Tiles;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using ProjectMove.Content.Tiles.TileTypes;
using System.Linq;
using ProjectMove.Content.Levels;
using ProjectMove.Content.Projectiles;

namespace ProjectMove
{
    public static class GameID
    {
        public static Dictionary<Type, int> WallID;//initalized in TileHandler; the same place this is set
        public static Dictionary<Type, int> ObjectID;//these could be one list, but are not for performance sake
        public static Dictionary<Type, int> FloorID;
        //public static Dictionary<Type, ushort> LevelID;
        public static Dictionary<Type, int> NpcID;
        public static Dictionary<Type, int> ProjectileID;

        public static void Initialize()
        {
            WallID = new Dictionary<Type, int>();
            ObjectID = new Dictionary<Type, int>();
            FloorID = new Dictionary<Type, int>();

            NpcID = new Dictionary<Type, int>();
            ProjectileID = new Dictionary<Type, int>();
        }

        public static int GetWallID<T>() where T : WallBase
        {
            return WallID[typeof(T)];
        }

        public static int GetObjectID<T>() where T : ObjectBase
        {
            return ObjectID[typeof(T)];
        }

        public static int GetFloorID<T>() where T : FloorBase
        {
            return FloorID[typeof(T)];
        }

        //public static int GetLevelID<T>() where T : LevelBase
        //{
        //    return LevelID[typeof(T)];
        //}

        public static int GetNpcID<T>() where T : NpcBase
        {
            return NpcID[typeof(T)];
        }

        public static int GetProjectileID<T>() where T : ProjectileBase
        {
            return ProjectileID[typeof(T)];
        }
    }
}