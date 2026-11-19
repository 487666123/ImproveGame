<div style="padding: 20px;display: flex;flex-direction: column;align-items: center;">

<h1 align="center" style="border-bottom: 0">🤝 跨 Mod 支持</h1>

如果你是一名玩家，并想要让其他模组修复与更好的体验不兼容的问题，\
你可以向其他模组作者提出请求，让他们阅读此文档并添加跨 **Mod** 支持。

除了以 `GetXX` 开头的，其他 **Mod.Call** 的返回值是一个 `bool`，指示这个操作是否成功执行。

</div>

### GetAmmoChainSequence

获取指定物品的弹药链序列
建议将本模组源码的 [AmmoChain.cs](Content/Functions/ChainedAmmo/AmmoChain.cs) 和 [ItemTypeData.cs](Core/ItemTypeData.cs) 复制到你的模组源码中，以便操作弹药链

#### 参数

- `Item` 要获取弹药链序列的物品实例

#### 返回值

- `TagCompound` 以TagCompound形式存储的弹药链数据，数据读取方式参考 [AmmoChain.cs](Content/Functions/ChainedAmmo/AmmoChain.cs) 和 [ItemTypeData.cs](Core/ItemTypeData.cs)。如果物品没有弹药链，返回 `null`

### GetUniversalAmmoId

获取“任意弹药”物品的ID，这是一个用于弹药链中，表示该位置弹药任意的物品，搭配弹药链使用

#### 参数

无

#### 返回值

- `int` “任意弹药”物品的ID

### GetBigBagItems

获取大背包中的物品

#### 参数

- `Player` 大背包所属玩家的实例

#### 返回值

- `List<Item>` 大背包中共100格的物品的实例，包括空气

### IgnoreInfItem

为某个/些指定的物品添加无尽增益忽略，以防止在拥有30个时无限提供增益

#### 参数

- `int/List<int>` 不提供无限增益的某个物品/一些物品的ID

### AddPotion

为某个指定的物品添加无尽增益支持(药水类)，对于没有设置Item.buffType或拥有多个增益的物品十分有用，药水类指需要达到30堆叠才能提供增益

#### 参数

- `int` 添加药水类无尽增益支持的物品的ID
- `int/List<int>` 该物品提供的一个/一些增益的ID

### ConsumePotion

让某个/些指定的物品在触发无尽增益的情况下也会被正常消耗

#### 参数

- `int/List<int>` 会被正常消耗的某个物品/一些物品的ID

### BuffConflict

设置当玩家拥有某个增益时，一个/些增益会被清除

#### 参数

- `int` 某个增益的ID
- `int/List<int>` 会被清除的增益ID

### AddStation

为某个指定的物品添加无尽增益支持(放置站类)，放置站类指有一个即可提供增益。放置站一般需要手动提供支持

#### 参数

- `int` 添加放置站类无尽增益支持的物品的ID
- `int/List<int>` 该物品提供的一个/一些增益的ID

### AddPortableCraftingStation

为某个指定的物品添加便携制作站支持，对于没有设置Item.createTile但是应该充当某种制作站的物品十分有用，你可以指定多个制作站。
如果你想要使其充当水源，应将“该物品充当的制作站的物块ID”(即第二个参数)设置为水槽 (TileID.Sink)

#### 参数

- `int` 添加便携制作站支持的物品的ID
- `int/List<int>` 该物品可充当的一个/一些制作站的物块ID

### AddFishingAccessory

为某个指定的物品添加自动钓鱼机的钓鱼饰品支持，可设置钓鱼速度加成、渔力加成、是否应被视为钓具箱和是否可在岩浆钓鱼。一般需要手动提供饰品支持

#### 参数

- `int` 添加自动钓鱼机的钓鱼饰品支持的物品的ID
- `float` 该物品提供的钓鱼速度加成
- `float` 该物品提供的渔力加成
- `bool` 该物品是否应被视为钓具箱
- `bool` 该物品是否给予在熔岩中钓鱼的能力

### AddStatCategory

向属性面板添加一个属性类别，后续可以使用 `AddStat` 添加属性

