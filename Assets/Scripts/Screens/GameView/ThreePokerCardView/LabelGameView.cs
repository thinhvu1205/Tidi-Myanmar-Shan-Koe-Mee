using UnityEngine;
using TMPro;  // Thêm không gian tên cho TextMeshPro
using Spine;
using Spine.Unity;
using System.Collections;

public class LabelGameView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;  // TextMeshProUGUI thay cho Unity's Text component
    [SerializeField] private SkeletonGraphic m_AnimPoint;
    public void OnShow(string str, bool isHighlight = false)
    {
        Debug.Log("đã chạy vào đây");
        label.transform.localScale = Vector3.zero;  // Set label scale to 0 (hide it initially)
        m_AnimPoint.gameObject.SetActive(true);
        m_AnimPoint.Initialize(true);
       m_AnimPoint.AnimationState.SetAnimation(0, "point", true);

        // Set the label string if it's not null
        if (str != null)
        {
            label.text = str;
        }

        // Animate label scaling
        label.transform.localScale = Vector3.zero;
        StartCoroutine(ScaleLabel(Vector3.one, 0.5f));
        if (isHighlight)
        {
            // Khi isHighlight là true, đổi màu outline thành rgb(9, 113, 255)
            label.outlineColor = new Color(9f / 255f, 113f / 255f, 255f / 255f); // RGB(9, 113, 255)
        }
        else
        {
            // Khi isHighlight là false, đổi màu outline thành rgb(255, 154, 9)
            label.outlineColor = new Color(255f / 255f, 154f / 255f, 9f / 255f); // RGB(255, 154, 9)
        } // Scale to original size with easing
    }

    // Resolve the label's state based on whether it's a win or loss
    public void OnResolve(bool isWin)
    {
        label.transform.localScale = Vector3.zero;
        StartCoroutine(ScaleLabel(Vector3.one, 0.5f));  // Animate the scaling effect
        if (isWin)
        {
            // Khi isHighlight là true, đổi màu outline thành rgb(9, 113, 255)
            label.outlineColor = new Color(9f / 255f, 113f / 255f, 255f / 255f); // RGB(9, 113, 255)
        }
        else
        {
            // Khi isHighlight là false, đổi màu outline thành rgb(255, 154, 9)
            label.outlineColor = new Color(255f / 255f, 154f / 255f, 9f / 255f); // RGB(255, 154, 9)
        }
    }

    // Hide the label with scaling animation and destroy the object after it's hidden
    public void OnHide()
    {
        StartCoroutine(HideAndDestroy());
    }

    // Coroutine to scale the label with easing
    private IEnumerator ScaleLabel(Vector3 targetScale, float duration)
    {
        Vector3 initialScale = label.transform.localScale;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            label.transform.localScale = Vector3.Lerp(initialScale, targetScale, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        label.transform.localScale = targetScale;
    }

    // Coroutine to hide and destroy the label
    private IEnumerator HideAndDestroy()
    {
        float time = 0.5f;  // Duration of the hide animation
        Vector3 initialScale = label.transform.localScale;
        Vector3 targetScale = Vector3.zero;

        // Animate the scale down
        while (time > 0)
        {
            label.transform.localScale = Vector3.Lerp(initialScale, targetScale, (0.5f - time) / 0.5f);
            time -= Time.deltaTime;
            yield return null;
        }

        label.transform.localScale = targetScale;

        // Destroy the GameObject after hiding
        Destroy(gameObject);
    }
}
