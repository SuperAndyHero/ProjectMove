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
            npc.size = new Vector2(16, 16);
        }

        public override void AI()
        {
            
        }
    }
}