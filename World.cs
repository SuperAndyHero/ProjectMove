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
using ProjectMove.Content.Player;
using ProjectMove.Content.Npcs;
using ProjectMove.Content.Levels;
using ProjectMove.Content.Tiles;
using ProjectMove.Content.Tiles.TileTypes;
using static ProjectMove.GameID;

namespace ProjectMove
{
    //global stuff
    public class World//a static instance of this is made in gamemain
    {
        public Level level;

        public Player player;

        public List<Npc> npcs;

        public Point Size
        {
            get => level.size;
        }

        public Tile[,] tile;

        public void Initialize()
        {
            //initialize the npc list
            npcs = new List<Npc>();

            //set the current level, placeholder for now
            level = new Level(LevelHandler.LevelIdByName("DebugScene"));

            tile = new Tile[level.size.X, level.size.Y];//initialize the tile array

            GenerateWorld();

            player = new Player { currentWorld = this };
            player.Initialize();

            level.Setup(this);
        }

        public void Update()
        {
            level.Update();

            foreach (Npc npc in npcs)//updating every npc instance in the main npc list
                npc.Update();

            player.Update();

            if(!GameMain.lockCamera)
                GameMain.cameraPosition = player.position.ToPoint() - GameMain.ScreenSize.Half().ToPoint();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawTiles(spriteBatch);

            foreach (Npc npc in npcs)
                npc.Draw(spriteBatch);

            player.Draw(spriteBatch);

            level.Draw(spriteBatch);//after for special vfx, may split this into multiple methods as needed
        }

        private void DrawTiles(SpriteBatch spriteBatch)//iterates over every tile in the array and draws it
        {
            for (int i = 0; i < Size.X; i++)
            {
                for (int j = 0; j < Size.Y; j++)
                {
                    tile[i, j].Draw(spriteBatch, i, j);
                }
            }
        }



        public bool IsTileInWorld(Point tileCoordPoint)
        {
            if (tileCoordPoint.X >= 0 && tileCoordPoint.Y >= 0 && tileCoordPoint.X < tile.GetLength(0) && tileCoordPoint.Y < tile.GetLength(1))
            {
                return true;
            }
            return false;
        }

        public void GenerateWorld()
        {
            ClearWorld();//sets everything to air
            level.Worldgen(this);
        }

        public void ClearWorld()
        {
            FillWorld(GetTileID<Air>());//air seems to be at index 0 anyway, but to be sure
        }

        public void FillWorld(ushort type)
        {
            for (int i = 0; i < Size.X; i++)
            {
                for (int j = 0; j < Size.Y; j++)
                {
                    tile[i, j] = new Tile(type);
                }
            }
        }

        public void PlaceTile(ushort type, Vector2 position)
        {
            if (IsTileInWorld(position.ToPoint()))
            {
                tile[(int)position.X, (int)position.Y] = new Tile(type);
            }
        }

        public void PlaceTile(ushort type, Point position)
        {
            if (IsTileInWorld(position))
            {
                tile[position.X, position.Y] = new Tile(type);
            }
        }

        public void PlaceTile(ushort type, int posX, int posY)
        {
            if (IsTileInWorld(new Point(posX, posY)))
            {
                tile[posX, posY] = new Tile(type);
            }
        }
    }
}