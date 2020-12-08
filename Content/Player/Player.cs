using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;
using ProjectMove.Content.Npcs;
using ProjectMove.Content.Tiles;
using System.Reflection.Metadata.Ecma335;

namespace ProjectMove.Content.Player
{
    public class Player : Entity
    {
        #region const stats
        public const int Default_Width = 16;
        public const int Default_Height = 24;

        public const float Default_MaxSpeed = 3.5f;
        public const float Default_Acceleration = 0.09f;
        public const float Default_Deceleration = 0.90f;
        public const int   Default_MaxHealth = 350;
        public const int   Default_DamageCooldown = 60;
        //public const float Default_WallDrag = 0.9f;
        public const int   Default_InvulnLength = 50;
        #endregion

        #region variable player stats
        //all this stuff gets set when the player is created
        //these may get updated in a UpdateStats method, or replaced with properties, these are just here to it will be easier to change later
        public float maxSpeed = Default_MaxSpeed;
        public float acceleration = Default_Acceleration;
        public float deceleration = Default_Deceleration;
        public int   maxHealth = Default_MaxHealth;
        //public float WallDrag = ?; may just be based off decel, for icy tiles
        public int   invulnLength = Default_InvulnLength;
        #endregion

        public int health = Default_MaxHealth;
        public int invulnTimer = 0;

        public bool IsInvuln
        {
            get => invulnTimer > 0 ? true : false;
            set {
                if (value)
                    invulnTimer = invulnLength;
                else
                    invulnTimer = 0; }
        }

        public void Initialize(World world)
        {
            currentWorld = world;

            size = new Vector2(Default_Width, Default_Height);

            //active = true;
        }

        private Vector2 moveDir = Vector2.Zero;

        public void Hurt(int damage)
        {
            if (!IsInvuln)
            {
                IsInvuln = true;
                health -= damage;
                if (health < 0)
                    health = 0;
            }
        }

        public void Update()
        {
            //float frameSkip = (float)gameTime.ElapsedGameTime.TotalSeconds;//frame skip
            KeyboardState keystate = Keyboard.GetState();
            moveDir = Vector2.Zero;
            
            if (keystate.IsKeyDown(Keys.W))
                moveDir += new Vector2(0, -1);
            if (keystate.IsKeyDown(Keys.S))
                moveDir += new Vector2(0, 1);
            if (keystate.IsKeyDown(Keys.A))
                moveDir += new Vector2(-1, 0);
            if (keystate.IsKeyDown(Keys.D))
                moveDir += new Vector2(1, 0);

            if (IsInvuln)
                invulnTimer--;

            //update speed/accel modifiers here or earlier
            if (moveDir != Vector2.Zero)
            {
                float moveDist = (maxSpeed * acceleration);// * frameSkip;//not sure if frameskip is needed, all framerate accounting for will need to be changed
                velocity += (Vector2.Normalize(moveDir) * moveDist);

                if (velocity.Length() > maxSpeed)
                {
                    velocity = Vector2.Normalize(velocity) * maxSpeed;
                }

                Direction = GetDirection();//updated here since it may be used other places in the game
            }
            else if (velocity != Vector2.Zero)
            {
                velocity = Vector2.Normalize(velocity) * (velocity.Length() * deceleration);
            }

            //float steps = (float)Math.Ceiling(velocity.Length() / 2);
            //for (int e = 0; e < steps; e++)
            //{
                //position += (velocity / steps);
            position += velocity;

            TileCollisions();
            //}

            if (Math.Abs(velocity.X) < 0.1f)//if velocity is below an amount set it to zero
            {
                velocity.X = 0;
            }

            if (Math.Abs(velocity.Y) < 0.1f)//if velocity is below an amount set it to zero
            {
                velocity.Y = 0;
            }

            UpdateFrame();

            oldPosition = position;
        }

        public byte Direction = 0;
        private byte GetDirection()
        {
            byte dirX = (byte)(moveDir.X < 0 ? 3 : 2);
            byte dirY = (byte)(moveDir.Y < 0 ? 1 : 0);
            return Math.Abs(moveDir.Y) >= Math.Abs(moveDir.X) ? dirY : dirX;
        }

        public void UpdateFrame()//this could be called from Draw() or Update()
        {
            frame.X = Direction * FrameWidth;//direction is updated in Update() with the movement code
            if (moveDir != Vector2.Zero)
                frame.Y = ((GameMain.mainUpdateCount / 8) % 4) * FrameHeight;
            else
                frame.Y = 0;
        }


        public const int FrameCountX = 4;
        public const int FrameCountY = 4;
        public int FrameWidth { get { return GameMain.playerTexture.Width / FrameCountX; } }
        public int FrameHeight { get { return GameMain.playerTexture.Height / FrameCountY; } }
        public Point FrameSize { get { return new Point(FrameWidth, FrameHeight); } }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle playerFrame = new Rectangle(frame, FrameSize);
            //sprite
            Color playerColor = new Color(1f, 1f, 1f, IsInvuln ? 0f : 1f);
            spriteBatch.Draw(GameMain.playerTexture, Center.WorldToScreenCoords(), playerFrame, playerColor, default, FrameSize.ToVector2() / 2, GameMain.spriteScaling, default, default);

            //health text
            string healthStr = health.ToString();
            Vector2 textSize = GameMain.font_Arial_Bold.MeasureString(healthStr);
            spriteBatch.DrawString(GameMain.font_Arial_Bold, healthStr, Rect.Center.WorldToScreenCoords(), Color.White, default, new Vector2(textSize.X / 2, textSize.Y + size.Y / 2), 1f, default, default);

            //debug
            if (GameMain.debug)
            {
                //velocity text
                spriteBatch.DrawString(GameMain.font_Arial, velocity.ToString(), position.WorldToScreenCoords(), Color.LightGoldenrodYellow, default, FrameSize.ToVector2() / 2 - new Vector2(20, 0), 1f, default, default);//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
                
                //hitbox
                spriteBatch.Draw(GameMain.debugTexture, Rect.WorldToScreenCoords(), Color.Red);

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        Point tilePos = Center.WorldToTileCoords() + new Point(i, j);
                        if (currentWorld.IsTileInWorld(tilePos))
                        {
                            spriteBatch.Draw(GameMain.debugTexture, new Rectangle((Center.WorldToTileCoords() + new Point(i, j)).TileToScreenCoords(), new Point(TileHandler.tileSize)), new Color(0, j * 16, i * 16));
                        }  
                    }
                }
            }
        }
    }
}