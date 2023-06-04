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

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 25 percent
            player.GetDamage(DamageClass.Generic) += 0.25f;

            // Increase Life regen by +1 
            player.lifeRegen += 1;

            //Decrease damage taken by 17%
            player.endurance += 0.17f;
        }

        public override void PlushieOnHitNPCWithItem(Player player, Item item, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            target.AddBuff(BuffID.Chilled, 600);
            target.AddBuff(BuffID.Frostburn, 600);
            target.AddBuff(BuffID.Slow, 600);

            if (hit.Crit)
            {
                target.AddBuff(BuffID.Frozen, 120);
            }
        }

        public override void PlushieOnHitNPCWithProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone, int amountEquipped)
        {
            target.AddBuff(BuffID.Chilled, 600);
            target.AddBuff(BuffID.Frostburn, 600);
            target.AddBuff(BuffID.Slow, 600);

            if (hit.Crit)
            {
                target.AddBuff(BuffID.Frozen, 120);
            }
        }

        public override void PlushieModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers, int amountEquipped)
        {
            if ((int)Main.rand.Next(0, 12) == 9)
            {
                modifiers.SourceDamage += 9;
            }
        }

        public override void PlushieModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers, int amountEquipped)
        {
            if ((int)Main.rand.Next(0, 12) == 9)
            {
                modifiers.SourceDamage += 9;
            }
        }
    }
}