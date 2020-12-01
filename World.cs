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
using ProjectMove.Content.Tiles.TileTypes.Walls;
using ProjectMove.Content.Levels.LevelTypes;
using static ProjectMove.GameID;
using ProjectMove.Content.Tiles.TileTypes.Floor;
using ProjectMove.Content.Tiles.TileTypes.Objects;

namespace ProjectMove
{
    //global stuff
    public class World //a static instance of this is made in gamemain
    {
        public LevelBase level;

        public Player player;

        public List<Npc> npcs;

        public Point Size
        {
            get => level.Size();
        }

        public ObjectTile[,] objectLayer;

        public WallTile[,] wallLayer;

        public FloorTile[,] floorLayer;

        public enum TileLayer
        {
            Object,
            Wall,
            Floor
        }

        public void Initialize()
        {
            npcs = new List<Npc>();

            level = (LevelBase)Activator.CreateInstance(LevelHandler.BaseTypes[GetLevelID<DebugScene>()]);

            objectLayer = new ObjectTile[Size.X, Size.Y];
            wallLayer = new WallTile[Size.X, Size.Y];
            floorLayer = new FloorTile[Size.X, Size.Y];

            GenerateWorld();

            player = new Player();
            player.Initialize(this);

            level.Setup(this);
        }


        public void Update()
        {
            level.Update();

            for (int i = 0; i < npcs.Count; i++)//updating every npc instance in the main npc list
            {
                npcs[i].Update();
            }

            player.Update();

            if(!GameMain.lockCamera)
                GameMain.cameraPosition = (player.position.ToPoint() - GameMain.ScreenSize.Half())/*.MultBy(GameMain.zoom)*/;
        }


        public void DrawTiles(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Size.X; i++) {
                for (int j = 0; j < Size.Y; j++) {
                    floorLayer[i, j].Draw(spriteBatch, i, j); } }

            for (int i = 0; i < Size.X; i++) {
                for (int j = 0; j < Size.Y; j++) {
                    wallLayer[i, j].Draw(spriteBatch, i, j); } }

            for (int i = 0; i < Size.X; i++) {
                for (int j = 0; j < Size.Y; j++) {
                    objectLayer[i, j].Draw(spriteBatch, i, j); } }
        }

        public void DrawEntities(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < npcs.Count; i++)//updating every npc instance in the main npc list
            {
                npcs[i].Draw(spriteBatch);
            }

            player.Draw(spriteBatch);
        }

        public void PostDrawTiles(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Size.X; i++) {
                for (int j = 0; j < Size.Y; j++) {
                    floorLayer[i, j].PostDraw(spriteBatch, i, j); } }

            for (int i = 0; i < Size.X; i++) {
                for (int j = 0; j < Size.Y; j++) {
                    wallLayer[i, j].PostDraw(spriteBatch, i, j); } }

            for (int i = 0; i < Size.X; i++) {
                for (int j = 0; j < Size.Y; j++) {
                    objectLayer[i, j].PostDraw(spriteBatch, i, j); } }
        }

        public void ExtraDraw(SpriteBatch spriteBatch)
        {
            level.Draw(spriteBatch);//after for special vfx, may split this into multiple methods as needed
        }


        public Npc SpawnNpc(ushort type, Vector2 position, Vector2 velocity)
        {
            if (npcs.Count <= NpcHandler.MaxNpcs)
            {
                npcs.Insert(0, new Npc
                {
                    type = type,
                    position = position,
                    velocity = velocity,
                });
                npcs[0].Initialize(this);
                return npcs[0];
            }
            return null;
        }

        public bool IsTileInWorld(Point tileCoordPoint)
        {
            if (tileCoordPoint.X >= 0 && tileCoordPoint.Y >= 0 && tileCoordPoint.X < Size.X && tileCoordPoint.Y < Size.Y)
            {
                return true;
            }
            return false;
        }

        public bool IsTileInWorld(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < Size.X && y < Size.Y)
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
            FillLayer(GetFloorID<AirFloor>(), (int)TileLayer.Floor);//air seems to be at index 0 anyway, but to be sure
            FillLayer(GetWallID<AirWall>(), (int)TileLayer.Wall);
            FillLayer(GetObjectID<AirObject>(), (int)TileLayer.Object);
        }

        public void FillLayer(ushort type, int layer)
        {
            for (int i = 0; i < Size.X; i++)
            {
                for (int j = 0; j < Size.Y; j++)
                {
                    PlaceTile(type, i, j, layer);
                }
            }
        }

        public void PlaceTile(ushort type, Vector2 position, int layer)
        {
            PlaceTile(type, (int)position.X, (int)position.Y, layer);
        }

        public void PlaceTile(ushort type, Point position, int layer)
        {
            PlaceTile(type, position.X, position.Y, layer);
        }

        public void PlaceTile(ushort type, int posX, int posY, int layer)
        {
            if (IsTileInWorld(posX, posY))
            {
                switch (layer)
                {
                    case (int)TileLayer.Floor:
                        floorLayer[posX, posY] = new FloorTile(type);
                        break;
                    case (int)TileLayer.Wall:
                        wallLayer[posX, posY] = new WallTile(type);
                        break;
                    case (int)TileLayer.Object:
                        objectLayer[posX, posY] = new ObjectTile(type);
                        break;
                }
            }
        }
    }
}