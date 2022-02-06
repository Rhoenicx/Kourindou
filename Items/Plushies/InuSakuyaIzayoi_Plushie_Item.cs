using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class InuSakuyaIzayoi_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Inu Sakuya Izayoi Plushie");
            Tooltip.SetDefault("Does the real Sakuya like this? Probably. It's adorable, why wouldn't she?");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 9, 7, 5);
            item.rare = ItemRarityID.Cyan;

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
            item.createTile = TileType<InuSakuyaIzayoi_Plushie_Tile>();

            item.shootSpeed = 8f;

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<InuSakuyaIzayoi_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {

        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            // 1 watch
            // 2 silver cloth
            // 2 white cloth
            // 2 brown cloth
            // 1 teal cloth
            // 2 silver thread
            // 2 white thread
            // 2 teal thread
            // 5 stuffing
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}