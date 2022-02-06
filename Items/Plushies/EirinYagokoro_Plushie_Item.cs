using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class EirinYagokoro_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eirin Yagokoro Plushie");
            Tooltip.SetDefault("The genius of the Moon.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.White;

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
            item.createTile = TileType<EirinYagokoro_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }
        
        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<EirinYagokoro_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 25 percent
            player.allDamage += 0.25f;

            // Life regen increased by 10 points
            player.lifeRegen += 10;

            // Arrow Damage increased by 10 percent
            player.arrowDamage += 0.1f;

            // Max life increased by 50 points
            player.statLifeMax2 += 50;

            // Reduce potion delay times by 35%
            player.potionDelayTime = (int)((double)player.potionDelayTime * 0.65);
            player.restorationDelayTime = (int)((double)player.restorationDelayTime * 0.65);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            // 2 silver cloth
            // 2 red cloth
            // 2 blue cloth
            // 1 red thread
            // 1 blue thread
            // 1 silver thread
            // 5 Stuffing
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}