using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using static Terraria.ModLoader.ModContent;

namespace Kourindou.Items.Weapons
{
    public class KoninginDerNacht : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Koningin Der Nacht");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(0, 50, 0, 0);
            Item.rare = ItemRarityID.LightRed;

            Item.width = 84;
            Item.height = 84;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.autoReuse = true;
            Item.noUseGraphic = false;

            Item.damage = 175;
            Item.crit = 10;
            Item.knockBack = 5f;
            Item.DamageType = DamageClass.Melee;

            Item.UseSound = SoundID.Item7;
        }
    }
}
