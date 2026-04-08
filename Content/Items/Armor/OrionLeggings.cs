using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	public class OrionLeggings : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.defense = 13;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			Player player = Main.LocalPlayer;

			bool inSocialSlot =
				player.armor[12].type == Type;

			if (inSocialSlot)
				return;

			tooltips.Add(new TooltipLine(Mod, "DamageBonus", "8% increased damage"));
			tooltips.Add(new TooltipLine(Mod, "CritBonus", "8% increased critical strike chance"));
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.08f;
			player.GetCritChance(DamageClass.Generic) += 8f;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 16)
				.AddIngredient(ItemID.FallenStar, 12)
				.AddIngredient(ItemID.SoulofLight, 8)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
