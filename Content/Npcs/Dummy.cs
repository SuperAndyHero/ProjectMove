using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;

namespace ProjectMove.Content.Npcs.NpcTypes
{
    public class Dummy : NpcBase
    {
        public override void Setup()
        {
            npc.displayName = "Dummy";
            npc.maxHealth = 1000;
            npc.damage = 0;
            npc.size = new Vector2(20, 20);
        }

        public override bool TileInteract() => false;
        public override bool NpcInteract() => false;
        public override bool PlayerInteract() => false;

        public override void AI()
        {
            
        }
    }
}