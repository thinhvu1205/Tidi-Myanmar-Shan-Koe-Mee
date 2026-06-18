using System.Collections;
using Globals;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScene : MonoBehaviour
{
    //https://console.cloud.google.com/storage/browser/my-shankoemee
    [SerializeField] private BundleDownloader m_BundleBD;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        string storedUrl = PlayerPrefs.GetString(BundleDownloader.STORED_BUNDLE_URL, "https://storage.googleapis.com/my-shankoemee/AssetBundles");
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
