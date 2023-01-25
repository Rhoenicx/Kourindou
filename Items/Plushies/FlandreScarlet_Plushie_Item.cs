using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Projectiles.Plushies.PlushieEffects;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;
using Kourindou.Items.CraftingMaterials;
using Kourindou.Tiles.Furniture;

namespace Kourindou.Items.Plushies
{
    public class FlandreScarlet_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flandre Scarlet Plushie");
            Tooltip.SetDefault("The ultimate basement lurker. The scarlet sky has since tempted her presence outside the mansion"); 
        }

        public override string AddEffectTooltip()
        {
            return "Critical hits explode! +50% damage, +10% crit";
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
            Item.createTile = TileType<FlandreScarlet_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            Item.accessory = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<FlandreScarlet_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddRecipeGroup("Kourindou:Gemstone", 1)
                .AddIngredient(ItemType<RainbowFabric>(), 1)
                .AddIngredient(ItemType<RedFabric>(), 2)
                .AddIngredient(ItemType<YellowFabric>(), 2)
                .AddIngredient(ItemID.Silk, 3)
                .AddIngredient(ItemType<RedThread>(), 2)
                .AddIngredient(ItemType<WhiteThread>(), 2)
                .AddRecipeGroup("Kourindou:Stuffing", 5)
                .AddTile(TileType<SewingMachine_Tile>())
                .Register();
        }

        public override void PlushieUpdateEquips(Player player, int amountEquipped)
        {
            // Increase damage by 25 percent
            player.GetDamage(DamageClass.Generic) += 0.50f;

            // Increase Life regen by +1 
            player.lifeRegen += 1;

            // Increase crit by 10 percent
            player.GetCritChance(DamageClass.Generic) += 10;

            // Crit hits explode
        }

        public override void PlushieOnHit(Player myPlayer, Item item, Projectile proj, NPC npc, Player player, int damage, float knockback, bool crit, int amountEquipped)
        {
            if (crit)
            {
                Vector2 position = Vector2.Zero;

                if (npc != null)
                {
                    int immune = item != null ? item.useAnimation : npc.immune[myPlayer.whoAmI];

                    position = npc.Center;

                    Projectile.NewProjectile(
                        Item.GetSource_Accessory(Item),
                        position,
                        Vector2.Zero,
                        ProjectileType<FlandreScarlet_Plushie_Explosion>(),
                        damage * 2 + 80,
                        0f,
                        Main.myPlayer,
                        npc.whoAmI,
                        immune
                    );

                    if (proj != null)
                    {
                        npc.immune[myPlayer.whoAmI] = 0;
                    }
                }

                if (player != null)
                {
                    int immune = item != null ? item.useAnimation : player.immuneTime;

                    position = player.Center;

                    Projectile.NewProjectile(
                        myPlayer.GetSource_Accessory(Item),
                        position,
                        Vector2.Zero,
                        ProjectileType<FlandreScarlet_Plushie_Explosion>(),
                        damage * 2 + 80,
                        0f,
                        Main.myPlayer,
                        myPlayer.whoAmI + 10000,
                        immune
                    );

                    if (proj != null)
                    {
                        player.immuneTime = 0;
                    }
                }

                SoundEngine.PlaySound(
                    SoundID.DD2_ExplosiveTrapExplode with { Volume = .8f, PitchVariance = .1f },
                    position);

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // Send sound packet for other clients
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)KourindouMessageType.PlaySound);
                    packet.Write((string)"DD2_ExplosiveTrapExplode");
                    packet.Write((float)0.8f);
                    packet.Write((float)1f);
                    packet.Write((int)position.X);
                    packet.Write((int)position.Y);
                    packet.Send(-1, Main.myPlayer);
                }
            }
        }
    }
}