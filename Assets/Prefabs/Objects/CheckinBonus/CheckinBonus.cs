using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Globals;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckinBonus : MonoBehaviour
{
    [SerializeField] private Button buttonDaily, buttonWeekly, buttonClose, buttonReceiveDaily;
    [SerializeField] private List<SkeletonGraphic> listAnimGiftDaily;
    [SerializeField] private List<TextMeshProUGUI> listTextChipBonusDaily, listTextTimeCountDown;
    [SerializeField] private Image imageCurrentChipBonusDaily, imageBoxChip;
    [SerializeField] private List<Sprite> listSpriteCurrentChipBonus, listSpriteChipBonusGray;
    [SerializeField] private TextMeshProUGUI textCurrentChipBonusDaily;
    [SerializeField] private Sprite spriteChoose, spriteNotChoose, spriteButtonReceiveGray, spriteButtonReceiveGreen, spriteButtonReceiveYellow;
    [SerializeField] private Sprite spriteBoxGray, spriteBoxPurple, spriteBoxYellow, spriteBoxGrayDay7, spriteBoxPurpleDay7, spriteBoxYellowDay7;
    [SerializeField] private GameObject daily, weekly, popupCheckinBonus;
    [SerializeField] private Slider sliderDaily;
    [SerializeField] private List<Day> listDayWeekly;
    [SerializeField] private Sprite spriteBoxChipGray, spriteBoxChipRed;
    [SerializeField] private List<Image> listImageFrameTime;
    private Vector3 originalScale;
    public Coroutine sendCoroutine;
    public static CheckinBonus instance = null;
    void Awake()
    {
        instance = this;
        originalScale = popupCheckinBonus.transform.localScale;
    }
    void OnEnable()
    {

        popupCheckinBonus.transform.localScale = originalScale;
        int index = CheckInBonusModel.Promotion.CurrentWeekly.OD - 1;
        for (int i = 0; i < listDayWeekly.Count; i++)
        {
            listDayWeekly[i].IdDay = i;
            listDayWeekly[i].textChipBonus.text = $"{CheckInBonusModel.Promotion.CurrentWeekly.listDP[i]} <sprite name=chipYellow>";
            listDayWeekly[i].imageTick.gameObject.SetActive(false);
            listDayWeekly[i].imageCoin.sprite = listSpriteCurrentChipBonus[i];
            listDayWeekly[i].imageCoin.SetNativeSize();
            if (CheckInBonusModel.Promotion.CurrentWeekly.OD < 15)
            {
                if (i < index)
                {
                    listDayWeekly[i].textButtonReceive.text = $"Received";
                    listDayWeekly[i].imageFrame.sprite = spriteBoxGray;
                    listDayWeekly[i].imageFrame.SetNativeSize();
                    listDayWeekly[i].imageTick.gameObject.SetActive(true);
                    listDayWeekly[i].buttonReceive.gameObject.SetActive(true);
                    listDayWeekly[i].buttonReceive.image.sprite = spriteButtonReceiveGray;
                    listDayWeekly[i].buttonReceive.interactable = false;
                    listDayWeekly[i].buttonReceive.image.SetNativeSize();
                    listDayWeekly[i].imageCoin.sprite = listSpriteChipBonusGray[i];
                    listDayWeekly[i].textChipBonus.color = Color.white;
                    listDayWeekly[i].textChipBonus.text = $"{CheckInBonusModel.Promotion.CurrentWeekly.listDP[i]} <sprite name=chipGray>";
                    listDayWeekly[i].imageBoxChip.sprite = spriteBoxChipGray;
                    listDayWeekly[i].imageBoxChip.SetNativeSize();
                }
                else if (i == index)
                {
                    listDayWeekly[i].imageFrame.sprite = spriteBoxYellow;
                    if (i == 6)
                    {
                        listDayWeekly[i].imageFrame.sprite = spriteBoxYellowDay7;
                    }
                    listDayWeekly[i].textButtonReceive.text = $"Receive";
                    listDayWeekly[i].imageFrame.SetNativeSize();
                    listDayWeekly[i].imageTick.gameObject.SetActive(false);
                    listDayWeekly[i].buttonReceive.interactable = true;
                    listDayWeekly[i].buttonReceive.image.sprite = spriteButtonReceiveGreen;
                    listDayWeekly[i].buttonReceive.gameObject.SetActive(true);
                    listDayWeekly[i].buttonReceive.image.SetNativeSize();
                    listDayWeekly[i].imageCoin.sprite = listSpriteCurrentChipBonus[i];
                    listDayWeekly[i].textChipBonus.color = Color.yellow;
                    listDayWeekly[i].textChipBonus.text = $"{CheckInBonusModel.Promotion.CurrentWeekly.listDP[i]} <sprite name=chipYellow>";
                    listDayWeekly[i].imageBoxChip.sprite = spriteBoxChipRed;
                    listDayWeekly[i].imageBoxChip.SetNativeSize();
                }
                else
                {
                    if (i == index + 1)
                    {
                        listDayWeekly[i].imageFrame.sprite = spriteBoxPurple;
                        if (i == 6)
                        {
                            listDayWeekly[i].imageFrame.sprite = spriteBoxPurpleDay7;
                        }
                        listDayWeekly[i].imageFrame.SetNativeSize();
                        listDayWeekly[i].imageTick.gameObject.SetActive(false);
                        listDayWeekly[i].buttonReceive.interactable = false;
                        listDayWeekly[i].buttonReceive.image.sprite = spriteButtonReceiveYellow;
                        listDayWeekly[i].buttonReceive.image.SetNativeSize();
                        listDayWeekly[i].imageCoin.sprite = listSpriteCurrentChipBonus[i];
                        listDayWeekly[i].textChipBonus.color = Color.yellow;
                        listDayWeekly[i].textChipBonus.text = $"{CheckInBonusModel.Promotion.CurrentWeekly.listDP[i]} <sprite name=chipYellow>";
                        listDayWeekly[i].buttonReceive.image.gameObject.SetActive(true);
                        listDayWeekly[i].textButtonReceive.text = $"Next Day";
                        listDayWeekly[i].imageBoxChip.sprite = spriteBoxChipRed;
                        listDayWeekly[i].imageBoxChip.SetNativeSize();
                    }
                    else
                    {
                        listDayWeekly[i].imageFrame.sprite = spriteBoxPurple;
                        if (i == 6)
                        {
                            listDayWeekly[i].imageFrame.sprite = spriteBoxPurpleDay7;
                        }
                        listDayWeekly[i].imageFrame.SetNativeSize();
                        listDayWeekly[i].imageTick.gameObject.SetActive(false);
                        listDayWeekly[i].buttonReceive.interactable = false;
                        listDayWeekly[i].buttonReceive.image.gameObject.SetActive(false);
                        listDayWeekly[i].buttonReceive.image.SetNativeSize();
                        listDayWeekly[i].imageCoin.sprite = listSpriteCurrentChipBonus[i];
                        listDayWeekly[i].textChipBonus.color = Color.yellow;
                        listDayWeekly[i].textChipBonus.text = $"{CheckInBonusModel.Promotion.CurrentWeekly.listDP[i]} <sprite name=chipYellow>";
                        listDayWeekly[i].imageBoxChip.sprite = spriteBoxChipRed;
                        listDayWeekly[i].imageBoxChip.SetNativeSize();
                    }
                }
            }
            else
            {
                if (i < CheckInBonusModel.Promotion.CurrentWeekly.OD - 15)
                {
                    listDayWeekly[i].textButtonReceive.text = $"Received";
                    listDayWeekly[i].imageFrame.sprite = spriteBoxGray;
                    listDayWeekly[i].imageFrame.SetNativeSize();
                    listDayWeekly[i].imageTick.gameObject.SetActive(true);
                    listDayWeekly[i].buttonReceive.interactable = false;
                    listDayWeekly[i].buttonReceive.image.sprite = spriteButtonReceiveGray;
                    listDayWeekly[i].buttonReceive.gameObject.SetActive(true);
                    listDayWeekly[i].buttonReceive.image.SetNativeSize();
                    listDayWeekly[i].imageCoin.sprite = listSpriteChipBonusGray[i];
                    listDayWeekly[i].textChipBonus.color = Color.white;
                    listDayWeekly[i].textChipBonus.text = $"{CheckInBonusModel.Promotion.CurrentWeekly.listDP[i]} <sprite name=chipGray>";
                    listDayWeekly[i].imageBoxChip.sprite = spriteBoxChipGray;
                    listDayWeekly[i].imageBoxChip.SetNativeSize();
                }
                else if (i == CheckInBonusModel.Promotion.CurrentWeekly.OD - 15)
                {
                    listDayWeekly[i].imageFrame.sprite = spriteBoxPurple;
                    listDayWeekly[i].imageFrame.SetNativeSize();
                    listDayWeekly[i].imageTick.gameObject.SetActive(false);
                    listDayWeekly[i].buttonReceive.interactable = false;
                    listDayWeekly[i].buttonReceive.image.sprite = spriteButtonReceiveYellow;
                    listDayWeekly[i].buttonReceive.image.SetNativeSize();
                    listDayWeekly[i].imageCoin.sprite = listSpriteCurrentChipBonus[i];
                    listDayWeekly[i].textChipBonus.color = Color.yellow;
                    listDayWeekly[i].textChipBonus.text = $"{CheckInBonusModel.Promotion.CurrentWeekly.listDP[i]} <sprite name=chipYellow>";
                    listDayWeekly[i].buttonReceive.gameObject.SetActive(true);
                    listDayWeekly[i].textButtonReceive.text = $"Next Day";
                    listDayWeekly[i].imageBoxChip.sprite = spriteBoxChipRed;
                    listDayWeekly[i].imageBoxChip.SetNativeSize();
                }
                else
                {
                    listDayWeekly[i].imageFrame.sprite = spriteBoxPurple;
                    if (i == 6)
                    {
                        listDayWeekly[i].imageFrame.sprite = spriteBoxPurpleDay7;
                    }
                    listDayWeekly[i].imageFrame.SetNativeSize();
                    listDayWeekly[i].imageTick.gameObject.SetActive(false);
                    listDayWeekly[i].buttonReceive.interactable = false;
                    listDayWeekly[i].buttonReceive.gameObject.SetActive(false);
                    listDayWeekly[i].buttonReceive.image.SetNativeSize();
                    listDayWeekly[i].imageCoin.sprite = listSpriteCurrentChipBonus[i];
                    listDayWeekly[i].textChipBonus.color = Color.yellow;
                    listDayWeekly[i].textChipBonus.text = $"{CheckInBonusModel.Promotion.CurrentWeekly.listDP[i]} <sprite name=chipYellow>";
                    listDayWeekly[i].imageBoxChip.sprite = spriteBoxChipRed;
                    listDayWeekly[i].imageBoxChip.SetNativeSize();
                }
                if (CheckInBonusModel.Promotion.CurrentWeekly.OD - 15 == 7)
                {
                    int indexSpecial = 6;
                    listDayWeekly[indexSpecial].imageFrame.sprite = spriteBoxGrayDay7;
                    listDayWeekly[indexSpecial].imageFrame.SetNativeSize();
                    listDayWeekly[indexSpecial].imageTick.gameObject.SetActive(true);
                    listDayWeekly[indexSpecial].buttonReceive.interactable = false;
                    listDayWeekly[indexSpecial].buttonReceive.image.sprite = spriteButtonReceiveGray;
                    listDayWeekly[indexSpecial].buttonReceive.image.SetNativeSize();
                    listDayWeekly[indexSpecial].imageCoin.sprite = listSpriteChipBonusGray[indexSpecial];
                    listDayWeekly[indexSpecial].textChipBonus.color = Color.white;
                    listDayWeekly[indexSpecial].textChipBonus.text = $"{CheckInBonusModel.Promotion.CurrentWeekly.listDP[indexSpecial]} <sprite name=chipGray>";
                    listDayWeekly[indexSpecial].imageBoxChip.sprite = spriteBoxChipGray;
                    listDayWeekly[indexSpecial].imageBoxChip.SetNativeSize();
                }
            }
        }
        DOVirtual.DelayedCall(0.1f, () =>
        {
            if (CheckInBonusModel.Promotion.CurrentDaily.T != 0)
            {
                buttonReceiveDaily.interactable = false;
                buttonReceiveDaily.image.sprite = spriteButtonReceiveGray;
                buttonReceiveDaily.image.SetNativeSize();
            }
            UpdateSlider(CheckInBonusModel.Promotion.CurrentDaily.OC);
            SetDataPromotion();
        });
    }
    void Start()
    {
        ClickButtonDaily();
        buttonReceiveDaily.onClick.AddListener(ClickButtonReceiveDaily);
        buttonDaily.onClick.AddListener(ClickButtonDaily);
        buttonWeekly.onClick.AddListener(ClickButtonWeekly);
        buttonClose.onClick.AddListener(ClickButtonClose);
    }
    private void SetDataPromotion()
    {
        int indexOfList = CheckInBonusModel.Promotion.CurrentDaily.OC;
        int chipBonus = CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[indexOfList];
        textCurrentChipBonusDaily.text = $"{chipBonus} <sprite index=0>";
        for (int i = 0; i < listTextChipBonusDaily.Count; i++)
        {
            if (CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i] <= 1000)
            {
                listTextChipBonusDaily[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i]} <sprite index=0>";
            }
            else
            {
                listTextChipBonusDaily[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i] / 1000}k <sprite index=0>";
            }
            if (i < CheckInBonusModel.Promotion.CurrentDaily.OC)
            {
                listImageFrameTime[i].gameObject.SetActive(false);
                listTextTimeCountDown[i].gameObject.SetActive(false);
                listAnimGiftDaily[i].AnimationState.SetAnimation(0, "received", true);
                listTextChipBonusDaily[i].color = Color.white;
                if (CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i] <= 1000)
                {
                    listTextChipBonusDaily[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i]} <sprite name=chipGraySmall>";
                }
                else
                {
                    listTextChipBonusDaily[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i] / 1000}k <sprite name=chipGraySmall>";
                }
            }
            else if (i == CheckInBonusModel.Promotion.CurrentDaily.OC)
            {
                if (CheckInBonusModel.Promotion.CurrentDaily.T == 0)
                {
                    SetCanReceivePromotionOnline(i);
                }
                else
                {
                    listImageFrameTime[i].gameObject.SetActive(true);
                    listTextTimeCountDown[i].gameObject.SetActive(true);
                    StartCoroutine(CountDownTextTime(i));
                    listAnimGiftDaily[i].AnimationState.SetAnimation(0, "not_receive", true);
                    listTextChipBonusDaily[i].color = Color.yellow;
                    if (CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i] <= 1000)
                    {
                        listTextChipBonusDaily[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i]} <sprite name=chipYellowSmall>";
                    }
                    else
                    {
                        listTextChipBonusDaily[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i] / 1000}k <sprite name=chipYellowSmall>";
                    }
                    buttonReceiveDaily.interactable = false;
                    imageBoxChip.sprite = spriteBoxChipRed;
                    imageBoxChip.SetNativeSize();
                    buttonReceiveDaily.image.sprite = spriteButtonReceiveGray;
                    buttonReceiveDaily.image.SetNativeSize();
                    imageCurrentChipBonusDaily.sprite = listSpriteCurrentChipBonus[i];
                    imageCurrentChipBonusDaily.SetNativeSize();
                    textCurrentChipBonusDaily.text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i]} <sprite name=chipYellow>";
                }
            }
            else
            {
                listTextChipBonusDaily[i].color = Color.yellow;
                if (CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i] <= 1000)
                {
                    listTextChipBonusDaily[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i]} <sprite name=chipYellowSmall>";
                }
                else
                {
                    listTextChipBonusDaily[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i] / 1000}k <sprite name=chipYellowSmall>";
                }
                listImageFrameTime[i].gameObject.SetActive(false);
                listTextTimeCountDown[i].gameObject.SetActive(false);
                listAnimGiftDaily[i].AnimationState.SetAnimation(0, "not_receive", true);
                // if (/*CheckInBonusModel.Promotion.CurrentDaily.T == 0 &&*/ i == CheckInBonusModel.Promotion.CurrentDaily.OC + 1)
                // {
                //     listImageFrameTime[i].gameObject.SetActive(true);
                //     listTextTimeCountDown[i].gameObject.SetActive(true);
                //     listTextTimeCountDown[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.GetNextWaitingTimeString()}";
                // }
            }
        }
    }
    private void SetCanReceivePromotionOnline(int i)
    {
        listImageFrameTime[i].gameObject.SetActive(false);
        listTextTimeCountDown[i].gameObject.SetActive(false);
        listAnimGiftDaily[i].AnimationState.SetAnimation(0, "receive", true);
        listTextChipBonusDaily[i].color = Color.yellow;
        if (CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i] <= 1000)
        {
            listTextChipBonusDaily[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i]} <sprite name=chipYellowSmall>";
        }
        else
        {
            listTextChipBonusDaily[i].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i] / 1000}k <sprite name=chipYellowSmall>";
        }
        buttonReceiveDaily.interactable = true;
        imageBoxChip.sprite = spriteBoxChipRed;
        imageBoxChip.SetNativeSize();
        buttonReceiveDaily.image.sprite = spriteButtonReceiveGreen;
        buttonReceiveDaily.image.SetNativeSize();
        imageCurrentChipBonusDaily.sprite = listSpriteCurrentChipBonus[i];
        imageCurrentChipBonusDaily.SetNativeSize();
        textCurrentChipBonusDaily.text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[i]} <sprite name=chipYellow>";
    }
    private IEnumerator CountDownTextTime(int index, bool activeButtonReceive = true)
    {
        if (index < 0
            || index >= listImageFrameTime.Count
            || index >= listTextTimeCountDown.Count)
        {
            yield break;
        }

        int counter = 0;
        var frameObj = listImageFrameTime[index].gameObject;
        var textObj = listTextTimeCountDown[index];

        frameObj.SetActive(true);
        textObj.gameObject.SetActive(true);

        while (CheckInBonusModel.Promotion.CurrentDaily.T >= 0)
        {
            counter++;
            textObj.text = CheckInBonusModel.Promotion.CurrentDaily
                .GetTimeRemainFormatted(CheckInBonusModel.Promotion.CurrentDaily.T);

            if (counter % 5 == 0)
            {
                SocketSend.sendPromotion();
            }
            yield return new WaitForSeconds(1);
            CheckInBonusModel.Promotion.CurrentDaily.T--;
        }

        DOVirtual.DelayedCall(2f, () =>
        {
            SocketSend.sendCheckTime();
            SocketSend.sendPromotion();
            if (!activeButtonReceive) return;
            SetCanReceivePromotionOnline(CheckInBonusModel.Promotion.CurrentDaily.OC);
        });
    }

    private void ClickButtonReceiveDaily()
    {
        UIManager.instance.hasDailyBonus = false;
        int index = CheckInBonusModel.Promotion.CurrentDaily.OC;
        SocketSend.sendReceivePromotion(CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[index]);
        SocketSend.sendPromotion();
        UpdateSlider(CheckInBonusModel.Promotion.CurrentDaily.OC + 1);
        Debug.Log($"OC: {CheckInBonusModel.Promotion.CurrentDaily.OC}");
        StartCoroutine(CountDownTextTime(CheckInBonusModel.Promotion.CurrentDaily.OC + 1));
        // buttonReceiveDaily.interactable = false;
        listAnimGiftDaily[index].AnimationState.SetAnimation(0, "click_receive", false).Complete += (entry) =>
        {
            // buttonReceiveDaily.interactable = true;
            textCurrentChipBonusDaily.color = Color.white;
            imageCurrentChipBonusDaily.sprite = listSpriteChipBonusGray[index];
            imageBoxChip.sprite = spriteBoxChipGray;
            imageBoxChip.SetNativeSize();
            imageCurrentChipBonusDaily.SetNativeSize();
            SocketSend.sendUAG();
            listAnimGiftDaily[index].AnimationState.SetAnimation(0, "received", true);
            listTextChipBonusDaily[index].color = Color.white;
            if (CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[index] <= 1000)
            {
                listTextChipBonusDaily[index].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[index]} <sprite name=chipGraySmall>";
            }
            else
            {
                listTextChipBonusDaily[index].text = $"{CheckInBonusModel.Promotion.CurrentDaily.OnlinePolicy.chipBonus[index] / 1000}k <sprite name=chipGraySmall>";
            }
        };
        buttonReceiveDaily.interactable = false;
        buttonReceiveDaily.image.sprite = spriteButtonReceiveGray;
        buttonReceiveDaily.image.SetNativeSize();
    }
    private void UpdateSlider(int value)
    {
        value = Mathf.Clamp(value, 0, 6);
        float normalizedValue = (float)value / 6f;
        sliderDaily.value = normalizedValue;
    }


    private void ClickButtonDaily()
    {
        daily.SetActive(true);
        weekly.SetActive(false);
        buttonDaily.image.sprite = spriteChoose;
        buttonDaily.image.SetNativeSize();
        buttonWeekly.image.sprite = spriteNotChoose;
        buttonWeekly.image.SetNativeSize();
    }
    private void ClickButtonWeekly()
    {
        daily.SetActive(false);
        weekly.SetActive(true);
        buttonDaily.image.sprite = spriteNotChoose;
        buttonDaily.image.SetNativeSize();
        buttonWeekly.image.sprite = spriteChoose;
        buttonWeekly.image.SetNativeSize();
    }
    private void ClickButtonClose()
    {
        UIManager.instance.canShowPopupCheckinBonus = false;
        popupCheckinBonus.transform.DOScale(Vector3.zero, 0.25f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                popupCheckinBonus.transform.localScale = originalScale;
            });
        UIManager.instance.SetStateIconNotiCheckinBonus();
    }
    public void SetDayReceived(int idDay)
    {
        if (idDay == 6)
        {
            listDayWeekly[idDay].imageFrame.sprite = spriteBoxGrayDay7;
        }
        else
        {
            listDayWeekly[idDay].imageFrame.sprite = spriteBoxGray;
        }
        listDayWeekly[idDay].textButtonReceive.text = $"Received";
        listDayWeekly[idDay].imageFrame.SetNativeSize();
        listDayWeekly[idDay].imageTick.gameObject.SetActive(true);
        listDayWeekly[idDay].buttonReceive.interactable = false;
        listDayWeekly[idDay].buttonReceive.image.sprite = spriteButtonReceiveGray;
        listDayWeekly[idDay].buttonReceive.image.SetNativeSize();
        listDayWeekly[idDay].imageCoin.sprite = listSpriteChipBonusGray[idDay];
        listDayWeekly[idDay].textChipBonus.color = Color.white;
        listDayWeekly[idDay].textChipBonus.text = $"{CheckInBonusModel.Promotion.CurrentWeekly.listDP[idDay]} <sprite name=chipGray>";
        listDayWeekly[idDay].imageBoxChip.sprite = spriteBoxChipGray;
        listDayWeekly[idDay].imageBoxChip.SetNativeSize();
    }
}