# discord.possebot
Discord Posse Bot. Will play themes when users enter voice channels among other things.

## Setup
This bot require opus.dll, libsodium.dll, and ffmpeg.exe. Because of this the current implementation will only compile and run on a windows machine.

### Configuration
The bot uses a json file to hold its settings. This file is *settings.json*. You will need to make a copy of [settings.example.json](keeganstudios.possebot/settings.example.json) rename it appropriately. It should be placed in the same directory that the application is running from.

#### settings.json
This file contains all of the system level bot settings. Please replace each placeholder with an appropriate value.
```json
{
  "configuration": {
    "token": "<Token>",
    "botPrefix": "<BotPrefix>",
    "dbFolder": "<DbFolder>"
  }
}
```
You will then need to replace the values inside.
* **configuration**: Property used to hold all bot configuration settings
  * **token**: Property that holds your discord bot's token. **this token is private and should not be shared or commited to source control**
  * **botPrefix**: Property that holds the chat prefix the bot will respond to.
  * **dbFolder**: Property that holds the folder that the database file will be created.

### Audio Files
When using the bot commands to set a user's theme the system will save all audio files in a *files* directory. This directory will be located in the same location that the bot is running from. The directory will have the following structure:
```
  -files
    -guildId
      -audiofile
```
