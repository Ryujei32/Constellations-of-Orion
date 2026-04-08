using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ConstellationsOfOrion.Content.Items.Weapons
{
    public class AetherGun : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Aether Gun");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 62;
            Item.DamageType = DamageClass.Ranged;

            Item.width = 64;
            Item.height = 40;

            Item.useTime = 6;
            Item.useAnimation = 6;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;

            Item.knockBack = 3f;

            Item.value = Item.sellPrice(0, 12, 0, 0);
            Item.rare = ItemRarityID.LightRed;

            Item.UseSound = SoundID.Item158;
            Item.autoReuse = true;

            
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 16f;

            
            Item.useAmmo = AmmoID.Bullet;
        }

        
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10f, 2f);
        }

        
        public override bool Shoot(Player player,
            Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            Vector2 perturbed = velocity.RotatedByRandom(MathHelper.ToRadians(3));

            Projectile.NewProjectile(
                source,
                position,
                perturbed,
                type, 
                damage,
                knockback,
                player.whoAmI
            );

            return false; 
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
