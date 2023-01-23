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
    public class Gensokyo_Stone_Fairy_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stone Fairy Plushie");
            Tooltip.SetDefault("");
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
            Item.createTile = TileType<Gensokyo_Stone_Fairy_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Gensokyo_Stone_Fairy_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieUpdateEquips(Player player)
        {
            if (Kourindou.GensokyoLoaded)
            {
                player.npcTypeNoAggro[Kourindou.Gensokyo_Fairy_Stone_Type] = true;
            }
        }
        public override string AddEffectTooltip()
        {
            return "Stone fairies become friendly";
        }

        public override void AddRecipes()
        {
            if (Kourindou.GensokyoLoaded)
            {
                CreateRecipe(1)
                    .AddIngredient(ItemID.StoneBlock, 12)
			        .AddIngredient(ItemType<BlackFabric>(), 2)
                    .AddIngredient(ItemType<SilverFabric>(), 1)
                    .AddIngredient(ItemType<YellowFabric>(), 2)
                    .AddIngredient(ItemID.Silk, 1)
			        .AddIngredient(ItemID.BlackThread, 1)
                    .AddIngredient(ItemType<YellowThread>(), 2)
                    .AddIngredient(ItemType<SilverThread>(), 2)
			        .AddIngredient(ItemType<WhiteThread>(), 1)
                    .AddRecipeGroup("Kourindou:Stuffing", 5)
                    .AddTile(TileType<SewingMachine_Tile>())
                    .Register();
            }
        }
		
		public override bool? CanBurnInLava()
		{
			return false;
		}
    }
}