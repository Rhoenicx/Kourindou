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
    public class MomijiInubashiri_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Momiji Inubashiri Plushie");
            Tooltip.SetDefault("The Youkai Mountain's guard dog. Calling her that might get you killed");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.LightRed;

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
            Item.createTile = TileType<MomijiInubashiri_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {            
                shootSpeed = 8f;
                projectileType = ProjectileType<MomijiInubashiri_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 10 percent
            player.GetDamage(DamageClass.Generic) += 0.10f;

            // Increase melee damage by 25 percent
            player.GetDamage(DamageClass.Melee) += 0.25f;

            // Increase melee crit by 10 points
            player.GetCritChance(DamageClass.Melee) += 10;

            // Sniper scope effect
            player.scope = true;

            // Permanent dangersense and hunter effects
            player.dangerSense = true;
            player.detectCreature = true;
        }

        public override string AddEffectTooltip()
        {
            return "Permanent dangersense and hunter buff\r\n" +
                    "+10 % damage, +25 % melee damage, +10 % melee crit";
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemType<RedFabric>(), 2)
                .AddIngredient(ItemID.Silk, 3)
                .AddIngredient(ItemID.BlackThread, 1)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}