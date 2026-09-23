using ImproveGame.Common.Configs;
using ImproveGame.Common.Configs.FavoritedSystem;
using ImproveGame.Common.ModSystems;
using ImproveGame.UI.ModernConfig.Categories;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;
using ImproveGame.UIFramework.SUIElements;
using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.ModernConfig.OptionElements;

public class ModernConfigOption : TimerView
{
    public static ModernConfigOption WrapIt(UIElement parent, ModConfig modConfig, PropertyFieldWrapper variable, object item, object list = null, Type arrayType = null, int index = -1, ModernConfigOption owner = null, Action<ModernConfigOption> preLabelAppend = null)
    {
        Type type = variable.Type;
        if (arrayType != null)
        {
            type = arrayType;
        }
        ModernConfigOption option;
        if (type == typeof(bool))
            option = new OptionToggle();
        else if (OptionSlider.SupportedTypes.Contains(type))
            option = new OptionSlider();
        else if (type == typeof(Vector2))
            option = new OptionVector2();
        else if (type == typeof(Color))
            option = new OptionColor();
        else if (type.IsEnum)
            option = new OptionDropdownList();
        else if (type == typeof(string))
        {
            var ost = ConfigManager.GetCustomAttributeFromMemberThenMemberType<OptionStringsAttribute>(variable, item, list);
            option = ost != null ? new OptionDropdownList() : new OptionEditableText();
        }
        else if (type.IsArray)
            option = new OptionArray();
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            option = new OptionList();
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
            option = new OptionHashSet();
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            option = new OptionDictionary();
        else if (type.IsSubclassOf(typeof(EntityDefinition)))
            option = new OptionDefinition();
        else if (type == typeof(object))
            option = new OptionNotSupportText();
        else
            option = new OptionObject();
        //{
        //    var attri = ConfigManager.GetCustomAttributeFromMemberThenMemberType<CustomModConfigItemAttribute>(variable, item, list);
        //    option = attri != null ? new OptionCustomUIConfig() : new OptionObject();

        //}

        option.index = index;
        option.List = (IList)list;
        option.Item = item;
        option.path = [];
        if (owner != null)
        {
            //if(ConfigOptionsPanel.GlobalPath != null)
            //    option.path.AddRange(ConfigOptionsPanel.GlobalPath);
            if (owner.path != null)
                option.path.AddRange(owner.path);
            if (owner.List == null)
                option.path.Add(owner.VariableInfo.Name);
            //else
            //    option.path.Add(variable.Name);
            if (list != null)
                option.path.Add(index.ToString());
            option.owner = owner;
        }
        /*if (parent is ModernConfigOption parentOption)
        {
            if (parentOption.path != null)
                option.path.AddRange(parentOption.path);
            option.path.Add(parentOption.VariableInfo.Name);
        }*/
        try
        {
            option.Bind(modConfig, variable, preLabelAppend);

        }
        catch
        {
            option = new OptionNotSupportText();
            option.Bind(modConfig, variable, preLabelAppend);

        }
        /*if (parent.Parent.Parent is SUIScrollViewDraggingSortable scroll)
            scroll.AddToList(option);
        else*/
        option.JoinParent(parent);

        return option;
    }
    public static PropertyFieldWrapper GetWrapper(Type type, string optionName)
    {
        BindingFlags flag = BindingFlags.Instance | BindingFlags.Public;
        var fieldInfo = type.GetField(optionName, flag);
        var propertyInfo = type.GetProperty(optionName, flag);
        PropertyFieldWrapper result = null;
        if (fieldInfo != null)
            result = new PropertyFieldWrapper(fieldInfo);
        else if (propertyInfo != null)
            result = new PropertyFieldWrapper(propertyInfo);
        else
            throw new Exception($"Field \"{optionName}\" not found in type \"{type.Name}\"");
        return result;
    }
    protected static Mod _modInMemory;
    protected static Category _categoryInMemory;
    public virtual int labelReservedWidth => 70;
    public Type VarType => List != null ? List[index].GetType() : VariableInfo.Type;

