using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.NPCs.Bosses.Astraeus
{
    public class SunBeam : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 75;
            Projectile.height = 75;
            Projectile.alpha = 0;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = false;
            Projectile.hostile = true;
        }

        private NPC Astraeus;
        public override void AI()
        {
            Astraeus = Main.npc[(uint)Projectile.ai[0]];
            if (Astraeus != null || !Astraeus.active)
            {
                Projectile.velocity = Vector2.Zero;
                Vector2 sincos = new(MathF.Cos(Projectile.ai[2]), MathF.Sin(Projectile.ai[2]));
                Projectile.Center = Astraeus.Center + sincos * (80f + Projectile.height * Projectile.ai[1]);
                Projectile.rotation = Projectile.ai[2];

                if (!Astraeus.active || Astraeus == null || Astraeus.life <= 0) Projectile.Kill();

                Projectile.alpha = (int)((1f - MathF.Sin(Projectile.timeLeft / 120f * MathF.PI)) * 255f);

                if (Projectile.timeLeft > 30)
                {
                    float s = 1f - Projectile.timeLeft / 120;
                    Vector2 rot = new(MathF.Cos(Projectile.rotation), MathF.Sin(Projectile.rotation));
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + rot * Main.rand.Next(-20, 55), DustID.GemTopaz, rot * 5f, 0, default, s * 4f);
                    dust.noGravity = true;
                    Lighting.AddLight(Projectile.Center, new Vector3(1f, 1f, 0.5f));
                }
            }
            else Projectile.Kill();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            target.AddBuff(BuffID.Darkness, 300);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = Projectile.ai[1] > 0 ? new(0, 0, 75, 23) : new(0, 24, 75, 23);
            Color color = Color.White * (1f - Projectile.alpha / 255f);
            Color color2 = Color.Gold * (1f - Projectile.alpha / 255f) * 0.3f;
            Vector2 scale = new(1, MathF.Sin(Projectile.timeLeft / 120f * MathF.PI) * 0.6f);
            Vector2 scale2 = new(1, MathF.Sin(Projectile.timeLeft / 120f * MathF.PI) * 1.2f);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, color2, Projectile.rotation, frame.Size() / 2f, scale2, SpriteEffects.None, 0f);
            return false;
        }
    }
}