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
using ProjectMove.Content.Tiles.TileTypes.Walls;
using ProjectMove.Content.Npcs.NpcTypes;

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
            ushort type = GameID.GetWallID<StoneWall>();
            ushort type2 = GameID.GetWallID<AirWall>();
            for (int i = 0; i < world.Size.X; i++)
            {
                for (int j = 0; j < world.Size.Y; j++)
                {
                    //if(GameMain.random.NextBool())
                        world.PlaceTile(i > 8 ? type : type2, i, j, (int)World.TileLayer.Wall);

                    //ushort type = (ushort)GameMain.random.Next(TileHandler.TileInternalNames.Count);
                    //world.PlaceTile(type, i, j);
                }
            }
        }

        public override void Setup(World world)
        {
            world.SpawnNpc(GameID.GetNpcID<Seeker>(), GameMain.ScreenSize.ToVector2() / 2, Vector2.Zero);

            world.player.position = Size().TileToWorldCoords().ToVector2() / 2;
        }
    }
}