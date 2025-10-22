using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class XocdiaView : GameView
{
    public static XocdiaView instance;
    [SerializeField] private GameObject m_AvatarChung;
    [SerializeField] private GameObject m_Prefab_popup_player;
    [SerializeField] private GameObject m_PopupRule;
    [SerializeField] private TextMeshProUGUI m_TxtRule;
    public TextMeshProUGUI lb_noti;
    [SerializeField] private List<Button> m_ChipBet;
    public List<Player> listPlayerXocdia = new List<Player>();
    public List<GameObject> chipResultControl;
    public NodePlayerXocdia NodeListPlayer = null;
    private List<int> ListValueChip = new List<int>();
    private Queue<ChipXocDia> chipPool = new Queue<ChipXocDia>();
    [SerializeField] private Transform m_ContainerChip;
    [SerializeField] private GameObject m_ChatInGame;

    public int PositionChipbet = 0;
    public List<GameObject> listBtnBet;
    public ChipXocDia chipForFly;
    public GameObject historyChildren;
    public XocDiaHistory xocdiaHistory;
    public Transform historyNode;
    public TextMeshProUGUI[] historyLabels;
    public GameObject bowlControl;
    public GameObject plateControl;
    public GameObject timerControl;
    public GameObject nodeSellPot;
    public GameObject nodeBuyPot;
    public GameObject nodeNotify;
    public GameObject iconBankerDealer;
    public GameObject bg_bet;
    public GameObject btn_BecomeBanker;
    public GameObject btn_CancelBanker;

    public Button btn_Double;
    public Sprite chipVang;
    public Sprite chipTrang;
    public Sprite titleRed;
    public Sprite titlePurple;
    public List<TextMeshProUGUI> arrayLabelTop;
    public List<TextMeshProUGUI> arrayLabelBottom;
    public SkeletonGraphic animStart;
    public SkeletonGraphic animBet;
    public SkeletonGraphic animBat;
    public SkeletonGraphic animDealer;
    private List<Vector2> buttonBetVector = new List<Vector2>();
    private XocDiaHistory popupHistory;
    private GameObject bkgNoti;
    private int? lastHistoryInstanceResult;
    private Vector3 bowlPos;
    private Vector3 platePos;

    private int timeToBet = 18;
    private int CurrentChipSelected = 0;
    private int historySprieState = 0;
    private int HISTORY_LAST_RESULT_COL = 0;
    private int HISTORY_LAST_RESULT_ROW = 0;
    private int HISTORY_STREAK = 0;
    private int? HISTORY_COL_BEFORE_STREAK = 0;
    private int idCurrentDealer = 0;
    private int numberPotSell = -1;
    private int spamDouble = 0;
    private int dealerId = 0;
    private bool isXoc = false;
    private bool HISTORY_STATE_IS_VERTICAL = false;
    private int? HISTORY_LAST_VALUE = null;

    public ChipXocDia XocDiaChipManager;
    private List<int> gateValue = new List<int> { 0, 0, 0, 0, 0, 0 };
    private List<int> selfGateValue = new List<int> { 0, 0, 0, 0, 0, 0 };
    private bool isFinish = false;
    private List<List<ChipXocDia>> gateNumber = new List<List<ChipXocDia>>()
{
    new List<ChipXocDia>(),
    new List<ChipXocDia>(),
    new List<ChipXocDia>(),
    new List<ChipXocDia>(),
    new List<ChipXocDia>(),
    new List<ChipXocDia>()
};

    private List<List<int>> saveListBetted = new List<List<int>>
{
    new List<int> { 0, 1 },
    new List<int> { 0, 2 },
    new List<int> { 0, 3 },
    new List<int> { 0, 4 },
    new List<int> { 0, 5 },
    new List<int> { 0, 6 }
};

    private List<List<int>> historyArray = new List<List<int>>();

    private string dataHistory;
    private int acceptBanker = 100;
    private List<int> TEMP_VALUE_GOLD_COINS = new List<int> {  1,
            20,
            100,
            500,
            1000,
            5000,
            10000,
            50000,
            100000 };
    private class AniObj
    {
        public string animStart;
        public string animBet;
    }
    private void InitHistoryArray(int numRows = 6, int numCols = 50)
    {
        historyArray = new List<List<int>>();

        for (int i = 0; i < numRows; i++)
        {
            List<int> row = new List<int>();
            for (int j = 0; j < numCols; j++)
            {
                row.Add(0);
            }
            historyArray.Add(row);
        }
    }

    public void Awake()
    {
        base.Awake();
        instance = this;
        agTable = 100;
        ListValueChip = new List<int> { agTable, agTable * 2, agTable * 5, agTable * 10, agTable * 20 };
        InitHistoryArray();
        this.timeToBet = 18;
        thisPlayer = null;
        gateValue = new List<int> { 0, 0, 0, 0, 0, 0 };
        selfGateValue = new List<int> { 0, 0, 0, 0, 0, 0 };
        gateNumber = new List<List<ChipXocDia>>
{
    new List<ChipXocDia>(),
    new List<ChipXocDia>(),
    new List<ChipXocDia>(),
    new List<ChipXocDia>(),
    new List<ChipXocDia>(),
    new List<ChipXocDia>()
};
        bowlPos = bowlControl.transform.position;
        platePos = plateControl.transform.localPosition;
        isXoc = false;
        CurrentChipSelected = 0;
        nodeSellPot.SetActive(false);
        nodeBuyPot.SetActive(false);
        nodeNotify.SetActive(false);
        btn_Double.interactable = false;
        nodeBuyPot.transform.SetAsLastSibling(); // giống zIndex
        nodeNotify.transform.SetAsLastSibling();
        dataHistory = "";
        getButtonBetVector();
        XocDiaChipManager.SetListValueChip(ListValueChip);
        for (int i = 0; i < 80; i++)
        {
            var chip = Instantiate(chipForFly, m_ContainerChip);
            if (chip != null)
            {
                chip.GetComponent<ChipXocDia>().SetValueChip(TEMP_VALUE_GOLD_COINS[0]); // Set giá trị mặc định là mức thấp nhất
                chip.gameObject.SetActive(false);
                chipPool.Enqueue(chip);
            }
        }
    }

    protected override void updatePositionPlayerView()
    {
        listPlayerXocdia = new List<Player>();

        players.Sort((a, b) =>
        {
            return b.ag.CompareTo(a.ag);
        });


        for (int i = 0; i < players.Count; i++)
        {
            if (thisPlayer == players[i])
            {
                players.RemoveAt(i);
                break;
            }
        }
        players.Insert(0, thisPlayer);
        for (int i = 0; i < players.Count; i++)
        {

            if (players[i] == null || players[i].playerView == null) continue;
            players[i].playerView.transform.localScale = players[i] == thisPlayer ? new Vector2(0.8f, 0.8f) : new Vector2(0.7f, 0.7f);
            if (i >= 8)
            {
                listPlayerXocdia.Add(players[i]);
                players[i].playerView.gameObject.SetActive(false);
                players[i].playerView.transform.localPosition = m_AvatarChung.transform.localPosition;
            }
            else
            {
                players[i].playerView.transform.localPosition = listPosView[i];
                players[i].updatePlayerView();
                players[i].playerView.gameObject.SetActive(true);
                players[i].updateItemVip(players[i].vip);
            }

        }
    }
    public void SetValueInchip()
    {
        for (int i = 0; i < ListValueChip.Count; i++)
        {
            TextMeshProUGUI nodeText = m_ChipBet[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            nodeText.text = Globals.Config.FormatMoney2(ListValueChip[i], true, true);
            nodeText.transform.localScale = new Vector2(1, 1);
        }
    }
    public void ChooseChip(GameObject chip)
    {
        SoundManager.instance.soundClick();
        for (int i = 0; i < m_ChipBet.Count; i++)
        {
            m_ChipBet[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        Button selectedButton = chip.GetComponent<Button>();

        if (selectedButton != null)
        {
            selectedButton.transform.GetChild(0).gameObject.SetActive(true);
            PositionChipbet = m_ChipBet.IndexOf(selectedButton);
        }
    }
    public void onClickShowPlayer()
    {
        NodeListPlayer = Instantiate(m_Prefab_popup_player, transform).GetComponent<NodePlayerXocdia>();
        NodeListPlayer.transform.SetSiblingIndex(300);

    }
    public void onClickDouble(GameObject eventObject)
    {
        if (saveListBetted.Count == 0) return;

        Button btn = eventObject.GetComponent<Button>();
        btn.interactable = false;
        for (int i = 0; i < saveListBetted.Count; i++)
        {
            if (saveListBetted[i][0] > 0)
            {
                sendBet(saveListBetted[i][0], saveListBetted[i][1]);
            }

            saveListBetted[i][0] += saveListBetted[i][0]; // x2
        }
        DOVirtual.DelayedCall(1.5f, () =>
        {
            btn.interactable = true;
        });
    }
    public void sendBet(int Money, int Gate)
    {
        if (Money <= thisPlayer.ag && Money > 0)
        {
            SocketSend.sendBetXocDia(Money, Gate);
        }
        else
        {
            if (thisPlayer != null)
            {
                SocketSend.sendBetXocDia((int)thisPlayer.ag, Gate);
            }
        }
    }
    public void onClickCancelBecomdealer()
    {
        SocketSend.sendCancelDealer();
        btn_CancelBanker.SetActive(false);
    }
    public override void handleCTable(string strData)
    {
        base.handleCTable(strData);
        JObject data = JObject.Parse(strData);
        agTable = (int)data["M"];
        acceptBanker = (int)data["F"];
        JToken betsToken = data["bets"];
        if (betsToken != null && betsToken.Type == JTokenType.Array && ((JArray)betsToken).Count > 0)
        {
            JArray betsArray = (JArray)betsToken;
            ListValueChip = betsArray.ToObject<List<int>>();

        }
        else
        {
            ListValueChip = new List<int> { agTable, agTable * 2, agTable * 5, agTable * 10, agTable * 20 };
        }

        XocDiaChipManager.SetListValueChip(ListValueChip);
        stateGame = Globals.STATE_GAME.WAITING;
        SetValueInchip();
        SetStateButton(false);
        m_AvatarChung.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + listPlayerXocdia.Count.ToString();
    }
    public void SetStateButton(bool isInteractable)
    {
        foreach (GameObject btn in listBtnBet)
        {
            btn.SetActive(isInteractable);
        }
        if (listBtnBet != null)
        {
            foreach (GameObject btn in listBtnBet)
            {
                btn.GetComponent<Button>().interactable = isInteractable;
            }
        }
    }
    private ChipXocDia GetChipFromPool()
    {
        if (chipPool.Count > 0)
        {
            var chip = chipPool.Dequeue();
            chip.gameObject.SetActive(true);
            chip.gameObject.transform.SetParent(m_ContainerChip);
            return chip;
        }
        return Instantiate(chipForFly, m_ContainerChip);
    }
    public override void handleRJTable(string strData)
    {
        base.handleRJTable(strData);
        JObject data = JObject.Parse(strData);
        agTable = (int)data["M"];
        JToken betsToken = data["bets"];
        if (betsToken != null && betsToken.Type == JTokenType.Array && ((JArray)betsToken).Count > 0)
        {
            JArray betsArray = (JArray)betsToken;
            ListValueChip = betsArray.ToObject<List<int>>();
        }
        else
        {
            ListValueChip = new List<int> { agTable, agTable * 2, agTable * 5, agTable * 10, agTable * 20 };
        }
        SetValueInchip();
        XocDiaChipManager.SetListValueChip(ListValueChip);
        acceptBanker = (int)data["F"];
        stateGame = (bool)data["isWaitNewGame"] ? Globals.STATE_GAME.WAITING : Globals.STATE_GAME.VIEWING;
        timeToBet = (int)data["T"];
        SetTimer();
        JArray listPlayer = (JArray)data["ArrP"];
        foreach (JObject playerData in listPlayer)
        {
            SetupBetting(playerData);
        }
        SetStateButton((int)data["T"] > 0);

        if (data["H"] != null && ((string)data["H"]).Length > 0)
        {
            dataHistory += (string)data["H"];
        }
        int len = dataHistory.Length;
        if (len >= 80)
        {
            ImportHistory(dataHistory.Substring(len - 80));
        }
        else
        {
            ImportHistory(dataHistory);
        }
        if ((int)data["dealerId"] != 0)
        {
            this.idCurrentDealer = (int)data["dealerId"];
            var player = getPlayerWithID((int)data["dealerId"]);

            for (int i = 0; i < this.players.Count; i++)
            {
                if (player == this.players[i])
                {
                    PlayerViewTienlen playerView = getPlayerView(this.players[i]);
                    if (playerView != null)
                        playerView.SetIsBanker(true);
                }
                else
                {
                    PlayerViewTienlen playerView = getPlayerView(this.players[i]);
                    if (playerView != null)
                        playerView.SetIsBanker(false);
                }
            }

            btn_BecomeBanker.transform.parent.gameObject.SetActive(false);
            iconBankerDealer.SetActive(false);
        }
        else
        {
            if ((bool)data["isWaitNewGame"])
            {
                if (thisPlayer.ag >= agTable * acceptBanker)
                {
                    btn_BecomeBanker.transform.parent.gameObject.SetActive(true);
                }
            }
            else
            {
                btn_BecomeBanker.transform.parent.gameObject.SetActive(false);
            }

            iconBankerDealer.SetActive(true);

            for (int i = 0; i < players.Count; i++)
            {
                PlayerViewTienlen playerView = getPlayerView(players[i]);
                if (playerView != null)
                    playerView.SetIsBanker(false);
            }
        }
        bg_bet.SetActive(thisPlayer.id != idCurrentDealer);
    }


    public override void handleSTable(string strData)
    {
        base.handleSTable(strData);
        JObject data = JObject.Parse(strData);
        agTable = getInt(data, "M");
        m_AvatarChung.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + listPlayerXocdia.Count.ToString();
        SetStateButton(false);
        acceptBanker = (int)data["F"];
        JToken betsToken = data["bets"];
        if (betsToken != null && betsToken.Type == JTokenType.Array && ((JArray)betsToken).Count > 0)
        {
            JArray betsArray = (JArray)betsToken;
            ListValueChip = betsArray.ToObject<List<int>>();
        }
        else
        {
            ListValueChip = new List<int> { agTable, agTable * 2, agTable * 5, agTable * 10, agTable * 20 };
        }
        SetValueInchip();
        XocDiaChipManager.SetListValueChip(ListValueChip);
        stateGame = (bool)data["isWaitNewGame"] ? Globals.STATE_GAME.WAITING : Globals.STATE_GAME.VIEWING;
        JArray listPlayer = (JArray)data["ArrP"];
        foreach (JObject playerData in listPlayer)
        {
            SetupBetting(playerData);
        }
        if (!string.IsNullOrEmpty((string)data["H"]))
        {
            dataHistory += data["H"];
            ImportHistory((string)data["H"]);
        }
        int timeRemaining = (int)data["T"];
        if (timeRemaining > 22 && timeRemaining <= 25)
        {
            float delay = 25 - timeRemaining - 0.5f;

            DOVirtual.DelayedCall(delay, () =>
            {
                animBet.gameObject.SetActive(true);
                SkeletonGraphic skeleton = animBet.GetComponent<SkeletonGraphic>();
                animBet.AnimationState.SetAnimation(0, "animation", true);
                DOVirtual.DelayedCall(1f, () =>
                {
                    animBet.gameObject.SetActive(false);
                    timeToBet = timeRemaining;
                    SetTimer();
                    SetStateButton(true);
                });
            });
        }
        else if (timeRemaining > 18 && timeRemaining <= 22)
        {
            animBet.gameObject.SetActive(true);
            SkeletonGraphic skeleton = animBet.GetComponent<SkeletonGraphic>();
            animBet.AnimationState.SetAnimation(0, "animation", true);

            DOVirtual.DelayedCall(1f, () =>
            {
                animBet.gameObject.SetActive(false);
                timeToBet = timeRemaining;
                SetTimer();
                SetStateButton(true);
            });
        }
        else
        {
            timeToBet = timeRemaining;
            SetTimer();
            SetStateButton(true);
        }
        if ((int)data["dealerId"] != 0)
        {
            idCurrentDealer = (int)data["dealerId"];
            var dealerPlayer = getPlayerWithID((int)data["dealerId"]);
            foreach (var p in players)
            {
                if (p == dealerPlayer)
                {
                    PlayerViewTienlen playerView = getPlayerView(p);
                    playerView.SetIsBanker(true);
                }
                else
                {
                    PlayerViewTienlen playerView = getPlayerView(p);
                    if (playerView != null)
                    {
                        playerView.SetIsBanker(false);
                    }
                }
            }
            btn_BecomeBanker.transform.parent.gameObject.SetActive(false);
            iconBankerDealer.SetActive(false);
        }
        else
        {
            if ((bool)data["isWaitNewGame"])
            {
                if (thisPlayer.ag >= agTable * acceptBanker)
                {
                    btn_BecomeBanker.transform.parent.gameObject.SetActive(true);
                }
            }
            else
            {
                btn_BecomeBanker.transform.parent.gameObject.SetActive(false);
            }
            iconBankerDealer.SetActive(true);
            foreach (var p in players)
            {
                PlayerViewTienlen playerView = getPlayerView(p);
                if (playerView != null)
                {
                    playerView.SetIsBanker(false);
                }
            }
        }

        m_AvatarChung.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + listPlayerXocdia.Count.ToString();
    }
    private PlayerViewTienlen getPlayerView(Player player)
    {
        if (player != null)
        {
            return (PlayerViewTienlen)player.playerView;
        }
        return null;
    }
    private Coroutine timerCoroutine;

    public void SetTimer()
    {
        Debug.Log("có vào chỗ set time nhé");
        if (timerControl == null) return;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        timerControl.SetActive(true);

        timerCoroutine = StartCoroutine(RunTimer());
    }

    private IEnumerator RunTimer()
    {
        Debug.Log("có chạy vào cái runtime");
        timerControl.SetActive(true);
        TextMeshProUGUI lbTimer = timerControl.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        Transform timerTransform = timerControl.transform;
        Debug.Log(timerControl.activeSelf + "xem là chỗ này có timer ko" + timeToBet);
        while (timeToBet >= 0)
        {
            lbTimer.text = timeToBet.ToString();
            if (timeToBet < 5)
            {
                AnimateScale(timerTransform, 0.25f, 0.8f, 1.2f);
            }
            else
            {
                playSound(Globals.SOUND_GAME.CLOCK_TICK);
                AnimateScale(timerTransform, 0.5f, 0.8f, 1.2f);
            }

            yield return new WaitForSeconds(1f);
            timeToBet--;
        }

        timerControl.SetActive(false);
    }
    private void AnimateScale(Transform target, float duration, float scaleMin, float scaleMax)
    {
        target.localScale = Vector3.one * scaleMin;
        target.DOScale(Vector3.one * scaleMax, duration / 4f)
              .SetLoops(2, LoopType.Yoyo)
              .SetEase(Ease.InOutSine);
    }


    public void SetupBetting(JObject list)
    {
        JArray arr = (JArray)list["Arr"];
        for (int j = 0; j < arr.Count; j++)
        {
            JObject element = (JObject)arr[j];
            int uid = element.Value<int>("uid");

            Player player = getPlayerWithID(uid) ?? getPlayer(list.Value<string>("N"));
            if (player == null) return;
            JObject data = new JObject();
            data["evt"] = "bet";
            data["N"] = player.namePl;
            data["uid"] = uid;
            data["Num"] = element.Value<int>("N");
            data["M"] = element.Value<int>("M");
            HandleBet(data, true);
        }
    }
    public void HandleBet(JObject data, bool isEff = false)
    {
        playSound(Globals.SOUND_GAME.THROW_CHIP);
        SetStateButton(true);
        Player player = getPlayerWithID((int)data["uid"]);
        int totalBet = (int)data["M"];
        int gate = (int)data["Num"];
        if (player != null && !isEff)
        {
            player.ag -= totalBet;
        }
        player.updatePlayerView();
        if (player == thisPlayer)
        {
            stateGame = Globals.STATE_GAME.PLAYING;
        }
        if (!TEMP_VALUE_GOLD_COINS.Contains(totalBet))
        {
            ChipSpread(player, totalBet, gate);
        }
        else
        {
            EffectGetReady(player, totalBet, gate);
        }
        SetValueBetGate(player, totalBet, gate);
        // if (spamDouble == 0)
        //     btn_Double.interactable = true;
        // spamDouble++;
    }
    public void SetValueBetGate(Player player, int totalBet, int gate)
    {
        int value = totalBet;
        gateValue[gate - 1] += value;
        if (arrayLabelTop[gate - 1] != null)
        {
            arrayLabelTop[gate - 1].text = Globals.Config.FormatMoney2(gateValue[gate - 1], true, true);
        }
        if (player == thisPlayer)
        {

            selfGateValue[gate - 1] += value;
            if (arrayLabelBottom[gate - 1] != null)
            {
                arrayLabelBottom[gate - 1].text = Globals.Config.FormatMoney2(selfGateValue[gate - 1], true, true);

                Transform parent = arrayLabelBottom[gate - 1].transform.parent;
                if (parent != null && !parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                }
            }
            btn_Double.interactable = thisPlayer.ag >= selfGateValue.Sum() ? true : false;
        }
    }

    public void ChipSpread(Player player, int totalBet, int gate)
    {
        // int chipBet = totalBet;
        // for (int i = TEMP_VALUE_GOLD_COINS.Count - 1; i >= 0; i--)
        // {
        //     int chipValue = TEMP_VALUE_GOLD_COINS[i];

        //     while (chipBet >= chipValue)
        //     {
        // chipBet -= chipValue;
        EffectGetReady(player, totalBet, gate);
        //     }
        // }
    }
    public void ResetValueBetGate()
    {
        if (arrayLabelBottom == null || arrayLabelBottom.Count == 0)
            return;

        for (int i = 0; i < arrayLabelBottom.Count; i++)
        {

            arrayLabelTop[i].text = "0";
            arrayLabelBottom[i].text = "0";

            Transform parent = arrayLabelBottom[i].transform.parent;
            if (parent != null && parent.gameObject.activeSelf)
            {
                parent.gameObject.SetActive(false);
            }
        }
        foreach (GameObject gate in listBtnBet)
        {
            GameObject winningGO = gate.transform.GetChild(3).gameObject;
            winningGO.SetActive(false);
        }
    }
    public void HandleStartGame()
    {
        timeToBet = 18;
        btn_Double.interactable = false;
        btn_BecomeBanker.transform.parent.gameObject.SetActive(false);
        stateGame = Globals.STATE_GAME.WAITING;
        playSound(Globals.SOUND_HILO.START_GAME);
        if (iconBankerDealer.activeSelf)
        {
            if (thisPlayer.ag >= agTable * acceptBanker)
            {
                btn_BecomeBanker.SetActive(true);
            }

            btn_CancelBanker.SetActive(false);
        }
        DOVirtual.DelayedCall(6f, () =>
        {
            SetTimer();
            SetStateButton(true);
        });
        if (thisPlayer.id == idCurrentDealer)
        {
            stateGame = Globals.STATE_GAME.PLAYING;
        }

        DOTween.Sequence()
            .AppendCallback(() =>
            {
                animStart.gameObject.SetActive(true);
                SkeletonGraphic skeleton = animStart.GetComponent<SkeletonGraphic>();
                animStart.AnimationState.SetAnimation(0, "animation", true);
                playSound(Globals.SOUND_HILO.START_GAME);
            })
            .AppendInterval(1f)
            .AppendCallback(() => animStart.gameObject.SetActive(false))
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                plateControl.SetActive(false);
                bowlControl.SetActive(false);
                animDealer.gameObject.SetActive(true);
                bg_bet.SetActive(thisPlayer.id != idCurrentDealer);

                Sequence dealerAnim = DOTween.Sequence();
                dealerAnim.Append(animDealer.transform.DOLocalMove(new Vector2(0, 100), 0.5f).SetEase(Ease.OutSine));
                dealerAnim.AppendCallback(() =>
                {
                    animDealer.transform.parent.SetSiblingIndex(2);
                    btn_BecomeBanker.transform.parent.SetSiblingIndex(1);
                    animDealer.AnimationState.SetAnimation(0, "xocdia", false);
                });
                dealerAnim.AppendInterval(1.8f);
                dealerAnim.Append(animDealer.transform.DOLocalMove(new Vector2(0, 271), 0.5f).SetEase(Ease.OutSine));
                dealerAnim.AppendCallback(() =>
                {
                    animDealer.AnimationState.SetAnimation(0, "bt", true);
                    animDealer.transform.parent.SetSiblingIndex(1);
                    btn_BecomeBanker.transform.parent.SetSiblingIndex(2);
                });
                DOVirtual.DelayedCall(1f, () =>
                {
                    playSound(Globals.SOUND_HILO.DICE_SHAKE);
                });
            })
            .AppendInterval(3.7f)
            .AppendCallback(() =>
            {
                animBat.gameObject.SetActive(false);
                bowlControl.SetActive(true);
                plateControl.SetActive(true);

                bowlControl.transform.localPosition = bowlPos;
                plateControl.transform.localPosition = platePos;
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                animBet.gameObject.SetActive(true);
                SkeletonGraphic skeleton = animBet.GetComponent<SkeletonGraphic>();
                animBet.AnimationState.SetAnimation(0, "animation", true);
            })
            .AppendInterval(1f)
            .AppendCallback(() => animBet.gameObject.SetActive(false));
    }
    public void EffectGetReady(Player player, int chipType, int betGate)
    {
        ChipXocDia chipFly;
        chipFly = GetChipFromPool();
        chipFly.transform.localScale = Vector3.one * 0.33f;
        chipFly.GetComponent<ChipXocDia>().SetValueChip(
            chipType
        );
        chipFly.GetComponent<ChipXocDia>().playerID = player.id;
        chipFly.transform.SetParent(m_ContainerChip, false);
        chipFly.transform.position = player.playerView.transform.localPosition;
        MoveChipWithDOTween(chipFly, player.playerView.transform.localPosition, listBtnBet[betGate - 1].transform.localPosition);
        gateNumber[betGate - 1].Add(chipFly);
    }
    private void MoveChipWithDOTween(ChipXocDia chip, Vector2 startPos, Vector2 endPos)
    {
        chip.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        chip.transform.localPosition = startPos;
        Vector2 direction = (endPos - startPos).normalized;
        float offsetDistance = 30f;
        Vector2 offsetPosition = endPos - direction * offsetDistance;
        float randomOffsetX = Random.Range(-15f, 15f);
        float randomOffsetY = Random.Range(-15f, 15f);
        Vector2 randomEndPos = new Vector2(endPos.x + randomOffsetX, endPos.y + randomOffsetY);
        DOTween.Sequence()
            .Append(chip.transform.DOLocalJump(new Vector2(offsetPosition.x, offsetPosition.y), 100f, 1, 0.3f)
                .SetEase(Ease.InSine))
            .Join(chip.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.3f).SetEase(Ease.InSine)) // Phóng to
            .Append(chip.transform.DOLocalJump(new Vector2(randomEndPos.x, randomEndPos.y), 40f, 1, 0.3f)
                .SetEase(Ease.InSine))
            .Join(chip.transform.DOScale(new Vector3(0.7f, 0.7f, 0.7f), 0.3f).SetEase(Ease.InSine)) // Thu nhỏ lại
            .OnComplete(() =>
            {
                chip.gameObject.SetActive(true);
            });
    }
    public void getButtonBetVector()
    {
        buttonBetVector.Clear();

        for (int i = 0; i < listBtnBet.Count; i++)
        {
            float posX = listBtnBet[i].transform.localPosition.x;
            float posY = listBtnBet[i].transform.localPosition.y - 27f;

            var Obj = new Vector2(posX, posY);
            buttonBetVector.Add(Obj);
        }
        for (int i = 0; i < buttonBetVector.Count; i++)
        {
            Vector2 v = buttonBetVector[i];
        }
    }
    public void HandleStartSell(JObject data)
    {
        btn_Double.interactable = false;
        Player player = getPlayerWithID((int)data["dealerId"]);
        if (player == thisPlayer)
        {
            nodeSellPot.SetActive(true);
            GameObject animGateEven = nodeSellPot.transform.GetChild(1).gameObject;
            GameObject animGateOdd = nodeSellPot.transform.GetChild(2).gameObject;

            if (gateValue[0] <= 0)
            {
                animGateEven.SetActive(false);
            }
            else
            {
                animGateEven.SetActive(true);
            }

            if (gateValue[1] <= 0)
            {
                animGateOdd.SetActive(false);
            }
            else
            {
                animGateOdd.SetActive(true);
            }
        }

        timeToBet = (int)data["timeOut"];

        string text = (string)data["mess"];
        if (!string.IsNullOrEmpty(text))
        {
            bkgNoti = CreateNotification((string)data["mess"], false, titleRed);
            CanvasGroup canvasGroup = bkgNoti.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(1f, 0.5f);
            }

            DOVirtual.DelayedCall(2f, () =>
            {
                if (bkgNoti != null)
                    Destroy(bkgNoti);
            });
        }

        SetTimer();
    }
    private void Update()
    {

    }
    public GameObject CreateNotification(string text, bool isCountDown = false, Sprite spriteTitle = null, float posX = 0f, float posY = 0f)
    {
        GameObject notiBkg = new GameObject("NotificationNode", typeof(RectTransform));
        Image spr = notiBkg.AddComponent<Image>();
        spr.sprite = spriteTitle;
        spr.type = Image.Type.Sliced;
        if (spriteTitle != null) spr.SetNativeSize();
        notiBkg.transform.SetParent(this.transform, false);
        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(notiBkg.transform, false);
        TextMeshProUGUI label = content.AddComponent<TextMeshProUGUI>();
        lb_noti = label;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = Vector2.zero;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        lb_noti.alignment = TextAlignmentOptions.Center;
        lb_noti.text = isCountDown ? text + "..." : text;
        lb_noti.enableAutoSizing = true;
        lb_noti.fontSizeMin = 18;
        lb_noti.fontSizeMax = 36;
        RectTransform notiRect = notiBkg.GetComponent<RectTransform>();
        notiRect.pivot = new Vector2(0.5f, 0.5f);
        notiRect.anchorMin = new Vector2(0.5f, 0.5f);
        notiRect.anchorMax = new Vector2(0.5f, 0.5f);
        notiRect.anchoredPosition = new Vector2(posX, posY);
        CanvasGroup canvasGroup = notiBkg.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1f, 0.5f);
        if (isCountDown)
        {
            string text1 = text + ".";
            string text2 = text + "..";
            string text3 = text + "...";

            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() => lb_noti.text = text1).AppendInterval(1f);
            seq.AppendCallback(() => lb_noti.text = text2).AppendInterval(1f);
            seq.AppendCallback(() => lb_noti.text = text3).AppendInterval(1f);
            seq.SetLoops(-1);
        }

        notiBkg.SetActive(true);
        return notiBkg;
    }
    public void handleSellBet(JObject data)
    {
        Debug.Log("data cuar thằng sellbet" + data);
        var player = getPlayerWithID((int)data["uid"]);
        if (player != thisPlayer)
        {
            nodeBuyPot.SetActive(true);
            nodeBuyPot.GetComponent<BuySellPopup>().setInfo(data, (int)thisPlayer.ag);
            nodeBuyPot.transform.SetAsLastSibling();
        }
        timeToBet = (int)data["timeOut"];
        numberPotSell = int.Parse((string)data["Num"]) - 1;
        gateValue[numberPotSell] = int.Parse("0" + (string)data["M"]);
        arrayLabelTop[numberPotSell].text = Globals.Config.FormatMoney2(gateValue[numberPotSell], true, true);
        SetTimer();
    }

    public void handleBuyBet(JObject data)
    {
        int uid = (int)data["uid"];
        int Num = (int)data["Num"];
        int M = (int)data["M"];
        nodeBuyPot.GetComponent<BuySellPopup>().setInfo(data);
        nodeBuyPot.transform.SetAsLastSibling();
        Player player = getPlayerWithID(uid);
        if (player != null)
        {
            player.ag -= M;
            player.playerView.effectFlyMoney(-M);
        }
        player.updatePlayerView();
        gateValue[Num - 1] += M;
        arrayLabelTop[Num - 1].text = Globals.Config.FormatMoney2(gateValue[Num - 1], true, true);
        returnChipBuyPot(gateNumber[Num - 1], uid, M);
    }
    public void handleDealer(JObject data)
    {
        idCurrentDealer = (int)data["dealerId"];
        btn_BecomeBanker.transform.parent.gameObject.SetActive(false);

        if ((int)data["dealerId"] != 0)
        {
            var player = getPlayerWithID((int)data["dealerId"]);
            if (player != null)
            {
                PlayerViewTienlen playerView = getPlayerView(player);
                playerView.SetIsBanker(true);
            }
            iconBankerDealer.SetActive(false);
        }
        else
        {
            iconBankerDealer.SetActive(true);
            if (thisPlayer.ag >= agTable * acceptBanker)
            {
                btn_BecomeBanker.SetActive(true);
            }
            btn_CancelBanker.SetActive(false);

            for (int i = 0; i < players.Count; i++)
            {
                PlayerViewTienlen playerView = getPlayerView(players[i]);
                if (playerView != null)
                {
                    playerView.SetIsBanker(false);
                }
            }
        }

        string text = (string)data["mess"];
        if (!string.IsNullOrEmpty(text))
        {
            bkgNoti = CreateNotification(text, false, titlePurple, 0, 270);
            StartCoroutine(FadeTo(bkgNoti, 0.5f, 1f)); // giả sử fade alpha từ 0 đến 1
            StartCoroutine(DestroyAfterDelay(bkgNoti, 2f));
        }
    }
    public void handleFindDealer(JObject data)
    {
        dealerId = (int)data["dealerId"];

        if (thisPlayer.ag >= agTable * acceptBanker)
        {
            btn_BecomeBanker.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            btn_BecomeBanker.transform.parent.gameObject.SetActive(false);
        }
    }

    public void handleReturnAg(JObject data)
    {
        nodeBuyPot.SetActive(false);
        JArray dataArr = JArray.Parse(data["data"].ToString());
        returnChipForWinner(new List<int> { numberPotSell });
        foreach (JObject item in dataArr)
        {
            int uid = (int)item["uid"];
            int M = (int)item["M"];
            int AG = (int)item["AG"];
            Player player = getPlayerWithID(uid);
            if (player != null)
            {
                player.playerView.effectFlyMoney(M);
                player.ag = AG;
                Debug.Log("xem data chỗ gate" + gateNumber.Count + "và số pot sell" + numberPotSell);
                //    returnChipBuyPot(gateNumber[numberPotSell], uid, M);
                //   arrayLabelTop[numberPotSell].text = Globals.Config.FormatMoney(gateValue[numberPotSell] + M);
                if (player == thisPlayer)
                {
                    if (numberPotSell > -1)
                    {
                        selfGateValue[numberPotSell] -= int.Parse("0" + M.ToString());
                        arrayLabelBottom[numberPotSell].text = Globals.Config.FormatMoney2(selfGateValue[numberPotSell], true, true);
                    }
                }
            }
            player.updatePlayerView();
        }
    }
    public void handleLastHistory(JObject data)
    {
        JArray resultsArray = (JArray)data["results"];
        List<int> resultsList = new List<int>();
        foreach (var item in resultsArray)
        {
            resultsList.Add(item.Value<int>());
        }
        popupHistory.setTableHistory(resultsList);
    }
    public void SetTextureResult(int winType)
    {
        int totalYellowSprite = 0;
        int historyGateWin = 0;

        switch (winType)
        {
            case 1:
                totalYellowSprite = 2;
                historyGateWin = 0;
                break;
            case 6:
                totalYellowSprite = 0;
                historyGateWin = 0;
                break;
            case 3:
                totalYellowSprite = 4;
                historyGateWin = 0;
                break;
            case 4:
                totalYellowSprite = 1;
                historyGateWin = 1;
                break;
            case 5:
                totalYellowSprite = 3;
                historyGateWin = 1;
                break;
            default:
                break;
        }

        for (int i = 0; i < 4; i++)
        {
            var chip = chipResultControl[i];
            var img = chip.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = i < totalYellowSprite ? chipVang : chipTrang;
            }
            img.SetNativeSize();
            chip.SetActive(true);
            CanvasGroup cg = chip.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = chip.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            cg.DOFade(1f, 0.2f);
        }

        UpdateHistory(historyGateWin);
    }
    public void OnClickButtonMakeBet(string betType)
    {
        if (thisPlayer.id == idCurrentDealer)
            return;
        int gate = 0;
        int money = getCurrentMoney();

        switch (betType)
        {
            case "1":
                gate = 1;
                break;
            case "2":
                gate = 2;
                break;
            case "3":
                gate = 3;
                break;
            case "4":
                gate = 4;
                break;
            case "5":
                gate = 5;
                break;
            case "6":
                gate = 6;
                break;
            default:
                break;
        }

        if (money > thisPlayer.ag)
        {
            money = (int)thisPlayer.ag;
        }
        if (thisPlayer.ag > 0)
        {
            sendBet(money, gate);
            saveListBetted[gate - 1][0] += money;
        }
    }
    int getCurrentMoney()
    {
        for (int i = 0; i < m_ChipBet.Count; i++)
        {
            if (m_ChipBet[i].transform.GetChild(0).gameObject.activeSelf)
            {
                CurrentChipSelected = ListValueChip[i];
                break;
            }
        }
        return CurrentChipSelected;
    }
    public void onClickCancelSell()
    {
        nodeSellPot.SetActive(false);
        SocketSend.sendCancelSell();
    }
    public void onClickBecomeDealer()
    {
        btn_BecomeBanker.transform.parent.gameObject.SetActive(false);
        SocketSend.sendBecomeDealer();
        btn_BecomeBanker.SetActive(false);
        btn_CancelBanker.SetActive(true);
    }
    public void onClickChooseSellBet(int data)
    {
      // nodeSellPot.SetActive(false);
        NotifyXocDia popupNotify = nodeNotify.GetComponent<NotifyXocDia>();
        popupNotify.transform.SetAsLastSibling();
        popupNotify.setDataSellBet(data, nodeSellPot);
        popupNotify.onPopOn();
    }
    public void onClickHistory()
    {

        popupHistory = Instantiate(xocdiaHistory);

        if (popupHistory.transform.parent == null)
        {
            popupHistory.transform.SetParent(this.transform, false);

        }
        popupHistory.transform.SetAsLastSibling();
        popupHistory.gameObject.SetActive(true);

        if (lastHistoryInstanceResult != null)
        {
            GameObject itemObj = Instantiate(historyChildren);
            HistoryManager item = itemObj.GetComponent<HistoryManager>();

            if (lastHistoryInstanceResult == 1)
            {
                item.SetTexture(0);
            }
            else
            {
                item.SetTexture(1);
            }

            popupHistory.setResult(itemObj);
        }
        SocketSend.sendHistoryXocDia(thisPlayer.id);
    }
    public void UpdateHistory(int state)
    {
        if (state == 1)
        {
            int currentVal = int.Parse(historyLabels[0].text);
            currentVal++;
            historyLabels[0].text = currentVal.ToString();
            dataHistory += "1;";
        }
        else
        {
            int currentVal = int.Parse(historyLabels[1].text);
            currentVal++;
            historyLabels[1].text = currentVal.ToString();
            dataHistory += "2;";
        }
        int len = dataHistory.Length;

        GetPositionToInsertSprite(state);
    }
    public void GetPositionToInsertSprite(int result)
    {
        if (!HISTORY_LAST_VALUE.HasValue)
        {
            HISTORY_LAST_RESULT_ROW = 0;
            HISTORY_LAST_RESULT_COL = 0;
            HISTORY_LAST_VALUE = result;
            historyArray[HISTORY_LAST_RESULT_ROW][HISTORY_LAST_RESULT_COL] = 1;
        }
        else
        {
            if (result == HISTORY_LAST_VALUE.Value)
            {
                if (HISTORY_STREAK == 0)
                    HISTORY_STATE_IS_VERTICAL = true;

                HISTORY_STREAK++;
                if (HISTORY_STATE_IS_VERTICAL)
                {

                    if (HISTORY_LAST_RESULT_ROW >= 5 || historyArray[HISTORY_LAST_RESULT_ROW + 1][HISTORY_LAST_RESULT_COL] == 1)
                    {
                        HISTORY_STATE_IS_VERTICAL = false;
                        HISTORY_LAST_RESULT_COL++;
                        historyArray[HISTORY_LAST_RESULT_ROW][HISTORY_LAST_RESULT_COL] = 1;
                    }
                    else
                    {

                        if (historyArray[HISTORY_LAST_RESULT_ROW + 1][HISTORY_LAST_RESULT_COL] == 1)
                        {
                            HISTORY_STATE_IS_VERTICAL = false;
                            HISTORY_LAST_RESULT_COL++;
                            historyArray[HISTORY_LAST_RESULT_ROW][HISTORY_LAST_RESULT_COL] = 1;
                        }
                        else
                        {
                            HISTORY_LAST_RESULT_ROW++;
                            historyArray[HISTORY_LAST_RESULT_ROW][HISTORY_LAST_RESULT_COL] = 1;
                        }
                    }

                    if (HISTORY_STREAK == 5)
                        HISTORY_COL_BEFORE_STREAK = HISTORY_LAST_RESULT_COL;
                }
                else
                {
                    HISTORY_LAST_RESULT_COL++;
                    historyArray[HISTORY_LAST_RESULT_ROW][HISTORY_LAST_RESULT_COL] = 1;
                }

                HISTORY_LAST_VALUE = result;
            }
            else
            {
                for (int j = 0; j < historyArray[0].Count; j++)
                {
                    if (historyArray[0][j] == 0)
                    {
                        HISTORY_LAST_RESULT_COL = j;
                        break;
                    }
                }
                HISTORY_STREAK = 0;
                HISTORY_LAST_RESULT_ROW = 0;
                HISTORY_COL_BEFORE_STREAK = null;
                HISTORY_LAST_VALUE = result;
                historyArray[HISTORY_LAST_RESULT_ROW][HISTORY_LAST_RESULT_COL] = 1;
            }
        }

        if (HISTORY_LAST_RESULT_COL == 49)
        {
            ResetHistoryBoard();
        }
        Vector2 pos = ConvertArrayIndexToPosition(HISTORY_LAST_RESULT_COL, HISTORY_LAST_RESULT_ROW);
        SetPositionSpriteHistory(pos, result);
        ScrollRect scrollRect = historyNode.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.horizontalNormalizedPosition = 1f;
        }
    }

    public void ResetHistoryBoard()
    {
        HISTORY_LAST_VALUE = null;
        HISTORY_LAST_RESULT_COL = 0;
        HISTORY_LAST_RESULT_ROW = 0;
        HISTORY_STREAK = 0;
        HISTORY_COL_BEFORE_STREAK = null;

        Reset2DArray();

        foreach (Transform child in historyNode.transform)
        {
            Destroy(child.gameObject);
        }

    }

    public void ImportHistory(string data)
    {
        int even = 0;
        int odd = 0;

        string[] historyData = data.Split(';');

        foreach (string entry in historyData)
        {
            if (string.IsNullOrEmpty(entry))
                continue;

            if (entry == "1")
            {
                even++;
                GetPositionToInsertSprite(0);
            }
            else
            {
                odd++;
                GetPositionToInsertSprite(1);
            }
        }

        if (historyLabels != null && historyLabels.Length > 1)
        {
            historyLabels[0].text = odd.ToString();
            historyLabels[1].text = even.ToString();
        }
    }

    private void Reset2DArray()
    {
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 50; j++)
                historyArray[i][j] = 0;
    }
    private Vector2 ConvertArrayIndexToPosition(int col, int row)
    {
        return new Vector2(col * 29, row * -28);
    }
    public void SetPositionSpriteHistory(Vector2 vector, int state)
    {
        GameObject itemObj = Instantiate(historyChildren, historyNode.transform);
        HistoryManager item = itemObj.GetComponent<HistoryManager>();

        if (state == 1)
        {
            item.SetTexture(0);
        }
        else
        {
            item.SetTexture(1);
        }

        lastHistoryInstanceResult = state;

        RectTransform rectTransform = itemObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = vector;
        }
    }


    public void handleFinish(JObject data)
    {

        isFinish = true;
        nodeSellPot.SetActive(false);
        nodeNotify.GetComponent<NotifyXocDia>().onPopOff();
        SetStateButton(false);
        spamDouble = 0;
        btn_Double.interactable = false;
        SetTextTureResult((int)data["result"]);
        EffectOpenBowl(bowlPos);
        HighLightGateWin((int)data["result"], data);
        timerControl.SetActive(false);
        saveListBetted = new List<List<int>>
        {
            new List<int>{0,1},
            new List<int>{0,2},
            new List<int>{0,3},
            new List<int>{0,4},
            new List<int>{0,5},
            new List<int>{0,6}
        };
        DelayedReset();
    }
    public void SetTextTureResult(int winType)
    {
        int totalYellowSprite = 0;
        int historyGateWin = 0;

        switch (winType)
        {
            case 1:
                totalYellowSprite = 2;
                historyGateWin = 0;
                break;
            case 6:
                totalYellowSprite = 0;
                historyGateWin = 0;
                break;
            case 3:
                totalYellowSprite = 4;
                historyGateWin = 0;
                break;
            case 4:
                totalYellowSprite = 1;
                historyGateWin = 1;
                break;
            case 5:
                totalYellowSprite = 3;
                historyGateWin = 1;
                break;
            default:
                break;
        }

        for (int i = 0; i < 4; i++)
        {
            Image img = chipResultControl[i].GetComponent<Image>();
            if (i < totalYellowSprite)
            {
                img.sprite = chipVang;
            }
            else
            {
                img.sprite = chipTrang;
            }
            img.SetNativeSize();
            chipResultControl[i].SetActive(true);
        }
        UpdateHistory(historyGateWin);
    }
    private async void DelayedReset()
    {
        while (isFinish)
        {
            await UniTask.Delay(50);
        }
        if (this == null || this.gameObject == null)
        {
            return;
        }
        stateGame = Globals.STATE_GAME.WAITING;
        ReSetGameDisplay();
        checkAutoExit();
    }

    public void ReSetGameDisplay()
    {

        for (int i = 0; i < gateValue.Count; i++)
        {
            gateValue[i] = 0;
        }
        for (int i = 0; i < selfGateValue.Count; i++)
        {
            selfGateValue[i] = 0;
        }
        if (arrayLabelTop != null)
        {
            foreach (var label in arrayLabelTop)
            {
                if (label != null && label.transform.parent != null)
                {
                    label.transform.parent.gameObject.SetActive(true);
                }
            }
        }

        ResetValueBetGate();

        if (chipResultControl != null)
        {
            for (int i = 0; i < 4 && i < chipResultControl.Count; i++)
            {
                if (chipResultControl[i] != null)
                {
                    chipResultControl[i].SetActive(false);
                }
            }
        }

        timeToBet = 18;
    }
    public void EffectOpenBowl(Vector2 target)
    {
        StartCoroutine(OpenBowlSequence(target));
    }

    private IEnumerator OpenBowlSequence(Vector2 target)
    {
        Image bowlImage = bowlControl.GetComponent<Image>();
        if (bowlImage == null)
        {
            yield break;
        }
        bowlControl.transform.DOMove(new Vector3(target.x, target.y + 80f), 0.5f).SetEase(Ease.InExpo);
        bowlImage.DOFade(0f, 0.5f);
        yield return new WaitForSeconds(3.5f);
        bowlImage.DOFade(1f, 0.5f);
        bowlControl.transform.DOMove(new Vector3(target.x, target.y), 2f).SetEase(Ease.InOutSine);
    }

    public void HighLightGateWin(int dataNumber, JObject data)
    {
        List<int> totalYellowSprite = new List<int>();

        switch (dataNumber)
        {
            case 1:
                totalYellowSprite = new List<int> { 0 };
                break;
            case 6:
                totalYellowSprite = new List<int> { 0, 5 };
                break;
            case 3:
                totalYellowSprite = new List<int> { 0, 2 };
                break;
            case 4:
                totalYellowSprite = new List<int> { 1, 3 };
                break;
            case 5:
                totalYellowSprite = new List<int> { 1, 4 };
                break;
            default:
                break;
        }
        for (int i = 0; i < listBtnBet.Count; i++)
        {
            listBtnBet[i].SetActive(true);
            GameObject gate = listBtnBet[i].transform.GetChild(3).gameObject;
            if (gate != null && !totalYellowSprite.Contains(i))
            {
                gate.SetActive(true);
            }
        }
        foreach (int i in totalYellowSprite)
        {
            GameObject winningGO = listBtnBet[i].transform.GetChild(2).gameObject;
            winningGO.SetActive(true);

            Image img = winningGO.GetComponent<Image>();
            if (img == null)
            {
                continue;
            }
            Sequence seq = DOTween.Sequence();
            float blinkDuration = 4f;
            int blinkCount = 15;
            float singleBlink = blinkDuration / (blinkCount * 2);

            for (int j = 0; j < blinkCount; j++)
            {
                seq.Append(img.DOFade(0f, singleBlink));
                seq.Append(img.DOFade(1f, singleBlink));
            }

            seq.OnComplete(() => winningGO.SetActive(false));
        }
        List<int> mangToTotal = new List<int> { 0, 1, 2, 3, 4, 5 };
        var difference = mangToTotal.Except(totalYellowSprite).ToList();
        Sequence fullSequence = DOTween.Sequence();
        fullSequence.AppendInterval(4f);
        fullSequence.AppendCallback(() => ChipReturnEffect(totalYellowSprite));
        fullSequence.AppendInterval(1f);
        fullSequence.AppendCallback(() => returnChipForWinner(totalYellowSprite));
        fullSequence.AppendInterval(0.5f);
        fullSequence.AppendCallback(() => EffectMoneyForPlayer(data));
        fullSequence.AppendInterval(2.5f);
        fullSequence.AppendCallback(() =>
        {
            foreach (List<ChipXocDia> gate in gateNumber)
            {
                foreach (ChipXocDia chip in gate)
                {
                    if (chip != null)
                    {
                        ReturnChipToPool(chip);
                    }
                }
                gate.Clear();
            }
            gateNumber = new List<List<ChipXocDia>>()
            {
                new List<ChipXocDia>(),
                new List<ChipXocDia>(),
                new List<ChipXocDia>(),
                new List<ChipXocDia>(),
                new List<ChipXocDia>(),
                new List<ChipXocDia>()
            };

        }).AppendInterval(1.5f).
        AppendCallback(
            () =>
             isFinish = false);
    }
    public void ChipReturnEffect(List<int> arrayGateWin)
    {
        int count = 0;

        for (int i = 0; i < gateNumber.Count; i++)
        {
            if (gateNumber[i].Count > 0 && count == 0)
            {
                playSound(Globals.SOUND_HILO.CHIP_LOSER);
                count++;
            }
            if (!arrayGateWin.Contains(i))
            {
                foreach (var chip in gateNumber[i])
                {
                    var img = chip.GetComponent<Image>();
                    if (img != null)
                    {
                        Sequence seq = DOTween.Sequence();
                        seq.Append(chip.transform.DOMove(bowlPos, 0.5f).SetEase(Ease.InExpo)) // Di chuyển
                           .AppendCallback(() =>
                           {
                               ReturnChipToPool(chip);
                           });
                    }
                    else
                    {
                        Sequence seq = DOTween.Sequence();
                        seq.Append(chip.transform.DOMove(bowlPos, 0.5f).SetEase(Ease.InExpo)) // Di chuyển
                        .AppendCallback(() =>
                        {
                            ReturnChipToPool(chip);
                        });
                    }
                }
            }
        }
    }

    public void EffectMoneyForPlayer(JObject data)
    {
        string JData = (string)data["data"];
        JArray playerData = JArray.Parse(JData);
        foreach (JObject item in playerData)
        {
            int uid = (int)item["uid"];
            Player player = getPlayerWithID(uid);
            if (player != null)
            {
                player.ag = (int)item["AG"];
                player.playerView.effectFlyMoney((int)item["M"], 40);
            }
            player.updatePlayerView();
        }
        if (thisPlayer.ag < agTable * acceptBanker)
        {
            btn_BecomeBanker.transform.parent.gameObject.SetActive(false);
        }
        else if (thisPlayer.id == idCurrentDealer && thisPlayer.ag >= agTable * acceptBanker)
        {
            btn_BecomeBanker.transform.parent.gameObject.SetActive(true);
            btn_BecomeBanker.SetActive(false);
        }
        // else
        // {
        //     btn_BecomeBanker.transform.parent.gameObject.SetActive(true);
        //     btn_CancelBanker.SetActive(true);
        //     btn_BecomeBanker.SetActive(true);
        // }
    }

    private IEnumerator FadeTo(GameObject go, float duration, float targetAlpha)
    {

        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = go.AddComponent<CanvasGroup>();
        }
        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }
    private IEnumerator DestroyAfterDelay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (go != null)
        {
            Destroy(go);
        }
    }

    public void returnChipBuyPot(List<ChipXocDia> gateBuy, int playerID, int moneyBuy)
    {
        Player playerBuy = getPlayerWithID(playerID);
        for (int j = 0; j < gateBuy.Count; j++)
        {
            if (playerBuy == null) continue;
            if (moneyBuy <= 0) break;

            ChipXocDia chipComponent = gateBuy[j];
            int valueChip = (int)chipComponent.GetValue();

            if (moneyBuy >= valueChip)
            {
                moneyBuy -= valueChip;

                chipComponent.transform.DOMove(playerBuy.playerView.transform.position, 0.5f)
      .SetEase(Ease.InQuad)
      .OnComplete(() =>
      {
          chipComponent.playerID = 0;
          ReturnChipToPool(chipComponent);
      });
            }
            else
            {
                ChipXocDia chipCreateComponent = GetChipFromPool();
                chipCreateComponent.SetValueChip(moneyBuy);
                chipComponent.SetValueChip(valueChip - moneyBuy);
                moneyBuy = 0;
                chipCreateComponent.transform.position = chipComponent.transform.position;
                CanvasGroup cg = chipCreateComponent.GetComponent<CanvasGroup>();
                if (cg == null) cg = chipCreateComponent.gameObject.AddComponent<CanvasGroup>();
                Sequence seq = DOTween.Sequence();
                seq.Append(chipCreateComponent.transform.DOMove(playerBuy.playerView.transform.position, 0.5f).SetEase(Ease.InQuad));
                seq.Join(cg.DOFade(0, 0.6f));
                seq.OnComplete(() => ReturnChipToPool(chipCreateComponent));
            }
        }
    }
    public void ShowDialogRule()
    {
        string key = "text_rule_" + Globals.Config.curGameId.ToString();
        string messageTemplate = Globals.Config.getTextConfig(key);
        string message = messageTemplate.Replace("XXX", acceptBanker.ToString());
        m_PopupRule.SetActive(true);
        m_TxtRule.text = message;
    }
    public void CloseDialogRule()
    {
        if (m_PopupRule != null)
        {
            m_PopupRule.SetActive(false);
        }
    }
    private void ReturnChipToPool(ChipXocDia chip)
    {
        chip.gameObject.SetActive(false);
        chip.transform.SetParent(m_ContainerChip, false);
        chipPool.Enqueue(chip);
    }


    public void returnChipForWinner(List<int> arrayGateWin)
    {
        int count = 0;

        for (int i = 0; i < gateNumber.Count; i++)
        {
            if (arrayGateWin.Contains(i))
            {
                if (gateNumber[i].Count > 0 && count == 0)
                {
                    playSound(Globals.SOUND_HILO.CHIP_WINNER);
                    count++;
                }
                for (int j = 0; j < gateNumber[i].Count; j++)
                {

                    ChipXocDia chipScript = gateNumber[i][j];
                    if (chipScript == null) continue;
                    Player player = getPlayerWithID(chipScript.playerID);
                    if (player == null) continue;
                    MoveAndFadeChip_DOTween(chipScript,
                     player.playerView.transform.localPosition);
                    chipScript.playerID = 0;
                }
            }
        }
    }
    private void MoveAndFadeChip_DOTween(ChipXocDia chip, Vector3 targetPosition)
    {
        float durationMove = 0.5f;
        float durationFade = 0.3f;
        Sequence seq = DOTween.Sequence();
        seq.Append(chip.transform.DOLocalMove(targetPosition, durationMove).SetEase(Ease.InOutSine)) // Di chuyển trước
           .AppendCallback(() =>
           {
               ReturnChipToPool(chip);
           });
    }
    public override async void handleJTable(string objData)
    {
        while (isFinish)
        {
            await UniTask.Delay(50);
        }
        base.handleJTable(objData);
        m_AvatarChung.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + listPlayerXocdia.Count.ToString();
        if (this.idCurrentDealer == 0 && this.stateGame == Globals.STATE_GAME.WAITING && this.thisPlayer.ag >= this.agTable * acceptBanker)
        {
            btn_BecomeBanker.transform.parent.gameObject.SetActive(true);
        }
    }
    public override async void handleLTable(JObject data)
    {

        while (isFinish)
        {
            await UniTask.Delay(50);
        }
        var namePl = (string)data["Name"];
        var player = getPlayer(namePl);
        if (player == null) return;
        if (player != thisPlayer)
        {
            removePlayer(namePl);
        }
        if (m_AvatarChung != null && m_AvatarChung.transform != null)
        {
            var childTransform = m_AvatarChung.transform.GetChild(0);
            if (childTransform != null)
            {
                var textComponent = childTransform.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = "+" + listPlayerXocdia.Count.ToString();
                }
            }
        }
        if (players.Count <= 1)
        {
            btn_BecomeBanker.transform.parent.gameObject.SetActive(false);
        }
    }

    public void onClickChat(string isChatText)
    {
        SoundManager.instance.soundClick();
        if (stateGame == Globals.STATE_GAME.VIEWING)
        {
            return;
        }
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        var subViewObj = Instantiate(m_ChatInGame, parentCanvas.transform);
        var subView = subViewObj.GetComponent<ChatIngameView>();
        subView.gameObject.SetActive(true);
        subView.transform.SetAsLastSibling();
        subView.onClickTab(isChatText);
        RectTransform rt = subView.GetComponent<RectTransform>();

    }
}

