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
    public class ShanghaiDoll_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shanghai Doll Plushie");
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

            item.melee = true;

            // Tile placement fields
            item.consumable = true;
            item.createTile = TileType<ShanghaiDoll_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;

        }
        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<ShanghaiDoll_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void PlushieEquipEffects(Player player)
        {
            // Gravity Globe effect. Hehe.
            // Oh, maybe also a chance to reflect projectiles at low HP?
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<BlackFabric>(), 2);
            recipe.AddIngredient(ItemType<RedFabric>(), 1);
			recipe.AddIngredient(ItemType<YellowFabric>(), 1);
            recipe.AddIngredient(ItemID.Silk, 2);
            recipe.AddIngredient(ItemID.BlackThread, 2);
            recipe.AddIngredient(ItemType<RedThread>(), 1);
			recipe.AddIngredient(ItemType<YellowThread>(), 1);
            recipe.AddIngredient(ItemType<WhiteThread>(), 2);
            recipe.AddRecipeGroup("Kourindou:Stuffing", 5);
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
		
		public override bool CanBurnInLava()
		{
			return false;
		}
    }
} 