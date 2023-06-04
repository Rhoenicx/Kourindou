using Kourindou.Projectiles;
using System;
using Terraria;
using Terraria.ID;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards.ProjectileModifiers
{
    public class Luminescence : CardItem
    {
        public override void SetStaticDefaults()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.ProjectileModifier, (byte)ProjectileModifier.Luminescence, Type);
        }

        public override void SetCardDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.ProjectileModifier;
            Spell = (byte)ProjectileModifier.Luminescence;
            Variant = 0;
            Amount = 1f;
            Value = 0.5f;
            AddUseTime = 0;
            AddCooldown = 0;
            AddRecharge = 0;
            AddSpread = 0f;
            FixedAngle = 0f;
            IsRandomCard = false;
            IsConsumable = false;
            NeedsProjectileCard = true;

            // Card Color
            CardColor = CardColors.Blue;

            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;
        }

        public override float GetValue(bool max = false)
        {
            return (float)Math.Pow(Value, Amount);
        }

        public override void ExecuteCard(ref SpellCardProjectile proj)
        {
            if (proj.Light == 0f)
            {
                proj.Light = GetValue();
            }
            else
            {
                proj.Light *= GetValue();
            }
        }
    }
}
