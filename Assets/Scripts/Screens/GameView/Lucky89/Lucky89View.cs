using System;
using System.Collections;
using System.Collections.Generic;
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
    private bool? _DrawACard = null;
    private int _CurBankerId;
    private Vector2 touchStartPos;
    private bool isTrackingSwipe = false;
    private bool _isRevealMyCards = false;
    private bool showButtonInPanelAction = true;
    long potValue = 0;

    protected override void Awake()
    {
        base.Awake();
        agTable = 0;
        SocketSend.sendUpdateJackpot((int)GAMEID.SHAN_KOE_MEE);
    }
    protected override void Start()
    {
        base.Start();
        m_DealerSG.AnimationState.Complete += (x) => m_DealerSG.AnimationState.SetAnimation(0, "normal", true);
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
        ActionDrawCard();
        playSound(SOUND_GAME.CLICK);
        if (thisPlayerView != null && thisPlayerView.isBanker)
        {
            SocketSend.SendEventBanker(2);
            Debug.Log($"Tinh=))Send: Draw And Declare");
        }
        else
        {
            SocketSend.SendDrawACardLucky89(1);
        }
    }
    public void DoClickDontDraw()
    {
        DisablePanelAction();
        playSound(SOUND_GAME.CLICK);
        if (thisPlayerView != null && thisPlayerView.isBanker)
        {
            SocketSend.SendEventBanker(3);
            Debug.Log($"Tinh=))Send: Don't Draw And Declare");
        }
        else
        {
            SocketSend.SendDrawACardLucky89(0);
        }
    }
    public void DoClickDeclareWith_3Card()
    {
        panelAction.SetActive(false);
        playSound(SOUND_GAME.CLICK);
        SocketSend.SendEventBanker(1);
        Debug.Log($"Tinh=))Send: Declare with 3 card");
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
        List<int> coefficients = new() { 1, 5, 10, 50, 100 };
        foreach (int coefficient in coefficients) _BetValues.Add(agTable * coefficient);
    }
    protected override void updatePositionPlayerView()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].playerView.transform.localPosition = listPosView[i];
            players[i].updateItemVip(players[i].vip);
            PlayerViewLucky89 pv = (PlayerViewLucky89)players[i].playerView;
            pv.SetBetPosition(i);
            pv.SetCardPosition(i);
            pv.SetIconBankerPosition(i);
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
        handleUpdatePot(data);
        setGameInfo(m: betValue, id: tableId, maxBett: 0);
        JObject bankerInfo = data.ContainsKey("bankerInfoTransfer") ? (JObject)data["bankerInfoTransfer"] : null;
        int bankerId = bankerInfo != null && bankerInfo.ContainsKey("pid") ? (int)bankerInfo["pid"] : -1;
        int gameRemaining = bankerInfo.ContainsKey("gameRemain") ? (int)bankerInfo["gameRemain"] : 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerView != null)
                Destroy(players[i].playerView.gameObject);
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
        thisPlayer.playerView.setDark(true);
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
            PlayerViewLucky89 pv = (PlayerViewLucky89)player.playerView;
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
            }
            else
            {
                pv.ShowIconBanker(false);
            }
        }

        updatePositionPlayerView();
        foreach (JObject jPl in dataPlayers)
        {
            int pid = (int)jPl["id"];
            Player player = players.Find(x => x.id == pid);
            if (player == null) continue;
            PlayerViewLucky89 pv = player.playerView as PlayerViewLucky89;
            if (pv == null)
            {
                player.playerView = pv;
            }

            JArray arrCards = jPl.ContainsKey("Arr") ? (JArray)jPl["Arr"] : new JArray();
            List<int> cardCodes = new List<int>();
            foreach (JToken card in arrCards)
                cardCodes.Add((int)card);

            int rate = jPl.ContainsKey("rate") ? (int)jPl["rate"] : 0;
            int score = jPl.ContainsKey("score") ? (int)jPl["score"] : 0;

            _DistributeCardsToAPlayer(pv, cardCodes, rate, score);
        }


        Debug.Log($"Tinh=))handleVTable done - total players: {players.Count}, bankerId: {bankerId}");
    }

    public override void handleSTable(string strData)
    {
        Debug.Log($"Tinh=))handleSTable: {strData}");

        _WaitForFinishCompleteCb = () =>
        {
            JObject data = JObject.Parse(strData);
            int tableId = data.ContainsKey("Id") ? (int)data["Id"] : 0;
            int betValue = data.ContainsKey("M") ? (int)data["M"] : 0;
            int maxBetValue = data.ContainsKey("maxBet") ? (int)data["maxBet"] : 0;
            handleUpdatePot(data);
            setGameInfo(m: betValue, id: tableId, maxBett: maxBetValue);
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playerView != null)
                    Destroy(players[i].playerView.gameObject);
            }
            players.Clear();
            JArray dataPlayers = (JArray)data["ArrP"];
            if (dataPlayers == null)
            {
                Debug.LogWarning("handleSTable: ArrP null!");
                return;
            }

            for (int i = 0; i < dataPlayers.Count; i++)
            {
                JObject jPl = (JObject)dataPlayers[i];
                Player player = new Player();
                readDataPlayer(player, jPl);

                player.playerView = createPlayerView();

                if (player.id == User.userMain.Userid)
                {
                    thisPlayer = player;
                    players.Insert(0, thisPlayer);
                    thisPlayerView = getPlayerView(player);
                }
                else
                {
                    players.Add(player);
                }
            }
            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                PlayerViewLucky89 pv = (PlayerViewLucky89)player.playerView;
                player.updatePlayerView();
                pv
                    .SetBetPosition(i)
                    .ShowHideBetChips(player.agBet > 0, player.agBet)
                    .HideAllCards()
                    .UpdateCardsParentPositionAndRotation();

                pv.SetCardPosition(i);
                pv.SetIconBankerPosition(i);
                if (pv.isBanker)
                {
                    pv.ShowIconBanker(true);
                    Debug.Log($"Tinh=))BatIconBanker");
                }
                else
                {
                    pv.ShowIconBanker(false);
                }
            }

            updatePositionPlayerView();
            bool isSD = data.ContainsKey("issd") && (bool)data["issd"];
            bool noLimited = data.ContainsKey("noLimited") && (bool)data["noLimited"];
            int waitTime = data.ContainsKey("waitTime") ? (int)data["waitTime"] : 0;
            int bankerWaitTime = data.ContainsKey("bankerWaitTime") ? (int)data["bankerWaitTime"] : 0;

            Debug.Log($"handleSTable => TableId={tableId}, Bet={betValue}, State={data["S"]}, issd={isSD}, noLimited={noLimited}, waitTime={waitTime}, bankerWaitTime={bankerWaitTime}");
        };

        // ====== Chạy callback nếu không ở trạng thái VIEWING ======
        if (stateGame != STATE_GAME.VIEWING)
        {
            _WaitForFinishCompleteCb?.Invoke();
            _WaitForFinishCompleteCb = null;
        }

        stateGame = STATE_GAME.WAITING;
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
        handleUpdatePot(data);
        setGameInfo(m: betValue, id: tableId, maxBett: 0);
        JObject bankerInfo = data.ContainsKey("bankerInfoTransfer") ? (JObject)data["bankerInfoTransfer"] : null;
        int bankerId = bankerInfo != null && bankerInfo.ContainsKey("pid") ? (int)bankerInfo["pid"] : -1;
        int gameRemaining = bankerInfo.ContainsKey("gameRemain") ? (int)bankerInfo["gameRemain"] : 0;
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
            PlayerViewLucky89 pv = (PlayerViewLucky89)player.playerView;
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
                pv.ShowIconBanker(false);
            }
        }

        updatePositionPlayerView();
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

                PlayerViewLucky89 pv = (PlayerViewLucky89)dp.PlayerP.playerView;
                _DistributeCardsToAPlayer(pv, dp.cardCodes, dp.rate, dp.score);

                // if (pv.isBanker)
                // {
                //     Debug.Log($"Banker: {dp.PlayerP.namePl}, score={dp.score}, rate={dp.rate}");
                //     pv.ShowIconBanker(true, gameRemaining);
                // }

            }
        }
    }


    public override void handleLTable(JObject data)
    {
        if (_WaitForFinishCompleteCb != null) _WaitForFinishCompleteCb += () => base.handleLTable(data);
        else base.handleLTable(data);
        DOVirtual.DelayedCall(1f, () =>
        {
            if (thisPlayerView != null && thisPlayerView.isBanker)
            {
                SocketSend.sendUAG();
            }
        });
    }
    private void _HandleStartGame(JObject data)
    {
        showButtonInPanelAction = true;
        _isRevealMyCards = false;
        listCodeCard.Clear();
        playSound(SOUND_GAME.START_GAME);
        // stateGame = STATE_GAME.PLAYING;
        float time = (float)data["timeAction"] / 1000;
        StartCoroutine(RunCountDownStartAndBet(time, "Start in"));
        m_DealerPVL89.ShowHideBetChips(false).ShowAnimResult(false, 0).ShowScore(false, 0, 0).ShowRate(0).HideAllCards();
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
        }
        else
        {
            StartCoroutine(ShowBetOption(false));
        }
    }
    IEnumerator RunCountDownAction(float timeAction, string content)
    {
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
        imageTimeActionRemain.fillAmount = 1f;
        imageTimeActionRemain.DOFillAmount(0f, timeCountDown);
        while (timeCountDown > 0)
        {
            textTimeActionCountDown.text = Mathf.CeilToInt(timeCountDown).ToString();
            yield return new WaitForSeconds(1f);
            timeCountDown -= 1f;
        }
        panelAction.SetActive(false);
        DisablePanelAction();
        // DoClickDontDraw();
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
        handleUpdatePot(data);
        for (int i = 0; i < players.Count; i++)
        {
            PlayerViewLucky89 playerView = getPlayerView(players[i]);
            playerView.ShowIconBanker(false);
            playerView.isBanker = false;
        }
        int idBanker = getInt(data, "pid");
        int gameRemaining = getInt(data, "gameRemain");
        long ag = getLong(data, "ag");
        var playerBanker = getPlayerWithID(idBanker);
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
                    if (thisPlayerView.isBanker)
                    {
                        thisPlayerView.ShowAnimWaitBetTime(false);
                    }
                }
                // if (stateGame != STATE_GAME.PLAYING) return;
                if (!thisPlayerView.isBanker)
                {
                    StartCoroutine(ShowBetOption());
                }
                StartCoroutine(RunCountDownStartAndBet(timeAction, "Bet Time", false));
                m_DealerPVL89.ShowAnimWaitBetTime(false);
            }
        }
        playSound(SOUND_GAME.CLICK);
        if (player == null) return;
        int betChips = (int)data["chipBet"];
        ((PlayerViewLucky89)player.playerView).ShowHideBetChips(true, betChips);
        player.setTurn(false);
        player.ag -= ((PlayerViewLucky89)player.playerView).GetBetValue();
        player.updatePlayerView();
        if (player == thisPlayer)
        {
            foreach (TextMeshProUGUI tmp in m_BetOptionTMPs) tmp.transform.parent.gameObject.SetActive(false);
        }
    }
    private PlayerViewLucky89 getPlayerView(Player player)
    {
        if (player != null)
        {
            return (PlayerViewLucky89)player.playerView;
        }
        return null;

    }
    private void _HandleReceiveMyCards(JObject data)
    {
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
            PlayerViewLucky89 thisPVL89 = (PlayerViewLucky89)thisPlayer.playerView;

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
            thisPlayerView.ShowRate((int)data["rate"]).ShowScore(true, myScore, listCodeCard.Count);
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


    // private void _HandleAnyoneReceivesLuckyCards(JObject data)
    // {
    //     StartCoroutine(handleData());
    //     //======================================================
    //     IEnumerator handleData()
    //     {
    //         yield return new WaitForSeconds(2 * CARD_FLYING_DURATION);
    //         JArray dataLuckyPlayers = JArray.Parse((string)data["data"]);
    //         bool isDealerLucky = false, isThisPlayerLucky = false;
    //         foreach (JToken dataLucky in dataLuckyPlayers)
    //         {
    //             Player playerP = players.Find(x => x.namePl.Equals((string)dataLucky["N"]));
    //             PlayerViewLucky89 pvl89 = playerP == null ? m_DealerPVL89 : (PlayerViewLucky89)playerP.playerView;
    //             JArray dataCards = (JArray)dataLucky["arr"];
    //             List<Card> cardCs = pvl89.GetListCards();
    //             for (int i = 0; i < dataCards.Count; i++) _RevealACard(cardCs[i], (int)dataCards[i], cardCs[i].transform.localEulerAngles);
    //             int score = (int)dataLucky["score"];
    //             pvl89.ShowRate((int)dataLucky["rate"]).ShowScore(true, score);
    //             if (score >= (int)SCORE.LUCKY_8)
    //             {
    //                 if (pvl89 == m_DealerPVL89) isDealerLucky = true;
    //                 else if (playerP.id == User.userMain.Userid) isThisPlayerLucky = true;
    //             }
    //         }
    //         if (stateGame == STATE_GAME.VIEWING) yield break;
    //     }
    // }
    // private void _HandleAnyoneTimeOut(JObject data)
    // {
    //     StartCoroutine(handleData());
    //     //======================================================
    //     IEnumerator handleData()
    //     {
    //         Player player = players.Find(x => x.namePl.Equals((string)data["NN"]));
    //         float time = (float)data["T"] / 1000;
    //         if (player != null)
    //         {
    //             if (player == thisPlayer)
    //             {
    //                 float timeVibrate = -1f;
    //                 if (_DrawACard == true) SocketSend.SendDrawACardLucky89(1);
    //                 else if (_DrawACard == false) SocketSend.SendDrawACardLucky89(0);
    //                 else timeVibrate = 3f;
    //                 player.setTurn(true, time, timeVibrate: timeVibrate);
    //                 // _IsMyDrawTime = true;
    //                 // _SetTickDraw(false, 0);
    //                 yield return new WaitForSeconds(time);
    //                 // _IsMyDrawTime = false;
    //             }
    //             else player.setTurn(true, time, timeVibrate: -1f);
    //         }
    //     }
    // }
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
                    thisPlayerView.ShowAnimWaitOpenCard(false);
                }
            });
            float time = (float)data["timeAction"] / 1000;
            StartCoroutine(RunCountDownAction(time, "Countdown"));
        }

        if (thisPlayerView != null && !thisPlayerView.isBanker)
        {
            ShowPanelAction(data, 2.5f);
        }
        if (!data.ContainsKey("N"))
        {
            Debug.LogWarning("_HandleAnyoneDrawsCard missing 'N' field");
            return;
        }

        string name = (string)data["N"];
        Player player = players?.Find(x => x.namePl.Equals(name));
        bool isDealer = player == null;
        bool isMe = !isDealer && player == thisPlayer;

        // Nếu là dealer => dùng view dealer, nếu không => dùng view player (có thể null)
        PlayerViewLucky89 playerView = isDealer
            ? m_DealerPVL89
            : (PlayerViewLucky89)(player?.playerView);

        if (playerView == null)
        {
            Debug.LogError($"_HandleAnyoneDrawsCard: playerView NULL for {name} (isDealer={isDealer})");
            return;
        }
        int cardCode = data["C"] != null ? data["C"].Value<int>() : 0;

        // 🚀 Luôn chơi hiệu ứng rút bài (dù C = 0, nghĩa là chưa biết lá thật)
        StartCoroutine(_DrawCard(playerView, isMe ? cardCode : 0));
        playerView.UpdateCardsParentPositionAndRotation();
        playerView.ShowAnimWaitOpenCard(false);
        // Hiển thị điểm và rate nếu là chính mình
        if (isMe)
        {
            int score = data.ContainsKey("score") ? (int)data["score"] : 0;
            int rate = data.ContainsKey("rate") ? (int)data["rate"] : 0;

            playerView.ShowScore(true, score, listCodeCard.Count).ShowRate(rate);

            listCodeCard.Add(cardCode);
            listCardBig[2].setTextureWithCode(cardCode);

            if (listCodeCard.Count > 1)
            {
                List<Card> cardCs = playerView.GetListCards();
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
        if (thisPlayerView != null && thisPlayerView.isBanker)
        {
            ShowPanelAction(data, 0f);
        }
    }
    private void ShowPanelActionBankerLucky()
    {
        float timeAction = 5f;
        if (timeAction > 0)
        {
            panelAction.SetActive(true);
            imageCardRotate.gameObject.SetActive(true);
            imageCardRotate.transform.localRotation = Quaternion.Euler(Vector3.zero);
            for (int i = 0; i < listCodeCard.Count; i++)
            {
                listCardBig[i].transform.localPosition = new Vector3(0f, 120f, 0f);
                listCardBig[i].transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
            }
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
                    listCardBig[i].transform.DORotate(Vector3.zero, 0.5f)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        animHand.Initialize(true);
                        animHand.gameObject.SetActive(true);
                    });
                }
            });
            buttonDraw.gameObject.SetActive(false);
            buttonNotDraw.gameObject.SetActive(false);
            buttonDeclare3Card.gameObject.SetActive(false);
            StartCoroutine(DetectSwipe(true));
            StartCoroutine(RunCountDownTimeAction(timeAction));
        }
    }
    private void ShowPanelAction(JObject data, float timeDelay)
    {
        if (!data.ContainsKey("timeAction")) return;
        float timeAction = getFloat(data, "timeAction") / 1000 - timeDelay; // fallback 5s
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
                buttonDraw.gameObject.SetActive(false);
                buttonNotDraw.gameObject.SetActive(false);
                buttonDeclare3Card.gameObject.SetActive(false);
                StartCoroutine(DetectSwipe(thisPlayerView.isLucky));
                StartCoroutine(RunCountDownTimeAction(timeAction));
            });
        }
    }
    private void ActionDrawCard()
    {
        imageCardDraw.gameObject.SetActive(true);
        imageCardDraw.transform.localRotation = Quaternion.Euler(Vector3.zero);
        imageCardDraw.transform.localPosition = new Vector3(900, 800, 0);
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

        float duration = 1.5f;
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

    private void DisablePanelAction()
    {
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

    IEnumerator DetectSwipe(bool bankerLucky = false, bool isDrawCard = false)
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
                            DisablePanelAction();
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
                            DisablePanelAction();
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
                                    buttonDeclare3Card.gameObject.SetActive(true);
                                }
                            }
                            else
                            {
                                DisablePanelAction();
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
    private void _HandleFinishGame(JObject data)
    {
        boxTimeAction.gameObject.SetActive(false);
        // Ẩn hiệu ứng chờ mở bài
        for (int i = 0; i < players.Count; i++)
        {
            PlayerViewLucky89 plview = getPlayerView(players[i]);
            plview.ShowAnimWaitOpenCard(false);
        }

        // Bắt đầu xử lý kết quả
        StartCoroutine(handleData());
        handleUpdatePot(data);

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

            foreach (JToken pData in playerResults)
            {
                int pid = pData["pid"]?.Value<int>() ?? -1;
                bool isBanker = pData["isBanker"]?.Value<bool>() ?? false;
                long chipWin = pData["chipWin"]?.Value<long>() ?? 0;
                long ag = pData["ag"]?.Value<long>() ?? 0;
                int score = pData["score"]?.Value<int>() ?? 0;
                int rate = pData["rate"]?.Value<int>() ?? 1;
                JArray arrCard = (JArray)pData["arr"];

                Player player = players.Find(x => x.id == pid);
                if (player == null)
                {
                    Debug.LogWarning($"[Lucky89] Player not found for pid={pid}, skip display.");
                    continue;
                }

                PlayerViewLucky89 playerView = (PlayerViewLucky89)player.playerView;
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
                try
                {
                    playerView.ShowScore(true, score, listCodeCard.Count)
                              .ShowRate(rate)
                              .ShowAnimResult(true, chipWin);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Lucky89] Error ShowScore for pid={pid}: {ex.Message}");
                }
                if (!isBanker)
                {
                    if (chipWin > 0)
                        playerWinCbs.Add(() => StartCoroutine(playerWinChips(pid, chipWin, ag, bankerPlayer)));
                    else if (chipWin < 0)
                        playerLoseCbs.Add(() => StartCoroutine(playerLoseChips(pid, chipWin, ag, bankerPlayer)));
                    else
                    {
                        player.ag += playerView.GetBetValue();
                        player.updatePlayerView();
                    }
                }
                else
                {
                    finalDealerCb = () => playerView.effectFlyMoney(chipWin);
                }
            }

            if (bankerPlayer == null)
                Debug.LogWarning("[Lucky89] BankerPlayer not found in player list.");
            foreach (Action cb in playerLoseCbs) cb.Invoke();
            if (playerLoseCbs.Count > 0) yield return new WaitForSeconds(2 * LOSE_CHIP_DURATION + 1);
            foreach (Action cb in playerWinCbs) cb.Invoke();
            if (playerWinCbs.Count > 0) yield return new WaitForSeconds(3 * WIN_CHIP_DURATION);
            finalDealerCb?.Invoke();
            yield return new WaitForSeconds(1f);
            // 🔹 Bay từng lá bài về potTransform, rồi ẩn đi + trả về vị trí cũ
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

                    // Bay về pot
                    c.transform.DOMove(potTransform.position, 0.5f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            // Khi tới pot -> tắt lá bài
                            c.gameObject.SetActive(false);

                            // Sau đó trả về vị trí cũ (ẩn sẵn)
                            c.transform.position = originalPos;
                            c.transform.rotation = originalRot;
                        });

                    yield return new WaitForSeconds(0.05f); // bay lần lượt từng lá
                }
            }

            yield return new WaitForSeconds(0.6f);

            foreach (Player p in players)
            {
                ((PlayerViewLucky89)p.playerView)
                    .ShowHideBetChips(false)
                    .ShowAnimResult(false, 0)
                    .ShowScore(false, 0, 0)
                    .ShowRate(0)
                    .HideAllCards();
            }

            // 🔹 Kết thúc game
            _WaitForFinishCompleteCb?.Invoke();
            _WaitForFinishCompleteCb = null;
            checkAutoExit();
        }
    }


    IEnumerator playerLoseChips(int pId, long changedChips, long currentChips, Player bankerPlayer)
    {
        if (changedChips >= 0) yield break;

        // ✅ Đích đến: banker người chơi hoặc dealer máy
        // Transform targetTransform = bankerPlayer != null ? bankerPlayer.playerView.transform : m_DealerPVL89.transform;
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

            // 🔸 Bay chip từ người thua → banker
            chipTf.DOMove(targetTransform.position, 2 * LOSE_CHIP_DURATION)
                  .SetEase(Ease.OutQuad)
                  .OnComplete(() => chipTf.gameObject.SetActive(false));

            yield return new WaitForSeconds(.05f);
        }

        yield return new WaitForSeconds(LOSE_CHIP_DURATION);

        Player player = players.Find(x => x.id == pId);
        if (player.id == User.userMain.Userid) User.userMain.AG = currentChips;
        player.playerView.effectFlyMoney(changedChips);
        player.ag = currentChips;
        player.updatePlayerView();
    }

    IEnumerator playerWinChips(int pId, long changedChips, long currentChips, Player bankerPlayer)
    {
        if (changedChips <= 0) yield break;

        // ✅ Nguồn chip: banker người chơi hoặc dealer máy
        // Transform sourceTransform = bankerPlayer != null ? bankerPlayer.playerView.transform : m_DealerPVL89.transform;
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

            // 🔸 Bay chip từ banker → người thắng
            chipTf.DOMove((Vector2)(players.Find(x => x.id == pId).playerView.transform.position)
                          + new Vector2(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f)),
                          2 * WIN_CHIP_DURATION)
                  .SetEase(Ease.OutQuart)
                  .OnComplete(() => chipTf.gameObject.SetActive(false));

            yield return new WaitForSeconds(.05f);
        }

        yield return new WaitForSeconds(3 * WIN_CHIP_DURATION);

        Player player = players.Find(x => x.id == pId);
        if (player.id == User.userMain.Userid) User.userMain.AG = currentChips;
        player.playerView.effectFlyMoney(changedChips);
        playSound(SOUND_GAME.REWARD);
        player.ag = currentChips;
        player.updatePlayerView();
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
    // private string _ShowAnimOnBegin(bool isStart = true)
    // {
    //     string animName = isStart ? "start" : "continue";
    //     m_BeginGameSG.gameObject.SetActive(true);
    //     m_BeginGameSG.AnimationState.SetAnimation(0, animName, false);
    //     return animName;
    // }
    public void handleUpdatePot(JObject jsonData)
    {
        if (jsonData == null)
        {
            Debug.LogWarning("[handleUpdatePot] jsonData is null!");
            return;
        }

        JToken potToken = null;
        if (jsonData.ContainsKey("pot"))
        {
            potToken = jsonData["pot"];
        }
        else if (jsonData.ContainsKey("bankerInfoTransfer"))
        {
            potToken = jsonData["bankerInfoTransfer"]?["pot"];
            potValue = (long)jsonData["bankerInfoTransfer"]?["pot"];
        }

        if (potToken == null)
        {
            Debug.LogWarning("[handleUpdatePot] No 'pot' found in data!");
            return;
        }

        string curJackPotBinh = potToken.Value<long>().ToString();

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
    // private void _SetTickDraw(bool show, int draw)
    // {
    //     m_TickDontDraw.transform.parent.gameObject.SetActive(show);
    //     m_TickDraw.transform.parent.gameObject.SetActive(show);
    //     m_TickDraw.SetActive(draw > 0);
    //     m_TickDontDraw.SetActive(draw < 0);
    //     if (show && draw > 0) _DrawACard = true;
    //     else if (show && draw < 0) _DrawACard = false;
    //     else _DrawACard = null;
    // }
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
    private void _DistributeCardsToAPlayer(PlayerViewLucky89 playerView, List<int> codes, int rate, int score)
    {
        playSound(SOUND_GAME.DISPATCH_CARD);
        playerView.HideAllCards();
        List<Card> cardCs = playerView.GetListCards();
        for (int i = 0; i < cardCs.Count; i++) if (i < codes.Count) StartCoroutine(_DrawCard(playerView, codes[i]));
        int totalCode = 0;
        foreach (int code in codes) totalCode += code;
        playerView.UpdateCardsParentPositionAndRotation().ShowRate(totalCode > 0 ? rate : 0).ShowScore(totalCode > 0, score, cardCs.Count);
    }

    private class DataPlayer
    {
        public List<int> cardCodes = new();
        public Player PlayerP;
        public int rate, score;
    }
}
