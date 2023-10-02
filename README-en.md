<h1 align="center">Quality of Life</h1>

<div align="center">

English | [简体中文](README.md)

[Change log](ChangeLog-en.md)

A mod that aims to raise quality of gameplay to the next level, to benefit the community

</div>

## ✨ Features
For a detailed list, check mod configurations or changelog above
1. General features like max stack, weapon auto reuse (benchmarking Luiafk the classic qol mod)
2. Non-vanity accessories can be worn in vanity slots
3. Town NPC spawn mechanic improvements: can respawn during nighttime, ignore respwaning limitations if bestiary is unlocked
4. Mod items such as Wand of Space, Wand of Architecture, Wand of Bursts and Fishing Machine which greatly improve gameplay experience
5. Integration of qol features like instantly refreash Angler's quests and disable tomb stone spawning
6. Time-saving features like Infinite Buffs and Portable Crafting Stations
7. Features specially made for multiplayer, for example sharing your infinite buffs, portable crafting stations and even more with your teammates
8. A Huge Inventory providing 100 item slots, no warries of storing your potions and banners
9. Potion Bag and Banner Box, integrate all your potions or banners as a single item (and you are free to take them out)
10. Most features are configurable, use on your own preference

## ⬇️ Download
You can subscribe and download it via steam workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=2797518634

## 💻 How to compile
This mod contains Nuget package, which means you should not use the compilation of tModLoader, but you should use the compilation function of the code IDE (such as Visual Studio, Rider) to compile this mod
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
This mod: https://gitee.com/MyGoold/improve-game (The original repo, the github repo is a mirror)
WMITF: https://github.com/gardenappl/WMITF
ChevyRay's coroutine class: https://github.com/ChevyRay/Coroutines
ProjectStarlight.Interchange: https://github.com/ProjectStarlight/ProjectStarlight.Interchange
Auto Piggy Bank: https://github.com/diniamo/auto-piggy-bank

## 🤝 Cross-mod Support (Mod.Call)
### IgnoreInfItem
```"IgnoreInfItem", int/List<int> ItemID(s)```

Ignore infinite buff(s) provided by certain item(s), hence it/they no longer provide(s) infinite buff(s)
### AddPotion
```"AddPotion", int ItemID, int BuffID```

Add infinite buff(s) for a potion, especially effective to items which do not have Item.buffType or have multiple buffs. 30 potions are needed to activate the effect
### AddStation
```"AddStation", int ItemID, int BuffID```

Add infinite buff(s) for a buff station, usually you have to call manually for mod buff station. Only 1 item is needed to activate its effect
### AddPortableCraftingStation
```"AddPortableCraftingStation", int ItemID, int/List<int> TileID(s)```

Add portable crafting station support for a specified item. This is useful for items that do not have Item.createTile set but should act as some sort of crafting station. Multiple crafting stations can be specified.

If you want it to act as a water source, you should set the TileID to TileID.Sink

### Example
Here is an example of adding infinite buffs for 2 mod buff stations
```CSharp
public override void PostSetupContent() {
    if (ModLoader.TryGetMod("ImproveGame", out Mod improveGame)) {
        improveGame.Call(
            "AddStation", // Add your first buff station
            ModContent.ItemType<MyStation1>(), // ItemID 1
            ModContent.BuffType<MyStationBuff1>() // BuffID 1
        );
        improveGame.Call(
            "AddStation", // Add your second buff station
            ModContent.ItemType<MyStation2>(), // ItemID 2
            ModContent.BuffType<MyStationBuff2>() // BuffID 2
        );
    }
}
```