using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;

namespace ProjectMove.Content.Npcs.NpcTypes
{
    public class Seeker : NpcBase
    {
        public override void Setup()
        {
            npc.displayName = "Heat Seeker";
            npc.maxHealth = 200;
            npc.size = new Vector2(16, 16);
        }

        public override void AI()
        {
            npc.velocity = GameMain.mainWorld.player.position - npc.position != Vector2.Zero ? 
                               Vector2.Normalize(GameMain.mainWorld.player.position - npc.position) : 
                                   Vector2.Zero;
        }
    }
}