    // 原来的构造函数改成OnBind了
    // 因为现在要构造出来另外赋一些值再Bind，都写构造函数太杂乱了
    public void Bind(ModConfig config, PropertyFieldWrapper propertyFieldWrapper, Action<ModernConfigOption> preLabelAppend)
    {
        Config = config;
        OptionName = propertyFieldWrapper.Name;
        VariableInfo = propertyFieldWrapper;
        Item ??= ConfigOptionsPanel.GlobalItem ?? config;
        if (ConfigOptionsPanel.GlobalPath != null)
            path ??= [.. ConfigOptionsPanel.GlobalPath];
        RelativeMode = RelativeMode.Vertical;
        OverflowHidden = true;
        Width.Set(0f, 1f);
        Height.Set(46f, 0f);
        Rounded = new Vector4(12f);
        SetPadding(12, 4);
        oldValue = GetValue();
        if (GetAttribute<CustomModConfigItemAttribute>() != null)
        {
            var customButton = new SUIDrawingImage(v =>
            {
                var dimension = v.GetDimensions();
                if (v.IsMouseHovering)
                    SDFGraphics.BarStarX(dimension.Center(), new(.5f), dimension.Width * .5f, 4, .5f, TextureAssets.Extra[180].Value, Main.GlobalTimeWrappedHourly, 0.15f, MyUtils.GetMatrix(true));
                else
                    SDFGraphics.NoBorderStarX(dimension.Center(), new(.5f), dimension.Width * .5f, 4, .5f, Color.Yellow, MyUtils.GetMatrix(true));
                //SDFRectangle.HasBorder(dimension.Position(),dimension.Size(),v.Rounded,v.BgColor,v.);
            })
            {
                RelativeMode = RelativeMode.None,
                //BgColor = Color.Black * 0.4f,
                //Rounded = new Vector4(4f),
                //Left = new(-120, 1),
                Top = new(6, 0),
                Width = new(25, .0f),
                Height = new(25, .0f)
            };
            customButton.OnLeftClick += (evt, elem) =>
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                _modInMemory = ModernConfigUI.Instance.currentMod;
                _categoryInMemory = ConfigOptionsPanel.CurrentCategory;
                Config.Open(() =>
                {
                    ModernConfigUI.Instance.Open(_modInMemory);
                    ConfigOptionsPanel.CurrentCategory = _categoryInMemory;
                }, OptionName);
            };
            customButton.OnMouseOver += (evt, elem) =>
            {
                string info = MyUtils.GetText("ModernConfig.CustomItemTip");
                ModernConfigUI.PopNewInfo(info, elem.GetDimensions().Position() - FontAssets.MouseText.Value.MeasureString(info), Color.Cyan);
            };

            // 这个按钮是如果有CustomModConfigItemAttribute的话，点击就会跳转过去，但是我觉得这个功能没什么用，而且和整体风格不匹配，所以删了
            // 取消注释下面的代码就可以恢复
            // customButton.JoinParent(this);
        }

        preLabelAppend?.Invoke(this);

