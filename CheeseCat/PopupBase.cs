using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PopupBase : MonoBehaviour
{
    public GameObject xBtn;
    [HideInInspector]
    public string popupType;

    [HideInInspector]
    public float popupBaseScaleX = 1f;
    public float popupBaseScaleY = 1f;

    public bool commonBgFlag = false;
    public bool _closeLockFlag = false; // true면 닫기 불가
    public bool isShowChangeBattlePower = false;

    public bool _isButtonLock = false;  // true면 팝업내에 버튼 안눌림

    protected virtual void Awake()
    {
        if(xBtn == null)
        {
            xBtn = GameObject.Find("xBtn");
        }
        popupBaseScaleX = transform.localScale.x;
        popupBaseScaleY = transform.localScale.y;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case Const.SCENE_LOBBY:
                PopupMgr.Instance.DelayRecommendEqp();
                break;
        }
        isShowChangeBattlePower = false;
    }

    public virtual void Init(string type)
    {
        if (type != null)
        {
            popupType = type;
        }
    }

    public void ShowPopup(params object[] args)
    {
        // 공통
        gameObject.SetActive(true);
        GetComponent<RectTransform>().SetAsLastSibling();
    
        switch (SceneManager.GetActiveScene().name)
        {
            case Const.SCENE_INGAME:
                GameMgr.Unit.SetTempBattlePower();
                break;
        }

        ShowPopupCustom(args);
    }

    public virtual void SetParams(params object[] args)
    {

    }


    public virtual void ShowPopupCustom(params object[] args)
    {

    }

    public virtual void SetParamsDataCustom()
    {

    }

    // 팝업 오픈 연출
    public void ShowOpenAni()
    {
        gameObject.transform.localScale = new Vector3(0.7f,0.7f, 0);
        transform.DOScale(new Vector3(1,1,0), 0.2f).SetEase(Ease.OutBack);
    }

    public void ClickHidePopup()
    {
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_CANCEL);

        HidePopup();
    }

    public void HidePopup()
    {

        PopupMgr.Instance.HidePopup(popupType);
    }

    public bool ActHidePopup()
    {
        if (_closeLockFlag)
        {
            return false;
        }
        if (GameMgr.GlobalBtnLockFlag)
        {
            if (!GameMgr.GlobalBtnLockPassOnceFlag) // 글로벌버튼락 1번만 뚫기
            {
                return false;
            }
        }
        GameMgr.GlobalBtnLockPassOnceFlag = false;
        if (!HidePopupCustom())
        {
            return false;
        }

        gameObject.SetActive(false);
        return true;
    }

    public virtual bool HidePopupCustom()
    {
        return true;
    }

    public GameObject GetXBtn()
    {
        return xBtn;
    }
}
