using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Kourindou.KourindouSpellcardSystem;
using System.Collections.Generic;
using Kourindou.Items.Spellcards;
using Terraria.ModLoader.IO;
using Kourindou.Projectiles.Catalysts;

namespace Kourindou.Items.Catalysts
{
    public class DebugCatalyst : CatalystItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Testing Catalyst 1");
            Tooltip.SetDefault("");
        }

        public override void SetCatalystDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 50;
            Item.height = 50;

            // Damage
            Item.damage = 10;
            Item.knockBack = 1f;
            Item.crit = 5;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.channel = true;
            Item.noMelee = true;

            // Shoot
            Item.shoot = ProjectileType<DebugCatalyst_Catalyst>();
            Item.shootSpeed = 1f;

            // Catalyst base properties
            CastAmount = 1;
            CardSlotAmount = 12;
            HasAlwaysCastCard = false;
            AlwaysCastCard = GetCardItem((byte)Groups.Empty, 0);
            ShufflingCatalyst = false;
            BaseRecharge = 10;
            BaseCooldown = 60;
            BaseDamageMultiplier = 1f;
            BaseKnockbackMultiplier = 1f;
            BaseVelocityMultiplier = 1f;
            BaseSpread = 0f;
            BaseCrit = 0;
        }
    }
}