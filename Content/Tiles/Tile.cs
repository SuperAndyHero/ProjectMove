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

        //public static Texture2D[] WallTexture;
        //public static Texture2D[] WallSideTexture;
        //public static Texture2D[] WallBottomTexture;

        //public static Texture2D[] ObjectTexture;
        //public static Texture2D[] FloorTexture;

        //public static Texture2D Outline;

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

        public static int GetAirTile(int layer)
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

            //tiles can be at any location, but the textures must be in tiles/textures
            //move all reflection to load handler
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
            foreach (WallBase wallBase in WallBases)
                wallBase.Load();
            foreach (ObjectBase objectBase in ObjectBases)
                objectBase.Load();
            foreach (FloorBase floorBase in FloorBases)
                floorBase.Load();
        }
    }

    #region bases
    public abstract class TileBase : MainBase
    {
        public string texture;
        public TileBase() => texture = TexturePath() ?? TileHandler.tileTextureLocation + (TextureName() ?? GetType().Name);
        public Rectangle DefaultRect => new Rectangle(Point.Zero, new Point(TileHandler.tileSize));
        public void DrawTile(SpriteBatch spriteBatch, int i, int j)
        {
            if (Draw(spriteBatch, i, j))//if this tile should be drawn
            {
                Rectangle rect1 = DrawRect();
                Vector2 drawPos = (rect1.Location.ToVector2() + (new Vector2(i, j) * TileHandler.tileSize));
                Vector2 drawSize = (rect1.Size.ToVector2() / TileHandler.tileSize);
                spriteBatch.Draw(TextureHandler.GetTexture(texture), drawPos.WorldToScreenCoords(), null, Color.White, default, default, drawSize * GameMain.spriteScaling, default, default);
            }
        }


        public virtual bool IsSolid() => true;
        public virtual bool Draw(SpriteBatch spriteBatch, int i, int j) { return true; }
        public virtual void PostDraw(SpriteBatch spriteBatch, int i, int j) { }
        /// <summary>
        /// rect position is relative to tile position
        /// </summary>
        /// <returns></returns>
        /// 
        public virtual Rectangle[] CollisionRect() { return new Rectangle[1] { DefaultRect }; }
        public virtual Rectangle DrawRect() => DefaultRect;
        public virtual void Load() => TextureHandler.LoadTexture(texture);
    }

    #region tile bases
    public abstract class WallBase : TileBase
    {
        public string sideTexture;
        public string bottomTexture;
        public WallBase() : base()
        {
            string path = TexturePath() ?? TileHandler.tileTextureLocation;
            sideTexture = path + (SideTextureName() ?? TextureName() ?? GetType().Name) + "_Side";
            bottomTexture = path + (SideTextureName() ?? TextureName() ?? GetType().Name) + "_Bottom";
        }
        public void DrawTileBottom(SpriteBatch spriteBatch, int i, int j)
        {
            if (DrawBottom(spriteBatch, i, j))
            {
                Rectangle rect1 = DrawRect();//gets the draw rect
                Texture2D texture = TextureHandler.GetTexture(bottomTexture);
                Point drawPos = ((rect1.Location + new Point(0, rect1.Size.Y)) + (new Point(i, j).MultBy(TileHandler.tileSize)));
                Point drawSize = new Point(rect1.Size.X, (int)(texture.Height * GameMain.spriteScaling));
                spriteBatch.Draw(texture, new Rectangle(drawPos, drawSize).WorldToScreenCoords(), Color.White);
            }
        }
        public void DrawTileSides(SpriteBatch spriteBatch, int i, int j)
        {
            if (DrawSides(spriteBatch, i, j))
            {
                Rectangle rect1 = DrawRect();//gets the draw rect
                Texture2D texture = TextureHandler.GetTexture(sideTexture);
                Point drawPos = (rect1.Location + new Point(i, j).MultBy(TileHandler.tileSize));
                Point drawSize = (texture.Size() * GameMain.spriteScaling).ToPoint();

                spriteBatch.Draw(texture, new Rectangle(drawPos, drawSize).WorldToScreenCoords(), null, Color.White, default, new Vector2(texture.Width, 0), default, default);
                spriteBatch.Draw(texture, new Rectangle(drawPos, drawSize).WorldToScreenCoords(), null, Color.White, default, new Vector2(-rect1.Size.X / 2, 0), SpriteEffects.FlipHorizontally, default);
            }
        }


        public virtual bool HasSides() => true;
        public virtual bool HasBottom() => true;
        public virtual string SideTextureName() => null;
        public virtual string BottomTextureName() => null;
        public virtual bool DrawSides(SpriteBatch spriteBatch, int i, int j) => HasSides();
        public virtual bool DrawBottom(SpriteBatch spriteBatch, int i, int j) => HasBottom();
        public virtual new void Load()
        {
            if (HasSides())
                TextureHandler.LoadTexture(sideTexture);
            if (HasBottom())
                TextureHandler.LoadTexture(bottomTexture);
            base.Load();
        }

        //public virtual bool DrawOutline(SpriteBatch spriteBatch, int i, int j) => true;
        //public virtual Color OutlineColor() => Color.Black;
        /*public void DrawOutline(SpriteBatch spriteBatch, int i, int j)
        {
            if (Base.DrawOutline(spriteBatch, i, j))
            {
                Point drawPos = (Base.DrawRect().Location + new Point(i, j).MultBy(TileHandler.tileSize));
                spriteBatch.Draw(TileHandler.Outline, drawPos.WorldToScreenCoords() - (Vector2.One * GameMain.spriteScaling), null, Base.OutlineColor(), default, default, GameMain.spriteScaling, default, default);
            }
        }*/
    }

    public abstract class ObjectBase : TileBase
    {

    }

    public abstract class FloorBase : TileBase
    {
        public virtual new bool IsSolid() => false;
    }
    #endregion
    #endregion

    #region tile objects
    public struct ObjectTile
    {
        public int type;
        public ObjectTile(int tileType = 0) => type = tileType;
        public ObjectBase Base { get => TileHandler.ObjectBases[type]; }
    }

    public struct WallTile
    {
        public int type;
        private bool borderingEmpty;
        public WallTile(int tileType = 0) 
        { 
            type = tileType;
            borderingEmpty = true;
        }
        public WallBase Base { get => TileHandler.WallBases[type]; }
    }

    public struct FloorTile
    {
        public int type;
        public FloorTile(int tileType = 0) => type = tileType;
        public FloorBase Base { get => TileHandler.FloorBases[type]; }
    }
    #endregion
}