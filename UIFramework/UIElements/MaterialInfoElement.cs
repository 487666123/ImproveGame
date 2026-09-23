using ImproveGame.UIFramework.SUIElements;

namespace ImproveGame.UIFramework.UIElements
{
    /// <summary>
    /// 一个单元的掉率信息
    /// </summary>
    public class MaterialInfoElement : UIPanel
    {
        private readonly Item iconItem;
        public int ItemType;
        public int StackRequired;
        public UITextPanel<string> StackInfo;
        public UITextPanel<string> StackCheckedInfo;

        public MaterialInfoElement(int itemType, int stackRequired)
        {
            this.SetSize(new Vector2(0f, 28f), 1f);
            BorderColor = Color.Transparent;
            BackgroundColor = Color.Transparent;

            StackRequired = stackRequired;
            ItemType = itemType;
            iconItem = new(ItemType);

            StackInfo = new(MyUtils.GetTextWith("ConstructGUI.MaterialInfo.Requirement", new { Stack = stackRequired }), 1f)
            {
                IgnoresMouseInteraction = true,
                DrawPanel = false,
                VAlign = 0.5f,
                Left = new StyleDimension(-110f, 1f)
            };
            Append(StackInfo);

            StackCheckedInfo = new(stackRequired.ToString(), 1f)
            {
                IgnoresMouseInteraction = true,
                DrawPanel = false,
                VAlign = 0.5f,
                Left = new StyleDimension(-250f, 1f)
            };
            Append(StackCheckedInfo);

            UITextPanel<string> itemNameInfo = new(Lang.GetItemNameValue(ItemType), 1f)
            {
                IgnoresMouseInteraction = true,
                DrawPanel = false,
                VAlign = 0.5f,
                Left = new StyleDimension(40f, 0f)
            };
            Append(itemNameInfo);
        }

        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            var inventory = MyUtils.GetAllInventoryItemsList(Main.LocalPlayer, "portable").ToArray();
            MyUtils.ItemCount(inventory, (item) => item.type == ItemType, out int stackCount);
            StackCheckedInfo.SetText(MyUtils.GetTextWith("ConstructGUI.MaterialInfo.Stored", new { Stack = stackCount }));
            if (stackCount > 99999)
            {
                StackCheckedInfo.SetText(MyUtils.GetTextWith("ConstructGUI.MaterialInfo.Stored", new { Stack = ">99999" }));
            }
            if (stackCount < StackRequired)
            {
                StackCheckedInfo.TextColor = Color.Yellow;
            }
            else
            {
                StackCheckedInfo.TextColor = Color.LightGreen;
            }
            Recalculate();

            base.DrawSelf(spriteBatch);

            var dimensions = GetDimensions();
            dimensions.Width = 28f;
            dimensions.X += 16f;
            BigBagItemSlot.DrawItemIcon(sb: spriteBatch,
                                   item: iconItem,
                                   lightColor: Color.White,
                                   dimensions: dimensions,
                                   maxSize: 26);
        }
    }

    /// <summary>
    /// 用于在教程页面展示的
    /// </summary>
    public class MaterialInfoDoll : UIPanel
    {
        private readonly Item iconItem;
        public int ItemType;
        public int StackRequired;
        public UITextPanel<string> StackInfo;
        public UITextPanel<string> StackCheckedInfo;

        public MaterialInfoDoll(int itemType, int stackRequired)
        {
            this.SetSize(315f, 28f);
            BorderColor = Color.Transparent;
            BackgroundColor = Color.Transparent;

            StackRequired = stackRequired;
            ItemType = itemType;
            iconItem = new(ItemType);

            StackInfo = new(MyUtils.GetTextWith("ConstructGUI.MaterialInfo.Requirement", new { Stack = stackRequired }), 1f)
            {
                IgnoresMouseInteraction = true,
                DrawPanel = false,
                VAlign = 0.5f,
                Left = new StyleDimension(-90f, 1f)
            };
            float textWidth = FontAssets.MouseText.Value.MeasureString(StackInfo.Text).X;
            if (textWidth > 70f)
            {
                StackInfo.TextScale = 70f / textWidth;
            }
            Append(StackInfo);

            StackCheckedInfo = new(stackRequired.ToString(), 1f)
            {
                IgnoresMouseInteraction = true,
                DrawPanel = false,
                VAlign = 0.5f,
                Left = new StyleDimension(-180f, 1f)
            };
            Append(StackCheckedInfo);

            var text = Lang.GetItemNameValue(ItemType);
            UITextPanel<string> itemNameInfo = new(text, 1f)
            {
                IgnoresMouseInteraction = true,
                DrawPanel = false,
                VAlign = 0.5f,
                Left = new StyleDimension(30f, 0f)
            };
            textWidth = FontAssets.MouseText.Value.MeasureString(text).X;
            if (textWidth > 80)
            {
                itemNameInfo.TextScale = 80f / textWidth;
                itemNameInfo.Left.Pixels -= 8f;
            }
            Append(itemNameInfo);
        }

        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            var inventory = MyUtils.GetAllInventoryItemsList(Main.LocalPlayer, "portable").ToArray();
            MyUtils.ItemCount(inventory, (item) => item.type == ItemType, out int stackCount);
            StackCheckedInfo.SetText(MyUtils.GetTextWith("ConstructGUI.MaterialInfo.Stored", new { Stack = stackCount }));
            if (stackCount > 99)
            {
                StackCheckedInfo.SetText(MyUtils.GetTextWith("ConstructGUI.MaterialInfo.Stored", new { Stack = ">99" }));
            }
            if (stackCount < StackRequired)
            {
                StackCheckedInfo.TextColor = Color.Yellow;
            }
            else
            {
                StackCheckedInfo.TextColor = Color.LightGreen;
            }
            float textWidth = FontAssets.MouseText.Value.MeasureString(StackCheckedInfo.Text).X;
            if (textWidth > 80)
            {
                StackCheckedInfo.TextScale = 80f / textWidth;
            }
            Recalculate();

            base.DrawSelf(spriteBatch);

            var dimensions = GetDimensions();
            dimensions.Width = 28f;
            dimensions.X += 16f;
            BigBagItemSlot.DrawItemIcon(sb: spriteBatch,
                                   item: iconItem,
                                   lightColor: Color.White,
                                   dimensions: dimensions,
                                   maxSize: 26);
        }
    }
}
