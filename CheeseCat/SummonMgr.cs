using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LitJson;

public class SummonMgr
{

    public delegate void UpdateSummonData();
    public UpdateSummonData onUpdateSummonData;

    public delegate void UpdateSummonRewardData();
    public UpdateSummonRewardData onUpdateSummonRewardData;

    public delegate void UpdateSupplySummonLvData();
    public UpdateSupplySummonLvData onUpdateSupplySummonLvData;

    public void Init() {
        // TimeMgr.Instance.onUpdateDaily += ResetDailyPass;
    }

    public void IncSummonResult(string type, List<SummonTableData> summonTableList, string paymentType, int value)
    {
        string logType = "";
        switch (type)
        {
            case Const.TYPE_EQP:
                for (int i = 0; i < summonTableList.Count; i++)
                {
                    GameMgr.Eqp.AddEqp(summonTableList[i].UniqueKey);
                }
                logType = LogMgr.LOG_SUMMON_EQUIP;
                break;
            case Const.TYPE_ACC:
                for (int i = 0; i < summonTableList.Count; i++)
                {
                    GameMgr.Eqp.AddAcc(summonTableList[i].UniqueKey);
                }
                logType = LogMgr.LOG_SUMMON_ACC;
                break;
            case Const.TYPE_FRIEND:
                for (int i = 0; i < summonTableList.Count; i++)
                {
                    GameMgr.Unit.AddFriendPiece(summonTableList[i].UniqueKey);
                }
                logType = LogMgr.LOG_SUMMON_CAT;
                break;
            case Const.TYPE_GIFT:
                for (int i = 0; i < summonTableList.Count; i++)
                {
                    DataMgr.Instance.IncPayment(summonTableList[i].UniqueKey, 1);
                }
                break;
        }

        // 로그
        JsonData logJson = GameMgr.Log.GetLogObject(logType, summonTableList.Count);
        JsonData rewardJson = new JsonData();
        for (int i = 0; i < summonTableList.Count; i++)
        {
            JsonData data = new JsonData
            {
                summonTableList[i].UniqueKey,
                1
            };
            rewardJson.Add(data);
        }
        JsonData etcJson = logJson["etcData"];
        etcJson["price_type"] = paymentType;
        etcJson["price_value"] = value;
        etcJson["rewardArr"] = rewardJson;
        logJson["etcData"] = etcJson;
        string logdata = logJson.ToJson();

        DataMgr.Instance.SaveData(true, logdata);
    }


    public List<SummonTableData> SummonMulti(string type, int count, string paymentType, int value)
    {
        List<SummonTableData> summonTableList = new();
        for (int i = 0; i < count; i++)
        {
            summonTableList.Add(SummonOne(type, paymentType));   
        }

        // 실제 수량 증가
        IncSummonResult(type, summonTableList, paymentType, value);
        // 소환 수 증가
        IncSummonCount(type, count);
        
        // 전투력 체크
        GameMgr.Unit.CheckChangeBattlePower();

        onUpdateSummonData?.Invoke();
        DataMgr.Instance.onUpdateAlbum?.Invoke();

        return summonTableList;
    }


    public SummonTableData SummonOne(string type, string paymentType)
    {
        int summonLv = GetSummonLv(type);
        List<SummonTableData> summonTableDatas = GameMgr.Table.GetSummonTableList(type);
        SummonTableData summonTableData = null;

        int maxProb = summonTableDatas[summonTableDatas.Count - 1].GetCumulativeProb(summonLv);
        
        if (summonTableDatas != null)
        {
            if(type == Const.TYPE_ACC && paymentType == "FREE")
            {
                for (int i = 0; i < summonTableDatas.Count; i++)
                {
                    if(summonTableDatas[i].UniqueKey == "HAT_C_1")
                    {
                        summonTableData = summonTableDatas[i];
                        break;
                    }
                }
            }
            else
            {
                int ranProb = UnityEngine.Random.Range(0, maxProb);
                for (int i = 0; i < summonTableDatas.Count; i++)
                {
                    if (summonTableDatas[i].GetCumulativeProb(summonLv) > ranProb)
                    {
                        summonTableData = summonTableDatas[i];
                        break;
                    }
                }
            }
        }

        return summonTableData;
    }