#### 参数

- `string` 添加的属性类别的字符串标识符
- `Texture2D` 指示属性类别的图标
- `string` 该属性类别的名称的本地化键
- `Texture2D` 指示该属性类别所属模组的图标，建议使用模组的icon_small贴图

### AddStat

向某个属性类别添加一个属性

#### 参数

- `string` 该属性要被添加到的属性类别的字符串标识符
- `string` 该属性的名称的本地化键
- `Func<string>` 该属性的值的获取函数

### AddHomeTpItem

添加回家物品

#### 参数

- `int/List<int>` 你要添加的某个物品/一些物品的ID
- `bool` 该物品是否应被视为药水，若为药水则需要堆叠超过无尽药水需求才能快捷使用
- `bool` 该物品是否应被视为折返药水，在回程时会创造一个折返传送门

### GetFisherItems
获取指定自动钓鱼机内的物品
#### 参数
- `Point16/TEAutoFisher` 钓鱼机坐标/实例，支持钓鱼机覆盖的任一图格坐标

##### 另一种样式

- `int` 钓鱼机位置的横坐标，世界图格坐标
- `int` 钓鱼机位置的纵坐标，世界图格坐标

#### 返回值

- `Item[]` 长度为43的数组，包括空气，槽位说明：
  - 0-39: 渔获
  - 40: 钓竿
  - 41: 鱼饵
  - 42: 钓鱼饰品
  - 若钓鱼机获取失败返回空数组（`Array.Empty<Item>()`而非`null`，可以通过数组长度判断）
- Note: 返回的数组为快照，玩家交互可能导致钓鱼机绑定物品实例改变，需要注意数据过期问题

### SyncFisherItems

同步指定自动钓鱼机内的物品，用于在单人模式请求钓鱼机UI刷新和多人模式服务器端物品同步
#### 参数
- `Point16/TEAutoFisher` 钓鱼机坐标/实例，支持钓鱼机覆盖的任一图格坐标
- `int` 需要同步物品的栏位id，对应关系参考`GetFisherItems`返回值说明
- `int` 物品数量的变化量
##### 另一种样式
- `int` 钓鱼机位置的横坐标，世界图格坐标
- `int` 钓鱼机位置的纵坐标，世界图格坐标
- `int` 需要同步物品的栏位id，同前
- `int` 物品数量的变化量
#### 行为说明
- **单人模式**：
  - 强制刷新钓鱼机UI，`栏位id` 需在0-42范围内，`变化量` 参数值被忽略
- **多人模式**：
  - `变化量` 为0时：同步指定槽位物品的完整状态（类型、数量、属性）
  - `变化量` 非0时：仅更新物品数量（支持增量修改）
- Note：多人模式中，客户端调用无效，会被拦截并直接返回false。

### 使用例

以下是一个为自己的放置站添加支持的例子

```CSharp
public override void PostSetupContent() {
    if (ModLoader.TryGetMod("ImproveGame", out Mod improveGame)) {
        improveGame.Call(
            "AddStation",//加入你自己的增益站1
            ModContent.ItemType<MyStation1>(), // 物品ID 1
            ModContent.BuffType<MyStationBuff1>() // BuffID 1
        );
        improveGame.Call(
            "AddStation",//加入你自己的增益站2
            ModContent.ItemType<MyStation2>(), // 物品ID 2
            ModContent.BuffType<MyStationBuff2>() // BuffID 2
        );
    }
}
```

### RegisterCategory

注册分类卡

#### 参数

- `Mod` 注册的分类卡所在的模组的实例
- `List<KeyValuePair<string, ModConfig>>` 分类卡中的选项信息，键为字段/属性名，值为该模组设置的实例
- `int` 分类卡的物品图标对应物品id，可选参数，默认值为0
- `Func<Texture2D>` 获取分类卡图标的函数，会覆盖上面的物品图标id的效果，默认值为null
- `Func<string>` 获取该分类卡的标签的函数，默认值为null
- `Func<string>` 获取该分类卡的描述的函数，默认值为null

### SetAboutPage

注册 “关于” 页面

#### 参数

