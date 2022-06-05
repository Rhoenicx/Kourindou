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
            Tooltip.SetDefault("The magician of the scarlet mansion");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.LightPurple;

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
            Item.createTile = TileType<PatchouliKnowledge_Plushie_Tile>();
            
            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
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
            player.GetDamage(DamageClass.Magic) += 0.40f;

            // All other damage types deal zero, really into negatives because other mods might increase this
            player.GetDamage(DamageClass.Ranged) -= -1000f;
            player.GetDamage(DamageClass.Summon) -= -1000f;
            player.GetDamage(DamageClass.Melee) -= -1000f;
            player.GetDamage(DamageClass.Throwing) -= 1000f;

            // Increase magic crit rate by 30 percent
            player.GetCritChance(DamageClass.Magic) += 30;

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
            CreateRecipe(1)
                .AddIngredient(ItemID.Book, 1)
                .AddIngredient(ItemType<PinkFabric>(), 2)
                .AddIngredient(ItemType<PurpleFabric>(), 1)
                .AddIngredient(ItemType<VioletFabric>(), 2)
                .AddIngredient(ItemID.Silk, 1)
                .AddIngredient(ItemID.PinkThread, 1)
                .AddIngredient(ItemType<PurpleThread>(), 1)
                .AddIngredient(ItemType<VioletThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}