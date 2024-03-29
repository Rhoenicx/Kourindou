﻿using Kourindou.Projectiles.Spellcards;
using Terraria;
using Terraria.ID;
using static Kourindou.KourindouSpellcardSystem;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items.Spellcards.Projectiles
{
    public class ShotBlue : CardItem
    {
        public override void SetStaticDefaults()
        {
            // When loading this card, register it!
            RegisterCardItem((byte)Groups.Projectile, (byte)KourindouSpellcardSystem.Projectile.ShotBlue, Type);
        }

        public override void SetCardDefaults()
        {
            // Defaults of this card
            Group = (byte)Groups.Projectile;
            Spell = (byte)KourindouSpellcardSystem.Projectile.ShotBlue;
            Variant = 0;
            Amount = 1f;
            Value = ProjectileType<ShotBlueProj>();
            AddUseTime = 0;
            AddCooldown = 0;
            AddRecharge = 0;
            AddSpread = 0f;
            FixedAngle = 0f;
            IsRandomCard = false;
            IsConsumable = false;
            NeedsProjectileCard = false;
            CanBeMultiplied = false;

            // Card Color
            CardColor = CardColors.White;

            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;
        }
    }
}
