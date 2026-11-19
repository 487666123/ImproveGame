<div align="center" style="padding-top: 25px;display: flex;flex-direction: column;align-items: center">

<img src="./icon_workshop.png" width="150"/>
<h3>Quality of Terraria</h3>

English | [简体中文](README.md)

[Change log](ChangeLog-en.md)\
[跨 Mod 支持](./CrossModSupport.md)

</div>

## 📖 简介

**更好的体验** 模组的开发方向主要围绕以下三大模块展开：

### 1. 原版基础机制
允许玩家自由调整一些游戏基础机制，例如：
**物品最大堆叠数量**、**物品拾取范围**、**工具使用速度**、**额外 BUFF 栏**、**草药正常不受**、**重生加速** 等。

### 2.强力道具
新增一系列功能强大或玩法新颖的道具，例如：
**法爆魔杖**、**空间魔杖**、**科技魔杖**、**液体魔杖**、**虚空魔杖**、**钓鱼机**、**存储管理器** 等。

### 3. 玩家能力强化系统
提升角色便利性与自动化体验，例如：
**大背包扩容**、**扩展背包可拾取物品**、**无限药水 & 增益站**、**自动垃圾桶**、**自动开袋**、**快速微光净化**、**弹药链系统** 等。

📌 补充说明：

- 部分 **能力增强** 功能需通过特定道具激活，如：
**天气之书**、**定位球**、**瓶中微光船** 等。

- **更好的体验** 拥有独树一帜的 **UI 界面设计**，视觉风格与交互逻辑均与其他模组显著不同，是本模组的重要特色之一。

## ✨ Features

For a detailed list, check mod configurations or changelog above

1. General features like max stack
2. Non-vanity accessories can be worn in vanity slots
3. Town NPC spawn mechanic improvements: can respawn during nighttime, ignore respwaning limitations if bestiary is unlocked
4. Mod items such as Wand of Space, Wand of Architecture, Wand of Bursts and Fishing Machine which greatly improve gameplay experience
5. Integration of qol features like instantly refreash Angler's quests and disable tomb stone spawning
6. Time-saving features like Infinite Buffs and Portable Crafting Stations
7. Features specially made for multiplayer, for example sharing your infinite buffs, portable crafting stations and even more with your teammates
8. A Huge Inventory providing 100 item slots, no warries of storing your potions and banners
9. Potion Bag and Banner Box, integrate all your potions or banners as a single item (and you are free to take them out)

Most features are `configurable`, use on your own preference

## ⬇️ Download

You can subscribe and download it via steam workshop: <https://steamcommunity.com/sharedfiles/filedetails/?id=2797518634>

## 💻 How to compile

This mod contains Nuget package, which means you should not use the compilation of tModLoader, but you should use the compilation function of the code IDE (such as Visual Studio, Rider) to compile this mod

