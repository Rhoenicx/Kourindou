using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Kourindou.KourindouSpellcardSystem;
using System.Collections.Generic;

namespace Kourindou.Items.Catalysts
{
    public class Catalyst : ModItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Testing Catalyst");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.White;

            // Hitbox
            Item.width = 20;
            Item.height = 28;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            List<Card> CatalystCards = new List<Card>
            {
                new Card() { Group = (byte)Groups.Projectile, Spell = (byte)KourindouSpellcardSystem.Projectile.ShotBlue }
            };

            Cast cast = GenerateCast(CatalystCards, null, true, 0, 1, false, false);



            return !cast.FailedToCast;
        }
    }
}
