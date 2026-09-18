using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImproveGame.Content.Functions.Construction;
using System.Collections.ObjectModel;
using System.ComponentModel;
using BuildingData = ImproveGame.Content.Items.CreateWand.BuildingData;
using CWand = ImproveGame.Content.Items.CreateWand;
namespace ImproveGame.UserInterfaces.CreateWand;

public partial class CreateWandViewModel : ObservableObject
{
    /*
    [ObservableProperty]
    public partial string Title { get; set; } = GetText("UI.CreateWandController.Title");

    [ObservableProperty]
    public partial string ImportFromStructure { get; set; } = GetText("UI.CreateWandController.ImportFromStructure");

    [ObservableProperty]
    public partial string ImportFromDatamap { get; set; } = GetText("UI.CreateWandController.ImportFromDatamap");

    [ObservableProperty]
    public partial string BuildingMaterial { get; set; } = GetText("UI.CreateWandController.BuildingMaterial");

    [ObservableProperty]
    public partial string StructureSelection { get; set; } = GetText("UI.CreateWandController.StructureSelection");
    */

    [ObservableProperty]
    public partial BuildingData BuildingData { get; set; }

    public ObservableCollection<BuildingData> BuildingDataList { get; }

    public ObservableCollection<string> QotStructureList { get; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(BuildingData):
                CWand.SetBuildingData(BuildingData);
                break;
        }
    }
    public CreateWandViewModel()
    {
        BuildingDataList = [];

        foreach (var data in CWand.BuildingDataPreview.Keys)
            BuildingDataList.Add(data);
        CWand.OnBuildingDataPreviewAdded += CWand_OnBuildingDataPreviewAdded;

        QotStructureList = [];
        FileOperator.OnFileListChanged += StructureGUI_OnStructureFilesUpadted;
        StructureGUI_OnStructureFilesUpadted();
    }

    private void StructureGUI_OnStructureFilesUpadted()
    {
        QotStructureList.Clear();
        // 导入qot建筑列表
        var filePaths = Directory.GetFiles(FileOperator.SavePath);
        foreach (string path in filePaths)
        {
            string extension = Path.GetExtension(path);
            // 添加识别 .qolstruct，旧版支持
            if (extension == FileOperator.Extension || extension == ".qolstruct")
                QotStructureList.Add(path);
        }
    }

    private void CWand_OnBuildingDataPreviewAdded(BuildingData data)
    {
        BuildingDataList.Add(data);
    }

    private CWand _model;

    public void SetModel(CWand wand) => _model = wand;

    [RelayCommand]
    public void SetBuildingData(BuildingData data)
    {
        BuildingData = data;
    }

    public void SetMaterial(Item item, int index)
    {
        _model?.BuildingMaterials[index] = item;
    }

    [RelayCommand]
    public static void RegisterFromQotStructure(string path)
    {
        CWand.RegisterFromQotStructureFile(path);
    }

    [RelayCommand]
    public static void RegisterFromDatamap()
    {
        CWand.OpenDialogAndChooseDataMap();
    }

    [RelayCommand]
    public static void OpenFolder()
    {
        TrUtils.OpenFolder(Path.Combine(Main.SavePath, "Mods", "ImproveGame", "CreateWand"));
    }
}
