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
using ProjectMove.Content.Npcs.NpcTypes;
using ProjectMove.Content.Player;
using ProjectMove;
using static ProjectMove.GameID;

namespace ProjectMove.Content.Npcs
{
    #region npc handler
    public static class NpcHandler//the stuff that could be in Npc or Main, but isnt for the sake of being clean
    {
        public const int MaxNpcs = 200;
        public static List<Type> BaseTypes;

        public static Texture2D[] NpcTexture;

        public static void Initialize()
        {
            BaseTypes = new List<Type>();

            NpcID = new Dictionary<Type, ushort>();

            List<Type> TypeList = Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(NpcBase)) && t.Namespace == "ProjectMove.Content.Npcs.NpcTypes")
                      .ToList();

            for (ushort i = 0; i < TypeList.Count; i++)
            {
                Type type = TypeList[i];

                BaseTypes.Add(type);
                NpcID.Add(type, i);
            }
        }

        public static void LoadNpcTextures() => BaseTypes.LoadObjectTextures(ref NpcTexture, "Npcs/");
    }
    #endregion

    #region npc base
    //this class is tightly coupled to the npc class, and is instanced alongside every npc instance
    public abstract class NpcBase : EntityBase //its is the "brain" of the npc
    {
        //public NpcBase Instance { get; set; }

        public Npc npc;

        public virtual void Setup() { }

        public virtual void AI() { }

        public virtual int FrameCountX() { return 0; }
        public virtual int FrameCountY() { return 0; }

        public virtual bool Draw(SpriteBatch spriteBatch) { return true; }
    }
    #endregion

    #region npc class
    public class Npc : Entity
    {
        public NpcBase npcBase;

        public ushort type;//the type, set when this is spawned    (this could be changed to a readonly by adding a ctor to this class, minor change)

        public int damage = 0;//the amount of damage this npc does //unused

        public int maxHealth = 10;//the health cap (here instead on virtual on the base to enemies of the same type can vary in max health

        public int health;//the current health

        public bool Invuln = false;

        public string displayName;//name to be seen in-game, set in the npc base else gets autoset to the class name 

        public float spriteScale = 1f;

        public void Initialize(World world)//this is a seperate initalize method, levels use a ctor for this instead
        {
            currentWorld = world;

            if (npcBase == null)//if no npc base, gets the npc base of this type
                npcBase = (NpcBase)Activator.CreateInstance(NpcHandler.BaseTypes[type]);

            npcBase.npc = this;//sets the npc instance on the npc base side
            npcBase.Setup();//setup methods on the npc base

            if (displayName == null)//if the display name was not set in the npc base this falls back to the class-name
                displayName = NpcHandler.BaseTypes[type].Name;

            health = maxHealth;//spawns at max health   Add a post-setup method later here to get around this

            //active = true;//is this active, this has no checks but later will be checked in drawing and will be used for removing npcs
        }

        public void Update()//could be disabled by making AI return a bool?
        {
            npcBase.AI();//the ai is on the npc bases side

            //standard stuff for every npc
            position += velocity;//updating

            if (npcBase.NpcInteract())
                EntityCollisions();
            if (npcBase.PlayerInteract())
                PlayerContact();

            bool tileCollide = npcBase.TileCollide();
            bool tileInteract = npcBase.TileInteract();
            if (tileInteract || tileCollide)
                if (TileCollisions(tileCollide, tileInteract) && tileCollide)
                    npcBase.OnTileCollide();

            if (velocity.Length() < 0.1)//if velocity is below an amount set it to zero
                velocity = Vector2.Zero;
            //increase health by regen rate, once that exists as a value (for natural regen and regen effects?)

            oldPosition = position;
        }

        private void EntityCollisions()//only gets called if the npc has entity collisions enabled
        {
            foreach (Npc curNpc in currentWorld.npcs)
            {
                //TODO check active here
                if (curNpc.npcBase.NpcInteract() && Rect.Intersects(curNpc.Rect) && Rect.Center != curNpc.Rect.Center)//good for ~200 npcs, if this is disabled can do thousands
                {
                    if (npcBase.OnNpcCollide(curNpc))
                    {
                        Vector2 difference = Vector2.Normalize((Rect.Center - curNpc.Rect.Center).ToVector2());
                        //Vector2 difference = (Rect.Center - a.Rect.Center).ToVector2() / 16;//performance difference negligible
                        position += difference / ((Rect.Width + Rect.Height) / 32);
                        curNpc.position -= difference;
                    }
                }
            }
        }

        private void PlayerContact()
        {
            if (Rect.Intersects(currentWorld.player.Rect))
            {
                if(damage > 0)
                    currentWorld.player.Hurt(damage);
                else if (damage < 0)
                    currentWorld.player.Heal(damage);

                if (npcBase.OnPlayerCollide(currentWorld.player) && Rect.Center != currentWorld.player.Rect.Center)
                {
                    Vector2 difference = Vector2.Normalize((Rect.Center - currentWorld.player.Rect.Center).ToVector2());
                    position += difference / ((Rect.Width + Rect.Height) / 32);
                    currentWorld.player.position -= difference;
                }
            }
        }

        public int FrameWidth { 
            get {
                int count = npcBase.FrameCountX();
                int size = NpcHandler.NpcTexture[type].Width;
                return count == 0 ? size : size / count; } }

        public int FrameHeight { 
            get {
                int count = npcBase.FrameCountY();
                int size = NpcHandler.NpcTexture[type].Height;
                return count == 0 ? size : size / count; } }

        public Point FrameSize { get { return new Point(FrameWidth, FrameHeight); } }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Vector2 spriteOffset = GameMain.debugTexture.Size() / 2;

            //sprite
            if (npcBase.Draw(spriteBatch))
                spriteBatch.Draw(NpcHandler.NpcTexture[type], Center.WorldToScreenCoords(), null, Color.White, default, NpcHandler.NpcTexture[type].Size() / 2, spriteScale * GameMain.spriteScaling, default, default);

            //debug
            if (GameMain.debug)
                spriteBatch.Draw(GameMain.debugTexture, Rect.WorldToScreenCoords(), Color.Blue); //hitbox
        }

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