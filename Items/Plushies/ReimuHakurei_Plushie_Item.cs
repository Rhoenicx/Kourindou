using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Kourindou.Tiles.Plushies;
using Kourindou.Projectiles.Plushies;

namespace Kourindou.Items.Plushies
{
    public class ReimuTarget 
    {
        public int n {get; set;}
        public float d {get; set;}

        public ReimuTarget (int n, float d)
        {
            this.n = n;
            this.d = d;
        }
    }

    public class ReimuHakurei_Plushie_Item : PlushieItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Reimu Hakurei Plushie");
            Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            // Information
            item.value = Item.buyPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.White;

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
            item.createTile = TileType<ReimuHakurei_Plushie_Tile>();

            // Register as accessory, can only be equipped when plushie power mode setting is 2
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                shootSpeed = 8f;
                projectileType = ProjectileType<ReimuHakurei_Plushie_Projectile>();
            }
            return base.UseItem(player);
        }

        // This only executes when plushie power mode is 2
        public override void PlushieEquipEffects(Player player)
        {
            // Reduce damage by 25 Percent
            player.allDamage -= 0.25f;

            // Increase Life regen by +1 
            player.lifeRegen += 1;

            // Homing on all projectiles that this player owns
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.type == ProjectileID.IceBlock || !(proj.magic || proj.melee || proj.ranged || proj.thrown))
                {
                    continue;
                }

                // if this player is the owner of this active projectile
                if (proj.owner == player.whoAmI && proj.active)
                {
                    //  Create list with valid targets
                    List<ReimuTarget> target = new List<ReimuTarget>();

                    // search for all the valif targets and save their id and distance
                    foreach (NPC npc in Main.npc)
                    {
                        if (!npc.friendly && npc.active && Collision.CanHit(proj.Center, 1, 1, npc.position, npc.width, npc.height) && Vector2.Distance(npc.Center, proj.Center) < 500f)
                        {
                            target.Add(new ReimuTarget(npc.whoAmI, Vector2.Distance(npc.position, proj.position)));
                        }
                    }

                    // Find nearest target in the valid target list
                    ReimuTarget nearest = target.Any() ? target[0] : null;
                    if (nearest != null)
                    {
                        for (int i = 0; i < target.Count; i++)
                        {
                            if (target[i].d < nearest.d)
                            {
                                nearest = target[i];
                            }
                        }

                        Main.NewText(proj.damage);
                        
                        float distance = Vector2.Distance(proj.Center, Main.npc[nearest.n].Center);
                        float magnitude = distance < 500f ? (1f * (1f - distance / 500f)) : 0f;

                        Vector2 direction = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(proj.velocity), Vector2.Normalize(Main.npc[nearest.n].Center - proj.Center), magnitude));

                        proj.velocity = direction * proj.velocity.Length();
                    }
                }
            }
        }
    }
}