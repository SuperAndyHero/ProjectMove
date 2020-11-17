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
        public const int tileSize = 32;
        public const string tileTextureLocation = "Tiles/Textures/";

        public static List<TileBase> TileBases;//static list, read directly from by tiles since nothing needs to be stored per-tile
        public static List<ObjectBase> ObjectBases;
        public static List<Floorbase> FloorBases;

        public static Texture2D[] TileTexture;
        public static Texture2D[] ObjectTexture;
        public static Texture2D[] FloorTexture;

        public static void Initialize()
        {
            TileBases = new List<TileBase>();
            ObjectBases = new List<ObjectBase>();
            FloorBases = new List<Floorbase>();

            TileID = new Dictionary<Type, ushort>();
            ObjectID = new Dictionary<Type, ushort>();
            FloorID = new Dictionary<Type, ushort>();

            //tiles can be at any location, but the textures must be in tiles/textures
            List<Type> TypeList = Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.Namespace.Length >= 35 && t.Namespace.Substring(0, 35)  == "ProjectMove.Content.Tiles.TileTypes" && t.IsClass && !t.IsAbstract)
                      .ToList();

            for (ushort i = 0; i < TypeList.Count; i++)
            {
                Type type = TypeList[i];
                if(type.IsSubclassOf(typeof(TileBase)))//would have used a switch case if a could
                {
                    TileBases.Add((TileBase)Activator.CreateInstance(type));
                    TileID.Add(type, i);
                }
                else if(type.IsSubclassOf(typeof(ObjectBase)))
                {
                    ObjectBases.Add((ObjectBase)Activator.CreateInstance(type));
                    ObjectID.Add(type, i);
                }
                else if(type.IsSubclassOf(typeof(Floorbase)))
                {
                    FloorBases.Add((Floorbase)Activator.CreateInstance(type));
                    FloorID.Add(type, i);
                }
            }
        }

        public static void LoadTileTextures()
        {
            TileBases.LoadObjectTextures(ref TileTexture, tileTextureLocation);
            ObjectBases.LoadObjectTextures(ref ObjectTexture, tileTextureLocation);
            FloorBases.LoadObjectTextures(ref FloorTexture, tileTextureLocation);
        }

        #region obsolete
        //banished to the comment realm
        //[Obsolete("use GameID.GetTileID<>() instead")]
        //public static ushort TileIdByName(string name)
        //{
        //    ushort index = 0;
        //    foreach (string str in TileInternalNames)
        //    {
        //        if (str == name)
        //            return index;
        //        index++;
        //    }
        //    return 0;
        //}

        [Obsolete("use IsPointInWorld on world side")]
        public static bool IsPointWithinArray(Point tileCoordPoint, ref Tile[,] array)
        {
            if (tileCoordPoint.X >= 0 && tileCoordPoint.Y >= 0 && tileCoordPoint.X < array.GetLength(0) && tileCoordPoint.Y < array.GetLength(1))
            {
                return true;
            }
            return false;
        }

        [Obsolete("This method is deprecated, use the non-static version of this method in world instead")]
        public static void PlaceTile(ref World world, ushort type, Vector2 position)
        {
            if (position.X < world.wallGrid.GetLength(0) && position.Y < world.wallGrid.GetLength(1))//is within bounds
            {
                world.wallGrid[(int)position.X, (int)position.Y] = new Tile(type);
            }
        }

        [Obsolete("This method is deprecated, use the non-static version of this method in world instead")]
        public static void PlaceTile(ref World world, ushort type, Point position)
        {
            if (position.X < world.wallGrid.GetLength(0) && position.Y < world.wallGrid.GetLength(1))//is within bounds
            {
                world.wallGrid[position.X, position.Y] = new Tile(type);
            }
        }
        #endregion
    }


    public abstract class TileBase
    {
        public virtual bool IsSolid() => true;

        public virtual bool Draw(SpriteBatch spriteBatch, int i, int j) { return true; }

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



        public Rectangle DefaultRect { get => new Rectangle(Point.Zero, new Point(TileHandler.tileSize)); }
    }


    public abstract class ObjectBase : TileBase
    {

    }

    public abstract class Floorbase : TileBase
    {

    }


    //tile object
    public struct Tile
    {
        public Tile(ushort tileType = 0) { type = tileType; }
        public TileBase Base { get => TileHandler.TileBases[type]; }

        public ushort type;

        public void Draw(SpriteBatch spriteBatch, int i, int j)//default drawing
        {
            Rectangle rect1 = Base.DrawRect();//gets the draw rect, defaults to the collision rect unless overridden 
            Rectangle tileRect = new Rectangle(rect1.Location + new Point(i, j).MultBy(TileHandler.tileSize), rect1.Size);

            if (Base.Draw(spriteBatch, i, j))//if this tile should be drawn
            {
                spriteBatch.Draw(TileHandler.TileTexture[type], tileRect.WorldToScreenCoords(), Color.White);
            }

            if (GameMain.debug)//DEBUG
            {
                //spriteBatch.Draw(GameMain.debugTexture, tileRect.WorldToScreenCoords(), new Color(i * 16, j * 16, 0));

                foreach (Rectangle rect in Base.CollisionRect())
                {
                    Rectangle collisionRect = new Rectangle(rect.Location + new Point(i, j).MultBy(TileHandler.tileSize), rect.Size);
                    spriteBatch.Draw(GameMain.debugTexture, collisionRect.WorldToScreenCoords(), new Color(i * 16, j * 16, 0));
                }

                string str = type.ToString();
                Vector2 textSize = GameMain.font_Arial_Bold.MeasureString(str);
                spriteBatch.DrawString(GameMain.font_Arial_Bold, str, tileRect.Center.WorldToScreenCoords(), Color.White, default, new Vector2(textSize.X / 2, textSize.Y / 2), 1, default, default);
            }
        }
    }
}