using UnityEngine;
using Globals;
using Newtonsoft.Json.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

using UnityEngine.EventSystems;


using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Spine.Unity;

public class ThreePokerCardGameView : GameView
{
    [HideInInspector] public List<int> ListValueChip = new List<int>();
    [SerializeField] private GameObject m_PrepabChip;
    [SerializeField] private GameObject m_TextBetChangeMoneyBet;
    [SerializeField] private GameObject m_AnimPlayFold;
    [SerializeField] private Transform layerChip;
    [SerializeField] private List<GameObject> m_ListGate;
    [SerializeField] private Transform m_ContainerChipBet;
    [SerializeField] private List<GameObject> m_ChipBet;
    [SerializeField] private GameObject m_Mask;
    [SerializeField] private GameObject m_Rule;
    [SerializeField] private Transform m_ContainerCards;
    [SerializeField] private Transform m_GroupBtnPlay;
    [SerializeField] private Transform m_GroupBtnBet;
    [SerializeField] private Transform m_ContainerLabel;
    [SerializeField] private GameObject m_Label;
    [SerializeField] private GameObject m_FrameChip;
    public List<long> MoneyAllInGate = new List<long>();
    public List<long> ListChipMeGateBefore = new List<long>();
    public List<long> ListChipMeGateLast = new List<long>();
    public ThreePokerChipManager ThreePokerChipManager;
    public List<GameObject> chipBetPool = new List<GameObject>();
    public int PositionChipbet = 0;
    public List<List<Card>> ListCardPlayer = new List<List<Card>>();
    private float _DEALER_SCALE = 0.5f, _PLAYER_SCALE = 0.7f, _OTHER_SCALE = 0.35f;
    private List<Vector2> listPositionCardDealer = new List<Vector2> { new Vector2(-90f, 215f), new Vector2(0f, 215f), new Vector2(90f, 215f) };
    private List<GameObject> listFrameChip = new List<GameObject>();
    private List<GameObject> listLabel = new List<GameObject>();
    [SerializeField] List<TextMeshProUGUI> txtJackpot;
    [SerializeField] private Animation m_JackpotAnimA;
    protected override void updatePositionPlayerView()
    {
        players.Remove(thisPlayer);
        players.Insert(0, thisPlayer);
        for (int i = 0; i < players.Count; i++)
        {
            if (i < listPosView.Count)
            {
                players[i].playerView.transform.localPosition = listPosView[i];
                players[i].playerView.transform.localScale = players[i] == thisPlayer ? new Vector2(0.8f, 0.8f) : new Vector2(0.7f, 0.7f);
                players[i].updatePlayerView();
                players[i].playerView.gameObject.SetActive(true);
                players[i].updateItemVip(players[i].vip);
            }
        }
    }

