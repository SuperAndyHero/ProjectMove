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

namespace ProjectMove.Content.Tiles.TileTypes.Objects
{
    public class Desk : ObjectBase
    {
        public override Rectangle DrawRect()
        {
            return new Rectangle(Point.Zero, new Point(TileHandler.tileSize, TileHandler.tileSize + 6));//do something to make tiles auto-clip up (redo tile side and take image size into account)
            //also TODO: draw half of objects before the player (ones that are above) and half after, and add a pre-draw method to bypass this
        }
    }
}