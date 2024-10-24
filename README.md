# RoR2-VTSIntegration

Automatically sends picked up items to VTube Studio. Some setup required!

## Setup

If this is your first time using a VTube Studio plugin, you can learn more about them [here](https://github.com/DenchiSoft/VTubeStudio/wiki/Plugins)

**Before launching the game, make sure VTube Studio is open and the API is enabled in settings**

![Image of "Start API" button](https://i.imgur.com/aCBOrnj.png)

After launching the game, the mod will ask for permission to use the API in VTube Studio, then will ask again to load custom items. Denying either of these will prevent the mod from working. This only has to be done when launching the mod for the first time.

![Image of Authentication popup](https://i.imgur.com/6yqakFo.png)

![Image of custom item popup](https://i.imgur.com/mDOKMK1.png)

## Configuring items

After the mod is authenticated, picking up any item will make the item appear in VTS. Position of the item will be saved if moved, however, due to limitations in its API, rotating, resizing, and pinning the item normally won't be saved

---

### Loading an item for configuration

To send an item to VTS to be modified, open your logbook and press F2 while hovering the item you wish to configure. Press F2 again to hide the item. The logbook will be used for several other configuration options as well

---

### Moving/flipping the item

For changing an items position, just drag it in VTS. Ctrl + click will flip the item.

---

### Pinning the item

To pin an item to your model, first lock the item in place, away from your model, by double clicking it. Next, click anywhere on your model. If successful, the item will be moved and pinned to wherever you clicked. Pinning an item this way will be saved.

---

### Rotating/resizing the item

Rotating/resizing the item uses similar controls as VTS, but hovering the item in the logbook instead.

Mouse wheel up/down to grow/shrink the item.

Ctrl + Mouse wheel up/down to rotate the item clockwise/counter-clockwise.

Both can be sped up by also holding shift.

Changing rotation/size this way will be saved

**Items will NOT scale with your model. You should be configuring items with your model in the size/position you plan to use most of the time**

---

### Making an item appear behind your model

By default, all items will appear in front of your model. Press F3 while hovering the item in your logbook to make the item appear behind your model. Press again to put it back in front. Note that VTS only allows 30 items each in front and behind your model.

---

### Using custom images instead of the item sprite

Press F4 while hovering over the item you want to replace the image for. Then, load any image/Live2D item into VTS using it's menu. If successful, the image you spawned will be removed, and it will be used whenever you pickup the item. Press Ctrl + F4 to reset the item back to its default sprite.

---

### Disabling an item

To prevent an item from being sent to VTS again, drag it to the trash can at the bottom right of the screen. It can be re-enabled by pressing F2 while hovering the item in your logbook.
**Other methods that delete items (such as "remove all items in scene") will also disable them.**

---

### Extra logbook entries

This mod causes some items to appear in the logbook, allowing you to use the configuration options above. Notably consumed versions of items, tonic afflictions, and the elite aspects. Items can be added or removed in the config.

## Ko-fi

If you enjoy the mod, consider
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/U7U514O95B)

## Streamer Comments

*If you would like to be featured here, message me on discord @doomy64 with a comment about the mod*

`Mind the Bungus`

--[h2f_Pinkis](https://twitch.tv/h2f_Pinkis)

## Credits

h2f_Pinkis - For testing every hellish iteration of this plugin before release

[sta](https://github.com/sta/websocket-sharp) - For the websocket-sharp library 
