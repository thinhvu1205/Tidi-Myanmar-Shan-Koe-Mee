using System;
using System.Collections;
using System.Collections.Generic;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BtnBetContainer : MonoBehaviour
{
    [SerializeField]
    GameObject btn_Fold;

    [SerializeField]
    TextMeshProUGUI textBtn_Fold;

    [SerializeField]
    TextMeshProUGUI textCurrentChip;

    [SerializeField]
    GameObject btn_Raise;

    [SerializeField]
    TextMeshProUGUI textBtn_Raise;

    [SerializeField]
    GameObject btn_Call;

    [SerializeField]
    TextMeshProUGUI textBtn_Call;

    [SerializeField]
    TextMeshProUGUI textBet;

    [SerializeField]
    Image thanhKeo;

    [SerializeField]
    GameObject btn_Allin_Call;

    [SerializeField]
    List<Button> listButton;

    [SerializeField]
    Slider handleSlider;

    [SerializeField] private GameObject lb_max;


    private float valueBet = 0, valueTableAg = 500, valuePot = 0;
    private float valueThisPlayer = 500f;
    private float timeCountDow = 0;
    private bool isClickRaise = false, isCountDow = false;
    const double EPSILON = 1e-4;

    void Start()
    {
        handleSlider.onValueChanged.AddListener((vl) =>
        {
            CallBackSilder();
        });
    }

    private void Update()
    {
        if (isCountDow)
        {
            timeCountDow -= Time.deltaTime;
            if (timeCountDow < 0.5 && isClickRaise)
            {
                btn_Raise.gameObject.SetActive(true);
                SocketSend.sendMakeBetShow("pRaise", (int)valueTableAg);
                isCountDow = false;
                isClickRaise = false;
                gameObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        handleSlider.transform.parent.gameObject.SetActive(false);

    }

    public void update_slider(long value)
    {
        valueThisPlayer = value;
    }

    public void CallBackSilder()
    {
        float valueMoney = 0;
        float _progress = handleSlider.value;

        if (_progress <= 0.7f)
        {

            valueMoney = Mathf.FloorToInt((_progress * valueThisPlayer / 2) * (1 / 0.7f));
            if (_progress <= valueTableAg / valueThisPlayer / 0.7)
            {
                handleSlider.value = valueTableAg / valueThisPlayer / 0.7f;
            }
            Globals.Logging.Log("_progress <= 0.7f valueMoney:" + valueMoney);
        }
        else
        {
            valueMoney = Mathf.FloorToInt(valueThisPlayer / 2 + ((_progress - 0.7f) * valueThisPlayer / 2 * (1 / 0.3f)));
            if (_progress >= 0.99)
            {
                handleSlider.value = 1;
            }
        }

        lb_max.SetActive(Mathf.Approximately(handleSlider.value, 1));
        thanhKeo.fillAmount = handleSlider.value;
        if (valueMoney <= valueTableAg)
        {
            valueMoney = valueTableAg;
            textBet.text = Globals.Config.FormatMoney((int)valueTableAg);
        }
        else if (valueMoney < valueThisPlayer)
        {
            textBet.text = Globals.Config.FormatMoney((int)valueMoney);
        }
        else
        {
            textBet.text = Globals.Config.FormatMoney((int)Mathf.Floor(valueThisPlayer));
            valueMoney = valueThisPlayer;
        }
        valueBet = valueMoney;

    }

    public void setValueInfo(long valueAgPlayer, int valueTableAgg, int valuePott)
    {
        valueThisPlayer = valueAgPlayer;
        valueTableAg = valueTableAgg;
        valuePot = valuePott;
        valueBet = valueTableAg;
        textBet.text = Globals.Config.FormatMoney((int)valueTableAg);
        if (valueAgPlayer <= 0)
        {
            btn_Allin_Call.gameObject.SetActive(true);
            btn_Raise.gameObject.SetActive(false);
            btn_Call.gameObject.SetActive(false);
        }
        else
        {
            btn_Raise.gameObject.SetActive(true);
            btn_Call.gameObject.SetActive(true);
            btn_Allin_Call.gameObject.SetActive(false);
        }

    }

    public void setInfoBtn(string btn_1, string btn_2, string btn_3, int amount = 0)
    {
        if (btn_3 == "Bet")
        {
            textBtn_Raise.text = Globals.Config.getTextConfig("show_lb_bet");
        }
        else
        {
            textBtn_Raise.text = Globals.Config.getTextConfig("show_lb_raise");
        }

        if (btn_2 == "Call")
        {
            if (amount > 0)
            {
                string str = Globals.Config.FormatMoney(amount);
                textBtn_Call.text = Globals.Config.getTextConfig("show_lb_call") + "(" + str + ")";
            }
            else
            {
                textBtn_Call.text = Globals.Config.getTextConfig("show_lb_call");
            }
        }
        else
        {
            textBtn_Raise.text = Globals.Config.getTextConfig("show_lb_check");
        }

    }

    public void onClickRaise()
    {
        btn_Raise.gameObject.SetActive(false);
        SoundManager.instance.soundClick();
        isClickRaise = true;
        handleSlider.transform.parent.gameObject.SetActive(true);
        resetSlider();
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickBet_%s", CURRENT_VIEW.getCurrentSceneName()));
    }

    public void offRaise()
    {
        btn_Raise.gameObject.SetActive(true);
        isClickRaise = false;
        handleSlider.transform.parent.gameObject.SetActive(false);
        resetSlider();
    }

    public void onClickComfirm()
    {
        SoundManager.instance.soundClick();
        SocketSend.sendMakeBetShow(Math.Abs(valueBet - valueThisPlayer) < EPSILON ? "pAllin" : "pRaise", (int)valueBet);
        btn_Raise.gameObject.SetActive(true);
        gameObject.SetActive(false);
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickConfirm_%s", CURRENT_VIEW.getCurrentSceneName()));
    }

    public void onClickFold()
    {
        SoundManager.instance.soundClick();
        SocketSend.sendMakeBetShow("pFold");
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickFold_%s", CURRENT_VIEW.getCurrentSceneName()));
        gameObject.SetActive(false);
    }

    public void onClickCall()
    {
        SoundManager.instance.soundClick();
        if (textBtn_Call.text == Globals.Config.getTextConfig("show_lb_check"))
        {// GameManager.getInstance().getTextConfig("show_lb_check")) {
            SocketSend.sendMakeBetShow("pCheck");
            SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickCheck_%s", CURRENT_VIEW.getCurrentSceneName()));
        }
        else
        {
            SocketSend.sendMakeBetShow("pCall");
            SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickCall_%s", CURRENT_VIEW.getCurrentSceneName()));

        }
        gameObject.SetActive(false);
    }

    public void onClickBtnAllIn()
    {
        SoundManager.instance.soundClick();
        btn_Raise.gameObject.SetActive(true);
        SocketSend.sendMakeBetShow("pAllin");
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickBetAllin_%s", CURRENT_VIEW.getCurrentSceneName()));
        gameObject.SetActive(false);
    }

    private void resetSlider()
    {
        handleSlider.value = valueTableAg / valueThisPlayer / 0.7f >= 1 ? 1 : valueTableAg / valueThisPlayer / 0.7f;
        thanhKeo.fillAmount = handleSlider.value;
        textBet.text = Config.FormatMoney((int)valueTableAg);
    }

    public void onClickBtnAllinForCall()
    {
        SoundManager.instance.soundClick();
        SocketSend.sendMakeBetShow("pCall");
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickCall_%s", CURRENT_VIEW.getCurrentSceneName()));
        gameObject.SetActive(false);
    }

    public void AutoBetIfClickRaise(int time)
    {
        timeCountDow = time;
        isCountDow = true;
    }

    public void SetFalseIsCountDown()
    {
        timeCountDow = 0;
        isCountDow = false;
        isClickRaise = false;
    }

    public void On1per2Click()
    {
        SoundManager.instance.soundClick();
        float cBet = (valuePot / 2.0f);

        if (cBet < valueTableAg)
        {
            cBet = valueTableAg;
        }
        btn_Raise.gameObject.SetActive(true);
        if (valueThisPlayer > cBet)
        {
            SocketSend.sendMakeBetShow("pRaise", Mathf.FloorToInt(cBet));
        }
        else
        {
            SocketSend.sendMakeBetShow("pAllin");
        }
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickBet1Per2_%s", CURRENT_VIEW.getCurrentSceneName()));
        gameObject.SetActive(false);
    }

    public void On1per4Click()
    {
        SoundManager.instance.soundClick();
        var cBet = (valuePot / 4.0f);

        if (cBet < valueTableAg)
        {
            cBet = valueTableAg;
        }
        btn_Raise.gameObject.SetActive(true);
        if (valueThisPlayer > cBet)
        {
            SocketSend.sendMakeBetShow("pRaise", Mathf.FloorToInt(cBet));
        }
        else
        {
            SocketSend.sendMakeBetShow("pAllin");
        }
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickBet1Per4_%s", CURRENT_VIEW.getCurrentSceneName()));
        gameObject.SetActive(false);
    }

    public void On1per8Click()
    {
        SoundManager.instance.soundClick();
        var cBet = (valuePot / 8.0f);

        if (cBet < valueTableAg)
        {
            cBet = valueTableAg;
        }
        btn_Raise.gameObject.SetActive(true);
        if (valueThisPlayer > cBet)
        {
            SocketSend.sendMakeBetShow("pRaise", Mathf.FloorToInt(cBet));
        }
        else
        {
            SocketSend.sendMakeBetShow("pAllin");
        }
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickBet1Per8_%s", CURRENT_VIEW.getCurrentSceneName()));
        gameObject.SetActive(false);
    }

}
