using Globals;
using UnityEngine;

public class PopupSupport : BaseView
{
    [SerializeField] private GameObject m_Messenger, m_Telegram;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_Messenger.SetActive(!Config.fanpageID.Equals("") && Config.is_bl_fb);
        m_Telegram.SetActive(!Config.chat_tele_support_link.Equals(""));
    }
    public void OnclickTele()
    {
        Application.OpenURL(Config.chat_tele_support_link);
    }

    public void OnclickMess()
    {
        Application.OpenURL(Config.u_chat_fb);
    }
    public void OnclickClose()
    {
        SoundManager.instance.soundClick();
        hide();
    }

}
