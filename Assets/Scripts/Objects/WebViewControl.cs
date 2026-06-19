using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Gpm.WebView;


public class WebViewControl : MonoBehaviour
{
  public static WebViewControl instance = null;

  public string Url;
  public Text status;
  [SerializeField] public TextMeshProUGUI lbTitle;
  [SerializeField] public GameObject titleBar;
  [SerializeField] public RectTransform Rect;

  private void Awake()
  {
  }
  private void Start()
  {
    WebViewControl.instance = this;
  }
  public void OpenWebPage(string URL)
  {
    GpmWebView.ShowUrl(
        URL,
        new GpmWebViewRequest.Configuration()
        {
          style = GpmWebViewStyle.POPUP,
          orientation = GpmOrientation.UNSPECIFIED,
          isClearCookie = true,
          isClearCache = true,
          isNavigationBarVisible = true,
          isCloseButtonVisible = true,
          margins = new GpmWebViewRequest.Margins
          {
            hasValue = true,
            left = 0,
            top = 200,
            right = 0,
            bottom = 0
          },
          supportMultipleWindows = true,
#if UNITY_IOS
          contentMode = GpmWebViewContentMode.MOBILE,
          isMaskViewVisible = true,
#endif
        },
        OnCallback,
        new List<string>()
        {
              "USER_ CUSTOM_SCHEME"
        });
  }
  private void OnCallback(GpmWebViewCallback.CallbackType callbackType, string data, GpmWebViewError error)
  {
    Debug.Log("OnCallback: " + callbackType);
    switch (callbackType)
    {
      case GpmWebViewCallback.CallbackType.Open:
        if (error != null)
        {
          Debug.LogFormat("Fail to open WebView. Error:{0}", error);
        }
        break;
      case GpmWebViewCallback.CallbackType.Close:
        if (error != null)
        {
          Debug.LogFormat("Fail to close WebView. Error:{0}", error);
        }
        break;
      case GpmWebViewCallback.CallbackType.PageStarted:
        if (string.IsNullOrEmpty(data) == false)
        {
          Debug.LogFormat("PageStarted Url : {0}", data);
        }
        break;
      case GpmWebViewCallback.CallbackType.PageLoad:
        if (string.IsNullOrEmpty(data) == false)
        {
          Debug.LogFormat("Loaded Page:{0}", data);
        }
        break;
      case GpmWebViewCallback.CallbackType.MultiWindowOpen:
        Debug.Log("MultiWindowOpen");
        break;
      case GpmWebViewCallback.CallbackType.MultiWindowClose:
        Debug.Log("MultiWindowClose");
        break;
      case GpmWebViewCallback.CallbackType.Scheme:
        if (error == null)
        {
          if (data.Equals("USER_ CUSTOM_SCHEME") == true || data.Contains("CUSTOM_SCHEME") == true)
          {
            Debug.Log(string.Format("scheme:{0}", data));
          }
        }
        else
        {
          Debug.Log(string.Format("Fail to custom scheme. Error:{0}", error));
        }
        break;
      case GpmWebViewCallback.CallbackType.GoBack:
        Debug.Log("GoBack");
        break;
      case GpmWebViewCallback.CallbackType.GoForward:
        Debug.Log("GoForward");
        break;
      case GpmWebViewCallback.CallbackType.ExecuteJavascript:
        Debug.LogFormat("ExecuteJavascript data : {0}, error : {1}", data, error);
        break;
#if UNITY_ANDROID
        case GpmWebViewCallback.CallbackType.BackButtonClose:
          Debug.Log("BackButtonClose");
          break;
#endif
    }
  }
  public void loadUrl(string urlWeb, string title = "")
  {
    Url = urlWeb;
    if (title != "")
    {
      lbTitle.text = title;
      lbTitle.gameObject.SetActive(true);
    }
    StartCoroutine(TaiURL());
  }
  public IEnumerator TaiURL()
  {
    yield return null;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        // webViewObject.bitmapRefreshCycle = 1;
#endif
    if (UIManager.instance.gameView != null)
    {
      if (UIManager.instance.gameView.transform.eulerAngles.z == 0)
      {
      }
      else
      {
        Globals.Logging.Log("Webview:Screen.currentResolution1:" + Screen.currentResolution);
        UIManager.instance.changeOrientation(ScreenOrientation.LandscapeLeft);
        Globals.Logging.Log("Webview:Screen.currentResolution2:" + Screen.currentResolution);
        Globals.Logging.Log("Webview:safe area= " + Screen.safeArea);
      }
    }
    else
    {
    }


#if !UNITY_WEBPLAYER && !UNITY_WEBGL
    if (Url.StartsWith("http"))
    {
    }
    else
    {
      var exts = new string[]{
                ".jpg",
                ".js",
                ".html"  // should be last
            };
      foreach (var ext in exts)
      {
        var url = Url.Replace(".html", ext);
        var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
        var dst = System.IO.Path.Combine(Application.persistentDataPath, url);
        byte[] result = null;
        if (src.Contains("://"))
        {  // for Android
#if UNITY_2018_4_OR_NEWER
          // NOTE: a more complete code that utilizes UnityWebRequest can be found in https://github.com/gree/unity-webview/commit/2a07e82f760a8495aa3a77a23453f384869caba7#diff-4379160fa4c2a287f414c07eb10ee36d
          var unityWebRequest = UnityWebRequest.Get(src);
          yield return unityWebRequest.SendWebRequest();
          result = unityWebRequest.downloadHandler.data;
#else
                    var www = new WWW(src);
                    yield return www;
                    result = www.bytes;
#endif
        }
        else
        {
          result = System.IO.File.ReadAllBytes(src);
        }
        System.IO.File.WriteAllBytes(dst, result);
        if (ext == ".html")
        {
          break;
        }
      }
    }
#else
    if (Url.StartsWith("http"))
    {
      // webViewObject.LoadURL(Url.Replace(" ", "%20"));
    }
    else
    {
      // webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
    }
#endif
  }

