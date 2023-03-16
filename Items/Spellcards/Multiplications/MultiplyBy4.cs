using System;
using Terraria;
using Terraria.ID;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards.Multiplications
{
    public class MultiplyBy4 : CardItem
    {
        public override void SetStaticDefaults()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.Multiplication, (byte)Multiplication.MultiplyBy4, Type);
            
            DisplayName.SetDefault("Multiply by 4");
            Tooltip.SetDefault("");
        }

        public override void SetCardDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.Multiplication;
            Spell = (byte)Multiplication.MultiplyBy4;
            Variant = 0;
            Amount = 1f;
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
            // The input is the multiplication amount, so should be 2f to 5f
            float value = Amount * input > 5f ? 5f : Amount * input;
            Amount *= value;
            AddUseTime = (int)Math.Ceiling(this.AddUseTime * value);
            AddCooldown = (int)Math.Ceiling(this.AddCooldown * value);
            AddRecharge = (int)Math.Ceiling(this.AddRecharge * value);
            AddSpread *= value;
        }

        public override float GetValue()
        {
            return 4f * Amount;
        }
    }
}
