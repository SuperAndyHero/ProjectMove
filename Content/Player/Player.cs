using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using System;
using ProjectMove.Content.Npcs;
using ProjectMove.Content.Tiles;

namespace ProjectMove.Content.Player
{
    public class Player : Entity
    {
        #region const stats

        public const int Default_Width = 16;
        public const int Default_Height = 32;

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
        public bool entityCollide = true;
        public bool noHeal = false;
        #endregion

        #region other const values
        private const int HealthOpacityMin = 64;
        #endregion

        //! Stats that will not be replaced later
        public int health = Default_MaxHealth;
        public int invulnTimer = 0;

        /// <summary>
        /// setting this to true starts the invuln timer
        /// </summary>
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

        public void Hurt(int amount)
        {
            if (!IsInvuln)
            {
                IsInvuln = true;
                health -= amount;
                healthOpacity = 255;
                health = MathHelper.Clamp(health, 0, maxHealth);
            }
        }

        public void Heal(int amount)
        {
            if (!noHeal)
            {
                health += amount;
                healthOpacity = 255;
                health = MathHelper.Clamp(health, 0, maxHealth);
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
            else if(healthOpacity > 64) {
                healthOpacity -= 5;
                if (healthOpacity < HealthOpacityMin)
                    healthOpacity = HealthOpacityMin; }

            //update speed/accel modifiers here or earlier

            //if a direction is being held
            if (moveDir != Vector2.Zero)
            {
                float moveDist = (maxSpeed * acceleration);
                velocity += (Vector2.Normalize(moveDir) * moveDist);

                if (velocity.Length() > maxSpeed)
                {
                    velocity = Vector2.Normalize(velocity) * maxSpeed;
                }

                Direction = GetDirection(moveDir);//set here since it will get used elsewhere
            }
            //else decelerate
            else if (velocity != Vector2.Zero)
            {
                velocity = Vector2.Normalize(velocity) * (velocity.Length() * deceleration);
            }

            #region old velocity/steps code
            //float steps = (float)Math.Ceiling(velocity.Length() / 2);
            //for (int e = 0; e < steps; e++)
            //{
            //position += (velocity / steps);
            //this would include tile collisions and updating position
            #endregion
            //update position based on velocity
            position += velocity;

            //check tile collisions
            TileCollisions();

            if (Math.Abs(velocity.X) < 0.1f)
                velocity.X = 0;
            if (Math.Abs(velocity.Y) < 0.1f)
                velocity.Y = 0;

            //update the npc frame
            UpdateFrame();

            oldPosition = position;
        }

        #region frames and direction
        //GetDirection() was moved to the entity side, these may as well
        public byte Direction = 0;//public since it may be used elsewhere

        public void UpdateFrame()
        {
            frame.X = Direction * FrameWidth;//Direction is set in Update
            if (moveDir != Vector2.Zero)
                WalkingFrame();
            else
                IdleFrame();
        }

        private void WalkingFrame()
        {
            frame.Y = ((GameMain.mainUpdateCount / 8) % 4) * FrameHeight;
        }

        private void IdleFrame()
        {
            frame.Y = 0;
        }

        public const int FrameCountX = 4;
        public const int FrameCountY = 4;
        public int FrameWidth { get { return GameMain.playerTexture.Width / FrameCountX; } }
        public int FrameHeight { get { return GameMain.playerTexture.Height / FrameCountY; } }
        public Point FrameSize { get { return new Point(FrameWidth, FrameHeight); } }
        #endregion

        #region drawing
        private int healthOpacity = HealthOpacityMin;
        private Color PlayerColor {
            get {
                if (IsInvuln) {
                    float level = ((float)Math.Sin(GameMain.mainUpdateCount / 2) / 2) + 0.5f;
                    return new Color(level, level, level, level); }
                else
                    return Color.White; } 
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle playerFrame = new Rectangle(frame, FrameSize);
            //sprite

            spriteBatch.Draw(GameMain.playerTexture, new Vector2(Center.X, position.Y).WorldToScreenCoords() - new Vector2(0, (FrameSize.Y * 2 - size.Y)), playerFrame, PlayerColor, default, new Vector2(FrameSize.X / 2, 0), GameMain.spriteScaling, default, default);

            //health text
            string healthStr = health.ToString();
            Vector2 textSize = GameMain.font_Arial_Bold.MeasureString(healthStr);
            spriteBatch.DrawString(GameMain.font_Arial_Bold, healthStr, Rect.Center.WorldToScreenCoords(), new Color(healthOpacity, healthOpacity, healthOpacity, healthOpacity), default, new Vector2(textSize.X / 2, -textSize.Y + size.Y / 3), 1f, default, default);

            //debug
            if (GameMain.debug)
            {
                //velocity text
                spriteBatch.DrawString(GameMain.font_Arial, velocity.ToString(), position.WorldToScreenCoords(), Color.LightGoldenrodYellow, default, FrameSize.ToVector2() / 2 - new Vector2(20, 0), 1f, default, default);//position + new Vector2(20, 0) //new Vector2(GameMain.screenWidth / 2, GameMain.screenHeight / 2)
                
                //hitbox
                spriteBatch.Draw(GameMain.debugTexture, Rect.WorldToScreenCoords(), Color.Red);
            }
        }
        #endregion
    }
}