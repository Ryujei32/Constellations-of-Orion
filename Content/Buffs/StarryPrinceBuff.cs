using Terraria;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Buffs
{
	public class StarryPrinceBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Starry Prince");
			// Description.SetDefault("A celestial slime follows you");

			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				if (player.ownedProjectileCounts[
					ModContent.ProjectileType<Content.Projectiles.Pets.StarryPrincePet>()
				] <= 0)
				{
					Projectile.NewProjectile(
						player.GetSource_Buff(buffIndex),
						player.Center,
						Microsoft.Xna.Framework.Vector2.Zero,
						ModContent.ProjectileType<Content.Projectiles.Pets.StarryPrincePet>(),
						0,
						0f,
						player.whoAmI
					);
				}
			}

			player.buffTime[buffIndex] = 18000;
		}
	}
}
