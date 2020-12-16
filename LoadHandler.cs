using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System.Collections.Generic;
using System;
using ProjectMove.Content.Player;
using ProjectMove.Content.Npcs;
using ProjectMove.Content.Tiles;
using System.ComponentModel;
using ProjectMove.Content;
using System.IO;

namespace ProjectMove
{
    public static class LoadHandler
    {

        //https://discord.com/channels/103110554649894912/271236615823687680/788422550253469736
        //^ for single texture loads

        public static Texture2D LoadTexture(string location)
        {
            try
            {
                return GameMain.Instance.Content.Load<Texture2D>(location);
            }
            catch (ContentLoadException)
            {
                System.Diagnostics.Debug.WriteLine("Missing Texture: " + location);
                return GameMain.Instance.Content.Load<Texture2D>("Debug1");
            }
        }
    }
}