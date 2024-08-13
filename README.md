# Map Link

[Video Demo](https://github.com/user-attachments/assets/1f7bfdab-98aa-4c1d-8b1e-e8294bd4b2f2)

Automatically opens the last linked flag coordinates from chat

## Notes
- This is filled with bad coding practices and bugs
- This has only been tested on the Global EN version

## Installation

[Custom Plugin Repository](https://gist.githubusercontent.com/sofia819/fb17fff59d39923fde123538dbf8b92b/raw/sofia-plugins.json)

## Settings

![settings](settings.png)

-   Enabled: turns MapLink on/off
-   Log: logs `[MapLink] Player Name posts a map link` in chat
-   Player Name Input: adds `Player Name` to the filter
-   Player Name :heavy_check_mark:: enables filtering by `Player Name`
-   Player Name :x:: removes `Player Name` from the filter

#### Notes

-   If the filter is empty or if all filter entries are disabled, it will watch all messages

## Commands

-   `/mpl`: toggles MapLink on/off and logs `[MapLink] ON/OFF` in chat
-   `/mpl cfg`: opens MapLink settings
-   `/mpl [Player Name]`: adds a player to the filter