    public void handleUpdateJackpot(JObject jsonData)
    {
        var curJackPotBinh = "0";
        if (jsonData != null && jsonData.ContainsKey("M"))
        {
            curJackPotBinh = (long)jsonData["M"] + "";
        }
        var indexRun = curJackPotBinh.Length - 1;
        for (var i = txtJackpot.Count - 1; i >= 0; i--)
        {
            if (indexRun >= 0)
            {
                txtJackpot[i].text = curJackPotBinh[indexRun] + "";
            }
            else
            {
                txtJackpot[i].text = "0";
            }
            indexRun--;
            StartCoroutine(animateJackPot(txtJackpot[i].gameObject));
        }
    }
    private IEnumerator animateJackPot(GameObject node)
    {
        float duration = 0.15f, time = 0;
        Vector3 initialScale = node.transform.localScale, targetScale = initialScale * 1.2f;
        while (time < duration)
        {
            time += Time.deltaTime;
            node.transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
            yield return null;
        }
        time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            node.transform.localScale = Vector3.Lerp(targetScale, initialScale, time / duration);
            yield return null;
        }
        node.transform.localScale = initialScale;
    }

    public override void handleCTable(string data)
    {
        base.handleCTable(data);
        thisPlayer.playerView.setPosThanhBarThisPlayer();
        showNoti(2);
        JObject jData = JObject.Parse(data);
        agTable = getInt(jData, "M");
        ListValueChip = new List<int> { agTable, agTable * 5, agTable * 10, agTable * 50, agTable * 100 };
        SetValueInchip();
        ThreePokerChipManager.SetListValueChip(ListValueChip);
    }
    private void showNoti(int type)
    {
        switch (type)
        {
            case 0:
                UIManager.instance.showToast("Mangyaring maghintay para sa susunod na laro");
                break;
            case 1:
                UIManager.instance.showToast("Lahat sa mesa ang naglalaro, maaring lamang antayin ang susunod na laro!!");
                break;
            case 2:
                UIManager.instance.showToast("Magsisimula ang Laro, mangyaring maghintayâ€¦");
                break;
        }
    }
    public void SetValueInchip()
    {
        for (int i = 0; i < m_ChipBet.Count; i++)
        {

            if (m_ChipBet[i].transform.childCount == 0)
            {
                Debug.LogError($"m_ChipBet[{i}] does not have any children.");
                continue;
            }
            TextMeshProUGUI nodeText = m_ChipBet[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            nodeText.text = Globals.Config.FormatMoney2(ListValueChip[i], true);
            nodeText.transform.localScale = new Vector2(1, 1);
            nodeText.color = Color.black;
        }
    }
    public override void handleSTable(string data)
    {

        base.handleSTable(data);
        cleanTable();
        thisPlayer.playerView.setPosThanhBarThisPlayer();
        JObject jData = JObject.Parse(data);
        agTable = getInt(jData, "M");
        ListValueChip = new List<int> { agTable, agTable * 5, agTable * 10, agTable * 50, agTable * 100 };
        SetValueInchip();
        ThreePokerChipManager.SetListValueChip(ListValueChip);
    }
    private void showNodeChip()
    {
        PositionChipbet = 0;
        SetValueInchip();
        ChooseChip(m_ChipBet[0]);
        m_ContainerChipBet.gameObject.SetActive(true);
        m_ContainerChipBet.transform.DOScale(new Vector3(1.05f, 1.05f, 1.05f), .3f).OnComplete(() =>
        {
            m_ContainerChipBet.transform.DOScale(Vector2.one, .3f);
        });
        // float y = -Screen.height / 2 + 50; // Tọa độ y cách đáy màn hình 120 đơn vị
        // Vector2 targetPosition = new Vector2(0, y);
        // Debug.Log($"Target position for m_ContainerChipBet: {targetPosition}");
        // RectTransform containerRect = m_ContainerChipBet.GetComponent<RectTransform>();
        // if (containerRect != null)
        // {
        //     containerRect.DOAnchorPos(targetPosition, 0.4f).SetEase(Ease.OutCubic);
        // }
        // else
        // {
        //     Debug.LogError("m_ContainerChipBet is not a RectTransform.");
        // }
    }
    public ThreePokerChipManager createChip(long valueChip)
    {
        // Tìm chip không hoạt động trong pool
        GameObject go = chipBetPool.Find(chip => !chip.activeSelf);

        if (go == null)
        {
            // Nếu không có chip nào khả dụng, tạo mới
            go = BundleHandler.Instantiate(m_PrepabChip, layerChip);
            chipBetPool.Add(go); // Thêm chip mới vào pool
        }

        ThreePokerChipManager chipBet = go.GetComponent<ThreePokerChipManager>();
        chipBet.SetValueChip(valueChip);
        chipBet.transform.SetSiblingIndex(transform.childCount - 2);
        chipBet.gameObject.SetActive(true); // Kích hoạt chip
        return chipBet;
    }
    public override void handleLTable(JObject data)
    {
        base.handleLTable(data);
        updatePositionPlayerView();
        cleanTable();
    }
    public void handleBet(JObject data)
    {
        Player player = getPlayerWithID(getInt(data, "pid"));
        if (player == null) return;
        JObject jData = (JObject)data["data"];
        int gate = (int)getInt(jData, "typeBet");
        long value = (long)getLong(jData, "ag");
        long money = player.ag - value;
        TextMeshProUGUI text = m_ListGate[gate].transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        if (player == thisPlayer)
        {
            MoneyAllInGate[gate] += value;
            text.gameObject.SetActive(true);
        }
        text.text = Globals.Config.FormatMoney2(MoneyAllInGate[gate], true);
        player.ag = money;
        player.setAg();
        if (player == thisPlayer)
        {
            Debug.Log("thằng người chơi có trả về" + gate.ToString());
            ListChipMeGateBefore[gate] += value;
            if (gate == 0)
            {
                OpenGate(2, true);
                OpenGate(3, true);
            }
            if (thisPlayer.ag >= ListChipMeGateBefore.Sum() + ListChipMeGateBefore[0])
            {
                activeDoubleOrRebet(true, 0);
            }
            else
            {
                activeDoubleOrRebet(false, 0);
            }
            if (ListChipMeGateBefore.Sum() > 0)
            {
                activeDoubleOrRebet(false, 1);
            }
        }
        else
        {
            createFrameChip(player, value);

        }
        ThreePokerChipManager go = createChip(value);
        Vector2 startPos = player.playerView.transform.position;
        // if (player == thisPlayer)
        // {
        //     Debug.Log(startPos.ToString());
        // }
        Vector2 endPos = m_ListGate[gate].transform.localPosition;
        go.transform.position = startPos;
        go.transform.localScale = new Vector2(0.7f, 0.7f);
        MoveChipWithDOTween(go, startPos, endPos);
        // Instantiate the effectChange GameObject from the prefab and parent it to m_ContainerChipBet


        // Bắt đầu hoạt ảnh cho hiệu ứng thay đổi giá trị
        StartCoroutine(AnimateMoneyChange(value, m_ListGate[gate]));

    }
    void createFrameChip(Player player, long value)
    {
        List<Vector2> listPostCard = getListPositionCardPlayer(player);
        Vector2 newPosition = new Vector2(listPostCard[1].x, listPostCard[1].y - 80);
        bool positionExists = false;
        GameObject existingFrameChip = null;  // Để lưu trữ đối tượng đã có tại vị trí

        foreach (GameObject frameChip in listFrameChip)
        {
            // So sánh localPosition (Vector3) của frameChip với newPosition (Vector2)
            if (frameChip != null && frameChip.transform.localPosition.x == newPosition.x && frameChip.transform.localPosition.y == newPosition.y)
            {
                positionExists = true;
                existingFrameChip = frameChip;
                break;
            }
        }
        if (positionExists)
        {
            FrameChipView frameChipView = existingFrameChip.GetComponent<FrameChipView>();
            frameChipView.OnBet(value);
        }
        else
        {
            GameObject item = BundleHandler.Instantiate(m_FrameChip, m_ContainerLabel);
            listFrameChip.Add(item);
            item.transform.localPosition = newPosition;
            FrameChipView frameChipView = item.GetComponent<FrameChipView>();
            frameChipView.OnBet(value);
        }


    }

    private IEnumerator AnimateMoneyChange(long value, GameObject gate)
    {
        GameObject effectChange = BundleHandler.Instantiate(m_TextBetChangeMoneyBet, m_ContainerChipBet);
        TextMeshProUGUI effectText = effectChange.GetComponentInChildren<TextMeshProUGUI>();
        effectText.text = "+ " + Globals.Config.FormatMoney2(value, true);
        effectChange.transform.SetParent(gate.transform);
        effectChange.transform.position = gate.transform.position;
        Image image = effectChange.GetComponent<Image>();
        if (image != null)
        {
            image.enabled = false; // Tắt Image nếu có
        }
        effectText.raycastTarget = false;
        effectText.enableKerning = false; // Tắt kerning nếu không cần thiết
        effectText.enableWordWrapping = false; // Tắt word wrapping nếu không cần
        RectTransform rectTransform = effectChange.GetComponent<RectTransform>();
        float elapsedTime = 0f;
        Vector3 startPosition = rectTransform.localPosition;
        Vector3 targetPosition = startPosition + new Vector3(0, 40, 0);
        while (elapsedTime < 1f)
        {
            rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / 1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rectTransform.localPosition = targetPosition;
        float fadeTime = 0.5f;
        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(effectChange);
    }

    private void MoveChipWithDOTween(ThreePokerChipManager chip, Vector2 startPos, Vector2 endPos)
    {
        chip.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        chip.transform.position = startPos;
        Vector2 direction = (endPos - startPos).normalized;
        float offsetDistance = 30f;
        Vector2 offsetPosition = endPos - direction * offsetDistance;
        float randomOffsetX = Random.Range(-15f, 15f);
        float randomOffsetY = Random.Range(-5f, 30f);
        Vector2 randomEndPos = new Vector2(endPos.x + randomOffsetX, endPos.y + randomOffsetY);
        DOTween.Sequence()
            .Append(chip.transform.DOLocalJump(new Vector2(offsetPosition.x, offsetPosition.y), 100f, 1, 0.8f)
                .SetEase(Ease.InSine))
            .Join(chip.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.8f).SetEase(Ease.InSine)) // Phóng to
            .Append(chip.transform.DOLocalJump(new Vector2(randomEndPos.x, randomEndPos.y), 40f, 1, 0.3f)
                .SetEase(Ease.InSine))
            .Join(chip.transform.DOScale(new Vector3(0.7f, 0.7f, 0.7f), 0.3f).SetEase(Ease.InSine)) // Thu nhỏ lại
            .OnComplete(() =>
            {
                chip.gameObject.SetActive(true);
            });
    }
    private void hideNodeChip()
    {
        PositionChipbet = 0;
        SetValueInchip();
        ChooseChip(m_ChipBet[0]);
        m_ContainerChipBet.gameObject.SetActive(false);
        // float y = -Screen.height / 2 - 120;
        // Vector2 targetPosition = new Vector2(0, y);
        // RectTransform containerRect = m_ContainerChipBet.GetComponent<RectTransform>();
        // if (containerRect != null)
        // {
        //     containerRect.DOAnchorPos(targetPosition, 0.4f).SetEase(Ease.OutCubic);
        // }
        // else
        // {
        //     Debug.LogError("m_ContainerChipBet is not a RectTransform.");
        // }
    }
    public void ShowRule()
    {
        m_Mask.SetActive(true);
        RectTransform ruleRect = m_Rule.GetComponent<RectTransform>();
        RectTransform parentRect = m_Rule.transform.parent.GetComponent<RectTransform>();
        if (ruleRect != null && parentRect != null)
        {
            Vector2 targetPosition = new Vector2(parentRect.rect.width / 2 - ruleRect.rect.width / 2, ruleRect.anchoredPosition.y);
            ruleRect.DOAnchorPos(targetPosition, 0.6f).SetEase(Ease.OutCubic);
        }
    }
    public void HideRule()
    {
        m_Mask.SetActive(false);
        RectTransform ruleRect = m_Rule.GetComponent<RectTransform>();
        RectTransform parentRect = m_Rule.transform.parent.GetComponent<RectTransform>();
        if (ruleRect != null)
        {
            Vector2 originalPosition = new Vector2(parentRect.rect.width, 0);
            ruleRect.DOAnchorPos(originalPosition, 0.6f).SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    m_Mask.SetActive(false);
                });
        }
    }
    public Card spawnCard()
    {
        // Tìm lá bài không hoạt động trong pool
        foreach (var cardList in ListCardPlayer)
        {
            foreach (var card in cardList)
            {
                if (card != null && !card.gameObject.activeSelf)
                {
                    return card; // Tái sử dụng lá bài
                }
            }
        }
        Card cardTemp = getCard();
        cardTemp.setTextureWithCode(0);
        cardTemp.transform.localPosition = new Vector2(0f, 300f);
        cardTemp.transform.SetParent(m_ContainerCards);
        return cardTemp;
    }
    public void ChooseChip(GameObject chip)
    {
        SoundManager.instance.soundClick();
        Debug.Log(m_ChipBet[0].transform.childCount.ToString() + "check var phát xem nào");

        for (int i = 0; i < m_ChipBet.Count; i++)
        {
            m_ChipBet[i].transform.GetChild(1).gameObject.SetActive(false);
            m_ChipBet[i].transform.localScale = new Vector2(0.9f, 0.9f);
        }


        if (chip != null)
        {
            chip.transform.GetChild(1).gameObject.SetActive(true);
            chip.transform.localScale = new Vector2(1.2f, 1.2f);
            PositionChipbet = m_ChipBet.IndexOf(chip);
            if (PositionChipbet != -1)
            {
                Debug.Log("The selected chip is the " + (PositionChipbet + 1) + "th chip in the list.");
            }
            else
            {
                Debug.Log("Chip not found in the list.");
            }
        }
        else
        {
            Debug.Log("The selected GameObject is not a Button.");
        }
    }
    public override void handleJTable(string data)
    {
        base.handleJTable(data);
    }
    public override void handleVTable(string data)
    {
        base.handleVTable(data);
        Debug.Log(data + " handleVTable");
        JObject jData = JObject.Parse(data);
        thisPlayer.playerView.setPosThanhBarThisPlayer();
        agTable = getInt(jData, "M");
        ListValueChip = new List<int> { agTable, agTable * 5, agTable * 10, agTable * 50, agTable * 100 };
        SetValueInchip();
        ThreePokerChipManager.SetListValueChip(ListValueChip);
        setDataView(data);
        showNoti(1);
        // showNodeChip();
    }
    public void OpenGate(int index, bool isOn)
    {
        m_ListGate[index].transform.GetChild(0).gameObject.SetActive(!isOn);
        Button button = m_ListGate[index].GetComponent<Button>();
        button.interactable = isOn;
    }
    public void ResetValueGate()
    {
        for (int i = 0; i < m_ListGate.Count; i++)
        {
            m_ListGate[i].transform.GetChild(3).gameObject.SetActive(true);
        }
    }
    public void ChooseGate(GameObject gate)
    {
        SoundManager.instance.soundClick();
        GameObject choose = gate.transform.GetChild(2).gameObject;
        choose.SetActive(true);
        DOVirtual.DelayedCall(1f, () =>
        {
            choose.SetActive(false);
        });
    }
    public void OnMouseEnterGate(GameObject gate)
    {
        if (m_ListGate.IndexOf(gate) == 1)
        {
            return;
        }
        if (gate.transform.GetChild(0).gameObject.activeSelf == true) { return; }
        // Lấy phần tử con cần bật khi di chuột vào
        GameObject highlight = gate.transform.GetChild(2).gameObject; // Giả sử phần tử con thứ 2 là highlight
        highlight.SetActive(true); // Bật highlight
    }

    public void OnMouseExitGate(GameObject gate)
    {
        if (m_ListGate.IndexOf(gate) == 1)
        {
            return;
        }
        if (gate.transform.GetChild(0).gameObject.activeSelf == true) { return; }
        GameObject highlight = gate.transform.GetChild(2).gameObject; // Giả sử phần tử con thứ 2 là highlight
        highlight.SetActive(false);
    }
    void offHighlight()
    {
        foreach (var gate in m_ListGate)
        {
            GameObject highlight = gate.transform.GetChild(2).gameObject; // Giả sử phần tử con thứ 2 là highlight
            highlight.SetActive(false);
        }
    }
    public void handleLc(JObject jData)
    {
        // showNodeChip();
        onHide();

        m_GroupBtnBet.gameObject.SetActive(false);
        m_GroupBtnPlay.gameObject.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            if (i == 1)
            {
                continue;
            }
            OpenGate(i, false);
        }
        hideNodeChip();
        JArray cardValues = getJArray(jData, "data"); // Dữ liệu bài của người chơi chính
        int timeAction = getInt(jData, "timeAction"); // Thời gian hành động                                            // m_ListGate.ForEach(gate => gate.SetActive(false));
        m_Mask.SetActive(false);
        foreach (var player in players)
        {
            if (player == thisPlayer)
            {
                if (stateGame == STATE_GAME.PLAYING)
                {
                    DealCardPlayer(player, cardValues.ToObject<List<int>>()); // Chia bài cho người chơi chính
                }
            }
            else
            {
                DealCardPlayer(player);
            }
        }
        DealCardPlayer(null);
        showCardLabel(null);
        DOVirtual.DelayedCall(2f, () =>
        {
            foreach (var player in players)
            {
                int time = timeAction > 0 ? timeAction - 2 : 10;
                if (player == thisPlayer)
                {
                    if (stateGame == STATE_GAME.PLAYING)
                    {
                        player.setTurn(true, time); // Bật lượt cho người chơi chính
                    }
                }
                else
                {
                    player.setTurn(true, time); // Bật lượt cho các người chơi khác
                }
            }
            if (stateGame == STATE_GAME.PLAYING)
            {
                m_GroupBtnPlay.gameObject.SetActive(true);
                DOVirtual.DelayedCall(timeAction - 2, () =>
                {
                    m_GroupBtnPlay.gameObject.SetActive(false);
                });
            }
        }
        );
    }
    private Vector2 getPositionHandUser(Player player)
    {
        int index = players.IndexOf(player);
        if (index == 0)
        {
            return new Vector2(-150f, -220f);
        }
        else if (index <= 2 && index > 0)
        {
            Transform tranformPlayer = player.playerView.transform;
            return new Vector2(tranformPlayer.localPosition.x - 100f, tranformPlayer.localPosition.y);
        }
        else
        {
            Transform tranformPlayer = player.playerView.transform;
            return new Vector2(tranformPlayer.localPosition.x + 100f, tranformPlayer.localPosition.y);
        }

    }
    private List<Vector2> getListPositionCardPlayer(Player player)
    {
        List<Vector2> vector2 = new List<Vector2>();
        Vector2 positionHandUser = getPositionHandUser(player);
        int space = player == thisPlayer ? 150 : 20;
        if (players.IndexOf(player) > 0 && players.IndexOf(player) < 3)
        {
            space = space * -1;
        }
        for (int i = 0; i < 3; i++)
        {
            vector2.Add(new Vector2(positionHandUser.x + i * space, positionHandUser.y));
        }
        Debug.Log(vector2.Count + "check xem có bao nhiêu card");
        return vector2;
    }
    public void handleStartBet(JObject jData)
    {
        OnShowBet();
        stateGame = STATE_GAME.PLAYING;
        foreach (var player in players)
        {
            player.setTurn(true, getInt(jData, "timeAction"));
        }
        m_GroupBtnPlay.gameObject.SetActive(false);
        m_GroupBtnBet.gameObject.SetActive(true);
        playSound(SOUND_GAME.ALL_IN);
        showNodeChip();
    }
    private void setDataView(string data)
    {
        JObject jData = JObject.Parse(data);
        JArray ArrP = getJArray(jData, "ArrP");
        JArray dealer = getJArray(jData, "dealerCard");
        List<Card> dealerCards = ListCardPlayer[5];
        if (getString(jData, "gameStatus") == "FINISHED")
        {
            stateGame = STATE_GAME.WAITING;
            return;
        }
        if (getString(jData, "gameStatus") == "BET")
        {
            foreach (Player player in players)
            {
                if (player == thisPlayer)
                {
                    if (stateGame == STATE_GAME.PLAYING)
                    {
                        player.setTurn(true, getInt(jData, "CT"));
                        OnShowBet();
                    }
                }
                else
                {
                    player.setTurn(true, getInt(jData, "CT"));
                }
            }
        }
        for (int i = 0; i < dealer.Count; i++)
        {
            Card cardTemp;
            if (i < dealerCards.Count)
            {
                cardTemp = dealerCards[i];
            }
            else
            {
                cardTemp = spawnCard();
                dealerCards.Add(cardTemp);
            }
            cardTemp.transform.localScale = new Vector3(_DEALER_SCALE, _DEALER_SCALE, 1f);
            cardTemp.setTextureWithCode((int)dealer[i]);
            cardTemp.transform.localPosition = listPositionCardDealer[i];
        }
        for (int i = 0; i < ArrP.Count; i++)
        {
            Debug.Log("xem là như nào");
            JObject jPlayer = (JObject)ArrP[i];
            Player player = getPlayerWithID(getInt(jPlayer, "id"));
            if (getInt(jPlayer, "totalBet") > 0)
            {
                long totalBet = getInt(jPlayer, "totalBet");
                player.ag -= totalBet;
            }
            JArray Arr = getJArray(jPlayer, "Arr");
            if (Arr.Count > 0) DealCardPlayer(player, Arr.ToObject<List<int>>());
            if (player != thisPlayer)
            {
                createFrameChip(player, getInt(jPlayer, "totalBet"));
                if (getString(jData, "gameStatus") == "PLAYING" && getBool(jPlayer, "isDecided") == false)
                {
                    player.setTurn(true, getInt(jData, "gameStatus"));
                }
            }
            else
            {
                if (getJArray(jPlayer, "listBeted").Count > 0)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (j == 1)
                        {
                            continue;
                        }
                        OpenGate(j, true);
                        TextMeshProUGUI text = m_ListGate[j].transform.GetChild(3).GetComponent<TextMeshProUGUI>();
                        text.gameObject.SetActive(true);
                        text.text = Globals.Config.FormatMoney2(MoneyAllInGate[j], true);
                    }
                }
                for (int j = 0; j < getJArray(jPlayer, "listBeted").Count; j++)
                {
                    setPlayerDataBeted(getJArray(jPlayer, "listBeted")[j].ToObject<JObject>());
                }

                if (getString(jData, "gameStatus") == "PLAY" &&
     getBool(jPlayer, "isDecided") == false &&
     stateGame == STATE_GAME.PLAYING &&
     getInt(jData, "CT") > 0)
                {
                    // Dừng tất cả các action đang chạy
                    m_GroupBtnPlay.DOKill();

                    // Hiển thị nút chơi
                    m_GroupBtnPlay.gameObject.SetActive(true);

                    // Tạo sequence animation
                    DOTween.Sequence()
                        .AppendInterval(getInt(jData, "CT")) // Delay theo thời gian CT
                        .OnComplete(() =>
                        {
                            m_GroupBtnPlay.gameObject.SetActive(false);
                        });
                    player.setTurn(true, getInt(jData, "CT"));
                }
            }
        }

    }
    private void setPlayerDataBeted(JObject data)
    {

        List<int> listChip = resolveChipAmount(getLong(data, "ag"));
        List<long> listBet = new List<long>();
        for (int i = 0; i < ListValueChip.Count; i++)
        {
            listBet.Add(ListValueChip[i]);
        }
        for (int i = 0; i < listChip.Count; i++)
        {
            for (int j = 0; j < listChip[i]; j++)
            {
                ThreePokerChipManager chip = createChip(listBet[i]);
                Vector2 startPos = thisPlayer.playerView.transform.position;
                Vector2 endPos = m_ListGate[getInt(data, "typeBet")].transform.localPosition;
                MoveChipWithDOTween(chip, startPos, endPos);
            }
        }
        int gateIndex = getInt(data, "typeBet");
        MoneyAllInGate[gateIndex] += getLong(data, "ag");
        TextMeshProUGUI text = m_ListGate[gateIndex].transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        text.gameObject.SetActive(true);
        text.text = Globals.Config.FormatMoney2(getLong(data, "ag"), true);
    }
    public override void handleRJTable(string data)
    {
        base.handleRJTable(data);
        JObject jData = JObject.Parse(data);
        agTable = getInt(jData, "M");
        ListValueChip = new List<int> { agTable, agTable * 5, agTable * 10, agTable * 50, agTable * 100 };
        SetValueInchip();
        ThreePokerChipManager.SetListValueChip(ListValueChip);
        setDataView(data);
        if (getString(jData, "gameStatus") == "FINISHED")
        {
            showNoti(2);
            stateGame = STATE_GAME.WAITING;
        }
    }
    public void handleShowCard(JObject data)
    {
        JArray array = getJArray(data, "data");
        if (array.Count == 0)
        {
            return;
        }
        Player player = getPlayerWithID(getInt(data, "pid"));

        RevealCard(player, array.ToObject<List<int>>());
        showCardLabel(player);

    }
    public void DealCardPlayer(Player player, List<int> cardValues = null)
    {
        int index = player == null ? 5 : players.IndexOf(player);

        List<Card> cardList = ListCardPlayer[index];
        if (cardList.Count > 0) return;
        List<Vector2> listPos = player != null ? getListPositionCardPlayer(player) : listPositionCardDealer;
        if (listPos == null || listPos.Count == 0) return;
        float scale = player == null ? _DEALER_SCALE : (player != thisPlayer ? _OTHER_SCALE : _PLAYER_SCALE);
        for (int i = 0; i < 3; i++)
        {
            Card cardTemp = spawnCard();
            cardList.Add(cardTemp);
            Vector3 stackPos = new Vector3(0, 350, 0);
            cardTemp.transform.localPosition = stackPos;
            cardTemp.transform.localScale = Vector3.zero;
            cardTemp.transform.rotation = Quaternion.Euler(0, 0, -180);
            if (index >= 0 && index < 3)
            {
                cardTemp.transform.SetAsFirstSibling();
            }
            if (index == 1)
            {
                Debug.Log((index >= 0 && index < 3 ? (2 - i) * 2 : i).ToString() + "xem thứ tự lá bài" + listPos[i].x);
            }
            Vector3 targetPosition = new Vector3(listPos[i].x, listPos[i].y, 0);
            Vector3 controlPoint = new Vector3(stackPos.x, stackPos.y - 300, 0);
            Vector3[] bezierPoints = new Vector3[] { stackPos, controlPoint, targetPosition };
            DG.Tweening.Sequence cardSequence = DOTween.Sequence();
            cardSequence.SetDelay(0.2f * i);

            cardSequence.Append(cardTemp.transform.DOScale(new Vector3(scale, scale, 1f), 1f).SetEase(Ease.InOutCubic))  // Tăng thời gian scale và sử dụng Ease mượt hơn
                        .Join(cardTemp.transform.DOLocalRotate(Vector3.zero, 1f).SetEase(Ease.InOutCubic))  // Tăng thời gian xoay và dùng easing tốt hơn
                        .Join(DOTween.To(() => 0f, x =>
                        {
                            float t = x / 1f;
                            Vector3 m1 = Vector3.Lerp(bezierPoints[0], bezierPoints[1], t);
                            Vector3 m2 = Vector3.Lerp(bezierPoints[1], bezierPoints[2], t);
                            cardTemp.transform.localPosition = Vector3.Lerp(m1, m2, t);
                        }, 1f, 1.5f).SetEase(Ease.InOutQuad))  // Điều chỉnh độ mượt cho đường bezier
                        .OnStart(() =>
                        {
                            playSound(SOUND_GAME.CARD_FLIP_1);
                        })
                        .OnComplete(() =>
                        {
                            cardTemp.transform.localPosition = targetPosition;
                            cardTemp.transform.rotation = Quaternion.identity;

                        });
        }
        if (cardValues != null)
        {
            DOVirtual.DelayedCall(2f, () =>  // Tăng độ trễ khi mở bài
            {
                RevealCard(player, cardValues);
                showCardLabel(player);
            });
        }
    }


    private IEnumerator RevealCardCoroutine(Player player, List<int> cardValues)
    {
        if (player == null)
        {
            Debug.Log("Có chạy vào lật bài nhà cái");
        }

        int index = player == null ? 5 : players.IndexOf(player);
        List<Card> cardList = ListCardPlayer[index];
        if (cardList.Count == 0) yield break; // Nếu không có bài thì dừng Coroutine

        float scale = player == null ? _DEALER_SCALE : (player != thisPlayer ? _OTHER_SCALE : _PLAYER_SCALE);
        playSound(SOUND_DOMINO.SHOW_RESULTS);

        for (int i = 0; i < cardList.Count; i++)
        {
            int j = i;
            Card cardTemp = cardList[j];

            // Hủy bỏ các hành động hiện tại của lá bài (nếu có)
            cardTemp.transform.DOKill();

            // Tạo sequence cho hiệu ứng lật bài
            DG.Tweening.Sequence flipSequence = DOTween.Sequence();

            // Lật bài và thay đổi texture
            flipSequence.Append(cardTemp.transform.DOScaleX(0, 0.3f).SetEase(Ease.InBack))
                .AppendCallback(() =>
                {
                    int value = cardValues[j];
                    cardTemp.setTextureWithCode(value); // Gán giá trị (mã) cho lá bài
                    cardTemp.setEffect_Twinkle(true, 2f); // Hiển thị hiệu ứng bài
                })
                .Append(cardTemp.transform.DOScaleX(scale, 0.3f).SetEase(Ease.OutBack)); // Phóng to bài trở lại kích thước ban đầu

            // Xoay lá bài
            cardTemp.transform.DOLocalRotate(new Vector3(0, 0, -10), 0.3f).SetEase(Ease.InOutBack)
                .OnComplete(() =>
                {
                    cardTemp.transform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack);
                });

            // Đợi một khoảng thời gian trước khi lật bài tiếp theo
            yield return new WaitForSeconds(0.1f * i); // Trì hoãn theo thời gian tương ứng với mỗi lá bài
        }
    }

    // Hàm gọi Coroutine từ bên ngoài
    public void RevealCard(Player player, List<int> cardValues)
    {
        StartCoroutine(RevealCardCoroutine(player, cardValues));
    }


    protected override void Start()
    {
        base.Start();
        foreach (var gate in m_ListGate)
        {
            // Đảm bảo mỗi gate có Collider để bắt sự kiện
            if (gate.GetComponent<Collider>() == null)
            {
                gate.AddComponent<BoxCollider>(); // Thêm BoxCollider nếu chưa có
            }

            // Gắn sự kiện di chuột vào
            EventTrigger trigger = gate.AddComponent<EventTrigger>();

            // Sự kiện OnPointerEnter
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((eventData) => { OnMouseEnterGate(gate); });
            trigger.triggers.Add(entryEnter);

            // Sự kiện OnPointerExit
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((eventData) => { OnMouseExitGate(gate); });
            trigger.triggers.Add(entryExit);
        }

    }
    public void SendPlay()
    {
        SocketSend.sendPokerRaise();
        HideGroupBtnPlayWithEffect();
    }
    public void SendFold()
    {
        SocketSend.sendPokerFold();
        HideGroupBtnPlayWithEffect();
    }
    private void HideGroupBtnPlayWithEffect()
    {
        // Đảm bảo m_GroupBtnPlay không bị null
        if (m_GroupBtnPlay == null)
        {
            Debug.LogError("m_GroupBtnPlay is null.");
            return;
        }

        CanvasGroup canvasGroup = m_GroupBtnPlay.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = m_GroupBtnPlay.gameObject.AddComponent<CanvasGroup>();
        }
        DOTween.Sequence()
            .Append(canvasGroup.DOFade(0, 0.5f)) // Mờ dần
            .Join(m_GroupBtnPlay.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack)) // Thu nhỏ
            .OnComplete(() =>
            {
                m_GroupBtnPlay.gameObject.SetActive(false); // Ẩn sau khi hiệu ứng hoàn tất
                canvasGroup.alpha = 1; // Reset alpha để sử dụng lại
                m_GroupBtnPlay.localScale = Vector3.one; // Reset scale
            });
    }
    public void handlePlayerRaise(JObject data)
    {
        Debug.Log("check var" + data.ToString());
        Player player = getPlayerWithID(getInt(data, "pid"));
        if (player == null) return;
        player.setTurn(false);
        JObject jData = (JObject)data["data"];
        int gate = (int)getInt(jData, "typeBet");
        long value = (long)getLong(jData, "ag");
        long money = player.ag - value;
        player.ag = money;
        player.setAg();
        TextMeshProUGUI text = m_ListGate[gate].transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        if (player == thisPlayer)
        {
            MoneyAllInGate[gate] += value;
            text.gameObject.SetActive(true);
        }
        text.text = Globals.Config.FormatMoney2(MoneyAllInGate[gate], true);
        ThreePokerChipManager go = createChip(value);
        Vector2 startPos = player.playerView.transform.position;
        Vector2 endPos = m_ListGate[gate].transform.localPosition;
        go.transform.position = startPos;
        go.transform.localScale = new Vector2(0.7f, 0.7f);
        MoveChipWithDOTween(go, startPos, endPos);
        ShowPlayerPlayAnim(player, "play");
        StartCoroutine(AnimateMoneyChange(value, m_ListGate[gate]));
    }


    public void handleFinish(JObject data)
    {
        // Dừng hiển thị lượt đánh của tất cả người chơi
        foreach (var player in players)
        {
            player.setTurn(false);
        }
        m_GroupBtnBet.gameObject.SetActive(false);
        m_GroupBtnPlay.gameObject.SetActive(false);
        hideNodeChip();

        // Lấy dữ liệu từ server
        JArray arrP = getJArray(data, "data");
        JArray dealerCards = getJArray(data, "dealerCards");
        int timeAction = getInt(data, "timeAction");

        // Hiển thị bài của nhà cái
        RevealCard(null, dealerCards.ToObject<List<int>>());
        showCardLabel(null);
        foreach (JObject playerData in arrP)
        {
            Player player = getPlayerWithID(getInt(playerData, "pid"));
            if (player == null) continue;
            if (player != thisPlayer)
            {
                DOVirtual.DelayedCall(4f, () =>
                {
                    long oldAg = player.ag;
                    long newAg = getLong(playerData, "AG");
                    long moneyChange = getLong(playerData, "M");
                    player.playerView.effectFlyMoney(moneyChange);
                    player.ag = newAg;
                    player.setAg();
                });
            }
            else
            {
                // Xử lý người chơi chính
                float delay = 3f;

                // Kiểm tra jackpot
                if (playerData["jackpot"] != null && getLong(playerData, "jackpot") > 0)
                {
                    long jackpotValue = getLong(playerData, "jackpot");
                    // Hiển thị hiệu ứng jackpot
                    showAnimJackpot(jackpotValue);
                    delay = 4f;
                }

                // Xử lý chip thắng/thua sau khoảng thời gian delay
                DOVirtual.DelayedCall(delay, () =>
                {
                    resolveChipFinish(playerData);
                });
            }
        }


        DOVirtual.DelayedCall(10f, () =>
        {
            cleanTable();
            stateGame = STATE_GAME.WAITING;
            checkAutoExit();
            showNoti(0);
        });
    }

    // Hàm xử lý chip thắng/thua
    private void resolveChipFinish(JObject playerData)
    {
        // Tạo danh sách giá trị chip
        List<long> listBet = new List<long>();
        for (int i = 0; i < ListValueChip.Count; i++)
        {
            listBet.Add(ListValueChip[i]);
        }

        long totalWin = 0;
        long totalLost = 0;

        // Xử lý từng cổng cược
        JArray lstWinLost = getJArray(playerData, "lstWinLost");
        foreach (JObject gateData in lstWinLost)
        {
            int typeBet = getInt(gateData, "typeBet");
            long ag = getLong(gateData, "ag");

            if (ag >= 0)
            {
                // Xử lý thắng
                totalWin += ag;

                if (ag > 0)
                {
                    showGateWin(typeBet);
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        bool isAgBet = gateData["isAgBet"] != null && (bool)gateData["isAgBet"];

                        if (!isAgBet)
                        {
                            // Tạo chip thắng bay từ nhà cái đến cổng
                            List<int> listChip = resolveChipAmount(ag);
                            for (int i = 0; i < listChip.Count; i++)
                            {
                                for (int j = 0; j < listChip[i]; j++)
                                {
                                    ThreePokerChipManager chip = createChip(listBet[i]);
                                    Vector2 startPos = m_ContainerCards.transform.position; // Vị trí nhà cái
                                    Vector2 endPos = m_ListGate[typeBet].transform.localPosition;
                                    MoveChipWithDOTween(chip, startPos, endPos);
                                }
                            }

                            // Hiển thị tổng tiền thắng
                            TextMeshProUGUI text = m_ListGate[typeBet].transform.GetChild(3).GetComponent<TextMeshProUGUI>();
                            text.gameObject.SetActive(true);
                            text.text = Globals.Config.FormatMoney2(ag, true);
                        }

                        // Thu hồi chip sau 1 giây
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            resolveGate(typeBet, true);
                        });
                    });
                }
            }
            else
            {
                // Xử lý thua
                totalLost += ag;
                resolveGate(typeBet, false);
            }
        }

        // Xử lý hiệu ứng tiền thắng/thua
        if (totalLost < 0)
        {
            long tempAg = getLong(playerData, "AG") + totalLost;

            if (totalWin == 0)
            {
                tempAg = getLong(playerData, "AG");
                playSound(SOUND_GAME.LOSE);
                thisPlayer.ag = tempAg;
                thisPlayer.setAg();
            }

            thisPlayer.playerView.effectFlyMoney(totalLost);
        }

        if (totalWin > 0)
        {
            DOVirtual.DelayedCall(3f, () =>
            {
                thisPlayer.playerView.effectFlyMoney(totalWin);
                thisPlayer.ag = getLong(playerData, "AG");
                thisPlayer.setAg();
                getPlayerView(thisPlayer).ShowAniWin();
                playSound(SOUND_GAME.WIN);
            });
        }
    }
    private PlayerViewThreePokerCard getPlayerView(Player player)
    {
        if (player != null)
        {
            return (PlayerViewThreePokerCard)player.playerView;
        }
        return null;

    }
    // Hàm hiển thị hiệu ứng thắng ở cổng cược
    private void showGateWin(int gateIndex)
    {
        if (gateIndex < 0 || gateIndex >= m_ListGate.Count) return;

        GameObject gate = m_ListGate[gateIndex];
        GameObject winEffect = gate.transform.GetChild(1).gameObject;
        winEffect.SetActive(true);

        DOVirtual.DelayedCall(2f, () =>
        {
            winEffect.SetActive(false);
        });
    }

    // Hàm xử lý thu hồi chip ở cổng cược
    private void resolveGate(int gateIndex, bool isWin)
    {
        if (gateIndex < 0 || gateIndex >= m_ListGate.Count) return;

        // Tìm tất cả chip ở gateIndex và thu hồi
        foreach (var chip in chipBetPool)
        {
            if (chip.activeSelf && Vector2.Distance(chip.transform.localPosition, m_ListGate[gateIndex].transform.localPosition) < 50f)
            {
                if (isWin)
                {
                    // Hiệu ứng chip bay về người chơi
                    Vector2 targetPos = thisPlayer.playerView.transform.localPosition;
                    chip.transform.DOLocalMove(targetPos, 0.8f).SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            chip.SetActive(false);
                        });
                }
                else
                {
                    // Hiệu ứng chip bay về vị trí dealer (0, 350)
                    Vector2 dealerPos = new Vector2(0, 350);
                    chip.transform.DOLocalMove(dealerPos, 0.8f).SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            chip.SetActive(false);
                        });
                }
            }
        }

        // Ẩn text hiển thị tiền
        m_ListGate[gateIndex].transform.GetChild(3).gameObject.SetActive(false);
    }

    // Hàm hiển thị hiệu ứng jackpot
    private void showAnimJackpot(long value)
    {
    }

    // Hàm phân tích số tiền thành các chip
    private List<int> resolveChipAmount(long value)
    {
        List<int> result = new List<int> { 0, 0, 0, 0, 0 };
        if (value <= 0) return result;

        long moneyLeft = value;
        List<long> listChip = new List<long>();

        for (int i = 0; i < ListValueChip.Count; i++)
        {
            listChip.Add(ListValueChip[i]);
        }

        for (int i = listChip.Count - 1; i >= 0; i--)
        {
            if (moneyLeft >= listChip[i])
            {
                int chipAmount = (int)(moneyLeft / listChip[i]);
                moneyLeft -= chipAmount * listChip[i];

                if (chipAmount > 8) chipAmount = 8;
                result[i] = chipAmount;
            }
        }

        return result;
    }




    public void handlePlayerFold(JObject data)
    {
        Player player = getPlayerWithID(getInt(data, "pid"));
        if (player == null) return;
        player.setTurn(false);
        playSound(SOUND_GAME.FOLD);
        ShowPlayerPlayAnim(player, "fold");
    }
    public void SendBet(int gate)
    {
        SocketSend.sendPokerBet(ListValueChip[PositionChipbet], gate);
    }
    public void SendReBet()
    {
        if (ListChipMeGateBefore[0] > 0 || ListChipMeGateLast[0] == 0)
        {
            return;
        }
        for (int i = 0; i < ListChipMeGateLast.Count; i++)
        {
            if (ListChipMeGateLast[i] != 0)
            {
                SocketSend.sendPokerBet(ListChipMeGateLast[i], i);
            }
        }

        activeDoubleOrRebet(false, 1);
    }

    public void SendDouble()
    {

        for (int i = 0; i < ListChipMeGateBefore.Count; i++)
        {
            if (ListChipMeGateBefore[i] != 0)
            {
                SocketSend.sendPokerBet(ListChipMeGateBefore[i], i);
            }
        }
        activeDoubleOrRebet(false, 0);
    }
    void onHide()
    {
        offHighlight();
        m_GroupBtnBet.gameObject.SetActive(false);
        m_GroupBtnPlay.gameObject.SetActive(false);
        for (int i = 0; i < m_ListGate.Count; i++)
        {
            if (i == 1)
            {
                continue;
            }
            m_ListGate[i].transform.GetChild(0).gameObject.SetActive(true);
            Button btn = m_ListGate[i].GetComponent<Button>();
            btn.interactable = false;
        }
        if (ListChipMeGateBefore[0] > 0)
        {
            ListChipMeGateLast = ListChipMeGateBefore;
        }
        for (int i = 0; i < 4; i++)
        {
            ListChipMeGateLast.Add(new long());
        }
    }
    void activeDoubleOrRebet(bool isTrue, int index)
    {
        GameObject btnDouble = m_GroupBtnBet.transform.GetChild(index).gameObject;
        btnDouble.GetComponent<Button>().interactable = isTrue;
        GameObject btnDouble_interactive_false = btnDouble.transform.GetChild(0).gameObject;
        btnDouble_interactive_false.SetActive(!isTrue);
    }
    public void OnShowBet()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i == 1)
            {
                continue;
            }
            if (i == 0)
            {
                OpenGate(i, true);
            }
            else
            {
                OpenGate(i, false);
            }

        }
        m_GroupBtnBet.gameObject.SetActive(true);
        m_GroupBtnPlay.gameObject.SetActive(false);
        activeDoubleOrRebet(false, 0);

        bool isRebet = ListChipMeGateLast[0] > 0 ? true : false;
        activeDoubleOrRebet(isRebet, 1);

        //   MoneyAllInGate.Clear();
    }
    void cleanTable()
    {
        foreach (Transform child in m_ContainerLabel)
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject child in listLabel)
        {
            Destroy(child);
        }
        foreach (GameObject child in listFrameChip)
        {
            Destroy(child);
        }
        for (int j = 0; j < ListCardPlayer.Count; j++)
        {
            List<Card> playerCards = ListCardPlayer[j];

            // Tạo hiệu ứng cho từng lá bài
            for (int i = 0; i < playerCards.Count; i++)
            {
                Card card = playerCards[i];
                if (card == null || !card.gameObject.activeSelf) continue;

                // Tạo sequence với delay tăng dần
                DOTween.Sequence()
                    .AppendInterval(0.1f * i) // Delay 0.1s cho mỗi lá
                    .Append(card.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack)) // Thu nhỏ với easeInBack
                    .OnComplete(() =>
                    {

                        card.transform.localPosition = new Vector3(0, 400, 0);
                        Destroy(card);
                    });
            }
        }

        // 2. Xử lý bài của dealer với hiệu ứng tương tự
        List<Card> dealerCards = ListCardPlayer[5];
        for (int i = 0; i < dealerCards.Count; i++)
        {
            Card card = dealerCards[i];
            if (card == null || !card.gameObject.activeSelf) continue;

            DOTween.Sequence()
                .AppendInterval(0.1f * i)
                .Append(card.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack))
                .OnComplete(() =>
                {
                    card.transform.localPosition = new Vector3(0, 400, 0);
                    Destroy(card);
                });
        }

        // 3. Xử lý các chip trên bàn với hiệu ứng tương tự
        float chipDelay = 0f;
        foreach (var chip in chipBetPool)
        {
            if (chip != null && chip.activeSelf)
            {
                DOTween.Sequence()
                    .AppendInterval(chipDelay)
                    .Append(chip.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack))
                    .OnComplete(() =>
                    {
                        chip.transform.localPosition = new Vector3(0, 400, 0);
                        chip.SetActive(false);
                    });
                chipDelay += 0.05f; // Delay ngắn hơn cho chip
            }
        }
        DOVirtual.DelayedCall(1f, () =>
        {
            for (int i = 0; i < m_ListGate.Count; i++)
            {
                m_ListGate[i].transform.GetChild(3).gameObject.SetActive(false);
                MoneyAllInGate[i] = 0;
            }
            foreach (var cardList in ListCardPlayer)
            {
                cardList.Clear();
            }
            ListChipMeGateBefore = new List<long>(new long[4]);
        });

    }


    protected override void Awake()
    {
        base.Awake();
        agTable = 0;
        if (TableView.instance != null)
        {
            handleUpdateJackpot(TableView.instance.currentJackpot);
        }
        DOTween.Sequence().AppendCallback(() =>
   {
       SocketSend.sendUpdateJackpot(Globals.Config.curGameId);
   }).AppendInterval(5.0f).SetLoops(-1).SetId("updateJackpotInGame");
        m_JackpotAnimA.Stop();
        m_JackpotAnimA.gameObject.SetActive(true);
        m_JackpotAnimA.Play();
        RectTransform parentRect = m_Rule.transform.parent.GetComponent<RectTransform>();
        m_Rule.transform.localPosition = new Vector2(parentRect.rect.width, 0);
        m_GroupBtnBet.gameObject.SetActive(false);
        m_GroupBtnPlay.gameObject.SetActive(false);
        for (int i = 0; i < m_ListGate.Count; i++)
        {
            if (i == 1)
            {
                continue;
            }
            m_ListGate[i].transform.GetChild(0).gameObject.SetActive(true);
            Button btn = m_ListGate[i].GetComponent<Button>();
            btn.interactable = false;
        }
        for (int i = 0; i < 6; i++)
        {
            ListCardPlayer.Add(new List<Card>());
            if (i < 4)
            {
                MoneyAllInGate.Add(new long());
                ListChipMeGateLast.Add(new long());
                ListChipMeGateBefore.Add(new long());
            }
        }

    }
    public void onClickRuleJP()
    {
        UIManager.instance.openRuleJPThreeCard();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        DOTween.Kill("updateJackpotInGame");
    }
    public bool CheckPokerPair(List<Card> listCard)
    {
        List<Card> listSorted = new List<Card>(listCard);

        listSorted.Sort((x, y) => x.N - y.N);
        for (int i = 0; i < listSorted.Count - 1; i++)
        {
            if (listSorted[i].N == listSorted[i + 1].N)
                return true;
        }
        return false;
    }
    public bool CheckPoker3Card(List<Card> listCard)
    {
        Card baseC = listCard[0];
        if (listCard.Count < 3) return false;
        for (int i = 0; i < listCard.Count; i++)
        {
            if (baseC.N != listCard[i].N) return false;
        }
        return true;
    }
    bool checkPokerStrFlush(List<Card> listCard)
    {
        return CheckPokerFlush(listCard) && CheckPokerStraight(listCard) ? true : false;
    }
    public bool CheckPokerFlush(List<Card> listCard)
    {
        Card baseC = listCard[0];
        for (int i = 1; i < listCard.Count; i++)
        {
            if (listCard[i].S != baseC.S) return false;
        }
        return true;
    }
    public bool CheckPokerStraight(List<Card> listCard)
    {
        List<Card> listSorted = new List<Card>(listCard);
        bool caseAce1 = true;
        bool caseAce14 = true;
        listSorted.Sort((x, y) => x.N - y.N);
        Card baseC = listSorted[0];
        for (int i = 1; i < listSorted.Count; i++)
        {
            if (listSorted[i].N != baseC.N + i)
            {
                caseAce14 = false;
                break;
            }
        }

        listSorted.ForEach(element =>
        {
            if (element.N == 14) element.N = 1;
        });
        listSorted.Sort((x, y) => x.N - y.N);
        baseC = listSorted[0];
        for (int i = 1; i < listSorted.Count; i++)
        {
            if (listSorted[i].N != baseC.N + i)
            {
                caseAce1 = false;
                break;
            }
        }
        if (caseAce1 || caseAce14) return true;
        return false;
    }
    private string getTextLabel(List<Card> listCard)
    {
        if (checkPokerStrFlush(listCard))
        {
            return "Straight Flush";
        }
        else if (CheckPokerFlush(listCard))
        {
            return "Flush";
        }
        else if (CheckPokerStraight(listCard))
        {
            return "Straight";
        }
        else if (CheckPoker3Card(listCard))
        {
            return "Three of a kind";
        }
        else if (CheckPokerPair(listCard))
        {
            return "Pair";
        }
        else
        {
            return "High Card";
        }

    }
    private IEnumerator ShowCardLabelWithDelay(Player player)
    {
        yield return new WaitForSeconds(0.8f);

        float scale = (player != null && player != thisPlayer) ? 0.5f : 1f;
        List<Vector2> listPos = player != null ? getListPositionCardPlayer(player) : new List<Vector2> { new Vector2(-150f, 245f), new Vector2(0f, 245f), new Vector2(150f, 245f) };
        float offset = (player == null) ? 55f : (player == thisPlayer) ? 120f : 60f;

        int index = (player == null) ? 5 : players.IndexOf(player);
        List<Card> cardList = ListCardPlayer[index];
        if (cardList.Count == 0 || cardList[0].code == 0)
        {
            yield break; // Dừng Coroutine nếu không có bài
        }
        string value = getTextLabel(cardList);
        GameObject item = BundleHandler.Instantiate(m_Label, m_ContainerLabel);
        listLabel.Add(item);
        LabelGameView labelGameView = item.GetComponent<LabelGameView>();
        item.transform.localPosition = new Vector2(listPos[1].x, listPos[1].y - offset);
        item.transform.localScale = new Vector2(scale, scale);
        bool isHighlight = value == "High Card" ? true : false;
        labelGameView.OnShow(value, isHighlight);
    }

    // Gọi Coroutine từ bên ngoài
    void showCardLabel(Player player)
    {
        StartCoroutine(ShowCardLabelWithDelay(player));
    }



    public void ShowPlayerPlayAnim(Player player, string anim)
    {
        if (player == null) return;

        // Instantiate prefab
        GameObject animNode = BundleHandler.Instantiate(m_AnimPlayFold, player.playerView.transform);
        animNode.SetActive(true);

        // Lấy component Animator (hoặc nếu dùng SkeletonAnimation thì thay bằng component thích hợp)
        SkeletonGraphic animComponent = animNode.GetComponentInChildren<SkeletonGraphic>();
        if (animComponent == null)
        {
            Debug.LogError("Animator component not found on the instantiated object.");
            return;
        }

        // Set animation (ở đây chúng ta giả sử 'anim' là tên của animation trong Animator Controller)
        animComponent.Initialize(true);
        animComponent.AnimationState.SetAnimation(0, anim, false);
        // Chạy coroutine để xử lý thời gian delay và hủy đối tượng
        StartCoroutine(DestroyAfterDelay(animNode, 2f));  // Thời gian delay là 2 giây
    }

    // Coroutine để hủy GameObject sau một khoảng thời gian
    private IEnumerator DestroyAfterDelay(GameObject animNode, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(animNode);  // Xóa đối tượng sau khi kết thúc thời gian delay
    }


}

