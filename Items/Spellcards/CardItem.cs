using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards
{
    public abstract class CardItem : ModItem
    {
        // Group of this card
        public byte Group; //= (byte)Groups.Empty;

        // Spell of this card
        public byte Spell; //= 0;

        // Variant of this card
        public byte Variant; //= 0;

        // Amount of times this card has been duplicated.
        public float Amount; //= 1f;

        // UseTime that this card adds to the catalyst
        public int AddUseTime; //= 0;

        // Cooldown that this card adds to the catalyst
        public int AddCooldown; //= 0;

        // Recharge that this card adds to the catalyst
        public int AddRecharge; //= 0;

        // Spread that this card adds to the catalyst
        public float AddSpread; //= 0f;

        // Angle of this card, used for formation scatter angles
        public float FixedAngle; //= 0f

        // If this card needs to be replaced by another card
        // EXCLUSIVE FOR CARDS THAT NEEDS TO BE REPLACED!!!
        // NOT FOR RANDOM VALUES!!! => ADD THOSE IN GETCARDVALUE()
        public bool IsRandomCard; //= false;

        // If this card is a cunsumable card
        public bool IsConsumable; //= false;

        // If this card needs a projectile to work
        public bool NeedsProjectileCard; //= false;

        // If this card has been inserted
        public bool IsInsertedCard;

        // Position of this card on the catalyst slots
        public int SlotPosition;

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;

            spriteBatch.Draw(
                texture,
                Item.Center - Main.screenPosition,
                texture.Bounds,
                lightColor,
                rotation,
                texture.Size() * 0.5f,
                scale,
                SpriteEffects.None,
                0);

            return false;
        }

        public virtual void ApplyMultiplication(float input)
        {
            // The input is the multiplication amount, so should be 2f to 5f
            Amount *= input;
            AddUseTime = (int)Math.Ceiling(this.AddUseTime * input);
            AddCooldown = (int)Math.Ceiling(this.AddCooldown * input);
            AddRecharge = (int)Math.Ceiling(this.AddRecharge * input);
            AddSpread *= input;
        }

        public virtual float GetValue()
        {
            return 1f * Amount;
        }
    }
}