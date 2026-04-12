using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.NPCs.Bosses.Astraeus
{
    [AutoloadBossHead]
    public class CosmicAstraeus : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;

            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.NPCBestiaryDrawModifiers drawMods = new()
            {
                PortraitScale = 0.2f,
                PortraitPositionYOverride = 0,
                PortraitPositionXOverride = 0
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawMods);
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = NPCAIStyleID.Slime;
            NPC.width = 150;
            NPC.height = 80;
            NPC.alpha = 40;
            NPC.lifeMax = 44000;

            NPC.damage = 111;
            NPC.defense = 77;
            NPC.knockBackResist = 0f;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            if (!Main.dedServ)
            {
                Music = MusicID.Boss2;
                if (!Main.swapMusic == Main.drunkWorld && !Main.remixWorld)
                {
                    Music = MusicID.OtherworldlyBoss2;
                }
            }

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.boss = true;
            NPC.npcSlots = 5f;
            NPC.SpawnWithHigherTime(30);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.AdamantiteBar, 1, 16, 24));
            npcLoot.Add(ItemDropRule.Common(ItemID.HolyArrow, 1, 20, 26));
            npcLoot.Add(ItemDropRule.Common(ItemID.CrystalBullet, 1, 20, 30));
        }

        public override bool CanHitPlayer(Player target, ref int cooldown)
        {
            cooldown = ImmunityCooldownID.Bosses;
            return true;
        }



        private uint animation;
        private uint animationTimer;
        private uint superJumpTimer;
        private uint SunBeamTimer;
        private uint starTimer;
        private float targetRotation = 0;
        private Vector2 freeze;

        public override void OnSpawn(IEntitySource source)
        {
            animation = 0;
            animationTimer = 10;
            superJumpTimer = (uint)Main.rand.Next(100, 200);
            SunBeamTimer = (uint)Main.rand.Next(300, 480);
            starTimer = (uint)Main.rand.Next(3, 7);
        }

        public override void AI()
        {
            NPC.TargetClosest(false);
            Lighting.AddLight(NPC.Center, new Vector3(2f, 1f, 2f));
            bool onGround = NPC.collideY;

            // MainAnimation
            if (onGround)
            {
                if (animationTimer == 0)
                {
                    animationTimer = 10;
                    NPC.frame.Y += NPC.frame.Height;
                }
                NPC.frame.Y %= NPC.frame.Height * Main.npcFrameCount[Type];
                animationTimer--;
            }
            else NPC.frame.Y = NPC.frame.Height * 2;

            if (NPC.HasValidTarget)
            {
                NPC.immortal = false;

                // SunBeam
                if (NPC.life <= NPC.lifeMax / 2f)
                {
                    if (SunBeamTimer >= 150) targetRotation = (Main.player[NPC.target].Center - NPC.Center).ToRotation();
                    if (SunBeamTimer >= 120 && SunBeamTimer < 240)
                    {
                        freeze = NPC.velocity;
                        for (int i = 0; i < 100; i++)
                        {
                            float s = 1f - (SunBeamTimer - 120f) / 120;
                            Vector2 rot = new(MathF.Cos(targetRotation), MathF.Sin(targetRotation));
                            Dust dust = Dust.NewDustPerfect(NPC.Center + rot * (80f + Main.rand.Next(0, 3000)), DustID.GemTopaz, rot * Main.rand.NextFloat(1, 3), 0, default, s * 0.5f);
                            dust.noGravity = true;
                        }
                    }
                    if (SunBeamTimer < 120)
                    {
                        NPC.velocity = Vector2.Zero;
                    }
                    if (SunBeamTimer == 119)
                    {
                        int type = ModContent.ProjectileType<SunBeam>();
                        for (int i = 0; i < 40; i++) Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, type, NPC.damage * 2, 6f, -1, NPC.whoAmI, i, targetRotation);
                    }
                    if (SunBeamTimer == 0)
                    {
                        NPC.velocity = freeze;
                        SunBeamTimer = (uint)Main.rand.Next(180, 300);
                    }
                    SunBeamTimer--;
                }

                // SuperJump
                if (superJumpTimer == 0 && onGround)
                {
                    superJumpTimer = (uint)Main.rand.Next(200, 400);
                    NPC.velocity.Y -= 12;
                }
                superJumpTimer--;

                // FallenStars
                if (starTimer == 0)
                {
                    starTimer = (uint)Main.rand.Next(3, 7);
                    int type = ModContent.ProjectileType<FallenCrystalstar>();
                    Vector2 position = NPC.Center + new Vector2(Main.rand.Next(-1000, 1000), -1000);
                    Projectile.NewProjectile(null, position, Vector2.Zero, type, NPC.damage / 2, 6f);
                }
                starTimer--;
            }
            else NPC.immortal = true;
            animation++;
        }

        public override void OnKill()
        {
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return (NPC.color == default ? Color.White : NPC.color) * (1 - NPC.alpha / 255f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float value = MathF.Sin(animation / 300f * MathF.PI * 2f) + 1f / 2f;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            // Star Spectral
            Vector2 origin = NPC.frame.Size() * new Vector2(0.5f, 0.9f);
            Vector2 position = NPC.position + new Vector2(NPC.width / 2f, NPC.height) - screenPos;
            Color color1 = Color.Magenta * 0.3f * (1 - value * 0.6f);
            Color color2 = Color.Pink * 0.4f * (1 - value * 0.5f);
            Color color3 = Color.Gold * 0.5f * (1 - value * 0.5f);
            float scale1 = NPC.scale * (value * 0.3f + 1.3f);
            float scale2 = NPC.scale * (value * 0.2f + 1.2f);
            float scale3 = NPC.scale * (value * 0.1f + 1.1f);
            spriteBatch.Draw(texture, position, NPC.frame, color1, 0f, origin, scale1, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, NPC.frame, color2, 0f, origin, scale2, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, position, NPC.frame, color3, 0f, origin, scale3, SpriteEffects.None, 0f);
            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D textureSunRing = ModContent.Request<Texture2D>(Texture + "_SunRing").Value;

            // Sun Ring
            if (NPC.life <= NPC.lifeMax / 2f && SunBeamTimer < 150 && NPC.HasValidTarget)
            {
                Vector2 position = NPC.Center - screenPos + new Vector2(MathF.Cos(targetRotation), MathF.Sin(targetRotation)) * 50f;
                float rotation = animation / 150f * MathF.PI * 2f;
                float alpha = MathF.Sin(SunBeamTimer / 150f * MathF.PI);
                float scale = NPC.scale * (alpha * 1.3f);
                Rectangle frame = new(0, 0, 150, 150); Rectangle frame2 = new(150, 0, 14, 32); Vector2 origin = new(75, 75); Vector2 origin2 = new(7, 74f + alpha * 20f);
                spriteBatch.Draw(textureSunRing, position, frame, Color.Yellow * alpha, rotation, origin, scale * 1.01f, SpriteEffects.None, 0f);
                for (int i = 0; i < 16; i++) spriteBatch.Draw(textureSunRing, position, frame2, Color.Yellow * alpha, rotation + MathF.PI / 8f * i, origin2, scale * 1.01f, SpriteEffects.None, 0f);
                spriteBatch.Draw(textureSunRing, position, frame, Color.White * alpha, rotation, origin, scale, SpriteEffects.None, 0f);
                for (int i = 0; i < 16; i++) spriteBatch.Draw(textureSunRing, position, frame2, Color.White * alpha, rotation + MathF.PI / 8f * i, origin2, scale, SpriteEffects.None, 0f);
            }
        }
    }
}