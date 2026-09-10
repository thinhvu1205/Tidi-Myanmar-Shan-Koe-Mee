using System.Collections;
using System.IO;
using UnityEngine;

public class ScreenshotCapturer : MonoBehaviour
{   // must use with Canvas ScreenSpace - Camera
    private Coroutine _photoCoroutine;

    [ContextMenu("Take Screenshot")]
    private void _TakeScreenShot()
    {
        ScreenCapture.CaptureScreenshot(_GetFullPathName());
    }
    [ContextMenu("Take Screenshot Without UI")]
    private void _TakeScreenshotWithoutUI()
    {
        if (_photoCoroutine != null) StopCoroutine(_photoCoroutine);
        _photoCoroutine = StartCoroutine(takeScreenshotWithoutUI());
        //////////////////////////////////////////////////////////////////////////////
        IEnumerator takeScreenshotWithoutUI()
        {
            int oldCullingMask = Camera.main.cullingMask;
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("UI")); // hide specific layer named UI for taking screenshot
            ScreenCapture.CaptureScreenshot(_GetFullPathName());

            yield return new WaitForEndOfFrame();
            Camera.main.cullingMask = oldCullingMask; // restore the original one, show all layers that was hidden
        }
    }
    [ContextMenu("Get stored location")]
    public void _GetStoredLocation()
    {
        Debug.Log("The screenshots are stored in: " + Application.persistentDataPath);
    }
    private string _GetFullPathName()
    {
        string screenshotName = "Screenshot_" + System.DateTime.Now.ToString("HH-mm-ss_ddMMyyyy") + ".png";
        return Path.Combine(Application.persistentDataPath, screenshotName);
    }
}