- `Mod` 注册的 “关于” 页面所在的模组的实例
- `Func<string>` 获取 “关于” 页面中所写的文本的函数
- `int`  “关于” 页面的物品图标对应物品id，可选参数，默认值为0
- `Func<Texture2D>` 获取 “关于” 页面图标的函数，会覆盖上面的物品图标id的效果，默认值为null
- `Func<string>` 获取该 “关于” 页面的标签的函数，默认值为null
- `Func<string>` 获取该 “关于” 页面的描述的函数，默认值为null

### RemoveCategory

移除模组注册的所有分类卡

#### 参数

- `Mod` 目标模组的实例

### RemoveAboutPage

移除模组注册的 “关于” 页面

#### 参数

- `Mod` 目标模组的实例

### AddModernConfigTitle

设置模组的配置中心在模组设置入口处的文本标题

#### 参数

- `Mod` 目标模组的实例
- `LocalizedText` 标题的本地化文本实例

### RegisterPreview

注册预览绘制

#### 参数

- `PropertyFieldWrapper` 注册的预览绘制对应的选项的字段/属性信息，注意需要直接属于某个Config才有效
- `Action<UIElement, ModConfig, PropertyFieldWrapper, object, IList, int>`
注册的预览绘制内容

##### Action参数的参数

- `UIElement` 绘制框元素
- `ModConfig` 预览绘制的选项对应的设置实例
- `PropertyFieldWrapper` 对应的选项的字段/属性信息
- `object` 直接隶属对象
- `IList` 直接隶属列表
- `int` 列表中的下标

### OnGlobalConfigPreview

添加全局预览绘制，与上一个唯一的区别是这个不针对任何一个特定选项，用来批量添加绘制

#### 参数

- `Action<UIElement, ModConfig, PropertyFieldWrapper, object, IList, int>`
参考上个条目的内容

### 使用例

```CSharp
// 此处示例使用了ImproveGame_ModernConfigCrossModHelper.cs文件的内容
// 这个是一个Mod类的Load函数
public override void Load()
{
    if (Main.netMode == NetmodeID.Server ||
    !ModLoader.TryGetMod("ImproveGame", out var qot)) return;

    //添加标题
    AddModernConfigTitle(qot,  this,
    Language.GetOrRegister("Mods.MyMod.MyModernConfigTitle"));

    SetAboutPage(qot, this, () => "自己适配配置中心好累哦\n不如反射生成(逃",
    (int)ItemID.IronShortsword, null, () => "关于示例", () => "请不要在意吐槽");
    //上面三个文本自己写本地化获取文本吧(

    //此处示例为给单个设置实例批量添加设置选项
    //MyConfig.Instance为加载时获取的MyConfig实例
    RegisterCategory(qot, this, MyConfig.Instance,
    [
        nameof(MyConfig.SomeField),
        nameof(MyConfig.SomeProperty),
        nameof(MyConfig.SomeArray),
        nameof(MyConfig.SomeDefinition),
    ],
    ItemID.Cog, null, () => "这是一些数据", () => "我顺带告诉你这个支持哪些东西了");
    //上面两个文本自己写本地化获取文本吧(

    //此处示例为给多个设置示例批量添加设置选项
    //一个选项名一个设置实例那种太麻烦了我就不写示例了
    RegisterCategory(qot, this,
    [
        (MyConfig.Instance, //上面水过的再水一遍(
        [
            nameof(MyConfig.SomeField),
            nameof(MyConfig.SomeProperty),
            nameof(MyConfig.SomeArray),
            nameof(MyConfig.SomeDefinition),
        ]),
        (SeverConfig.Instance,
        [
            nameof(MyConfig.SomeVector2),
            nameof(MyConfig.SomeColor),
            nameof(MyConfig.SomePoint),
            nameof(MyConfig.SomeClass),
        ])
    ],
        ItemID.WireKite, null, () => "两家的拼在一起！",
        () => "也许有些内容同时需要客户端和服务端来管理，"
            + "这时这个就大抵能派上很大用场了，嗯。");
}
```

