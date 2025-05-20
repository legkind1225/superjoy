using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using VoxelBusters.EssentialKit;
using BestHTTP.SocketIO;
using UnityEngine.Playables;
using LitJson;
using Unity.VisualScripting;
using VoxelBusters.CoreLibrary;

#if UNITY_IOS && !UNITY_EDITOR
using Unity.Advertisement.IosSupport;
#endif

public class GameMgr : Singleton<GameMgr>
{
    ResourceMgr _resource = new ResourceMgr();
    public static ResourceMgr Resource { get { return Instance._resource; } }

    TableMgr _table = new TableMgr();
    public static TableMgr Table { get { return Instance._table; } }

    SummonMgr _summon = new();
    public static SummonMgr Summon { get { return Instance._summon; }}

    UnitMgr _unit = new UnitMgr();
    public static UnitMgr Unit { get { return Instance._unit; } }

    SkillMgr _skill = new SkillMgr();
    public static SkillMgr Skill { get { return Instance._skill; } }

    EqpMgr _eqp = new EqpMgr();
    public static EqpMgr Eqp { get { return Instance._eqp; } }

    LocaleMgr _locale = new LocaleMgr();
    public static LocaleMgr Locale { get { return Instance._locale; } }

    QuestMgr _quest = new QuestMgr();
    public static QuestMgr Quest { get { return Instance._quest; } }

    ShopMgr _shop = new ShopMgr();
    public static ShopMgr Shop { get {return Instance._shop; } }
    
    PassMgr _pass = new PassMgr();
    public static PassMgr Pass { get {return Instance._pass; }}

    PostMgr _post = new();
    public static PostMgr Post { get {return Instance._post; }}

    AdsMgr _ads = new AdsMgr();
    public static AdsMgr Ads { get { return Instance._ads; } }

    SettingMgr _setting = new SettingMgr();
    public static SettingMgr Setting { get { return Instance._setting; } }

    UserMgr _user = new UserMgr();
    public static UserMgr User { get { return Instance._user; } }

    LogMgr _log = new LogMgr();
    public static LogMgr Log { get { return Instance._log; } }

    DungeonMgr _dungeon = new DungeonMgr();
    public static DungeonMgr Dungeon { get { return Instance._dungeon; } }

    AlramMgr _alram = new AlramMgr();
    public static AlramMgr Alram { get { return Instance._alram; } }

    PastureMgr _pasture = new PastureMgr();
    public static PastureMgr Pasture { get { return Instance._pasture; } }


    // 스프라이트 아틀라스 연결
    public SpriteAtlas uiAtlas;
    public SpriteAtlas eqpAtlas;
    public SpriteAtlas cashShopAtlas;
    public SpriteAtlas albumAtlas;
    public SpriteAtlas pastureAtlas;
    public SpriteAtlas rouletteAtlas;

    public TMP_FontAsset baseFont;
    public TMP_FontAsset outlineFont;

    public string[] arguments;

    bool isPaused = false;  // 앱 비활성화 상태인지

    // 글로벌 버튼 락
    public static bool GlobalBtnLockFlag = false;
    public static bool GlobalBtnLockPassOnceFlag = false; // 글로벌락 한번만 뚫기.

    float _inputDelay = 0;
    public bool LoadedDataTableFlag = false;

    // 다음 이동 씬 임시 저장
    public string NextSceneName;
    
    // 로비 이동시 오픈될 팝업 
    public string LobbyOpenPopup = "";

    // 아이템 풀
    public Transform itemSlotParent;
    public Transform giftSlotParent;
    public Transform heroSlotParent;
    public Transform airshipSlotParent;

    // 로컬푸시
    static NotificationSettings noti_setting;

    // 핵클
    Hackle hackle;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        SetResolution();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;  // 화면 안꺼짐
        Application.targetFrameRate = 60;   // 프레임 고정

        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.Portrait;  // 화면 세로 고정

