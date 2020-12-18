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

namespace ProjectMove
{
    public static class GameID
    {
        public static Dictionary<Type, ushort> WallID;//initalized in TileHandler; the same place this is set
        public static Dictionary<Type, ushort> ObjectID;//these could be one list, but are not for performance sake
        public static Dictionary<Type, ushort> FloorID;

        //public static Dictionary<Type, ushort> LevelID;

        public static Dictionary<Type, ushort> NpcID;



        public static ushort GetWallID<T>() where T : WallBase
        {
            return WallID[typeof(T)];
        }

        public static ushort GetObjectID<T>() where T : ObjectBase
        {
            return ObjectID[typeof(T)];
        }

        public static ushort GetFloorID<T>() where T : FloorBase
        {
            return FloorID[typeof(T)];
        }

        //public static ushort GetLevelID<T>() where T : LevelBase
        //{
        //    return LevelID[typeof(T)];
        //}

        public static ushort GetNpcID<T>() where T : NpcBase
        {
            return NpcID[typeof(T)];
        }
    }
}