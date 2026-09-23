using ImproveGame.Common.ModSystems;
using ImproveGame.Core;
using ImproveGame.UI;
using ImproveGame.UIFramework;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Functions.Construction
{
    public class FileOperator : ILoadable
    {
        internal static Dictionary<string, TagCompound> CachedStructureDatas = new();
        internal static string SavePath => Paths.SavePath;
        internal static string Extension => ".qotstruct";

        public static event Action OnFileListChanged;

        /// <summary>
        /// 暴露给StruceturePanel来触发事件...重构构造魔杖之前只能这么干了
        /// </summary>
        public static void CallFileListChanged() => OnFileListChanged?.Invoke();

        public static void SaveAsFile(Rectangle rectInWorld)
        {
            TrUtils.TryCreatingDirectory(SavePath);

            string name = $"{MyUtils.GetText("ConstructGUI.Structure")}{Extension}";
            string thisPath = Path.Combine(SavePath, name);
            if (File.Exists(thisPath))
            {
                for (int i = 2; i <= 999; i++)
                {
                    name = $"{MyUtils.GetText("ConstructGUI.Structure")} ({i}){Extension}";
                    thisPath = Path.Combine(SavePath, name);
                    if (!File.Exists(thisPath))
                    {
                        break;
                    }
                }
            }

            TagIO.ToFile(new QoLStructure(rectInWorld).Tag, thisPath);

            MyUtils.AddNotification(MyUtils.GetText("ConstructGUI.SavedAs") + name, Color.Yellow);
            // AddNotification(GetText("ConstructGUI.SavedAs") + thisPath, Color.Yellow);

            CachedStructureDatas.Clear();
            if (StructureGUI.Visible && UISystem.Instance.StructureGUI is not null)
            {
                UISystem.Instance.StructureGUI.CacheSetupStructures = true;
                if (WandSystem.ConstructFilePath == thisPath)
                    WandSystem.ConstructFilePath = string.Empty;
            }
            OnFileListChanged?.Invoke();
        }

        public static bool LoadFile(string path)
        {
            TagCompound tag = TagIO.FromFile(path);
            CachedStructureDatas.Add(path, tag);
            return true;
        }

        public static TagCompound GetTagFromFile(string path)
        {
            if (!CachedStructureDatas.TryGetValue(path, out TagCompound tag))
            {
                try
                {
                    LoadFile(path);
                }
                catch
                {
                    MyUtils.AddNotification(MyUtils.GetText("ConstructGUI.FileInfo.LoadError"), Color.Red);
                    return null;
                }

                tag = CachedStructureDatas[path];
            }

            return tag;
        }

        public void Load(Mod mod) { }

        public void Unload() => CachedStructureDatas = null;
    }
}
