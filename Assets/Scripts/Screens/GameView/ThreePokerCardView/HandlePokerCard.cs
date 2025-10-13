using Newtonsoft.Json.Linq;

public class HandlePokerCard
{
    public static void processData(JObject jData) // class nay dung de viet them cac evt rieng cua game binh a nhe. Con may cai chung nhu stable,ctable o ben handleGame co r/
    {
        var gameView = (ThreePokerCardGameView)UIManager.instance.gameView;
        if (gameView == null) return;
        string evt = (string)jData["evt"];
        switch (evt)
        {
            case "finish":
                gameView.handleFinish(jData);
                break;
            case "chattable":
                gameView.handleChatTable(jData);
                break;
            case "lc":
                gameView.handleLc(jData);
                break;
            case "raise":
                gameView.handlePlayerRaise(jData);
                break;
            case "fold":
                gameView.handlePlayerFold(jData);
                break;
            case "bet":
                gameView.handleBet(jData);
                break;
            case "startBet":
                gameView.handleStartBet(jData);
                break;
            case "autoExit":
                gameView.handleAutoExit(jData);
                break;
            case "showCard":
                gameView.handleShowCard(jData);
                break;
        }
    }
}