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
    public class Gensokyo_Thorn_Fairy_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Thorn Fairy Plushie");
            Tooltip.SetDefault("");
        }

        public override string AddEffectTooltip()
        {
            return "Thorn fairies become friendly";
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Purple;

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
            Item.createTile = TileType<Gensokyo_Thorn_Fairy_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Gensokyo_Thorn_Fairy_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            if (Kourindou.GensokyoLoaded)
            {
                CreateRecipe(1)
				    .AddIngredient(ItemID.Deathweed, 4)
			        .AddIngredient(ItemType<BlackFabric>(), 2)
                    .AddIngredient(ItemType<SilverFabric>(), 2)
                    .AddIngredient(ItemType<PurpleFabric>(), 2)
			        .AddIngredient(ItemID.BlackThread, 1)
                    .AddIngredient(ItemType<SilverThread>(), 2)
				    .AddIngredient(ItemType<PurpleThread>(), 2)
                    .AddRecipeGroup("Kourindou:Stuffing", 5)
                    .AddTile(TileType<SewingMachine_Tile>())
                    .Register();
            }
        }

        public override void PlushieUpdateEquips(Player player)
        {
            if (Kourindou.GensokyoLoaded)
            {
                player.npcTypeNoAggro[Kourindou.Gensokyo_Fairy_Thorn_Type] = true;
            }
        }

        public override bool PlushieCanbeHitByNPC(Player myPlayer, NPC npc, ref int cooldownSlot)
        {
            if (Kourindou.GensokyoLoaded)
            {
                return npc.type != Kourindou.Gensokyo_Fairy_Thorn_Type;
            }

            return base.PlushieCanbeHitByNPC(myPlayer, npc, ref cooldownSlot);
        }
    }
}