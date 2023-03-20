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
            Item.width = 20;
            Item.height = 28;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.noUseGraphic = false;
            Item.useTime = 15;
            Item.useAnimation = 15;

            // Shoot
            Item.shoot = ProjectileID.Bullet;

            // Catalyst base properties
            CastAmount = 1;
            CardSlotAmount = 3;
            HasAlwaysCastCard = false;
            AlwaysCastCard = GetCardItem((byte)Groups.Empty, 0);
            ShufflingCatalyst = false;
            BaseRecharge = 10;
            BaseCooldown = 300;
        }
    }
}