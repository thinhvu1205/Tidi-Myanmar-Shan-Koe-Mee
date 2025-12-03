using System.Collections;
using Globals;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScene : MonoBehaviour
{
    [SerializeField] private BundleDownloader m_BundleBD;
    //https://console.cloud.google.com/storage/browser/my-shankoemee/AssetBundles/Android?pageState=(%22StorageObjectListTable%22:(%22f%22:%22%255B%255D%22))
    private void Awake()
    {
        Application.targetFrameRate = 60;
        // SceneManager.LoadScene("MainScene");
        // "D:/Unity projects/Tidi-Phil-Win777/Assets/AssetBundles";
        // https://storage.googleapis.com/tongitswar/AssetBundles;
        string storedUrl = PlayerPrefs.GetString(BundleDownloader.STORED_BUNDLE_URL, "https://console.cloud.google.com/storage/browser/my-shankoemee/AssetBundles");
        // storedUrl = "D:/Unity projects/Tidi-Phil-Win777/Assets/AssetBundles";
        m_BundleBD.CheckAndDownloadAssets(storedUrl, 1f,
            () =>
            {
                m_BundleBD.SetProgressText("Retrying ...");
                StartCoroutine(retry());
            },
            () =>
            {
                SceneManager.LoadScene("MainScene");
            });

        IEnumerator retry()
        {
            while (BundleHandler.MAIN.BundleUrl == null || BundleHandler.MAIN.BundleUrl.Equals(""))
                yield return new WaitForSeconds(1f);
            m_BundleBD.CheckAndDownloadAssets(BundleHandler.MAIN.BundleUrl, 0,
                () =>
                {
                    StartCoroutine(retry());
                },
                () =>
                {
                    SceneManager.LoadScene("MainScene");
                });
        }

    }
}
