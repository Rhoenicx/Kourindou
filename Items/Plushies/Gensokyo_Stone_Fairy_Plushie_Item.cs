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
    public class Gensokyo_Stone_Fairy_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stone Fairy Plushie");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Gray;

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
            item.createTile = TileType<Gensokyo_Stone_Fairy_Plushie_Tile>();

            item.shootSpeed = 8f;

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Gensokyo_Stone_Fairy_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {

        }

        public override void AddRecipes()
        {
            if (Kourindou.GensokyoLoaded)
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemID.StoneBlock, 12);
			    recipe.AddIngredient(ItemType<BlackFabric>(), 2);
                recipe.AddIngredient(ItemType<SilverFabric>(), 1);
                recipe.AddIngredient(ItemType<YellowFabric>(), 2);
                recipe.AddIngredient(ItemID.Silk, 1);
			    recipe.AddIngredient(ItemID.BlackThread, 1);
                recipe.AddIngredient(ItemType<YellowThread>(), 2);
                recipe.AddIngredient(ItemType<SilverThread>(), 2);
			    recipe.AddIngredient(ItemType<WhiteThread>(), 1);
                recipe.AddRecipeGroup("Kourindou:Stuffing", 5);
                recipe.AddTile(TileType<SewingMachine_Tile>());
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }
		
		public override bool CanBurnInLava()
		{
			return false;
		}
    }
}