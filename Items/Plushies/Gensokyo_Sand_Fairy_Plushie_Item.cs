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
    public class Gensokyo_Sand_Fairy_Plushie_Item : PlushieItem
    {
        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Yellow;

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
            Item.createTile = TileType<Gensokyo_Sand_Fairy_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Gensokyo_Sand_Fairy_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.SandBlock, 12)
                .AddIngredient(ItemID.Waterleaf, 1)
                .AddIngredient(ItemType<YellowFabric>(), 2)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemID.Silk, 1)
			    .AddIngredient(ItemID.BlackThread, 1)
                .AddIngredient(ItemType<YellowThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            if (Kourindou.GensokyoLoaded)
            {
                player.npcTypeNoAggro[Kourindou.Gensokyo_Fairy_Sand_Type] = true;
            }
        }

        public override bool PlushieCanBeHitByNPC(Player myPlayer, NPC npc, ref int cooldownSlot, int amountEquipped)
        {
            if (Kourindou.GensokyoLoaded)
            {
                return npc.type != Kourindou.Gensokyo_Fairy_Sand_Type;
            }

            return base.PlushieCanBeHitByNPC(myPlayer, npc, ref cooldownSlot, amountEquipped);
        }
    }
}