using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class SuwakoMoriya_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Suwako Moriya Plushie");
            Tooltip.SetDefault("One of the Moriya Shrine's gods. Handles the shrine's divine services.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Lime;

            // Hitbox
            item.width = 32;
            item.height = 32;

            // Usage and Animation
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 15;
            item.useAnimation = 15;
            item.autoReuse = true;
            item.useTurn = true;

            // Tile placement fields
            item.consumable = true;
            item.createTile = TileType<SuwakoMoriya_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<SuwakoMoriya_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Mining speed halved
            player.pickSpeed /= 2;

            // Increase reach by 10 blocks
            player.blockRange += 10;

            // Increase tile break range by 10 blocks
            if (player.whoAmI == Main.myPlayer)
            {
                Player.tileRangeX += 10;
                Player.tileRangeY += 10;
            }
        }
    }
}