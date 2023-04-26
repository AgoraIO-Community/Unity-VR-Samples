# Agora RTC on Oculus

## Sample Scene

![OculusRTCDemo](https://user-images.githubusercontent.com/1261195/234465208-0e35d5c9-a83e-4b04-8fa0-b1e93317520e.jpg)


## Developer Environment Prerequisites
- Unity3d 2021.3 LTS
- Oculus Quest/Quest2
- Agora Developer Account

## Quick Start



This section shows you how to prepare, build, and run the sample application.

  

### Obtain an App ID

  

To build and run the sample application, get an App ID:

1. Create a developer account at [agora.io](https://dashboard.agora.io/signin/). Once you finish the signup process, you will be redirected to the Dashboard.

2. Navigate in Agora Console on the left to **Projects** > **More** > **Create** > **Create New Project**.

3. Save the **App ID** from the Dashboard for later use.

  

### Run the Application

  

#### Build to Oculus

1. Clone this repo and open the project from this folder
2. Set up Unity environment for Oculus Quest ([see offical guide](https://developer.oculus.com/documentation/unity/unity-gs-overview/))
3. Download [Agora Video SDK for Unity](https://assetstore.unity.com/packages/tools/video/agora-video-sdk-for-unity-134502) (Version 4.1.0 or above)

4. Fill in App ID and make sure all other fields gets filled too.  ![OculusRTCDemo-AppID](https://user-images.githubusercontent.com/1261195/234465370-7a09702f-7429-43d7-8fea-6feb4b573149.jpg)

5. Make sure if your AppID has token or not.  Things won't work if you don't supply a token if your AppID requires one.  We recommend use an AppID for testing first before applying token logic.
6. Use [the Web Demo](https://webdemo.agora.io/basicVideoCall/index.html)as a second user to test the RTC call.
  

  

## License

The MIT License (MIT).