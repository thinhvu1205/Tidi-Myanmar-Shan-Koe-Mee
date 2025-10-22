using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class PlayerViewTienlen : PlayerView
{
    [SerializeField] private GameObject m_IsBanker;

    [SerializeField] private SkeletonGraphic m_AniWin;
    public Image imageIconPot;
    public void ShowAniWin()
    {
        m_AniWin.gameObject.SetActive(true);
        m_AniWin.timeScale = 2f;
        m_AniWin.AnimationState.SetAnimation(0, "wincam", false);
        Debug.Log("Có chạy vào show ani win");
        StartCoroutine(StopAniWinAfterDelay(2.7f / m_AniWin.timeScale));
    }
    public void SetIsBanker(bool isBanker)
    {
        m_IsBanker.SetActive(isBanker);
    }
    private IEnumerator StopAniWinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        try
        {
            m_AniWin.AnimationState.ClearTrack(0);
            m_AniWin.gameObject.SetActive(false);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PlayerViewTienlen] StopAniWinAfterDelay error: {ex.Message}\n{ex.StackTrace}");
        }
    }

}
