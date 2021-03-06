# Dragon Front Companion Mobile App
![](DragonFrontCompanion.iOS/Assets.xcassets/LaunchImage.launchimage/Default.png)

## Download from App Store
* [iOS: App Store](https://itunes.apple.com/us/app/df-companion/id1181274447) 
* [Android: Google Play](https://play.google.com/store/apps/details?id=com.benreierson.dragonfrontcompanion)
* [Windows 10: Marketplace](https://www.microsoft.com/store/apps/9p9lfr99bfg7) (Desktop)

**This is not an official app. Dragon Front is a trademark or registered trademark of High Voltage Software, Inc. in the U.S. and/or other countries.**

### Discuss the app and provide feedback on Discord - https://discord.gg/9jzt4nY
### Join the Dragon Front Community on Discord - https://discord.gg/c3XntB3


Dragon Front Companion is a way to learn all the details of every card in the Dragon Front game, build your own decks, and share them with the community. It was initially built by a single developer over approxmiately two months as a side-project to support the growing Dragon Front community, and as a way to explore the latest Xamarin development tools and techniques.

## Development 
Master Branch - Android [![Build status](https://build.appcenter.ms/v0.1/apps/86a9b93d-0a39-41a7-bd1b-095dd3c60bad/branches/master/badge)](https://appcenter.ms)    iOS [![Build status](https://build.appcenter.ms/v0.1/apps/f8a07383-f377-47f7-95ac-76d54540e807/branches/master/badge)](https://appcenter.ms)    UWP [![Build status](https://build.appcenter.ms/v0.1/apps/314986bc-890b-4391-be88-7a5bb6242e6d/branches/master/badge)](https://appcenter.ms)

The app is written in C# using Xamarin.Forms, MVVMLight, and various Xamarin plugins including FFImageLoading, PCLStorage, SlideOverKit, DeviceInfo, and RG Popup. It utilizes [DragonFrontDB] (https://github.com/BenReierson/DragonFrontDb) for the game data. Throughout initial development, it was hosted on Visual Studio online, but has now been migrated to Github and integrated with mobile.azure.com for CI, UITests, analytics, and alpha testing.

Currently (July 2017), the app does not require a server-backend or an internet connection, though it queries for updated card data when possible. This makes it an excellent real-world, but relatively straight-forward, example of a Xamarin.Forms project that is currently deployed and maintained in all major app stores. The vast majority of code is shared between all three platforms. Card and faction images, as seen in the app store versions, are not included in this repository due to copyright restrictions. However, the app will build and run without them.

## Contributing
This app is currently maintained by [Ben Reierson] (https://github.com/BenReierson), and releases to the store come directly from this repository's master branch (after images are added). Feature requests, issues, and pull requests are all welcome and will be considered. Feel free to reach out. 
