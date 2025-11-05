using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Globals;
using DG.Tweening;
using Spine.Unity;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json;

public class BaucuaGameView : GameView
{
    public static BaucuaGameView instance;
    [SerializeField] private GameObject m_Avatar_chung;
    [SerializeField] private TextMeshProUGUI m_MyChip;
    [SerializeField] private SkeletonGraphic m_AniStart;
    [SerializeField] private SkeletonGraphic m_AniClock;
    [SerializeField] private SkeletonGraphic m_AniXoc;
    [SerializeField] private SkeletonGraphic m_AniWin;
    [SerializeField] private SkeletonGraphic m_AniLose;
    [SerializeField] private Transform m_Popup;
    [SerializeField] private List<Button> m_ChipBet;
    [SerializeField] private TextMeshProUGUI m_Chipdeal;
    [SerializeField] private List<GameObject> m_Gatebet;
    [SerializeField] private Transform layerChip;
    [SerializeField] private GameObject m_PrepabChip;
    [SerializeField] private List<Image> m_ListXucSac;
    [SerializeField] private Button m_Deal;
    [SerializeField] private Button m_Cancel;
    [SerializeField] private TextMeshProUGUI m_LbWin;
    [SerializeField] private TextMeshProUGUI m_LbLose;
    [SerializeField] private GameObject m_TableHistory;
    [SerializeField] private GameObject m_Prefab_popup_player;
    [SerializeField] private GameObject m_ItemHistory;
    [SerializeField] private GameObject m_FrameHistory;

    [HideInInspector] public List<long> ListValueChip = new List<long>();
    public List<GameObject> chipBetPool = new List<GameObject>();
    public List<Dictionary<string, int>> ListNamePlayerInGate = new List<Dictionary<string, int>>();
    public List<long> DictionMeBetInGate = new List<long>();
    public List<long> DictionMeBetInGateLast = new List<long>();
    public List<long> ListGateAllMoney = new List<long>();
    public List<Player> ListPlayerBaucua = new List<Player>();
    public int PositionChipbet = 0;
    public List<List<int>> DataHistory = new List<List<int>>();
    public NodePlayerBaucua listPlayer = null;

    public BaucuaChipManager BaucuaChipManager;
    public TableHistory TableHistory;
    public List<int> GateWin = new List<int>();

    private List<long> TEMP_VALUE_GOLD_COINS = new List<long>
{
    1, 5, 10, 50, 100, 500, 1000, 5000, 10000, 50000,
    100000, 500000, 1000000, 5000000, 10000000, 50000000,
    100000000, 500000000, 1000000000, 5000000000
};

    public List<List<int>> ListBetDefine = new List<List<int>>()
{
    new List<int> { 1 },
      new List<int> { 2 },
        new List<int> { 3 },
          new List<int> { 4 },
            new List<int> { 5 },
              new List<int> { 6 },
                new List<int> { 1,2 },
                  new List<int> { 1,3 },
                    new List<int> { 1,4 },
                      new List<int> { 1 ,5},
                        new List<int> { 1,6 },
                          new List<int> { 2,3 },
                            new List<int> { 2,4 },
                              new List<int> { 2,5 },
                                new List<int> { 2,6 },
                                  new List<int> { 3,4 },
                                    new List<int> { 3,5 },
                                      new List<int> { 3,6 },
                                        new List<int> { 4,5 },
                                          new List<int> { 4,6 },
                                            new List<int> { 5,6 },
    };

