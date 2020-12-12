using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;

namespace ProjectMove.Content.Npcs.NpcTypes
{
    public class Dasher : NpcBase
    {
        const float Deceleration = 0.90f;

        public override void Setup()
        {
            npc.displayName = "Dasher";
            npc.maxHealth = 400;
            npc.damage = 10;
            npc.size = new Vector2(32, 32);
        }

        public override void AI()
        {
            if (npc.velocity != Vector2.Zero)
                npc.velocity = Vector2.Normalize(npc.velocity) * (npc.velocity.Length() * Deceleration);

            if(GameMain.mainUpdateCount % 120 == 0)
                npc.velocity += Vector2.Normalize(GameMain.mainWorld.player.position - npc.position) * 5f;
        }
    }
}