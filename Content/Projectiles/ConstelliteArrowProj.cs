using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Projectiles
{
	public class ConstelliteArrowProj : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);

			Projectile.penetrate = 3; // ⭐ pierces 3 enemies

			AIType = ProjectileID.WoodenArrowFriendly;
		}

		public override void AI()
		{
			// ⭐ Trail dust
			if (Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					Main.rand.NextBool() ? DustID.GemAmethyst : DustID.GemTopaz
				);

				dust.noGravity = true;
				dust.scale = 1.2f;
				dust.velocity *= 0.2f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			SpawnDust();
		}

		// ⭐ CORRECT TILE HIT METHOD
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			SpawnDust();
			return true; // destroy projectile like normal arrow
		}

		private void SpawnDust()
		{
			for (int i = 0; i < 8; i++)
			{
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					Main.rand.NextBool() ? DustID.GemAmethyst : DustID.GemTopaz
				);

				dust.noGravity = true;
				dust.scale = 1.5f;
				dust.velocity *= 1.2f;
			}
		}
	}
}
