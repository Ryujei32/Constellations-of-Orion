using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.NPCs.Bosses.Astraeus
{
    public class FallenCrystalstar : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 34;
            Projectile.alpha = 0;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.friendly = false;
            Projectile.hostile = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            float rotation = Main.rand.NextFloat(-MathF.PI / 4f, MathF.PI / 4f);
            float speed = Main.rand.NextFloat(12f, 16f);
            Projectile.velocity = new Vector2(MathF.Sin(rotation), MathF.Cos(rotation)) * speed;
            Projectile.rotation = Main.rand.NextFloat(0, MathF.PI * 2f);
            SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.5f, 1f));
            if (Main.rand.NextBool(10))
            {
                float rot = (-Projectile.velocity).ToRotation() + Main.rand.NextFloat(-MathF.PI / 4f, MathF.PI / 4f);
                Dust dust = WUAUISAUI(rot, Main.rand.NextFloat(3, 6));
                dust.noGravity = true;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Slow, 300);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 300);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < Main.rand.Next(10, 20); i++)
            {
                float rot = Main.rand.NextFloat(0, MathF.PI * 2f);
                WUAUISAUI(rot, Main.rand.NextFloat(1, 3));
            }
        }

        private Dust WUAUISAUI(float rotation, float velocity)
        {
            Vector2 v = new Vector2(MathF.Cos(rotation), MathF.Sin(rotation)) * velocity;
            int type = Main.rand.NextBool() ? DustID.GemAmethyst : (Main.rand.NextBool() ? DustID.GemRuby : DustID.GemTopaz);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, type, v, 0, default, Main.rand.NextFloat(1, 2));
            if (Main.rand.NextBool())
            {
                dust.noGravity = true;
                dust.noLightEmittence = false;
            }
            else
            {
                dust.noGravity = false;
                dust.noLightEmittence = true;
            }
            dust.color = Color.Magenta;
            return dust;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Main.spriteBatch.Draw(texture, position, null, Color.White * 0.3f, Projectile.rotation, texture.Size() / 2f, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            return true;
        }
    }
}