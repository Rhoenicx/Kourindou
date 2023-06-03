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
    public class LilyWhite_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Lily White Plushie");
            // Tooltip.SetDefault("Spring is here!");
        }

        public override string AddEffectTooltip()
        {
            return "In forest biome gain permanent sunflower buff and greatly increased life regen";
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Pink;

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
            Item.createTile = TileType<LilyWhite_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {            
                shootSpeed = 8f;
                projectileType = ProjectileType<LilyWhite_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<PinkFabric>(), 3)
                .AddIngredient(ItemType<YellowFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemID.PinkThread, 2)
                .AddIngredient(ItemType<YellowThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase all damage done by 5%
            player.GetDamage(DamageClass.Generic) += 0.05f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            if (!player.ZoneBeach
                && !player.ZoneCorrupt
                && !player.ZoneCrimson
                && !player.ZoneDesert
                && !player.ZoneDungeon
                && !player.ZoneGlowshroom
                && !player.ZoneHallow
                && !player.ZoneJungle
                && !player.ZoneMeteor
                && !player.ZoneSnow
                && (player.ZoneSkyHeight || player.ZoneOverworldHeight))
            {
                player.AddBuff(BuffID.Sunflower, 20);
                Main.buffNoTimeDisplay[146] = true;

                player.lifeRegen += 10;
            }
        }
    }
}