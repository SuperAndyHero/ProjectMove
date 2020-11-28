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
        public static Texture2D[] ObjectTexture;
        public static Texture2D[] FloorTexture;

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
            WallBases.LoadObjectTextures(ref WallTexture, tileTextureLocation);
            ObjectBases.LoadObjectTextures(ref ObjectTexture, tileTextureLocation);
            FloorBases.LoadObjectTextures(ref FloorTexture, tileTextureLocation);
        }
    }


    public abstract class TileDefaultBase : DefaultBase
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

        public static implicit operator TileDefaultBase(WallTile v)
        {
            throw new NotImplementedException();
        }
    }



    public abstract class WallBase : TileDefaultBase
    {
        public virtual int C() { return 9; }
    }

    public abstract class ObjectBase : TileDefaultBase
    {
        public virtual int B() { return 7; }
    }

    public abstract class FloorBase : TileDefaultBase
    {
        public virtual new bool  IsSolid() => false;
        public virtual int A() { return 5; }
    }


    public struct ObjectTile
    {
        public ObjectTile(ushort tileType = 0) { type = tileType; }
        public ObjectBase Base { get => TileHandler.ObjectBases[type]; }
        public ushort type;
        public void Draw(SpriteBatch spriteBatch, int i, int j)
        {//default drawing
            if (Base.Draw(spriteBatch, i, j))
            {//if this tile should be drawn
                Rectangle rect1 = Base.DrawRect();//gets the draw rect, defaults to the collision rect unless overridden 
                Vector2 drawPos = (rect1.Location.ToVector2() + (new Vector2(i, j) * TileHandler.tileSize))/* * GameMain.zoom*/;
                Vector2 drawSize = (rect1.Size.ToVector2() / TileHandler.tileSize)/* * GameMain.zoom*/;
                spriteBatch.Draw(TileHandler.ObjectTexture[type], drawPos.WorldToScreenCoords(), null, Color.White, default, default, drawSize * GameMain.spriteScaling, default, default);
            }
        }
        public void PostDraw(SpriteBatch spriteBatch, int i, int j){
            Base.PostDraw(spriteBatch, i, j);
            if (GameMain.debug){//DEBUG
                Rectangle rect1 = Base.DrawRect();//gets the draw rect, defaults to the collision rect unless overridden 
                Rectangle tileRect = new Rectangle(rect1.Location + new Point(i, j).MultBy(TileHandler.tileSize), rect1.Size);
                if (Base.IsSolid()){
                    foreach (Rectangle rect in Base.CollisionRect()){
                        Rectangle collisionRect = new Rectangle(rect.Location + new Point(i, j).MultBy(TileHandler.tileSize), rect.Size);
                        spriteBatch.Draw(GameMain.debugTexture, collisionRect.WorldToScreenCoords(), new Color(i * 16, j * 16, 0));
                    }
                }
                string str = type.ToString();
                Vector2 textSize = GameMain.font_Arial_Bold.MeasureString(str);
                spriteBatch.DrawString(GameMain.font_Arial_Bold, str, (tileRect.Center + new Point(TileHandler.tileSize / 4)).WorldToScreenCoords(), Color.Red, default, textSize / 2, 1f, default, default);
            }
        }
    }

    public struct WallTile
    {
        public WallTile(ushort tileType = 0) { type = tileType; }
        public WallBase Base { get => TileHandler.WallBases[type]; }
        public ushort type;
        public void Draw(SpriteBatch spriteBatch, int i, int j){//default drawing
            if (Base.Draw(spriteBatch, i, j)){//if this tile should be drawn
                Rectangle rect1 = Base.DrawRect();//gets the draw rect, defaults to the collision rect unless overridden 
                Vector2 drawPos = (rect1.Location.ToVector2() + (new Vector2(i, j) * TileHandler.tileSize))/* * GameMain.zoom*/;
                Vector2 drawSize = (rect1.Size.ToVector2() / TileHandler.tileSize)/* * GameMain.zoom*/;
                spriteBatch.Draw(TileHandler.WallTexture[type], drawPos.WorldToScreenCoords(), null, Color.White, default, default, drawSize * GameMain.spriteScaling, default, default);
            }
        }
        public void PostDraw(SpriteBatch spriteBatch, int i, int j) {
            Base.PostDraw(spriteBatch, i, j);
            if (GameMain.debug){//DEBUG
                Rectangle rect1 = Base.DrawRect();//gets the draw rect, defaults to the collision rect unless overridden 
                Rectangle tileRect = new Rectangle(rect1.Location + new Point(i, j).MultBy(TileHandler.tileSize), rect1.Size);
                if (Base.IsSolid()){
                    foreach (Rectangle rect in Base.CollisionRect()){
                        Rectangle collisionRect = new Rectangle(rect.Location + new Point(i, j).MultBy(TileHandler.tileSize), rect.Size);
                        spriteBatch.Draw(GameMain.debugTexture, collisionRect.WorldToScreenCoords(), new Color(i * 16, j * 16, 0));
                    }
                }
                string str = type.ToString();
                Vector2 textSize = GameMain.font_Arial_Bold.MeasureString(str);
                spriteBatch.DrawString(GameMain.font_Arial_Bold, str, tileRect.Center.WorldToScreenCoords(), Color.Blue, default, textSize / 2, 1f, default, default);
            }
        }
    }

    public struct FloorTile
    {
        public FloorTile(ushort tileType = 0) { type = tileType; }
        public FloorBase Base { get => TileHandler.FloorBases[type]; }
        public ushort type;
        public void Draw(SpriteBatch spriteBatch, int i, int j)
        {//default drawing
            if (Base.Draw(spriteBatch, i, j))
            {//if this tile should be drawn
                Rectangle rect1 = Base.DrawRect();//gets the draw rect, defaults to the collision rect unless overridden 
                Vector2 drawPos = (rect1.Location.ToVector2() + (new Vector2(i, j) * TileHandler.tileSize))/* * GameMain.zoom*/;
                Vector2 drawSize = (rect1.Size.ToVector2() / TileHandler.tileSize)/* * GameMain.zoom*/;
                spriteBatch.Draw(TileHandler.FloorTexture[type], drawPos.WorldToScreenCoords(), null, Color.LightGray, default, default, drawSize * GameMain.spriteScaling, default, default);
            }
        }
        public void PostDraw(SpriteBatch spriteBatch, int i, int j){
            Base.PostDraw(spriteBatch, i, j);
            if (GameMain.debug){//DEBUG
                Rectangle rect1 = Base.DrawRect();//gets the draw rect, defaults to the collision rect unless overridden 
                Rectangle tileRect = new Rectangle(rect1.Location + new Point(i, j).MultBy(TileHandler.tileSize), rect1.Size);
                if (Base.IsSolid()){
                    foreach (Rectangle rect in Base.CollisionRect()){
                        Rectangle collisionRect = new Rectangle(rect.Location + new Point(i, j).MultBy(TileHandler.tileSize), rect.Size);
                        spriteBatch.Draw(GameMain.debugTexture, collisionRect.WorldToScreenCoords(), new Color(i * 16, j * 16, 0));
                    }
                }
                string str = type.ToString();
                Vector2 textSize = GameMain.font_Arial_Bold.MeasureString(str);
                spriteBatch.DrawString(GameMain.font_Arial_Bold, str, (tileRect.Center - new Point(TileHandler.tileSize / 4)).WorldToScreenCoords(), Color.Green, default, textSize / 2, 1f, default, default);
            }
        }
    }
}