    // 소환 레벨
    public int GetSummonLv(string summonType)
    {
        switch (summonType)
        {
            case Const.TYPE_EQP:
                return DataMgr.Instance.GameData.eqpSummonLv;
            case Const.TYPE_ACC:
                return DataMgr.Instance.GameData.accSummonLv;
            case Const.TYPE_FRIEND:
                return DataMgr.Instance.GameData.friendSummonLv;
        }
        return 1;
    }
    public void SetSummonLv(string summonType, int lv)
    {
        switch (summonType)
        {
            case Const.TYPE_EQP:
                DataMgr.Instance.GameData.eqpSummonLv = lv;
                break;
            case Const.TYPE_ACC:
                DataMgr.Instance.GameData.accSummonLv = lv;
                break;
            case Const.TYPE_FRIEND:
                DataMgr.Instance.GameData.friendSummonLv = lv;
                break;
        }
    }
    // 소환 횟수
    public int GetSummonCount(string summonType)
    {
        switch (summonType)
        {
            case Const.TYPE_EQP:
                return DataMgr.Instance.GameData.eqpSummonCount;
            case Const.TYPE_ACC:
                return DataMgr.Instance.GameData.accSummonCount;
            case Const.TYPE_FRIEND:
                return DataMgr.Instance.GameData.friendSummonCount;
        }
        return 0;
    }
    public void SetSummonCount(string summonType, int count)
    {
        switch (summonType)
        {
            case Const.TYPE_EQP:
                DataMgr.Instance.GameData.eqpSummonCount = count;
                break;
            case Const.TYPE_ACC:
                DataMgr.Instance.GameData.accSummonCount = count;
                break;
            case Const.TYPE_FRIEND:
                DataMgr.Instance.GameData.friendSummonCount = count;
                break;
        }
    }
    public void AddSummonCount(string summonType, int count)
    {
        switch (summonType)
        {
            case Const.TYPE_EQP:
                DataMgr.Instance.GameData.eqpSummonCount += count;
                break;
            case Const.TYPE_ACC:
                DataMgr.Instance.GameData.accSummonCount += count;
                break;
            case Const.TYPE_FRIEND:
                DataMgr.Instance.GameData.friendSummonCount += count;
                break;
        }
    }
    public int GetSummonTotalCount(string summonType)
    {
        switch (summonType)
        {
            case Const.TYPE_EQP:
                return DataMgr.Instance.GameData.eqpSummonTotalCount;
            case Const.TYPE_ACC:
                return DataMgr.Instance.GameData.accSummonTotalCount;
            case Const.TYPE_FRIEND:
                return DataMgr.Instance.GameData.friendSummonTotalCount;
        }
        return 0;
    }
    public void AddSummonTotalCount(string summonType, int count)
    {
        switch (summonType)
        {
            case Const.TYPE_EQP:
                DataMgr.Instance.GameData.eqpSummonTotalCount += count;
                GameMgr.Quest.IncQuestCount(Const.QUEST_SUM_SUMMON_EQP, count);
                break;
            case Const.TYPE_ACC:
                DataMgr.Instance.GameData.accSummonTotalCount += count;
                GameMgr.Quest.IncQuestCount(Const.QUEST_SUM_SUMMON_ACC, count);
                break;
            case Const.TYPE_FRIEND:
                DataMgr.Instance.GameData.friendSummonTotalCount += count;
                GameMgr.Quest.IncQuestCount(Const.QUEST_SUM_SUMMON_FRIEND, count);
                break;
        }
    }

