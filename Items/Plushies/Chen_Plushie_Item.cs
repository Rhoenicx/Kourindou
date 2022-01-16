using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Plushies
{
    public class Chen_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Chen Plushie");
            Tooltip.SetDefault("The cutest shikigami's shikigami!");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.Orange;

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
            item.createTile = TileType<Chen_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;

        }
        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Chen_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 25 percent
            player.allDamage += 0.25f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase running speed by 9 mph
            player.maxRunSpeed += 0.5f;

            // Increase movement speed by 35 percent
            player.moveSpeed += 0.35f;
			
			// On Kill effect handled in globalnpc
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod); 
            recipe.AddIngredient(ItemType<GreenFabric>(), 2); 
            recipe.AddIngredient(ItemType<SilverFabric>(), 1); 
            recipe.AddIngredient(ItemType<RedFabric>(), 2);
            recipe.AddIngredient(ItemType<BrownFabric>(), 2);
            recipe.AddIngredient(ItemType<RedThread>(), 2); 
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}