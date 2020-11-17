using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;
using ProjectMove.Content.Player;
using ProjectMove.Content.Npcs;
using ProjectMove.Content.Tiles;
using System.ComponentModel;

namespace ProjectMove
{
    public static class Extensions
    {
        public static Vector2 Size(this Texture2D texture)               { return new Vector2(texture.Width, texture.Height); }

        public static Vector2 Half(this Vector2 vector)                { return vector / 2; }

        public static Point Half(this Point point)                     { return new Point(point.X / 2, point.Y / 2); }

        public static bool NextBool(this Random random)                  { return random.Next(2) == 0; }

        public static Point MultBy(this Point point, int value)          { return new Point(point.X * value, point.Y * value); }

        public static Point DivideBy(this Point point, int value)        { return new Point(point.X / value, point.Y / value); }

        #region vector control
        public static float ToRotation(this Vector2 v)                   { return (float)Math.Atan2((double)v.Y, (double)v.X); }

        public static Vector2 ToRotationVector2(this float f)            { return new Vector2((float)Math.Cos((double)f), (float)Math.Sin((double)f)); }

        public static Vector2 RotatedBy(this Vector2 spinningpoint, double radians, Vector2 center = default(Vector2))
        {
            float dirX = (float)Math.Cos(radians);
            float dirY = (float)Math.Sin(radians);
            Vector2 vector = spinningpoint - center;
            Vector2 result = center;
            result.X += vector.X * dirX - vector.Y * dirY;
            result.Y += vector.X * dirY + vector.Y * dirX;
            return result;
        }
        #endregion

        #region coordinate conversion
        ///<summary>
        ///offsets by the camera position
        ///</summary>
        public static Vector2 WorldToScreenCoords(this Point point)        { return (point - GameMain.cameraPosition).ToVector2(); }

        ///<summary>
        ///offsets by the camera position (reduces accuracy to that of Point)
        ///</summary>
        public static Vector2 WorldToScreenCoords(this Vector2 vector)     { return (vector.ToPoint() - GameMain.cameraPosition).ToVector2(); }

        ///<summary>
        ///offsets by the camera position
        ///</summary>
        public static Rectangle WorldToScreenCoords(this Rectangle rect)   { return new Rectangle(rect.Location - GameMain.cameraPosition, rect.Size); }
        


        public static Point ScreenToWorldCoords(this Point point)          { return point + GameMain.cameraPosition; }

        public static Vector2 ScreenToWorldCoords(this Vector2 vector)     { return vector + GameMain.cameraPosition.ToVector2(); }



        public static Point WorldToTileCoords(this Point point)            { return point.DivideBy(TileHandler.tileSize); }

        public static Point WorldToTileCoords(this Vector2 vector)       { return (vector / TileHandler.tileSize).ToPoint(); }



        public static Point ScreenToTileCoords(this Point point)           { return (point + GameMain.cameraPosition).DivideBy(TileHandler.tileSize); }

        public static Vector2 ScreenToTileCoords(this Vector2 vector)      { return (vector + GameMain.cameraPosition.ToVector2()) / TileHandler.tileSize; }



        public static Point TileToWorldCoords(this Point point)            { return point.MultBy(TileHandler.tileSize); }



        public static Point TileToScreenCoords(this Point point)           { return point.MultBy(TileHandler.tileSize) - GameMain.cameraPosition; }

        #endregion
    }
}