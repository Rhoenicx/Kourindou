using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;
using Terraria.ModLoader;

namespace Kourindou.Items.Plushies
{
    public class NueHoujuu_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
        }

        public override void SetDefaults()
        {
            // Information
            Item.value = Item.buyPrice(0, 21, 21, 21);
            Item.rare = ItemRarityID.Gray;

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
            Item.createTile = TileType<NueHoujuu_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<NueHoujuu_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase armor penetration by 10
            player.GetArmorPenetration(DamageClass.Generic) += 10;

            // Increase crit rate by 4%
            player.GetCritChance(DamageClass.Generic) += 4f;

            // Increase Life regen by +1 
            player.lifeRegen += 1;
        }

        public override void PlushieModifyWeaponDamage(Player player, Item item, ref StatModifier damage, int amountEquipped)
        {
            // Increase spear damage by 50%
            if (Kourindou.SpearItems.Contains(item.type))
            {
                damage += 0.5f;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 3)
                .AddIngredient(ItemType<BlueFabric>(), 1)
                .AddIngredient(ItemType<RedFabric>(), 1)
                .AddIngredient(ItemID.Silk, 1)
                .AddIngredient(ItemID.BlackThread, 2)
                .AddIngredient(ItemType<BlueThread>(), 1)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }
    }
}