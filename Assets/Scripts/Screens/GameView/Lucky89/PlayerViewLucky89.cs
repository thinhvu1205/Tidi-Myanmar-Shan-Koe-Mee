using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerViewLucky89 : PlayerView
{
    public enum BetInfoPosition { NONE, ABOVE, RIGHT, BELLOW, LEFT, BELLOW_RIGHT, BELLOW_LEFT }
    [SerializeField] private List<Card> m_CardCs;
    [SerializeField] private List<GameObject> m_Rates;
    [SerializeField] private SkeletonGraphic m_LuckySG, m_WinSg, m_LoseSg, m_DrawSG;
    [SerializeField] private TextMeshProUGUI m_BetTMP, m_ScoreTMP, m_GameRemainTMP;
    [SerializeField] private Image imageIconBanker;
    [SerializeField] private GameObject CardParent, scoreParent, rateParent;
    [SerializeField] private SkeletonGraphic animWaitBetTime, animWaitOpenCard;
    private BetInfoPosition _BetInfoBIP = BetInfoPosition.ABOVE;
    private int _BetValue;
    public bool isBanker, isLucky;
    public void ShowAnimWaitBetTime(bool isShow, string nameAnim = "White")
    {
        if (isShow)
        {
            animWaitBetTime.Initialize(true);
            animWaitBetTime.AnimationState.SetAnimation(0, nameAnim, true);
            animWaitBetTime.gameObject.SetActive(true);
        }
        else
        {
            animWaitBetTime.gameObject.SetActive(false);
        }
    }
    public void ShowAnimWaitOpenCard(bool isShow)
    {
        if (isBanker) return;
        animWaitBetTime.Initialize(true);
        animWaitOpenCard.gameObject.SetActive(isShow);
    }
    public void ShowIconBanker(bool isShow, int gameRemain = 0)
    {
        imageIconBanker.gameObject.SetActive(isShow);
        if (isShow)
        {
            animWaitBetTime.gameObject.SetActive(false);
            if (gameRemain > 0)
            {
                m_GameRemainTMP.gameObject.SetActive(true);
                m_GameRemainTMP.text = $"{gameRemain}";
            }
        }
    }

    public PlayerViewLucky89 ShowAnimResult(bool show, long changedChips)
    {
        if (!isBanker)
        {
            bool isWin = changedChips > 0, isDraw = changedChips == 0, isLose = changedChips < 0;
            m_WinSg.gameObject.SetActive(show && isWin);
            m_DrawSG.gameObject.SetActive(show && isDraw);
            m_LoseSg.gameObject.SetActive(show && isLose);
            if (!show) return this;
            if (isWin) m_WinSg.AnimationState.SetAnimation(0, "win", false);
            if (isDraw) m_DrawSG.AnimationState.SetAnimation(0, "eng", false);
            if (isLose) m_LoseSg.AnimationState.SetAnimation(0, "lose", false);
        }
        return this;
    }
    public PlayerViewLucky89 HideAllCards()
    {
        foreach (Card card in m_CardCs) card.gameObject.SetActive(false);
        return this;
    }
    public PlayerViewLucky89 UpdateCardsParentPositionAndRotation()
    {
        Card thirdCard = m_CardCs.Last();
        bool isShowAll = thirdCard.gameObject.activeSelf;
        RectTransform cardsParentRT = thirdCard.transform.parent.GetComponent<RectTransform>();
        float tweenDuration = .2f;
        // cardsParentRT.DOLocalMoveX(isShowAll ? 0 : 10, tweenDuration);
        cardsParentRT.DOLocalRotate(new Vector3(0, 0, isShowAll ? 0 : -15), tweenDuration);
        return this;
    }
    public List<Card> GetListCards()
    {
        return m_CardCs.Where(c => c.gameObject.activeSelf).ToList();
    }

    public Card GetACard()
    {
        foreach (Card cardC in m_CardCs) if (!cardC.gameObject.activeSelf) return cardC;
        return null;
    }
    public PlayerViewLucky89 ShowScore(bool show, int score, int cardCount)
    {
        if (m_LuckySG == null) Debug.LogError(">>> m_LuckySG NULL");
        if (m_ScoreTMP == null) Debug.LogError(">>> m_ScoreTMP NULL");
        if (m_ScoreTMP != null && m_ScoreTMP.transform.parent == null) Debug.LogError(">>> ScoreTMP parent NULL");
        bool isLucky = (cardCount == 2) && (score >= 8);
        bool isLucky8 = score >= 8;
        bool isLucky9 = score >= 9;
        m_LuckySG.gameObject.SetActive(show && isLucky);
        if (m_ScoreTMP != null && m_ScoreTMP.transform?.parent != null)
            m_ScoreTMP.transform.parent.gameObject.SetActive(show && !isLucky);
        else Debug.LogError("PlayerViewLucky89: m_ScoreTMP hoặc parent NULL");
        if (!show) return this;
        if (isLucky9)
            m_LuckySG.AnimationState.SetAnimation(0, "lucky9", false);
        else if (isLucky8) m_LuckySG.AnimationState.SetAnimation(0, "lucky8", false);
        else if (score >= (int)Lucky89View.SCORE.FACE_CARDS) m_ScoreTMP.text = "Face cards";
        else if (score >= (int)Lucky89View.SCORE.STRAIGHT_FLUSH) m_ScoreTMP.text = "Straight flush";
        else if (score >= (int)Lucky89View.SCORE.FLUSH) m_ScoreTMP.text = "Flush";
        else m_ScoreTMP.text = score + " points";
        return this;
    }

    public PlayerViewLucky89 ShowRate(int rate)
    {
        if (m_Rates == null || m_Rates.Count == 0)
        {
            Debug.LogWarning("ShowRate: m_Rates is null or empty");
            return this;
        }
        if (rate < 2)
        {
            foreach (GameObject go in m_Rates)
            {
                if (go != null)
                    go.SetActive(false);
            }
            return this;
        }
        int index = rate - 2;
        if (index < 0 || index >= m_Rates.Count)
        {
            Debug.LogWarning($"ShowRate: invalid rate={rate}, index={index}, m_Rates.Count={m_Rates.Count}");
            foreach (GameObject go in m_Rates)
            {
                if (go != null)
                    go.SetActive(false);
            }
            return this;
        }
        for (int i = 0; i < m_Rates.Count; i++)
        {
            GameObject go = m_Rates[i];
            if (go == null) continue;
            go.SetActive(i == index);
        }

        return this;
    }

    public void SetCardPosition(int idPlayerview)
    {
        switch (idPlayerview)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                CardParent.transform.localPosition = new Vector2(140, -36);
                animWaitOpenCard.transform.localPosition = new Vector2(140, -36);
                scoreParent.transform.localPosition = new Vector2(140, -80);
                m_LuckySG.rectTransform.anchoredPosition = new Vector2(140, -128);
                rateParent.transform.localPosition = new Vector2(144, -30);
                break;
            case 4:
            case 5:
            case 6:
                CardParent.transform.localPosition = new Vector2(-140, -36);
                animWaitOpenCard.transform.localPosition = new Vector2(-140, -36);
                scoreParent.transform.localPosition = new Vector2(-140, -80);
                m_LuckySG.rectTransform.anchoredPosition = new Vector2(-140, -128);
                rateParent.transform.localPosition = new Vector2(-138, -30);
                break;
        }
    }
    public void SetIconBankerPosition(int idPlayerview)
    {
        switch (idPlayerview)
        {
            case 0:
                imageIconBanker.rectTransform.anchoredPosition = new Vector2(144, 60);
                break;
            case 1:
            case 2:
                imageIconBanker.rectTransform.anchoredPosition = new Vector2(220, -52);
                break;
            case 3:
                imageIconBanker.rectTransform.anchoredPosition = new Vector2(158, -124);
                break;
            case 4:
                imageIconBanker.rectTransform.anchoredPosition = new Vector2(-158, -124);
                break;
            case 5:
            case 6:
                imageIconBanker.rectTransform.anchoredPosition = new Vector2(-220, -52);
                break;
        }
    }
    public PlayerViewLucky89 SetBetPosition(int idPlayerview)
    {
        switch (idPlayerview)
        {
            case 0:
                _BetInfoBIP = BetInfoPosition.ABOVE;
                break;
            case 1:
            case 2:
                _BetInfoBIP = BetInfoPosition.RIGHT;
                break;
            case 3:
                _BetInfoBIP = BetInfoPosition.BELLOW_RIGHT;
                break;
            case 4:
                _BetInfoBIP = BetInfoPosition.BELLOW_LEFT;
                break;
            case 5:
            case 6:
                _BetInfoBIP = BetInfoPosition.LEFT;
                break;
        }
        return this;
    }
    public PlayerViewLucky89 ShowHideBetChips(bool show, int betValue = 0)
    {
        _BetValue = betValue;
        RectTransform rt = m_BetTMP.transform.parent.GetComponent<RectTransform>();
        rt.gameObject.SetActive(show);
        if (show)
        {
            animWaitBetTime.gameObject.SetActive(false);
        }
        StartCoroutine(DelaySetBetPosition(rt));
        if (!show)
            return this;

        if (_BetValue > 0)
            m_BetTMP.text = Config.FormatMoney(_BetValue, true).ToString();

        return this;
    }

    private IEnumerator DelaySetBetPosition(RectTransform rt)
    {
        yield return null; // chờ 1 frame
        switch (_BetInfoBIP)
        {
            case BetInfoPosition.ABOVE:
                rt.anchoredPosition = new Vector2(144, 60);
                animWaitBetTime.rectTransform.anchoredPosition = new Vector2(144, 60);
                break;
            case BetInfoPosition.RIGHT:
                rt.anchoredPosition = new Vector2(272, -52);
                animWaitBetTime.rectTransform.anchoredPosition = new Vector2(272, -52);
                break;
            case BetInfoPosition.BELLOW:
                rt.anchoredPosition = new Vector2(5, -130);
                animWaitBetTime.rectTransform.anchoredPosition = new Vector2(5, -130);
                break;
            case BetInfoPosition.LEFT:
                rt.anchoredPosition = new Vector2(-272, -52);
                animWaitBetTime.rectTransform.anchoredPosition = new Vector2(-272, -52);
                break;
            case BetInfoPosition.BELLOW_LEFT:
                rt.anchoredPosition = new Vector2(-158, -124);
                animWaitBetTime.rectTransform.anchoredPosition = new Vector2(-158, -124);
                break;
            case BetInfoPosition.BELLOW_RIGHT:
                rt.anchoredPosition = new Vector2(158, -124);
                animWaitBetTime.rectTransform.anchoredPosition = new Vector2(158, -124);
                break;
            default:
                rt.anchoredPosition = Vector2.zero;
                break;
        }
    }

    public int GetBetValue() { return _BetValue; }
}
