using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScene : MonoBehaviour
{
    [SerializeField] private BundleDownloader m_BundleBD;
    // https://console.cloud.google.com/storage/browser/my-shankoemee/AssetBundles/
    private void Awake()
    {
        Application.targetFrameRate = 60;
        string storedUrl = PlayerPrefs.GetString(BundleDownloader.STORED_BUNDLE_URL, "https://storage.googleapis.com/my-shankoemee/AssetBundles");
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
            while (string.IsNullOrEmpty(BundleHandler.MAIN.BundleUrl))
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
