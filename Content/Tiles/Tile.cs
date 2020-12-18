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
using ProjectMove.Content.Npcs.NpcTypes;
using ProjectMove;
using ProjectMove.Content.Tiles.TileTypes;
using static ProjectMove.GameID;
using ProjectMove.Content.Tiles.TileTypes.Walls;
using ProjectMove.Content.Tiles.TileTypes.Floors;
using ProjectMove.Content.Tiles.TileTypes.Objects;

namespace ProjectMove.Content.Tiles
{
    public static class TileHandler
    {
        public const int tileSize = 32;//2X sprite size, because of text issues at lower resolutions
        public const string tileTextureLocation = "Tiles/Textures/";

        public static List<WallBase> WallBases;//static list, read directly from by tiles since nothing needs to be stored per-tile
        public static List<ObjectBase> ObjectBases;
        public static List<FloorBase> FloorBases;

        public static Texture2D[] WallTexture;
        public static Texture2D[] WallSideTexture;
        public static Texture2D[] WallBottomTexture;

        public static Texture2D[] ObjectTexture;
        public static Texture2D[] FloorTexture;

        public static Texture2D Outline;

        public enum TileLayer
        {
            Object,
            Wall,
            Floor
        }

        public static int FloorTypeCount => FloorBases.Count() - 1;
        public static int WallTypeCount => WallBases.Count() - 1;
        public static int ObjectTypeCount => ObjectBases.Count() - 1;

        public static int TypeCount(int layer)
        {
            return layer switch
            {
                (int)TileLayer.Floor => FloorTypeCount,
                (int)TileLayer.Wall => WallTypeCount,
                _ => ObjectTypeCount,
            };
        }

        public static ushort GetAirTile(int layer)
        {
            return layer switch
            {
                (int)TileLayer.Floor => GetFloorID<AirFloor>(),
                (int)TileLayer.Wall => GetWallID<AirWall>(),
                _ => GetObjectID<AirObject>(),
            };
        }

        public static void Initialize()
        {
            WallBases = new List<WallBase>();
            ObjectBases = new List<ObjectBase>();
            FloorBases = new List<FloorBase>();

            WallID = new Dictionary<Type, ushort>();
            ObjectID = new Dictionary<Type, ushort>();
            FloorID = new Dictionary<Type, ushort>();

            //tiles can be at any location, but the textures must be in tiles/textures
            List<Type> TypeList = Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.IsClass && !t.IsAbstract && t.Namespace.Contains("ProjectMove.Content.Tiles.TileTypes"))
                      .ToList();

            ushort objectCount = 0;
            ushort wallCount = 0;
            ushort floorCount = 0;

            foreach (Type type in TypeList)
            {
                if (type.IsSubclassOf(typeof(ObjectBase)))
                {
                    ObjectBases.Add((ObjectBase)Activator.CreateInstance(type));
                    ObjectID.Add(type, objectCount);
                    objectCount++;
                }
                else if (type.IsSubclassOf(typeof(WallBase)))//would have used a switch case if a could
                {
                    WallBases.Add((WallBase)Activator.CreateInstance(type));
                    WallID.Add(type, wallCount);
                    wallCount++;
                }
                else if(type.IsSubclassOf(typeof(FloorBase)))
                {
                    FloorBases.Add((FloorBase)Activator.CreateInstance(type));
                    FloorID.Add(type, floorCount);
                    floorCount++;
                }
            }
        }

