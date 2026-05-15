// ============================================
// CONTENT/ITEMS/WEAPONS/STARFISHBURSTER.CS
// FULL tModLoader 1.4.4.9 CODE
// ============================================

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
	public class Starfishburster : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 58;
			Item.height = 24;

			Item.damage = 40;
			Item.DamageType = DamageClass.Ranged;

			Item.useTime = 4;
			Item.useAnimation = 12;
			Item.reuseDelay = 8;
			Item.useLimitPerAnimation = 3;

			Item.useStyle = ItemUseStyleID.Shoot;

			Item.knockBack = 2.5f;

			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.LightRed;

			Item.UseSound = SoundID.Item9;

			Item.autoReuse = true;

			Item.noMelee = true;

			// Uses Super Star Shooter stars
			Item.shoot = ProjectileID.SuperStar;
			Item.shootSpeed = 18f;

			Item.useAmmo = AmmoID.FallenStar;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-6f, 0f);
		}

		public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			// 40% chance not to consume stars
			return Main.rand.NextFloat() >= 0.40f;
		}

		public override bool Shoot(
			Player player,
			Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
			Vector2 position,
			Vector2 velocity,
			int type,
			int damage,
			float knockback)
		{
			for (int i = 0; i < 3; i++)
			{
				Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(6));

				perturbedSpeed *= 1f - Main.rand.NextFloat(0.08f);

				Projectile.NewProjectile(
					source,
					position,
					perturbedSpeed,
					ProjectileID.SuperStar,
					damage,
					knockback,
					player.whoAmI
				);
			}

			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient<Starburster>(1)
				.AddIngredient(ItemID.HallowedBar, 12)
				.AddIngredient(ItemID.SharkFin, 3)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}