using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.UI.ModernConfig;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace ImproveGame.Packets;

//[AutoSync]// 不知道为什么path会同步失败，就自己发包了
public class ConfigOptionPacket : NetModule
{
    private string _modName;
    private string _configName;
    private string _json;
    private string _valueTypeFullName;
    private string[] path;
    private string _popInfo;
    private bool _rejected;
    private bool isNull;
    public static void Send(ModConfig modConfig, object value, List<string> path = null)
    {
        #region ColorHandlerCheck
        if (path != null && path.Count > 0)
        {
            // 我们发现要同步的值发送自ColorHandler，所以就干脆写入它处理好的Color本身
            string[] colorPropNames = ["Red", "Green", "Blue", "Hue", "Saturation", "Lightness", "Alpha", "Hex"];
            if (colorPropNames.Contains(path[^1]) && ConfigHelper.GetItemViaPath(modConfig, path[..^1]) is Color color)
            {
                value = color;
                path.RemoveAt(path.Count - 1);
            }
            if (path.Count > 2)
            {
                // HashSet中的元素的编辑是用IHashSetWrapper实现的，它有个Value属性，而这个在原本的HashSet中的元素显然是没有的
                // 字典虽然用类似的手段实现，但是字典直接遍历得到的是键值对，也就是本来就有Value属性，所以不需要移除最后一层路径
                if (path[^1] == "Value")
                {
                    var setType = ConfigHelper.GetItemViaPath(modConfig, path[..^2]).GetType();
                    if (setType.IsGenericType && setType.GetGenericTypeDefinition() == typeof(HashSet<>))
                    {
                        path.RemoveAt(path.Count - 1);
                    }
                }
            }
        }
        #endregion
        var module = NetModuleLoader.Get<ConfigOptionPacket>();
        module._modName = modConfig.Mod.Name;
        module._configName = modConfig.Name;
        if (value == null)
            // 我不知道null序列化会变成什么，就算是{}那也很可能会对不上号，就加了个isNull
            module.isNull = true;
        else
        {
            string json = JsonConvert.SerializeObject(value, ConfigManager.serializerSettings);
            module._json = json;
            module.isNull = false;
            module._valueTypeFullName = value.GetType().FullName + "," + value.GetType().Assembly.FullName;
            // 上面这个是反序列化的时候查找类型用的，虽然不知道为什么有时候还是查找不到
        }
        List<string> cachedPath = [];
        if (path != null)
            cachedPath.AddRange(path);
        module.path = [.. cachedPath]; // 其实按说这里如果path是null应该可以直接报警(划掉) 报错了，但是万一有直接给config本身赋值的时候呢..?
        path?.ToArray();
        module._popInfo = "";
        module._rejected = false;
        module.Send();
    }
    public override void Send(ModPacket p)
    {
        p.Write(_modName);
        p.Write(_configName);
        p.Write(isNull);
        if (!isNull)
        {
            p.Write(_json);
            p.Write(_valueTypeFullName);
        }

        p.Write(path.Length);
        foreach (var str in path)
            p.Write(str);
        p.Write(_popInfo);
        p.Write(_rejected);
        base.Send(p);
    }
    public override void Read(BinaryReader r)
    {
        _modName = r.ReadString();
        _configName = r.ReadString();
        isNull = r.ReadBoolean();
        if (!isNull)
        {
            _json = r.ReadString();
            _valueTypeFullName = r.ReadString();
        }
        int length = r.ReadInt32();
        path = new string[length];
        for (int n = 0; n < length; n++)
            path[n] = r.ReadString();
        _popInfo = r.ReadString();
        _rejected = r.ReadBoolean();
        base.Read(r);
    }
    public override void Receive()
    {

        var modConfig = ConfigManager.Configs[ModLoader.GetMod(_modName)].Find(i => i.Name == _configName);
        if (Main.netMode is NetmodeID.Server)
        {
            if (_modName == "ImproveGame" && ImproveConfigs.Instance.OnlyHostByPassword && !NetPasswordSystem.Registered[Sender])// 理论上不可能出现的情况，没有验证还是发了包
            {
                //ChatHelper.SendChatMessageToClient(
                //        new NetworkText(GetText("Configs.ImproveConfigs.OnlyHostByPassword.Unaccepted"),
                //        NetworkText.Mode.Literal), Color.Red, Sender);
                SendRejectedConfig(MyUtils.GetText("Configs.ImproveConfigs.OnlyHostByPassword.Unaccepted"), modConfig);
                return;
            }

        }
        object value;
        if (isNull)
            value = null;
        else
        {

            var valueTypeFullName = System.Type.GetType(_valueTypeFullName);
            //valueTypeFullName ??= TypeDescriptor.GetConverter(typeof(Type)).ConvertFrom(_valueTypeFullName) as Type;
            valueTypeFullName ??= ConfigHelper.GetTypeViaPath(modConfig, path, true); // 如果通过发来的信息没找到，通过path来直接查找到某个子对象的type
            if (valueTypeFullName == null)
                throw new Exception($"Type Not Found:{_valueTypeFullName}");
            value = JsonConvert.DeserializeObject(_json, valueTypeFullName, ConfigManager.serializerSettings);
        }


        // 转发到全体
        if (Main.netMode is NetmodeID.Server)
        {
            ModConfig pendingConfig = ConfigManager.GeneratePopulatedClone(modConfig);
            ConfigHelper.SetItemViaPath(pendingConfig, path, value);// 通过path直接给某个子对象写入值
            var netText = NetworkText.FromKey("tModLoader.ModConfigAccepted");
            bool flag = modConfig.AcceptClientChanges(pendingConfig, Sender, ref netText);// 检测挂起的config是否合法
            if (!flag)
            {
                SendRejectedConfig(netText.ToString(), modConfig);
                return;
            }
            var text = "";
#pragma warning disable CS0618 // Type or member is obsolete
            flag = modConfig.AcceptClientChanges(pendingConfig, Sender, ref text);
            if (!flag)
            {
                SendRejectedConfig(text, modConfig);
                return;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            ConfigHelper.SetItemViaPath(modConfig, path, value);// 给modconfig正式写入值
            try
            {
                ConfigManager.Save(modConfig);// try一脚是因为我自己测的时候总是有文件访问冲突
            }
            catch { }
            if (!Main.gameMenu)
                modConfig.OnChanged();
            _popInfo = Language.GetTextValue("tModLoader.ModConfigServerResponse", netText.ToString());
            _rejected = false;
            Send();// 转发给所有客户端，赋值大胜利
        }
        else
        {
            ConfigHelper.SetItemViaPath(modConfig, path, value);
            if (!Main.gameMenu)
                modConfig.OnChanged();
            if (_popInfo != null && _popInfo.Length > 0)
            {
                if (ModernConfigUI.Instance.Enabled)
                {
                    //ConfigOptionsPanel.Instance.RefreshCurrentPage();
                    ModernConfigUI.PopNewInfo(_popInfo, Main.MouseScreen - FontAssets.MouseText.Value.MeasureString(_popInfo) * new Vector2(1f, 0f) - new Vector2(64, 32), _rejected ? Color.Red : Color.Green);

                }
            }
        }

    }
    public void SendRejectedConfig(string text, ModConfig modConfig)
    {
        _rejected = true;
        _popInfo = text;
        //var modConfig = ConfigManager.Configs[ModLoader.GetMod(_modName)].Find(i => i.Name == _configName);
        object item = ConfigHelper.GetItemViaPath(modConfig, path);
        _json = JsonConvert.SerializeObject(item, ConfigManager.serializerSettings);
        Send(); //打回，发还原始信息并同步，不过按说这里也许发还给发送者就可以了？
    }
}

[AutoSync]
public class CompleteConfigPacket : NetModule //摆烂尝试发整个包，但是似乎又有新的问题了，就还是放弃了
{
    string _configName;
    string _modName;
    string _Json;
    string _popInfo;
    bool _rejected;
    public static void Send(ModConfig modConfig)
    {
        var module = NetModuleLoader.Get<CompleteConfigPacket>();
        module._modName = modConfig.Mod.Name;
        module._configName = modConfig.Name;
        string json = JsonConvert.SerializeObject(modConfig, ConfigManager.serializerSettings);
        module._Json = json;
        module._popInfo = "";
        module._rejected = false;
        module.Send();
    }
    public override void Receive()
    {
        var modConfig = ConfigManager.Configs[ModLoader.GetMod(_modName)].Find(i => i.Name == _configName);
        if (Main.netMode == NetmodeID.Server)
        {
            var pendingConfig = JsonConvert.DeserializeObject(_Json, modConfig.GetType()) as ModConfig;
            var netText = NetworkText.FromKey("tModLoader.ModConfigAccepted");
            bool flag = modConfig.AcceptClientChanges(pendingConfig, Sender, ref netText);
            if (!flag)
            {
                SendRejectedConfig(netText.ToString(), modConfig);
                return;
            }
            var text = "";
#pragma warning disable CS0618 // Type or member is obsolete
            flag = modConfig.AcceptClientChanges(pendingConfig, Sender, ref text);
            if (!flag)
            {
                SendRejectedConfig(text, modConfig);
                return;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            try
            {
                ConfigManager.Save(modConfig);
            }
            catch { }
            if (!Main.gameMenu)
                modConfig.OnChanged();

            JsonConvert.PopulateObject(_Json, modConfig, ConfigManager.serializerSettingsCompact);
            modConfig.OnChanged();

            _popInfo = Language.GetTextValue("tModLoader.ModConfigServerResponse", netText.ToString());
            _rejected = false;
            Send();
        }
        else
        {
            JsonConvert.PopulateObject(_Json, modConfig, ConfigManager.serializerSettingsCompact);
            modConfig.OnChanged();
            if (_popInfo != null && _popInfo.Length > 0)
            {
                if (ModernConfigUI.Instance.Enabled)
                {
                    ConfigOptionsPanel.Instance.RefreshCurrentPage();
                    ModernConfigUI.PopNewInfo(_popInfo, Main.MouseScreen - FontAssets.MouseText.Value.MeasureString(_popInfo) * new Vector2(1f, 0f) - new Vector2(64, 32), _rejected ? Color.Red : Color.Green);

                }
            }
        }
    }

    public void SendRejectedConfig(string text, ModConfig modConfig)
    {
        _rejected = true;
        _popInfo = text;
        _Json = JsonConvert.SerializeObject(modConfig, ConfigManager.serializerSettings);
        Send();
    }
}