    // 소환 수량증가 (공용 로직)
    public bool IncSummonCount(string summonType, int count)
    {
        if(summonType == Const.TYPE_GIFT)  // 호감도 선물 소환은 소환 수량 증가가 없음
        {
            return false;
        }

        if(summonType == Const.TYPE_FRIEND)
        {
            // 고양이 소환의 경우 이벤트 고양이 패스 카운트 증가
            if(TopUI.Instance.IsActiveEventCatPass())
            {
                DataMgr.Instance.IncPayment(Const.PaymentType.EVENT_PASS_CAT_ITEM, count);
            }
        }

        AddSummonCount(summonType, count);
        AddSummonTotalCount(summonType, count);

        // 레벨업 조건 달성시 레벨업 처리
        int lv = GetSummonLv(summonType);
        if (lv < Const.MAX_SUMMON_LV)
        {
            int needCount = GameMgr.Table.GetSummonLevelTableData(lv + 1).GetNeedCount(summonType);
            if (GetSummonCount(summonType) >= needCount)
            {    // 1렙 증가씩 체크
                AddSummonCount(summonType, -needCount);
                SetSummonLv(summonType, lv+1);
                if (GetSummonLv(summonType) > Const.MAX_SUMMON_LV)
                {
                    SetSummonLv(summonType, Const.MAX_SUMMON_LV);
                }
                // // 한정 상품 체크
                // CheckOpenTimeLimitProduct(summonType);
                // GameMgr.Quest.IncQuestCount(Const.QUEST_GET_SUMMON_LV, 0);
                IncSummonCount(summonType, 0); // 추가 레벨업 체크
                return true;
            }
        }
        return false;
    }

