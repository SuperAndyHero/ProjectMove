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
    public abstract class EntityBase//used for everything besides levels
    {
        public virtual string TextureName() { return null; }

        //maybe move spriteOffset here has a virtual?
    }

    public abstract class Entity
    {
        public World currentWorld;

        public Vector2 position       = new Vector2();

        public Vector2 velocity       = new Vector2();
        public Vector2 oldPosition    = new Vector2();
        public Vector2 oldVelocity    = new Vector2();
        public Vector2 size           = new Vector2(16, 16);
        //public Vector2 spriteOffset   = new Vector2(); //unused atm (why keep a value for this here, that is tracked server side(?) and on every entity)

        public bool active = false;

        public Vector2 Center
        {
            get { return position + size.Half(); }
            set { position = Center - size.Half(); }
        }

        public Rectangle Rect {
            get { return new Rectangle(position.ToPoint(), size.ToPoint()); }
            set { position = Rect.Location.ToVector2(); size = Rect.Size.ToVector2(); }}

        public bool TileCollisions(float wallDrag = 0.9f)
        {
            bool collided = false;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (position == oldPosition)
                        return collided;
                    Point currentTilePos = Center.WorldToTileCoords() + new Point(i, j);
                    if (currentWorld.IsTileInWorld(currentTilePos))
                    {
                        //less pretty, but will simplify all floor creation, and should perform better
                        //later may make a method for getting the collsion rects via location and layer ID, so a for loop may be used, but this may be unnecessary
                        if (currentWorld.floorLayer[currentTilePos.X, currentTilePos.Y].Base.IsSolid())
                            Collide(currentTilePos, currentWorld.floorLayer[currentTilePos.X, currentTilePos.Y].Base.CollisionRect());

                        if (currentWorld.wallLayer[currentTilePos.X, currentTilePos.Y].Base.IsSolid())
                            Collide(currentTilePos, currentWorld.wallLayer[currentTilePos.X, currentTilePos.Y].Base.CollisionRect());

                        if (currentWorld.objectLayer[currentTilePos.X, currentTilePos.Y].Base.IsSolid())
                            Collide(currentTilePos, currentWorld.objectLayer[currentTilePos.X, currentTilePos.Y].Base.CollisionRect());
                        //not sure how much more performant this is over casting the tile object to a intermediate object that just grabs if its solid
                    }
                }
            }
            return collided;

            void Collide(Point location, Rectangle[] rectList)
            {
                foreach (Rectangle rect in rectList)
                {
                    Rectangle testRect = new Rectangle(rect.Location + location.TileToWorldCoords(), rect.Size);

                    if(testRect.Intersects(new Rectangle(new Point((int)position.X, (int)position.Y), size.ToPoint())))
                    {
                        if(position == oldPosition)
                        {
                            //TODO, compare rect centers for direction and seperate
                        }
                        else
                        {
                            collided = true;
                            if (testRect.Intersects(new Rectangle(new Point((int)position.X, (int)oldPosition.Y), size.ToPoint())))
                            {
                                position.X = oldPosition.X;
                                velocity.X = 0;
                                velocity.Y *= wallDrag;
                            }
                            if (testRect.Intersects(new Rectangle(new Point((int)oldPosition.X, (int)position.Y), size.ToPoint())))
                            {
                                position.Y = oldPosition.Y;
                                velocity.Y = 0;
                                velocity.X *= wallDrag;
                            }
                        }
                    }
                }
            }
        }
    }
}