using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

/// <summary>
/// Forces iOS Info.plist Facebook tracking flags off so the binary matches
/// App Store Connect: this app does not track users (no ATT required).
/// </summary>
public static class IOSPrivacyPostProcess
{
    [PostProcessBuild(1000)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
#if UNITY_IOS
        if (buildTarget != BuildTarget.iOS)
        {
            return;
        }

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        if (!File.Exists(plistPath))
        {
            return;
        }

        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        plist.root.SetBoolean("FacebookAdvertiserIDCollectionEnabled", false);
        plist.root.SetBoolean("FacebookAutoLogAppEventsEnabled", false);

        plist.WriteToFile(plistPath);
#endif
    }
}