        public static void LoadTileTextures()
        {
            WallBases.LoadWallTextures(ref WallTexture, ref WallBottomTexture, ref WallSideTexture);
            ObjectBases.LoadObjectTextures(ref ObjectTexture, tileTextureLocation);
            FloorBases.LoadObjectTextures(ref FloorTexture, tileTextureLocation);
            Outline = LoadHandler.LoadTexture(tileTextureLocation + "Outline");
        }
    }


    public abstract class TileBase : MainBase
    {
        public virtual bool IsSolid() => true;

        public virtual bool Draw(SpriteBatch spriteBatch, int i, int j) { return true; }

        public virtual void PostDraw(SpriteBatch spriteBatch, int i, int j) { }

        /// <summary>
        /// rect position is relative to tile position
        /// </summary>
        /// <returns></returns>
        /// 
        public virtual Rectangle[] CollisionRect() { return new Rectangle[1] { DefaultRect }; }

        /// <summary>
        /// Only used when Draw() returns true
        /// </summary>
        /// <returns></returns>
        public virtual Rectangle DrawRect() { return DefaultRect; }

        /// <summary>
        /// Name of the texture to load, uses class name if null
        /// </summary>
        /// <returns></returns>

        public Rectangle DefaultRect { get => new Rectangle(Point.Zero, new Point(TileHandler.tileSize)); }
    }



    public abstract class WallBase : TileBase
    {
        /// <summary>
        /// if this base should load the side textures
        /// </summary>
        /// <returns></returns>
        public virtual bool HasEdges() => true;
        public virtual bool DrawSides(SpriteBatch spriteBatch, int i, int j) => HasEdges();
        public virtual bool DrawBottom(SpriteBatch spriteBatch, int i, int j) => HasEdges();
        //public virtual bool DrawOutline(SpriteBatch spriteBatch, int i, int j) => true;
        //public virtual Color OutlineColor() => Color.Black;
        public virtual string BottomTextureName() { return null; }
        public virtual string SideTextureName() { return null; }

    }

    public abstract class ObjectBase : TileBase
    {
    }

    public abstract class FloorBase : TileBase
    {
        public virtual new bool IsSolid() => false;
    }


    public struct ObjectTile
    {
        public ObjectTile(ushort tileType = 0) { type = tileType; }
        public ObjectBase Base { get => TileHandler.ObjectBases[type]; }
        public ushort type;
        public void Draw(SpriteBatch spriteBatch, int i, int j)
        {
            if (Base.Draw(spriteBatch, i, j))//if this tile should be drawn
            {
                Rectangle rect1 = Base.DrawRect();//gets the draw rect, defaults to the collision rect unless overridden 
                Vector2 drawPos = (rect1.Location.ToVector2() + (new Vector2(i, j) * TileHandler.tileSize));
                Vector2 drawSize = (rect1.Size.ToVector2() / TileHandler.tileSize);
                spriteBatch.Draw(TileHandler.ObjectTexture[type], drawPos.WorldToScreenCoords(), null, Color.White, default, default, drawSize * GameMain.spriteScaling, default, default);
            }
        }
        public void PostDraw(SpriteBatch spriteBatch, int i, int j) =>
            Base.PostDraw(spriteBatch, i, j);
    }

    public struct WallTile
    {
        public WallTile(ushort tileType = 0) 
        { 
            type = tileType;
            borderingEmpty = true;
        }
        public WallBase Base { get => TileHandler.WallBases[type]; }
        public ushort type;

        private bool borderingEmpty;

        public void Draw(SpriteBatch spriteBatch, int i, int j)
        {
            if (Base.Draw(spriteBatch, i, j))//if this tile should be drawn
            {
                Rectangle rect1 = Base.DrawRect();
                Vector2 drawPos = (rect1.Location.ToVector2() + (new Vector2(i, j) * TileHandler.tileSize));
                Vector2 drawSize = (rect1.Size.ToVector2() / TileHandler.tileSize);
                spriteBatch.Draw(TileHandler.WallTexture[type], drawPos.WorldToScreenCoords(), null, Color.White, default, default, drawSize * GameMain.spriteScaling, default, default);            
            }
        }

        //public void DrawOutline(SpriteBatch spriteBatch, int i, int j)
        //{
        //    if(Base.DrawOutline(spriteBatch, i, j))
        //    {
        //        Point drawPos = (Base.DrawRect().Location + new Point(i, j).MultBy(TileHandler.tileSize));
        //        spriteBatch.Draw(TileHandler.Outline, drawPos.WorldToScreenCoords() - (Vector2.One * GameMain.spriteScaling), null, Base.OutlineColor(), default, default, GameMain.spriteScaling, default, default);
        //    }
        //}

        public void DrawBottom(SpriteBatch spriteBatch, int i, int j)
        {
            if (Base.DrawBottom(spriteBatch, i, j) && borderingEmpty)
            {
                Rectangle rect1 = Base.DrawRect();//gets the draw rect
                Point drawPos = ((rect1.Location + new Point(0, rect1.Size.Y)) + (new Point(i, j).MultBy(TileHandler.tileSize)));
                Point drawSize = new Point(rect1.Size.X, (int)(TileHandler.WallBottomTexture[type].Height * GameMain.spriteScaling));
                spriteBatch.Draw(TileHandler.WallBottomTexture[type], new Rectangle(drawPos, drawSize).WorldToScreenCoords(), Color.White);
            }
        }

        public void DrawSides(SpriteBatch spriteBatch, int i, int j)
        {
            if (Base.DrawSides(spriteBatch, i, j) && borderingEmpty)
            {
                Rectangle rect1 = Base.DrawRect();//gets the draw rect
                Point drawPos = (rect1.Location + new Point(i, j).MultBy(TileHandler.tileSize));
                Point drawSize = (TileHandler.WallSideTexture[type].Size() * GameMain.spriteScaling).ToPoint();

                spriteBatch.Draw(TileHandler.WallSideTexture[type], new Rectangle(drawPos, drawSize).WorldToScreenCoords(), null, Color.White, default, new Vector2(TileHandler.WallSideTexture[type].Width, 0), default, default);
                spriteBatch.Draw(TileHandler.WallSideTexture[type], new Rectangle(drawPos, drawSize).WorldToScreenCoords(), null, Color.White, default, new Vector2(-rect1.Size.X / 2, 0), SpriteEffects.FlipHorizontally, default);
            }
        }

        public void PostDraw(SpriteBatch spriteBatch, int i, int j) =>
            Base.PostDraw(spriteBatch, i, j);
    }

    public struct FloorTile
    {
        public FloorTile(ushort tileType = 0) { type = tileType; }
        public FloorBase Base { get => TileHandler.FloorBases[type]; }
        public ushort type;
        public void Draw(SpriteBatch spriteBatch, int i, int j)
        {
            if (Base.Draw(spriteBatch, i, j))//if this tile should be drawn
            {
                Rectangle rect1 = Base.DrawRect();//gets the draw rect, defaults to the collision rect unless overridden 
                Vector2 drawPos = (rect1.Location.ToVector2() + (new Vector2(i, j) * TileHandler.tileSize));
                Vector2 drawSize = (rect1.Size.ToVector2() / TileHandler.tileSize);
                spriteBatch.Draw(TileHandler.FloorTexture[type], drawPos.WorldToScreenCoords(), null, Color.LightGray, default, default, drawSize * GameMain.spriteScaling, default, default);
            }
        }
        public void PostDraw(SpriteBatch spriteBatch, int i, int j) =>
            Base.PostDraw(spriteBatch, i, j);
    }
}