using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
	public class GelatinousSpray : ModItem
	{
		private const float VisualRecoilKick = 0.36f;
		private const float VisualRecoilMax = 0.9f;
		private const float AimSmoothness = 0.22f;
		private float visualRecoil;
		private static Asset<Texture2D> inventoryGuiTexture;

		public override void Load()
		{
			if (!Main.dedServ)
				inventoryGuiTexture = ModContent.Request<Texture2D>(Texture + "A");
		}

		public override void Unload()
		{
			inventoryGuiTexture = null;
		}

		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 3));
			ItemID.Sets.AnimatesAsSoul[Item.type] = false;
		}


		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.BubbleGun);
			Item.damage = 58;
			Item.DamageType = DamageClass.Magic;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.mana = 10;
			Item.knockBack = 3f;
			Item.value = Item.buyPrice(gold: 7);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item21;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<Content.Projectiles.GelBubble1>();
			Item.shootSpeed = 5.5f;
		}

		public override bool Shoot(Player player,
			EntitySource_ItemUse_WithAmmo source,
			Vector2 position,
			Vector2 velocity,
			int type,
			int damage,
			float knockback)
		{
			visualRecoil = MathHelper.Clamp(visualRecoil + VisualRecoilKick, 0f, VisualRecoilMax);

			int numberProjectiles = 4;

			for (int i = 0; i < numberProjectiles; i++)
			{
				Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(12));
				perturbedSpeed *= 1f - Main.rand.NextFloat(0.2f);
				perturbedSpeed *= 0.9f;

				int projType = Main.rand.NextBool()
					? ModContent.ProjectileType<Content.Projectiles.GelBubble1>()
					: ModContent.ProjectileType<Content.Projectiles.GelBubble2>();

				Projectile.NewProjectile(
					source,
					position,
					perturbedSpeed,
					projType,
					damage,
					knockback,
					player.whoAmI
				);
			}

			return false;
		}

		public override void HoldItem(Player player)
		{
			if (player.whoAmI != Main.myPlayer)
				return;

			Vector2 aim = Main.MouseWorld - player.MountedCenter;
			if (aim.LengthSquared() <= 0.001f)
				return;

			int direction = aim.X >= 0f ? 1 : -1;
			player.ChangeDir(direction);

			float targetRotation = aim.ToRotation();
			if (direction == -1)
				targetRotation += MathHelper.Pi;

			targetRotation -= direction * visualRecoil;
			player.itemRotation = player.itemRotation.AngleLerp(targetRotation, AimSmoothness);
			visualRecoil = MathHelper.Lerp(visualRecoil, 0f, 0.2f);
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-6f, 0f);
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (inventoryGuiTexture?.IsLoaded != true)
				return true;

			Texture2D tex = inventoryGuiTexture.Value;
			Rectangle guiFrame = tex.Frame();
			Vector2 guiOrigin = guiFrame.Size() * 0.5f;
			spriteBatch.Draw(tex, position, guiFrame, drawColor, 0f, guiOrigin, scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
