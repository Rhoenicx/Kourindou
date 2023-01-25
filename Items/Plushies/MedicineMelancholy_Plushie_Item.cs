using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;
using Kourindou.Buffs;

namespace Kourindou.Items.Plushies
{
    public class MedicineMelancholy_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Medicine Melancholy Plushie");
            Tooltip.SetDefault("A young doll youkai that lives on the Nameless Hill. She can harness the poison of anything");
        }

        public override string AddEffectTooltip()
        {
            return "Every hit has a 12% chance to inflict a stackable poison debuff. Also gain poison immunity";
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
            Item.createTile = TileType<MedicineMelancholy_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<MedicineMelancholy_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemType<BlackFabric>(), 1)
                .AddIngredient(ItemType<RedFabric>(), 2)
			    .AddIngredient(ItemType<YellowFabric>(), 1)
                .AddIngredient(ItemID.Silk, 2)
                .AddIngredient(ItemID.BlackThread, 2)
                .AddIngredient(ItemType<RedThread>(), 1)
                .AddIngredient(ItemType<YellowThread>(), 1)
                .AddIngredient(ItemType<WhiteThread>(), 1)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase hp regen by 1 point
            player.lifeRegen += 1;

            // Increase all damage by 5 percent
            player.GetDamage(DamageClass.Generic) += 0.05f;

            // Immunity to poison debuff
            player.buffImmune[BuffID.Poisoned] = true;
        }

        public override void PlushieOnHit(Player myPlayer, Item item, Projectile proj, NPC npc, Player player, int damage, float knockback, bool crit, int amountEquipped)
        {
            if (npc != null)
            {
                if ((int)Main.rand.Next(0, 100) < 12)
                {
                    npc.AddBuff(BuffType<DeBuff_MedicineMelancholy>(), 600);
                }
                npc.AddBuff(BuffID.Poisoned, 600);
            }

            if (player != null)
            {
                if ((int)Main.rand.Next(0, 100) < 12)
                {
                    player.AddBuff(BuffType<DeBuff_MedicineMelancholy>(), 300);
                }
                player.AddBuff(BuffID.Poisoned, 300);
            }
        }
    }
}