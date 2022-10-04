# Dragon Front Companion Mobile App
![](DragonFrontCompanion.iOS/Assets.xcassets/LaunchImage.launchimage/Default.png)

**This is not an official app. Dragon Front is a trademark or registered trademark of High Voltage Software, Inc. in the U.S. and/or other countries.**

## This app has been removed from the app stores now that Dragon Front has been shut down. The codebase is not being maintained, but it's available as an example and for education purposes. 

## Development 
The app is written in C# using Xamarin.Forms, MVVMLight, and various Xamarin plugins including FFImageLoading, PCLStorage, SlideOverKit, DeviceInfo, and RG Popup. It utilizes [DragonFrontDB] (https://github.com/BenReierson/DragonFrontDb) for the game data. Throughout initial development, it was hosted on Visual Studio online, but has now been migrated to Github and integrated with mobile.azure.com for CI, UITests, analytics, and alpha testing.

The app does not require a server-backend or an internet connection, though it queries for updated card data when possible. This makes it an excellent real-world, but relatively straight-forward, example of a Xamarin.Forms project that is currently deployed and maintained in all major app stores. The vast majority of code is shared between all three platforms. Card and faction images, as seen in the app store versions, are not included in this repository due to copyright restrictions. However, the app will build and run without them.

