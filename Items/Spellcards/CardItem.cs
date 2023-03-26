using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using ReLogic.Content;
using static Terraria.ModLoader.ModContent;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards
{
    public abstract class CardItem : ModItem
    {
        //----- Card properties -----//

        // Group of this card
        public byte Group = (byte)Groups.Empty;

        // Spell of this card
        public byte Spell = 0;

        // Variant of this card
        public byte Variant = 0;

        // Amount of times this card has been duplicated.
        public float Amount = 1f;

        // UseTime that this card adds to the catalyst
        public int AddUseTime = 0;

        // Cooldown that this card adds to the catalyst
        public int AddCooldown = 0;

        // Recharge that this card adds to the catalyst
        public int AddRecharge = 0;

        // Spread that this card adds to the catalyst
        public float AddSpread = 0f;

        // Angle of this card, used for formation scatter angles
        public float FixedAngle = 0f;

        // If this card needs to be replaced by another card
        public bool IsRandomCard = false;

        // If this card is a cunsumable card
        public bool IsConsumable = false;

        // If this card needs a projectile to work
        public bool NeedsProjectileCard = false;

        //----- Casting properties -----//

        // If this card has been inserted
        public bool IsInsertedCard = false;

        // If this card is currently contained in a wrap-around action
        public bool IsWrapped = false;

        // If this card is an always-cast card
        public bool IsAlwaysCast = false;

        // If this card is currently considered as payload for a trigger
        public bool IsPayload = false;

        // If this card has been copied for an multi-cast operation
        public bool IsMulticasted = false;

        // Position of this card on the catalyst slots
        public int SlotPosition = 0;

        //----- Card Textures -----//
        protected CardColors CardColor = 0;
        private Asset<Texture2D> BlueCardBack;
        private Asset<Texture2D> BrownCardBack;
        private Asset<Texture2D> GreenCardBack;
        private Asset<Texture2D> LightBlueCardBack;
        private Asset<Texture2D> PurpleCardBack;
        private Asset<Texture2D> RedCardBack;
        private Asset<Texture2D> WhiteCardBack;

        public CardItem()
        { 
            
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;

            BlueCardBack ??= Request<Texture2D>("Kourindou/Items/Spellcards/CardBack/BlueCard");
            BrownCardBack ??= Request<Texture2D>("Kourindou/Items/Spellcards/CardBack/BrownCard");
            GreenCardBack ??= Request<Texture2D>("Kourindou/Items/Spellcards/CardBack/GreenCard");
            LightBlueCardBack ??= Request<Texture2D>("Kourindou/Items/Spellcards/CardBack/LightBlueCard");
            PurpleCardBack ??= Request<Texture2D>("Kourindou/Items/Spellcards/CardBack/PurpleCard");
            RedCardBack ??= Request<Texture2D>("Kourindou/Items/Spellcards/CardBack/RedCard");
            WhiteCardBack ??= Request<Texture2D>("Kourindou/Items/Spellcards/CardBack/WhiteCard");

            float Width = (float)Math.Cos((Item.timeSinceItemSpawned / 60f) * MathHelper.Pi) * texture.Width;

            if (Width < 0f)
            {
                Width *= -1;

                switch (CardColor)
                {
                    case CardColors.Blue: texture = BlueCardBack.Value; break;
                    case CardColors.Brown: texture = BrownCardBack.Value; break;
                    case CardColors.Green: texture = GreenCardBack.Value; break;
                    case CardColors.LightBlue: texture = LightBlueCardBack.Value; break;
                    case CardColors.Purple: texture = PurpleCardBack.Value; break;
                    case CardColors.Red: texture = RedCardBack.Value; break;
                    case CardColors.White: texture = WhiteCardBack.Value; break;
                    default: texture = BlueCardBack.Value; break;
                }
            }

            int CopyAmount = 5;
            for (int i = 0; i < CopyAmount; i++)
            {
                spriteBatch.Draw(
                texture,
                Item.Center - Main.screenPosition + new Vector2(4f, 0f).RotatedBy(MathHelper.ToRadians(Item.timeSinceItemSpawned + (360 / CopyAmount * i))),
                texture.Bounds,
                new Color(140, 120, 255, 77),
                rotation,
                texture.Size() * 0.5f,
                new Vector2(Width / texture.Width, 1f),
                SpriteEffects.None,
                0);
            }

            spriteBatch.Draw(
                texture,
                Item.Center - Main.screenPosition,
                texture.Bounds,
                lightColor,
                rotation,
                texture.Size() * 0.5f,
                new Vector2(Width / texture.Width, 1f),
                SpriteEffects.None,
                0);

            return false;
        }

        public override ModItem Clone(Item newEntity)
        {
            if (newEntity.ModItem is CardItem clone)
            {
                clone.Group = this.Group;
                clone.Spell = this.Spell;
                clone.Variant = this.Variant;
                clone.Amount = this.Amount;
                clone.AddUseTime = this.AddUseTime;
                clone.AddCooldown = this.AddCooldown;
                clone.AddRecharge = this.AddRecharge;
                clone.AddSpread = this.AddSpread;
                clone.FixedAngle = this.FixedAngle;
                clone.IsRandomCard = this.IsRandomCard;
                clone.IsConsumable = this.IsConsumable;
                clone.NeedsProjectileCard = this.NeedsProjectileCard;
                clone.CardColor = this.CardColor;
                clone.IsInsertedCard = this.IsInsertedCard;
                clone.IsWrapped = this.IsWrapped;
                clone.IsAlwaysCast = this.IsAlwaysCast;
                clone.IsPayload = this.IsPayload;
                clone.IsMulticasted = this.IsMulticasted;
                clone.SlotPosition = this.SlotPosition;
            }

            return base.Clone(newEntity);
        }

        public abstract void SetCardDefaults();

        public override void SetDefaults()
        {
            SetCardDefaults();

            IsInsertedCard = false;
            IsAlwaysCast = false;
            IsWrapped = false;
            IsPayload = false;
            IsMulticasted = false;
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

        public virtual void ExecuteCard(ref ProjectileInfo info)
        { 
        
        }

        //public virtual void ExecuteCardOnInstance(ref SpellcardProjectile proj)
        //{ 
        //
        //}

        public enum CardColors
        { 
            Blue,
            Brown,
            Green,
            LightBlue,
            Purple,
            Red,
            White
        }
    }
}