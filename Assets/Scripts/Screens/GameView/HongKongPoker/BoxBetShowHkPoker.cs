using System.Collections;
using System.Collections.Generic;
using Globals;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoxBetShowHkPoker : MonoBehaviour
{
    [SerializeField]
    Image spIcStatus;

    [SerializeField]
    List<Sprite> listSprite;

    [SerializeField]
    TextMeshProUGUI lbChip;

    public int chip = 0, theFirst = 0;
    public string status = "";

    public void setInfo(string statusStr, int indexDynamic, int chipBet = 0)
    {
        chip = chipBet;
        transform.localScale = indexDynamic <= 4 ? Vector2.one : Vector2.one * -1;
        lbChip.transform.localScale = indexDynamic <= 4 ? Vector2.one : Vector2.one * -1;
        spIcStatus.transform.localScale = indexDynamic <= 4 ? Vector2.one : Vector2.one * -1;
        status = statusStr;
        if (chip == 0)
        {
            GetComponent<Image>().enabled = false;
            lbChip.text = "";
        }
        else
        {
            if (!GetComponent<Image>().enabled)
            {
                GetComponent<Image>().enabled = true;
            }
            lbChip.text = Config.FormatMoney2(chipBet, true);
        }
        switch (status)
        {
            case "Allin":
                spIcStatus.sprite = listSprite[0];
                SoundManager.instance.playEffectFromPath(SOUND_GAME.ALL_IN);
                break;
            case "Raise":
                spIcStatus.sprite = listSprite[1];
                SoundManager.instance.playEffectFromPath(SOUND_GAME.BET);
                break;
            case "Call":
                spIcStatus.sprite = chipBet == 0 ? listSprite[3] : listSprite[2];
                SoundManager.instance.playEffectFromPath(SOUND_GAME.BET);
                break;
            case "Check":
                GetComponent<Image>().enabled = false;
                spIcStatus.sprite = listSprite[3];
                break;
            case "Fold":
                GetComponent<Image>().enabled = false;
                lbChip.text = "";
                spIcStatus.sprite = listSprite[4];
                break;
            default:
                spIcStatus.enabled = false;
                spIcStatus.sprite = null;
                break;
        }
    }

    public void offSpriteAll()
    {
        GetComponent<Image>().enabled = false;
        lbChip.text = "";
    }
}
