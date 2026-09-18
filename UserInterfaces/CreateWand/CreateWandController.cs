using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;
using SilkyUIFramework.Extensions;
using SilkyUIFramework.Layout;
using SilkyUIFramework.StyleSystem;
using CWand = ImproveGame.Content.Items.CreateWand;

namespace ImproveGame.UserInterfaces.CreateWand;

[RegisterUI]
public partial class CreateWandController : BaseBody
{
    public static CreateWandController Instance { get; private set; }

    public void Toggle(CWand wand)
    {
        Enabled = !Enabled;

        if (LocalDataContext is CreateWandViewModel cwvm)
        {
            cwvm.SetModel(wand);
            for (int n = 0; n < wand.BuildingMaterials.Length; n++)
                ItemSlots_Interanl[n]?.Item = wand.BuildingMaterials[n];
        }
    }

    public override IEnumerable<UIView> BlurElements => [MainContainer];
    private SUIBuildMaterialItemSlot[] ItemSlots_Interanl { get; } = new SUIBuildMaterialItemSlot[30];
    public IReadOnlyList<SUIBuildMaterialItemSlot> ItemSlots => ItemSlots_Interanl;

    protected override void OnInitialize()
    {
        Instance = this;

        LocalDataContext = new CreateWandViewModel();

        InitializeComponent();

        MainContainer.BorderColor = SUIColor.Border;
        MainContainer.BackgroundColor = SUIColor.Background * 0.75f;

        Header.ControlTarget = this;
        Title.UseDeathText();

        X.Texture2D = ModAsset.X;
        X.LeftMouseDown += (_, _) => Enabled = false;

        Title.Text = GetText("UI.CreateWandController.Title");
        //FromDatamapButton.Text = GetText("UI.CreateWandController.ImportFromDatamap");
        MaterialButton.Text = GetText("UI.CreateWandController.BuildingMaterial");
        BuildingDataListButton.Text = GetText("UI.CreateWandController.StructureSelection");
        StructDataListButton.Text = GetText("UI.CreateWandController.ImportFromStructureFile");

        Folder.Texture2D = ModAsset.folder;
        ImportButton.Texture2D = ModAsset.Download;

        foreach (var item in new Span<SUIImage>([ImportButton, Folder, X]))
        {
            item.StyleSheet.SetStyle(UIElementState.Normal, new StyleDefinition()
            {
                [nameof(item.ImageColor)] = Color.White * 0.5f
            });

            item.StyleSheet.SetStyle(UIElementState.Hover, new StyleDefinition()
            {
                [nameof(item.ImageColor)] = Color.White
            });
        }

        foreach (var item in new Span<UIView>([MaterialButton, BuildingDataListButton, StructDataListButton]))
        {
            item.StyleSheet.SetStyle(UIElementState.Normal, new StyleDefinition()
            {
                [nameof(BackgroundColor)] = Color.Transparent
            });

            item.StyleSheet.SetStyle(UIElementState.Hover, new StyleDefinition()
            {
                [nameof(BackgroundColor)] = Color.Black * 0.25f
            });
        }

        ItemSlot_Container.Container.SetTemplateColumns(GridTrack.Repeat(6, TemplateType.Fraction, 1f));

        for (int i = 0; i < 30; i++)
        {
            var slot = new SUIBuildMaterialItemSlot()
            {
                Width = new Dimension(48),
                Height = new Dimension(48),
                BorderRadius = new Vector4(8),
                BorderColor = SUIColor.Border * 0.75f,
                BackgroundColor = SUIColor.Background * 0.5f,
                FitHeight = true,
                AspectRatio = 1,
            }.Join(ItemSlot_Container.Container);

            var k = i;
            slot.ItemChanged += (sender, arg) =>
            {
                if (LocalDataContext is CreateWandViewModel cwvm)
                    cwvm.SetMaterial(arg.NewValue, k);
            };
            ItemSlots_Interanl[i] = slot;
        }

        MaterialButton.LeftMouseClick += SwitchToMaterialList;
        BuildingDataListButton.LeftMouseClick += SwitchToBuildingDataList;
        StructDataListButton.LeftMouseClick += SwitchToStructDataList;

        BuildingDataList.ViewTemplate = StructurePreviewCardTemplate.Instance;
        StructureFileList.ViewTemplate = ConstructStructureCardTemplate.Instance;
    }

    private void SwitchToMaterialList(UIView sender, SilkyUIFramework.UIMouseEvent evt)
    {
        ItemSlot_Container.Invalid = false;
        BuildingDataListPanel.Invalid = true;
        StructureFileList.Invalid = true;
    }

    private void SwitchToBuildingDataList(UIView sender, SilkyUIFramework.UIMouseEvent evt)
    {
        ItemSlot_Container.Invalid = true;
        BuildingDataListPanel.Invalid = false;
        StructureFileList.Invalid = true;
    }

    private void SwitchToStructDataList(UIView sender, SilkyUIFramework.UIMouseEvent evt)
    {
        ItemSlot_Container.Invalid = true;
        BuildingDataListPanel.Invalid = true;
        StructureFileList.Invalid = false;
    }

    protected override void OnEnterTree()
    {
        LocalDataContext = new CreateWandViewModel();
    }
}