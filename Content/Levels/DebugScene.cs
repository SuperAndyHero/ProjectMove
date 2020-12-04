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
using ProjectMove.Content.Tiles.TileTypes.Floors;

namespace ProjectMove.Content.Levels.LevelTypes
{
    public class DebugScene : LevelBase
    {
        //public override void Initialize()
        //{

        //}

        public override Point Size() => new Point(56, 56);

        public override void Worldgen(World world)
        {
            ushort stoneWallType = GameID.GetWallID<StoneWall>();
            for (int i = 0; i < world.Size.X; i++)
            {
                for (int j = 0; j < world.Size.Y; j++)
                {
                    if(i == 0 || i == world.Size.X - 1 || j == 0 || j == world.Size.Y - 1)
                    {
                        world.PlaceTile(stoneWallType, i, j, (int)TileHandler.TileLayer.Wall);
                    }
                }
            }

            world.FillLayer(GameID.GetFloorID<Dirt>(), (int)TileHandler.TileLayer.Floor);
        }

        public override void Setup(World world)
        {
            world.player.position = Size().TileToWorldCoords().ToVector2() / 2;
            world.SpawnNpc(GameID.GetNpcID<Dasher>(), world.player.position + (Vector2.One * 100), Vector2.Zero);
            world.SpawnNpc(GameID.GetNpcID<Seeker>(), world.player.position + (Vector2.One * 100), Vector2.Zero);
        }
    }
}