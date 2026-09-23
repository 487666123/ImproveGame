using ImproveGame.Packets;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.SUIElements;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace ImproveGame.UI.ModernConfig.OptionElements
{
    public abstract class OptionCollections : ModernConfigOption
    {
        protected DefaultListValueAttribute DefaultListValueAttribute { get; set; }
        protected NullAllowedAttribute NullAllowedAttribute;
        protected JsonDefaultListValueAttribute JsonDefaultListValueAttribute { get; set; }
        protected object Data { get; set; }
        private bool expanded = true;
        protected float Scale { get; set; } = 1f;
        protected virtual bool CanItemBeAdded => true;

        protected bool pendingChanges = false;

        protected object CreateCollectionElementInstance(Type type)
        {
            object toAdd;

            if (DefaultListValueAttribute != null)
            {
                toAdd = DefaultListValueAttribute.Value;
            }
            else
            {
                toAdd = ConfigManager.AlternateCreateInstance(type);

                if (!type.IsValueType && type != typeof(string))
                {
                    string json = JsonDefaultListValueAttribute?.Json ?? "{}";

                    JsonConvert.PopulateObject(json, toAdd, ConfigManager.serializerSettings);
                }
            }

            return toAdd;
        }

        protected void NetSyncManually()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                List<string> pathDirectly = [];
                if (path != null)
                    pathDirectly.AddRange(path);
                if (List == null)
                    pathDirectly.Add(VariableInfo.Name);
                ConfigOptionPacket.Send(Config, Data, pathDirectly);
            }
        }

        // SetupList called in base.ctor, but children need Types.
        protected abstract void PrepareTypes();

        protected abstract void AddItem();

        protected abstract void InitializeCollection();

        protected virtual void NullCollection()
        {
            Data = null;
            SetValueDirect(Data);
            NetSyncManually();
        }

        protected abstract void ClearCollection();

        protected abstract void SetupList();

        protected SUIScrollView2 OptionView;

        SUITriangleIcon ExpandButton;
        SUITriangleIcon InitialButton;
        SUICross DeleteButton;
        SUIPlus AddButton;

        // 使用自动排列从右向左排，加个透明Box来控制位置，子元素从左到右全部加到这个Box里，就能从右向左排了
        View HelperBox;

        protected override void CheckAttributes()
        {
            base.CheckAttributes();
            var expandAttribute = GetAttribute<ExpandAttribute>();
            if (expandAttribute != null)
                expanded = expandAttribute.Expand;

            DefaultListValueAttribute = GetAttribute<DefaultListValueAttribute>();
            NullAllowedAttribute = GetAttribute<NullAllowedAttribute>();
        }

        protected override void OnBind()
        {
            Data = GetValue();
            Height.Set(360, 0);
            MaxHeight.Set(360, 0);
            MinHeight.Set(60, 0);

            HelperBox = new View
            {
                IsAdaptiveWidth = true,
                HAlign = 1f,
                VAlign = 0.5f,
                Height = StyleDimension.Fill,
                PaddingTop = 8,
            };
            HelperBox.JoinParent(this);

            /*ListPanel = new View
            {
                HAlign = 1f,
                VAlign = 0.5f,
                Top = { Pixels = 20f },
                Height = StyleDimension.Fill,
                Width = { Pixels = -30f, Percent = 1f },
                MaxWidth = { Pixels = -30f, Percent = 1f },
                MaxHeight = { Pixels = 300}
            };
            ListPanel.JoinParent(this);*/
            InitialButton = new SUITriangleIcon()
            {
                RelativeMode = RelativeMode.Horizontal,
                Spacing = new Vector2(4f, 0f),
                BgColor = Color.Black * 0.4f,
                Rounded = new Vector4(4f),
                Width = new(25, 0.0f),
                Height = new(25, 0.0f),
            };
            InitialButton.OnLeftClick += (evt, elem) =>
            {
                SoundEngine.PlaySound(SoundID.Tink);
                InitializeCollection();
                pendingChanges = true;
                expanded = true;
                SetupList();
            };
            DeleteButton = new SUICross()
            {
                RelativeMode = RelativeMode.Horizontal,
                Spacing = new Vector2(4f, 0f),
                BgColor = Color.Black * 0.4f,
                Rounded = new Vector4(4f),
                Width = new(25, 0.0f),
                Height = new(25, 0.0f),
            };
            DeleteButton.OnLeftClick += (evt, elem) =>
            {
                SoundEngine.PlaySound(SoundID.Tink);
                if (NullAllowedAttribute != null)
                    NullCollection();
                else
                    ClearCollection();
                SetupList();
                pendingChanges = true;
            };
            ExpandButton = new SUITriangleIcon()
            {
                RelativeMode = RelativeMode.Horizontal,
                Spacing = new Vector2(4f, 0f),
                BgColor = Color.Black * 0.4f,
                Rounded = new Vector4(4f),
                Width = new(25, 0.0f),
                Height = new(25, 0.0f)
            };
            //ExpandButton.trianglePercentCoord[1] = new(0f, .5f);
            ExpandButton.OnLeftClick += (evt, elem) =>
            {
                expanded = !expanded;
                pendingChanges = true;
            };
            AddButton = new SUIPlus
            {
                RelativeMode = RelativeMode.Horizontal,
                Spacing = new Vector2(4f, 0f),
                BgColor = Color.Black * 0.4f,
                Rounded = new Vector4(4f),
                Width = new(25, 0.0f),
                Height = new(25, 0.0f)
            };
            //ExpandButton.trianglePercentCoord[1] = new(0f, .5f);
            AddButton.OnLeftClick += (evt, elem) =>
            {
                SoundEngine.PlaySound(SoundID.Tink);
                AddItem();
                SetupList();
                pendingChanges = true;
                expanded = true;
            };
            OptionView = new SUIScrollView2(Orientation.Vertical)
            {
                RelativeMode = RelativeMode.None,
                Top = new(46, 0),
                Spacing = new Vector2(6)
            };
            OptionView.SetPadding(0f, 0f);
            OptionView.SetSize(0f, -60, 1f, 1f);
            OptionView.JoinParent(this);




            PrepareTypes();

            SetupList();

            pendingChanges = true;
            Recalculate();

            base.OnBind();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //var dimension = ListPanel.GetDimensions();
            //var dimension2 = GetDimensions();
            #region Expand的更新
            if (ExpandButton != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 targetCoord = i switch
                    {
                        0 => expanded ? new(0.0f, 0.3f) : new(0.7f, 0.0f),
                        1 => expanded ? new(0.5f, 0.8f) : new(0.2f, 0.5f),
                        2 or _ => expanded ? new(1.0f, 0.3f) : new(0.7f, 1.0f)
                    };
                    ExpandButton.trianglePercentCoord[i] = Vector2.Lerp(ExpandButton.trianglePercentCoord[i], targetCoord, .15f + i * .05f);

                }
                //var targetCoord = expanded ? new Vector2(1, .5f) : new(.5f, 0f);
                //ExpandButton.trianglePercentCoord[0] = Vector2.Lerp(ExpandButton.trianglePercentCoord[0],targetCoord,.25f);
            }
            #endregion
            #region 高度上限调整
            if (dragging)
            {
                showMaxHeight = Main.MouseScreen.Y - mousePos.Y + oldHeight;
                showMaxHeight = Math.Max(150, showMaxHeight); // 别拖没了
                pendingChanges = true;
            }
            #endregion

            if (!pendingChanges)
                return;
            pendingChanges = false;

            if (CanItemBeAdded)
            {
                HelperBox.RemoveChild(InitialButton);
                HelperBox.RemoveChild(AddButton);
                HelperBox.RemoveChild(DeleteButton);
            }
            HelperBox.RemoveChild(ExpandButton);
            RemoveChild(OptionView);

            if (Data == null)
            {
                HelperBox.Append(InitialButton);
            }
            else
            {
                HelperBox.Append(ExpandButton);
                if (CanItemBeAdded)
                {
                    HelperBox.Append(AddButton);
                    HelperBox.Append(DeleteButton);
                }
                if (expanded)
                    Append(OptionView);
            }
            //Parent?.Recalculate();
            UIElement target = this;
            ModernConfigOption optionObject = this;
            while (target.Parent != null)
            {
                target = target.Parent;
                if (target is OptionObject optionO)
                    optionObject = optionO;
                if (target is OptionCollections optionC)
                    optionObject = optionC;
                if (target is OptionDefinition optionD)
                    optionObject = optionD;
            }
            optionObject.Recalculate();
            optionObject.Parent?.Recalculate();

        }
        public override void Recalculate()
        {
            if (!expanded || GetValue() == null)
            {
                Height.Set(46, 0);
                base.Recalculate();
                return;
            }
            float h = OptionView.ListView.GetDimensions().Height + 70;
            h = Utils.Clamp(h, 70, showMaxHeight);
            if (h < showMaxHeight && h > 80)
                showMaxHeight = Math.Max(h, 150);
            Height.Set(h, 0f);
            Parent?.Height.Set(h, 0f);
            base.Recalculate();
        }
        public override void ScrollWheel(UIScrollWheelEvent evt)
        {
            base.ScrollWheel(evt);

            if (!OptionView.ListView.IsMouseHovering)
                return;
        }
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            var dimemsion = GetDimensions();
            if ((Main.mouseY > dimemsion.Y + dimemsion.Height * .9f && IsMouseHovering) || dragging)
            {
                Main.instance.MouseText("[i:536]", 0, 0, Main.mouseX + 16, Main.mouseY - 10);
            }
            // 提示，因为OptionCollections要显示专门的操作提示（教玩家可以拖动元素），所以这里单独处理
            if (!IsMouseHovering)
                return;

            string text = "";
            if (ReloadRequired)
                text += $" - [c/{Color.Orange.Hex3()}:{Language.GetTextValue("tModLoader.ModReloadRequiredMemberTooltip")}]\n";
            text += Tooltip;

            // 这里如果Tooltip为空，就显示默认提示
            if (text == "")
                text = MyUtils.GetText("ModernConfig.NoTooltip");

            text += $"\n{MyUtils.GetText("ModernConfig.CollectionsTip")}";

            TooltipPanel.SetText(text);
        }
        float showMaxHeight = 360;
        float oldHeight;
        Vector2 mousePos;
        bool dragging;

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            var dimension = GetDimensions();
            if (evt.Target == this && evt.MousePosition.Y > dimension.Y + .9f * dimension.Height)
            {
                mousePos = evt.MousePosition;
                oldHeight = showMaxHeight;
                dragging = true;
            }
            base.LeftMouseDown(evt);
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            if (dragging)
            {
                dragging = false;

            }
            base.LeftMouseUp(evt);
        }
        protected override void OnSetValueExternal(object value)
        {
            Data = GetValue();
            SetupList();
            Recalculate();
        }
    }
}
