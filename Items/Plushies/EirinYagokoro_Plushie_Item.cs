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
    public class EirinYagokoro_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Eirin Yagokoro Plushie");
            // Tooltip.SetDefault("The genius of the Moon");
        }

        public override string AddEffectTooltip()
        {
            return "+10 HP regen, +50 max HP, +10% arrow damage, +25% damage, reduced potion cooldown";
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.White;

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
            Item.createTile = TileType<EirinYagokoro_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }
        
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<EirinYagokoro_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlueFabric>(), 2)
                .AddIngredient(ItemType<RedFabric>(), 2)
                .AddIngredient(ItemType<SilverFabric>(), 2)
                .AddIngredient(ItemType<BlueThread>(), 1)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<SilverThread>(), 3)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 25 percent
            player.GetDamage(DamageClass.Generic) += 0.25f;

            // Life regen increased by 10 points
            player.lifeRegen += 10;

            // Arrow Damage increased by 10 percent
            player.arrowDamage += 0.10f;

            // Max life increased by 50 points
            player.statLifeMax2 += 50;

            // Reduce potion delay times by 35%
            player.potionDelayTime = (int)((double)player.potionDelayTime * 0.75);
            player.restorationDelayTime = (int)((double)player.restorationDelayTime * 0.75);
            player.mushroomDelayTime = (int)((double)player.mushroomDelayTime * 0.75);
        }
    }
}