using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class SanaeKochiya_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sanae Kochiya Plushie");
            Tooltip.SetDefault("The wind god shrine maiden. She doesn't look like Reimu.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Lime;

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
            item.createTile = TileType<SanaeKochiya_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<SanaeKochiya_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increased damage by 5 percent
            player.allDamage += 0.05f;

            // Increase Life regen by +1 
            player.lifeRegen += 1;

            // Increase Critrate by 25 percent
            player.magicCrit += 25;
            player.rangedCrit += 25;
            player.meleeCrit += 25;

            // Immunity to mighty wind debuff
            player.buffImmune[BuffID.WindPushed] = true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            // 2 green cloth
            // 2 blue cloth
            // 2 white cloth
            // 1 red thread
            // 2 green thread
            // 1 blue thread
            // 1 white thread
            // 5 stuffing 
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}