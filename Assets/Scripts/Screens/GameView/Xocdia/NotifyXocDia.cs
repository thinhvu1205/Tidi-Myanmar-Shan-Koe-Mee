using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class NotifyXocDia : MonoBehaviour
{
    public TextMeshProUGUI lbNamePot;             // Tương đương cc.Label
    public int typePot = 1;
    private GameObject nodeSellBet;    // nodeActive từ Cocos

    private void Start()
    {
        // Không cần onLoad như Cocos
    }

    public void setDataSellBet(int typePot, GameObject nodeActive)
    {
        this.typePot = typePot;
        this.nodeSellBet = nodeActive;
        //   this.nodeSellBet.SetActive(false);

        string textTemplate = Globals.Config.getTextConfig("want_to_sell"); // như require('GameManager').getInstance()
        if (this.typePot == 1)
        {
            lbNamePot.text = textTemplate.Replace("XX", "EVEN");
        }
        else if (this.typePot == 2)
        {
            lbNamePot.text = textTemplate.Replace("XX", "ODD");
        }
    }

    public void onPopOn()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.one * 0.8f;
        CanvasGroup canvas = GetComponent<CanvasGroup>();
        if (canvas == null) canvas = gameObject.AddComponent<CanvasGroup>();
        canvas.alpha = 200f / 255f;

        transform.DOScale(1.0f, 0.1f).SetEase(Ease.OutBack);
        canvas.DOFade(1f, 0.1f);
    }

    public void onPopOff()
    {
        CanvasGroup canvas = GetComponent<CanvasGroup>();
        if (canvas == null) canvas = gameObject.AddComponent<CanvasGroup>();

        transform.DOScale(0.8f, 0.1f).SetEase(Ease.InBack);
        canvas.DOFade(0f, 0.1f).SetEase(Ease.InCirc).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void onClickSendSellBet()
    {
         this.nodeSellBet.SetActive(false);
        SocketSend.sendSellBet(typePot);
        onPopOff();
    }

    public void onClosePopUp()
    {
        onPopOff();
        if (nodeSellBet != null)
        {
            nodeSellBet.SetActive(true);
        }
    }
}
