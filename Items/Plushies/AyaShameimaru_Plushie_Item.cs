using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Plushies
{
    [AutoloadEquip(EquipType.Wings)]
    public class AyaShameimaru_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Aya Shameimaru Plushie");
            Tooltip.SetDefault("A crow tengu. Runs a newspaper that's mostly sensational gossip");
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(240, 7f, 2.0f);
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Red;

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
            Item.createTile = TileType<AyaShameimaru_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }
        
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<AyaShameimaru_Plushie_Projectile>();
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
        }
        
        public override string AddEffectTooltip()
        {
            return "Functions as wings";
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 2)
                .AddIngredient(ItemType<RedFabric>(), 1)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.BlackThread, 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            constantAscend = 0.15f;
            ascentWhenFalling = 0.9f;
            ascentWhenRising = 0.25f;
            maxCanAscendMultiplier = 1f;
            maxAscentMultiplier = 3f;
        }
    }
}