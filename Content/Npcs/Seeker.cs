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
            npc.damage = 10;
            npc.size = new Vector2(16, 16);
        }
        public override void AI()
        {
            //npc.currentWorld.PlaceTile(1, npc.position.WorldToTileCoords(), 2);
            if(GameMain.mainWorld.player.position - npc.position != Vector2.Zero)
            {
                npc.velocity = Vector2.Normalize(GameMain.mainWorld.player.position - npc.position);
            }
        }
    }
}