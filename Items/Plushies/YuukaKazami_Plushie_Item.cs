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
    public class YuukaKazami_Plushie_Item : PlushieItem
    {
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
            Item.createTile = TileType<YuukaKazami_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<YuukaKazami_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }  

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<LimeFabric>(), 2)
                .AddIngredient(ItemType<RedFabric>(), 2)
                .AddIngredient(ItemType<YellowFabric>(), 1)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<LimeThread>(), 2)
                .AddIngredient(ItemType<RedThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 25 percent
            player.GetDamage(DamageClass.Generic) += 0.25f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Also some kind of dash
            player.dashType = 1;

            // Check if player has Happy! buff
            if (player.HasBuff(BuffID.Sunflower))
            {
                // Increase defense by 7 points
                player.statDefense += 7;

                // Increase max HP by 50 points
                player.statLifeMax2 += 50;

                // Increase armor penetration by 20 points
                player.GetArmorPenetration(DamageClass.Generic) += 20;

                // TODO flowe boots effect

                // Double movement speed
                if (player.accRunSpeed > player.maxRunSpeed)
                {
                    player.accRunSpeed *= 2f;
                    player.maxRunSpeed *= 2f;
                }
                else
                {
                    player.maxRunSpeed *= 2f;
                    player.accRunSpeed = player.maxRunSpeed;
                }
            }
        }
    }
}