﻿using Terraria;
using Terraria.ID;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards.ProjectileModifiers
{
    public class RotateRight45 : CardItem
    {
        public override void Load()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.ProjectileModifier, (byte)ProjectileModifier.RotateRight45, Type);
            base.Load();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rotate Right 45");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.ProjectileModifier;
            Spell = (byte)ProjectileModifier.RotateRight45;
            Variant = 0;
            Amount = 1f;
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
    }
}
