using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;
using System.CodeDom.Compiler;

namespace ProjectMove.Content
{
    public abstract class Entity
    {
        public int index = 0;
        public Vector2 position       = new Vector2();
        
        public Vector2 Center {
            get { return position + size.Half(); }
            set { position = Center - size.Half(); }}

        public Vector2 velocity       = new Vector2();
        public Vector2 oldPosition    = new Vector2();
        public Vector2 oldVelocity    = new Vector2();
        public Vector2 size           = new Vector2();
        public Vector2 spriteOffset   = new Vector2();

        public Rectangle Rect {
            get { return new Rectangle(position.ToPoint(), size.ToPoint()); }
            set { position = Rect.Location.ToVector2(); size = Rect.Size.ToVector2(); }}

        public bool active = false;
    }
}