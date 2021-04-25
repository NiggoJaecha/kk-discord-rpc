
# Koikatsu: Discord Rich Presence

Alpha out now!

Requires (newest version would be best) Version of KKAPI and Sideloader.

## What this does

This mod enables Discord Rich Presence for Koikatsu. 

It's completly customisable via the BepInExs Config. (To open the Config in Koikatsu press F1)

## How to use

You can change every displayed message to any freetext. 

Using keywords in <> you can insert variables into the messages.

For example `<heroine_name>` will be replaced with the full name of the leading character in a H-Scene.

For a List of possible Keywords hover over the ConfigEntry in the Settings.

Right now you can choose between 4 images to show for each activity, but more images are planned.

## Installation

1. Download the latest release [here](https://github.com/NiggoJaecha/kk-discord-rpc/releases)
2. Download the latest discord-rpc release for Windows from [here](https://github.com/discord/discord-rpc)
3. Copy `discord-rpc.dll` from `discord-rpc-win\discord-rpc\win64-dynamic\bin` to your Koikatsu root folder (where the .exe files are)
4. Drop `KK_DiscordRPC.dll` into `[Koikatsu Root]\BepInEx\plugins`
5. Configure the Rich Presence inside the Game

## Credits

Parts of the Code are "borrowed" from [here](https://github.com/eai04191/craftopia-rpc) which "borrowed" the code from [here](https://github.com/Weilbyte/RWRichPresence)!

Base Code forked from Varkaria/kk-discord-rpc

Thanks to #mod-programming on the Koikatsu Discord for extensive help with the KKAPI
