﻿using Terraria;
using Terraria.ID;
using static Kourindou.KourindouSpellcardSystem;

namespace Kourindou.Items.Spellcards.Formations
{
    public class TripleFork : CardItem
    {
        public override void SetStaticDefaults()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.Formation, (byte)Formation.TripleFork, Type);
        }

        public override void SetCardDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.Formation;
            Spell = (byte)Formation.TripleFork;
            Variant = (byte)FormationVariant.Fork;
            Amount = 1f;
            Value = 3f;
            AddUseTime = 0;
            AddCooldown = 0;
            AddRecharge = 0;
            AddSpread = 0f;
            FixedAngle = 0f;
            IsRandomCard = false;
            IsConsumable = false;
            NeedsProjectileCard = true;

            // Card Color
            CardColor = CardColors.LightBlue;

            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;
        }
    }
}
