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

namespace ProjectMove.Content.Tiles.TileTypes.Walls
{
    public class AirWall : WallBase
    {
        public override bool HasSides() => false;
        public override bool HasBottom() => false;
        public override bool Draw(SpriteBatch spriteBatch, int i, int j) => false;
        //public override bool DrawOutline(SpriteBatch spriteBatch, int i, int j) => false;
        public override bool IsSolid() => false;
        public override string TextureName() => "Air";
    }
}