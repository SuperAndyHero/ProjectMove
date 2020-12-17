using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;
using ProjectMove.Content.Tiles;
using ProjectMove.Content.Npcs;

namespace ProjectMove.Content
{
    public abstract class MainBase//used for Tiles, Npcs, projectiles, and particles
    {
        public virtual string TextureName() { return null; }
        public virtual string TexturePath() { return null; }
    }

    public abstract class EntityBase : MainBase//Npcs and projectiles
    {
        public virtual bool NpcInteract() => true;//can be used for physics (npcs) or oh hit effects (projectiles)
        public virtual bool TileInteract() => true;//if this should interact with tiles (activate onTileCollide)
        public virtual bool TileCollide() => true;//if this should actually collide with tiles
        public virtual bool PlayerInteract() => true;

        /// <summary>
        /// Called once per npc intersection. Return false to stop npc collisions to taking place
        /// </summary>
        /// <param name="hitNpc"></param>
        /// <returns></returns>
        public virtual bool OnNpcCollide(Npc hitNpc) => true;
        /// <summary>
        /// Called once per player intersection. Return false to stop player collisions to taking place
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual bool OnPlayerCollide(Player.Player player) => true;
        /// <summary>
        /// Called once if the player collides with any tiles
        /// </summary>
        public virtual void OnTileCollide() { }

        //?maybe move spriteOffset here as a virtual?
    }

    public abstract class Entity
    {
        public Vector2 position       = new Vector2();
        public Vector2 velocity       = new Vector2();
        public Vector2 oldPosition    = new Vector2();
        public Vector2 oldVelocity    = new Vector2();
        public Vector2 size           = new Vector2(16, 16);
        //public bool active            = false; unused

        //unimportant variables (stuff that would not be tracked by the server)
        internal World currentWorld;
        internal Point frame = Point.Zero;
        //internal Vector2 spriteOffset

        public Vector2 Center
        {
            get { return position + size.Half(); }
            set { position = Center - size.Half(); }
        }

        public Rectangle Rect {
            get { return new Rectangle(position.ToPoint(), size.ToPoint()); }
            set { position = value.Location.ToVector2(); size = value.Size.ToVector2(); }}

        public Rectangle OldRect
        {
            get { return new Rectangle(oldPosition.ToPoint(), size.ToPoint()); }
            set { oldPosition = value.Location.ToVector2(); size = value.Size.ToVector2(); }
        }

        internal byte GetDirection(Vector2 direction)
        {
            byte dirX = (byte)(direction.X < 0 ? 3 : 2);
            byte dirY = (byte)(direction.Y < 0 ? 1 : 0);
            return Math.Abs(direction.Y) >= Math.Abs(direction.X) ? dirY : dirX;
        }

        public bool TileCollisions(bool tileCollide = true, bool tileTrigger = true, float wallDrag = 0.9f)
        {
            bool collided = false;
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    //if (position == oldPosition)
                    //    return collided; disabled because may cause issues with OnCollide hooks on some tiles, enable later and try again
                    Point currentTilePos = Center.WorldToTileCoords() + new Point(i, j);
                    if (currentWorld.IsTileInWorld(currentTilePos))
                    {
                        //these are by reference iirc
                        FloorBase floorBase = currentWorld.floorLayer[currentTilePos.X, currentTilePos.Y].Base;
                        if (Collide(currentTilePos, floorBase.CollisionRect(), tileCollide ? floorBase.IsSolid() : false) && tileTrigger)
                        {
                            //TODO
                        }

                        WallBase wallBase = currentWorld.wallLayer[currentTilePos.X, currentTilePos.Y].Base;
                        if (Collide(currentTilePos, wallBase.CollisionRect(), tileCollide ? wallBase.IsSolid() : false) && tileTrigger)
                        {
                            //TODO
                        }

                        ObjectBase objectBase = currentWorld.objectLayer[currentTilePos.X, currentTilePos.Y].Base;
                        if (Collide(currentTilePos, objectBase.CollisionRect(), tileCollide ? objectBase.IsSolid() : false) && tileTrigger)
                        {
                            //TODO //tileBase.OnCollide(this)
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

                    if (testRect.Intersects(Rect))
                    {
                        if (solid)//if solid, so position changing stuff, else just return
                        {
                            if (testRect.Intersects(OldRect))
                            {   //anti-stuck
                                position += Vector2.Normalize((Rect.Center - testRect.Center).ToVector2()) * 2;//multiplier at the ned is the distance it moves (speed)
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