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
    public class YuyukoSaigyouji_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Yuyuko Saigyouji Plushie");
            Tooltip.SetDefault("The princess of the Netherworld.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Pink;

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
            item.createTile = TileType<YuyukoSaigyouji_Plushie_Tile>();

            item.shootSpeed = 8f;

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<YuyukoSaigyouji_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase minion knockback by 2
            player.minionKB += 2;

            // Increase sentry slots by 3
            player.maxTurrets += 3;

            // Increase minion slots by 3
            player.maxMinions += 3;

            if (player.HasBuff(BuffID.WellFed))
            {
                // Increase attack damage by 25 percent
                player.allDamage += 0.25f;

                // Increase life regen by 1 point
                player.lifeRegen += 1;

                // Increase movement speed by 30 percent
                if (player.accRunSpeed > player.maxRunSpeed)
                {
                    player.accRunSpeed *= 1.3f;
                    player.maxRunSpeed *= 1.3f;
                }
                else
                {
                    player.maxRunSpeed *= 1.3f;
                    player.accRunSpeed = player.maxRunSpeed;
                }
            }
            else
            {
                // Decrease attack damage by 25 percent
                player.allDamage -= 0.25f;

                // Decrease life regen by 1 point
                if (player.lifeRegen > 1)
                {
                    player.lifeRegen -= 1;
                }

                // Decrease movement speed by 30 percent
                if (player.accRunSpeed > player.maxRunSpeed)
                {
                    player.accRunSpeed *= 0.7f;
                    player.maxRunSpeed *= 0.7f;
                }
                else
                {
                    player.maxRunSpeed *= 0.7f;
                    player.accRunSpeed = player.maxRunSpeed;
                }
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<BlackFabric>(), 1);
            recipe.AddIngredient(ItemType<SkyBlueFabric>(), 3);
            recipe.AddIngredient(ItemType<PinkFabric>(), 2);
            recipe.AddIngredient(ItemID.Silk, 2);
            recipe.AddIngredient(ItemType<RedThread>(), 1);
            recipe.AddIngredient(ItemType<SkyBlueThread>(), 3);
            recipe.AddIngredient(ItemID.PinkThread, 2);
            recipe.AddIngredient(ItemType<WhiteThread>(), 2);
            recipe.AddRecipeGroup("Kourindou:Stuffing", 5);
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}