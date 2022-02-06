using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Projectiles.Plushies.PlushieEffects;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Plushies
{
    public class YoumuKonpaku_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Youmu Konpaku Plushie");
            Tooltip.SetDefault("Hakugyokurou's gardener, and Yuyuko's servant.");
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
            item.createTile = TileType<YoumuKonpaku_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<YoumuKonpaku_Plushie_Projectile>();
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

            // Increase melee speed by 50 percent
            player.meleeSpeed += 0.5f;

            // Increase melee crit by 15 percent
            player.meleeCrit += 15;

            // Increase armor penetration by 6 points
            player.armorPenetration += 6;

            // Half Phantom Minion
            bool petProjectileNotSpawned = player.ownedProjectileCounts[ProjectileType<YoumuKonpaku_Plushie_HalfPhantom>()] <= 0;
            if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer) 
            {
                Projectile.NewProjectile(
                    player.Center,
                    Vector2.Zero,
                    ProjectileType<YoumuKonpaku_Plushie_HalfPhantom>(),
                    player.statLifeMax2 / 2,
                    5f,
                    Main.myPlayer,
                    0,
					0
				);
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.BlackThread, 1);
            recipe.AddIngredient(ItemType<GreenFabric>(), 3);
            recipe.AddIngredient(ItemType<SilverFabric>(), 2);
            recipe.AddIngredient(ItemID.Silk, 1);
            recipe.AddIngredient(ItemType<SilverThread>(), 2);
            recipe.AddIngredient(ItemID.GreenThread, 2);
            recipe.AddIngredient(ItemType<WhiteThread>(), 1);
            recipe.AddRecipeGroup("Kourindou:Stuffing", 5);
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}