        var labelElement = new OptionLabelElement(config, OptionName, labelReservedWidth, Label)
        {
            RelativeMode = RelativeMode.Horizontal,
            IgnoresMouseInteraction = true,
            ResetAnotherPosition = true,
        };
        labelElement.OnUpdate += _ =>
        {
            labelElement.TextColor = MarkedAsFavorite
                ? Color.Gold
                : Color.White;
            if (ReloadRequired)
                labelElement.DisplayText = MyUtils.ConvertLeftRight(Label) + (ValueChanged ? $" - [c/FF0000:{Language.GetTextValue("tModLoader.ModReloadRequired")}]" : "");
            //else if (labelElement.DisplayText == "")
            //    labelElement.DisplayText = ConvertLeftRight(Label);
            else
                labelElement.DisplayText = MyUtils.ConvertLeftRight(Label);

        };
        labelElement.JoinParent(this);
        CheckAttributes();
        ResetDebugText();
        OnBind();
    }

    protected virtual void OnBind()
    {


    }
    public void SetValueDirect(object value, bool broadCast = true)
    {
        if (!Interactable) return;
        blockNextCheck = true;

        if (item.GetType().IsValueType)//VariableInfo.Type
        {
            VariableInfo.SetValue(item, value);
            owner?.SetValueDirect(item, broadCast);
        }
        else
            ConfigHelper.SetConfigValue(Config, VariableInfo, value, Item, broadCast, path, List, index);

    }
    protected T GetAttribute<T>() where T : Attribute => ConfigManager.GetCustomAttributeFromMemberThenMemberType<T>(VariableInfo, Item, List);
    protected object GetValue()
    {
        if (List != null)
            return List[index];

        return VariableInfo.GetValue(Item);
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        var displayConditionAttribute = VariableInfo.MemberInfo.GetCustomAttribute<DisplayConditionAttribute>();
        if (displayConditionAttribute == null)
        {
            base.Draw(spriteBatch);
            DrawDebugText(spriteBatch);
            return;
        }

        if (displayConditionAttribute.IsVisible)
        {
            if (Height.Pixels != 40)
            {
                Height.Set(44, 0f);
                Parent.Recalculate();
            }
        }
        else
        {
            if (Height.Pixels != 0)
            {
                Height.Set(0, 0f);
                Parent.Recalculate();
            }
        }

        base.Draw(spriteBatch);

        if (displayConditionAttribute.IsVisible)
            DrawDebugText(spriteBatch);
    }

    // 为了让UI之间实际上无间隔，防止鼠标滑过时Tooltip文字闪现，这里重写绘制，而不使用Spacing
    public override void DrawSelf(SpriteBatch spriteBatch)
    {

        var dimensions = GetDimensions();
        var dimensionsRect = dimensions.ToRectangle();
        var position = dimensions.Position();
        var size = dimensions.Size();

        // 这里修改这两个值，而不使用Spacing
        position.Y += 3f;
        size.Y -= 6f;

        // 背景板
        //var panelColor = HoverTimer.Lerp(UIStyle.PanelBgLight, UIStyle.PanelBgLightHover);
        Color panelColor;
        if (BgColor != Color.Transparent)
            panelColor = HoverTimer.Lerp(BgColor.MultiplyRGBA(new Color(100, 100, 100)), BgColor);
        else
            panelColor = HoverTimer.Lerp(UIStyle.ListItemColor * 0.2f, UIStyle.ListItemColor * 0.4f);
        if (!Interactable)
            panelColor = Color.Gray * 0.3f;

        if (Highlighted)
        {
            SDFRectangle.HasBorder(position, size, new Vector4(8f), panelColor, UIStyle.ItemSlotBorderSize, UIStyle.ItemSlotBorderFav * 0.8f, Main.UIScaleMatrix);
        }
        else
        {
            SDFRectangle.NoBorder(position, size, new Vector4(8f), panelColor * 0.8f, Main.UIScaleMatrix);
        }

        // 提示
        if (!IsMouseHovering)
            return;

        string text = "";
        if (ReloadRequired)
            text += $" - [c/{Color.Orange.Hex3()}:{Language.GetTextValue("tModLoader.ModReloadRequiredMemberTooltip")}]\n";
        text += Tooltip;
        TooltipPanel.SetText(text);
        // 不可控制，为什么呢？



        bool f = CantOperateDueToOnlyGetter;

        if (f)
        {
            string readOnlyTip = MyUtils.GetText("ModernConfig.ReadOnlyTip");
            UICommon.TooltipMouseText(readOnlyTip);
        }
        if (Interactable)
            return;

        if (CantOperateDueToOnlyGetter)
        {
            string readOnlyTip = MyUtils.GetText("ModernConfig.ReadOnlyTip");
            UICommon.TooltipMouseText(readOnlyTip);
        }
        else if (ReloadRequired)
        {
            string reloadTip =
                Language.GetTextValue("tModLoader.ModConfigCantSaveBecauseChangesWouldRequireAReload");
            UICommon.TooltipMouseText(reloadTip);
        }
        else if (CantOperateDueToHostVerification)
        {
            string hostTip = MyUtils.GetText("Configs.ImproveConfigs.OnlyHost.Tips");
            UICommon.TooltipMouseText(hostTip);
        }
        else if (CantOperateDueToPasswordVerification)
        {
            string passwordTip = MyUtils.GetText("Configs.ImproveConfigs.OnlyHostByPassword.Tips");
            UICommon.TooltipMouseText(passwordTip);
        }
        //else if (CantOperateDueToCustomCondition(out var networkText)) 
        //{
        //    string tip = networkText.ToString();
        //    UICommon.TooltipMouseText(tip);
        //}
    }

    private void DrawDebugText(SpriteBatch spriteBatch)
    {
        if (!UIConfigs.Instance.ShowMoreData)
            return;

        var dimensions = GetDimensions();
        var dimensionsRect = dimensions.ToRectangle();
        var position = dimensions.Position();
        var size = dimensions.Size();

        // 文字
        var text = DebugText ?? "";
        var textPosition = dimensionsRect.Top();
        textPosition.Y += 6;
        textPosition.X -= 80;

        MyUtils.DrawString(textPosition, text, Color.Gray, Color.Black, Vector2.Zero, 0.8f, false, 1);

        // 不希望绘制Tag文字
        // ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, textPosition, Color.Gray, Color.Black, 0f, Vector2.Zero, new Vector2(0.8f), -1f, 1f);
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        if (VariableInfo.MemberInfo.DeclaringType.IsSubclassOf(typeof(ModConfig)))
            TooltipPanel.SetOption(this);
    }
    public override void MouseOut(UIMouseEvent evt)
    {
        base.MouseOut(evt);
        if (VariableInfo.MemberInfo.DeclaringType.IsSubclassOf(typeof(ModConfig)))
            TooltipPanel.SetOption(null);
    }

    /// <summary>
    /// 外部操作(如联机同步)写入新值时的更新
    /// </summary>
    /// <param name="value"></param>
    protected virtual void OnSetValueExternal(object value)
    {

    }

    public override void RightMouseDown(UIMouseEvent evt)
    {
        base.RightMouseDown(evt);
        if (evt.Target != this || !Interactable) return;

        var defaultValueAttribute = GetAttribute<DefaultValueAttribute>();
        bool cantSetValue = false;
        object defaultValue = defaultValueAttribute?.Value; // 有默认值标签就默认值优先
        if (defaultValue == null || index > -1)
        {
            string json = "{}";
            var dummyConfig = Config.Clone();
            JsonConvert.PopulateObject(json, dummyConfig, ConfigManager.serializerSettings); // 否则从默认的config里面摘录出来

            var item = ConfigHelper.GetItemViaPathForSetDefault(dummyConfig, path, out cantSetValue);
            // 会有一个专门的ForSetDefault版本是因为Config这里实现的特殊性
            // 像列表那些，下标在原始config中有的就采取那个的值，否则采取list内新添加元素的默认值
            // 除此之外，如果点到了字典里的pair，那么还要另外转字典内那个代理元素
            var itemType = item?.GetType();
            if (itemType != VarType)//path == null || path.Count == 0 || !int.TryParse(path[^1], out _) ||
            {
                if (item == null)
                    cantSetValue = true;
                else
                {
                    if (item is Color color)
                        item = Activator.CreateInstance(VariableInfo.MemberInfo.DeclaringType, [color]);
                    // 欸，这里转ColorHandler为什么不像字典的那个一样在上面函数里就转好，我不知道，当时做昏头了有点，但是下面还会有别的特殊处理的，嗯

                    //if (itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    //    defaultValue = itemType.GetProperty(VariableInfo.Name, BindingFlags.Instance | BindingFlags.Public).GetValue(item);
                    //else
                    defaultValue = VariableInfo.GetValue(item);

                }
            }
            else
                defaultValue = item;

        }
        if (!cantSetValue)
        {
            // 直接交由子元素代理赋值了
            switch (defaultValue)
            {
                case ISetElementWrapper setWrapper:
                {
                    if (this is OptionObject objectOption && ConfigHelper.GetItemViaPath(objectOption, ["OptionView", "ListView", "Elements"], true) is IEnumerable elems)
                        foreach (var elem in elems)
                        {
                            if (elem is ModernConfigOption subOption && subOption.VariableInfo.Name == "Value")
                            {
                                subOption.SetValueDirect(setWrapper.Value);
                                break;
                            }
                        }
                    break;
                }
                case IDictionaryElementWrapper dictWrapper:
                {
                    if (this is OptionObject objectOption && ConfigHelper.GetItemViaPath(objectOption, ["OptionView", "ListView", "Elements"], true) is IEnumerable elems)
                        foreach (var elem in elems)
                        {
                            if (elem is ModernConfigOption subOption)
                            {
                                if (subOption.VariableInfo.Name == "Value")
                                    subOption.SetValueDirect(dictWrapper.Value);
                                else if (subOption.VariableInfo.Name == "Key")
                                    subOption.SetValueDirect(dictWrapper.Key);
                            }
                        }
                    break;
                }
                default:
                {
                    SetValueDirect(defaultValue);
                    break;
                }
            }
            OnSetValueExternal(defaultValue);
            //Recalculate();
            SoundEngine.PlaySound(SoundID.Chat);
        }

    }

    public override void MiddleMouseDown(UIMouseEvent evt)
    {
        base.MiddleMouseDown(evt);
        FavoritedOptionDatabase.ToggleFavoriteForOption(Config, OptionName);

        if (ConfigOptionsPanel.CurrentCategory.LocalizationKey is nameof(Favorites))
            ConfigOptionsPanel.Instance.RefreshCurrentPage();

        // 特效
        SoundEngine.PlaySound(FavoritedOptionDatabase.IsFavorited(Config, OptionName)
            ? SoundID.ResearchComplete
            : SoundID.Research);
        ModernConfigUI.Instance.GenerateParticleAtMouse();
    }

    protected virtual void CheckAttributes()
    {
        //我把只有输入框那个删了，然后Min这些值交由Slider处理，毕竟只有它用得到这些
        ReloadRequired = GetAttribute<ReloadRequiredAttribute>() is not null;
        if (ReloadRequired && List == null && Item is ModConfig modConfig)
        {
            ModConfig loadTimeConfig = ConfigManager.GetLoadTimeConfig(modConfig.Mod, modConfig.Name);
            LoadTimeValue = VariableInfo.GetValue(loadTimeConfig);
        }
        var colorAttribute = GetAttribute<BackgroundColorAttribute>();
        if (colorAttribute != null)
            BgColor = colorAttribute.Color;
    }

    public void ResetDebugText()
    {
        DebugText = $"cfg:{Config.Name}, opt:{OptionName}, label:{Label}";
    }

    public override void Update(GameTime gameTime)
    {
        if (CheckExternalModify)
        {
            var value = GetValue();

            if (oldValue?.Equals(value) != true && !(oldValue == null && value == null) && !blockNextCheck)
                OnSetValueExternal(value);
            oldValue = value;
            blockNextCheck = false;
        }

        base.Update(gameTime);
    }


    /// <summary>
    /// 是否被高光显示，用于搜索
    /// </summary>
    public bool Highlighted;

    protected bool Interactable => !CantOperateDueToOnlyGetter && (!CantOperateInGame || Main.gameMenu);

    public virtual string Label => List != null ? (index + 1).ToString() : (ConfigManager.GetLocalizedText<LabelKeyAttribute, LabelArgsAttribute>(VariableInfo, "Label") ?? ConfigHelper.GetLabel(Config, OptionName));
    public string Tooltip => ConfigManager.GetLocalizedText<TooltipKeyAttribute, TooltipArgsAttribute>(VariableInfo, "Tooltip") ?? ConfigHelper.GetTooltip(Config, OptionName);

    //public FieldInfo FieldInfo { get; }
    public PropertyFieldWrapper VariableInfo { get; private set; }
    public string DebugText { get; set; }
    public ModConfig Config { get; private set; }
    public string OptionName { get; private set; }

    internal bool ReloadRequired;
    internal LabelKeyAttribute LabelKeyAttribute;
    internal TooltipKeyAttribute TooltipKeyAttribute;

    private bool CantOperateDueToHostVerification =>
        Config.Mode is ConfigScope.ServerSide && Main.netMode is NetmodeID.MultiplayerClient &&
        (Config is ImproveConfigs configs && configs.OnlyHost) && !Main.countsAsHostForGameplay[Main.myPlayer];

    private bool CantOperateDueToPasswordVerification =>
        Config.Mode is ConfigScope.ServerSide && Main.netMode is NetmodeID.MultiplayerClient &&
        (Config is ImproveConfigs configs && configs.OnlyHostByPassword) && !NetPasswordSystem.LocalPlayerRegistered;

    private bool CantOperateDueToOnlyGetter =>
        !VariableInfo.CanWrite;

    private bool CantOperateInGame =>
        ReloadRequired || CantOperateDueToPasswordVerification || CantOperateDueToHostVerification;//|| CantOperateDueToCustomCondition(out _);

    private bool CantOperateDueToCustomCondition(out NetworkText text)
    {
        text = NetworkText.FromKey("tModLoader.ModConfigAccepted");
        return Config.AcceptClientChanges(Config, Main.myPlayer, ref text);
    }

    private bool MarkedAsFavorite =>
        FavoritedOptionDatabase.FavoritedOptions.Contains($"{Config.Name}.{OptionName}") &&
        ConfigOptionsPanel.CurrentCategory.LocalizationKey is not nameof(Favorites);

    // 是的是的，我不仅把原版那一套抄了一点过来让一切变得更混乱，还自己加了点更奇怪的东西
    /// <summary>
    /// 指向的目标所属的List
    /// </summary>
    public IList List { get; set; }
    /// <summary>
    /// List中的索引
    /// </summary>
    public int index = -1;
    /// <summary>
    /// 当前设置块指向的目标
    /// <br>为了和object等适配，不能直接在config改了</br>
    /// </summary>
    public object Item
    {
        get => item;
        set => item = value;
    }
    object item;
    /// <summary>
    /// 导航到目标字段的路径，具体ctrl点进来看注释
    /// </summary>
    public List<string> path;
    // 到当前目标的字段/属性路径  直接在某config下是null 在它的字段myField下是["myField"]，再在字段myField2下是["myField","myField2"]，依此类推
    // 是为了和联机同步那边的代码实现妥协的产物，那边之前直接是给config的某个字段设置就直接很多，这里不得不记录下字段路径了
    // 原版的做法似乎是直接把整个config都重新写入了一遍？
    public ModernConfigOption owner;//当前选项所属的设置选项
    object LoadTimeValue;
    bool blockNextCheck;
    object oldValue;//上一帧的值，用于检测是否被外部修改
    protected bool CheckExternalModify = true;
    protected bool ValueChanged => !ConfigManager.ObjectEquals(LoadTimeValue, GetValue());
}