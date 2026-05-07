using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
	public class HeliosSword : ModItem
	{
		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.IronBroadsword);

			Item.width = 52;
			Item.height = 52;

			Item.DamageType = DamageClass.Melee;
			Item.damage = 82;
			Item.ResearchUnlockCount = 1;
			Item.knockBack = 5.5f;

			Item.useTime = 20;
			Item.useAnimation = 20;

			Item.autoReuse = true;

			// ⭐ Required for Shoot()
			Item.shoot = ProjectileID.StarCannonStar;
			Item.shootSpeed = 10f;

			Item.rare = ItemRarityID.LightPurple;
			Item.value = Item.buyPrice(gold: 6);
		}

		public override bool Shoot(Player player,
			Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
			Vector2 position,
			Vector2 velocity,
			int type,
			int damage,
			float knockback)
		{
			// ⭐ Rare proc (1 in 6 swings)
			if (!Main.rand.NextBool(6))
				return false;

			int starCount = 2;

			for (int i = 0; i < starCount; i++)
			{
				Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(8)) * 1.2f;

				// ⭐ MIX OF STAR TYPES
				int projType = Main.rand.NextBool(2)
					? ProjectileID.StarCannonStar
					: ProjectileID.SuperStar;

				int starDamage = (int)(Item.damage * 0.8f);

				Projectile.NewProjectile(
					source,
					player.Center,
					perturbedSpeed,
					projType,
					starDamage,
					knockback,
					player.whoAmI
				);
			}

			return false;
		}

		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustDirect(
					hitbox.TopLeft(),
					hitbox.Width,
					hitbox.Height,
					Main.rand.NextBool() ? DustID.GemTopaz : DustID.GemAmethyst
				);

				dust.noGravity = true;
				dust.scale = 1.3f;
				dust.velocity *= 0.3f;
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Effect",
				"Occasionally release stars when swinging"));

		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 12)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
