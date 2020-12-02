using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;
using System.CodeDom.Compiler;
using ProjectMove.Content.Tiles;

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
                    //if (position == oldPosition)
                    //    return collided;
                    Point currentTilePos = Center.WorldToTileCoords() + new Point(i, j);
                    if (currentWorld.IsTileInWorld(currentTilePos))
                    {
                        //these are by reference iirc
                        FloorBase floorBase = currentWorld.floorLayer[currentTilePos.X, currentTilePos.Y].Base;
                        if (Collide(currentTilePos, floorBase.CollisionRect(), floorBase.IsSolid()))
                        {
                            //TODO
                        }

                        WallBase wallBase = currentWorld.wallLayer[currentTilePos.X, currentTilePos.Y].Base;
                        if (Collide(currentTilePos, wallBase.CollisionRect(), wallBase.IsSolid()))
                        {
                            //TODO
                        }

                        ObjectBase objectBase = currentWorld.objectLayer[currentTilePos.X, currentTilePos.Y].Base;
                        if(Collide(currentTilePos, objectBase.CollisionRect(), objectBase.IsSolid()))
                        {
                            //TODO //objectBase.OnCollide(this)
                        }
                    }
                }
            }
            return collided;

            bool Collide(Point location, Rectangle[] rectList, bool solid)
            {
                bool hasCollided = false;
                foreach (Rectangle tileRect in rectList)
                {
                    Rectangle testRect = new Rectangle(location.TileToWorldCoords() + tileRect.Location, tileRect.Size);

                    if(testRect.Intersects(Rect))
                    {
                        if (solid)//if solid, so position changing stuff, else just return
                        {
                            if (testRect.Intersects(new Rectangle(new Point((int)oldPosition.X, (int)oldPosition.Y), size.ToPoint())))
                            {   //anti-stuck
                                position += Vector2.Normalize((Rect.Center - testRect.Center).ToVector2()) * 2;
                            }
                            else
                            {
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
                        hasCollided = true;
                    }
                }
                if (hasCollided)
                {
                    collided = true;
                }
                return hasCollided;
            }
        }
    }
}