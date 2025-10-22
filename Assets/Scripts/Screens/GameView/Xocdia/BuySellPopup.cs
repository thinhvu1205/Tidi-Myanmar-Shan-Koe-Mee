using Globals;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuySellPopup : MonoBehaviour
{
    public TextMeshProUGUI lbTotalMoneyPot;
    public TextMeshProUGUI lbNamePot;
    public Image bgHandleOn;
    public Slider slider;

    public int totalSell = 0;
    public float currentSell = 0f;

    // Giả định GameManager có singleton instance và các hàm tương tự
    private void Awake()
    {

    }

    public void setInfo(JObject data, int agPlayer = int.MaxValue) // hoặc nullable int? agPlayer = null rồi xử lý tùy chọn
    {
        Debug.Log("xem cái data chỗ sell"+ data.ToString());
        // data = { evt, uid, Num, M, totalSell }
        totalSell = (int)data["totalSell"];
        if (totalSell <= 0)
        {
            this.gameObject.SetActive(false);
        }
        // else
        // {
        //     this.gameObject.SetActive(true);
        // }
        if (agPlayer < totalSell)
            totalSell = agPlayer;

        currentSell = totalSell;
        slider.value = 1f;
        bgHandleOn.fillAmount = 1f;
        lbTotalMoneyPot.text = Globals.Config.FormatMoney(totalSell);

        string numStr = (string)data["Num"];
        switch (numStr)
        {
            case "1":
                lbNamePot.text = Config.getTextConfig("select_quantity").Replace("XX", "EVEN");
                break;
            case "2":
                lbNamePot.text = Config.getTextConfig("select_quantity").Replace("XX", "ODD");
                break;
        }
    }


    public void handleSlider()
    {
        currentSell = totalSell * slider.value;
        lbTotalMoneyPot.text = Globals.Config.FormatMoney((int)currentSell);
        bgHandleOn.fillAmount = slider.value;
    }

    public void onClickExit()
    {
        gameObject.SetActive(false);
    }

    public void onClickConfirm()
    {
        gameObject.SetActive(false);
        SocketSend.sendBuyBet((int)currentSell);
    }
}
