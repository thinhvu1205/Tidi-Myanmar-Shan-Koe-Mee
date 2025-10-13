using System;
using Spine.Unity;
using UnityEngine;

public class MysteriousSlot : MonoBehaviour
{
    [SerializeField] private GameObject m_BgUnselectable, m_BgSelectable, m_UnselectedGold, m_SelectedGold, m_UnselectedBomb, m_SelectedBomb;
    [SerializeField] private SkeletonGraphic m_AnimBombSG, m_AnimExplodeSG, m_AnimOpenGoldSG;
    private Action _onClickCb;
    private int _id;

    #region Button
    public void DoClickOpen()
    {
        _onClickCb?.Invoke();
    }
    #endregion

    public MysteriousSlot TurnUnchosable(bool show)
    {
        m_BgUnselectable.SetActive(show);
        m_AnimExplodeSG.gameObject.SetActive(false);
        m_AnimOpenGoldSG.gameObject.SetActive(false);
        return this;
    }
    public MysteriousSlot TurnChosable(bool show) { m_BgSelectable.SetActive(show); return this; }
    public MysteriousSlot TurnUnchosenGold(bool show) { m_UnselectedGold.SetActive(show); return this; }
    public MysteriousSlot TurnChosenGold(bool show)
    {
        m_SelectedGold.SetActive(show);
        if (show) _ShowAnimOnOpenSlot(m_AnimOpenGoldSG);
        return this;
    }
    public MysteriousSlot TurnUnchosenBomb(bool show) { m_UnselectedBomb.SetActive(show); return this; }
    public MysteriousSlot TurnChosenBomb(bool show)
    {
        m_SelectedBomb.SetActive(show);
        if (show) _ShowAnimOnOpenSlot(m_AnimExplodeSG);
        return this;
    }
    public MysteriousSlot SetOnclickCB(Action cb) { _onClickCb = cb; return this; }
    public bool IsBomb() { return m_UnselectedBomb.activeSelf; }
    public int GetId() { return _id; }
    public MysteriousSlot SetId(int id) { _id = id; return this; }
    private void _ShowAnimOnOpenSlot(SkeletonGraphic animSG)
    {
        animSG.gameObject.SetActive(true);
        animSG.AnimationState.SetAnimation(0, animSG.startingAnimation, false);
    }
    private void Start()
    {
        m_AnimOpenGoldSG.Initialize(true);
        m_AnimExplodeSG.Initialize(true);
        m_AnimOpenGoldSG.AnimationState.Complete += x => m_AnimOpenGoldSG.gameObject.SetActive(false);
        m_AnimExplodeSG.AnimationState.Complete += x => m_AnimExplodeSG.gameObject.SetActive(false);
    }
}
