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
    public class Rumia_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Gray;

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
            Item.createTile = TileType<Rumia_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Rumia_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemType<RedFabric>(), 1)
                .AddIngredient(ItemType<YellowFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.BlackThread, 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<YellowThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 25 percent
            player.GetDamage(DamageClass.Generic) += 0.05f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Grant infinite flight time, sneaky thing here: just putting the elapsed flight time back to 1. 
            // Now only gets applied if the player has the obstructed debuff since it could be cleared by other mods
            if (player.HasBuff(BuffID.Obstructed))
            {
                player.wingTime += 1;
            }

            // Permanently afflicted with obstructed debuff
            player.AddBuff(BuffID.Obstructed, 60, false);
        }
    }
}