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
            Tooltip.SetDefault("Hakugyokurou's gardener, and Yuyuko's servant");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Green;

            // Hitbox
            Item.width = 32;
            Item.height = 32;

            // Usage and Animation
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.autoReuse = true;
            Item.useTurn = true;

            // Tile placement fields
            Item.consumable = true;
            Item.createTile = TileType<YoumuKonpaku_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
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
            player.GetDamage(DamageClass.Generic) += 0.25f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase melee speed by 50 percent
            player.GetAttackSpeed(DamageClass.Melee) += 0.5f;

            // Increase melee crit by 15 percent
            player.GetCritChance(DamageClass.Melee) += 15;

            // Increase armor penetration by 6 points
            player.GetArmorPenetration(DamageClass.Melee) += 15;

            // Half Phantom Minion
            bool petProjectileNotSpawned = player.ownedProjectileCounts[ProjectileType<YoumuKonpaku_Plushie_HalfPhantom>()] <= 0;
            if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer) 
            {
                Projectile.NewProjectile(
                    player.GetSource_Accessory(this.Item),
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
            CreateRecipe(1)
                .AddIngredient(ItemID.BlackThread, 1)
                .AddIngredient(ItemType<GreenFabric>(), 3)
                .AddIngredient(ItemType<SilverFabric>(), 2)
                .AddIngredient(ItemID.Silk, 1)
                .AddIngredient(ItemType<SilverThread>(), 2)
                .AddIngredient(ItemID.GreenThread, 2)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}