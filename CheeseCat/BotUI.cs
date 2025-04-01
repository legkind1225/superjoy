using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class BotUI : Singleton<BotUI>
{
    public GameObject playerNew;
    public GameObject friendNew;
    public GameObject duneonNew;
    public GameObject roomNew;
    public GameObject shopNew;
    public GameObject supplyLvupNew;

    public GameObject roomLike;

    public GameObject playerOff;
    public GameObject friendOff;
    public GameObject duneonOff;
    public GameObject roomOff;
    public GameObject shopOff;

    public GameObject playerLimit;
    public GameObject friendLimit;
    public GameObject duneonLimit;
    public GameObject roomLimit;
    public GameObject shopLimit;

    public GameObject supplyAutoLimit;

    public GameObject supplyObj;
    public GameObject supplySummonObj;
    public GameObject supplySummonLvObj;
    public GameObject supplyAutoSummonObj;
    public TextMeshProUGUI supplySummonLvText;

    // 수집품 소환 연출
    public GameObject supplyBtnEff;
    public SkeletonGraphic footSkeleton;
    public SkeletonGraphic cloudSkeleton;
    public Image supplyIcon;
    public GameObject supplySellEff;
    public GameObject supplyAutoEff;
    public GameObject supplyAutoOn;
    public GameObject supplyAutoOff;

    protected override void Awake()
    {
        base.Awake();
        HideSupplySummonEff();
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateNewAll();
        UpdateLimitAll();
        UpdateSupplySummonLv();
        UpdateSupplyBtnEff();

        Invoke("CheckSupplySummonTouch", 10);
    }

    protected void OnEnable()
    {
        GameMgr.Unit.onUpdateFriendData += UpdateFriendNew;
        GameMgr.Eqp.onUpdateEqpData += UpdatePlayerNew;
        DataMgr.Instance.onUpdateMaxStage += UpdateLimitAll;
        DataMgr.Instance.onUpdateMaxStage += UpdateNewAll;
        GameMgr.Summon.onUpdateSummonData += UpdateNewAll;
        GameMgr.Shop.onUpdateShopData += UpdateNewAll;
        GameMgr.Ads.onUpdateAds += UpdateShopNew;
        GameMgr.Summon.onUpdateSummonRewardData += UpdateShopNew;
        GameMgr.Summon.onUpdateSupplySummonLvData += UpdateSupplySummonLv;
        GameMgr.Eqp.onUpdateSupplyData += UpdateSupplyBtnEff;
        GameMgr.Eqp.onUpdateSupplyData += CheckRemainSupply;
        PopupMgr.Instance.onChangePopupCount += UpdateSupplyBtnEff;
        DataMgr.Instance.onUpdatePayment += UpdateSupplyLvupNew;
        DataMgr.Instance.onUpdatePayment += UpdateFriendNew;
        DataMgr.Instance.onUpdatePayment += UpdatePlayerNew;
        DataMgr.Instance.onUpdateLvUp += UpdateSupplyAutoLimit;

        TimeMgr.Instance.onUpdateTimer += UpdateTimer;
    }

    protected void OnDisable()
    {
        GameMgr.Unit.onUpdateFriendData -= UpdateFriendNew;
        GameMgr.Eqp.onUpdateEqpData -= UpdatePlayerNew;
        DataMgr.Instance.onUpdateMaxStage -= UpdateLimitAll;
        DataMgr.Instance.onUpdateMaxStage -= UpdateNewAll;
        GameMgr.Summon.onUpdateSummonData -= UpdateNewAll;
        GameMgr.Shop.onUpdateShopData -= UpdateNewAll;
        GameMgr.Ads.onUpdateAds -= UpdateShopNew;
        GameMgr.Summon.onUpdateSummonRewardData -= UpdateShopNew;
        GameMgr.Summon.onUpdateSupplySummonLvData -= UpdateSupplySummonLv;
        GameMgr.Eqp.onUpdateSupplyData -= UpdateSupplyBtnEff;
        GameMgr.Eqp.onUpdateSupplyData -= CheckRemainSupply;
        PopupMgr.Instance.onChangePopupCount -= UpdateSupplyBtnEff;
        DataMgr.Instance.onUpdatePayment -= UpdateSupplyLvupNew;
        DataMgr.Instance.onUpdatePayment -= UpdateFriendNew;
        DataMgr.Instance.onUpdatePayment -= UpdatePlayerNew;
        DataMgr.Instance.onUpdateLvUp -= UpdateSupplyAutoLimit;

        TimeMgr.Instance.onUpdateTimer -= UpdateTimer;
    }

    public void UpdateTimer()
    {
        UpdateRoomLike();
        UpdateRoomNew();
    }

    public void UpdateNewAll()
    {
        UpdatePlayerNew();
        UpdateFriendNew();
        UpdateShopNew();
        UpdateSupplyLvupNew(Const.PaymentType.GOLD);
        UpdateRoomNew();
    }

    public void UpdatePlayerNew()
    {
        if(playerNew)
        {
            bool isEqpUpgrade = GameMgr.Eqp.IsUpgradeEqpAll(Const.EQP_WEAPON) || GameMgr.Eqp.IsUpgradeEqpAll(Const.EQP_ARMOR);
            bool isEqpAwake = GameMgr.Eqp.IsAwakeEqpAll(Const.EQP_WEAPON) || GameMgr.Eqp.IsAwakeEqpAll(Const.EQP_ARMOR);
            bool isEqp = GameMgr.Eqp.IsUpperEqp(GameMgr.Eqp.GetNowEqpData(Const.EQP_WEAPON)) || GameMgr.Eqp.IsUpperEqp(GameMgr.Eqp.GetNowEqpData(Const.EQP_ARMOR));
            bool isSkillTreePoint = !DataMgr.Instance.CheckLimitContentStage(Const.ContentLimit.MENU_PLAYER_SKILL) && DataMgr.Instance.IsUpgradeNextSkillTreeAll();  // DataMgr.Instance.GetRemainSKillTreePoint() > 0;
            bool isNewAcc = GameMgr.Eqp.IsNewAccForList(Const.EQP_HAT) || GameMgr.Eqp.IsNewAccForList(Const.EQP_TOP);


            playerNew.SetActive(isEqpUpgrade || isEqpAwake || isEqp || isSkillTreePoint || isNewAcc);
        }
    }
    public void UpdatePlayerNew(Const.PaymentType paymentType)
    {
        if(paymentType == Const.PaymentType.EQP_AWAKE_STONE)
        {
            UpdatePlayerNew();
        }
    }

    public void UpdateFriendNew()
    {
        if(friendNew)
        {
            bool isUpgrade = GameMgr.Unit.IsUpgradeFriendAll();
            bool isEvol = GameMgr.Unit.IsEvolFriendAll();

            friendNew.SetActive(isUpgrade || isEvol);
        }
    }
    public void UpdateFriendNew(Const.PaymentType paymentType)
    {
        if(paymentType == Const.PaymentType.CAT_EVOL_STONE)
        {
            UpdateFriendNew();
        }
    }

    public void UpdateRoomNew()
    {
        bool isComplete = false;
        SerializableDictionary<string, DecoData> decoDic = DataMgr.Instance.GetDecoDic();
        foreach (var item in decoDic)
        {
            if(item.Value.act == Const.BUILDING_ACT_UPGRADE)
            {
                if(item.Value.compT - TimeMgr.Instance.GetSvrNowT() <= 0)
                {
                    isComplete = true;
                    break;
                }
            }
        }
        roomNew.SetActive(isComplete);
    }

    public void UpdateShopNew()
    {
        if(shopNew)
        {
            if(!DataMgr.Instance.CheckLimitContentStage(Const.ContentLimit.MENU_SUMMON))
            {
                bool shopFree = GameMgr.Shop.IsAbleBuy("dc_free_gem_pack");
                bool shopAdsProduct = false; //GameMgr.Shop.IsProductAds() && GameMgr.Ads.IsAdsPass();
                bool shopAdsSummon = GameMgr.Summon.IsSummonAds() && GameMgr.Ads.IsAdsPass();
                bool shopSummonLvReward = GameMgr.Summon.IsSummonLvReward();
                // bool isSummonFriend = DataMgr.Instance.GetPayment(Const.PaymentType.SUMMON_FRIEND_TICKET) >= Const.BIG_SUMMON_COUNT;
                // bool isSummonEqp = DataMgr.Instance.GetPayment(Const.PaymentType.SUMMON_EQP_TICKET) >= Const.BIG_SUMMON_COUNT;
                // bool isSummonAcc = DataMgr.Instance.GetPayment(Const.PaymentType.SUMMON_ACC_TICKET) >= Const.ACC_BIG_SUMMON_COUNT;
                
                shopNew.SetActive(shopFree || shopAdsProduct || shopAdsSummon || shopSummonLvReward );  //|| isSummonFriend || isSummonEqp || isSummonAcc
            }
            else
            {
                shopNew.SetActive(false);
            }
        }
    }

    public void UpdateSupplyLvupNew(Const.PaymentType paymentType)
    {
        if(paymentType != Const.PaymentType.GOLD)
        {
            return;
        }

        int summonLv = GameMgr.Summon.GetSummonSupplyLv();

        if(summonLv == GameMgr.Table.GetMaxSupplySummonLv())
        {
            supplyLvupNew.SetActive(false);
        }
        else if(GameMgr.Summon.GetSummonSupplyLvUpTime() != 0)  // 레벨업 쿨타임 동안은 레드닷 노출안함
        {
            supplyLvupNew.SetActive(false);
        }
        else if(GameMgr.Summon.GetSupplyLvChargeCount() == Const.MAX_SUPPLY_LV_CHARGE)
        {
            supplyLvupNew.SetActive(true);
        }
        else
        {
            SupplySummonLvTableData summonNextLevelTableData = GameMgr.Table.GetSupplySummonLvData(summonLv+1);
            supplyLvupNew.SetActive(DataMgr.Instance.GetPayment(paymentType) >= summonNextLevelTableData.NeedValue);
        }
    }

    public bool IsSupplyLvup()
    {
        return supplyLvupNew.activeSelf;
    }

    public void UpdateRoomLike()
    {
        if(roomLike)
        {
            List<DecoData> decoDatas = DataMgr.Instance.GetDecoDataList();
            bool isLike = false;
            for (int i = 0; i < decoDatas.Count; i++)  // 이미 하트가 있을 경우 제외 (하트는 한번에 1개만 노출가능)
            {
                if(decoDatas[i].isLikeExp)
                {
                    isLike = true;
                    break;
                }
            }
            roomLike.SetActive(isLike);
        }
    }

    public void UpdateLimitAll()
    {
        UpdateLimit(Const.ContentLimit.MENU_FRIEND);
        UpdateLimit(Const.ContentLimit.DUNGEON_GOLD);
        UpdateLimit(Const.ContentLimit.MENU_PLAYGRUOND);
        UpdateLimit(Const.ContentLimit.MENU_SUMMON);
        UpdateSupplyAutoLimit();
    }

    private void UpdateLimit(Const.ContentLimit limitType)
    {
        switch (limitType)
        {
            case Const.ContentLimit.MENU_FRIEND:
                friendLimit.SetActive(DataMgr.Instance.CheckLimitContentStage(limitType));
                break;
            case Const.ContentLimit.DUNGEON_GOLD:
                duneonLimit.SetActive(DataMgr.Instance.CheckLimitContentStage(limitType));
                break;
            case Const.ContentLimit.MENU_PLAYGRUOND:
                roomLimit.SetActive(DataMgr.Instance.CheckLimitContentStage(limitType));
                break;
            case Const.ContentLimit.MENU_SUMMON:
                shopLimit.SetActive(DataMgr.Instance.CheckLimitContentStage(limitType));
                break;
        }
    }

    public void UpdateSupplyAutoLimit()
    {
        supplyAutoLimit.SetActive(DataMgr.Instance.CheckLimitContentStage(Const.ContentLimit.SUPPLY_AUTO_SUMMON));
    }

    public void HideAllOffBtn()
    {
        playerOff.SetActive(false);
        friendOff.SetActive(false);
        duneonOff.SetActive(false);
        roomOff.SetActive(false);
        shopOff.SetActive(false);
        supplyObj.SetActive(true);
    }

    public void ClickPlayerBtn()
    {
        if(GameMgr.GlobalBtnLockFlag)
        {
            return;
        }

        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);
        HideAllOffBtn();
        if(PopupMgr.Instance.IsActive(PopupMgr.PopupHero))
        {
            PopupMgr.Instance.HidePopup(PopupMgr.PopupHero);
            supplyObj.SetActive(true);
        }
        else
        {
            PopupMgr.Instance.HideAllPopup();
            PopupMgr.Instance.ShowPopup(PopupMgr.PopupHero);
            playerOff.SetActive(true);
            supplyObj.SetActive(false);
        }
    }

    public void ClickFriendBtn()
    {
        if(GameMgr.GlobalBtnLockFlag)
        {
            return;
        }

        if(friendLimit.activeSelf)
        {
            ClickLimitBtn("MENU_FRIEND");
            return;
        }

        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);
        HideAllOffBtn();
        if(PopupMgr.Instance.IsActive(PopupMgr.PopupFriend))
        {
            PopupMgr.Instance.HidePopup(PopupMgr.PopupFriend);
            supplyObj.SetActive(true);
        }
        else
        {
            PopupMgr.Instance.HideAllPopup();
            PopupMgr.Instance.ShowPopup(PopupMgr.PopupFriend);
            friendOff.SetActive(true);
            supplyObj.SetActive(false);
        }
    }

    public void ClickDungeonBtn()
    {
        if(GameMgr.GlobalBtnLockFlag)
        {
            return;
        }

        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);
        HideAllOffBtn();
        if(PopupMgr.Instance.IsActive(PopupMgr.PopupDungeon))
        {
            PopupMgr.Instance.HidePopup(PopupMgr.PopupDungeon);
            supplyObj.SetActive(true);
        }
        else
        {
            PopupMgr.Instance.HideAllPopup();
            PopupMgr.Instance.ShowPopup(PopupMgr.PopupDungeon);
            duneonOff.SetActive(true);
            supplyObj.SetActive(false);
        }
    }

    public void ClickRoomBtn()
    {
        if(GameMgr.GlobalBtnLockFlag)
        {
            return;
        }

        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);
        HideAllOffBtn();
        PopupMgr.Instance.HideAllPopup();

        // 임시
        RoomMgr.Instance.ShowRoom();

        // if(PopupMgr.Instance.IsActive(PopupMgr.PopupFriend))
        // {
        //     PopupMgr.Instance.HidePopup(PopupMgr.PopupFriend);
        // }
        // else
        // {
        //     PopupMgr.Instance.HideAllPopup();
        //     PopupMgr.Instance.ShowPopup(PopupMgr.PopupFriend);
        // }
    }

    public void ClickShopBtn()
    {
        if(GameMgr.GlobalBtnLockFlag)
        {
            return;
        }

        ActShopBtn();        
    }

    public void ActShopBtn(int tapNum = -1, string group = "")
    {
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);
        HideAllOffBtn();
        if(PopupMgr.Instance.IsActive(PopupMgr.PopupShop))
        {
            PopupMgr.Instance.HidePopup(PopupMgr.PopupShop);
            supplyObj.SetActive(true);
        }
        else
        {
            PopupMgr.Instance.HideAllPopup();
            if(tapNum == -1)
            {
                PopupMgr.Instance.ShowPopup(PopupMgr.PopupShop, 1, group);
            }
            else
            {
                PopupMgr.Instance.ShowPopup(PopupMgr.PopupShop, tapNum, group);
            }
            shopOff.SetActive(true);
            supplyObj.SetActive(false);
        }
    }
    
    public void ClickLimitBtn(string limitTypeStr)
    {
        if(GameMgr.GlobalBtnLockFlag)
        {
            return;
        }

        // if(limitTypeStr == "MENU_PLAYGRUOND")
        // {   
        //     PopupMgr.Instance.ShowAlert(PopupMgr.AlertNoContent);
        //     return;
        // }

        DataMgr.Instance.CheckLimitContentWarning(JoyUtil.StringToEnum<Const.ContentLimit>(limitTypeStr));
        // SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_CANCEL);
    }



    public void ClickSummonBtn()
    {
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);
        CheckSupplyAuto();
        if(isSummonAni)
        {
            return;
        }

        // StopAutoSummon();
        ActSummon(1);
    }

    public void ActSummon(int count, bool isAuto = false)
    {
        if(MiddleUI.Instance.CheckSupply())  // 기존에 소환해둔 수집품이 있을 경우 우선 띄워줌
        {
            return;
        }

        if(DataMgr.Instance.GetPayment(Const.PaymentType.CAT_JELLY) >= count)
        {
            DataMgr.Instance.IncPayment(Const.PaymentType.CAT_JELLY, -count);
            GameMgr.Summon.SummonSupplyMulti(count, isAuto);
        }
        else
        {
            CheckSupplyAuto();

            if(Const.IS_BATTLE_SCENE && DataMgr.Instance.nowDungeon == Const.DUNGEON_STAGE && !Const.IS_EVENT_MINIGAME && !PopupMgr.Instance.IsActive())
            {
                if(DataMgr.Instance.GetMaxStageNo() < 1003001)
                {
                    PopupMgr.Instance.ShowAlertNotEnoughtPayment(Const.PaymentType.CAT_JELLY);
                }
                else
                {
                    PopupMgr.Instance.ShowPopup(PopupMgr.PopupShortage, Const.PaymentType.CAT_JELLY);
                }
            }
            
            // 고양이 젤리 광고 비활성화 해둠
            // if(GameMgr.Ads.GetAbleAdsCount(AdsMgr.AdsType.ADS_CAT_JELLY) > 0)
            // {
            //     PopupMgr.Instance.ShowPopup(PopupMgr.PopupAdsCatJelly);
            // }
            // else
            // {
            //     CheckSupplyAuto();
            // }
        }
    }

    Coroutine co_autoSummon = null;
    public bool isAutoSummon = false;
    public bool isSummonAni = false;

    public void ClickSummonAutoBtn()
    {
        CheckSupplyAuto();
        if(isSummonAni)
        {
            return;
        }
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);

        if(MiddleUI.Instance.CheckSupply())  // 기존에 소환해둔 수집품이 있을 경우 우선 띄워줌
        {
            return;
        }

        PopupMgr.Instance.ShowPopup(PopupMgr.PopupSuppliesAutoSetting);
    } 

    public void ClickSummonAutoLock()
    {
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);
        DataMgr.Instance.CheckLimitContentWarning(Const.ContentLimit.SUPPLY_AUTO_SUMMON);
    }

    public void ClickSummonLvupBtn()
    {
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);
        PopupMgr.Instance.ShowPopup(PopupMgr.PopupSuppliesLvInfo);
    }

    public void StartAutoSummon()
    {
        // co_autoSummon = StartCoroutine(Co_autoSummon());
        if(isAutoSummon)
        {
            int summonCount = GameMgr.Eqp.GetSupplyFilterSummonCount();
            ActSummon(summonCount, true);
        }
    }
    public void StartAutoSummonDelay(float delayT)
    {
        // Invoke("StartAutoSummon", delayT);
        Invoke("StartAutoSummon", 1f);
    }

    public void StopAutoSummon()
    {
        if(co_autoSummon != null)
        {
            StopCoroutine(co_autoSummon);
            co_autoSummon = null;
        }
    }


    public void CheckSupplyAuto()
    {
        if(isAutoSummon)
        {
            PopupMgr.Instance.ShowAlert(PopupMgr.AlertJellyAutoOff);
            // PopupMgr.Instance.ShowPopup(PopupMgr.PopupShortage, _ticketType);
        }
        isAutoSummon = false;
        UpdateAutoBtn();
    }

    // public IEnumerator Co_autoSummon()
    // {
    //     // while(true)
    //     // {
            
    //     // }
    //     yield return new WaitForSecondsRealtime(2);
    //     ActSummon(true);
        
    // }


    public void UpdateSupplySummonLv()
    {
        supplySummonLvText.text = DataMgr.Instance.GetSupplySummonLv().ToString();
    }

    public void UpdateSupplyBtnEff()
    {
        if(Const.IS_BATTLE_SCENE)
        {
            if(PopupMgr.Instance.GetShowPopupCount() > 0)
            {
                supplyBtnEff.SetActive(false);
                supplySellEff.SetActive(false);
            }
            else
            {
                supplyBtnEff.SetActive(GameMgr.Eqp.GetTempSupplyDataCount() > 0);
            }
        }
        else
        {
            supplyBtnEff.SetActive(false);
            supplySellEff.SetActive(false);
        }
        
        UpdateAutoBtn();
    }

    public void UpdateAutoBtn()
    {
        if(Const.IS_BATTLE_SCENE)
        {
            if(PopupMgr.Instance.GetShowPopupCount() > 0)
            {
                supplyAutoEff.SetActive(false);
            }
            else
            {
                supplyAutoEff.SetActive(isAutoSummon);
            }
            supplyAutoOn.SetActive(isAutoSummon);
            supplyAutoOff.SetActive(!isAutoSummon);
        }
        else
        {
            supplyAutoEff.SetActive(false);
        }
    }

    
    List<DataMgr.PaymentData> _sellPaymentDatas;
    public void ShowSellEff(List<DataMgr.PaymentData> paymentDatas)
    {
        _sellPaymentDatas = paymentDatas;
        Invoke("ShowSellEff", 0.01f);
    }

    public void ShowSellEff()
    {
        if(Const.IS_BATTLE_SCENE && PopupMgr.Instance.GetShowPopupCount() <= 0)
        {
            _sellPaymentDatas = DataMgr.Instance.SumSamePaymentData(_sellPaymentDatas);
            for (int i = 0; i < _sellPaymentDatas.Count; i++)
            {
                Vector3 pos = supplySummonObj.transform.position + new Vector3(-1, i*0.3f-1, 0);
                BattleMgr.Instance.ShowGetPayment(_sellPaymentDatas[i].type, _sellPaymentDatas[i].value, pos);
            }

            SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_JELLY_SELL);

            supplySellEff.SetActive(false);
            supplySellEff.SetActive(true);
        }
    }

    public void ShowSupplySummonEff()
    {
        List<SupplyData> supplyDatas = GameMgr.Eqp.GetTempSupplyDataList();
        if(supplyDatas.Count <= 0)
        {
            return;
        }

        isSummonAni = true;
        Const.GradeTypeCapital grade = supplyDatas[0].Table.Grade;

        // supplyIcon.gameObject.SetActive(true);
        supplyIcon.sprite = GameMgr.Resource.LoadUIImage(supplyDatas[0].Table.IconName);

        string skinName = "skin" + (int)grade;
        string cloudAniName = "cloud" + (int)grade;
        cloudSkeleton.gameObject.SetActive(true);
        cloudSkeleton.Clear();
        cloudSkeleton.timeScale = 1.5f;
        cloudSkeleton.Skeleton.SetSkin(skinName);
        cloudSkeleton.Skeleton.SetSlotsToSetupPose();
        cloudSkeleton.AnimationState.SetAnimation(0, cloudAniName, false);
        cloudSkeleton.AnimationState.Complete += OnCloudAnimationComplete;
        cloudSkeleton.AnimationState.Event += OnCloudAnimationEvent;

        string footAniName = "foot" + (int)grade;
        footSkeleton.gameObject.SetActive(true);
        footSkeleton.Clear();
        footSkeleton.timeScale = 1.5f;
        footSkeleton.Skeleton.SetSkin(skinName);
        footSkeleton.Skeleton.SetSlotsToSetupPose();
        footSkeleton.AnimationState.SetAnimation(0, footAniName, false);
        footSkeleton.AnimationState.Complete += OnFootAnimationComplete;

        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_JELLY_TOUCH);

    }
    public void HideSupplySummonEff()
    {
        cloudSkeleton.gameObject.SetActive(false);
        footSkeleton.gameObject.SetActive(false);
        supplyIcon.gameObject.SetActive(false);
    }

    public void OnCloudAnimationComplete(Spine.TrackEntry trackEntry)
    {
        switch (trackEntry.Animation.Name)
        {
            case "idle":
                break;
            default:
                isSummonAni = false;
                cloudSkeleton.AnimationState.SetAnimation(0, "idle", true);
                GameMgr.Eqp.CheckSupplyEqp(isAutoSummon);
                break;
        }
    }

    private void OnCloudAnimationEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        switch (e.Data.Name)
        {
            case "icon":
                supplyIcon.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void OnFootAnimationComplete(Spine.TrackEntry trackEntry)
    {
        switch (trackEntry.Animation.Name)
        {
            default:
                footSkeleton.gameObject.SetActive(false);
                break;
        }
    }

    public void CheckRemainSupply()
    {
        if(isSummonAni)
        {
            return;
        }

        List<SupplyData> supplyDatas = GameMgr.Eqp.GetTempSupplyDataList();

        if(supplyDatas.Count > 0)
        {
            Const.GradeTypeCapital grade = supplyDatas[0].Table.Grade;
            supplyIcon.gameObject.SetActive(true);
            supplyIcon.sprite = GameMgr.Resource.LoadUIImage(supplyDatas[0].Table.IconName);

            string cloudSkinName = "skin" + (int)grade;
            string cloutAniName = "idle";
            cloudSkeleton.gameObject.SetActive(true);
            cloudSkeleton.Skeleton.SetSkin(cloudSkinName);
            cloudSkeleton.Skeleton.SetSlotsToSetupPose();
            cloudSkeleton.AnimationState.SetAnimation(0, cloutAniName, true);
        }
        else
        {
            HideSupplySummonEff();
        }
    }



    Coroutine co_TouchRepeat = null;

    public void CheckSupplySummonTouch()
    {
        if(co_TouchRepeat != null)
        {
            StopCoroutine(co_TouchRepeat);
            co_TouchRepeat = null;
        }

        if(DataMgr.Instance.GetPayment(Const.PaymentType.CAT_JELLY) > 0)
        {
            if(gameObject.activeSelf)
            {
                co_TouchRepeat = StartCoroutine(Co_TouchRepeat());
            }
        }
    }

    IEnumerator Co_TouchRepeat()
    {
        while(true)
        {
            if(!Const.IS_SAVE_POWER_MODE && !PopupMgr.Instance.IsActive() && Const.IS_BATTLE_SCENE && !isAutoSummon)
            {
                // PopupMgr.Instance.ShowTouchObj(supplySummonObj);
                ShowTouchObj();
            }
            yield return new WaitForSecondsRealtime(11f);
        }
    }

    public void ShowTouchObj()
    {
        PopupMgr.Instance.ShowTouchObj(supplySummonObj);
    }


    public GameObject GetShopObj()
    {
        return shopLimit;
    }
    public GameObject GetPlayerObj()
    {
        return playerLimit;
    }
    public GameObject GetFriendObj()
    {
        return friendLimit;
    }
    public GameObject GetDungeonObj()
    {
        return duneonLimit;
    }
    public GameObject GetRoomObj()
    {
        return roomLimit;
    }
    public GameObject GetSupplySummonObj()
    {
        return supplySummonObj;
    }
    public GameObject GetSupplySummonLvObj()
    {
        return supplySummonLvObj;
    }
    public GameObject GetSupplyAutoSummonObj()
    {
        return supplyAutoSummonObj;
    }
}
