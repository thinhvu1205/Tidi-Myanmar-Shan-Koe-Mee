using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BundleDownloader : MonoBehaviour
{
    public const string STORED_BUNDLE_URL = "storedBundleUrl";
    [SerializeField] private Image m_ProgressImg;
    [SerializeField] private TextMeshProUGUI m_ProgressTMPUI;
    private List<Coroutine> _SentGettingBundleCs = new();
    private List<BundleVersion> _NewDataBVs = new(), _StoredDataBVs = new();
    private Action _OnFailCb, _OnCompleteCb;
    private int _TotalBundles, _CachedBundles;

    public void CheckAndDownloadAssets(string _url, float _delay = 0, Action _onFailCb = null, Action _onCompleteCb = null)
    {
        if (_url.Equals(""))
        {
            _onFailCb?.Invoke();
            return;
        }
        if (!_url[^1].Equals('/')) _url += "/"; // if it does not end with "/" then add it
#if UNITY_ANDROID
        string platformFolder = "/" + BundleHandler.PLATFORM.Android.ToString() + "/";
#elif UNITY_IOS
        string platformFolder = "/" + BundleHandler.PLATFORM.iOS.ToString() + "/";
#endif
        if (!_url.EndsWith(platformFolder)) _url += platformFolder.Remove(0, 1);
        PlayerPrefs.SetString(STORED_BUNDLE_URL, _url);
        PlayerPrefs.Save();
        AssetBundle.UnloadAllAssetBundles(true);
        if (!_url.Contains("://")) _url = "file:///" + _url;
        _OnFailCb = _onFailCb;
        _OnCompleteCb = _onCompleteCb;
        StartCoroutine(_GetAssetBundles(_url, _delay));
    }
    public void SetProgressValue(float _value) => m_ProgressImg.fillAmount = (_CachedBundles + _value) / _TotalBundles;
    public void SetProgressText(string _content) => m_ProgressTMPUI.text = _content;
    private IEnumerator _GetAssetBundles(string _url, float _delay)
    {
        if (_delay > 0) yield return new WaitForSeconds(_delay);
        using UnityWebRequest aUWR = UnityWebRequest.Get(_url + BundleHandler.CATEGORY); // get new category content from server
        yield return aUWR.SendWebRequest();

        if (aUWR.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("|   ) )=3 Get category fail: " + aUWR.result.ToString() + " / " + aUWR.error + " / Path: " + aUWR.uri);
            _OnFailCb?.Invoke();
        }
        else
        {
            BundleHandler.MAIN.ClearAssetsDictionary();
            string newContent = aUWR.downloadHandler.text, storedPath = Application.persistentDataPath + "/" + BundleHandler.CATEGORY;
            if (!_TryParseCategory(_NewDataBVs, _TryParseJsonArray(newContent), _url))
            {
                Debug.Log("|   ) )=3 Wrong latest bundle info!");
                _OnFailCb?.Invoke();
                yield break;
            }
            if (File.Exists(storedPath))
            {
                if (!_TryParseCategory(_StoredDataBVs, _TryParseJsonArray(File.ReadAllText(storedPath)), ""))
                {
                    Debug.Log("|   ) )=3 Wrong stored bundles info, clear all cached bundles!");
                    Caching.ClearCache();
                    File.Delete(storedPath);
                }
            }
            File.WriteAllText(storedPath, newContent);
            _ClearOldCachedBundleVersions();
            _TotalBundles = _NewDataBVs.Count;
            _CachedBundles = 0;
            if (_TotalBundles > 0)
            {
                _SetProgressUI(0);
                _SentGettingBundleCs.Add(StartCoroutine(_LoadAssetBundles()));
            }
            else _CompleteLoadingAssets();
        }
    }
    private IEnumerator _LoadAssetBundles()
    {
        if (_NewDataBVs.Count > 0)
        {
            BundleVersion thisBV = _NewDataBVs[0];
            while (!Caching.ready) yield return null;
            using UnityWebRequest aUWR = UnityWebRequestAssetBundle.GetAssetBundle(thisBV.Url, thisBV.HashH128, 0);
            thisBV.State = BundleVersion.STATE.Downloading;
            aUWR.SendWebRequest();
            while (!aUWR.isDone)
            {
                _SetProgressUI(aUWR.downloadProgress);
                yield return null;
            }
            if (aUWR.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("|   ) )=3 Error getting asset bundle: " + aUWR.error + " | " + aUWR.url);
                thisBV.State = BundleVersion.STATE.Cloud;
                _OnFailCb?.Invoke();
                foreach (Coroutine sentRequestC in _SentGettingBundleCs) StopCoroutine(sentRequestC);
            }
            else
            {
                thisBV.BundleAB = DownloadHandlerAssetBundle.GetContent(aUWR);
                thisBV.State = BundleVersion.STATE.Downloaded;
                thisBV.AssetNamesHS = thisBV.BundleAB.GetAllAssetNames().ToHashSet();
                BundleHandler.MAIN.AddToLocalMap(thisBV);
                _NewDataBVs.Remove(thisBV);
                _CachedBundles += 1;
                _SentGettingBundleCs.Add(StartCoroutine(_LoadAssetBundles()));
            }
        }
        else _CompleteLoadingAssets();
    }
    private JSONArray _TryParseJsonArray(string _input)
    {
        try { return JSON.Parse(_input).AsArray; }
        catch (Exception e) { Debug.Log("|   ) )=3 Error parsing array: " + e); return null; }
    }
    private bool _TryParseCategory(List<BundleVersion> _storedBVs, JSONArray _categoryJA, string _url)
    {
        try
        {
            _storedBVs.Clear();
            for (int i = 0; i < _categoryJA.Count; i++)
            {
                string[] split = _categoryJA[i].Value.Split(BundleHandler.SPLIT, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length <= 1) continue;
                _storedBVs.Add(new() { Name = split[0], HashH128 = Hash128.Parse(split[1]), Url = _url + _categoryJA[i].Value });
            }
            return true;
        }
        catch (Exception e) { Debug.Log("|   ) )=3 Fail to parse Category content!!! " + e); return false; }
    }
    private void _ClearOldCachedBundleVersions()
    {
        foreach (BundleVersion aBV in _StoredDataBVs)
            if (_NewDataBVs.Find(x => x.Name.Equals(aBV.Name) && x.HashH128 != aBV.HashH128) != null)
                Caching.ClearCachedVersion(aBV.Name, aBV.HashH128);
    }
    private void _SetProgressUI(float _value)
    {
        SetProgressValue(_value);
        // SetProgressText((value >= 1 ? _TotalBundles : _CachedBundles) + "/" + _TotalBundles);
        SetProgressText("Loading " + (_value >= 1 ? "100%" : (m_ProgressImg.fillAmount * 100).ToString("F0") + "%"));
    }
    private void _CompleteLoadingAssets()
    {
        Debug.Log("|   ) )=3 Complete Loading AssetBundles");
        _SetProgressUI(1);
        _OnCompleteCb?.Invoke();
    }
}
