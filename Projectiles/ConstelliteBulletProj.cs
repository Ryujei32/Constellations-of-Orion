using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Projectiles
{
	public class ConstelliteBulletProj : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Bullet);

			AIType = ProjectileID.Bullet;

			Projectile.penetrate = -1;
			Projectile.timeLeft = 600;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();

			// ⭐ SUBTLE TRAIL (soft colors, not strong)
			if (Main.rand.NextBool(4))
			{
				int dustType = Main.rand.NextBool(3) ? DustID.PinkTorch :
							   Main.rand.NextBool() ? DustID.PurpleTorch :
							   DustID.YellowTorch;

				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					dustType
				);

				dust.noGravity = true;
				dust.scale = 0.9f;     // smaller = softer look
				dust.velocity *= 0.2f; // calm movement
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.penetrate == -1)
				Projectile.penetrate = 3;

			Projectile.penetrate--;

			if (Projectile.penetrate <= 0)
				return true;

			if (Projectile.velocity.X != oldVelocity.X)
				Projectile.velocity.X = -oldVelocity.X;

			if (Projectile.velocity.Y != oldVelocity.Y)
				Projectile.velocity.Y = -oldVelocity.Y;

			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			// ⭐ STRONGER HIT EFFECT
			for (int i = 0; i < 10; i++)
			{
				int dustType = Main.rand.NextBool(3) ? DustID.PinkTorch :
							   Main.rand.NextBool() ? DustID.PurpleTorch :
							   DustID.YellowTorch;

				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					dustType
				);

				dust.noGravity = true;
				dust.scale = 1.3f;   // stronger on hit
				dust.velocity *= 1.4f;
			}
		}
	}
}
