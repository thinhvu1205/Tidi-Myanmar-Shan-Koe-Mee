using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;
using System;

public class CreateAssetBundles
{
    [MenuItem("Asset Bundles/Clear Cache")] private static void _ClearCache() => Debug.Log(Caching.ClearCache() ? "Cache cleared!" : "Cache not cleared!!!");
    [MenuItem("Asset Bundles/Clear Prefs")] private static void _ClearPrefs() => PlayerPrefs.DeleteAll();

    [MenuItem("Asset Bundles/Build/Android")]
    private static void _BuildForAndroid()
    {
        string basePath = _PrepareRootFolder(BundleHandler.PLATFORM.Android);
        if (basePath.Equals("")) return;
        AssetBundleManifest manifestABM = BuildPipeline.BuildAssetBundles(basePath, BuildAssetBundleOptions.None, BuildTarget.Android);
        _BuildCategory(basePath, manifestABM);
    }
    [MenuItem("Asset Bundles/Build/iOS")]
    private static void _BuildForiOS()
    {
        string basePath = _PrepareRootFolder(BundleHandler.PLATFORM.iOS);
        if (basePath.Equals("")) return;
        AssetBundleManifest manifestABM = BuildPipeline.BuildAssetBundles(basePath, BuildAssetBundleOptions.None, BuildTarget.iOS);
        _BuildCategory(basePath, manifestABM);
    }
    private static string _PrepareRootFolder(BundleHandler.PLATFORM _platformType)
    {
        string basePath = BundleHandler.BASE_PATH;
        switch (_platformType)
        {
            case BundleHandler.PLATFORM.Android:
                {
                    basePath += BundleHandler.PLATFORM.Android.ToString();
                    break;
                }
            case BundleHandler.PLATFORM.iOS:
                {
                    basePath += BundleHandler.PLATFORM.iOS.ToString();
                    break;
                }
            default:
                {
                    basePath = "";
                    break;
                }
        }
        if (basePath.Equals("")) return "";
        if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
        else
        {
            Directory.Delete(basePath, true);
            Directory.CreateDirectory(basePath);
        }
        return basePath;
    }
    private static void _BuildCategory(string _path, AssetBundleManifest _manifestABM)
    {
        DirectoryInfo aDI = new(_path);
        FileInfo[] infoFIs = aDI.GetFiles();
        List<string> hashedNames = new();
        foreach (FileInfo infoFI in infoFIs) _RenameBundle(_manifestABM, AssetDatabase.GetAllAssetBundleNames(), infoFI, hashedNames);
        string categoryContent = "[" + string.Join(",", hashedNames) + "]";
        string categoryPath = _path + "/" + BundleHandler.CATEGORY;
        if (File.Exists(categoryPath)) File.Delete(categoryPath);
        File.WriteAllText(categoryPath, categoryContent);
    }
    private static void _RenameBundle(AssetBundleManifest _manifestABM, string[] _assetBundleNames, FileInfo _infoFI, List<string> _nodes)
    {
        if (!_infoFI.Name.EndsWith(".manifest") && _assetBundleNames.Contains(_infoFI.Name))
        {
            Hash128 hash = _manifestABM.GetAssetBundleHash(_infoFI.Name);
            string suffix = BundleHandler.SPLIT + hash.ToString();
            _nodes.Add('"' + _infoFI.Name + suffix + '"');
            File.Move(_infoFI.FullName, _infoFI.FullName + suffix);
        }
    }
}
