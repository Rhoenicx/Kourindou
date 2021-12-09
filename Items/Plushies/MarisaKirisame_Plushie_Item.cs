using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class MarisaKirisame_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Marisa Kirisame Plushie");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 50, 0, 0);
            item.rare = ItemRarityID.Red;

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
            item.createTile = TileType<MarisaKirisame_Plushie_Tile>();

            item.shootSpeed = 8f;

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                SetSecondaryStats();
                SynchronizeSecondary(player);
            }
            else
            {
                SetNormalStats();
            }
            
            return true;
        }

        // Change stats for normal use
        public virtual void SetNormalStats()
        {
            item.createTile = TileType<MarisaKirisame_Plushie_Tile>();
        }

        // Change stats for alt use
        public virtual void SetSecondaryStats()
        {
            item.shoot = ProjectileType<MarisaKirisame_Plushie_Projectile>();
        }
        

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {

        }
    }
}