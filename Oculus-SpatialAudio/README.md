# Spatial Audio with Agora on Oculus
This Unity project package includes the scene file and its dependent resources for running a spatial audio test on individual user objects.  Remote users from non VR platforms can be viewed on the Capsule. And their sound will be originated from the Capsule object in the VR environment. 
![oculus scene](https://user-images.githubusercontent.com/1261195/123018737-f0db1c00-d383-11eb-90e1-e2bacf3e03d7.gif)

## Developer Environment Requirements
- Unity3d 2019 LTS or above
- Oculus Quest
- extra non VR device with camera and microphone input

## Quick Start

This section shows you how to prepare, build, and run the sample application. 

### Obtain an App ID

To build and run the sample application, get an App ID:
1. Create a developer account at [agora.io](https://dashboard.agora.io/signin/). Once you finish the signup process, you will be redirected to the Dashboard.
2. Navigate in Agora Console on the left to **Projects** > **More** > **Create** > **Create New Project**.
3. Save the **App ID** from the Dashboard for later use.

### Run the Application   

1. Set up Unity environment for Oculus Quest ([see offical guide](https://developer.oculus.com/documentation/unity/unity-gs-overview/))
2. Download [Agora Video SDK for Unity](https://assetstore.unity.com/packages/tools/video/agora-video-sdk-for-unity-134502)
3. Import this package
4. Fill in App ID and make sure all other fields gets filled too.  Capsule is the default prefab but you may make your own for replacement.
5. Please make sure if your AppID has token or not.  Things won't work if you don't supply a token if your AppID requires one.
![AgoraRoot](https://user-images.githubusercontent.com/1261195/123020656-7dd3a480-d387-11eb-9fee-d4308cfed33d.png)


#### Test in Editor 
You can run this project in the Editor.  But you may have to fix a Oculus SDK bug:

Update the OculusSampleFrameworkUtil.cs script with the following code snippet:
``` 
private static void HandlePlayModeState(PlayModeStateChange state)
{
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            System.Version v0 = new System.Version(0, 0, 0);
            UnityEngine.Debug.Log("V0=" + v0.ToString());
#if UNITY_EDITOR
            OVRPlugin.SendEvent("load", v0.ToString(), "sample_framework");
#else
            OVRPlugin.SendEvent("load", OVRPlugin.wrapperVersion.ToString(), "sample_framework");
#endif
        }
 }
```
## Resources

- For potential Agora SDK issues, take a look at our [FAQ](https://docs.agora.io/en/faq) first
- Dive into [Agora SDK Samples](https://github.com/AgoraIO/Agora-Unity-Quickstart) to see more API samples for Unity
- Take a look at [these blogs](https://www.agora.io/en/category/developer/) for developer contents
- Repositories managed by developer communities can be found at [Agora Community](https://github.com/AgoraIO-Community)


## License
The MIT License (MIT).

