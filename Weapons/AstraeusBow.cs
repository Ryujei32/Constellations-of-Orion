using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
	public class AstraeusBow : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Astraeus");
		}

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 58;
			Item.ResearchUnlockCount = 1;

			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 22;
			Item.useAnimation = 22;

			Item.DamageType = DamageClass.Ranged;
			Item.damage = 60;
			Item.knockBack = 3f;

			Item.noMelee = true;
			Item.autoReuse = true;

			Item.useAmmo = AmmoID.Arrow;

			// ⭐ Default doesn't matter anymore (we override it)
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.shootSpeed = 12f;

			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.buyPrice(gold: 5);

			Item.UseSound = SoundID.Item5;
		}

		public override bool Shoot(Player player,
			Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
			Vector2 position,
			Vector2 velocity,
			int type,
			int damage,
			float knockback)
		{
			// ⭐ FORCE CONSTELLITE ARROWS
			int arrowType = ModContent.ProjectileType<Content.Projectiles.ConstelliteArrowProj>();

			int numberProjectiles = 3;

			for (int i = 0; i < numberProjectiles; i++)
			{
				// ⭐ Tight spread (feels good like Shotbow)
				Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(6));
				newVelocity *= 1f - Main.rand.NextFloat(0.05f);

				Projectile.NewProjectile(
					source,
					position,
					newVelocity,
					arrowType,
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
				.AddIngredient(ItemID.DemonBow)
				.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 6)
				.AddIngredient(ItemID.FallenStar, 4)
				.AddTile(TileID.MythrilAnvil)
				.Register();

			CreateRecipe()
				.AddIngredient(ItemID.TendonBow)
				.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 6)
				.AddIngredient(ItemID.FallenStar, 4)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
