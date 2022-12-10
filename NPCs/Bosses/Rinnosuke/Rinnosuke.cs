using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
//using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Filters = Terraria.Graphics.Effects.Filters;

namespace Kourindou.NPCs.Bosses.Rinnosuke
{
    public class Rinnosuke : BossNPC
    {
        protected override int StageAmount => 7;
        protected override int[] StageHealth => new int[] { 1000, 1000, 1000, 1000, 1000, 1000, 1000 };
        protected override int[] StageDefense => new int[] { 10, 10, 10, 10, 10, 10, 10 };
        protected override bool TargetDecoys => false;
        protected override short SpawnAnimationTime => 120;
        protected override short DefeatAnimationTime => 120;
        protected override short[] StageSwitchAnimationTime => new short[] { 120, 120, 120, 120, 120, 120, 120 };


        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;

            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused
                }
            };
            NPCID.Sets.DebuffImmunitySets[Type] = debuffData;

            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }
        public override void SetDefaults()
        {
            // AI
            NPC.aiStyle = -1;

            // Hitbox
            NPC.width = 36;
            NPC.height = 50;

            // Hitpoints and Defense
            NPC.lifeMax = GetMaxHealth();
            NPC.damage = 66;
            NPC.defense = GetAverageDefense();
            NPC.knockBackResist = 0f;

            // Miscellaneous
            NPC.value = Item.buyPrice(0, 10);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.lavaImmune = true;

            // Movement
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            // Sounds and Music
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * bossLifeScale);
            SetStageHealth();
        }

        public override void Conditioning()
        {

        }

        public override bool Attack()
        {
            switch (AttackIndex)
            {
                case (byte)Attacks.MoveOnly: // Special number used for Move-only
                    {

                    }
                    break;
                case 1:
                    {

                    }
                    break;
                case (byte)Attacks.StageSwitch: // Special number used for stage-switching
                    {

                    }
                    break;
            }

            return true;
        }

        public override bool Move()
        {
            switch (MoveIndex)
            {
                case 0:
                    {

                    }
                    break;
                case 1:
                    {

                    }
                    break;
            }

            return true;
        }
    }
}