        Const.InitConst();
        User.Init();
        Setting.Init();
    }

    // 테이블 로딩 완료 이후 1회만 호출
    public void InitManager()
    {
        // 유닛 관련 정보 초기화
        Eqp.Init();
        Resource.Init();
        Quest.Init();
        Post.Init();
        Shop.Init();
        Ads.Init();
        Dungeon.Init();
        Alram.Init();

        Pasture.Init();
        hackle = Hackle.GetInstance();
    }

    // iOS ATT 권한 팝업 호출
    public void CallIosATTPopup()
    {
#if UNITY_IOS && !UNITY_EDITOR
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus()
            == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif
    }
    
    // 데이터 로딩하기전 초기화 할 것들
    public void InitData()
    {
        // Quest.Init();
    }

    // Update is called once per frame
    void Update()
    {
        User.Update(); // IOS의 경우 로그인 상태 업데이트

        // 광고 보상 여부
        if (Ads.isSuccessAD)
        {
            Ads.isSuccessAD = false;
            Ads.SuccessAds(isFree: false);
            // SuccessAds();
        }

        if (_inputDelay > 0) {
            _inputDelay -= Time.deltaTime;
            if (_inputDelay < 0) {
                _inputDelay = 0;
            }
        } else {
            if (GlobalBtnLockFlag) {
                return;
            }

            
            // 키 입력 관리
            if (Input.GetKey(KeyCode.Escape))   // back
            {
                _inputDelay = 0.3f;

                // 통신로딩중일때는 back버튼 막음.
                if (PopupMgr.Instance.LoadingMask != null)
                {
                    if (PopupMgr.Instance.LoadingMask.activeSelf)
                    {
                        return;
                    }
                }

                // 튜토리얼 진행중 back버튼 막음
                if(TutoMgr.Instance.IsTutorial())
                {
                    return;
                }

                // 서버시간 동기화중일 때는 back버튼 막음
                if (PopupMgr.Instance.IsSyncSvrTime())
                {
                    return;
                }
                
                
                if (!PopupMgr.Instance.HideStackPopup()) {
                    switch (SceneManager.GetActiveScene().name)
                    {
                        case Const.SCENE_INTRO:
                            break;
                        case Const.SCENE_INGAME:
                            if(PopupMgr.Instance.GetShowPopupCount() <= 0)
                            {
                                PopupMgr.Instance.ShowPopup(PopupMgr.PopupExit);
                            }
                            break;
                    }
                }
                
            }
        }


    }

    public float ScaleMul = 1;

    public const int SetWidth = 1080;  // 사용자 설정 너비
    public const int SetHeight = 2440;  // 사용자 설정 높이

    public const int UISetHeight = 1920;

    /* 해상도 설정하는 함수 */
    public void SetResolution()
    {
        // int setWidth = 1080; // 사용자 설정 너비
        // int setHeight = 2440; // 사용자 설정 높이

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장
       
        // 종횡비를 계산했을 때 너무 길면 짧게 조정해줌.
        if (Screen.height / (float)Screen.width > 2.25f) { // 세로가 너무 김.
            // 가로를 줄이기
            Screen.SetResolution(SetWidth, (int)(((float)deviceHeight / deviceWidth) * SetWidth), false); // SetResolution 함수 제대로 사용하기

            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)SetWidth / SetHeight); // 새로운 높이
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
        }
        else if(Screen.height / (float)Screen.width < 1.778f)
        {
            float newWidth = ((float)SetWidth / UISetHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
        }

        if ((float)SetWidth / UISetHeight >= (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
        {
            if(deviceHeight >= SetHeight)
            {
                deviceHeight = SetHeight;
            }
            ScaleMul = ((float)deviceHeight / deviceWidth) / ((float)UISetHeight / SetWidth);
        }
    }

    // 앱 활성화/비활성화시 처리
    void OnApplicationPause(bool pause)
    {
        //Debug.Log($"OnApplicationPause");
        if (pause)
        {
            isPaused = true;
            /* 앱이 비활성화 되었을 때 처리 */
            TimeMgr.Instance.SaveNowTime();
        }
        else
        {
            if (isPaused)
            {
                isPaused = false;
                PopupMgr.Instance.ShowSyncSvrTime();
                /* 앱이 활성화 되었을 때 처리 */
                //TimeMgr.Instance.CheckOfflineTime();

                // 서버시간 동기화

            }
        }
    }

    // 앱이 종료 될 때 처리
    void OnApplicationQuit()
    {
        TimeMgr.Instance.SaveNowTime();
    }

    public static int GetVersionNumber()
    {
        string str = Application.version;
        string[] strs = str.Split('.');
        
        return int.Parse(strs[0])*1000000 + int.Parse(strs[1]) *1000 + int.Parse(strs[2]);
    }

    public Intro GetIntro()
    {
        return FindObjectOfType<Intro>();
    }

    public string GetMarketURL()
    {
        switch (Const.PLATFORM)
        {
            case Const.PLATFORM_ONE:
                return Const.ONE_MARKET_URL;
            case Const.PLATFORM_IOS:
                return Const.IOS_MARKET_URL;
            case Const.PLATFORM_GAL:
                return Const.GAL_MARKET_URL;
            default:
                return Const.PLAY_MARKET_URL;
        }
    }

    public void GotoMarket()
    {
        switch (Const.PLATFORM)
        {
            case Const.PLATFORM_ONE:
                Application.OpenURL(Const.ONE_MARKET_URL);
                break;
            case Const.PLATFORM_IOS:
                Application.OpenURL(Const.IOS_MARKET_URL);
                break;
            case Const.PLATFORM_GAL:
                Application.OpenURL(Const.GAL_MARKET_URL);
                break;
            default:
                Application.OpenURL(Const.PLAY_MARKET_URL);
                break;
        }
        
    }

    public void GotoNotice()
    {
        // TODO 언어에 따라서 다르게 보내기
        Application.OpenURL(Const.NOTICE_LINK);
    }

        

    // 친구초대 링크 띄우기
    public void GotoInvite()
    {
        string url = Const.INVITE_LINK;
        SettingMgr.EventSettingData eventData = Setting.GetActiveEventData(SettingMgr.EventType.EVENT_INVITE);
        if(eventData != null)
        {
            if(eventData.eventSubType != "")
            {
                url = eventData.eventSubType;
            }
        }

        ShareSheet shareSheet = ShareSheet.CreateInstance();
        shareSheet.AddText(LocaleMgr.GetString(4628, "{1}", User.GetUid()));
        shareSheet.AddURL(URLString.URLWithPath(url));
        shareSheet.SetCompletionCallback((result, error) => {
            Debug.Log("Share Sheet was closed. Result code: " + result.ResultCode);
        });
     
        shareSheet.Show();
    }
}
