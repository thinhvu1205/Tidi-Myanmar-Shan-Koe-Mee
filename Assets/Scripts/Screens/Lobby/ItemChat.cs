using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Globals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemChat : MonoBehaviour
{
    private const float PADDING = 12f;
    private const float EXTRA_PADDING = 10f;
    private const float MIN_WIDTH = 100f;
    private const float DEFAULT_MAX_WIDTH = 400f;
    private const float LOBBY_MAX_WIDTH = 800f;

    [SerializeField] private TextMeshProUGUI m_NamePeopleR;
    [SerializeField] private TextMeshProUGUI m_MessageR;
    [SerializeField] private TextMeshProUGUI m_TimeR;
    [SerializeField] private GameObject m_FrameContentR;
    [SerializeField] private GameObject m_AvatarR;
    [SerializeField] private Button m_LeftAudioBtn;
    [SerializeField] private Button m_RightAudioBtn;

    [SerializeField] private GameObject m_ChatRight;
    [SerializeField] private GameObject m_ChatLeft;
    [SerializeField] private TextMeshProUGUI m_NamePeopleL;
    [SerializeField] private TextMeshProUGUI m_MessageL;
    [SerializeField] private TextMeshProUGUI m_TimeL;
    [SerializeField] private GameObject m_FrameContentL;
    [SerializeField] private GameObject m_AvatarL;
    [SerializeField] private GameObject m_GroupLineL, m_GroupLineR;
    [SerializeField] private GameObject m_ButtonAudioL, m_ButtonAudioR;
    private ChatWorldLobbyData _DataCWLD;
    private AudioSource _AudioAS;
    public bool isLobby = true;

    // playback state
    private bool _isPlaying = false;
    private float _audioDuration = 0f;
    private float _startPlayTime = 0f;
    private Button _activeAudioBtn;
    private Color _defaultAudioBtnColor = Color.white;
    private Transform _activeLineGroup; // nhóm vạch đang phát (16 child)
    public int indexItem = -1;
    private static ItemChat _currentlyPlayingItem;
    private float height = 0, width = 0;


    private void AdjustFrameContent(GameObject frameContent, TextMeshProUGUI messageText, string message)
    {
        if (messageText == null || frameContent == null) return;

        messageText.text = message;
        messageText.enableWordWrapping = true;
        messageText.ForceMeshUpdate();

        float textWidth = messageText.preferredWidth;

        float maxWidth = isLobby ? LOBBY_MAX_WIDTH : DEFAULT_MAX_WIDTH;
        float finalWidth = Mathf.Clamp(textWidth + PADDING, MIN_WIDTH, maxWidth);

        messageText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth);
        messageText.ForceMeshUpdate();

        float finalHeight = messageText.preferredHeight + PADDING;

        RectTransform frameRect = frameContent.GetComponent<RectTransform>();
        if (frameRect != null)
        {
            frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth + PADDING * 6);
            frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight + PADDING);
            height = finalHeight + PADDING;
            width = finalWidth + PADDING * 6;
        }
    }

    // Toggle play/pause: nếu đang phát -> dừng, nếu dừng -> phát
    public void DoClickAudio()
    {
        // Dừng thằng đang phát nếu khác mình
        if (_currentlyPlayingItem != null && _currentlyPlayingItem != this)
        {
            _currentlyPlayingItem.StopPlaybackAndReset();
        }

        // Gán lại
        _currentlyPlayingItem = this;

        if (_AudioAS == null || _DataCWLD == null || string.IsNullOrEmpty(_DataCWLD.Content))
            return;

        // Nếu đang phát -> dừng và reset
        if (_isPlaying && _AudioAS.isPlaying)
        {
            StopPlaybackAndReset();
            return;
        }

        // Nếu đang ở trạng thái _isPlaying=true nhưng AudioSource không chơi (edge case) -> reset trước
        if (_isPlaying && !_AudioAS.isPlaying)
        {

            StopPlaybackAndReset();
        }
        Globals.COMMON_DATA.IndexItemChatChoose = indexItem;

        _activeLineGroup = m_ChatRight != null && m_ChatRight.activeSelf ? m_GroupLineR?.transform : m_GroupLineL?.transform;
        _activeAudioBtn = (m_ChatRight != null && m_ChatRight.activeSelf)
            ? m_ButtonAudioR?.GetComponent<Button>()
            : m_ButtonAudioL?.GetComponent<Button>();

        if (_activeAudioBtn != null)
            _defaultAudioBtnColor = _activeAudioBtn.image != null ? _activeAudioBtn.image.color : Color.white;

        // Giải nén và tạo AudioClip
        try
        {
            byte[] reversedBytes;
            using (MemoryStream input = new(Convert.FromBase64String(_DataCWLD.Content)))
            {
                using (DeflateStream deflate = new(input, CompressionMode.Decompress))
                {
                    using (MemoryStream output = new())
                    {
                        deflate.CopyTo(output);
                        reversedBytes = output.ToArray();
                    }
                }
            }

            int totalSamples = reversedBytes.Length / 4;
            if (totalSamples <= 0) return;
            float[] samples = new float[totalSamples];
            Debug.Log("xem lúc mới trả về" + samples.Length / 48000);
            Buffer.BlockCopy(reversedBytes, 0, samples, 0, reversedBytes.Length);
            // if (samples.Max() <= 0.001f)
            // {
            //     samples = new float[0];
            // }
            Transform lineGroup = _activeLineGroup;
            if (lineGroup != null)
            {
                int barCount = lineGroup.childCount;
                int segmentLength = samples.Length / barCount;

                for (int i = 0; i < barCount; i++)
                {
                    float max = 0f;

                    for (int j = 0; j < segmentLength; j++)
                    {
                        int idx = i * segmentLength + j;
                        if (idx >= samples.Length) break;
                        max = Mathf.Max(max, Mathf.Abs(samples[idx]));
                    }

                    float amplified = max * 20f; // thử hệ số khuếch đại 10 lần
                    float normalized = Mathf.Clamp01(amplified);

                    Transform bar = lineGroup.GetChild(i);

                    if (bar != null)
                    {
                        if (samples.Length != 0)
                        {
                            bar.localScale = new Vector3(1f, Mathf.Lerp(0.15f, 1f, normalized), 1f);
                        }
                        else
                        {
                            bar.localScale = new Vector3(1f, 0.1f, 0f);
                        }
                        // tránh quá nhỏ
                        Image img = bar.GetComponent<Image>();
                        if (img != null) img.color = Color.gray; // reset màu ban đầu
                    }
                }

                lineGroup.gameObject.SetActive(true);
            }
            AudioClip aAC = AudioClip.Create("AudioClip", samples.Length, 1, 16000, false);
            Debug.Log("xem lúc mới trả về sau này" + AudioSettings.outputSampleRate);
            aAC.SetData(samples, 0);

            // stop trước nếu AudioSource đang play clip khác
            if (_AudioAS.isPlaying) _AudioAS.Stop();

            _AudioAS.clip = aAC;
            _AudioAS.Play();

            _isPlaying = true;
            _startPlayTime = Time.time;
            _audioDuration = aAC.length > 0 ? aAC.length : 0.001f; // tránh chia 0
            Debug.Log("xem là bao nhiêu" + aAC.length + "   " + _audioDuration);

            Debug.Log($"Samples: {samples.Length}, SampleRate: 48000, Duration: {(float)samples.Length / 48000f}");

            // change button color to active
            if (_activeAudioBtn != null && _activeAudioBtn.image != null)
                _activeAudioBtn.image.color = Color.green;

            // đảm bảo nhóm vạch hiện được bật
            if (_activeLineGroup != null)
            {
                _activeLineGroup.gameObject.SetActive(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("DoClickAudio error: " + ex.Message);
            // an toàn: reset trạng thái
            StopPlaybackAndReset();
        }
    }

    // Gọi từ bên ngoài để setup item
    public void SetInfo(ChatWorldLobbyData dataCWLD, AudioSource dataAS = null, int index = -1, Action<float, float> onSizeCalculated = null)
    {
        indexItem = index;
        //Debug.Log(index + "check xem index item" + (Globals.COMMON_DATA.IndexItemChatChoose != indexItem));

        _isPlaying = Globals.COMMON_DATA.IndexItemChatChoose == indexItem;
        // if (_isPlaying)
        // {
        //     return;
        // }


        if (m_GroupLineL != null)
        {
            for (int i = 0; i < m_GroupLineL.transform.childCount; i++)
            {
                Transform bar = m_GroupLineL.transform.GetChild(i);
                bar.localScale = new Vector3(1f, 0.1f, 0f);
                bar.gameObject.GetComponent<Image>().color = _defaultAudioBtnColor;

            }
        }
        if (m_GroupLineR != null)
        {
            for (int i = 0; i < m_GroupLineR.transform.childCount; i++)
            {
                Transform bar = m_GroupLineR.transform.GetChild(i);
                bar.localScale = new Vector3(1f, 0.1f, 0f);
                bar.gameObject.GetComponent<Image>().color = _defaultAudioBtnColor;
            }
        }

        m_FrameContentL?.GetComponent<Image>().SetNativeSize();
        m_FrameContentR?.GetComponent<Image>().SetNativeSize();
        if (m_ButtonAudioR != null)
        {
            Image imgR = m_ButtonAudioR.GetComponent<Image>();
            if (imgR != null) imgR.color = _defaultAudioBtnColor;
        }

        if (m_ButtonAudioL != null)
        {
            Image imgL = m_ButtonAudioL.GetComponent<Image>();
            if (imgL != null) imgL.color = _defaultAudioBtnColor;
        }

        //        Debug.Log("xem có audio ko" + dataCWLD.IsAudio);

        _DataCWLD = dataCWLD;
        _AudioAS = dataAS;
        // Debug.Log("xem là đoạn audio đó dài bao nhiêu" + dataAS.clip.length);
        bool isMe = dataCWLD.Name.Equals(User.userMain.displayName);
        int avatar = dataCWLD.Avatar;

        m_ChatRight.SetActive(isMe);
        m_ChatLeft.SetActive(!isMe);
        if (!isLobby)
        {
            _activeAudioBtn = isMe ? m_ButtonAudioR?.GetComponent<Button>() : m_ButtonAudioL?.GetComponent<Button>();
            if (_activeAudioBtn != null && _activeAudioBtn.image != null)
                _defaultAudioBtnColor = _activeAudioBtn.image.color;
        }
        if (!isMe)
        {

            if (avatar > -1)
            {
                m_GroupLineL?.SetActive(dataCWLD.IsAudio);
                m_AvatarL.SetActive(true);
                m_AvatarR.SetActive(false);
                Avatar av = m_AvatarL.GetComponent<Avatar>();
                if (av != null)
                {
                    av.setSpriteWithID(avatar);
                    av.idAvt = avatar;
                    av.setVip(dataCWLD.Vip);
                }
                if (m_LeftAudioBtn != null) m_LeftAudioBtn.gameObject.SetActive(dataCWLD.IsAudio);
            }

            m_MessageL.gameObject.SetActive(!dataCWLD.IsAudio);
            m_NamePeopleL.text = dataCWLD.Name;
            if (!dataCWLD.IsAudio)
            {
                m_MessageL.text = dataCWLD.Content;
                AdjustFrameContent(m_FrameContentL, m_MessageL, dataCWLD.Content);
                onSizeCalculated?.Invoke(width, height);
                if (isLobby && dataCWLD.Vip >= Config.text_chat_gold_by_vip)
                {
                    m_MessageL.color = Color.yellow;
                }
                else
                {
                    m_MessageL.color = Color.white;
                }
            }
            m_TimeL.text = dataCWLD.Time;
        }
        else
        {
            if (avatar > -1)
            {
                m_GroupLineR?.SetActive(dataCWLD.IsAudio);
                m_AvatarL.SetActive(false);
                m_AvatarR.SetActive(true);
                Avatar av = m_AvatarR.GetComponent<Avatar>();
                if (av != null)
                {
                    av.setSpriteWithID(avatar);
                    av.idAvt = avatar;
                    av.setVip(Globals.User.userMain.VIP);
                }
                if (m_RightAudioBtn != null) m_RightAudioBtn.gameObject.SetActive(dataCWLD.IsAudio);
            }

            m_MessageR.gameObject.SetActive(!dataCWLD.IsAudio);

            m_NamePeopleR.text = dataCWLD.Name;
            if (!dataCWLD.IsAudio)
            {
                m_MessageR.text = dataCWLD.Content;
                AdjustFrameContent(m_FrameContentR, m_MessageR, dataCWLD.Content);
                onSizeCalculated?.Invoke(width, height);
                if (isLobby && dataCWLD.Vip >= Config.text_chat_gold_by_vip)
                {
                    m_MessageR.color = Color.yellow;
                }
                else
                {
                    m_MessageR.color = Color.white;
                }
            }
            m_TimeR.text = dataCWLD.Time;
        }
    }

    private void Update()
    {
        if (_currentlyPlayingItem != this)
            return;
        if (_isPlaying && _AudioAS != null && _AudioAS.isPlaying)
        {


            float elapsed = Time.time - _startPlayTime;
            Transform lineGroup = _activeLineGroup ??
                (m_ChatRight.activeSelf ? m_GroupLineR?.transform : m_GroupLineL?.transform);
            if (lineGroup == null) return;

            int totalLines = Mathf.Max(1, lineGroup.childCount);
            float progress = Mathf.Clamp01(elapsed / _audioDuration);
            int activeLines = Mathf.FloorToInt(progress * totalLines);

            for (int i = 0; i < totalLines; i++)
            {
                Image img = lineGroup.GetChild(i).GetComponent<Image>();
                if (img == null) continue;

                img.color = (i <= activeLines) ? Color.green : Color.gray;
            }
        }
        else if (_isPlaying && (_AudioAS == null || !_AudioAS.isPlaying))
        {
            StopPlaybackAndReset();
        }
    }


    // Dừng phát và reset giao diện
    private void StopPlaybackAndReset()
    {
        if (_currentlyPlayingItem == this)
            _currentlyPlayingItem = null;

        try
        {
            if (_AudioAS != null && _AudioAS.isPlaying)
                _AudioAS.Stop();
        }
        catch { }
        Globals.COMMON_DATA.IndexItemChatChoose = -1;
        _isPlaying = false;
        _startPlayTime = 0f;
        _audioDuration = 0f;

        // reset nút
        if (_activeAudioBtn != null && _activeAudioBtn.image != null)
            _activeAudioBtn.image.color = _defaultAudioBtnColor;

        // reset vạch về màu xám
        Transform lineGroup = _activeLineGroup ?? (m_ChatRight.activeSelf ? m_GroupLineR?.transform : m_GroupLineL?.transform);
        if (lineGroup != null)
        {
            for (int i = 0; i < lineGroup.childCount; i++)
            {
                Image img = lineGroup.GetChild(i).GetComponent<Image>();
                if (img != null) img.color = Color.gray;
            }
        }

        _activeLineGroup = null;
    }
}
