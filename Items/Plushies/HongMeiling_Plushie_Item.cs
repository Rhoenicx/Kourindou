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
    public class HongMeiling_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hong Meiling Plushie");
            Tooltip.SetDefault("The scarlet mansion's gatekeeper.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Green;

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
            item.createTile = TileType<HongMeiling_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }
        
        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<HongMeiling_Plushie_Projectile>();
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

            // Increase melee speed by 10 percent
            player.meleeSpeed += 0.10f;

            // Increase melee critrate by 10 percent
            player.meleeCrit += 10;

            if (player.velocity.Length() < 0.1f)
            {
                // Increase life regen by 20 additional points
                player.lifeRegen += 20;

                // add debuffs because you're not moving!
                player.AddBuff(BuffID.Dazed, 60);
                player.AddBuff(BuffID.Slow, 60);
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.GoldCoin, 1);
            recipe.AddIngredient(ItemType<GreenFabric>(), 2);
            recipe.AddIngredient(ItemType<RedFabric>(), 2);
            recipe.AddIngredient(ItemID.Silk, 2);
            recipe.AddIngredient(ItemID.GreenThread, 2);
            recipe.AddIngredient(ItemType<RedThread>(), 1);
            recipe.AddIngredient(ItemType<YellowThread>(), 2);
            recipe.AddIngredient(ItemType<WhiteThread>(), 2);
            recipe.AddRecipeGroup("Kourindou:Stuffing", 5);
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}