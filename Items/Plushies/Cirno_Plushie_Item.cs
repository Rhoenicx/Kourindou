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
    public class Cirno_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cirno Plushie");
            Tooltip.SetDefault("The ice fairy. It's stupidly strong, and stupid as well");
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 9, 9, 9);
            Item.rare = ItemRarityID.Cyan;

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
            Item.createTile = TileType<Cirno_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }
        
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<Cirno_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 25 percent OR on the 9th attack 9% chance to deal time 9 dmg
            if (player.GetModPlayer<KourindouPlayer>().CirnoPlushie_TimesNine)
            {
                player.GetDamage(DamageClass.Generic) *= 9f;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.25f;
            }

            // Increase Life regen by +1 
            player.lifeRegen += 1;

            //Decrease damage taken by 17%
            player.endurance += 0.17f;
        }
        
        public override string AddEffectTooltip()
        {
            return "Every 9th hit has 9% chance to deal 9 times damage! +25% damage, -17% damage taken";
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlueFabric>(), 2)
                .AddIngredient(ItemType<SkyBlueFabric>(), 2)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemType<BlueThread>(), 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddIngredient(ItemID.IceBlock, 9)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}