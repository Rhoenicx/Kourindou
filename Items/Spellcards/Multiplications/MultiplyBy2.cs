using System;
using Terraria;
using Terraria.ID;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards.Multiplications
{
    public class MultiplyBy2 : CardItem
    {
        public override void SetStaticDefaults()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.Multiplication, (byte)Multiplication.MultiplyBy2, Type);
        }

        public override void SetCardDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.Multiplication;
            Spell = (byte)Multiplication.MultiplyBy2;
            Variant = 0;
            Amount = 1f;
            Value = 2f;
            AddUseTime = 0;
            AddCooldown = 0;
            AddRecharge = 0;
            AddSpread = 0f;
            FixedAngle = 0f;
            IsRandomCard = false;
            IsConsumable = false;
            NeedsProjectileCard = false;

            // Card Color
            CardColor = CardColors.Red;

            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;
        }

        public override void ApplyMultiplication(float input)
        {
            Amount *= input;
            AddUseTime = (int)Math.Ceiling(this.AddUseTime * input);
            AddCooldown = (int)Math.Ceiling(this.AddCooldown * input);
            AddRecharge = (int)Math.Ceiling(this.AddRecharge * input);
            AddSpread *= input;
        }
    }
}
