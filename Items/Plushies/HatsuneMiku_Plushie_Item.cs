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
    public class HatsuneMiku_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hatsune Miku Plushie");
            Tooltip.SetDefault("The voice of the future, in pixel plushie form!\n"
									+ "Fires leeks from the heavens if powered.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(3, 9, 3, 9);
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
            item.createTile = TileType<HatsuneMiku_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;

        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<HatsuneMiku_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player) 
        {
            // Increase damage by 39 percent [placeholder]
            player.allDamage += 0.39f;

            // Increase damage reduction by 39 percent [placeholder]
            player.endurance += 0.39f;
        }
        
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<BlackFabric>(), 2);
            recipe.AddIngredient(ItemType<TealFabric>(), 3);
            recipe.AddIngredient(ItemID.Silk, 2);
            recipe.AddIngredient(ItemID.BlackThread, 2);
            recipe.AddIngredient(ItemType<RedThread>(), 1);
            recipe.AddIngredient(ItemType<TealThread>(), 2);
            recipe.AddIngredient(ItemType<WhiteThread>(), 1);
            recipe.AddRecipeGroup("Kourindou:Stuffing", 5);
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}