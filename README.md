<h1 align="center">更好的体验</h1>

<div align="center">

[English](README-en.md) | 简体中文

[更新日志](ChangeLog.md)

一个致力于给模组游玩体验提一个档次的辅助Mod

</div>

## ✨ 功能
详细列表请到模组配置或更新日志中查看
1. 物品最大堆叠、武器自动使用等辅助Mod普遍功能(对标老牌辅助Luiafk)
2. 非时装饰品也可放置在时装栏（启用模组直接生效）
3. 城镇NPC入住机制修改: 夜晚入住、图鉴解锁后无视条件入住
4. 空间魔杖、建筑魔杖、法爆魔杖和钓鱼机等大大优化游戏体验的模组物品
5. 渔夫任务无冷却、控制墓碑是否掉落等辅助模组功能的整合
6. 随身增益、随身制作站等节省时间的功能
7. 与同队好友共享无尽增益、制作站等专为联机设置的功能
8. 含100格的超大背包，再也不用担心旗帜和药水放在哪了
9. 药水袋与旗帜盒，更方便地将你的药水旗帜整合在一个物品，节省空间
10. 几乎所有功能都是可以调节的，自行选择适合你的功能

## ⬇️ 下载
本Mod已在Steam创意工坊发布，可直接订阅下载: https://steamcommunity.com/sharedfiles/filedetails/?id=2797518634

## 💻 编译
更好的体验由于使用了NuGet，因此无法使用tModLoader进行编译，需要使用IDE进行编译
1. 使用IDE，如Visual Studio 2022、Rider等打开项目
2. 如果先前在tModLoader中启用了此Mod，先禁用，然后重新加载
3. 使用IDE编译项目
4. 在tModLoader中打开此Mod，重新加载
5. 成了

## 📗 版权声明
本模组的“显示物品、NPC所属模组”功能部分代码来源于模组“WMITF”，本模组与模组“WMITF”皆在MIT许可协议下开源，意味着“被授权人有权利使用、复制、修改、合并、出版发行、散布、再授权及贩售软件及软件的副本，并允许软件提供者这样做，但须满足以下条件：

上述版权声明和本许可声明应包含在本软件的所有副本或主要部分中。

因此，该模组不存在版权侵犯问题。
ChevyRay 的协程类也受 MIT 许可，与上面相同。
TextureGIF.cs的部分代码来自ProjectStarlight.Interchange，该项目也受 MIT 许可，与上面相同。
本模组的自动存钱功能大部分代码来自模组“Auto Piggy Bank”，该模组也受 MIT 许可，与上面相同。

附:
本模组开源链接: https://gitee.com/MyGoold/improve-game (此为原仓库，Github仓库是由原仓库镜像而来)
WMITF开源链接: https://github.com/gardenappl/WMITF
ChevyRay的协程类: https://github.com/ChevyRay/Coroutines
ProjectStarlight.Interchange: https://github.com/ProjectStarlight/ProjectStarlight.Interchange
Auto Piggy Bank开源链接: https://github.com/diniamo/auto-piggy-bank

## 🤝 跨Mod支持 (Mod.Call)

如果你是一名玩家，并想要让其他模组修复与更好的体验不兼容的问题，你可以向其他模组作者提出请求，让他们阅读此文档并添加跨Mod支持。

### IgnoreInfItem
```"IgnoreInfItem", int/List<int> ItemID(s)```

为某个/些指定的物品添加无尽增益忽略，以防止在拥有30个时无限提供增益
### AddPotion
```"AddPotion", int ItemID, int BuffID```

为某个指定的物品添加无尽增益支持(药水类)，对于没有设置Item.buffType或拥有多个增益的物品十分有用，药水类指需要达到30堆叠才能提供增益
### AddStation
```"AddStation", int ItemID, int BuffID```

为某个指定的物品添加无尽增益支持(放置站类)，放置站类指有一个即可提供增益。放置站一般需要手动提供支持
### AddPortableCraftingStation
```"AddPortableCraftingStation", int ItemID, int/List<int> TileID(s)```

为某个指定的物品添加便携制作站支持，对于没有设置Item.createTile但是应该充当某种制作站的物品十分有用，你可以指定多个制作站。

如果你想要使其充当水源，应将TileID设置为水槽 (TileID.Sink)

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
    }
}
```