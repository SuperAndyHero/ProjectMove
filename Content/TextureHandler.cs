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
using System.Collections.Generic;
using System.Reflection;
using ProjectMove.Content.Tiles.TileTypes;
using System.Linq;
using ProjectMove.Content.Levels;
using ProjectMove.Content.Projectiles;

namespace ProjectMove.Content
{
    public static class TextureHandler
    {
        public static Dictionary<string, Texture2D> TextureDict;//initalized in TileHandler; the same place this is set
        public static void Initialize() => TextureDict = new Dictionary<string, Texture2D>();

        public static Texture2D GetTexture(string namePath) => TextureDict[namePath];
        public static void LoadTexture(string namePath)
        {
            if(!TextureDict.ContainsKey(namePath))
                TextureDict.Add(namePath, LoadHandler.LoadTexture(namePath));
        }
        public static Texture2D SafeGetTexture(string namePath)
        {
            if (!TextureDict.ContainsKey(namePath))
                LoadTexture(namePath);
            return TextureDict[namePath];
        }
    }
}