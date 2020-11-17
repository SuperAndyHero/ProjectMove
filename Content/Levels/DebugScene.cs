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
using ProjectMove.Content.Npcs;
using ProjectMove;
using ProjectMove.Content.Tiles;
using ProjectMove.Content.Tiles.TileTypes;

namespace ProjectMove.Content.Levels.LevelTypes
{
    public class DebugScene : LevelBase
    {
        //public override void Initialize()
        //{

        //}

        public override Point Size() => new Point(16, 24);

        public override void Worldgen(World world)
        {
            ushort type = GameID.GetTileID<Stone>();
            ushort type2 = GameID.GetTileID<Air>();
            for (int i = 0; i < world.Size.X; i++)
            {
                for (int j = 0; j < world.Size.Y; j++)
                {
                    //if(GameMain.random.NextBool())
                        world.PlaceTile(i > 8 ? type : type2, i, j);

                    //ushort type = (ushort)GameMain.random.Next(TileHandler.TileInternalNames.Count);
                    //world.PlaceTile(type, i, j);
                }
            }
        }

        public override void Setup(World world)
        {
            //spawning npcs, move these to where the world is setup later
            //NpcHandler.NewNpc(NpcHandler.NpcIdByName("Dasher"), Vector2.Zero, Vector2.Zero);
            //NpcHandler.NewNpc(NpcHandler.NpcIdByName("Seeker"), Vector2.Zero, Vector2.Zero);
            //NpcHandler.SpawnNpc(ref world, NpcHandler.NpcIdByName("Seeker"), GameMain.ScreenSize.Half(), Vector2.Zero);//spawning a dummy


            world.player.position = Size().TileToWorldCoords().ToVector2() / 2;
        }
    }
}