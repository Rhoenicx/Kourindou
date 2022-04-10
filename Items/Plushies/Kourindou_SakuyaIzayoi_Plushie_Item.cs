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
    public class Kourindou_SakuyaIzayoi_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sakuya Izayoi Plushie Kourindou ver.");
            Tooltip.SetDefault("The maid of the scarlet mansion.");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.White;

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
            Item.createTile = TileType<Kourindou_SakuyaIzayoi_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Kourindou_SakuyaIzayoi_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            if (player.whoAmI == Main.myPlayer && player.ItemAnimationMax -1 == player.ItemAnimation)
            {
                // Increase damage by 5 percent
                player.GetDamage(DamageClass.Generic) += 0.05f;

                // Increase life regen by 1 point
                player.lifeRegen += 1;

                // Increase Throwing damage by 40 percent
                player.thrownDamage += 0.40f;

                // Spawn 4 knifes on regular attack animations
			    for (int i = 0; i < 4; i++)
			    {
			    	Projectile.NewProjectile(
			    		player.Center,
			    		Vector2.Normalize(Main.MouseWorld - player.Center).RotatedBy(MathHelper.ToRadians(i >= 2 ? 5 * (i - 1) : -5 * (i + 1))) * (player.HeldItem.shootSpeed > 0f ? player.HeldItem.shootSpeed : 8f),
			    		ProjectileType<SakuyaIzayoi_Plushie_Knife>(),
			    		10 + (int)(player.statLifeMax2 / 15),
			    		1f,
			    		Main.myPlayer
			    	);
			    }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("Kourindou:Watch",1)
                .AddIngredient(ItemType<BlueFabric>(), 1)
                .AddIngredient(ItemType<SilverFabric>(), 2)
                .AddIngredient(ItemID.Silk, 3)
                .AddIngredient(ItemType<BlueThread>(), 1)
                .AddIngredient(ItemType<SilverThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .SetResult(this)
                .Register();
        }
    }
}