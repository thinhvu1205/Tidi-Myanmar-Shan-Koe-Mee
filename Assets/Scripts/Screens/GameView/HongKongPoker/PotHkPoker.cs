using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PotHkPoker : MonoBehaviour
{
    [SerializeField]
    List<TextMeshProUGUI> listNum;
    private int value = 0, valueChange = 0, potValue = 0;

    void Start()
    {
        for (int i = 0; i < listNum.Count; i++)
        {
            listNum[i].text = "0";
        }
    }

    public void setValue(int valueNew, float delayTime = 0)
    {
        valueChange = valueNew - value;
        potValue = valueNew;
        DOTween.Sequence()
            .AppendInterval(delayTime).
            AppendCallback(() =>
            {
                //EffectMoneyPotChange(valueChange);
                //value = valueNew;
                tweenPotTo(valueNew);
                //value = valueNew;
            });
    }

    public int getValue()
    {
        return potValue;
    }

    public int getValueChange()
    {
        return valueChange;
    }

    public void tweenPotTo(int newValue)
    {
        DOTween.To(() => value, x => value = x, newValue, 2.0f).OnUpdate(() =>
            {
                string valueStr = "";
                valueStr = value.ToString();
                int count = 0;
                for (int i = 0; i < listNum.Count; i++)
                {
                    if (i >= listNum.Count - valueStr.Length)
                    {
                        listNum[i].text = valueStr[count].ToString();
                        DOTween.Sequence()
                            .Append(listNum[i].transform.DOScale(new Vector2(1.4f, 1.4f), 0.1f))
                            .Append(listNum[i].transform.DOScale(Vector2.one, 0.1f)).SetEase(Ease.InBack);
                        count++;
                    }
                    else
                    {
                        listNum[i].text = "0";
                    }
                }
            }).OnComplete(() =>
            {
            })
            .SetEase(Ease.OutSine);
    }

}
