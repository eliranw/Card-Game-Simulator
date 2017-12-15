# Defining Custom Games
Card Game Simulator allows users to download custom card games to use within the application. The process for downloading a custom card game through the ui is documented in the [main documentation](README.md).

## CGS games directory
In addition to downloading a custom game from a url, custom card games can also be manually added by creating a new folder within the persistent game data directory. The location of this persistent data directory varies depending on platform. Some examples include:
- Android: /Data/Data/com.finoldigital.cardgamesim/files/games/
- Windows: C:/Users/\<Username\>/AppData/LocalLow/Finol Digital/Card Game Simulator/games/
- Mac: ~/Library/Application Support/Finol Digital/Card Game Simulator/games/

## Custom game folder structure
The structure of this custom game folder is:
- \<Name\>/
  - \<Name\>.json
  - AllCards.json
  - AllSets.json
  - Background.\<BackgroundImageFileType\>
  - CardBack.\<CardBackImageFileType\>
  - boards/
    - *Board:Id*.\<BoardFileType\>
    - ...
  - decks/
    - *Deck:Name*.\<DeckFileType\>
    - ...
  - sets/
    - *Set:Code*/
      - *Card:Id*.\<CardImageFileType\>
      - ...
    - ...

## JSON File Structure
When downloading a custom game from a url, the data that is being downloaded is the contents of the \<Name\>.json file. CGS generates the rest of the folder structure based off the information in that file. You can create your own json and validate against these schema:
- [CardGameDef](schema/CardGameDef.json)
- [AllCards](schema/AllCards.json)
- [AllSets](schema/AllSets.json)

## Examples
Functional examples can be found in the [CGS Google Drive folder](https://drive.google.com/open?id=1kVms-_CXRw1e4Ob18fRkS84MN_cxQGF5).