### AddPrison
添加新的建造魔杖建筑样式
#### 参数
- `Texture2D` 记录了建筑信息的数据图
- `Texture2D` 该建筑的一张预览图

#### 数据图格式

| 颜色 | Hex    | 代表的对象   |
| ---- | ------ | ------------ |
| 红色 | FF0000 | 实心块       |
| 黑色 | 000000 | 平台         |
| 白色 | FFFFFF | 火把         |
| 黄色 | FFFF00 | 椅子         |
| 粉色 | FF00FF | 桌子         |
| 蓝色 | 0000FF | 工作台       |
| 紫色 | 7F00FF | 床           |
| 青色 | 00FFFF | 禁止放置墙体 |
| 绿色 | 00FF00 | 门           |
| 透明 |        | 墙体         |

### RegisterFishingEvent

注册自动钓鱼机钓鱼事件，在钓鱼机钓起物品前触发，可修改钓鱼结果或取消钓鱼

#### 参数

委托实例，签名为 `void Method(TileEntity fisher, FishingAttempt fishingAttempt, Player player, ref int itemType, ref int itemStack, ref bool cancel)`

#### 使用示例

```csharp
public class MyMod : Mod
{
    public override void PostSetupContent()
    {
        if (!ModLoader.TryGetMod("ImproveGame", out Mod improveGame))
                return;

        // 定义委托签名并转换
        improveGame.Call("RegisterFishingEvent", (Delegate)OnFishingCallback);
    }
    
    private void OnFishingCallback(TileEntity fisher, FishingAttempt fishingAttempt, Player player, ref int itemType, ref int itemStack, ref bool cancel)//注意这里的签名需要和委托定义一致
    {
        // 直接通过 ref 参数修改
        itemType = ItemID.IronCrate;//全部变成铁匣子
    }

    //这里可以偷懒不卸载委托，因为模组在重新加载时本身会自动卸载清理所有的委托
}
```

### RegisterWeatherControl

往气候控制面板注册一个控制项，未指定栏目的项会落到面板里的“模组项”栏
建议把本项目根目录的 [ImproveGame_WeatherControlCrossModHelper.cs](ImproveGame_WeatherControlCrossModHelper.cs) 复制到你自己的模组里，直接用强类型 API 调用，省去手动凑 `Mod.Call` 参数

#### 参数

- `Mod` 注册源模组的实例，会被用作控制项的 ModName
- `string` 控制项的内部名，与 ModName 共同构成唯一标识 `ModName:Name`
- `Texture2D` 槽位图标，建议 32x32 以内，超过会自动按比例缩到 28x28
- `Func<string>/LocalizedText/string` 显示名提供器，悬停 tooltip 的顶行
- `Func<string>/LocalizedText/string` 悬停说明，可传 `null`
- `string[]/IReadOnlyList<string>` 档位名数组，长度至少 2，索引顺序就是档位顺序，名字仅用作内部键不会直接显示
- `Func<int>` 取当前档位下标的回调，未知时返回 `-1`
- `Action<int>` 应用指定档位到本地的回调，只负责落地状态不负责发包
- `bool` 可选，是否支持锁定，默认 `false`
- `Func<bool>` 可选，取当前锁定状态的回调
- `Action<bool>` 可选，应用锁定状态到本地的回调
- `Func<bool>` 可选，判断该项当前是否可用，不可用时槽位不会出现在面板上
- `int` 可选，排序优先级，越高越靠前，默认 `0`
- `string` 可选，所属内容栏的 Id，为空则放进默认的“模组项”栏

#### 返回值

- `bool` 注册是否成功，stages 数量不足或必填项缺失时返回 `false`

#### 说明

- `setStage` 与 `setLocked` 只在本地写入状态，跨客户端同步由 `SetWeatherControlStage` / `SetWeatherControlLocked` 统一派发，单机也走派发
- 同模组同名重复注册会覆盖旧条目

### UnregisterWeatherControl

移除一个气候控制项

#### 参数

- `Mod` 注册源模组的实例
- `string` 控制项的内部名

### RegisterWeatherControlGroup

注册一个自定义内容栏，模组可以把自己的 UI 视图挂到气候控制面板里，而不是只往“模组项”格子里塞图标

