using ImproveGame.Common.Configs;
using SilkyUIFramework;
using SilkyUIFramework.Attributes;
using SilkyUIFramework.Elements;

namespace ImproveGame.UserInterfaces.AutoTrash;

[RegisterUI("Auto Trash")]
public partial class AutoTrashListUI : BaseBody
{
    private LocalizedText _titleText;

    public override bool Enabled
    {
        set
        {
            if (field == value) return;
            field = value;
            SoundEngine.PlaySound(field ? SoundID.MenuOpen : SoundID.MenuClose);
        }
        get => UIConfigs.Instance.QoLAutoTrash && Main.playerInventory && field;
    }

    public void Toggle()
    {
        if (Enabled = !Enabled) UISceneManager.Instance.Activate(this);
    }

    protected override void OnInitialize()
    {
        InitializeComponent();

        BorderColor = SUIColor.Border;
        BackgroundColor = SUIColor.Background * 0.75f;

        Header.ControlTarget = this;
        Title.UseDeathText();
        _titleText = Language.GetText("Mods.ImproveGame.UI.AutoTrash.TrashedItemsList");
        Title.Text = _titleText.Value;

        //CloseButton.Texture2D = Main.Assets.Request<Texture2D>("Images/UI/SearchCancel");
        CrossButton.LeftMouseDown += (_, _) => Enabled = false;
    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);

        Title.Text = _titleText.Value;
        GarbageList.RefreshItems();
    }
}
