# Dragon Front Companion Mobile App
![](DragonFrontCompanion.iOS/Assets.xcassets/LaunchImage.launchimage/Default.png)

Now updated to dotnet Maui for the Dragon Front Rising re-release!

## Download from App Store
* [iOS: App Store](https://itunes.apple.com/us/app/df-companion/id1181274447) (v2.x)
* [Android: Google Play](https://play.google.com/store/apps/details?id=com.benreierson.dragonfrontcompanion) (v2.x)
* [Microsoft Store](https://apps.microsoft.com/detail/9P9LFR99BFG7) (v1.4 now, v2.x pending)

**This is not an official app. Dragon Front is a trademark or registered trademark of High Voltage Software, Inc. in the U.S. and/or other countries.**

### Join the Dragon Front Community on Discord - https://discord.gg/c3XntB3


Dragon Front Companion is a way to learn all the details of every card in the Dragon Front game, build your own decks, and share them with the community. It was initially built by a single developer over approxmiately two months as a side-project to support the growing Dragon Front community, and as a way to explore the latest Xamarin development tools and techniques.

## Development 
The app is written in C# using dotnet 8.0 and Maui along with several libraries incliding the Maui Community Toolkit and FfimageLoading. It utilizes [DragonFrontDB] (https://github.com/BenReierson/DragonFrontDb) for the game data. Initially the app was built with Xamarin Forms and has been recently ported to Maui, so it serves as a good example of such a port. 

The app does not require a server-backend or an internet connection, though it queries for updated card data when possible. This makes it an excellent real-world, but relatively straight-forward, example of a Maui project that is currently deployed and maintained. The vast majority of code is shared between platforms. Card and faction images, as seen in the app store versions, are not included in this repository due to copyright restrictions. However, the app will build and run without them.

## Contributing
This app is currently maintained by [Ben Reierson] (https://github.com/BenReierson), and releases to the store come directly from this repository's master branch (after images are added). Feature requests, issues, and pull requests are all welcome and will be considered. Feel free to reach out. 
