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
    public class PatchouliKnowledge_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Patchouli Knowledge Plushie");
            Tooltip.SetDefault("The magician of the scarlet mansion.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.LightPurple;

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
            item.createTile = TileType<PatchouliKnowledge_Plushie_Tile>();
            
            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<PatchouliKnowledge_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase Magic damage by 40 percent
            player.magicDamage += 0.40f;

            // All other damage types deal zero, really into negatives because other mods might increase this
            player.rangedDamage = -1000f;
            player.minionDamage = -1000f;
            player.meleeDamage = -1000f;
            player.thrownDamage = -1000f;

            // Increase magic crit rate by 30 percent
            player.magicCrit += 30;

            // Reduce mana consumption by 50 percent
            player.manaCost -= 0.50f;

            //reduce movespeed
            if (player.accRunSpeed > player.maxRunSpeed)
            {
                player.accRunSpeed *= 0.5f;
                player.maxRunSpeed *= 0.5f;
            }
            else
            {
                player.maxRunSpeed *= 0.5f;
                player.accRunSpeed = player.maxRunSpeed;
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Book, 1);
            recipe.AddIngredient(ItemType<PinkFabric>(), 2);
            recipe.AddIngredient(ItemType<PurpleFabric>(), 1);
            recipe.AddIngredient(ItemType<VioletFabric>(), 2);
            recipe.AddIngredient(ItemID.Silk, 1);
            recipe.AddIngredient(ItemID.PinkThread, 1);
            recipe.AddIngredient(ItemType<PurpleThread>(), 1);
            recipe.AddIngredient(ItemType<VioletThread>(), 1);
            recipe.AddIngredient(ItemType<WhiteThread>(), 2);
            recipe.AddRecipeGroup("Kourindou:Stuffing", 5);
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}