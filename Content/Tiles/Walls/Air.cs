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

namespace ProjectMove.Content.Tiles.TileTypes
{
    public class Air : TileBase
    {
        public override bool Draw(SpriteBatch spriteBatch, int i, int j) => false;
        public override bool IsSolid() => false;
    }
}