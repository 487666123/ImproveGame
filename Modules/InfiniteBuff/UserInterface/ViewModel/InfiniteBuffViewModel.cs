using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImproveGame.Styles;
using SilkyUIFramework.Interfaces;
using System.ComponentModel;

namespace ImproveGame.Modules.InfiniteBuff.UserInterface.ViewModel;

public partial class InfiniteBuffLocalization
{
    private static string GetTextValue(string key) => Language.GetTextValue($"Mods.ImproveGame.UI.InfiniteBUFFController.{key}");

    public string Title { get; set; } = GetTextValue("DisplayName");

    public string Placeholder { get; set; } = GetTextValue("Placeholder");

    public string EnemySpawnRate { get; set; } = GetTextValue("EnemySpawnRate");
}

public sealed partial class InfiniteBuffViewModel : ObservableObject, IUpdatable, IDisposable
{
    /// <summary>
    /// 共享唯一一个 <see cref="DefaultStyle"/>
    /// </summary>
    [ObservableProperty]
    public partial DefaultStyle Style { get; set; } = DefaultStyle.Instance;

    [ObservableProperty]
    public partial InfiniteBuffLocalization Localization { get; set; } = new();

    [ObservableProperty]
    public partial Asset<Texture2D> SearchCancel { get; set; } = Main.Assets.Request<Texture2D>("Images/UI/SearchCancel");

    [ObservableProperty]
    public partial Asset<Texture2D> EyeSwitch { get; set; } = ModAsset.EyeSwitch;

    private readonly SpawnRateSliderModPlayer _model;

    public InfiniteBuffViewModel()
    {
        _model = Main.LocalPlayer.GetModPlayer<SpawnRateSliderModPlayer>();
        _model.PropertyChanged += OnModelPropertyChanged;

        OpenSlider = _model.ShowSlider;
        SliderValue = new Vector2(_model.SpawnRateSliderValue, 0);
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender is not SpawnRateSliderModPlayer player) return;
        switch (e.PropertyName)
        {
            case nameof(SpawnRateSliderModPlayer.ShowSlider):
            {
                OpenSlider = player.ShowSlider; break;
            }
            case nameof(SpawnRateSliderModPlayer.SpawnRateSliderValue):
            {
                SliderValue = new Vector2(player.SpawnRateSliderValue, 0); break;
            }
        }
    }

    [ObservableProperty]
    public partial Vector2 SliderValue { get; private set; }

    [RelayCommand]
    private void SetSliderValue(Vector2 value) => _model.SpawnRateSliderValue = value.X;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HiddenSlider))]
    public partial bool HiddenSliderEyeButton { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HiddenSlider))]
    [NotifyPropertyChangedFor(nameof(EyeColor))]
    public partial bool OpenSlider { get; private set; }

    public bool HiddenSlider => HiddenSliderEyeButton || !OpenSlider;

    public Color EyeColor => OpenSlider ? Color.White : Color.White * 0.5f;

    [RelayCommand] public void ToggleSliderVisibility() => _model.ShowSlider = !_model.ShowSlider;

    void IUpdatable.Update(GameTime gameTime)
    {
        if (!Main.LocalPlayer.TryGetModPlayer<InfiniteBuffModPlayer>(out var infinitePlayer)) return;
        HiddenSliderEyeButton = !infinitePlayer.MeetsBattlerCombination();
    }

    public void Dispose() => _model.PropertyChanged -= OnModelPropertyChanged;
}