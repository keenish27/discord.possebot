# discord.possebot
Discord Posse Bot. Will play themes when users enter voice channels among other things.

## Setup
This bot require opus.dll, libsodium.dll, and ffmpeg.exe. Because of this the current implementation will only compile and run on a windows machine.

### Configuration
The bot uses two json files to hold settings. These files are *settings.json* and *themes.json*. You will need to make copies of [settings.example.json](keeganstudios.possebot/settings.example.json) and [themes.example.json](keeganstudios.possebot/themes.example.json) and rename them appropriately. They should be placed in the same directory that the application is running from.

#### settings.json
This file contains all of the system level bot settings. Please replace each placeholder with an appropriate value.
```json
{
  "configuration": {
    "token": "<Token>",
    "botPrefix": "<BotPrefix>"
  }
}
```
You will then need to replace the values inside.
* **configuration**: Property used to hold all bot configuration settings
  * **token**: Property that holds your discord bot's token. **this token is private and should not be shared or commited to source control**
  * **botPrefix**: Property that holds the chat prefix the bot will respond to.

#### themes.json
This file contains all of the saved user themes. If you wish to initialize the bot with a predetermined theme for a user then replace each placeholder with an appropriate value. You may also set **themes** to an empty array and use the bot commands to populate these values.
```json
{
  "themes": [
    {
      "audioPath": "<AudioPath>",
      "userId": "<UserId>",
      "guildId": "<GuildId>",
      "start": 0,
      "duration": 15,
      "enable": true
    }
  ]
}
```
* **themes**: Property used to hold all user theme settings
  * **audioPath**: Property that holds the path to the audio file to be played.
  * **userId**: Property that holds the user's id that the theme is associated with.
  * **guildId**: Property that holds the user's guild id that the theme is associated with (a user can have a seprate theme per guild).
  * **start**: Property that holds the start position (in seconds) of the audio file.
  * **duration**: Property that holds the duration (in seconds) of the audio file.
  * **enable**: Property that holds the boolen value that determines if the theme will play or not.

### Audio Files
When using the bot commands to set a user's theme the system will save all audio files in a *files* directory. This directory will be located in the same location that the bot is running from. The directory will have the following structure:
```
  -files
    -guildId
      -audiofile
```