    public void CheckOpenTimeLimitProduct(string summonType)
    {
        switch (summonType)
        {
            case Const.TYPE_EQP:
                GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_TYPE_SUMMON_EQP);
                break;
            case Const.TYPE_ACC:
                break;
            case Const.TYPE_FRIEND:
                GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_TYPE_SUMMON_FRIEND);
                break;
        }
    }


    // 광고 소환 남았는지 체크
    public bool IsSummonAds()
    {
        bool summonEqp = GameMgr.Ads.IsAds(AdsMgr.AdsType.ADS_SUMMON_EQUIP) && GameMgr.Ads.GetRemainAdsCoolTime(AdsMgr.AdsType.ADS_SUMMON_EQUIP) <= 0 && !DataMgr.Instance.CheckLimitContentStage(Const.ContentLimit.SUMMON_EQP);
        bool summonFriend = GameMgr.Ads.IsAds(AdsMgr.AdsType.ADS_SUMMON_FRIEND) && GameMgr.Ads.GetRemainAdsCoolTime(AdsMgr.AdsType.ADS_SUMMON_FRIEND) <= 0 && !DataMgr.Instance.CheckLimitContentStage(Const.ContentLimit.MENU_SUMMON);
        bool summonAcc = GameMgr.Ads.IsAds(AdsMgr.AdsType.ADS_SUMMON_ACC) && GameMgr.Ads.GetRemainAdsCoolTime(AdsMgr.AdsType.ADS_SUMMON_ACC) <= 0 && !DataMgr.Instance.CheckLimitContentStage(Const.ContentLimit.SUMMON_ACC);
        return summonEqp || summonFriend || summonAcc;
    }

    // 광고 소환 횟수
    public int GetSummonAdsViewCount(string summonType)
    {
        switch (summonType)
        {
            case Const.TYPE_EQP:
                return DataMgr.Instance.GameData.eqpSummonAdsCount;
            case Const.TYPE_ACC:
                return DataMgr.Instance.GameData.accSummonAdsCount;
            case Const.TYPE_FRIEND:
                return DataMgr.Instance.GameData.friendSummonAdsCount;
        }
        return 0;
    }
    public void AddSummonAdsViewCount(string summonType)
    {
        switch (summonType)
        {
            case Const.TYPE_EQP:
                DataMgr.Instance.GameData.eqpSummonAdsCount++;
                break;
            case Const.TYPE_ACC:
                DataMgr.Instance.GameData.accSummonAdsCount++;
                break;
            case Const.TYPE_FRIEND:
                DataMgr.Instance.GameData.friendSummonAdsCount++;
                break;
        }
    }
    public int GetSummonAdsCount(string summonType)
    {
        int summonCount = Const.ADS_SUMMON_MIN_COUNT;
        summonCount += GetSummonAdsViewCount(summonType);
        if(summonCount > Const.ADS_SUMMON_MAX_COUNT)
        {
            summonCount = Const.ADS_SUMMON_MAX_COUNT;
        }
        return summonCount;
    }


    public int GetSummonEffect(string summonType, Const.GradeTypeCapital grade)
    {
        SummonLevelTableData summonLevelTableData = GameMgr.Table.GetSummonLevelTableData(GetSummonLv(summonType));
        if(grade >= summonLevelTableData.GetEffectGrade2(summonType))
        {
            return 2;
        }
        else if(grade >= summonLevelTableData.GetEffectGrade(summonType))
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void ResetSummonLv()
    {
        DataMgr.Instance.GameData.eqpSummonAdsCount = 0;
        DataMgr.Instance.GameData.accSummonAdsCount = 0;
        DataMgr.Instance.GameData.friendSummonAdsCount = 0;

        DataMgr.Instance.GameData.eqpSummonCount = 0;
        DataMgr.Instance.GameData.accSummonCount = 0;
        DataMgr.Instance.GameData.friendSummonCount = 0;

        DataMgr.Instance.GameData.eqpSummonTotalCount = 0;
        DataMgr.Instance.GameData.accSummonTotalCount = 0;
        DataMgr.Instance.GameData.friendSummonTotalCount = 0;

        DataMgr.Instance.GameData.eqpSummonLv = 1;
        DataMgr.Instance.GameData.accSummonLv = 1;
        DataMgr.Instance.GameData.friendSummonLv = 1;

        onUpdateSummonData?.Invoke();
    }




    public int GetSummonRewardLv(string type)
    {
        if(!DataMgr.Instance.GameData.summonRewardLvDic.ContainsKey(type))
        {
            DataMgr.Instance.GameData.summonRewardLvDic[type] = 1;
        }
        return DataMgr.Instance.GameData.summonRewardLvDic[type];
    }
    public void SetSummonRewardLv(string type, int lv)
    {
        DataMgr.Instance.GameData.summonRewardLvDic[type] = lv;
    }
    public void ResetSummonRewardLv()
    {
        DataMgr.Instance.GameData.summonRewardLvDic.Clear();
    }
    // 소환 레벨 보상 받기 (받을 수 있는곳 까지 전부)
    public void GetSummonReward(string type)
    {
        List<SummonLevelTableData> summonLevelTableDatas = GameMgr.Table.GetSummonLevelTableDatas();
        List<DataMgr.PaymentData> paymentDatas = new();
        int startIdx = GetSummonRewardLv(type);
        int endIdx = GetSummonLv(type);

        for (int i = startIdx; i < endIdx; i++)
        {
            List<DataMgr.PaymentData> tempPaymentList = new();
            switch (type)
            {
                case Const.TYPE_EQP:
                    tempPaymentList = summonLevelTableDatas[i].GetEqpPaymentDataList();
                    break;
                case Const.TYPE_ACC:
                    tempPaymentList = summonLevelTableDatas[i].GetAccPaymentDataList();
                    break;
                case Const.TYPE_FRIEND:
                    tempPaymentList = summonLevelTableDatas[i].GetFriendPaymentDataList();
                    break;
            }
            paymentDatas.AddRange(tempPaymentList);
        }

        if(paymentDatas.Count > 0)
        {
            paymentDatas = DataMgr.Instance.IncPaymentDatas(paymentDatas);
            PopupMgr.Instance.ShowPopup(PopupMgr.PopupReward, paymentDatas);
        }
        SetSummonRewardLv(type, endIdx);
        DataMgr.Instance.SaveData(true);

        onUpdateSummonRewardData?.Invoke();
    }

    public bool IsSummonLvReward()
    {
        return IsSummonLvReward(Const.TYPE_EQP) || IsSummonLvReward(Const.TYPE_FRIEND) || IsSummonLvReward(Const.TYPE_ACC);
    }
    public bool IsSummonLvReward(string type)
    {
        return  GetSummonLv(type) > GetSummonRewardLv(type);
    }












    //// 고양이 수집품 소환 관련 로직
    // 고양이 수집품 소환 레벨
    public int GetSummonSupplyLv()
    {
        return DataMgr.Instance.GameData.supplySummonLv;
    }
    public void AddSummonSupplyLv()
    {
        DataMgr.Instance.GameData.supplySummonLv++;
        GameMgr.Quest.IncQuestCount(Const.QUEST_SUMMON_SUPPLY_LV, DataMgr.Instance.GameData.supplySummonLv);
    }

    public int GetSummonSupplyLvUpTime()
    {
        return DataMgr.Instance.GameData.supplySummonLvupTime;
    }
    public void SetSummonSupplyLvUpTime()
    {
        SupplySummonLvTableData data = GameMgr.Table.GetSupplySummonLvData(GetSummonSupplyLv()+1);
        int nowTime = TimeMgr.Instance.GetSvrNowT();
        DataMgr.Instance.GameData.supplySummonLvupTime = nowTime + data.LvUpTime;
    }
    public void ResetSummonSupplyLvUpTime()
    {
        DataMgr.Instance.GameData.supplySummonLvupTime = 0;
    }

    public void ChargeSupplyLv()
    {
        DataMgr.Instance.GameData.supplyLvChargeCount++;
        // 퀘스트
        GameMgr.Quest.IncQuestCount(Const.QUEST_SUMMON_SUPPLY_CHARGE, 1);
    }
    public void ResetChargeSupplyLv()
    {
        DataMgr.Instance.GameData.supplyLvChargeCount = 0;
    }
    public int GetSupplyLvChargeCount()
    {
        return DataMgr.Instance.GameData.supplyLvChargeCount;
    }
    public bool IsFullChargeSupplyLv()
    {
        return DataMgr.Instance.GameData.supplyLvChargeCount >= Const.MAX_SUPPLY_LV_CHARGE;
    }

    public void CheckSupplyLvup()
    {
        if(DataMgr.Instance.GameData == null)  // 데이터 로드 전에는 무시함
        {
            return;
        }

        int nowTime = TimeMgr.Instance.GetSvrNowT();
        int lvupTime = GetSummonSupplyLvUpTime();

        if(lvupTime != 0)
        {
            if(nowTime > lvupTime)
            {
                AddSummonSupplyLv();
                ResetSummonSupplyLvUpTime();
                ResetChargeSupplyLv();

                // 레벨업 시 연출
                SupplySummonLvTableData lvData = GameMgr.Table.GetSupplySummonLvData(GetSummonSupplyLv());
                if(Const.IS_BATTLE_SCENE && !Const.IS_SAVE_POWER_MODE)
                {
                    // if(TutoMgr.Instance.IsTutorial())  // 튜토리얼중에 컨텐츠 오픈 잠금
                    // {
                    //     return;
                    // }
                    PopupMgr.Instance.ShowPopup(PopupMgr.PopupLvUp, Const.LvUpType.JELLY, lvData);
                }

                onUpdateSupplySummonLvData?.Invoke();
            }
        }
    }

    public void AccelSupplyLvUpTime(int time)
    {
        DataMgr.Instance.GameData.supplySummonLvupTime -= time;
        CheckSupplyLvup();
    }

    // 고양이 수집품 다수 소환
    public List<SummonTableData> SummonSupplyMulti(int count, bool isAuto = false)
    {
        List<SummonTableData> summonTableList = new();
        for (int i = 0; i < count; i++)
        {
            summonTableList.Add(SummonSupplyOne());   
        }

        // 실제 수량 증가
        IncSupplySummonResult(summonTableList, isAuto);
        // 소환 수 증가
        IncSupplySummonCount(count);
        if(isAuto)
        {
            IncSupplyAutoSummonCount(count);
        }
        GameMgr.Quest.IncQuestCount(Const.QUEST_SUMMON_SUPPLY, count);
        GameMgr.Quest.IncQuestCount(Const.QUEST_SUMMON_SUPPLY_ADS, count);
        GameMgr.Quest.IncQuestCount(Const.QUEST_AUTO_SUMMON_SUPPLY, count);
        
        // onUpdateSummonData?.Invoke();

        return summonTableList;
    }

    
    private List<SummonTableData> _summonTableDatas = null;
    private int _maxProb = 0;

    // 고양이 수집품 1회 소환
    public SummonTableData SummonSupplyOne()
    {
        int summonLv = GetSummonSupplyLv();
        if(_summonTableDatas == null)
        {
            _summonTableDatas = GameMgr.Table.GetSupplySummonTableList();
        }
        // List<SummonTableData> _summonTableDatas = GameMgr.Table.GetSupplySummonTableList();
        SummonTableData summonTableData = null;
        
        int maxProb = _summonTableDatas[_summonTableDatas.Count - 1].GetCumulativeProb(summonLv);
        
        if (_summonTableDatas != null)
        {
            int ranProb = UnityEngine.Random.Range(0, maxProb);
            for (int i = 0; i < _summonTableDatas.Count; i++)
            {
                if (_summonTableDatas[i].GetCumulativeProb(summonLv) > ranProb)
                {
                    summonTableData = _summonTableDatas[i];
                    break;
                }
            }
        }

        return summonTableData;
    }
    // 소환 결과 처리
    public void IncSupplySummonResult(List<SummonTableData> summonTableList, bool isAuto = false)
    {
        string logType = LogMgr.LOG_SUMMON_SUPPLY;
        // for (int i = 0; i < summonTableList.Count; i++)
        // {
        //     // 소환된 장비 처리 (자동 분해, 장착 팝업)
        //     GameMgr.Eqp.Check(summonTableList[i]);
        // }
        GameMgr.Eqp.MakeAllSupplyEqp(summonTableList, isAuto);

        // 로그
        JsonData logJson = GameMgr.Log.GetLogObject(logType, (int)DataMgr.Instance.GetPayment(Const.PaymentType.CAT_JELLY));
        JsonData rewardJson = new JsonData();
        for (int i = 0; i < summonTableList.Count; i++)
        {
            JsonData data = new JsonData
            {
                summonTableList[i].UniqueKey,
                1
            };
            rewardJson.Add(data);
        }
        JsonData etcJson = logJson["etcData"];
        etcJson["rewardArr"] = rewardJson;
        etcJson["price_type"] = Const.PaymentType.CAT_JELLY.ToString();
        etcJson["price_value"] = summonTableList.Count;
        logJson["etcData"] = etcJson;
        string logdata = logJson.ToJson();

        DataMgr.Instance.SaveData(true, logdata);
    }
    // 소환 횟수 추가 ( 실제 사용은 안함 데이터만 저장됨)
    public void IncSupplySummonCount(int count)
    {
        DataMgr.Instance.GameData.supplySummonCount += count;
    }
    public void IncSupplyAutoSummonCount(int count)
    {
        DataMgr.Instance.GameData.supplyAutoSummonCount += count;
    }
    public double GetSupplyAutoSummonCount()
    {
        return DataMgr.Instance.GameData.supplyAutoSummonCount;
    }


    // 소환 확률 리스트
    public List<int> GetSupplySummonProbList(int lv)
    {
        List<SummonTableData> summonTableDatas = GameMgr.Table.GetSupplySummonTableList();
        List<int> supplySummonProbList = new();

        Const.GradeTypeCapital tempGrade = Const.GradeTypeCapital.C;
        int probSum = 0;
        for (int i = 0; i < summonTableDatas.Count; i++)
        {
            if(tempGrade != summonTableDatas[i].Grade)
            {
                supplySummonProbList.Add(probSum);
                tempGrade = summonTableDatas[i].Grade;
                probSum = 0;
                probSum += summonTableDatas[i].GetProb(lv);
            }
            else
            {
                probSum += summonTableDatas[i].GetProb(lv);
            }
        }
        supplySummonProbList.Add(probSum);

        return supplySummonProbList;
    }
    
}
