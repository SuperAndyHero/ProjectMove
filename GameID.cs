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

namespace ProjectMove
{
    public static class GameID
    {
        public static Dictionary<Type, ushort> TileID;//initalized in TileHandler; the same place this is set
        public static Dictionary<Type, ushort> ObjectID;
        public static Dictionary<Type, ushort> FloorID;



        public static ushort GetTileID<T>() where T : TileBase
        {
            return TileID[typeof(T)];
        }

        public static ushort GetObjectID<T>() where T : ObjectBase
        {
            return ObjectID[typeof(T)];
        }

        public static ushort GetFloorID<T>() where T : ObjectBase
        {
            return FloorID[typeof(T)];
        }
    }
}