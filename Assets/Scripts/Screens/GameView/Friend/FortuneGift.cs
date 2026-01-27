using UnityEngine;

public class FortuneGift : MonoBehaviour
{
    public void Rule()
    {
        UIManager.instance.showWebView(Globals.Config.RuleFortuneGift);
    }
    public void close()
    {
        Destroy(gameObject);
    }
    public void SendGift()
    {
        SocketSend.sendGiftFortune();

        Destroy(gameObject);
    }
}