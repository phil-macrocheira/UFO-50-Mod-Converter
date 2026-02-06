# UFO 50 Mod Converter
This is a reorganized version of the code in [GMLoader](https://github.com/Senjay-id/GMLoader) that converts a modded Game Maker `data.win` into a mod for GMLoader. It does so by exporting all files from both `data.win` files and copying out the files that differ. Code credit to Senjay, Kneesnap, Grossley, SolventMercury, Agentalex9, and the UndertaleModTool team.

Though it is designed for use with UFO 50 in mind, it can be used for any Game Maker game.

## Usage
Copy into the folder both a vanilla data.win called `vanilla.win` and a modded data.win called `data.win`. For best results, ensure that the `vanilla.win` is from the version of the game that the game was modded from.
### Exporting Textures
To export UFO 50 v1.7+ textures, make sure that the `Textures` folder from the UFO 50 game install files is also copied in.
### GMLoader.ini
You can modify the `GMLoader.ini` file to adjust some settings before use:
* **ExportCode:** Whether or not to export .GML code files
* **ExportTextures:** Whether or not to export .PNG textures and .JSON texture property files
* **ExportBackgrounds:** Whether or not to export .PNG background textures and .JSON background texture property files
* **ExportObjects:** Whether or not to export .JSON object data files
* **ExportRooms:** Whether or not to export .JSON room data files
* **ExportAudio:** Whether or not to export audio files and .JSON audio property files
* **ReuseVanillaExport:** Whether or not to reuse the exported vanilla files to skip exporting it next time if it already exists
* **AutoDeleteVanillaExport:** Whether or not to automatically delete the vanilla export at the end
* **AutoDeleteModdedExport:** Whether or not to automatically delete the modded export at the end
* **TexturesToIgnore:** A comma separated list of filenames of textures to skip and ignore when exporting textures. Already includes 2 filenames of unused test textures in UFO 50 that cause warnings.