    void updateValueChipBet()
    {
        // Kiểm tra người chơi và buttons
        if (thisPlayer == null || m_ChipBet.Count == 0)
        {
            Debug.Log("BauCuaLog->updateChipBet: Error - thisPlayer is null or no chip buttons");
            return;
        }

        long gold = thisPlayer.ag;

        // Tìm index phù hợp trong mảng TEMP_VALUE_GOLD_COINS
        int x = 4;  // Bắt đầu từ index 4
        for (int i = TEMP_VALUE_GOLD_COINS.Count - 1; i >= 0; i--)
        {
            long ag = TEMP_VALUE_GOLD_COINS[i];
            if (gold > ag)
            {
                x--;
                break;
            }
            x = i;
        }

        // Đảm bảo x không nhỏ hơn 4
        if (x < 4)
            x = 4;
        List<long> listValue = new List<long>();

        for (int i = 0; i < m_ChipBet.Count; i++)
        {
            long chipValue = TEMP_VALUE_GOLD_COINS[x - 4 + i];
            listValue.Add(chipValue);
        }
        ListValueChip = listValue;

    }
    protected override void updatePositionPlayerView()
    {
        ListPlayerBaucua.Clear();
        // Tạo danh sách người chơi mới để tránh trùng lặp
        List<Player> uniquePlayers = new List<Player>();
        HashSet<int> playerIds = new HashSet<int>();

        // Sắp xếp người chơi theo tiền
        for (int i = 0; i < players.Count - 1; i++)
        {
            if (players[i] == null) continue;
            for (int j = i + 1; j < players.Count; j++)
            {
                if (players[j] == null) continue;
                if (players[i].ag < players[j].ag)
                {
                    Player tempP = players[i];
                    players[i] = players[j];
                    players[j] = tempP;
                }
            }
        }

        // Tìm người chơi hiện tại
        Player playerP = players.Find(x => x.id == User.userMain.Userid);
        if (playerP != null) thisPlayer = playerP;

        // Thêm người chơi hiện tại vào đầu danh sách
        if (thisPlayer != null)
        {
            uniquePlayers.Add(thisPlayer);
            playerIds.Add(thisPlayer.id);
        }

        // Thêm các người chơi khác, tránh trùng lặp
        foreach (var player in players)
        {
            if (player != null && player != thisPlayer && !playerIds.Contains(player.id))
            {
                playerIds.Add(player.id);
                uniquePlayers.Add(player);

            }
        }

        // Cập nhật lại danh sách người chơi
        players.Clear();
        players.AddRange(uniquePlayers);
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == null || players[i].playerView == null) continue;
            players[i].playerView.transform.localScale = players[i] == thisPlayer ? new Vector2(0.8f, 0.8f) : new Vector2(0.7f, 0.7f);
            if (i >= 6)
            {
                m_Avatar_chung.SetActive(true);
                ListPlayerBaucua.Add(players[i]);
                players[i].playerView.gameObject.SetActive(false);
                players[i].playerView.transform.localPosition = m_Avatar_chung.transform.localPosition;
            }
            else
            {
                players[i].playerView.transform.localPosition = listPosView[i];
                players[i].updatePlayerView();
                players[i].playerView.gameObject.SetActive(true);
                players[i].updateItemVip(players[i].vip);
                m_Avatar_chung.SetActive(false);
            }

        }

        // Dọn dẹp các vị trí trống
        for (int i = 0; i < playerContainer.childCount; i++)
        {
            Transform child = playerContainer.GetChild(i);
            if (child.transform.localPosition == Vector3.zero)
            {
                Destroy(child.gameObject);
            }
        }
    }
    public void handleStart(string data)
    {
        Debug.Log("check xem từ đây là ban đầu như nào" + Config.isBackGame);
        m_LbWin.gameObject.SetActive(true);
        int timeData = 5;
        if (data != null)
        {
            timeData = int.Parse(data);
        }
        stateGame = STATE_GAME.WAITING;
        TweenCallback effectStart = () =>
        {
            m_AniStart.gameObject.SetActive(true);
            m_AniStart.Initialize(true);
            m_AniStart.AnimationState.SetAnimation(0, "start_myr", false);
            playSound(SOUND_HILO.START_GAME);
        };
        TweenCallback effectXocDia = () =>
        {
            m_AniStart.gameObject.SetActive(false);
            m_AniXoc.gameObject.SetActive(true);
            playSound(SOUND_HILO.DICE_SHAKE);
            m_AniXoc.Initialize(true);
            m_AniXoc.AnimationState.SetAnimation(0, "lac", false);
        };
        TweenCallback effectBetTime = () =>
        {
            m_AniStart.gameObject.SetActive(true);
            m_AniStart.Initialize(true);
            m_AniStart.AnimationState.SetAnimation(0, "bet_myr", false);
            DOTween.Sequence().AppendInterval(1.0f).AppendCallback(() =>
            {
                m_AniStart.gameObject.SetActive(false);
            });
        };
        TweenCallback effectAniClock = () =>
        {
            showClock(true, Mathf.FloorToInt(timeData / 1000) - 1);
            interactableButton(true);
        };
        DOTween.Sequence()
            .AppendCallback(effectStart)
            .AppendInterval(1.5f)
            .AppendCallback(effectXocDia)
            .AppendInterval(2.5f)
            .AppendCallback(effectBetTime)
            .AppendInterval(1f)
             .AppendCallback(effectAniClock);
    }
    public override void handleSTable(string data)
    {
        base.handleSTable(data);
        stateGame = STATE_GAME.WAITING;
        TableHistory.SetData(DataHistory);
        if (DataHistory.Count != 0)
        {
            setDataFrameHistory(DataHistory[DataHistory.Count - 1]);
        }
        else
        {
            setDataFrameHistory(new List<int>());
        }
        JObject jData = JObject.Parse(data);
        agTable = getInt(jData, "M");
        int timeData = getInt(jData, "T");
        if (timeData > 1)
        {
            interactableButton(true);
            showClock(true, timeData - 1);
        }
        updateValueChipBet();
        BaucuaChipManager.SetListValueChip(ListValueChip);
        SetValueInchip();
        JArray ArrP = getJArray(jData, "ArrP");

        for (int i = 0; i < ArrP.Count; i++)
        {
            JObject dataPlayer = (JObject)ArrP[i];
            Player player = getPlayerWithID(getInt(dataPlayer, "id"));
            if (player == thisPlayer)
            {
                m_MyChip.text = Globals.Config.FormatMoney(player.ag, true);
            }

            // Set current bet for players in table
            JArray playerBets = getJArray(dataPlayer, "Arr");
            if (playerBets != null)
            {
                foreach (JObject bet in playerBets)
                {
                    int gate = getInt(bet, "N");
                    int betAmount = getInt(bet, "M");
                    showAllChipBet(dataPlayer["N"].ToString(), gate, betAmount, player);
                }
            }
        }
        updatePositionPlayerView();
        m_Avatar_chung.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + ListPlayerBaucua.Count;
    }

    private void showAllChipBet(string playerName, int gate, int betAmount, Player player)
    {
        int numberGate = gate - 1;
        if (!ListNamePlayerInGate[numberGate].ContainsKey(playerName))
        {
            ListNamePlayerInGate[numberGate].Add(playerName, betAmount);
        }
        else
        {
            ListNamePlayerInGate[numberGate][playerName] += betAmount;
        }

        ListGateAllMoney[numberGate] += betAmount;
        GameObject spBet = m_Gatebet[numberGate].transform.GetChild(2).gameObject;
        spBet.SetActive(true);
        TextMeshProUGUI textLabelGate = spBet.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textLabelGate.text = Globals.Config.FormatMoney(ListGateAllMoney[numberGate], true) + "/" + ListNamePlayerInGate[numberGate].Count.ToString();

        // Sử dụng effectMoveChip để di chuyển chip đến các ô cược
        effectMoveChip(gate, betAmount, player);
    }
    public override void handleCTable(string data)
    {
        interactableButton(false);
        base.handleCTable(data);
        JObject jData = JObject.Parse(data);
        agTable = getInt(jData, "M");
        updateValueChipBet();
        BaucuaChipManager.SetListValueChip(ListValueChip);
        SetValueInchip();
        JArray ArrP = getJArray(jData, "ArrP");
        for (int i = 0; i < ArrP.Count; i++)
        {
            JObject dataPlayer = (JObject)ArrP[i];
            Player player = getPlayerWithID(getInt(dataPlayer, "id"));
            if (player == thisPlayer)
            {
                m_MyChip.text = Globals.Config.FormatMoney(player.ag, true);
            }
        }

        m_Avatar_chung.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + ListPlayerBaucua.Count;
    }
    public void OnEnable()
    {
        StartCoroutine(ShowButtons());
        IEnumerator ShowButtons()
        {
            foreach (Button btn in m_ChipBet)
            {
                yield return new WaitForSecondsRealtime(.1f);
                btn.gameObject.SetActive(true);
                btn.interactable = true;
                btn.transform.DOComplete();
                btn.transform.DOScale(new Vector2(1.2f, 1.2f), .05f).OnComplete(() => { btn.transform.DOScale(Vector2.one, .05f); });
            }
        }

    }
    public void SetValueInchip()
    {
        for (int i = 0; i < ListValueChip.Count; i++)
        {
            TextMeshProUGUI nodeText = m_ChipBet[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            nodeText.text = Globals.Config.FormatMoney(ListValueChip[i], true);
            nodeText.transform.localScale = new Vector2(1, 1);
            nodeText.color = Color.black;
        }
    }
    public BaucuaChipManager createChip(int positionChip, long valueChip)
    {
        GameObject go = Instantiate(m_PrepabChip, layerChip);
        BaucuaChipManager chipBet = go.GetComponent<BaucuaChipManager>();
        chipBet.SetValueChip(valueChip);
        chipBet.transform.SetSiblingIndex(transform.childCount - 2);
        chipBet.gameObject.SetActive(true);
        chipBetPool.Add(go); // Thêm chip vào danh sách chipBetPool
        return chipBet;
    }
    public void ChooseChip(GameObject chip)
    {
        SoundManager.instance.soundClick();
        // Vòng lặp ẩn tất cả các chip đã chọn
        for (int i = 0; i < m_ChipBet.Count; i++)
        {
            m_ChipBet[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        Button selectedButton = chip.GetComponent<Button>();

        if (selectedButton != null)
        {
            selectedButton.transform.GetChild(0).gameObject.SetActive(true);

            // Xác định vị trí (index) của chip trong m_ChipBet

            PositionChipbet = m_ChipBet.IndexOf(selectedButton);

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
    private bool HasEnoughMoney()
    {
        return thisPlayer.ag > 0;
    }
    public void ChooseGateBet(GameObject gate)
    {
        Image image = gate.transform.GetChild(1).GetComponent<Image>();
        FadeImageAlpha(image);
        int numberGate = m_Gatebet.IndexOf(gate);
        long betAmount = ListValueChip[PositionChipbet];
        if (!CanPlaceBet(betAmount))
        {
            return;
        }
        if (!HasEnoughMoney())
        {
            UIManager.instance.showToast("You do not have enough money to place this bet.");
            return;
        }
        if (thisPlayer.ag - DictionMeBetInGate.Sum() <= 0)
        {
            UIManager.instance.showToast("You do not have enough money to place this bet.");
            return;
        }
        if (thisPlayer.ag - DictionMeBetInGate.Sum() < betAmount)
        {
            long ag = thisPlayer.ag - DictionMeBetInGate.Sum();
            DictionMeBetInGate[numberGate] += ag;
            DictionMeBetInGateLast[numberGate] += ag;
        }
        else
        {
            DictionMeBetInGate[numberGate] += betAmount;
            DictionMeBetInGateLast[numberGate] += betAmount;
        }
        GameObject spBet = gate.transform.GetChild(0).gameObject;
        spBet.SetActive(true);
        TextMeshProUGUI textLabelGate = spBet.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textLabelGate.text = Globals.Config.FormatMoney(DictionMeBetInGateLast[numberGate], true);
        m_Chipdeal.text = Globals.Config.FormatMoney(DictionMeBetInGate.Sum(), true);
    }
    private void FadeImageAlpha(Image image)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
        image.DOFade(1f, 0.1f).OnComplete(() =>
        {
            DOVirtual.DelayedCall(0.04f, () =>
            {
                image.DOFade(0f, 0.1f);
            });
        });
    }
    private void showClock(bool isShow, int timeClock)
    {
        //aniClock.ga.stopAllActions();
        m_AniClock.gameObject.SetActive(false);
        if (isShow)
        {
            m_AniClock.gameObject.SetActive(true);
            int del = 1;
            TweenCallback time = () =>
            {
                //timeClock -= Config.;
                //require("GameManager").getInstance().time_out_game = 0;

                m_AniClock.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = timeClock.ToString();
                if (timeClock > 0)
                {
                    if (timeClock == 3) Config.Vibration();
                    timeClock--;
                    playSound(timeClock > 5 ? SOUND_GAME.TICKTOK : SOUND_GAME.CLOCK_HURRY);
                    m_AniClock.Initialize(true);
                    m_AniClock.AnimationState.SetAnimation(0, "animation", false);
                }
                else
                {
                    //aniClock.node.stopAction();
                    m_AniClock.gameObject.SetActive(false);
                }
            };
            DOTween.Sequence()
                .AppendCallback(time)
                .AppendInterval(del)
                .SetLoops(timeClock + 1);
        }
        else
        {
            timeClock = -1;
        }
    }
    public void handleBetGame(JObject data)
    {
        // ném chip và set label ô gate
        playSound(SOUND_GAME.BET);
        List<int> positionGates = data["Num"].ToString().Split(';').Select(int.Parse).ToList();
        List<int> valueBets = data["M"].ToString().Split(';').Select(int.Parse).ToList();
        string namePlayer = data["N"].ToString();
        Player player = getPlayer(getString(data, "N"), true);
        if (player == thisPlayer)
        {
            stateGame = STATE_GAME.PLAYING;
        }
        for (int i = 0; i < positionGates.Count; i++)
        {
            int betAmount = valueBets[i];

            // Kiểm tra nếu có thể đặt cược


            if (!ListNamePlayerInGate[positionGates[i] - 1].ContainsKey(namePlayer))
            {
                ListNamePlayerInGate[positionGates[i] - 1].Add(namePlayer, betAmount);
            }
            else
            {
                ListNamePlayerInGate[positionGates[i] - 1][namePlayer] += betAmount;
            }
            player.ag -= betAmount;
            player.setAg();
            effectMoveChip(positionGates[i], betAmount, player);
        }
    }


    void effectMoveChip(int gate, int valueBet, Player player)
    {
        GameObject gateBet = m_Gatebet[gate - 1];
        GameObject spBet = gateBet.transform.GetChild(2).gameObject;
        spBet.SetActive(true);
        ListGateAllMoney[gate - 1] += valueBet;
        TextMeshProUGUI textLabelGate = spBet.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textLabelGate.text = Globals.Config.FormatMoney(ListGateAllMoney[gate - 1], true) + "/" + ListNamePlayerInGate[gate - 1].Count.ToString();
        BaucuaChipManager go = createChip(PositionChipbet, valueBet);
        Vector2 startPos = player.playerView.transform.localPosition;
        Vector2 endPos = gateBet.transform.localPosition;
        go.transform.position = startPos;
        go.transform.localScale = new Vector2(0.5f, 0.5f);
        MoveChipWithDOTween(go, startPos, endPos);
    }

    private void MoveChipWithDOTween(BaucuaChipManager chip, Vector2 startPos, Vector2 endPos)
    {
        chip.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        chip.transform.localPosition = startPos;
        Vector2 direction = (endPos - startPos).normalized;
        float offsetDistance = 30f;
        Vector2 offsetPosition = endPos - direction * offsetDistance;

        // Thêm lệch vị trí ngẫu nhiên cho endPos
        float randomOffsetX = Random.Range(-15f, 15f);
        float randomOffsetY = Random.Range(-15f, 15f);
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
    private void MoveChipFast(BaucuaChipManager chip, Vector2 startPos, Vector2 endPos)
    {
        chip.transform.localPosition = startPos;
        float moveDuration = 0f; // Điều chỉnh thời gian di chuyển để chip di chuyển chậm hơn

        DOTween.Sequence()
         .AppendInterval(moveDuration)
         .Append(chip.transform.DOLocalMove(endPos, 0.6f).SetEase(Ease.OutSine))
     .OnComplete(() =>
     {
         chip.gameObject.SetActive(false); // Vô hiệu hóa chip sau khi di chuyển đến endPos
     });
    }

    private void interactableButton(bool isInteractable)
    {
        foreach (GameObject btn in m_Gatebet)
        {
            btn.GetComponent<Button>().interactable = isInteractable;
        }
        m_Deal.interactable = isInteractable;
        m_Cancel.interactable = isInteractable;
    }
    void setDataFrameHistory(List<int> ints)
    {

        TableHistory.SetData(DataHistory);

        if (m_FrameHistory.transform.childCount != 0)
        {
            GameObject item = m_FrameHistory.transform.GetChild(0).gameObject;

            ItemHistory itemHistory = item.GetComponent<ItemHistory>();
            if (ints.Count != 0)
            {
                itemHistory.SetSquarabet(ints);
                item.SetActive(true);
            }
            else
            {
                item.SetActive(false);
            }
        }
        else
        {
            GameObject item = Instantiate(m_ItemHistory, m_FrameHistory.transform);

            // item.transform.localPosition = new Vector2(7, -37);
            ItemHistory itemHistory = item.GetComponent<ItemHistory>();
            if (ints.Count != 0)
            {
                itemHistory.SetSquarabet(ints);
                item.SetActive(true);
            }
            else
            {
                item.SetActive(false);
            }
        }
    }
    private void ShowDiceResults(List<int> ints)
    {
        for (int i = 0; i < m_ListXucSac.Count; i++)
        {
            m_ListXucSac[i].gameObject.SetActive(true);
            foreach (Transform child1 in m_ListXucSac[i].transform) { child1.gameObject.SetActive(false); }
            m_ListXucSac[i].transform.GetChild(ints[i] - 1).gameObject.SetActive(true);
        }
    }


    private void UpdateMyWinloseDisplay(long moneyChange)
    {

        if (moneyChange > 0)
        {
            playSound(SOUND_HILO.WIN);
            m_LbWin.text = "+" + Config.FormatMoney(moneyChange, true);
            m_AniWin.gameObject.SetActive(true);
            m_AniWin.Initialize(true);
            m_AniWin.AnimationState.SetAnimation(0, "cam", false);
        }
        else if (moneyChange < 0)
        {
            playSound(SOUND_HILO.LOSE);
            m_LbLose.text = Config.FormatMoney(moneyChange, true);
            m_AniLose.gameObject.SetActive(true);
            m_AniLose.Initialize(true);
            m_AniLose.AnimationState.SetAnimation(0, "cam", false);
        }
        else
        {

            m_AniWin.gameObject.SetActive(false);
            m_AniLose.gameObject.SetActive(false);
        }
    }

    public void OnclickDeal()
    {
        string N = "";
        string M = "";

        SoundManager.instance.soundClick();

        // Tạo lại giá trị cho N và M
        for (int i = 0; i < DictionMeBetInGate.Count; i++)
        {
            if (DictionMeBetInGate[i] != 0)
            {
                N += (i + 1).ToString() + ";";
                M += DictionMeBetInGate[i].ToString() + ";";
            }
        }

        N = N.TrimEnd(';');
        M = M.TrimEnd(';');
        SocketSend.sendBetBaucua(M, N);
        thisPlayer.updatePlayerView();
        m_MyChip.text = Globals.Config.FormatMoney(thisPlayer.ag - DictionMeBetInGate.Sum(), true);
        ResetBetValuesAfterDeal();
    }
    public void ClickCancel()
    {
        SoundManager.instance.soundClick();
        cancelValueBet();
        ResetBetValuesAfterDeal();
    }
    private void cancelValueBet()
    {
        m_Chipdeal.text = "0";
        for (int i = 0; i < DictionMeBetInGate.Count; i++)
        {
            if (DictionMeBetInGate[i] != 0)
            {
                GameObject spBet = m_Gatebet[i].transform.GetChild(0).gameObject;
                TextMeshProUGUI textLabelGate = spBet.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                if (DictionMeBetInGateLast[i] - DictionMeBetInGate[i] > 0)
                {
                    DictionMeBetInGateLast[i] = DictionMeBetInGateLast[i] - DictionMeBetInGate[i];

                    textLabelGate.text = Globals.Config.FormatMoney(DictionMeBetInGateLast[i], true);
                }
                else
                {
                    DictionMeBetInGateLast[i] = DictionMeBetInGateLast[i] - DictionMeBetInGate[i];
                    spBet.SetActive(false);
                    textLabelGate.text = "0";
                }
                DictionMeBetInGate[i] = 0;
            }
        }
    }

    private void ResetBetValuesAfterDeal()
    {
        m_Chipdeal.text = "0";
        for (int i = 0; i < DictionMeBetInGate.Count; i++)
        {
            DictionMeBetInGate[i] = 0;
        }
    }
    private void ResetBetValuesAferFinish()
    {
        GateWin.Clear();
        stateGame = STATE_GAME.WAITING;
        m_Chipdeal.text = "0";
        for (int i = 0; i < DictionMeBetInGate.Count; i++)
        {
            DictionMeBetInGate[i] = 0;
            DictionMeBetInGateLast[i] = 0;
            ListGateAllMoney[i] = 0;
            ListNamePlayerInGate[i].Clear(); // Xóa thông tin tiền cược của từng người chơi trong mỗi cửa
        }

        foreach (GameObject gate in m_Gatebet)
        {

            gate.transform.GetChild(0).gameObject.SetActive(false);
            gate.transform.GetChild(2).gameObject.SetActive(false);
        }
        foreach (var chip in chipBetPool)
        {
            chip.SetActive(false); // Vô hiệu hóa chip
            Destroy(chip); // Hoặc xóa chip nếu không cần tái sử dụng
        }
        chipBetPool.Clear(); // Xóa danh sách chip
        HandleData.DelayHandleLeave = 0f;
    }


    public void ShowHistoryGame(bool isTrue)
    {
        SocketSend.sendHistoryBaucua();
        if (m_TableHistory == null)
        {
            Debug.LogError("m_TableHistory is not assigned.");
            return;
        }
        GameObject instance = null;
        foreach (Transform child in m_Popup)
        {
            if (child.gameObject.name == m_TableHistory.name)
            {
                instance = child.gameObject;
                break;
            }
        }// Tìm đối tượng theo tên prefab

        if (instance == null)
        {
            instance = Instantiate(m_TableHistory, m_Popup);
            instance.name = m_TableHistory.name;
        }

        instance.transform.localPosition = Vector2.zero;
        instance.transform.localScale = Vector2.one;
        instance.SetActive(isTrue);
        TableHistory = instance.GetComponent<TableHistory>();
        TableHistory.SetData(DataHistory);
    }

    public void handleHistory(JObject data)
    {
        try
        {
            DataHistory = JsonConvert.DeserializeObject<List<List<int>>>(data["data"].ToString());
            TableHistory.SetData(DataHistory);
            if (DataHistory.Count != 0)
            {
                setDataFrameHistory(DataHistory[DataHistory.Count - 1]);
            }
            else
            {
                setDataFrameHistory(new List<int>());
            }
        }
        catch (JsonException e)
        {
            Debug.LogError("JSON Parsing Error: " + e.Message);
        }
    }
    public override void handleJTable(string objData)
    {
        base.handleJTable(objData);
        m_Avatar_chung.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + ListPlayerBaucua.Count;
    }
    public override void handleLTable(JObject objData)
    {
        base.handleLTable(objData);
        m_Avatar_chung.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + ListPlayerBaucua.Count;
        playSound(SOUND_GAME.REMOVE);
    }

    public void onClickShowPlayer()
    {
        //if (listPlayer == null || buttonBet != null)
        listPlayer = Instantiate(m_Prefab_popup_player, transform).GetComponent<NodePlayerBaucua>();
        listPlayer.transform.SetSiblingIndex((int)GAME_ZORDER.Z_MENU_VIEW);

    }


    protected override void Awake()
    {
        base.Awake();
        SocketSend.sendHistoryBaucua();
        PositionChipbet = 0;
        agTable = 0;
        GateWin = new List<int>();
        m_Avatar_chung.SetActive(false);
        interactableButton(false);
        for (int i = 0; i < 21; i++)
        {
            ListNamePlayerInGate.Add(new Dictionary<string, int>());
            DictionMeBetInGate.Add(new int());
            DictionMeBetInGateLast.Add(new int());
            ListGateAllMoney.Add(new int());
        }
        ShowHistoryGame(false);
        TableHistory.SetData(DataHistory);
        if (DataHistory.Count > 0)
        {
            setDataFrameHistory(DataHistory[DataHistory.Count - 1]);
        }
        else
        {
            setDataFrameHistory(new List<int>());
        }
    }
    private bool CanPlaceBet(long betAmount)
    {
        // Kiểm tra nếu tổng số tiền cược hiện tại cộng với số tiền cược mới vượt quá giới hạn cho phép
        if (DictionMeBetInGateLast.Sum() + betAmount > agTable * 100)
        {
            string msg = Config.getTextConfig("txt_max_bet");
            UIManager.instance.showToast(msg);
            return false;
        }
        return true;
    }
    public void handleFinish(JObject data)
    {
        JArray dataArray = JArray.Parse(data["data"].ToString());
        foreach (JToken item in dataArray)
        {
            if (item["N"].ToString().Equals(thisPlayer.namePl) && (long)item["M"] != 0)
            {
                HandleData.DelayHandleLeave = 8f;
                break;
            }
        }
        try
        {
            // 1. Initial Reset and Disable

            // 2. Get Dice Results and Winning Gates
            int dice1 = int.Parse(data["dice1"].ToString());
            int dice2 = int.Parse(data["dice2"].ToString());
            int dice3 = int.Parse(data["dice3"].ToString());

            getGateWinToListData(dice1, dice2, dice3);
            List<int> ints = new List<int> { dice1, dice2, dice3 };
            ints.Sort();

            Debug.Log("check data trả về cho phần finish " + dice1.ToString() + dice2.ToString() + dice3.ToString());


            StartCoroutine(FinishSequence(data, GateWin, ints));
            setDataFrameHistory(ints);
            DataHistory.Add(ints);
            TableHistory.SetData(DataHistory);

            // Using coroutine to manage delays
        }
        catch (JsonReaderException e)
        {
            Debug.LogError("JSON Parsing Error: " + e.Message);
        }
    }
    private void getGateWinToListData(int dice1, int dice2, int dice3)
    {
        GateWin.AddRange(new int[] { dice1, dice2, dice3 });
        for (int i = 0; i < 3; i++)
        {

            int diceComp1 = 0;
            int diceComp2 = 0;
            if (i == 0)
            {
                diceComp1 = dice1;
                diceComp2 = dice2;
            }
            else if (i == 1)
            {
                diceComp1 = dice2;
                diceComp2 = dice3;
            }
            else if (i == 2)
            {
                diceComp1 = dice1;
                diceComp2 = dice3;
            }
            if (diceComp1 == diceComp2) continue;
            for (int j = 0; j < ListBetDefine.Count; j++)
            {
                List<int> gateDefine = ListBetDefine[j];
                if (gateDefine.IndexOf(diceComp1) != -1 && gateDefine.IndexOf(diceComp2) != -1)
                {

                    GateWin.Add(j + 1);
                }
            }
        }
    }

    private IEnumerator FinishSequence(JObject data, List<int> listTableWin, List<int> result)
    {
        interactableButton(false);
        JArray dataArray = JArray.Parse(data["data"].ToString());
        m_AniStart.gameObject.SetActive(true);
        m_AniStart.Initialize(true);
        m_AniStart.AnimationState.SetAnimation(0, "open_myr", false);
        cancelValueBet();
        yield return new WaitForSeconds(1.0f);
        ShowDiceResults(result);

        // Effect 2: show result dice
        m_AniStart.gameObject.SetActive(false);
        m_AniXoc.gameObject.SetActive(true);
        playSound(SOUND_HILO.DICE_OPEN);
        m_AniXoc.Initialize(true);
        m_AniXoc.AnimationState.SetAnimation(0, "open", false);
        yield return new WaitForSeconds(1.0f);

        // Effect 3: show gate win
        foreach (int gateIndex in listTableWin)
        {
            Image image = m_Gatebet[gateIndex - 1].transform.GetChild(1).GetComponent<Image>();
            image.gameObject.SetActive(true); // Ensure image is active

            // Flash effect
            image.DOFade(1f, 0.2f)
                 .SetLoops(6, LoopType.Yoyo)
                 .OnComplete(() =>
                 {
                     image.color = new Color(image.color.r, image.color.g, image.color.b, 1f); // Set alpha to 1
                 });

            // Wait for 2 seconds and then fade out to 0
            DOVirtual.DelayedCall(4f, () =>
            {
                image.DOFade(0f, 0.3f); // Fade out to 0
            });
        }
        //  genAgLose();

        yield return new WaitForSeconds(1.0f); // Adjust delay as needed
        foreach (var dice in m_ListXucSac)
        {
            dice.gameObject.SetActive(false);
        }
        m_AniXoc.AnimationState.SetAnimation(0, "khong lac", false);

        // Giai đoạn 1: Tạo chip và di chuyển chúng đến các ô thắng cược
        foreach (int gateIndex in listTableWin)
        {
            CreateAndMoveChipsToWinningGate(gateIndex);
        }

        yield return new WaitForSeconds(2.0f); // Adjust delay as needed
        // Giai đoạn 2: Tạo chip để bay về phía người thắng cược và cập nhật giá trị chip của người chơi
        foreach (JObject playerResult in dataArray)
        {
            string playerName = (string)playerResult["N"];
            int moneyChange = (int)playerResult["M"];

            Player player = getPlayer(playerName, true);
            if (player == null)
            {
                Debug.LogError("Player not found: " + playerName);
                continue;
            }
            player.playerView.agWin = (int)playerResult["AG"] - player.ag;
            if (player.playerView.agWin > 0)
            {
                player.playerView.effectFlyMoney(player.playerView.agWin, 40);
            }
            // Tạo chip để bay về phía người thắng cược nếu họ thắng cược
            if (moneyChange > 0)
            {
                CreateAndMoveChipsToPlayer(player, moneyChange);
            }
            // Update player's money
            player.ag = (int)playerResult["AG"]; // Cập nhật số tiền mới của người chơi
            player.setAg();
            if (player == thisPlayer)
            {
                m_MyChip.text = Globals.Config.FormatMoney(player.ag, true);
            }
        }

        yield return new WaitForSeconds(2.0f);
        ResetBetValuesAferFinish();
        ClickCancel(); // clear the bets
        OnEnable(); // restore the betting chips

        // Hiển thị animation thắng/thua
        foreach (JObject playerResult in dataArray)
        {
            string playerName = (string)playerResult["N"];
            long moneyChange = (long)playerResult["M"];
            Player player = getPlayer(playerName, true);
            if (player == thisPlayer)
            {
                UpdateMyWinloseDisplay(moneyChange > 0 ? player.playerView.agWin : moneyChange); // Assuming this function is defined already.
            }
        }
        yield return new WaitForSeconds(3f);
        m_LbWin.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        m_AniWin.gameObject.SetActive(false);
        m_AniLose.gameObject.SetActive(false);
        foreach (var chip in chipBetPool)
        {
            if (chip != null)
            {
                chip.transform.DOMove(Vector2.zero, 0.1f).OnComplete(() => chip.SetActive(false));
            }
        }
        Debug.Log("có chạy vào hàm checkExit" + Config.isBackGame);
        checkAutoExit();
    }

    private void genAgLose()
    {
        for (int i = 0; i < players.Count; i++)
        {
            PlayerView player = players[i].playerView;

            player.agLose = 0 - getMoneyToUser(players[i].displayName);

            if (player.agLose < 0)
            {
                player.effectFlyMoney(player.agLose, 40);
                playSound(SOUND_HILO.CHIP_LOSER);
            }
        }
    }
    private long getMoneyToUser(string name)
    {
        long money = 0;
        for (int i = 0; i < ListNamePlayerInGate.Count; i++)
        {
            if (ListNamePlayerInGate[i].ContainsKey(name))
            {
                money += ListNamePlayerInGate[i][name];
            }
        }
        return money;
    }
    private void CreateAndMoveChipsToWinningGate(int gateIndex)
    {
        // Chỉ tạo chip trả thưởng nếu ô này có cược
        if (ListGateAllMoney[gateIndex - 1] > 0)
        {
            GameObject gateBet = m_Gatebet[gateIndex - 1];
            long totalBet = ListGateAllMoney[gateIndex - 1];
            long chipValue = totalBet * 2; // Giá trị chip là tổng ô cược nhân x2
            BaucuaChipManager chip = createChip(PositionChipbet, chipValue);
            Vector2 startPos = Vector2.zero;
            Vector2 endPos = gateBet.transform.localPosition;
            chip.transform.position = startPos;
            chip.transform.localScale = new Vector2(0.7f, 0.7f);
            MoveChipWithDOTween(chip, startPos, endPos);
        }
    }

    private void CreateAndMoveChipsToPlayer(Player player, int moneyChange)
    {
        // Tạo chip để bay về phía người thắng cược
        BaucuaChipManager chip = createChip(PositionChipbet, moneyChange); // Giá trị chip có thể thay đổi tùy theo logic của bạn
        Vector2 startPos = Vector2.zero; // Vị trí bắt đầu là (0, 0)
        Vector2 endPos = player.playerView.transform.localPosition;
        chip.transform.position = startPos;
        chip.transform.localScale = new Vector2(0.7f, 0.7f);
        MoveChipFast(chip, startPos, endPos);
    }

}