  private void showWebView()
  {
    return;
    //if (UIManager.instance.gameView != null)
    //{
    //    if (UIManager.instance.gameView.transform.eulerAngles.z == 0)
    //    {
    //        webViewObject.SetMargins(0, Screen.currentResolution.height- (int)Screen.safeArea.height + (int)titleBar.GetComponent<RectTransform>().sizeDelta.y, 0, 0);
    //    }
    //    else
    //    {
    //        Globals.Logging.Log("Screen.currentResolution1:" + Screen.currentResolution);
    //        Globals.Logging.Log("Webview size1=" + GetComponent<RectTransform>().rect);
    //        UIManager.instance.changeOrientation(ScreenOrientation.LandscapeLeft);
    //        float tile = (float)Screen.currentResolution.width / 720;
    //        Globals.Logging.Log("Screen.currentResolution2:" + Screen.currentResolution);
    //        Globals.Logging.Log("safe area= " + Screen.safeArea);
    //        webViewObject.SetMargins(0, (int)titleBar.GetComponent<RectTransform>().sizeDelta.y, 0, 0);
    //        Globals.Logging.Log("Webview size2=" + GetComponent<RectTransform>().rect);
    //    }
    //}
    //else
    //{
    //    webViewObject.SetMargins(0, Screen.currentResolution.height - (int)Screen.safeArea.height + (int)titleBar.GetComponent<RectTransform>().sizeDelta.y, 0, 0);
    //}
  }
  public void closeWebView()
  {
    SocketSend.sendUAG();
    Destroy(gameObject);
    //if (Screen.orientation != ScreenOrientation.Portrait)
    //{
    //    UIManager.instance.changeOrientation(ScreenOrientation.Portrait);
    //}
  }
  private void OnDestroy()
  {
    SocketSend.sendUAG();
    var g = GameObject.Find("WebViewObject");
    if (g != null)
    {
      Destroy(g);
    }
    //if (Screen.orientation != ScreenOrientation.Portrait)
    //    UIManager.instance.changeOrientation(ScreenOrientation.Portrait);
  }

}