using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.Packets;
using ImproveGame.UI.ModernConfig.OptionElements;
using Newtonsoft.Json;
using System.Collections;
using System.Reflection;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace ImproveGame.Helpers;

public static class ConfigHelper
{
    static void InternalSetValue(ModConfig modConfig, PropertyFieldWrapper variableInfo, object value, object item, List<string> path, IList List, int Index)
    {
        if (!item.GetType().IsValueType)
            if (List != null)
                List[Index] = value;
            else
                variableInfo.SetValue(item, value);
        else
        {
            //给struct套struct做的神必适配
            //按说应该没人会把struct高强度套娃塞进ModConfig吧不会吧
            if (path == null)
                throw new Exception("You must give a path when setvalue to a struct");
            List<PropertyFieldWrapper> fldInfo = [];
            object obj = modConfig;
            int start = 0;
            IList objList = null;
            int listIndex = -1;
            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (int.TryParse(path[i], out var index))
                {
                    start = i + 1;
                    for (int k = 0; k < i; k++)
                    {
                        if (int.TryParse(path[k], out var idx))
                            obj = ((IList)obj)[idx];
                        else
                            obj = ModernConfigOption.GetWrapper(obj.GetType(), path[k]).GetValue(obj);
                    }
                    if (obj is IList list)
                    {
                        objList = list;
                        obj = objList[index];
                    }
                    else if (obj is IDictionary dict)
                    {
                        string type = path[i + 1];
                        objList = new List<object>();
                        foreach (var v in (type == "Value" ? dict.Values : dict.Keys))
                        {
                            objList.Add(v);
                        }
                        obj = objList[index];

                    }
                    listIndex = index;

                    break;
                }
            }
            List<object> objs = [obj];
            Type curType = obj.GetType();
            object curObj = obj;
            for (int i = start; i < path.Count; i++)
            {
                var wrapper = ModernConfigOption.GetWrapper(curType, path[i]);
                fldInfo.Add(wrapper);
                curObj = wrapper.GetValue(curObj);
                objs.Add(curObj);
                curType = wrapper.Type;
            }
            fldInfo.Add(variableInfo);
            variableInfo.SetValue(objs[^1], value);
            variableInfo.SetValue(item, value);//两个是不同的引用，一个在ModConfig里，一个在Option里，都得改

            int count = objs.Count;
            for (int k = 2; k <= count; k++)
            {
                fldInfo[^k].SetValue(objs[^k], objs[^(k - 1)]);
                if (!objs[^k].GetType().IsValueType && k != count)
                    break;
            }
            if (objList != null)
            {
                objList[listIndex] = objs[^1];
            }
        }
    }
    public static object GetItemViaPathForSetDefault(object target, IEnumerable<string> path, out bool failed, bool privateAllowed = false)
    {
        failed = false;
        object item = target;
        object lastItem = item;
        PropertyFieldWrapper prevWrapper = null;
        var bindFlag = BindingFlags.Public | BindingFlags.Instance;
        if (privateAllowed)
            bindFlag |= BindingFlags.NonPublic;
        if (path != null)
            foreach (var p in path)
            {
                if (item == null)
                {
                    failed = true;
                    return null;
                }
                var curType = item.GetType();
                var fld = curType.GetField(p, bindFlag);
                var prop = curType.GetProperty(p, bindFlag);
                if (fld != null)
                {
                    item = fld.GetValue(item);
                    prevWrapper = new PropertyFieldWrapper(fld);
                }
                else if (prop != null)
                {
                    item = prop.GetValue(item);
                    prevWrapper = new PropertyFieldWrapper(prop);
                }
                else if (item is IEnumerable collection && int.TryParse(p, out int index))
                {
                    int counter = 0;
                    bool useElementDefault = true;
                    foreach (var i in collection)
                    {
                        if (counter == index)
                        {
                            item = i;
                            useElementDefault = false;
                            break;
                        }
                        counter++;
                    }
                    if (useElementDefault)
                    {
                        object toAdd;
                        var DefaultListValueAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<DefaultListValueAttribute>(prevWrapper, lastItem, null);
                        var JsonDefaultListValueAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<JsonDefaultListValueAttribute>(prevWrapper, lastItem, null);
                        bool isList = curType.GetGenericTypeDefinition() == typeof(List<>);
                        bool isSet = curType.GetGenericTypeDefinition() == typeof(HashSet<>);
                        bool isDictionary = curType.GetGenericTypeDefinition() == typeof(Dictionary<,>);
                        if (!isList && !isDictionary && !isSet)
                            throw new Exception("Collection Not Support");
                        if (DefaultListValueAttribute != null)
                        {
                            toAdd = DefaultListValueAttribute.Value;
                        }
                        else
                        {
                            var type = curType.GetGenericArguments()[isDictionary ? 1 : 0];
                            toAdd = ConfigManager.AlternateCreateInstance(type);
                            if (!type.IsValueType && type != typeof(string))
                            {
                                string json = JsonDefaultListValueAttribute?.Json ?? "{}";

                                JsonConvert.PopulateObject(json, toAdd, ConfigManager.serializerSettings);
                            }
                        }
                        if (isList)
                        {
                            item = toAdd;
                        }
                        else if (isSet)
                        {
                            var genericType = typeof(SetElementWrapper<>).MakeGenericType(curType.GetGenericArguments()[0]);
                            item = Activator.CreateInstance(genericType, [toAdd, item]);
                        }
                        else
                        {
                            object keyValue;
                            var keyType = curType.GetGenericArguments()[0];

                            var defaultDictionaryKeyValueAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<DefaultDictionaryKeyValueAttribute>(prevWrapper, lastItem, null);
                            if (defaultDictionaryKeyValueAttribute != null)
                            {
                                keyValue = defaultDictionaryKeyValueAttribute.Value;
                            }
                            else
                            {
                                keyValue = ConfigManager.AlternateCreateInstance(keyType);
                                var jsonDefaultDictionaryKeyValueAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<JsonDefaultDictionaryKeyValueAttribute>(prevWrapper, lastItem, null);
                                if (!keyType.IsValueType && keyType != typeof(string))
                                {
                                    string json = jsonDefaultDictionaryKeyValueAttribute?.Json ?? "{}";

                                    JsonConvert.PopulateObject(json, keyValue, ConfigManager.serializerSettings);
                                }
                            }
                            Type genericType = typeof(DictionaryElementWrapper<,>).MakeGenericType(keyType, toAdd.GetType());

                            item = Activator.CreateInstance(genericType, [keyValue, toAdd, item]);

                        }
                    }
                }
                else
                    throw new Exception("Property or field doesn't exist in " + curType.Name);
                lastItem = item;
            }
        if (item == null) return null;
        var lastType = item.GetType();
        if (lastType.IsGenericType && lastType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            var wrapperType = typeof(DictionaryElementWrapper<,>).MakeGenericType(lastType.GenericTypeArguments[0], lastType.GenericTypeArguments[1]);
            var Key = lastType.GetProperty("Key").GetValue(item);
            var Value = lastType.GetProperty("Value").GetValue(item);
            item = Activator.CreateInstance(wrapperType, [Key, Value, null]);
        }
        return item;
    }
    public static object GetItemViaPath(object target, IEnumerable<string> path, bool privateAllowed = false)
    {
        object item = target;
        var bindFlag = BindingFlags.Public | BindingFlags.Instance;
        if (privateAllowed)
            bindFlag |= BindingFlags.NonPublic;
        if (path != null)
            foreach (var p in path)
            {
                var curType = item!.GetType();
                var fld = curType.GetField(p, bindFlag);
                var prop = curType.GetProperty(p, bindFlag);
                if (fld != null)
                    item = fld!.GetValue(item)!;
                else if (prop != null)
                    item = prop!.GetValue(item)!;
                else if (item is IEnumerable collection && int.TryParse(p, out int index))
                {
                    int counter = 0;
                    bool flag = true;
                    foreach (var i in collection)
                    {
                        if (counter == index)
                        {
                            item = i;
                            flag = false;
                            break;
                        }
                        counter++;
                    }
                    if (flag && p != path.Last())
                        throw new IndexOutOfRangeException();
                }
                else
                    throw new Exception("Property or field doesn't exist in " + curType.Name);

            }
        return item;
    }
    public static Type GetTypeViaPath(object target, IEnumerable<string> path, bool privateAllowed = false)
    {
        object item = target;
        var bindFlag = BindingFlags.Public | BindingFlags.Instance;
        if (privateAllowed)
            bindFlag |= BindingFlags.NonPublic;
        if (path != null)
        {
            List<string> pathListed = [];
            foreach (var p in path)
                pathListed.Add(p);
            foreach (var p in pathListed[..^1])
            {
                if (item == null) return null;
                var curType = item!.GetType();
                var fld = curType.GetField(p, bindFlag);
                var prop = curType.GetProperty(p, bindFlag);
                if (fld != null)
                    item = fld!.GetValue(item)!;
                else if (prop != null)
                    item = prop!.GetValue(item)!;
                else if (item is IEnumerable collection && int.TryParse(p, out int index))
                {
                    int counter = 0;
                    bool flag = true;
                    foreach (var i in collection)
                    {
                        if (counter == index)
                        {
                            item = i;
                            flag = false;
                            break;
                        }
                        counter++;
                    }
                    if (flag && p != path.Last())
                        throw new IndexOutOfRangeException();
                }
                else
                    throw new Exception("Property or field doesn't exist in " + curType.Name);

            }
            if (item != null)
            {
                Type itemType = item!.GetType();
                string lastPth = path.Last();
                if (int.TryParse(lastPth, out int _) && item is ICollection)
                {
                    if (itemType.IsArray)
                        return itemType.GetElementType();
                    else if (itemType.IsGenericType)
                    {
                        Type genericDefinition = itemType.GetGenericTypeDefinition();
                        if (genericDefinition == typeof(List<>))
                            return itemType.GetGenericArguments()[0];
                        else if (genericDefinition == typeof(HashSet<>))
                            return itemType.GetGenericArguments()[0];
                        else if (genericDefinition == typeof(Dictionary<,>))
                            return typeof(KeyValuePair<,>).MakeGenericType(itemType.GetGenericArguments());
                        else
                            return typeof(object);
                    }
                    else return typeof(object);
                }
                else
                {
                    var fld = itemType.GetField(lastPth, bindFlag);
                    var prop = itemType.GetProperty(lastPth, bindFlag);
                    if (fld != null)
                        return fld.FieldType;
                    if (prop != null)
                        return prop.PropertyType;
                }
            }
        }
        return null;
    }
    public static void SetConfigValue(ModConfig config, PropertyFieldWrapper variableInfo, object value, object item, bool broadcast = true, List<string> path = null, IList List = null, int Index = -1)
    {
        Type type = List != null ? List[Index].GetType() : variableInfo.Type;
        if (value != null && type != value.GetType())
            throw new Exception($"Field type mismatch: {variableInfo.Type} != {value.GetType()}");

        var modConfig = ConfigManager.Configs[config.Mod].Find(i => i.Name == config.Name);
        bool valueType = item.GetType().IsValueType;
        // TML的注释：
        // Main Menu: Save, leave reload for later
        // MP with ServerSide: Send request to server
        // SP or MP with ClientSide: Apply immediately if !NeedsReload
        if (Main.gameMenu)
        {
            InternalSetValue(config, variableInfo, value, item, path, List, Index);
            //fieldInfo.SetValue(config, value);
            ConfigManager.Save(config); // 保存配置到文件
            //ConfigManager.Load(modConfig); // 重新加载配置

            // modConfig.OnChanged(); delayed until ReloadRequired checked
            // Reload will be forced by Back Button in UIMods if needed
        }
        // 处于游戏内
        else
        {
            // 需要重新加载，不允许保存
            bool reloadRequired = variableInfo.MemberInfo.GetCustomAttribute<ReloadRequiredAttribute>() is not null;
            if (reloadRequired)
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
                Main.NewText(Language.GetTextValue("tModLoader.ModConfigCantSaveBecauseChangesWouldRequireAReload"),
                    Color.Red); //"Can't save because changes would require a reload."
                return;
            }
            if (modConfig.Mode is ConfigScope.ServerSide && Main.netMode is NetmodeID.MultiplayerClient && broadcast)
            {
                // 没有通过主机IP验证
                if (config.Mod.Name == "ImproveGame" && ImproveConfigs.Instance.OnlyHost && !Main.countsAsHostForGameplay[Main.myPlayer])
                {
                    // “无法更改: 你不是服务器主机玩家！”
                    Main.NewText(MyUtils.GetText("Configs.ImproveConfigs.OnlyHost.Unaccepted"), Color.Red);
                    return;
                }

                if (config.Mod.Name == "ImproveGame" && ImproveConfigs.Instance.OnlyHostByPassword && !NetPasswordSystem.LocalPlayerRegistered)
                {
                    // “无法更改：你没有通过密码验证！”
                    Main.NewText(MyUtils.GetText("Configs.ImproveConfigs.OnlyHostByPassword.Unaccepted"), Color.Red);
                    return;
                }

                if (item.GetType().Name == "ColorHandler")// 因为Color实现上的特殊性，这里不得不先写入一次了
                    InternalSetValue(config, variableInfo, value, item, path, List, Index);
                // 发送更好的体验自己的包
                List<string> pathDirectly = [];
                if (path != null)
                    pathDirectly.AddRange(path);
                if (List == null)
                    pathDirectly.Add(variableInfo.Name);
                // 这里是写入从Config实例到选项内容的完整路径
                // 所谓路径也就是一串 成员名或者下标 组成的东西
                ConfigOptionPacket.Send(config, value, pathDirectly);
                return;
            }

            // 本地配置，或者处于服务器端，或者处于客户端，但不需要广播
            //variableInfo.SetValue(item, value);
            InternalSetValue(config, variableInfo, value, item, path, List, Index);
            try
            {
                ConfigManager.Save(modConfig);
            }
            catch { }
            modConfig.OnChanged();

        }
    }
    //public static void SetConfigValue(ModConfig config, PropertyFieldWrapper variableInfo, object value, bool broadcast = true) => SetConfigValue(config, variableInfo, value, config, broadcast);

    public static void SetItemViaPath(object target, IEnumerable<string> path, object value)
    {
        object item = target;
        object lastItem = item; ;
        var bindFlag = BindingFlags.Public | BindingFlags.Instance;
        bindFlag |= BindingFlags.NonPublic;
        int max = path.Count();
        int count = 0;
        if (path == null || max == 0)
        {
            target = value;
            return;
        }
        else foreach (var p in path)
        {
            var curType = item!.GetType();
            var fld = curType.GetField(p, bindFlag);
            var prop = curType.GetProperty(p, bindFlag);
            if (count != max - 1)
            {
                lastItem = item;
                if (fld != null)
                    item = fld!.GetValue(item)!;
                else if (prop != null)
                    item = prop!.GetValue(item)!;
                else if (item is IEnumerable collection && int.TryParse(p, out int index))
                {
                    int counter = 0;
                    bool flag = true;
                    foreach (var i in collection)
                    {
                        if (counter == index)
                        {
                            item = i;
                            flag = false;
                            break;
                        }
                        counter++;
                    }
                    if (flag)
                        throw new IndexOutOfRangeException();
                }
                else
                    throw new Exception("Property or field doesn't exist in " + curType.Name);
            }
            else
            {
                if (lastItem is IDictionary dict)
                {
                    object Key = ((dynamic)item).Key;
                    object Value = ((dynamic)item).Value;

                    List<object> cachedKeys = [];
                    List<object> cachedValues = [];

                    foreach (var key in dict.Keys)
                        cachedKeys.Add(key);

                    foreach (var valueDummy in dict.Values)
                        cachedValues.Add(valueDummy);

                    int idx = cachedKeys.FindIndex(obj => obj.Equals(Key));
                    dict.Clear();
                    for (int n = 0; n < idx; n++)
                        dict.Add(cachedKeys[n], cachedValues[n]);

                    if (p == "Key")
                        dict.Add(value, Value);
                    else
                        dict.Add(Key, value);

                    for (int n = idx + 1; n < cachedKeys.Count; n++)
                        dict.Add(cachedKeys[n], cachedValues[n]);

                    //if(p == "Key"){}
                    //else
                    //dict[Key] = value;//按说直接这样应该就行了，但是有时候会有问题，

                    return;
                }
                fld?.SetValue(item, value);
                prop?.SetValue(item, value);
                if (item is IEnumerable collection && int.TryParse(p, out int index))
                {
                    if (item is Array array)
                    {
                        if (index < array.Length)
                            array.SetValue(value, index);
                        else
                            throw new IndexOutOfRangeException();
                    }
                    else if (item is IList list)
                    {
                        if (index < list.Count)
                            list[index] = value;
                        else
                            list.Add(value);
                    }
                    else if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(HashSet<>))
                    {
                        var addMethod = item.GetType().GetMethod("Add", bindFlag);
                        var removeMethod = item.GetType().GetMethod("Remove", bindFlag);
                        List<object> cache = [.. collection];
                        cache.Reverse();
                        int targetCount = cache.Count - index;
                        foreach (var i in cache[0..targetCount])
                        {
                            removeMethod?.Invoke(item, [i]);
                        }
                        addMethod?.Invoke(item, [value]);
                        for (int i = targetCount - 2; i >= 0; i--)
                            addMethod?.Invoke(item, [cache[i]]);
                    }
                    else if (item is IDictionary dictionary)
                    {
                        List<object> cachedKeys = [];
                        List<object> cachedValues = [];

                        foreach (var key in dictionary.Keys)
                            cachedKeys.Add(key);

                        foreach (var valueDummy in dictionary.Values)
                            cachedValues.Add(valueDummy);

                        object Key = ((dynamic)value).Key;
                        object Value = ((dynamic)value).Value;


                        dictionary.Clear();
                        for (int n = 0; n < index; n++)
                            dictionary.Add(cachedKeys[n], cachedValues[n]);
                        dictionary.Add(Key, Value);
                        for (int n = index + 1; n < cachedKeys.Count; n++)
                            dictionary.Add(cachedKeys[n], cachedValues[n]);

                        /*
                        cachedKeys.Reverse();
                        cachedValues.Reverse();

                        int targetCount = cachedKeys.Count - index;
                        foreach (var i in cachedKeys[0..targetCount])
                            dictionary.Remove(i);

                        object Key = ((dynamic)value).Key;
                        object Value = ((dynamic)value).Value;

                        dictionary.Add(Key, Value);
                        for (int i = targetCount - 2; i >= 0; i--)
                            dictionary.Add(cachedKeys[i], cachedValues[i]);
                        */
                    }
                }
            }
            count++;
        }
    }

    public static string GetModText(string modName, string str, out bool hasValue, params object[] arg)
    {
        string key = $"Mods.{modName}.{str}";
        string text = Language.GetTextValue(key, arg);
        hasValue = Language.Exists(key);
        return MyUtils.ConvertLeftRight(text);
    }
    public static string GetLocalizationKey(ModConfig config, string optionName)
        => $"Configs.{config.Name}.{optionName}";

    public static string GetTooltip(ModConfig config, string optionName)
    {
        string result = GetModText(config.Mod.Name, $"{GetLocalizationKey(config, optionName)}.Tooltip", out bool hasValue);
        return hasValue ? result : "";
    }

    public static string GetLabel(ModConfig config, string optionName)
    {
        string result = GetModText(config.Mod.Name, $"{GetLocalizationKey(config, optionName)}.Label", out bool hasValue);
        return hasValue ? result : optionName;
    }
}