更好的体验依赖于 **SilkyUI**，所以需要克隆 SilkyUI 关联的两个项目，SilkyUI 具体使用步骤请在 [SilkyUIFramework](https://github.com/487666123/SilkyUIFramework) 仓库查看

1. Open project with your IDE
2. If you have enabled this mod in tModLoader, disable it and reload first
3. Compile the project with IDE
4. Enable this mod in tModLoader and reload
5. Done

## 📗 Copyright Statement

A part of the source code of the 'showing which mod is an item from' function is from mod 'WMITF', this mod and WMITF are open-source using the MIT Licence, which means 'Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.'

Hence there is no copyright voilation in this mod.
ChevyRay's coroutine class is under MIT License as well, same as above.
The code of TextureGIF.cs is partly from ProjectStarlight.Interchange which is under MIT License as well, same as above.
The code of the 'auto save money' function is mostly from mod 'Auto Piggy Bank' which is under MIT License as well, same as above.

p.s.
Source code of:
This mod: <https://github.com/487666123/ImproveGame>
WMITF: <https://github.com/gardenappl/WMITF>
ChevyRay's coroutine class: <https://github.com/ChevyRay/Coroutines>
ProjectStarlight.Interchange: <https://github.com/ProjectStarlight/ProjectStarlight.Interchange>
Auto Piggy Bank: <https://github.com/diniamo/auto-piggy-bank>

## 🤝 Cross-mod Support (Mod.Call)

If you are a player who want to have other mods fix incompatibilities with Quality of Terraria, you can make a request to other modders to have them read this document and add cross-mod support

With the exception of GetXX-prefixed calls, all other Mod.Call methods return a `bool` value indicating whether the operation was executed successfully

### GetAmmoChainSequence

Get the ammo chain sequence of the specified item
It is recommended to copy the [AmmoChain.cs](Content/Functions/ChainedAmmo/AmmoChain.cs) and [ItemTypeData.cs](Core/ItemTypeData.cs) to your mod source to easily operate ammo chains

#### Parameters

- `Item` The item instance you are getting ammo chain from

#### Return Value

- `TagCompound` Ammo chain data saved as TagCompound. See [AmmoChain.cs](Content/Functions/ChainedAmmo/AmmoChain.cs) and [ItemTypeData.cs](Core/ItemTypeData.cs) to learn how to read the data. Returns `null` if the item has no ammo chain

### GetUniversalAmmoId

Get the item ID of "Unspecified Ammo", an item used in ammo chains to indicate that this ammo can be anything. Usually used with ammo chain related logic.

#### Parameters

None

#### Return Value

- `int` The item ID of "Unspecified Ammo"

### GetBigBagItems

Get all items in big backpack

#### Parameters

- `Player` The player instance

#### Return Value

- `List<Item>` All 100 items in the big backpack, including air

### IgnoreInfItem

Ignore infinite buff(s) provided by certain item(s), hence it/they no longer provide(s) infinite buff(s)

#### Parameters

- `int/List<int>` The item(s) being ignored

### AddPotion

Add infinite buff(s) for a potion, especially effective to items which do not have Item.buffType or have multiple buffs. 30 potions are needed to activate the effect

#### Parameters

- `int` The potion's item type
- `int/List<int>` Type(s) of buff(s) provided by the potion

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

Add infinite buff(s) for a buff station, usually you have to call manually for mod buff station. Only 1 item is needed to activate its effect

#### Parameters

- `int` The buff station's item type
- `int/List<int>` Type(s) of buff(s) provided by the buff station

### AddPortableCraftingStation

Add portable crafting station support for a specified item. This is useful for items that do not have `Item.createTile` set but should act as some sort of crafting station. Multiple crafting stations can be specified.
If you want it to act as a water source, you should set the crafting station type (the 2nd parameter) to `TileID.Sink`

#### Parameters

- `int` The portable crafting station's item type
- `int/List<int>` Type(s) of crafting station(s) that the item can act as

### AddFishingAccessory

Add fishing accessory support for a specific item with fishing machine. This support should include options to set fishing speed multiplier, fishing power bonus, whether it should be considered as a tackle box, and whether it enables lava fishing. Generally, accessory support needs to be manually provided

#### Parameters

- `int` The accessory's item type
- `float` The fishing speed bonus provided by the accessory
- `float` The fishing power bonus provided by the accessory
- `bool` Whether it should be considered as a tackle box
- `bool` Whether it enables lava fishing

### AddStatCategory

Add a stat category to the stats panel. Hence you can add displayed stats via `AddStat` afterwards

#### Parameters

- `string` String identifier of the stat category added
- `Texture2D` The category's icon
- `string` The localization key of the category's display name
- `Texture2D` Icon indicating the mod this category belongs to. It is recommended to use the icon_small sprite of your mod

### AddStat

Add a stat to an existing stat category

#### Parameters

- `string` String identifier of the stat category to add to
- `string` The localization key of the stat's display name
- `Func<string>` Getter function of the stat's display value

### AddHomeTpItem

Add home-teleporting item

#### Parameters

- `int/List<int>` The item(s) to be added
- `bool` Whether it should be considered as a potion. Potions should meet the Infinite Potion requirement (stacked over 30 by default and can be changed via mod config) in order to be used via hotkeys
- `bool` Whether it creates a return portal when used

### GetFisherItems

Retrieves items from the specified Auto-Fisher machine.

#### Parameters

- `Point16/TEAutoFisher` Coordinates or instance of the Auto-Fisher machine. Accepts **any valid tile coordinate** within the machine’s bounds.

##### Alternative Syntax

- `int` X-coordinate (world tile coordinates)
- `int` Y-coordinate (world tile coordinates)
#### Return Value

- `Item[]`An array of length **43**, containing items or air (empty slots). Slot definitions:
- **0-39**: Fishing catch items
- **40**: Fishing rod slot
- **41**: Bait slot
- **42**: Fishing accessory slot
- Returns `Array.Empty<Item>()` (not `null`) if the Auto-Fisher is inaccessible.
- *Note*: The returned array is a **snapshot** and not bound to the actual Auto-Fisher instance. Subsequent player interactions may invalidate the data.

### SyncFisherItems

Synchronizes items in the specified Auto-Fisher machine. Used for:
- UI refresh in single-player mode
- Server-side item synchronization in multiplayer mode

#### Parameters

- `Point16/TEAutoFisher` Coordinates or instance of the Auto-Fisher machine. Accepts **any valid tile coordinate** within the machine’s bounds.
- `int` Slot ID to synchronize (refer to `GetFisherItems` slot definitions)
- `int` Quantity delta (positive, negative, or zero)
##### Alternative Syntax

- `int` X-coordinate (world tile coordinates)
- `int` Y-coordinate (world tile coordinates)
- `int` Slot ID (same as above)
- `int` Quantity delta (same as above)

#### Behavior

- **Single-player Mode**:Forces UI refresh. The `quantity delta` parameter is **ignored**. Slot ID must be in range **0-42**.
- **Multiplayer Mode**:
- If `delta = 0`: Full synchronization of the slot (item type, stack count, and properties).
- If `delta ≠ 0`: Incremental stack count update only (supports positive/negative values).
- **Note**: Client calls in multiplayer mode are **invalid** and will be intercepted by the server, returning `false`.

### Example

Here is an example of adding infinite buffs for 2 mod buff stations

```CSharp
public override void PostSetupContent() {
    if (ModLoader.TryGetMod("ImproveGame", out Mod improveGame)) {
        improveGame.Call(
            "AddStation",//Add your first buff station 
            ModContent.ItemType<MyStation1>(), // ItemID 1
            ModContent.BuffType<MyStationBuff1>() // BuffID 1
        );
        improveGame.Call(
            "AddStation",//Add your second buff station 
            ModContent.ItemType<MyStation2>(), // ItemID 2
            ModContent.BuffType<MyStationBuff2>() // BuffID 2
        );
    }
}
```

### RegisterCategory  

Register a category card  

#### Parameters  

- `Mod` The instance of the mod where the category card is registered  
- `List<KeyValuePair<string, ModConfig>>` Configuration options in the category card. Key is the field/property name, value is the mod's config instance  
- `int` Item ID for the category card's icon (optional, default: 0)  
- `Func<Texture2D>` Function to get category card icon (overrides item ID icon, default: null)  
- `Func<string>` Function to get category card label (default: null)  
- `Func<string>` Function to get category card description (default: null)  

### SetAboutPage  

Register an "About" page  

#### Parameters  

- `Mod` The instance of the mod where the "About" page is registered  
- `Func<string>` Function to get text content for the "About" page  
- `int` Item ID for the "About" page icon (optional, default: 0)  
- `Func<Texture2D>` Function to get "About" page icon (overrides item ID icon, default: null)  
- `Func<string>` Function to get "About" page label (default: null)  
- `Func<string>` Function to get "About" page description (default: null)  

### RemoveCategory  

Remove all category cards registered by a mod  

#### Parameters  

- `Mod` Target mod instance  

### RemoveAboutPage  

Remove the "About" page registered by a mod  

#### Parameters  

- `Mod` Target mod instance  

### AddModernConfigTitle  

Set the config center title text for the mod's settings entry  

#### Parameters  

- `Mod` Target mod instance  
- `LocalizedText` Localized text instance for the title  

### RegisterPreview  

Register preview rendering  

#### Parameters  

- `PropertyFieldWrapper` Field/property info of the target option (must belong to a Config)  
- `Action<UIElement, ModConfig, PropertyFieldWrapper, object, IList, int>`  
Preview rendering content  

##### Action Parameters  

- `UIElement` Canvas element  
- `ModConfig` Config instance for the previewed option  
- `PropertyFieldWrapper` Field/property info  
- `object` Parent object  
- `IList` Parent list  
- `int` Index in the list  

### OnGlobalConfigPreview  

Add global preview rendering (batch rendering not tied to specific options)  

#### Parameters  

- `Action<UIElement, ModConfig, PropertyFieldWrapper, object, IList, int>`  
Same as above  

---

### Example  

```CSharp  
// Used some function from ImproveGame_ModernConfigCrossModHelper.cs  
// This is a Mod class's Load method
public override void Load()  
{  
    if (Main.netMode == NetmodeID.Server ||  
    !ModLoader.TryGetMod("ImproveGame", out var qot)) return;  
 
    // Add title  
    AddModernConfigTitle(qot,  this,  
    Language.GetOrRegister("Mods.MyMod.MyModernConfigTitle"));  
 
    SetAboutPage(qot, this, () => "Customizing the config center is exhausting\nBetter auto-generate via reflection(",  
    (int)ItemID.IronShortsword, null, () => "About Example", () => "Please ignore the complaints(");  
    // Replace these texts with your localized strings  
 
    // Batch-add options for a single config instance  
    // Replace these with localized texts  

    // Batch-add options for multiple config instances  
    RegisterCategory(qot, this,  
    [  
        (MyConfig.Instance,  
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
        ItemID.WireKite, null, () => "Combine Both!",  
        () => "When some features require both client and server management,\n"  
            + "this setup becomes particularly useful.");  
}  
```

### AddPrison
Add new house for Wand of Architecture
#### Parameters
- `Texture2D` Data texture containing the info of the house
- `Texture2D` A preview for the house

#### Data texture format

| Color       | Hex Code | Represents   |
|-------------|----------|--------------|
| Red         | FF0000   | Block        |
| Black       | 000000   | Platform     |
| White       | FFFFFF   | Torch        |
| Yellow      | FFFF00   | Chair        |
| Pink        | FF00FF   | Table        |
| Blue        | 0000FF   | Workbench    |
| Purple      | 7F00FF   | Bed          |
| Cyan        | 00FFFF   | NoWall       |
| Green       | 00FF00   | Door         |
| Transparent |          | Wall         |

### RegisterWeatherControl

Register a control item into the Weather Control panel. Items without a group go into the default "modded" grid on the panel
It is recommended to copy [ImproveGame_WeatherControlCrossModHelper.cs](ImproveGame_WeatherControlCrossModHelper.cs) into your mod and use the strongly-typed wrappers instead of raw `Mod.Call`

#### Parameters

- `Mod` The source mod instance, used as the ModName of the control
- `string` Internal name of the control, combined with ModName as the unique id `ModName:Name`
- `Texture2D` Slot icon, anything larger than 32x32 will be scaled down to fit
- `Func<string>/LocalizedText/string` Display name provider, shown as the first line of the hover tooltip
- `Func<string>/LocalizedText/string` Hover description, may be `null`
- `string[]/IReadOnlyList<string>` Stage name array with at least 2 entries. The index order is the stage order. Names are internal keys only and not displayed
- `Func<int>` Returns the current stage index, `-1` if unknown
- `Action<int>` Applies a stage locally, no networking required
- `bool` Optional, whether the item supports locking, default `false`
- `Func<bool>` Optional, returns the current locked state
- `Action<bool>` Optional, applies the locked state locally
- `Func<bool>` Optional, decides whether the item is currently available. Unavailable items are hidden from the panel
- `int` Optional, sort priority, higher first, default `0`
- `string` Optional, id of the group this item belongs to. Leave empty to fall into the default modded grid

#### Return Value

- `bool` Whether the registration succeeded. Returns `false` if stages are insufficient or required arguments are missing

#### Notes

- `setStage` and `setLocked` only need to write the local state. Network sync is handled by `SetWeatherControlStage` / `SetWeatherControlLocked`, which dispatch even in single player
- Registering twice with the same `(ModName, Name)` overwrites the previous entry

### UnregisterWeatherControl

Remove a registered weather control item

#### Parameters

- `Mod` The source mod instance
- `string` Internal name of the control

### RegisterWeatherControlGroup

Register a custom content group. A group lets your mod attach its own UI view to the Weather Control panel instead of dropping every item into the default modded grid

#### Parameters

- `Mod` The source mod instance
- `string` Internal name of the group
- `Func<string>/LocalizedText/string` Display name provider
- `int` Sort priority, higher first
- `Func<UIElement>` Builds the group view, invoked once each time the Weather Control panel opens
- `Func<bool>` Optional, decides whether the group is currently available

### UnregisterWeatherControlGroup

Remove a registered group

#### Parameters

- `Mod` The source mod instance
- `string` Internal name of the group

### QueryWeatherControlStage

Read the current stage index of a control. Works on builtin items too

#### Parameters

- `string` Full id of the control, formatted as `ModName:Name`

#### Return Value

- `int` Current stage index, `-1` when the control does not exist

### SetWeatherControlStage

Dispatch a stage change. All clients apply the update

#### Parameters

- `string` Full id of the control
- `int` Target stage index

### IsWeatherControlLocked

Read whether a control is currently locked

#### Parameters

- `string` Full id of the control

#### Return Value

- `bool` Whether the control is locked. Returns `false` when the control does not exist or does not support locking

### SetWeatherControlLocked

Dispatch a locked state change. Returns `false` if the target control does not support locking

#### Parameters

- `string` Full id of the control
- `bool` Target locked state

### Built-in Weather Control Ids

| Id | Item | Stages |
| --- | --- | --- |
| `ImproveGame:Time` | Time of day | `Dawn` / `Noon` / `Dusk` / `Midnight` |
| `ImproveGame:MoonPhase` | Moon phase | `Phase0` ~ `Phase7` |
| `ImproveGame:Rain` | Rain | `Off` / `On` |
| `ImproveGame:Sandstorm` | Sandstorm | `Off` / `On` |
| `ImproveGame:Wind` | Wind direction | `West` / `No` / `East` |

### Example

The snippet below registers a custom weather state from another mod through the helper (copy [ImproveGame_WeatherControlCrossModHelper.cs](ImproveGame_WeatherControlCrossModHelper.cs) into your project first):

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
    // Drop the registration so the Registry does not keep callbacks pointing into an unloaded assembly
    if (ModLoader.TryGetMod("ImproveGame", out var qot))
        ImproveGame_WeatherControlCrossModHelper.UnregisterWeatherControl(qot, this, "ScorchingDay");
}
```

Stage transitions go through `SetWeatherControlStage`, which works on builtin items as well:

```CSharp
if (ModLoader.TryGetMod("ImproveGame", out var qot))
{
    // Start raining across all clients
    ImproveGame_WeatherControlCrossModHelper.SetWeatherControlStage(
        qot, ImproveGame_WeatherControlCrossModHelper.RainId, 1);

    int now = ImproveGame_WeatherControlCrossModHelper.QueryWeatherControlStage(
        qot, ImproveGame_WeatherControlCrossModHelper.RainId);
}
```
