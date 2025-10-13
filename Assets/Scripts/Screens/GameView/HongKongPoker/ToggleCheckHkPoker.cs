using System.Collections;
using System.Collections.Generic;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ToggleCheckHkPoker : MonoBehaviour
{
    [SerializeField]
    Button toggleShow;

    [SerializeField]
    List<Toggle> listToggle;

    [SerializeField]
    TextMeshProUGUI textToggleFold;

    [SerializeField]
    TextMeshProUGUI textToggleCall;

    [SerializeField]
    TextMeshProUGUI textToggleCallAny;

    public void setInfo(int chipForCall, int chipBoxBet = 0, long chipPlayer = 0)
    {
        gameObject.SetActive(chipPlayer > 0);
        int temp = chipBoxBet;
        if (chipForCall <= 0 || chipForCall == temp)
        {
            textToggleFold.text = Globals.Config.getTextConfig("show_lb_fold_check");
            textToggleCall.text = Globals.Config.getTextConfig("show_lb_check");
        }
        else
        {
            textToggleFold.text = Globals.Config.getTextConfig("show_lb_fold");
            textToggleCall.text = Globals.Config.formatStr("%s(%s)", Globals.Config.getTextConfig("show_lb_call"), Globals.Config.FormatMoney(chipForCall - temp));
        }
        ;
        textToggleCallAny.text = Globals.Config.getTextConfig("show_lb_call_any").Replace('\n', ' ');
        if (chipForCall >= chipPlayer + temp)
        {
            listToggle[1].gameObject.SetActive(false);
            textToggleCallAny.text = Globals.Config.getTextConfig("show_lb_allin");
        }
        else
        {
            listToggle[1].gameObject.SetActive(true);
        }
    }

    public bool readInfoToggle()
    {
        for (int i = 0; i < listToggle.Count; i++)
        {
            if (listToggle[i].isOn)
            {
                if (i == 0)
                {
                    if (textToggleFold.text == Config.getTextConfig("show_lb_fold"))
                    {
                        SocketSend.sendMakeBetShow("pFold");
                    }
                    else
                    {
                        SocketSend.sendMakeBetShow("pCheck");
                    }
                }
                else
                {
                    SocketSend.sendMakeBetShow("pCall");
                }

                listToggle[i].isOn = false;
                gameObject.SetActive(false);
                return true;
            }
        }
        gameObject.SetActive(false);
        return false;
    }

    public void onClickToggleShow()
    {
        SocketSend.sendMakeBetShow("show");
        toggleShow.gameObject.SetActive(false);
        SoundManager.instance.soundClick();
    }

    public void onClickToggleFold()
    {
        SoundManager.instance.soundClick();
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickToggleFold_%s", CURRENT_VIEW.getCurrentSceneName()));
    }

    public void onClickToggleCall()
    {
        SoundManager.instance.soundClick();
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickToggleCall%s", CURRENT_VIEW.getCurrentSceneName()));
    }

    public void onClickToggleCallAny()
    {
        SoundManager.instance.soundClick();
        SocketIOManager.getInstance().emitSIOCCCNew(Config.formatStr("ClickToggleCallAny%s", CURRENT_VIEW.getCurrentSceneName()));
    }

}
