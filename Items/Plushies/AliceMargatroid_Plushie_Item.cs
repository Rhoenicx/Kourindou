using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class AliceMargatroid_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Alice Margatroid Plushie");
            Tooltip.SetDefault("The seven-colored magician. Ironically, she's a doll now.");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Pink;

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
            item.createTile = TileType<AliceMargatroid_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;           
        }
        
        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<AliceMargatroid_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Increase damage by 5 percent
            player.allDamage += 0.05f;

            // Increase life regen by 1 point
            player.lifeRegen += 1;

            // Increase max minions by 1 slot
            player.maxMinions += 1;

            // Increase magic crit by 15 percent
            player.magicCrit += 15;

            // Increase magic and minion damage by 10 percent
            player.magicDamage += 10f;
            player.minionDamage += 10f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            // Yellow Cloth 2
            // White Cloth 2
            // Blue Cloth 2
            // White Thread 2
            // red thread 1
            // Blue thread 2
            // Stuffing 5
            recipe.AddTile(TileType<SewingMachine_Tile>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}