#### 参数

- `Mod` 注册源模组的实例
- `string` 内容栏的内部名
- `Func<string>/LocalizedText/string` 显示名
- `int` 排序优先级，越高越靠前
- `Func<UIElement>` 创建该栏视图的回调，气候控制面板每次打开会调一次
- `Func<bool>` 可选，判断该栏当前是否可用

### UnregisterWeatherControlGroup

移除一个自定义内容栏

#### 参数

- `Mod` 注册源模组的实例
- `string` 内容栏的内部名

### QueryWeatherControlStage

查询某个控制项的当前档位下标，可以用来读取内置项的状态

#### 参数

- `string` 控制项的完整 Id，格式 `ModName:Name`

#### 返回值

- `int` 当前档位下标，控制项不存在时返回 `-1`

### SetWeatherControlStage

派发档位变更，所有客户端会同步应用

#### 参数

- `string` 控制项的完整 Id
- `int` 目标档位下标

### IsWeatherControlLocked

查询某个控制项是否处于锁定状态

#### 参数

- `string` 控制项的完整 Id

#### 返回值

- `bool` 当前是否锁定，控制项不存在或不支持锁定时返回 `false`

### SetWeatherControlLocked

派发锁定状态变更，所有客户端会同步应用，控制项不支持锁定时返回 `false`

#### 参数

- `string` 控制项的完整 Id
- `bool` 目标锁定状态

### 内置气候控制项 Id 速查

| Id | 项目 | 档位 |
| --- | --- | --- |
| `ImproveGame:Time` | 时间 | `Dawn` / `Noon` / `Dusk` / `Midnight` |
| `ImproveGame:MoonPhase` | 月相 | `Phase0` ~ `Phase7` |
| `ImproveGame:Rain` | 降雨 | `Off` / `On` |
| `ImproveGame:Sandstorm` | 沙暴 | `Off` / `On` |
| `ImproveGame:Wind` | 风向 | `West` / `No` / `East` |

### 使用例

下面这段演示一个外部模组怎么用 `Mod.Call`（这里用了 [ImproveGame_WeatherControlCrossModHelper.cs](ImproveGame_WeatherControlCrossModHelper.cs) 提供的强类型封装）注册一个自己的气候控制项：

```CSharp
public override void PostSetupContent()
{
    if (!ModLoader.TryGetMod("ImproveGame", out var qot)) return;

    ImproveGame_WeatherControlCrossModHelper.RegisterWeatherControl(
        qot: qot,
        source: this,
        name: "ScorchingDay",
        icon: TextureAssets.Item[ItemID.LivingFireBlock].Value,
        displayName: () => Language.GetTextValue("Mods.MyMod.ScorchingDay.Name"),
        tooltip:     () => Language.GetTextValue("Mods.MyMod.ScorchingDay.Tooltip"),
        stages: ["Inactive", "Active"],
        getStage: () => MySystem.Active ? 1 : 0,
        setStage: MySystem.ApplyStage,
        supportsLock: true,
        getLocked: () => MySystem.Locked,
        setLocked: locked => MySystem.Locked = locked,
        priority: 100);
}

public override void Unload()
{
    // 卸载时撤掉注册，免得 Registry 留着指向已经卸载程序集的回调
    if (ModLoader.TryGetMod("ImproveGame", out var qot))
        ImproveGame_WeatherControlCrossModHelper.UnregisterWeatherControl(qot, this, "ScorchingDay");
}
```

档位切换通过 `SetWeatherControlStage` 统一派发，下面这段是改造原版降雨、再读回当前档位的最小演示：

```CSharp
if (ModLoader.TryGetMod("ImproveGame", out var qot))
{
    // 让世界开始下雨
    ImproveGame_WeatherControlCrossModHelper.SetWeatherControlStage(
        qot, ImproveGame_WeatherControlCrossModHelper.RainId, 1);

    int now = ImproveGame_WeatherControlCrossModHelper.QueryWeatherControlStage(
        qot, ImproveGame_WeatherControlCrossModHelper.RainId);
}
```