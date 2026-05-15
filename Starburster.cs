// ============================================
// CONTENT/ITEMS/WEAPONS/STARBUSTER.CS
// FULL tModLoader 1.4.4.9 CODE
// ============================================

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
	public class Starburster : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 52;
			Item.height = 22;

			Item.damage = 42;
			Item.DamageType = DamageClass.Ranged;

			Item.useTime = 5;
			Item.useAnimation = 15;
			Item.reuseDelay = 14;
			Item.useLimitPerAnimation = 3;

			Item.useStyle = ItemUseStyleID.Shoot;

			Item.knockBack = 2f;

			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Orange;

			Item.UseSound = SoundID.Item9;

			Item.autoReuse = true;

			Item.noMelee = true;

			Item.shoot = ProjectileID.FallingStar;
			Item.shootSpeed = 16f;

			Item.useAmmo = AmmoID.FallenStar;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-4f, 0f);
		}

		public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			// 50% ammo conservation
			return Main.rand.NextFloat() >= 0.50f;
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
			// Rapid star spread
			for (int i = 0; i < 3; i++)
			{
				Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(8));

				perturbedSpeed *= 1f - Main.rand.NextFloat(0.12f);

				Projectile.NewProjectile(
					source,
					position,
					perturbedSpeed,
					type,
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
				.AddIngredient<Content.Items.Materials.SunstoneBar>(15)
				.AddIngredient(ItemID.FallenStar, 6)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}