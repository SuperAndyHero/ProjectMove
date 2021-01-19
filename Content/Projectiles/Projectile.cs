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
using ProjectMove;
using static ProjectMove.GameID;

namespace ProjectMove.Content.Projectiles
{
    #region npc handler
    //TODO: make not static
    public static class ProjectileHandler
    {
        public const int MaxProjectiles = 1200;
        public static List<Type> BaseTypes;

        public static Texture2D[] ProjectileTexture;

        public static void Initialize()
        {
            BaseTypes = new List<Type>();

            List<Type> TypeList = Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ProjectileBase)) && t.Namespace == "ProjectMove.Content.Projectile.ProjectileTypes")
                      .ToList();

            for (ushort i = 0; i < TypeList.Count; i++)
            {
                Type type = TypeList[i];

                BaseTypes.Add(type);
                ProjectileID.Add(type, i);
            }
        }

        public static void LoadProjectileTextures() => BaseTypes.LoadObjectTextures(ref ProjectileTexture, "Projectiles/");
    }
    #endregion

    #region projectile base
    public abstract class ProjectileBase : EntityBase //its is the "brain" of the npc
    {
        public Projectile projectile;

        public virtual void Setup() { }

        public virtual void AI() { }

        public virtual int FrameCountX() { return 0; }
        public virtual int FrameCountY() { return 0; }

        public virtual bool Draw(SpriteBatch spriteBatch) { return true; }
    }
    #endregion

    #region projectile class
    public class Projectile : Entity
    {
        public ProjectileBase projectileBase;

        public int type;

        public int damage = 0;

        public string displayName;//maybe

        public float spriteScale = 1f;

        public void Initialize(World world)
        {
            currentWorld = world;

            if (projectileBase == null)
                projectileBase = (ProjectileBase)Activator.CreateInstance(ProjectileHandler.BaseTypes[type]);

            projectileBase.projectile = this;
            projectileBase.Setup();

            if (displayName == null)
                displayName = ProjectileHandler.BaseTypes[type].Name;

            //active = true;//is this active, this has no checks but later will be checked in drawing and will be used for removing
        }

        public void Update()//could be disabled by making AI return a bool?
        {
            projectileBase.AI();

            position += velocity;

            bool tileCollide = projectileBase.TileCollide();
            bool tileInteract = projectileBase.TileInteract();
            if (tileInteract || tileCollide)
                if (TileCollisions(tileCollide, tileInteract) && tileCollide)
                    projectileBase.OnTileCollide();

            if (velocity.Length() < 0.1)
                velocity = Vector2.Zero;

            oldPosition = position;
        }

        public int FrameWidth { 
            get {
                int count = projectileBase.FrameCountX();
                int size = ProjectileHandler.ProjectileTexture[type].Width;
                return count == 0 ? size : size / count; } }

        public int FrameHeight { 
            get {
                int count = projectileBase.FrameCountY();
                int size = ProjectileHandler.ProjectileTexture[type].Height;
                return count == 0 ? size : size / count; } }

        public Point FrameSize { get { return new Point(FrameWidth, FrameHeight); } }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Vector2 spriteOffset = GameMain.debugTexture.Size() / 2;

            //sprite
            if (projectileBase.Draw(spriteBatch))
                spriteBatch.Draw(ProjectileHandler.ProjectileTexture[type], Center.WorldToScreenCoords(), null, Color.White, default, ProjectileHandler.ProjectileTexture[type].Size() / 2, spriteScale * GameMain.spriteScaling, default, default);

            //debug
            if (GameMain.debug)
                spriteBatch.Draw(GameMain.debugTexture, Rect.WorldToScreenCoords(), Color.Blue); //hitbox

            //health
            //string healthStr = health.ToString();
            //Vector2 textSize = GameMain.font_Arial_Bold.MeasureString(healthStr);
            //spriteBatch.DrawString(GameMain.font_Arial_Bold, healthStr, Rect.Center.WorldToScreenCoords(), Color.White, default, new Vector2(textSize.X / 2, textSize.Y + size.Y / 2), 1, default, default);

            //position drawn in center of screen
            //spriteBatch.DrawString(GameMain.font_Arial, position.ToString(), GameMain.ScreenSize.Center(), Color.LightGoldenrodYellow, default, Vector2.Zero, 1, default, default);

            //name text (moved to cursor)
            //Vector2 textSize2 = GameMain.font_Arial.MeasureString(displayName);
            //spriteBatch.DrawString(GameMain.font_Arial, displayName, Rect.Center.ToScreenCoords(), Color.LightGoldenrodYellow, default, new Vector2(textSize2.X / 2, -textSize2.Y + size.Y / 2), 1, default, default);
        }
    }
    #endregion
}