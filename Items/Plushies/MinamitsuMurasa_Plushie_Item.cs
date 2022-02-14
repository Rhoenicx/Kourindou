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
    public class MinamitsuMurasa_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Minamitsu Murasa Plushie");
            Tooltip.SetDefault("A phantom, and the Palanquin Ship's captain.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 5, 0, 0);
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
            item.createTile = TileType<MinamitsuMurasa_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {            
                shootSpeed = 8f;
                projectileType = ProjectileType<MinamitsuMurasa_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 10 percent
            player.allDamage += 0.10f;

            // Increase Life regen by 1 point
            player.lifeRegen += 1;

            // Increase breathmax by 200 points (double)
            player.breathMax += 200;

            // Act like flipper accessory
            player.accFlipper = true;

            // Makes Fishron mount speed permanent
            if (player.mount.Type == MountID.CuteFishron && player.mount.Active)
            {
                player.MountFishronSpecialCounter = 300;
            }

            // Boosted stats in water
            if ((player.wet || player.honeyWet) && !player.lavaWet)
            {
                // Additional 10 percent damage
                player.allDamage += 0.10f;

                // Increase all crit by 5 points
                player.meleeCrit += 5;
                player.magicCrit += 5;
                player.thrownCrit += 5;
                player.rangedCrit += 5;

                //Increase life regen by 5 points
                player.lifeRegen += 5;
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<BlackFabric>(), 2);
            recipe.AddIngredient(ItemType<BlueFabric>(), 2);
            recipe.AddIngredient(ItemType<YellowFabric>(), 1);
            recipe.AddIngredient(ItemID.Silk, 2);
            recipe.AddIngredient(ItemID.BlackThread, 2);
            recipe.AddIngredient(ItemType<BlueThread>(), 2);
            recipe.AddIngredient(ItemType<RedThread>(), 1);
            recipe.AddIngredient(ItemType<WhiteThread>(), 2);
            recipe.AddRecipeGroup("Kourindou:Stuffing", 5);
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}