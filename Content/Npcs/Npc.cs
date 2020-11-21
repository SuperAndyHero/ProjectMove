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
using ProjectMove;
using static ProjectMove.GameID;

namespace ProjectMove.Content.Npcs
{
    #region npc handler
    public static class NpcHandler//the stuff that could be in Npc or Main, but isnt for the sake of being clean
    {
        public const int MaxNpcs = 200;
        public static List<NpcBase> Bases;//static list of npc bases, copied(?) from when each npc is created

        public static Texture2D[] NpcTexture;

        public static void Initialize()
        {
            Bases = new List<NpcBase>();

            NpcID = new Dictionary<Type, ushort>();

            List<Type> TypeList = Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(NpcBase)) && t.Namespace == "ProjectMove.Content.Npcs.NpcTypes")
                      .ToList();

            for (ushort i = 0; i < TypeList.Count; i++)
            {
                Type type = TypeList[i];

                Bases.Add((NpcBase)Activator.CreateInstance(type));
                NpcID.Add(type, i);
            }
        }

        public static void LoadNpcTextures()
        {
            Bases.LoadObjectTextures(ref NpcTexture, "Npcs/");
        }

        [Obsolete("TODO: make a world-side version of this")]
        public static void SpawnNpc(ref World world, ushort type, Vector2 position, Vector2 velocity)
        {
            if (world.npcs.Count <= MaxNpcs)
            {
                Npc thisNpc = new Npc
                {
                    type = type,
                    position = position,
                    velocity = velocity,
                    npcBase = Bases[type]
                };
                thisNpc.Initialize();

                world.npcs.Insert(0, thisNpc);
                //Npcs[index].Initialize();//cant find correct index after adding, so its initalized before
                //maybe replace with array to solve this...
            }
        }
    }
    #endregion

    #region npc base
    //this class is tightly coupled to the npc class, and is instanced alongside every npc instance
    public abstract class NpcBase : DefaultBase //its is the "brain" of the npc
    {
        //public NpcBase Instance { get; set; }

        public Npc npc;

        public virtual void Setup() { }

        public virtual void AI() { }

        public virtual bool Draw(SpriteBatch spriteBatch) { return true; }
    }
    #endregion

    #region npc class
    public class Npc : Entity
    {
        public NpcBase npcBase;

        public ushort type;//the type, set when this is spawned    (this could be changed to a readonly by adding a ctor to this class, minor change)

        public int maxHealth;//the health cap

        public int health;//the current health

        public int damage;//the amount of damage this npc does //unused

        public string displayName;//name to be seen in-game, set in the npc base else gets autoset to the class name 

        public void Initialize()//this is a seperate initalize method, levels use a ctor for this instead
        {
            if (npcBase == null)//if no npc base, gets the npc base of this type
                npcBase = NpcHandler.Bases[type];

            npcBase.npc = this;//sets the npc instance on the npc base side
            npcBase.Setup();//setup methods on the npc base

            if (displayName == null)//if the display name was not set in the npc base this falls back to the class-name
                displayName = NpcHandler.Bases[type].GetType().Name;

            health = maxHealth;//spawns at max health   Add a post-setup method later here to get around this

            active = true;//is this active, this has no checks but later will be checked in drawing and will be used for removing npcs
        }

        public void Update()//could be disabled by making AI return a bool?
        {
            npcBase.AI();//the ai is on the npc bases side

            //standard stuff for every npc

            //check collision here?
            position += velocity;//updating

            if (velocity.Length() < 0.1)//if velocity is below an amount set it to zero
            {
                velocity = Vector2.Zero;
            }
            //increase health by regen rate, once that exists as a value (for natural regen and regen effects?)
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Vector2 spriteOffset = GameMain.debugTexture.Size() / 2;

            //sprite
            if (npcBase.Draw(spriteBatch))
            {
                spriteBatch.Draw(NpcHandler.NpcTexture[type], Rect.WorldToScreenCoords(), Color.White);
            }

            //debug
            if (GameMain.debug)
            {
                //hitbox
                spriteBatch.Draw(GameMain.debugTexture, Rect.WorldToScreenCoords(), Color.Blue);
            }

            //health
            string healthStr = health.ToString();
            Vector2 textSize = GameMain.font_Arial_Bold.MeasureString(healthStr);
            spriteBatch.DrawString(GameMain.font_Arial_Bold, healthStr, Rect.Center.WorldToScreenCoords(), Color.White, default, new Vector2(textSize.X / 2, textSize.Y + size.Y / 2), 1, default, default);

            //position drawn in center of screen
            //spriteBatch.DrawString(GameMain.font_Arial, position.ToString(), GameMain.ScreenSize.Center(), Color.LightGoldenrodYellow, default, Vector2.Zero, 1, default, default);

            //name text (moved to cursor)
            //Vector2 textSize2 = GameMain.font_Arial.MeasureString(displayName);
            //spriteBatch.DrawString(GameMain.font_Arial, displayName, Rect.Center.ToScreenCoords(), Color.LightGoldenrodYellow, default, new Vector2(textSize2.X / 2, -textSize2.Y + size.Y / 2), 1, default, default);
        }
    }
    #endregion
}