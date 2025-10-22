using Newtonsoft.Json.Linq;
using UnityEngine;

public class HandleXocdiaView
{
    public static void processData(JObject jData)
    {
        var gameView = (XocdiaView)UIManager.instance.gameView;
        if (gameView == null) return;
        string evt = (string)jData["evt"];
        Debug.Log("xem data ấy" + jData.ToString());
        switch (evt)
        {
            case "startgame":
                gameView.HandleStartGame();
                break;
            case "startsell":
                gameView.HandleStartSell(jData);
                break;
            case "buybet":
                gameView.handleBuyBet(jData);
                break;
            case "sellbet":
                  gameView.handleSellBet(jData);
                break;
            case "dealer":
                gameView.handleDealer(jData);
                break;
            case "findDealer":
                gameView.handleFindDealer(jData);
                break;
            case "returnAg":
                 gameView.handleReturnAg(jData);
                break;
            case "lastGameResult":
                gameView.handleLastHistory(jData);
                break;
            case "bet":
                gameView.HandleBet(jData);
                break;
            case "finish":
                gameView.handleFinish(jData);
                break;
        }
    }
}