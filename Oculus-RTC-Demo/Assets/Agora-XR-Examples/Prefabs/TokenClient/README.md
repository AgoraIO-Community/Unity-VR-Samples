# UnityTokenPrefab
A low code prefab helps to get and renew tokens for RTC and RTM engine.  Note that the RTM code is commented out. Uncommit to use with RTM.




# How to use this prefab
 

1.  Download the ClientToken.unitypackage from the release [here](https://github.com/AgoraIO-Community/UnityTokenPrefab/releases). Which belongs to the [GitHub repo](https://github.com/AgoraIO-Community/UnityTokenPrefab) for this prefab’s source.
    
2.  On Unity Editor, import the package from Assets->Import Package->Custom Package
    
3.  Drag the TokenClient prefab from Assets/AgoraEngine/Utils to the scene.
    
4.  Fill the information in the TokenClient’s properties (See picture below)
    

1.  Client Type - select Publisher or Subscriber. Note you may also assign this in code. E.g.  
    TokenClient.Instance.SetClient(ClientType.publisher);
    
2.  AppID: the App ID for your project. It is assumed you use the same AppID for both RTC and RTM engines.
    
3.  serverURL: the endpoint that serves the query
    
4.  ExpiractionSecs: how long the token lives
    

  
  
  

5.  ![](https://lh4.googleusercontent.com/bwGN3xWTw4jf5Pgsbh3kjTwQAD27O5WiTeFjSOUpSm2OltcBLlH4nz8qjEzH-9PgucBwNYYasfQhoHQrAU_jmnf7cJrnJbNeSZJ7ZoV1ONukq8pYWYJSSi9dNxfYf5QQMJcqz8YL)
    

5.  You will need to make calls from your main controller to assign the necessary engine instances and initiate GetToken. Sample code snippet below:  
      
    ```csharp
        // assign user id here, or use 0 for local user
        uint uid = 0;
        string UserName = "Me";
        TokenClient.Instance.RtcEngine = mRtcEngine;
        TokenClient.Instance.RtmClient = mRtmClient;
        TokenClient.Instance.GetTokens(channel, uid,
            (rtcToken, rtmToken) =>
            {
                // join channel with token
                mRtcEngine.JoinChannelByKey(rtcToken, channel, null, uid);
                mRtmClient.Login(rtmToken, UserName);
            }
        );
        // join channel without token
        // mRtcEngine.JoinChannel(channel, null, uid);
    ```

# Server Side
The endpoint query URLs that the Token Client are conforming to the sample server project as described in this blog:
[ How to Build a Token Server for Agora Applications using GoLang](https://www.agora.io/en/blog/how-to-build-a-token-server-using-golang/)
Its GoLang example's complete server code is in [this Repo](https://www.google.com/url?q=https://github.com/AgoraIO-Community/agora-token-service&sa=D&source=editors&ust=1627689782755000&usg=AOvVaw0JzTd92OZnjmV5SUo-nNQW). To recap, these are quick start steps:

1.  set up your GoLang environment,
2.  clone the Repo,
3.  install Gin framework,
4.  set your environment variable (AppID and AppCertificate)
5.  run main.go
