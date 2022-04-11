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
    public class TewiInaba_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tewi Inaba Plushie");
            Tooltip.SetDefault("A very old rabbit youkai. Despite this, she's childish.");
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
            Item.createTile = TileType<TewiInaba_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true; 
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<TewiInaba_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 5 percent
            player.GetDamage(DamageClass.Generic) += 0.05f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;
            
            // 25 percent chance for all NPC's to drop double loot
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemType<PinkFabric>(), 3)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.BlackThread, 1)
                .AddIngredient(ItemID.PinkThread, 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}