﻿using Terraria;
using Terraria.ID;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards.CatalystModifiers
{
    public class DelayCastBy2 : CardItem
    {
        public override void Load()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.CatalystModifier, (byte)CatalystModifier.DelayCastBy2, Type);
            base.Load();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Delay Cast By 2");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.CatalystModifier;
            Spell = (byte)CatalystModifier.DelayCastBy2;
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
            CardColor = CardColors.Purple;

            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;
        }
    }
}
