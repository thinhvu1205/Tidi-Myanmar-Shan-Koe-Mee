using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Globals;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Lucky89View : GameView // Lucky89_ShanKoeMee
{
    public enum SCORE
    {
        LUCKY_8 = 8, LUCKY_9 = 9, FACE_CARDS = 3000, STRAIGHT_FLUSH = 2000, FLUSH = 1000
    }
    [SerializeField] private Button buttonDone, buttonShowCard;
    [SerializeField] private GameObject parentCardBig;
    [SerializeField] private List<TextMeshProUGUI> m_BetOptionTMPs, listTextJackPot;
    [SerializeField] private GameObject popupRuleGame;
    [SerializeField] private Transform m_PrefabChipTf, m_ChipsTf;
    [SerializeField] private TextMeshProUGUI m_TipChipsTMP, m_TipThanksTMP, textTimeStartCountDown, textContent, textContentAction, textTimeAction, textTimeActionCountDown;
    [SerializeField] private SkeletonGraphic m_DealerSG, m_BeginSG, animHand;
    [SerializeField] private PlayerViewLucky89 m_DealerPVL89;
    [SerializeField] private GameObject boxTimeStart, panelAction, boxTimeAction;
    [SerializeField] private Image imageTimeActionRemain;
    [SerializeField] private List<Card> listCardBig;
    [SerializeField] private Image imageCardRotate, imageCardDraw;
    [SerializeField] private Transform potTransform;
    [SerializeField] private Button buttonDraw, buttonNotDraw, buttonDeclare3Card;
    public PlayerViewLucky89 thisPlayerView = null;
    private List<int> _BetValues = new();
    private List<int> listCodeCard = new();
    private Action _WaitForFinishCompleteCb = null;
    private const float CARD_FLYING_DURATION = .15f, CARD_ROTATING_DURATION = .25f, WIN_CHIP_DURATION = .5f, LOSE_CHIP_DURATION = .5f;
    // private bool? _DrawACard = null;
    // private int _CurBankerId;
    private Vector2 touchStartPos;
    private bool isTrackingSwipe = false;
    private bool _isRevealMyCards = false;
    private bool showButtonInPanelAction = true;
    long potValue = 0;
    private int gameRemaining;
    private string bankerName = "";
    Coroutine countdownCoroutine;
    Tweener countdownTween;


    protected override void Awake()
    {
        base.Awake();
        agTable = 0;
    }
    protected override void Start()
    {
        base.Start();
        m_DealerSG.AnimationState.Complete += (x) => m_DealerSG.AnimationState.SetAnimation(0, "normal", true);
        buttonDone.onClick.AddListener(ClickButtonDone);
        buttonShowCard.onClick.AddListener(ClickButtonShowCard);
    }

    #region Button
    public void DoClickBetButton(int buttonId)
    {
        playSound(SOUND_GAME.CLICK);
        SocketSend.SendBetLucky89((int)Mathf.Min(User.userMain.AG, _BetValues[buttonId]));
    }
    public void onClickRuleJP()
    {
        UIManager.instance.openRuleJPShanKoeMee();
    }
    public void DoClickDraw()
    {
        buttonDraw.gameObject.SetActive(false);
        buttonNotDraw.gameObject.SetActive(false);
        buttonDeclare3Card.gameObject.SetActive(false);
        // ActionDrawCard();
        playSound(SOUND_GAME.CLICK);
        if (thisPlayerView != null && thisPlayerView.isBanker)
        {
            SocketSend.SendEventBanker(2);
        }
        else
        {
            SocketSend.SendDrawACardLucky89(1);
        }
    }
    public void DoClickDontDraw()
    {
        // DisablePanelAction();
        ActiveButtonDone();
        playSound(SOUND_GAME.CLICK);
        if (thisPlayerView != null && thisPlayerView.isBanker)
        {
            SocketSend.SendEventBanker(3);
        }
        else
        {
            SocketSend.SendDrawACardLucky89(0);
        }
    }
    public void DoClickDeclareWith_3Card()
    {
        // panelAction.SetActive(false);
        DisablePanelAction();
        playSound(SOUND_GAME.CLICK);
        SocketSend.SendEventBanker(1);
    }
    public void DoClickTip()
    {
        playSound(SOUND_GAME.CLICK);
        SocketSend.sendTip();
    }
    public override void onClickRule()
    {
        playSound(SOUND_GAME.CLICK);
        popupRuleGame.SetActive(true);
    }
    #endregion
    public void ProcessResponseData(JObject jData)
    {
        switch ((string)jData["evt"]) //timeToStart//banker_info//bm//lc//bc//cbc//cdco//finish
        {
            case "timeToStart":
                Debug.Log($"Tinh=))timeToStart: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                _HandleStartGame(jData);
                break;
            case "banker_info":
                _HandleBankerInfor(jData);
                Debug.Log($"Tinh=))banker_info: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                break;
            case "bm":
                Debug.Log($"Tinh=))bm: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                _HandleAnyoneBets(jData);
                break;
            case "lc":
                Debug.Log($"Tinh=))lc: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                _HandleReceiveMyCards(jData);
                break;
            case "cShan":
                Debug.Log($"Tinh=))cShan: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                _HandleReceiveLuckyCards(jData);
                break;
            case "bc":
                Debug.Log($"Tinh=))bc: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                _HandleAnyoneDrawsCard(jData);
                break;
            case "cbc":
                _HandleCBC(jData);
                Debug.Log($"Tinh=))cbc: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                break;
            case "cdco":
                _HandleCDCO(jData);
                Debug.Log($"Tinh=))cdco: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                break;
            case "finish":
                Debug.Log($"Tinh=))finish: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                _HandleFinishGame(jData);
                break;
            case "finish_opt1":
                Debug.Log($"Tinh=))finish_opt1: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                _HandleFinishOtp1(jData);
                break;
            case "tip":
                Debug.Log($"Tinh=))tip: {jData.ToString(Newtonsoft.Json.Formatting.None)}");
                HandlerTip(jData);
                break;
        }
    }
    public override void setGameInfo(int m, int id = 0, int maxBett = 0)
    {
        base.setGameInfo(m, id, maxBett);
        if (_BetValues.Count > 0) return;
        List<int> coefficients = new() { 1, 2, 5, 10, 20, 100 };
        foreach (int coefficient in coefficients) _BetValues.Add(agTable * coefficient);
    }
    protected override void updatePositionPlayerView()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].playerView.transform.localPosition = listPosView[i];
            players[i].updateItemVip(players[i].vip);
            PlayerViewLucky89 pv = getPlayerView(players[i]);
            if (pv != null)
            {
                pv.SetBetPosition(i);
                pv.SetCardPosition(i);
                pv.SetIconBankerPosition(i);
                pv.ShowIconBanker(pv.isBanker, gameRemaining);
            }
        }
    }
    public override void handleCTable(string strData)
    {
        base.handleCTable(strData);
    }
    public override void handleCCTable(JObject data)
    {
        if (_WaitForFinishCompleteCb != null) _WaitForFinishCompleteCb += () => base.handleCCTable(data);
        else base.handleCCTable(data);
    }
    public override void handleVTable(string strData)
    {
        Debug.Log($"Tinh=))handleVTable: {strData}");

        stateGame = STATE_GAME.VIEWING;
        JObject data = JObject.Parse(strData);

        int tableId = data.ContainsKey("Id") ? (int)data["Id"] : 0;
        int betValue = data.ContainsKey("M") ? (int)data["M"] : 0;
        potValue = GetPotValue(data);
        handleUpdatePot(potValue);
        setGameInfo(m: betValue, id: tableId, maxBett: 0);

        JObject bankerInfo = data.ContainsKey("bankerInfoTransfer") ? (JObject)data["bankerInfoTransfer"] : null;
        int bankerId = bankerInfo != null && bankerInfo.ContainsKey("pid") ? (int)bankerInfo["pid"] : -1;
        gameRemaining = bankerInfo != null && bankerInfo.ContainsKey("gameRemain") ? (int)bankerInfo["gameRemain"] : 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerView != null)
                Destroy(((Component)players[i].playerView).gameObject);
        }
        players.Clear();
        thisPlayer = new Player
        {
            playerView = createPlayerView(),
            id = User.userMain.Userid,
            namePl = User.userMain.Username,
            displayName = User.userMain.displayName,
            ag = User.userMain.AG,
            agBet = 0,
            vip = User.userMain.VIP,
            avatar_id = User.userMain.Avatar,
            is_ready = true,
        };
        thisPlayer.fid = User.userMain.Tinyurl.IndexOf("fb.") != -1
            ? User.userMain.Tinyurl.Substring(3)
            : thisPlayer.fid;
        if (thisPlayer.playerView != null)
            ((PlayerViewLucky89)thisPlayer.playerView).setDark(true); // safe cast check
        players.Add(thisPlayer);

        JArray dataPlayers = (JArray)data["ArrP"];
        if (dataPlayers == null)
        {
            Debug.LogWarning("handleVTable: ArrP null!");
            return;
        }
        foreach (JObject jPl in dataPlayers)
        {
            Player player = new Player();
            readDataPlayer(player, jPl);

            player.playerView = createPlayerView();
            player.agBet = jPl.ContainsKey("AGC") ? (int)jPl["AGC"] : 0;

            players.Add(player);
        }
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            PlayerViewLucky89 pv = getPlayerView(player);
            if (pv == null)
            {
                player.playerView = createPlayerView();
                pv = getPlayerView(player);
                if (pv == null)
                {
                    Debug.LogError("handleVTable: cannot create PlayerViewLucky89 for player id " + player.id);
                    continue;
                }
            }
            if (player.id == User.userMain.Userid)
            {
                thisPlayerView = pv;
            }
            JObject jPl = null;
            for (int k = 0; k < dataPlayers.Count; k++)
            {
                JObject tmp = (JObject)dataPlayers[k];
                if (tmp != null && tmp.ContainsKey("id") && (int)tmp["id"] == player.id)
                {
                    jPl = tmp;
                    break;
                }
            }

            // set banker flag
            pv.isBanker = player.id == bankerId;

            player.updatePlayerView();

            // pv.SetBetPosition(i)
            //   .ShowHideBetChips(player.agBet > 0, player.agBet)
            //   .HideAllCards()
            //   .UpdateCardsParentPositionAndRotation();

            // pv.SetCardPosition(i);
            // pv.SetIconBankerPosition(i);

            // if (pv.isBanker)
            //     pv.ShowIconBanker(true, gameRemaining);
            // else
            //     pv.ShowIconBanker(false, gameRemaining);
        }
        updatePositionPlayerView();

        // distribute cards to each player using Arr from server
        foreach (JObject jPl in dataPlayers)
        {
            int pid = jPl.ContainsKey("id") ? (int)jPl["id"] : -1;
            var p = getPlayerWithID(pid);
            PlayerViewLucky89 pv = getPlayerView(p);

            JArray arrCards = jPl.ContainsKey("Arr") ? (JArray)jPl["Arr"] : new JArray();
            List<int> cardCodes = new List<int>();
            foreach (JToken card in arrCards)
                cardCodes.Add((int)card);

            int rate = jPl.ContainsKey("rate") ? (int)jPl["rate"] : 0;
            int score = jPl.ContainsKey("score") ? (int)jPl["score"] : 0;

            _DistributeCardsToAPlayer(pv, cardCodes, rate, score, arrCards.Count);
        }

        // Debug.Log($"Tinh=))handleVTable done - total players: {players.Count}, bankerId: {bankerId}");
    }
    public override void handleSTable(string strData)
    {
        stateGame = STATE_GAME.WAITING;

        JObject data = JObject.Parse(strData);
        int tableId = data.Value<int?>("Id") ?? 0;
        int betValue = data.Value<int?>("M") ?? 0;
        int maxBetValue = data.Value<int?>("maxBet") ?? 0;

        // potValue = GetPotValue(data);
        // handleUpdatePot(potValue);
        setGameInfo(m: betValue, id: tableId, maxBett: maxBetValue);

        JArray dataPlayers = (JArray)data["ArrP"];
        if (dataPlayers == null)
        {
            Debug.LogWarning("[handleSTable] ArrP null!");
            return;
        }

        // =========================
        // 1. COLLECT PID FROM SERVER
        // =========================
        HashSet<int> serverPids = new HashSet<int>();
        for (int i = 0; i < dataPlayers.Count; i++)
        {
            int pid = dataPlayers[i].Value<int>("id");
            serverPids.Add(pid);
        }

        // =========================
        // 2. REMOVE PLAYER LEFT TABLE
        // =========================
        for (int i = players.Count - 1; i >= 0; i--)
        {
            Player p = players[i];
            if (!serverPids.Contains(p.id))
            {
                if (p.playerView != null)
                    Destroy(p.playerView.gameObject);

                if (p == thisPlayer)
                {
                    thisPlayer = null;
                    thisPlayerView = null;
                }

                players.RemoveAt(i);
            }
        }

        // =========================
        // 3. ADD / UPDATE PLAYER
        // =========================
        for (int i = 0; i < dataPlayers.Count; i++)
        {
            JObject jPl = (JObject)dataPlayers[i];
            int pid = jPl.Value<int>("id");

            Player player = getPlayerWithID(pid);

            if (player == null)
            {
                // --- NEW PLAYER ---
                player = new Player();
                readDataPlayer(player, jPl);

                player.playerView = createPlayerView();

                if (player.id == User.userMain.Userid)
                {
                    thisPlayer = player;
                    players.Insert(0, player);
                    thisPlayerView = getPlayerView(player);
                }
                else
                {
                    players.Add(player);
                }
            }
            else
            {
                // --- UPDATE PLAYER ---
                readDataPlayer(player, jPl);
            }
        }

        // =========================
        // 4. UPDATE VIEW & POSITION
        // =========================
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            if (player.playerView == null) continue;
            PlayerViewLucky89 pv = getPlayerView(player);
            if (pv == null) continue;
            player.playerView.transform.localPosition = listPosView[i];
            player.updateItemVip(player.vip);
            player.updatePlayerView();
            pv.setDark(false);
            pv.SetBetPosition(i)
             .ShowHideBetChips(player.agBet > 0, player.agBet)
             .HideAllCards()
             .UpdateCardsParentPositionAndRotation();

            pv.SetCardPosition(i);
            pv.SetIconBankerPosition(i);
        }
    }


    public override void handleJTable(string strData)
    {
        if (_WaitForFinishCompleteCb != null) _WaitForFinishCompleteCb += () => base.handleJTable(strData);
        else base.handleJTable(strData);
    }
    public override void handleRJTable(string strData)
    {
        Debug.Log($"Tinh=))handleRJTable: {strData}");

        stateGame = STATE_GAME.PLAYING;
        JObject data = JObject.Parse(strData);
        int tableId = data.ContainsKey("Id") ? (int)data["Id"] : 0;
        int betValue = data.ContainsKey("M") ? (int)data["M"] : 0;
        potValue = GetPotValue(data);
        handleUpdatePot(potValue);
        setGameInfo(m: betValue, id: tableId, maxBett: 0);
        JObject bankerInfo = data.ContainsKey("bankerInfoTransfer") ? (JObject)data["bankerInfoTransfer"] : null;
        int bankerId = bankerInfo != null && bankerInfo.ContainsKey("pid") ? (int)bankerInfo["pid"] : -1;
        gameRemaining = bankerInfo.ContainsKey("gameRemain") ? (int)bankerInfo["gameRemain"] : 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerView != null)
                Destroy(players[i].playerView.gameObject);
        }
        players.Clear();
        JArray dataPlayers = (JArray)data["ArrP"];
        if (dataPlayers == null)
        {
            Debug.LogWarning("handleRJTable: ArrP null!");
            return;
        }
        foreach (JObject jPl in dataPlayers)
        {
            Player player = new Player();
            readDataPlayer(player, jPl);

            player.playerView = createPlayerView();
            player.agBet = jPl.ContainsKey("AGC") ? (int)jPl["AGC"] : 0;
            if (player.id == User.userMain.Userid)
            {
                thisPlayer = player;
                players.Insert(0, player);
            }
            else players.Add(player);
        }
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            PlayerViewLucky89 pv = getPlayerView(player);
            JObject jPl = null;
            for (int k = 0; k < dataPlayers.Count; k++)
            {
                JObject tmp = (JObject)dataPlayers[k];
                if (tmp != null && tmp.ContainsKey("id") && (int)tmp["id"] == player.id)
                {
                    jPl = tmp;
                    break;
                }
            }

            pv.isBanker = player.id == bankerId;

            player.updatePlayerView();

            pv.SetBetPosition(i)
              .ShowHideBetChips(player.agBet > 0, player.agBet)
              .HideAllCards()
              .UpdateCardsParentPositionAndRotation();

            pv.SetCardPosition(i);
            pv.SetIconBankerPosition(i);
            if (pv.isBanker)
            {
                pv.ShowIconBanker(true, gameRemaining);
                Debug.Log($"Tinh=))BatIconBanker");
            }
            else
            {
                pv.ShowIconBanker(false, gameRemaining);
            }
            updatePositionPlayerView();
        }

        List<DataPlayer> dps = new();
        bool distributeCards = false;

        foreach (JObject jPl in dataPlayers)
        {
            int pid = (int)jPl["id"];
            Player player = players.Find(x => x.id == pid);
            if (player == null) continue;

            JArray cardsArr = jPl.ContainsKey("Arr") ? (JArray)jPl["Arr"] : new JArray();
            DataPlayer dp = new DataPlayer
            {
                PlayerP = player,
                rate = jPl.ContainsKey("rate") ? (int)jPl["rate"] : 0,
                score = jPl.ContainsKey("score") ? (int)jPl["score"] : 0,
                cardCount = cardsArr.Count
            };

            foreach (JToken c in cardsArr)
                dp.cardCodes.Add((int)c);

            dps.Add(dp);
            if (player == thisPlayer)
            {
                foreach (int num in dp.cardCodes)
                    if (num > 0)
                    {
                        distributeCards = true;
                        break;
                    }
            }
        }
        if (distributeCards)
        {
            foreach (DataPlayer dp in dps)
            {
                if (dp.PlayerP == null) continue;

                PlayerViewLucky89 pv = getPlayerView(dp.PlayerP);
                _DistributeCardsToAPlayer(pv, dp.cardCodes, dp.rate, dp.score, dp.cardCount);

                if (pv.isBanker)
                {
                    pv.ShowIconBanker(true, gameRemaining);
                }

            }
        }
    }


    public override void handleLTable(JObject data)
    {
        HandleData.DelayHandleLeave = 0;
        if (_WaitForFinishCompleteCb != null) _WaitForFinishCompleteCb += () => base.handleLTable(data);
        else base.handleLTable(data);
        DOVirtual.DelayedCall(1f, () =>
        {
            if (thisPlayerView != null && thisPlayerView.isBanker && players.Count == 1)
            {
                handleUpdatePot(0);
                thisPlayerView.setAg(Globals.User.userMain.AG);
                thisPlayerView.ShowIconBanker(false);
            }
        });
        if ((string)data["Name"] == bankerName)
        {
            Debug.Log($"Banker_Out");
            handleUpdatePot(0);
            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                PlayerViewLucky89 playerView = getPlayerView(player);
                playerView.ShowIconBanker(false);
                player.setAg();
            }
        }
        for (int i = 0; i < players.Count; i++)
        {
            PlayerViewLucky89 playerView = getPlayerView(players[i]);
            Player player = players[i];
            if (playerView.isBanker && gameRemaining == 1)
            {
                handleUpdatePot(0);
                player.setAg();
                playerView.ShowIconBanker(false);
            }
        }
    }
    private void _HandleStartGame(JObject data)
    {
        showButtonInPanelAction = true;
        _isRevealMyCards = false;
        listCodeCard.Clear();
        playSound(SOUND_GAME.START_GAME);
        stateGame = STATE_GAME.WAITING;
        float time = (float)data["timeAction"] / 1000;
        StartCoroutine(RunCountDownStartAndBet(time, "စတင်မည်မှာ"));
        if (m_DealerPVL89 != null)
        {
            m_DealerPVL89.ShowHideBetChips(false).ShowAnimResult(false, 0).ShowScore(false, 0, 0).ShowRate(0).HideAllCards();
        }
        foreach (Player p in players) ((PlayerViewLucky89)p.playerView).ShowHideBetChips(false).ShowAnimResult(false, 0).ShowScore(false, 0, 0).ShowRate(0).HideAllCards().isLucky = false;
    }
    IEnumerator RunCountDownStartAndBet(float timeCountDown, string content, bool showAnimBegin = true)
    {
        boxTimeStart.SetActive(true);
        textContent.text = content;
        while (timeCountDown > 0)
        {
            textTimeStartCountDown.text = Mathf.CeilToInt(timeCountDown).ToString();
            yield return new WaitForSeconds(1f);
            timeCountDown -= 1f;
        }
        boxTimeStart.SetActive(false);
        if (showAnimBegin)
        {
            m_BeginSG.gameObject.SetActive(true);
            m_BeginSG.AnimationState.SetAnimation(0, "start", false).Complete += (entry) =>
            {
                m_BeginSG.gameObject.SetActive(false);
            };
            stateGame = STATE_GAME.PLAYING;
        }
        else
        {
            StartCoroutine(ShowBetOption(false));
        }
    }
    IEnumerator RunCountDownAction(float timeAction, string content)
    {
        if (boxTimeStart.activeSelf)
            boxTimeStart.SetActive(false);
        boxTimeAction.SetActive(true);
        textContentAction.text = content;
        while (timeAction > 0)
        {
            textTimeAction.text = Mathf.CeilToInt(timeAction).ToString();
            yield return new WaitForSeconds(1f);
            timeAction -= 1f;
        }
        boxTimeAction.SetActive(false);
    }
    IEnumerator RunCountDownTimeAction(float timeCountDown)
    {
        countdownTween?.Kill();
        imageTimeActionRemain.fillAmount = 1f;
        countdownTween = imageTimeActionRemain
         .DOFillAmount(0f, timeCountDown)
         .SetEase(Ease.Linear)
         .SetId("ActionTween");
        while (timeCountDown > 0)
        {
            textTimeActionCountDown.text = Mathf.CeilToInt(timeCountDown).ToString();
            yield return new WaitForSeconds(1f);
            timeCountDown -= 1f;
        }
        DisablePanelAction();
    }
    IEnumerator ShowBetOption(bool isShow = true)
    {
        for (int i = 0; i < m_BetOptionTMPs.Count; i++)
        {
            Transform tf = m_BetOptionTMPs[i].transform;
            m_BetOptionTMPs[i].text = i == m_BetOptionTMPs.Count - 1 ? "Max Bet" : Config.FormatMoney3(_BetValues[i]);
            if (isShow)
            {
                tf.parent.gameObject.SetActive(true);
            }
            else
            {
                tf.parent.gameObject.SetActive(false);
            }

            tf.DOLocalJump(tf.localPosition, 20f, 1, .1f);
            yield return new WaitForSeconds(.1f);
        }
    }

    private void _HandleBankerInfor(JObject data)
    {
        potValue = GetPotValue(data);
        handleUpdatePot(potValue);
        for (int i = 0; i < players.Count; i++)
        {
            PlayerViewLucky89 playerView = getPlayerView(players[i]);
            playerView.ShowIconBanker(false, gameRemaining);
            playerView.isBanker = false;
            players[i].setAg();
        }
        gameRemaining = getInt(data, "gameRemain");
        long ag = getLong(data, "ag");
        int idBanker = getInt(data, "pid");
        var playerBanker = getPlayerWithID(idBanker);
        bankerName = playerBanker != null ? playerBanker.namePl : "";
        PlayerViewLucky89 plViewLucky89 = getPlayerView(playerBanker);
        int idThisPLayer = Globals.User.userMain.Userid;
        var thisPlayer = getPlayerWithID(idThisPLayer);
        thisPlayerView = getPlayerView(thisPlayer);
        plViewLucky89.ShowIconBanker(true, gameRemaining);
        plViewLucky89.isBanker = true;
        plViewLucky89.setAg(ag);
    }

    private void _HandleAnyoneBets(JObject data)
    {
        float timeAction = 0f;
        Player player = players.Find(x => x.namePl.Equals((string)data["playerName"]));
        if (data.ContainsKey("timeAction"))
        {
            timeAction = getFloat(data, "timeAction") / 1000f;

            if (timeAction > 0)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    PlayerViewLucky89 playerView = getPlayerView(players[i]);
                    playerView.ShowAnimWaitBetTime(true, "white");
                    if (playerView.isBanker)
                    {
                        playerView.ShowAnimWaitBetTime(false);
                    }
                    if (stateGame != STATE_GAME.PLAYING && thisPlayerView != null)
                    {
                        thisPlayerView.ShowAnimWaitBetTime(false);
                    }
                }
                StartCoroutine(RunCountDownStartAndBet(timeAction, "ခုတ်လောင်းချိန်", false));
                m_DealerPVL89.ShowAnimWaitBetTime(false);
                if (stateGame != STATE_GAME.PLAYING) return;
                if (thisPlayerView != null && !thisPlayerView.isBanker)
                {
                    StartCoroutine(ShowBetOption());
                }
            }
        }
        else
        {
            playSound(SOUND_GAME.CLICK);
            if (player == null) return;
            long betChips = (long)data["chipBet"];
            PlayerViewLucky89 playerViewLucky89 = getPlayerView(player);
            playerViewLucky89.ShowHideBetChips(true, (int)betChips);
            player.ag -= betChips;
            player.setAg();
            player.updatePlayerView();
            if (player == thisPlayer)
            {
                foreach (TextMeshProUGUI tmp in m_BetOptionTMPs) tmp.transform.parent.gameObject.SetActive(false);
            }
        }
    }
    private PlayerViewLucky89 getPlayerView(Player player)
    {
        if (player == null)
            return null;

        var view = player.playerView as PlayerViewLucky89;
        if (view == null)
        {
            Debug.LogWarning($"[Lucky89] PlayerView is null or wrong type for pid={player.id}");
        }
        return view;
    }
    private void _HandleReceiveLuckyCards(JObject data)
    {
        // if (data == null) return;

        // string userName = data.Value<string>("userName");
        // if (string.IsNullOrEmpty(userName)) return;

        // JArray dataLuckyCards = (JArray)data["arr"];
        // if (dataLuckyCards == null || dataLuckyCards.Count == 0) return;

        // int score = data.Value<int?>("score") ?? 0;
        // int rate = data.Value<int?>("rate") ?? 1;

        // // Tìm PlayerView theo userName
        // PlayerViewLucky89 targetPV = null;
        // foreach (var pv in players)
        // {
        //     if (pv != null && pv != null &&
        //         pv.namePl == userName)
        //     {
        //         targetPV = getPlayerView(pv);
        //         break;
        //     }
        // }

        // if (targetPV == null) return;
        // if (score == 8)
        //     score = (int)SCORE.LUCKY_8;
        // else if (score == 9)
        //     score = (int)SCORE.LUCKY_9;

        // targetPV.isLucky = true;
        // List<Card> cards = targetPV.GetListCards();
        // for (int i = 0; i < cards.Count && i < dataLuckyCards.Count; i++)
        // {
        //     _RevealACard(
        //         cards[i],
        //         (int)dataLuckyCards[i],
        //         cards[i].transform.localEulerAngles
        //     );
        // }
        // targetPV
        //     .ShowRate(rate)
        //     .ShowScore(
        //         true,
        //         score,
        //         dataLuckyCards.Count,
        //         rate
        //     );

        // targetPV.ShowAnimWaitOpenCard(false);
        _HandleAnyoneReceivesLuckyCards(data);
    }

    private void _HandleAnyoneReceivesLuckyCards(JObject data)
    {
        if (data == null) return;
        StartCoroutine(handleData());

        //======================================================
        IEnumerator handleData()
        {
            yield return new WaitForSeconds(2.5f);
            string userName = data.Value<string>("userName");
            if (userName == Globals.User.userMain.Username) yield break;
            if (string.IsNullOrEmpty(userName)) yield break;

            JArray dataCards = (JArray)data["arr"];
            if (dataCards == null || dataCards.Count == 0) yield break;

            int score = data.Value<int?>("score") ?? 0;
            int rate = data.Value<int?>("rate") ?? 1;

            bool isDealerLucky = false;
            bool isThisPlayerLucky = false;

            // ===== Tìm player =====
            Player playerP = players.Find(x => x.namePl == userName);
            PlayerViewLucky89 pvl89 =
                playerP == null ? m_DealerPVL89 : (PlayerViewLucky89)playerP.playerView;

            if (pvl89 == null) yield break;

            // ===== Reveal bài =====
            List<Card> cardCs = pvl89.GetListCards();
            for (int i = 0; i < cardCs.Count && i < dataCards.Count; i++)
            {
                cardCs[i].setTextureWithCode((int)dataCards[i]);
                _RevealACard(
                    cardCs[i],
                    (int)dataCards[i],
                    cardCs[i].transform.localEulerAngles
                );
            }

            // ===== Chuẩn hóa score =====
            if (score == 8) score = (int)SCORE.LUCKY_8;
            else if (score == 9) score = (int)SCORE.LUCKY_9;

            pvl89.isLucky = score >= (int)SCORE.LUCKY_8;

            // ===== Show điểm + rate =====
            pvl89
                .ShowRate(rate)
                .ShowScore(true, score, dataCards.Count, rate);

            pvl89.ShowAnimWaitOpenCard(false);

            // ===== Flag =====
            if (pvl89 == m_DealerPVL89)
                isDealerLucky = pvl89.isLucky;
            else if (playerP != null && playerP.id == User.userMain.Userid)
                isThisPlayerLucky = pvl89.isLucky;

            if (stateGame == STATE_GAME.VIEWING) yield break;
        }
    }

    private void _HandleReceiveMyCards(JObject data)
    {
        if (stateGame != STATE_GAME.PLAYING)
        {
            return;
        }
        StartCoroutine(handleData());

        //======================================================
        void getAllACard(int myCardCode, bool updateCardsParent)
        {
            StartCoroutine(_DealCardsSequentially(myCardCode, updateCardsParent));
        }

        IEnumerator handleData()
        {
            _isRevealMyCards = false;
            JArray dataMyCards = (JArray)data["arr"];
            if (dataMyCards == null || dataMyCards.Count < 2)
                yield break;
            for (int i = 0; i < dataMyCards.Count; i++)
            {
                // listCardBig[i].gameObject.SetActive(true);
                listCardBig[i].setTextureWithCode((int)dataMyCards[i]);
                listCodeCard.Add((int)dataMyCards[i]);
            }
            if (m_DealerSG != null)
            {
                m_DealerSG.Initialize(true);
                m_DealerSG.AnimationState.SetAnimation(0, "chiabai", true);
            }
            boxTimeStart.SetActive(false);
            getAllACard(0, false);
            yield return new WaitForSeconds(CARD_FLYING_DURATION * (players.Count + 1));

            getAllACard(0, true);
            yield return new WaitForSeconds(CARD_FLYING_DURATION * (players.Count + 1));
            if (m_DealerSG != null)
            {
                m_DealerSG.Initialize(true);
                m_DealerSG.AnimationState.SetAnimation(0, "normal", true);
            }
            int myScore = (int)data["score"];
            PlayerViewLucky89 thisPVL89 = getPlayerView(thisPlayer);

            if (myScore == 8)
            {
                showButtonInPanelAction = false;
                myScore = (int)SCORE.LUCKY_8;
                thisPlayerView.isLucky = true;
            }

            else if (myScore == 9)
            {
                showButtonInPanelAction = false;
                myScore = (int)SCORE.LUCKY_9;
                thisPlayerView.isLucky = true;
            }


            yield return new WaitUntil(() => _isRevealMyCards);
            if (listCodeCard.Count > 2) yield break;
            List<Card> myCards = thisPlayerView.GetListCards();
            for (int i = 0; i < myCards.Count && i < dataMyCards.Count; i++)
            {
                _RevealACard(myCards[i], (int)dataMyCards[i], myCards[i].transform.localEulerAngles);
            }
            if (myScore == 8) myScore = (int)SCORE.LUCKY_8;
            else if (myScore == 9) myScore = (int)SCORE.LUCKY_9;
            thisPlayerView.ShowRate((int)data["rate"]).ShowScore(true, myScore, listCodeCard.Count, (int)data["rate"]);
            thisPlayerView.ShowAnimWaitOpenCard(false);
        }

        //======================================================
        IEnumerator _DealCardsSequentially(int myCardCode, bool updateCardsParent)
        {
            bool hasPlayerBanker = false;
            for (int i = 0; i < players.Count; i++)
            {
                PlayerViewLucky89 playerView = getPlayerView(players[i]);
                if (playerView.isBanker)
                {
                    hasPlayerBanker = true;
                }
            }
            if (!hasPlayerBanker)
            {
                StartCoroutine(_DrawCard(m_DealerPVL89, 0));
                yield return new WaitForSeconds(CARD_FLYING_DURATION);

                if (updateCardsParent)
                    m_DealerPVL89.UpdateCardsParentPositionAndRotation();
            }
            foreach (Player player in players)
            {
                bool isMe = player == thisPlayer;
                PlayerViewLucky89 playerView = (PlayerViewLucky89)player.playerView;

                StartCoroutine(_DrawCard(playerView, isMe ? myCardCode : 0));
                yield return new WaitForSeconds(CARD_FLYING_DURATION);

                if (updateCardsParent)
                    playerView.UpdateCardsParentPositionAndRotation();
            }
        }
    }

    private void _HandleAnyoneDrawsCard(JObject data)
    {
        if (data == null)
        {
            Debug.LogWarning("_HandleAnyoneDrawsCard called with null data");
            return;
        }
        if (data.ContainsKey("timeAction"))
        {
            DOVirtual.DelayedCall(2f, () =>
            {
                for (int i = 0; i < players.Count; i++)
                {
                    PlayerViewLucky89 plView = getPlayerView(players[i]);
                    plView.ShowAnimWaitOpenCard(true);
                }
                if (thisPlayerView != null)
                {
                    thisPlayerView.ShowAnimWaitOpenCard(false);
                }
            });
            float time = (float)data["timeAction"] / 1000;
            StartCoroutine(RunCountDownAction(time, "ရေတွက်ချိန်"));
        }

        if (thisPlayerView != null && !thisPlayerView.isBanker)
        {
            ShowPanelAction(data, 5f);
        }
        if (!data.ContainsKey("N"))
        {
            Debug.LogWarning("_HandleAnyoneDrawsCard missing 'N' field");
            return;
        }

        string name = (string)data["N"];
        Player player = players?.Find(x => x.namePl.Equals(name));
        PlayerViewLucky89 playerView = getPlayerView(player);
        bool isDealer = player == null;
        bool isMe = player == thisPlayer;

        if (playerView == null)
        {
            Debug.LogError($"_HandleAnyoneDrawsCard: playerView NULL for {name} (isDealer={isDealer})");
            return;
        }
        int cardCode = data["C"] != null ? data["C"].Value<int>() : 0;
        StartCoroutine(_DrawCard(playerView, isMe ? cardCode : 0));
        playerView.UpdateCardsParentPositionAndRotation();
        playerView.ShowAnimWaitOpenCard(false);
        if (isMe)
        {
            int score = data.ContainsKey("score") ? (int)data["score"] : 0;
            int rate = data.ContainsKey("rate") ? (int)data["rate"] : 0;

            listCodeCard.Add(cardCode);
            thisPlayerView.ShowScore(true, score, listCodeCard.Count, rate).ShowRate(rate);

            listCardBig[2].setTextureWithCode(cardCode);
            ActionDrawCard();
            if (listCodeCard.Count > 1)
            {
                List<Card> cardCs = thisPlayerView.GetListCards();
                int countToReveal = Mathf.Min(listCodeCard.Count - 1, cardCs.Count);

                for (int i = 0; i < countToReveal; i++)
                {
                    _RevealACard(cardCs[i], listCodeCard[i], cardCs[i].transform.localEulerAngles);
                }
            }
        }
    }
    private void _HandleCBC(JObject data)
    {
        if (thisPlayerView != null && thisPlayerView.isBanker && thisPlayerView.isLucky)
        {
            Debug.Log($"Tinh=))thisPlayerView.isBanker: {thisPlayerView.isBanker}");
            ShowPanelActionBankerLucky();
        }
    }
    private void _HandleCDCO(JObject data)
    {
        bool isDisableButtonDeclare3Card = true;
        var optTransfers = data["optTransfers"] as JArray;
        if (optTransfers != null)
        {
            foreach (var item in optTransfers)
            {
                int opt = item.Value<int>("opt");
                if (opt == 1)
                {
                    isDisableButtonDeclare3Card = item.Value<bool>("isDisable");
                    break;
                }
            }
        }
        if (thisPlayerView != null && thisPlayerView.isBanker)
        {
            ShowPanelAction(data, 0f, isDisableButtonDeclare3Card);
        }
    }
    private bool isBankerLuckyShowing = false;
    private Coroutine detectSwipeCoroutine;
    private void ShowPanelActionBankerLucky()
    {
        if (stateGame == STATE_GAME.VIEWING) return;
        if (isBankerLuckyShowing) return;
        isBankerLuckyShowing = true;
        DOTween.Kill("ActionTweenLucky");

        float timeAction = 5f;
        if (timeAction <= 0) return;
        panelAction.SetActive(true);
        imageCardRotate.gameObject.SetActive(true);
        imageCardRotate.transform.localRotation = Quaternion.Euler(Vector3.zero);
        for (int i = 0; i < listCodeCard.Count; i++)
        {
            listCardBig[i].transform.localPosition = new Vector3(0f, 120f, 0f);
            listCardBig[i].transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
        }
        imageCardRotate.transform
            .DORotate(new Vector3(0, 90, 0), 0.5f)
            .SetEase(Ease.InQuad)
            .SetId("ActionTweenLucky")
            .OnComplete(() =>
            {
                parentCardBig.transform.localPosition = Vector3.zero;
                parentCardBig.transform.localScale = Vector3.one;
                imageCardRotate.gameObject.SetActive(false);
                for (int i = 0; i < listCodeCard.Count; i++)
                {
                    listCardBig[i].gameObject.SetActive(true);
                }
                int last = listCardBig.Count - 1;
                for (int i = 0; i < listCardBig.Count; i++)
                {
                    var tween = listCardBig[i].transform
                        .DORotate(Vector3.zero, 0.5f)
                        .SetEase(Ease.Linear);

                    if (i == last)
                    {
                        tween.OnComplete(() =>
                        {
                            animHand.Initialize(true);
                            animHand.gameObject.SetActive(true);
                        });
                    }
                }
            });
        buttonDone.gameObject.SetActive(false);
        buttonDraw.gameObject.SetActive(false);
        buttonNotDraw.gameObject.SetActive(false);
        buttonDeclare3Card.gameObject.SetActive(false);
        if (detectSwipeCoroutine != null)
            StopCoroutine(detectSwipeCoroutine);

        detectSwipeCoroutine = StartCoroutine(DelaySwipe());
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(RunCountDownTimeAction(timeAction));
    }

    private IEnumerator DelaySwipe()
    {
        yield return new WaitForSeconds(0.5f);
        yield return DetectSwipe(true);
    }
    private void ShowPanelAction(JObject data, float timeDelay, bool isDisableButtonDeclare3Card = false)
    {
        if (stateGame == STATE_GAME.VIEWING) return;
        if (!data.ContainsKey("timeAction")) return;
        // StopAllCoroutines();
        DOTween.Kill("ActionTween");
        float timeAction = (getFloat(data, "timeAction") / 1000) - timeDelay; // fallback 5s
        if (timeAction > 0)
        {
            DOVirtual.DelayedCall(timeDelay, () =>
            {
                panelAction.SetActive(true);
                imageCardRotate.gameObject.SetActive(true);
                imageCardRotate.transform.localRotation = Quaternion.Euler(Vector3.zero);
                imageCardRotate.transform.DORotate(new Vector3(0, 90, 0), 0.5f).SetEase(Ease.InQuad).OnComplete(() =>
                {
                    parentCardBig.transform.localPosition = Vector3.zero;
                    parentCardBig.transform.localScale = Vector3.one;
                    imageCardRotate.gameObject.SetActive(false);
                    for (int i = 0; i < listCodeCard.Count; i++)
                    {
                        listCardBig[i].gameObject.SetActive(true);
                    }
                    for (int i = 0; i < listCardBig.Count; i++)
                    {
                        listCardBig[i].transform.localPosition = new Vector3(0f, 120f, 0f);
                        listCardBig[i].transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                        listCardBig[i].transform.DORotate(Vector3.zero, 0.5f)
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            animHand.Initialize(true);
                            animHand.gameObject.SetActive(true);
                        });
                    }
                });
                buttonDone.gameObject.SetActive(false);
                buttonDraw.gameObject.SetActive(false);
                buttonNotDraw.gameObject.SetActive(false);
                buttonDeclare3Card.gameObject.SetActive(false);
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    StartCoroutine(DetectSwipe(thisPlayerView.isLucky, false, isDisableButtonDeclare3Card));
                });
                if (countdownCoroutine != null)
                {
                    StopCoroutine(countdownCoroutine);
                }
                countdownCoroutine = StartCoroutine(RunCountDownTimeAction(timeAction));
            });
        }
    }
    private void ActionDrawCard()
    {
        imageCardDraw.gameObject.SetActive(true);
        imageCardDraw.transform.localRotation = Quaternion.Euler(Vector3.zero);
        imageCardDraw.transform.localPosition = new Vector3(1400, 1400, 0);
        Vector3[] path =
        {
        new Vector3(900, 800, 0),
        new Vector3(800, 700, 0),
        new Vector3(700, 600, 0),
        new Vector3(600, 500, 0),
        new Vector3(500, 400, 0),
        new Vector3(400, 300, 0),
        new Vector3(300, 200, 0),
        new Vector3(0, 120, 0)
    };

        float duration = 0.75f;
        imageCardDraw.transform
            .DOLocalPath(path, duration, PathType.CatmullRom)
            .SetEase(Ease.InOutQuad);
        imageCardDraw.transform
            .DOLocalRotate(new Vector3(0, 90, 0), duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                imageCardDraw.gameObject.SetActive(false);

                listCardBig[2].gameObject.SetActive(true);
                animHand.Initialize(true);
                animHand.gameObject.SetActive(true);
                StartCoroutine(DetectSwipe(false, true));
            });
    }
    private void ClickButtonDone()
    {
        if (thisPlayerView != null && thisPlayerView.isLucky && !thisPlayerView.isBanker)
        {
            SocketSend.SendDrawACardLucky89(0);
        }
        DisablePanelAction();
    }
    private void ClickButtonShowCard()
    {
        Debug.Log("ClickButtonShowCard");
        SocketSend.SendShowCard();
    }
    private void ActiveButtonDone()
    {
        buttonDone.gameObject.SetActive(true);
        buttonDone.interactable = true;
        buttonDraw.gameObject.SetActive(false);
        buttonNotDraw.gameObject.SetActive(false);
        buttonDeclare3Card.gameObject.SetActive(false);
    }

    private void DisablePanelAction()
    {
        animHand.gameObject.SetActive(false);
        buttonDone.interactable = false;
        parentCardBig.transform.DOLocalMove(new Vector3(137f, -206f, 0), 0.5f).SetEase(Ease.InQuad);
        parentCardBig.transform.DOScale(new Vector3(0.25f, 0.25f, 0.25f), 0.5f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            panelAction.SetActive(false);
            _isRevealMyCards = true;
            for (int i = 0; i < listCardBig.Count; i++)
            {
                listCardBig[i].gameObject.SetActive(false);
            }
        });

    }

    IEnumerator DetectSwipe(bool bankerLucky = false, bool isDrawCard = false, bool isDisableButtonDeclare3Card = false)
    {
        isTrackingSwipe = true;
        float minSwipeDistance = 50f;
        if (isDrawCard)
        {
            while (isTrackingSwipe)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    touchStartPos = Input.mousePosition;
                }
                if (Input.GetMouseButton(0))
                {
                    Vector2 touchEndPos = Input.mousePosition;
                    float distance = Vector2.Distance(touchStartPos, touchEndPos);
                    if (distance > minSwipeDistance)
                    {
                        listCardBig[1].transform.DOLocalMove(new Vector3(84f, 100f, 0), 0.5f).SetEase(Ease.Linear);
                        listCardBig[1].transform.DORotate(new Vector3(0f, 0f, -30f), 0.5f).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            // DisablePanelAction();
                            ActiveButtonDone();
                        });
                        isTrackingSwipe = false;
                        animHand.gameObject.SetActive(false);
                    }
                }
                yield return null;
            }
        }
        else
        {
            while (isTrackingSwipe)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    touchStartPos = Input.mousePosition;
                }
                if (Input.GetMouseButton(0))
                {
                    Vector2 touchEndPos = Input.mousePosition;
                    float distance = Vector2.Distance(touchStartPos, touchEndPos);
                    if (distance > minSwipeDistance)
                    {
                        animHand.gameObject.SetActive(false);
                        listCardBig[0].transform.DOLocalMove(new Vector3(-84f, 100f, 0f), 0.5f).SetEase(Ease.Linear);
                        listCardBig[0].transform.DORotate(new Vector3(0f, 0f, 30f), 0.5f).SetEase(Ease.Linear);
                        listCardBig[1].transform.SetParent(transform);
                        listCardBig[1].transform.SetParent(parentCardBig.transform);
                        if (bankerLucky)
                        {
                            // DisablePanelAction();
                            ActiveButtonDone();
                            isTrackingSwipe = false;
                        }
                        else
                        {
                            if (showButtonInPanelAction)
                            {
                                buttonDraw.gameObject.SetActive(true);
                                buttonNotDraw.gameObject.SetActive(true);
                                if (thisPlayerView != null && thisPlayerView.isBanker)
                                {
                                    if (!isDisableButtonDeclare3Card)
                                    {
                                        buttonDeclare3Card.gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        buttonDeclare3Card.gameObject.SetActive(false);
                                    }
                                }
                                buttonDone.gameObject.SetActive(false);
                            }
                            else
                            {
                                // DisablePanelAction();
                                ActiveButtonDone();
                            }
                            Debug.Log("aaaaa");
                            isTrackingSwipe = false;
                        }
                    }
                }

                yield return null;
            }
        }
    }
    private void _HandleFinishOtp1(JObject data)
    {
        if (panelAction.activeSelf)
        {
            DisablePanelAction();
            DOVirtual.DelayedCall(0.5f, () =>
            {
                StartCoroutine(handleData());
            });
        }
        else
        {
            StartCoroutine(handleData());
        }
        boxTimeAction.gameObject.SetActive(false);
        for (int i = 0; i < players.Count; i++)
        {
            PlayerViewLucky89 plview = getPlayerView(players[i]);
            plview.ShowAnimWaitOpenCard(false);
        }

        IEnumerator handleData()
        {
            // HandleData.DelayHandleLeave = 5f;
            stateGame = STATE_GAME.WAITING;

            List<Action> playerWinCbs = new(), playerLoseCbs = new();
            Action finalDealerCb = null;
            Player bankerPlayer = null;

            JArray playerResults = (JArray)data["declarePlayerTransferList"];
            if (playerResults == null)
            {
                Debug.LogError("[Lucky89] declarePlayerTransferList is null!");
                yield break;
            }
            // long totalChipWin = 0;
            foreach (JToken pData in playerResults)
            {
                int pid = pData["pid"]?.Value<int>() ?? -1;
                bool isBanker = pData["isBanker"]?.Value<bool>() ?? false;
                long chipWin = pData["chipWin"]?.Value<long>() ?? 0;
                // totalChipWin += chipWin;
                long ag = pData["ag"]?.Value<long>() ?? 0;
                int score = pData["score"]?.Value<int>() ?? 0;
                int rate = pData["rate"]?.Value<int>() ?? 1;
                JArray arrCard = (JArray)pData["arr"];
                var playerBanker = getPlayerWithID(pid);
                PlayerViewLucky89 playerView = getPlayerView(playerBanker);

                Player player = players.Find(x => x.id == pid);
                if (player == null)
                {
                    Debug.LogWarning($"[Lucky89] Player not found for pid={pid}, skip display.");
                    continue;
                }
                if (playerView == null)
                {
                    Debug.LogWarning($"[Lucky89] PlayerView is null for pid={pid}, skip.");
                    continue;
                }

                if (isBanker)
                    bankerPlayer = player;
                if (arrCard != null && arrCard.Count > 0)
                {
                    List<Card> cardCs = playerView.GetListCards();
                    for (int i = 0; i < arrCard.Count && i < cardCs.Count; i++)
                        _RevealACard(cardCs[i], arrCard[i]?.Value<int>() ?? -1, cardCs[i].transform.localEulerAngles);
                }
                if (playerView != null)
                {
                    try
                    {
                        playerView.ShowScore(true, score, arrCard.Count, rate);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Lucky89] Error ShowScore pid={pid}: {ex}");
                    }

                    try
                    {
                        playerView.ShowRate(rate);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Lucky89] Error ShowRate pid={pid}: {ex}");
                    }
                    try
                    {
                        playerView.ShowAnimResult(true, chipWin);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Lucky89] Error ShowRate pid={pid}: {ex}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Lucky89] Cannot show score, playerView is null for pid={pid}");
                }
            }
            DOVirtual.DelayedCall(0.5f, () =>
            {
                potValue = GetPotValue(data);
                handleUpdatePot(potValue);
            });
        }
    }
    private void _HandleFinishGame(JObject data)
    {
        if (panelAction.activeSelf)
        {
            DisablePanelAction();
            DOVirtual.DelayedCall(0.5f, () =>
            {
                StartCoroutine(handleData());
            });
        }
        else
        {
            StartCoroutine(handleData());
        }
        boxTimeAction.gameObject.SetActive(false);
        for (int i = 0; i < players.Count; i++)
        {
            PlayerViewLucky89 plview = getPlayerView(players[i]);
            plview.ShowAnimWaitOpenCard(false);
        }
        IEnumerator handleData()
        {
            HandleData.DelayHandleLeave = 5f;
            stateGame = STATE_GAME.WAITING;

            List<Action> playerWinCbs = new(), playerLoseCbs = new();
            Action finalDealerCb = null;
            Player bankerPlayer = null;

            JArray playerResults = (JArray)data["declarePlayerTransferList"];
            if (playerResults == null)
            {
                Debug.LogError("[Lucky89] declarePlayerTransferList is null!");
                yield break;
            }
            long totalChipWin = 0;
            foreach (JToken pData in playerResults)
            {
                int pid = pData["pid"]?.Value<int>() ?? -1;
                bool isBanker = pData["isBanker"]?.Value<bool>() ?? false;
                long chipWin = pData["chipWin"]?.Value<long>() ?? 0;
                totalChipWin += chipWin;
                long ag = pData["ag"]?.Value<long>() ?? 0;
                int score = pData["score"]?.Value<int>() ?? 0;
                int rate = pData["rate"]?.Value<int>() ?? 1;
                JArray arrCard = (JArray)pData["arr"];
                var playerBanker = getPlayerWithID(pid);
                PlayerViewLucky89 playerView = getPlayerView(playerBanker);

                Player player = players.Find(x => x.id == pid);
                if (player == null)
                {
                    Debug.LogWarning($"[Lucky89] Player not found for pid={pid}, skip display.");
                    continue;
                }
                if (playerView == null)
                {
                    Debug.LogWarning($"[Lucky89] PlayerView is null for pid={pid}, skip.");
                    continue;
                }

                if (isBanker)
                    bankerPlayer = player;
                if (arrCard != null && arrCard.Count > 0)
                {
                    List<Card> cardCs = playerView.GetListCards();
                    for (int i = 0; i < arrCard.Count && i < cardCs.Count; i++)
                        _RevealACard(cardCs[i], arrCard[i]?.Value<int>() ?? -1, cardCs[i].transform.localEulerAngles);
                }
                if (playerView != null)
                {
                    Debug.Log($"ChayVaoDay: chipwwin: {chipWin}");
                    playerView.ShowScore(true, score, arrCard.Count, rate);
                    playerView.ShowRate(rate);
                    playerView.ShowAnimResult(true, chipWin);
                }
                else
                {
                    Debug.LogWarning($"[Lucky89] Cannot show score, playerView is null for pid={pid}");
                }
                if (!isBanker)
                {
                    if (chipWin >= 0)
                        playerWinCbs.Add(() => StartCoroutine(playerWinChips(pid, chipWin, ag, bankerPlayer)));
                    else
                        playerLoseCbs.Add(() => StartCoroutine(playerLoseChips(pid, chipWin, ag, bankerPlayer)));
                    player.ag = ag;
                    player.setAg();
                    player.updatePlayerView();
                }
                else
                {
                    if (gameRemaining == 1)
                    {
                        DOVirtual.DelayedCall(0.5f, () =>
                        {
                            // Debug.Log($"Pot value before final update: {potValue}");
                            if (thisPlayerView != null && thisPlayerView.isBanker)
                            {
                                if (potValue > 0)
                                {
                                    StartCoroutine(playerWinChips(pid, potValue * 94 / 100, ag - potValue * 94 / 100, player, true));
                                }
                            }
                            else
                            {
                                if (potValue > 0)
                                {
                                    StartCoroutine(playerWinChips(pid, potValue * 94 / 100, ag - potValue * 94 / 100, player, true));
                                }
                            }
                            player.ag = ag;
                            player.setAg();
                        });
                    }
                    player.ag = ag;
                    player.setAg();
                }
            }

            if (gameRemaining == 1)
            {
                potValue -= totalChipWin * 100 / 94;
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    handleUpdatePot(potValue);
                    DOVirtual.DelayedCall(0.75f, () =>
                    {
                        potValue = GetPotValue(data);
                        handleUpdatePot(potValue);
                    });
                });
            }
            else
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    potValue = GetPotValue(data);
                    handleUpdatePot(potValue);
                });
            }
            if (bankerPlayer == null)
                Debug.LogWarning("[Lucky89] BankerPlayer not found in player list.");
            foreach (Action cb in playerLoseCbs) cb.Invoke();
            if (playerLoseCbs.Count > 0) yield return new WaitForSeconds(2 * LOSE_CHIP_DURATION + 1);
            foreach (Action cb in playerWinCbs) cb.Invoke();
            if (playerWinCbs.Count > 0) yield return new WaitForSeconds(2 * WIN_CHIP_DURATION + 1);
            finalDealerCb?.Invoke();

            yield return new WaitForSeconds(1f);
            foreach (Player p in players)
            {
                PlayerViewLucky89 pView = (PlayerViewLucky89)p.playerView;
                List<Card> cards = pView.GetListCards();

                for (int i = 0; i < cards.Count; i++)
                {
                    Card c = cards[i];
                    if (c == null || !c.gameObject.activeSelf) continue;

                    Vector3 originalPos = c.transform.position;
                    Quaternion originalRot = c.transform.rotation;
                    c.transform.DOMove(potTransform.position, 0.5f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            c.gameObject.SetActive(false);
                            c.transform.position = originalPos;
                            c.transform.rotation = originalRot;
                        });

                    yield return new WaitForSeconds(0.05f);
                }
            }

            yield return new WaitForSeconds(0.6f);

            foreach (Player p in players)
            {
                ((PlayerViewLucky89)p.playerView)
                    .ShowHideBetChips(false)
                    .ShowScore(false, 0, 0)
                    .ShowAnimResult(false, 0)
                    .ShowRate(0)
                    .HideAllCards();
            }
            _WaitForFinishCompleteCb?.Invoke();
            _WaitForFinishCompleteCb = null;
            checkAutoExit();
        }
    }
    long GetPotValue(JObject data)
    {
        if (data.ContainsKey("pot"))
            return (long)data["pot"];

        if (data.ContainsKey("bankerInfoTransfer"))
            return (long)(data["bankerInfoTransfer"]?["pot"] ?? 0);

        return potValue;
    }

    IEnumerator playerLoseChips(int pId, long changedChips, long currentChips, Player bankerPlayer)
    {
        if (changedChips >= 0) yield break;
        Transform targetTransform = potTransform;
        for (int i = 0; i < 3; i++)
        {
            Transform chipTf = null;
            foreach (Transform childTf in m_ChipsTf)
                if (!childTf.gameObject.activeSelf)
                {
                    chipTf = childTf;
                    break;
                }
            if (chipTf == null) chipTf = Instantiate(m_PrefabChipTf, m_ChipsTf);

            playSound(SOUND_GAME.GET_CHIP);
            chipTf.gameObject.SetActive(true);
            chipTf.position = (Vector2)(players.Find(x => x.id == pId).playerView.transform.position)
                              + new Vector2(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f));
            chipTf.DOMove(targetTransform.position, 2 * LOSE_CHIP_DURATION)
                  .SetEase(Ease.OutQuad)
                  .OnComplete(() => chipTf.gameObject.SetActive(false));

            yield return new WaitForSeconds(.05f);
        }

        yield return new WaitForSeconds(LOSE_CHIP_DURATION);

        Player player = players.Find(x => x.id == pId);
        player.playerView.effectFlyMoney(changedChips);
        player.updatePlayerView();
    }

    IEnumerator playerWinChips(int pId, long changedChips, long currentChips, Player bankerPlayer, bool isNotUpdateAg = false)
    {
        // if (changedChips <= 0) yield break;

        Transform sourceTransform = potTransform;
        for (int i = 0; i < 3; i++)
        {
            Transform chipTf = null;
            foreach (Transform childTf in m_ChipsTf)
                if (!childTf.gameObject.activeSelf)
                {
                    chipTf = childTf;
                    break;
                }
            if (chipTf == null) chipTf = Instantiate(m_PrefabChipTf, m_ChipsTf);

            playSound(SOUND_GAME.GET_CHIP);
            chipTf.gameObject.SetActive(true);
            chipTf.position = sourceTransform.position;
            chipTf.DOMove((Vector2)(players.Find(x => x.id == pId).playerView.transform.position)
                          + new Vector2(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f)),
                          2 * WIN_CHIP_DURATION)
                  .SetEase(Ease.OutQuart)
                  .OnComplete(() => chipTf.gameObject.SetActive(false));

            yield return new WaitForSeconds(.05f);
        }

        yield return new WaitForSeconds(3 * WIN_CHIP_DURATION);

        Player player = players.Find(x => x.id == pId);
        player.updatePlayerView();
        player.playerView.effectFlyMoney(changedChips);
        playSound(SOUND_GAME.REWARD);
    }

    public override void HandlerTip(JObject data)
    {
        if (User.userMain.AG < agTable) return;
        int chips = (int)data["AGTip"];
        thisPlayer.playerView.effectFlyMoney(-chips);
        User.userMain.AG -= chips;
        thisPlayer.ag = User.userMain.AG;
        thisPlayer.updatePlayerView();
        m_DealerPVL89.effectFlyMoney(chips);
        m_DealerSG.AnimationState.SetAnimation(0, "kiss", false);
        StartCoroutine(ShowThanksDialog());

        IEnumerator ShowThanksDialog()
        {
            GameObject parentObject = m_TipChipsTMP.transform.parent.gameObject;
            m_TipChipsTMP.text = Config.FormatNumber(chips);
            string playerName = (string)data["N"];
            m_TipThanksTMP.text = (playerName.Length >= 7 ? ((string)data["N"]).Substring(0, 7) + "..., " : playerName + ", ") + Globals.Config.getTextConfig("tip_thanks_1");
            parentObject.SetActive(true);
            yield return new WaitForSeconds(3f);
            parentObject.SetActive(false);
        }
    }

    public void handleUpdatePot(long potValue)
    {
        string curJackPotBinh = potValue.ToString();

        int indexRun = curJackPotBinh.Length - 1;
        for (int i = listTextJackPot.Count - 1; i >= 0; i--)
        {
            if (indexRun >= 0)
                listTextJackPot[i].text = curJackPotBinh[indexRun] + "";
            else
                listTextJackPot[i].text = "0";

            indexRun--;
            StartCoroutine(animateJackPot(listTextJackPot[i].gameObject));
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
    private void _RevealACard(Card cardC, int cardCode, Vector3 targetRotV3)
    {
        cardC.DOComplete();
        if (cardCode <= 0)
        {
            cardC.setTextureWithCode(0);
            cardC.transform.DOLocalRotate(targetRotV3, CARD_ROTATING_DURATION).SetEase(Ease.InQuad);
        }
        else
        {
            playSound(SOUND_GAME.CARD_FLIP_1);
            cardC.transform.DOLocalRotate(new Vector3(0, 90, 0) + targetRotV3, CARD_ROTATING_DURATION).SetEase(Ease.InQuad).OnComplete(() =>
            {
                cardC.setTextureWithCode(cardCode);
                cardC.transform.DOLocalRotate(targetRotV3, CARD_ROTATING_DURATION).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    cardC.transform.localEulerAngles = new(cardC.transform.localEulerAngles.x, 0, cardC.transform.localEulerAngles.z);
                });
            });
        }
    }



    private void _MoveACard(Transform cardTf, Vector2 targetPosV2)
    {
        cardTf.DOComplete();
        cardTf.DOLocalMove(targetPosV2, CARD_FLYING_DURATION).SetEase(Ease.OutQuad);
    }
    private IEnumerator _DrawCard(PlayerViewLucky89 playerView, int cardCode = 0)
    {
        playSound(SOUND_GAME.CARD_FLIP_1);
        Card cardC = playerView.GetACard();
        if (cardC == null) yield break;
        Transform cardParentTf = cardC.transform.parent;
        RectTransform cardRT = cardC.GetComponent<RectTransform>();
        Vector2 targetPosV2 = cardRT.anchoredPosition;
        Vector3 targetRotV3 = cardRT.transform.localEulerAngles;
        cardRT.SetParent(transform);
        float startOffsetY = 150f;
        cardRT.anchoredPosition = new Vector2(0, startOffsetY);
        cardRT.localRotation = Quaternion.identity;
        cardRT.gameObject.SetActive(true);
        cardRT.SetParent(cardParentTf);
        _MoveACard(cardRT, targetPosV2);
        _RevealACard(cardC, cardCode, targetRotV3);
        yield return new WaitForSeconds(CARD_FLYING_DURATION);
    }
    private void _DistributeCardsToAPlayer(PlayerViewLucky89 playerView, List<int> codes, int rate, int score, int cardCount = 0)
    {
        playSound(SOUND_GAME.DISPATCH_CARD);
        playerView.HideAllCards();
        List<Card> cardCs = playerView.GetListCards();
        for (int i = 0; i < codes.Count; i++)
        {
            if (i < codes.Count) StartCoroutine(_DrawCard(playerView, codes[i]));
            int totalCode = 0; foreach (int code in codes) totalCode += code;
            if (playerView != null)
            {
                playerView.UpdateCardsParentPositionAndRotation().ShowRate(rate).ShowScore(totalCode > 0, score, cardCount, rate);
            }
            // Debug.Log($"_DistributeCardsToAPlayer: Player {playerView.name} //score: {score}//rate: {rate}//cardCount: {cardCount}");
        }
    }

    private class DataPlayer
    {
        public List<int> cardCodes = new();
        public Player PlayerP;
        public int rate, score;
        public int cardCount;
    }
}
