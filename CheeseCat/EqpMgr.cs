using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;
using LitJson;
using System;

// 장비데이터 관리
public class EqpMgr
{
    //private int _eqpUniqueIdx = 1; // 발급한 장비의 고유번호. 1부터 시작

    //public List<EqpData> _eqpDataList;  // 보유한 장비
    // public List<EqpData> _eqpDataList = new();

    // 장비 상태 변경 시 연동 
    public delegate void UpdateEqpData();
    public UpdateEqpData onUpdateEqpData;

    public delegate void UpdateAccData();
    public UpdateAccData onUpdateAccData; 

    public delegate void UpdateSupplyData();
    public UpdateSupplyData onUpdateSupplyData;

    public delegate void UpdateToyData();
    public UpdateToyData onUpdateToyData;

    // 옷장 레벨업시 연동
    public delegate void UpdateWardrobeLvUp();
    public UpdateWardrobeLvUp onUpdateWardrobeLvUp;

    // 옷장 경험치업 시 연동
    public delegate void UpdateWardrobeExp();
    public UpdateWardrobeExp onUpdateWardrobeExp;

    // 룬 상태 연동
    public delegate void UpdateRuneData();
    public UpdateRuneData onUpdateRuneData;


    // 보유한 장비 정보
    public Dictionary<string, EqpData> _eqpDataDic = new();
    // 보유한 의상 정보
    public Dictionary<long, AccData> _accDataDic = new();
    // 보유 장난감 정보
    public Dictionary<string, ToyData> _toyDataDic = new();
    // 보유 룬 정보
    public Dictionary<string, RuneData> _runeDataDic = new();

    // 용품
    public Dictionary<string, SupplyData> _supplyDataDic = new();
    public List<SupplyData> _supplyTempDataList = new();

    // 전체 장비를 6개 부위별 리스트로 정리
    Dictionary<string, List<EqpTableData>> _eqpTypeTableDic = new Dictionary<string, List<EqpTableData>>();


    // 착용 장비
    Dictionary<string, EqpData> meleeEqpDic = new Dictionary<string, EqpData>();

    // 착용 의상
    Dictionary<string, AccData> _accEqpDic = new();

    // // 착용 장난감 코드
    // public List<string> _toyEqpCodeList = new() {"", ""};


    // // 장비에 붙을 수 있는 능력치
    // string[] STAT_LIST = new string[] { Const.STAT_ATTACK_POWER, Const.STAT_HP, Const.STAT_DEF, Const.STAT_CRITICAL_DAMAGE };

    // 장비 종류
    string[] EQP_TYPE_LIST = new string[] { Const.EQP_WEAPON, Const.EQP_HELMET, Const.EQP_ARMOR, Const.EQP_SHOES, Const.EQP_RING, Const.EQP_NECKLACE };


    //// 장비 추천용 데이터
    EqpData _tempEqpData = null;  // 임시 장비 데이터 (임시로 전투력 비교할때 잠깐씩 데이터 들어감)
    // List<RecommendEqpData> _recommendEqpDataList = new List<RecommendEqpData>();
    // public List<RecommendEqpData> RecommendEqpDataList => _recommendEqpDataList;
    // int _tempEqpGetCount = 0;


    public void Init()
    {
        Dictionary<string, EqpTableData> dic = GameMgr.Table.GetEqpTableDataAll();
        foreach (KeyValuePair<string, EqpTableData> d in dic)
        {
            if (!_eqpTypeTableDic.ContainsKey(d.Value.EqpType))
            {
                _eqpTypeTableDic[d.Value.EqpType] = new List<EqpTableData>();
            }
            _eqpTypeTableDic[d.Value.EqpType].Add(d.Value);
        }

        onUpdateEqpData += UpdateCallbackBase;
    }

    public void UpdateCallbackBase()
    {
    }

    // 착용장비를 dictionary로 구성
    public void UpdateEqpDic()
    {
        meleeEqpDic[Const.EQP_WEAPON] = GetEqpData(DataMgr.Instance.GameData.meleeWeaponEqpUnique);
        meleeEqpDic[Const.EQP_HELMET] = GetEqpData(DataMgr.Instance.GameData.meleeHelmetEqpUnique);
        meleeEqpDic[Const.EQP_ARMOR] = GetEqpData(DataMgr.Instance.GameData.meleeArmorEqpUnique);
        meleeEqpDic[Const.EQP_SHOES] = GetEqpData(DataMgr.Instance.GameData.meleeShoesEqpUnique);
        meleeEqpDic[Const.EQP_RING] = GetEqpData(DataMgr.Instance.GameData.meleeRingEqpUnique);
        meleeEqpDic[Const.EQP_NECKLACE] = GetEqpData(DataMgr.Instance.GameData.meleeNeckEqpUnique);

        _accEqpDic[Const.EQP_HAT] = GetAccData(DataMgr.Instance.GameData.accHatEqpUnique);
        _accEqpDic[Const.EQP_TOP] = GetAccData(DataMgr.Instance.GameData.accTopEqpUnique);
    }

    // 지정한 번호의 장비 획득
    public EqpData AddEqp(string eqpUnique, int count = 1)
    {
        if (!_eqpDataDic.ContainsKey(eqpUnique))
        {
            _eqpDataDic[eqpUnique] = new EqpData();
            _eqpDataDic[eqpUnique].eqpUnique = eqpUnique;
        }
        _eqpDataDic[eqpUnique].AddOwn(count);  // 보유수량 증가

        // GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_EQP_A, count, _eqpDataDic[eqpUnique].eqpUnique);

        return _eqpDataDic[eqpUnique];


    }

    // 지정한 장비정보 반환
    public EqpData GetEqpData(string eqpUnique)
    {
        if (_eqpDataDic.ContainsKey(eqpUnique))
        {
            return _eqpDataDic[eqpUnique];
        }
        else
        {
            return null;
        }
    }

    // 보유장비 전체 반환
    public Dictionary<string, EqpData> GetAllEqpData()
    {
        return _eqpDataDic;
    }

    // 해당 장비보다 좋은 장비가 있는지 여부 반환
    public bool IsUpperEqp(EqpData eqpData)
    {
        if(eqpData == null)  // 장착한 장비가 없을 경우 하나라도 다른 장비가 있으면 true
        {
            return _eqpDataDic.Count > 0;
        }
        else  // 장착한 장비가 있을 경우 더 좋은 장비가 있으면 true
        {
            double tempStat = 0f;
            double nowStat = GetEqpStat(eqpData.eqpUnique, eqpData.lv, eqpData.Table.StatCode);
            foreach (var item in _eqpDataDic)
            {
                if(item.Value.Table.EqpType == eqpData.Table.EqpType)  // 타입이 같은 경우만 체크
                {
                    tempStat = GetEqpStat(item.Value.eqpUnique, item.Value.lv, item.Value.Table.StatCode);
                    if(tempStat > nowStat)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    // 지정한 장비의 레벨별 능력치 반환
    public double GetEqpStat(string eqpUnique, int lv, string statType)
    {
        EqpTableData eqpTableData = GameMgr.Table.GetEqpTableData(eqpUnique);

        if (eqpTableData == null)
        {
            return 0;
        }

        CommonLvTableData lvTableData = GameMgr.Table.GetEqpCatLvData(lv);
        double lvStat = 1;
        if(statType == Const.STAT_ATTACK_POWER_PER)
        {
            lvStat = lvTableData.WeaponStatValue;
        }
        else if(statType == Const.STAT_HP_PER)
        {
            lvStat = lvTableData.ArmorStatValue;
        }

        string tempCode = eqpTableData.StatCode;
        if (tempCode == statType)
        {
            // %값은 1/10000 해서 반환
            StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
            if (statInfoData.IsPer)
            {
                return eqpTableData.StatValue*lvStat * 0.0001f;
            }
            else
            {
                return eqpTableData.StatValue*lvStat;
            }
        }

        return 0;
    }

    public string GetEqpStatString(string eqpUnique, int lv, string statType)
    {
        double eqpStat = GetEqpStat(eqpUnique, lv, statType);
        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
        string statText = LocaleMgr.GetString(statInfoData.NameCode);
        if (statInfoData.IsPer)
        {
            statText += " + " + JoyUtil.GetDecimalNum(eqpStat*100, 0) + "%";
        }
        else
        {
            statText += " + " + JoyUtil.GetDecimalNum(eqpStat, 0);
        }
        
        return statText;
    }

    // 지정한 장비의 레벨별 보유효과 반환
    public double GetEqpInventoryStat(string eqpUnique, int lv, string statType)
    {
        EqpTableData eqpTableData = GameMgr.Table.GetEqpTableData(eqpUnique);

        if (eqpTableData == null)
        {
            return 0;
        }

        CommonLvTableData lvTableData = GameMgr.Table.GetEqpCatLvData(lv);
        double lvStat = 1;
        if(statType == Const.STAT_ATTACK_POWER_PER)
        {
            lvStat = lvTableData.WeaponInventoryStatValue;
        }
        else if(statType == Const.STAT_HP_PER)
        {
            lvStat = lvTableData.ArmorInventoryStatValue;
        }

        string tempCode = eqpTableData.InventoryStatCode;
        if (tempCode == statType)
        {
            // %값은 1/10000 해서 반환
            StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
            if (statInfoData.IsPer)
            {
                return eqpTableData.InventoryStatValue*lvStat * 0.0001f;
            }
            else
            {
                return eqpTableData.InventoryStatValue*lvStat;
            }
        }

        return 0;
    }

    public string GetEqpInventoryStatString(string eqpUnique, int lv, string statType)
    {
        double eqpStat = GetEqpInventoryStat(eqpUnique, lv, statType);
        EqpData eqpData = GetEqpData(eqpUnique);
        if(eqpData != null)
        {
            eqpStat += GetEqpAwakeningStatValue(eqpData);
        }
        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
        string statText = LocaleMgr.GetString(statInfoData.NameCode);
        if (statInfoData.IsPer)
        {
            statText += " + " + JoyUtil.GetDecimalNum(eqpStat*100, 0) + "%";
        }
        else
        {
            statText += " + " + JoyUtil.GetDecimalNum(eqpStat, 0);
        }
        
        return statText;
    }
    public string GetEqpInventoryStatAllString(string eqpType)
    {
        double eqpStat = GetEqpInventoryStat(eqpType);
        string statType = GetEqpInventoryStatCode(eqpType);
        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
        string statText = LocaleMgr.GetString(statInfoData.NameCode);
        if (statInfoData.IsPer)
        {
            statText += " + " + JoyUtil.GetDecimalNum(eqpStat*100) + "%";
        }
        else
        {
            statText += " + " + JoyUtil.GetDecimalNum(eqpStat);
        }
        
        return statText;
    }
    

    // 장비 보유 효과
    public float GetEqpInventoryStatAll(string eqpUnique, int lv, string statType)
    {
        float statValue = 0;

        foreach (var item in _eqpDataDic)
        {
            
        }


        return statValue;
    }

    // 주인공 지정한 장비 착용하기
    public bool SetEqp(string eqpType, EqpData eqpData)
    {
        string unique = "";
        if (eqpData != null)
        {
            if (eqpData.Table.EqpType != eqpType)
            {
                return false;
            }
            unique = eqpData.eqpUnique;
        }

        meleeEqpDic[eqpType] = eqpData;
        switch (eqpType)
        {
            case Const.EQP_WEAPON:
                DataMgr.Instance.GameData.meleeWeaponEqpUnique = unique; break;
            case Const.EQP_HELMET:
                DataMgr.Instance.GameData.meleeHelmetEqpUnique = unique; break;
            case Const.EQP_ARMOR:
                DataMgr.Instance.GameData.meleeArmorEqpUnique = unique; break;
            case Const.EQP_SHOES:
                DataMgr.Instance.GameData.meleeShoesEqpUnique = unique; break;
            case Const.EQP_RING:
                DataMgr.Instance.GameData.meleeRingEqpUnique = unique; break;
            case Const.EQP_NECKLACE:
                DataMgr.Instance.GameData.meleeNeckEqpUnique = unique; break;
        }

        onUpdateEqpData?.Invoke();
        GameMgr.Unit.CheckChangeBattlePower();

        return true;
    }

    // 지정한 부위의 장비데이터 반환
    public EqpData GetNowEqpData(string eqpType)
    {
        return meleeEqpDic[eqpType];

    }

    // 지정한 부위의 장비코드 반환
    public string GetNowEqpUnique(string eqpType)
    {
        if (meleeEqpDic[eqpType] != null)
        {
            return meleeEqpDic[eqpType].eqpUnique;
        }

        return "";
    }

    public AccData GetNowAccData(string accType)
    {
        if(_accEqpDic.ContainsKey(accType))
        {
            return _accEqpDic[accType];
        }
        return null;
    }
    public long GetNowAccUnique(string accType)
    {
        if (_accEqpDic[accType] != null)
        {
            return _accEqpDic[accType].uniqueKey;
        }

        return 0;
    }

    // 주인공 지정한 장비 착용하기
    public bool SetEqpAcc(AccData accData)
    {
        long unique = 0;
        string eqpType = accData.Table.EqpType;
        string skin = "";
        if (accData != null)
        {
            unique = accData.uniqueKey;
            skin = accData.Table.EqpUnique;
        }
        else
        {
            return false;
        }

        _accEqpDic[eqpType] = accData;
        switch (eqpType)
        {
            case Const.EQP_HAT:
                DataMgr.Instance.GameData.accHatEqpUnique = unique; 
                DataMgr.Instance.GameData.playerHatSkin = skin;
                break;
            case Const.EQP_TOP:
                DataMgr.Instance.GameData.accTopEqpUnique = unique; 
                DataMgr.Instance.GameData.playerTopSkin = skin;
                break;
        }
        

        onUpdateEqpData?.Invoke();
        GameMgr.Unit.CheckChangeBattlePower();

        return true;
    }

    public bool UneqpAcc(AccData accData)
    {
        string eqpType = accData.Table.EqpType;

        _accEqpDic[eqpType] = null;
        switch (eqpType)
        {
            case Const.EQP_HAT:
                DataMgr.Instance.GameData.accHatEqpUnique = 0; 
                DataMgr.Instance.GameData.playerHatSkin = "HAT";
                break;
            case Const.EQP_TOP:
                DataMgr.Instance.GameData.accTopEqpUnique = 0; 
                DataMgr.Instance.GameData.playerTopSkin = "TOP";
                break;
        }

        onUpdateEqpData?.Invoke();
        GameMgr.Unit.CheckChangeBattlePower();

        return true;
    }





    // 주인공 착용한 장비의 능력치 합산 반환
    public double GetPlayerEqpStat(string statType)
    {
        Dictionary<string, EqpData> tempEqpDic = new Dictionary<string, EqpData>();

        foreach (var item in meleeEqpDic)
        {
            tempEqpDic.Add(item.Key, item.Value);
        }

        if (_tempEqpData != null)
        {
            string eqpType = _tempEqpData.Table.EqpType;
            tempEqpDic[eqpType] = _tempEqpData;
        }

        double value = 0;
        if (tempEqpDic != null)
        {
            for (var i = 0; i < EQP_TYPE_LIST.Length; ++i)
            {
                EqpData tempEqpData = tempEqpDic[EQP_TYPE_LIST[i]];
                if (tempEqpData != null)
                {
                    value += GetEqpStat(tempEqpData.eqpUnique, tempEqpData.lv, statType);
                }
            }
        }

        return value;
    }

    // 강화 효과
    public double GetEqpInventoryStat(string eqpType)
    {
        double value = 0;

        var eqpDataForType = _eqpDataDic.Where(x => x.Value.Table.EqpType == eqpType).Select(x => x.Value);  // 같은 타입의 장비 데이터 찾기

        foreach (var item in eqpDataForType)
        {
            value += GetEqpInventoryStat(item.eqpUnique, item.lv, item.Table.StatCode);
            // 장비 각성에 의한 보유효과도 합산
            // value += GetEqpAwakeningStatValue(item);
        }
        return value;
    }
    // 각성 효과
    public double GetEqpEvolInventoryStat(string eqpType)
    {
        double value = 0;

        var eqpDataForType = _eqpDataDic.Where(x => x.Value.Table.EqpType == eqpType).Select(x => x.Value);  // 같은 타입의 장비 데이터 찾기

        foreach (var item in eqpDataForType)
        {
            // value += GetEqpInventoryStat(item.eqpUnique, item.lv, item.Table.StatCode);
            // 장비 각성에 의한 보유효과도 합산
            value += GetEqpAwakeningStatValue(item);
        }
        return value;
    }

    public string GetEqpInventoryStatCode(string eqpType)
    {
        EqpTableData eqpTableData = GameMgr.Table.GetEqpTypeTableList(eqpType)[0];
        return eqpTableData.StatCode;
    }

    public bool IsLastEqp(EqpData eqpData)
    {
        List<EqpTableData> list = GameMgr.Table.GetEqpTypeTableList(eqpData.Table.EqpType);
        return list[^1].EqpUnique == eqpData.eqpUnique;
    }


    public bool IsUpgradeEqpAll(string eqpType)
    {
        var eqpDataForType = _eqpDataDic.Where(x => x.Value.Table.EqpType == eqpType).Select(x => x.Value);  // 같은 타입의 장비 데이터 찾기

        foreach (var item in eqpDataForType)
        {
            bool isUpgrade = IsUpgradeEqp(item);
            if(isUpgrade)
            {
                return true;
            }
        }
        return false;
    }
    public bool IsUpgradeEqp(EqpData eqpData)
    {
        if(eqpData.lv >= GameMgr.Table.GetCommonMaxLv() && IsLastEqp(eqpData))
        {
            return false;
        }

        int price = GameMgr.Table.GetLvupNeedPiece(eqpData.lv);
        int ableOwn = eqpData.own;
        if (ableOwn < price)
        {
            return false;
        }
        return true;
    }

    public List<UpgradeData> UpgradeEqpAll(string eqpType)
    {
        List<UpgradeData> upgradeDataList = new();

        var eqpDataForType = _eqpDataDic.Where(x => x.Value.Table.EqpType == eqpType).Select(x => x.Value).OrderBy(x => x.Table.Index);  // 같은 타입의 장비 데이터 찾기
        
        bool isOpenProduct = false;
        bool isOpenProduct2 = false;
        foreach (var item in eqpDataForType)
        {
            UpgradeData tempUpgradeData = UpgradeEqpOne(item, false);
            if(tempUpgradeData != null)
            {
                upgradeDataList.Add(tempUpgradeData);
                if(!isOpenProduct)
                {
                    isOpenProduct = GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_EQP_A, item.lv, item.eqpUnique);
                }
                if(!isOpenProduct2)
                {
                    isOpenProduct2 = GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_EQP_LEVEL, item.lv, item.eqpUnique);
                }
            }
        }

        onUpdateEqpData?.Invoke();
        GameMgr.Unit.CheckChangeBattlePower();
        DataMgr.Instance.onUpdateAlbum?.Invoke();
        GameMgr.Quest.IncQuestCount(Const.QUEST_ANY_UPGRADE_EQP, 1);

        return upgradeDataList;
    }


    public UpgradeData UpgradeEqpOne(EqpData eqpData, bool isUpdate = true)
    {
        UpgradeData upgradeData = new();
        upgradeData.type = Const.TYPE_EQP;
        upgradeData.uniqueKey = eqpData.eqpUnique;
        upgradeData.beforeLv = eqpData.lv;
        ActUpgrade(eqpData);
        upgradeData.afterLv = eqpData.lv;

        if(upgradeData.beforeLv == upgradeData.afterLv)
        {
            return null;
        }

        if(isUpdate)
        {
            onUpdateEqpData?.Invoke();
            GameMgr.Unit.CheckChangeBattlePower();
            DataMgr.Instance.onUpdateAlbum?.Invoke();
            GameMgr.Quest.IncQuestCount(Const.QUEST_ANY_UPGRADE_EQP, 1);
            GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_EQP_A, eqpData.lv, eqpData.eqpUnique);
            GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_EQP_LEVEL, eqpData.lv, eqpData.eqpUnique);
        }

        return upgradeData;
    }
    public void ActUpgrade(EqpData eqpData)
    {
        int price = GameMgr.Table.GetLvupNeedPiece(eqpData.lv);
        int ableOwn = eqpData.own;
        if (ableOwn < price)
        {
            return;
        }
        if (eqpData.lv >= GameMgr.Table.GetCommonMaxLv())
        {
            eqpData.lv = GameMgr.Table.GetCommonMaxLv();
            return;
        }
        eqpData.own -= price;
        eqpData.lv++;
        GameMgr.Quest.IncQuestCount(Const.QUEST_EQP_LV_UP, 1);

        ActUpgrade(eqpData);  // 가능한 레벨까지 업그레이드
    }


    // 지정한 장비를 상위 장비로 합성하기
    public MergeData MergeEqpOne(EqpData eqpData, bool isUpdate = true)
    {
        if(eqpData.lv < GameMgr.Table.GetCommonMaxLv())
        {
            return null;
        }

        MergeData mergeData = new();
        mergeData.type = Const.TYPE_EQP;
        mergeData.uniqueKey = eqpData.eqpUnique;
        int price = GameMgr.Table.GetLvupNeedPiece(eqpData.lv);
        int ableOwn = eqpData.own;
        if (ableOwn < price)
        {
            return null;
        }
        int cnt = Mathf.FloorToInt((float)(ableOwn) / (float)price);

        string nextEqpUnique = "";
        List<EqpTableData> list = GameMgr.Table.GetEqpTypeTableList(eqpData.Table.EqpType);
        for (int i = 0; i < list.Count - 1; ++i)
        {    // 마지막 무기는 상위장비 없음
            if (list[i].EqpUnique == eqpData.eqpUnique)
            {
                nextEqpUnique = list[i + 1].EqpUnique;
                break;
            }
        }
        if (nextEqpUnique != "")
        {
            if (cnt > 0)
            {
                mergeData.mergeUniqueKey = nextEqpUnique;
                mergeData.mergeCount = cnt;

                eqpData.own -= cnt * price;
                GameMgr.Eqp.AddEqp(nextEqpUnique, cnt);
                if(isUpdate)
                {
                    onUpdateEqpData?.Invoke();
                    GameMgr.Unit.CheckChangeBattlePower();
                }
                return mergeData;
            }
            else
            {
                return null;
            }
        }
        return null;
    }

    // 일괄 합성
    public List<MergeData> MergeEqpAll(string eqpType)
    {
        List<MergeData> mergeDataList = new();

        var eqpDataForType = _eqpDataDic.Where(x => x.Value.Table.EqpType == eqpType).Select(x => x.Value).OrderBy(x => x.Table.Index);  // 같은 타입의 장비 데이터 찾기
        
        foreach (var item in eqpDataForType)
        {
            MergeData tempMergeData = MergeEqpOne(item, false);
            if(tempMergeData != null)
            {
                mergeDataList.Add(tempMergeData);
            }
        }

        onUpdateEqpData?.Invoke();
        GameMgr.Unit.CheckChangeBattlePower();

        return mergeDataList;
    }



    ////의상 관련 로직
    //의상 데이터
    // 지정한 장비정보 반환
    public AccData GetAccData(long uniqueKey)
    {
        if (_accDataDic.ContainsKey(uniqueKey))
        {
            return _accDataDic[uniqueKey];
        }
        else
        {
            return null;
        }
    }

    // 보유장비 전체 반환
    public Dictionary<long, AccData> GetAllAccData()
    {
        return _accDataDic;
    }
    public List<AccData> GetAllAccDataList(string type = "")
    { 
        List<AccData> tempList = new();
        foreach (var item in _accDataDic)
        {
            if(type != "")
            {
                if(type == item.Value.Table.EqpType)
                {
                    tempList.Add(item.Value);
                }
            }
            else
            {
                tempList.Add(item.Value);
            }
        }
        tempList = tempList.OrderByDescending(x => x.GetGradeNum()).ToList();
        tempList = tempList.OrderByDescending(x => x.uniqueKey == GetEqpAccUnique(type)).ToList();
        return tempList;
    }
    public List<AccData> GetAllAccDataList(int grade, string type = "")
    { 
        List<AccData> tempList = GetAllAccDataList(type);
        List<AccData> newList = new();
        for (int i = 0; i < tempList.Count; i++)
        {
            if(tempList[i].GetGradeNum() == grade)
            {
                newList.Add(tempList[i]);
            }
        }

        return newList;
    }

    public bool IsNewAccForList(string type)
    {
        List<AccData> hatDataList = GetAllAccDataList(type);
        bool isNewHat = false;
        for (int i = 0; i < hatDataList.Count; i++)
        {
            if(hatDataList[i].IsNew())
            {
                isNewHat = true;
                break;
            }
        }
        return isNewHat;
    }

    public long GetEqpAccUnique(string type)
    {
        long eqpUnique = 0;
        switch (type)
        {
            case Const.EQP_HAT:
                eqpUnique = DataMgr.Instance.GameData.accHatEqpUnique; 
                break;
            case Const.EQP_TOP:
                eqpUnique = DataMgr.Instance.GameData.accTopEqpUnique;
                break;
        }
        return eqpUnique;
    }

    // 장착 중인 의상 리스트에서 제거
    public void RemoveEqpAccDataForList(List<AccData> accDatas, string type)
    {
        long eqpUnique = GetEqpAccUnique(type);

        for (int i = 0; i < accDatas.Count; i++)
        {
            if(accDatas[i].uniqueKey == eqpUnique)
            {
                accDatas.Remove(accDatas[i]);
                break;
            }
        }
    }

    // 잠금 의상 리스트에서 제거
    public void RemoveLockAccDataForList(List<AccData> accDatas)
    {
        for (int i = 0; i < accDatas.Count; i++)
        {
            if(accDatas[i].isLock)
            {
                accDatas.Remove(accDatas[i]);
                i--;
            }
        }
    }

    // 최대 등급 달성 의상 리스트에서 제거
    public void RemoveMaxAccDataForList(List<AccData> accDatas)
    {
        AccGradeTableData accGradeTableData = GameMgr.Table.GetAccLastGradeTableData();
        int maxGrade = (int)accGradeTableData.Grade;
        for (int i = 0; i < accDatas.Count; i++)
        {
            if(accDatas[i].GetGradeNum() >= maxGrade)
            {
                accDatas.Remove(accDatas[i]);
                i--;
            }
        }
    }

    // 지정한 번호의 장비 획득
    public List<AccData> AddAcc(string eqpUnique, int count = 1, int evol = 0)
    {
        List<AccData> accList = new();
        for (int i = 0; i < count; i++)
        {
            long uniqueKey = GetNewAccKey();

            if (!_accDataDic.ContainsKey(uniqueKey))
            {
                _accDataDic[uniqueKey] = new AccData(eqpUnique, uniqueKey);
                for (int j = 0; j < evol; j++)
                {
                    _accDataDic[uniqueKey].EvolUp();
                }

                accList.Add(_accDataDic[uniqueKey]);

                GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_ACC, _accDataDic[uniqueKey].GetGradeNum());
                if(_accDataDic[uniqueKey].Table.EqpType == Const.EQP_HAT)
                {
                    GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_HAT, _accDataDic[uniqueKey].GetGradeNum());
                }
                else if(_accDataDic[uniqueKey].Table.EqpType == Const.EQP_TOP)
                {
                    GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_TOP, _accDataDic[uniqueKey].GetGradeNum());
                }
            }
        }
        return accList;
    }
    public void RemoveAcc(long uniqueKey)
    {
        _accDataDic.Remove(uniqueKey);
    }
    public void EvolAcc(long uniqueKey)
    {
        _accDataDic[uniqueKey].EvolUp();

        GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_ACC, _accDataDic[uniqueKey].GetGradeNum());
        if(_accDataDic[uniqueKey].Table.EqpType == Const.EQP_HAT)
        {
            GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_HAT, _accDataDic[uniqueKey].GetGradeNum());
        }
        else if(_accDataDic[uniqueKey].Table.EqpType == Const.EQP_TOP)
        {
            GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_TOP, _accDataDic[uniqueKey].GetGradeNum());
        }
    }

    public long GetNewAccKey()
    {
        DataMgr.Instance.GameData.accUniqueKey++;
        return DataMgr.Instance.GameData.accUniqueKey;
    }

    public void ResetAccData()
    {
        _accDataDic.Clear();
        DataMgr.Instance.GameData.accHatEqpUnique = 0;
        DataMgr.Instance.GameData.accTopEqpUnique = 0;
        _accEqpDic[Const.EQP_HAT] = GetAccData(DataMgr.Instance.GameData.accHatEqpUnique);
        _accEqpDic[Const.EQP_TOP] = GetAccData(DataMgr.Instance.GameData.accTopEqpUnique);
    }


    // 의상 세공 옵션 선택
    public string RandomAccStat(List<string> awakenStatType)
    {
        // List<string> statList = GameMgr.Table.GetAccStatList();
        List<AccStatData> statList = GameMgr.Table.GetAccStatDupleList();

        AccStatData randomStat = statList[UnityEngine.Random.Range(0, statList.Count)];
        if(IsAccStatDuplicate(awakenStatType, randomStat))
        {
            return RandomAccStat(awakenStatType);
        }
        else
        {
            return randomStat.statType;
        }
    }
    public bool IsAccStatDuplicate(List<string> awakenStatType, AccStatData randomStat)
    {
        for (int i = 0; i < awakenStatType.Count; i++)
        {
            if(awakenStatType[i] == randomStat.statType && randomStat.duplicate == "N")  // 스탯이 같으면서  중복 허용이 안되는 스탯일 경우  중복으로 체크
            {
                return true;
            }
        }
        return false;
    }





    // 장난감 관련 로직


    public ToyData GetToyData(string code)
    {
        if(_toyDataDic.ContainsKey(code))
        {
            return _toyDataDic[code];
        }
        return null;
    }

    public Dictionary<string, ToyData> GetToyDataDic()
    {
        return _toyDataDic;
    }

    public List<ToyData> GetNowToyDatas()
    {
        List<ToyData> list = new();
        for (int i = 0; i < DataMgr.Instance.GameData.toyEqpCodeList.Count; i++)
        {
            list.Add(GetToyData(DataMgr.Instance.GameData.toyEqpCodeList[i]));
        }
        return list;
    }

    public void AddToyData(string eqpUnique)
    {
        if (!_toyDataDic.ContainsKey(eqpUnique))
        {
            _toyDataDic[eqpUnique] = new ToyData();
            _toyDataDic[eqpUnique].toyUnique = eqpUnique;
            _toyDataDic[eqpUnique].lv = 1;
        }
    }


    public void SetToyEqpCode(string code)
    {
        for (int i = 0; i < DataMgr.Instance.GameData.toyEqpCodeList.Count; i++)
        {
            if(DataMgr.Instance.GameData.toyEqpCodeList[i] == "")
            {
                DataMgr.Instance.GameData.toyEqpCodeList[i] = code;
                onUpdateToyData?.Invoke();
                break;
            }
        }
    }

    public void SetToyEqpCode(string beforeCode, string afterCode)
    {
        for (int i = 0; i < DataMgr.Instance.GameData.toyEqpCodeList.Count; i++)
        {
            if(DataMgr.Instance.GameData.toyEqpCodeList[i] == beforeCode)
            {
                DataMgr.Instance.GameData.toyEqpCodeList[i] = afterCode;
                onUpdateToyData?.Invoke();
                break;
            }
        }
    }

    public void SetToyUnEqpCode(string code)
    {
        for (int i = 0; i < DataMgr.Instance.GameData.toyEqpCodeList.Count; i++)
        {
            if(DataMgr.Instance.GameData.toyEqpCodeList[i] == code)
            {
                DataMgr.Instance.GameData.toyEqpCodeList[i] = "";
                onUpdateToyData?.Invoke();
                break;
            }
        }
    }

    public bool IsToyEqped(string code)
    {
        for (int i = 0; i < DataMgr.Instance.GameData.toyEqpCodeList.Count; i++)
        {
            if(DataMgr.Instance.GameData.toyEqpCodeList[i] == code)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsAbleToyEqp()
    {
        bool isAbleEqp = false;
        for (int i = 0; i < DataMgr.Instance.GameData.toyEqpCodeList.Count; i++)
        {
            if(DataMgr.Instance.GameData.toyEqpCodeList[i] == "")
            {
                isAbleEqp = true;
                break;
            }
        }
        return isAbleEqp;
    }

    public bool IsBuyToyAll()
    {
        bool isBuy = false;
        List<ToyTableData> toyTableDatas = GameMgr.Table.GetToyTableDatas("TOY");
        for (int i = 0; i < toyTableDatas.Count; i++)
        {
            if(IsBuyToy(toyTableDatas[i]))
            {
                isBuy = true;
                break;
            }
        }
        return isBuy;
    }

    public bool IsBuyToy(ToyTableData toyTableData)
    {
        ToyData toyData = GetToyData(toyTableData.EqpUnique);
        int lv = 0;
        if(toyData != null)
        {
            lv = toyData.lv;
        }

        CommonLvTableData lvData = GameMgr.Table.GetToyLvTable(lv+1);
        for (int i = 0; i < toyTableData.PaymentTypeList.Count; i++)
        {
            double needCount = Math.Round(toyTableData.PaymentValueList[i]*lvData.NeedItemCountList[i]); 
            double myCount = DataMgr.Instance.GetPayment(toyTableData.PaymentTypeList[i]);
            if(needCount > myCount)
            {
                return false;
            }
        }

        return true;
    }

    public void PayToyPrice(ToyTableData toyTableData)
    {
        ToyData toyData = GetToyData(toyTableData.EqpUnique);
        int lv = 0;
        if(toyData != null)
        {
            lv = toyData.lv;
        }

        CommonLvTableData lvData = GameMgr.Table.GetToyLvTable(lv+1);
        for (int i = 0; i < toyTableData.PaymentTypeList.Count; i++)
        {
            double needCount = Math.Round(toyTableData.PaymentValueList[i]*lvData.NeedItemCountList[i]); 
            // DataMgr.PaymentData paymentData = DataMgr.Instance.GetPaymentData(toyTableData.PaymentTypeList[i], needCount);
            DataMgr.Instance.IncPayment(toyTableData.PaymentTypeList[i], -needCount);
        }

        // 장난감 별 경험치 배율 계산
        double exp = Math.Round(lvData.Exp*toyTableData.ExpMul);
        GameMgr.Pasture.IncToyFactoryExp(exp);
    }

    public void MakeToy(ToyTableData toyTableData)
    {
        PayToyPrice(toyTableData);

        AddToyData(toyTableData.EqpUnique);
        DataMgr.Instance.SaveData(true);
        onUpdateToyData?.Invoke();
    }
    public void UpgradeToy(ToyTableData toyTableData)
    {
        PayToyPrice(toyTableData);

        ToyData toyData = GetToyData(toyTableData.EqpUnique);
        toyData.lv++;
        DataMgr.Instance.SaveData(true);

        onUpdateToyData?.Invoke();
    }




    // 장난감 재료 대기열 로직
    public List<ToyMaterialProductData> GetToyMaterialProductList()
    {
        return DataMgr.Instance.GameData.toyMaterialProductDatas;
    }
    
    public void SetToyMaterial(ToyTableData toyTableData)
    {
        if(IsAbleSetToyMaterial())
        {
            int productTime = 0;
            for (int i = 0; i < DataMgr.Instance.GameData.toyMaterialProductDatas.Count; i++)
            {
                if(i == 0)
                {
                    productTime = DataMgr.Instance.GameData.toyMaterialProductDatas[i].productTime;
                }
                productTime += DataMgr.Instance.GameData.toyMaterialProductDatas[i].needTime;
            }

            if(productTime == 0)
            {
                productTime = TimeMgr.Instance.GetSvrNowT();
            }

            ToyMaterialProductData data = new()
            {
                UniqueKey = toyTableData.EqpUnique,
                productTime = productTime,
                needTime = toyTableData.ProductTime,
                paymentType = toyTableData.EqpUnique,
                paymentValue = toyTableData.ProductValue
            };

            DataMgr.Instance.GameData.toyMaterialProductDatas.Add(data);
        }
    }

    public bool IsAbleSetToyMaterial()
    {
        return DataMgr.Instance.GameData.toyMaterialProductDatas.Count < Const.TOY_MATERIAL_WAIT_COUNT;
    }

    public void GetCompleteToyMaterial()
    {
        List<ToyMaterialProductData> datas = GetToyMaterialProductList();
        int nowTime = TimeMgr.Instance.GetSvrNowT();

        List<DataMgr.PaymentData> paymentDatas = new();

        for (int i = 0; i < datas.Count; i++)
        {
            if(datas[i].productTime + datas[i].needTime < nowTime)
            {
                DataMgr.PaymentData paymentData = DataMgr.Instance.GetPaymentData(datas[i].paymentType, datas[i].paymentValue);   
                paymentDatas.AddRange(DataMgr.Instance.IncPaymentData(paymentData));

                datas.RemoveAt(0);
                i--;
            }
        }

        if(paymentDatas.Count > 0)
        {
            PopupMgr.Instance.ShowPopup(PopupMgr.PopupReward, paymentDatas);
            GameMgr.Eqp.onUpdateToyData?.Invoke();

            DataMgr.Instance.SaveData(true);
        }
    }

    public void ActAccelTimeToyMaterial(int time)
    {
        List<ToyMaterialProductData> datas = GetToyMaterialProductList();
        for (int i = 0; i < datas.Count; i++)
        {
            datas[i].productTime -= time;
        }
    }
    
    public bool IsAllToymaterialComplete()
    {
        List<ToyMaterialProductData> datas = GetToyMaterialProductList();

        if(datas.Count <= 0)
        {
            return false;
        }

        int nowTime = TimeMgr.Instance.GetSvrNowT();
        bool isComplete = true;

        for (int i = 0; i < datas.Count; i++)
        {
            int productTime = datas[i].productTime;
            int needTime = datas[i].needTime;
            int completeTime = productTime + needTime;
            if(nowTime < completeTime)  // 완료 안됨!
            {
                isComplete = false;
            }
        }

        return isComplete;
    }



    public double GetToyEqpStat(string statType, ToyTableData toyTableData = null)
    {
        double value = 0;

        ToyData toyData = null;
        int lv = 1;
        if(toyTableData != null)
        {
            toyData = GetToyData(toyTableData.EqpUnique);
        }
        if(toyData != null)
        {
            lv = toyData.lv;
        }

        CommonLvTableData lvData = GameMgr.Table.GetToyLvTable(lv);

        for (int i = 0; i < toyTableData.StatCodeList.Count; i++)
        {
            if(toyTableData.StatCodeList[i] == statType)
            {
                value += lvData.StatValueList[i]*toyTableData.StatValueList[i];
            }
        }

        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
        if(statInfoData == null)
        {
            return 0;
        }
        if (statInfoData.IsPer)
        {
            return value * 0.0001f;
        }
        else
        {
            return value;
        }
    }

    public double GetAllToyEqpStat(string statType)
    {
        double value = 0;

        for (int i = 0; i < DataMgr.Instance.GameData.toyEqpCodeList.Count; i++)
        {
            ToyData toyData = GetToyData(DataMgr.Instance.GameData.toyEqpCodeList[i]);
            if(toyData != null)
            {
                value += GetToyEqpStat(statType, toyData.Table);
            }
        }
        return value;
    }



    public double GetToyInventoryStat(int index, ToyTableData toyTableData = null)
    {
        double value = 0;

        ToyData toyData = null;
        int lv = 1;
        if(toyTableData != null)
        {
            toyData = GetToyData(toyTableData.EqpUnique);
        }
        if(toyData != null)
        {
            lv = toyData.lv;
        }

        CommonLvTableData lvData = GameMgr.Table.GetToyLvTable(lv);

        value += lvData.InventoryStatValueList[index]*toyTableData.InventoryStatValueList[index];

        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(toyTableData.InventoryStatCodeList[index]);
        if(statInfoData == null)
        {
            return 0;
        }
        if (statInfoData.IsPer)
        {
            return value * 0.0001f;
        }
        else
        {
            return value;
        }
    }



    public double GetToyAllInventoryStat(string statType)
    {
        double value = 0;

        foreach (var item in _toyDataDic)
        {
            ToyTableData toyTableData = item.Value.Table;
            CommonLvTableData lvData = GameMgr.Table.GetToyLvTable(item.Value.lv);
            for (int i = 0; i < toyTableData.InventoryStatCodeList.Count; i++)
            {
                if(toyTableData.InventoryStatCodeList[i] == statType)
                {
                    value += lvData.InventoryStatValueList[i]*toyTableData.InventoryStatValueList[i];
                }
            }
        }

        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
        if(statInfoData == null)
        {
            return 0;
        }
        if (statInfoData.IsPer)
        {
            return value * 0.0001f;
        }
        else
        {
            return value;
        }
    }
    




    // 옷장 레벨
    // 경험치 증가. 전투중에는 레벨업되지 않음.
    public bool IncAccExp(double value)
    {
        DataMgr.Instance.GameData.wardrobeExp += value;

        // 경험치 증가와 동시에 레벨업 체크
        if (CheckLevelUp())
        {
            // 레벨업 시 연출
            CommonLvTableData lvData = GameMgr.Table.GetWardrobeLvTable(GetLv());
            if(Const.IS_BATTLE_SCENE && !Const.IS_SAVE_POWER_MODE)
            {
                // PopupMgr.Instance.ShowPopup(PopupMgr.PopupLvUp, Const.LvUpType.PLAYER, lvData);
                PopupMgr.Instance.ShowAlert(PopupMgr.AlertWardrobeLvup);
                GameMgr.Eqp.onUpdateEqpData?.Invoke();  // 정보 팝업 킨상태로 레벨업시 팝업 업데이트
            }
            GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_TYPE_COSTUME);
            return true;
        }
        else
        {
            onUpdateWardrobeExp?.Invoke();
            return false;
        }
    }

    // 옷장 레벨업 체크
    public bool CheckLevelUp(bool onlyCheckFlag = false)
    {
        bool isLvup = false;
        int lv = DataMgr.Instance.GameData.wardrobeLv;
        for (; lv < Const.MAX_WARDROBE_LEVEL; ++lv)
        {
            if (DataMgr.Instance.GameData.wardrobeExp >= GameMgr.Table.GetWardrobeLvTable(lv + 1).ExpSum)
            {
                if (onlyCheckFlag)
                {
                    return true;
                }
                else
                {
                    DataMgr.Instance.GameData.wardrobeLv++;
                    GameMgr.Quest.IncQuestCount(Const.QUEST_WARDROBE_LV, 1);

                    onUpdateWardrobeLvUp?.Invoke();
                    onUpdateWardrobeExp?.Invoke();
                    DataMgr.Instance.onUpdateBattlePower();
                    // GameMgr.Quest.IncQuestCount(Const.QUEST_PLAYER_MAKE_LEVEL, _gameData.lv);
                    // GameMgr.Quest.IncQuestCount(Const.QUEST_HERO_LEVEL_UP, 1);
                    isLvup = true;
                    // 연속레벨업이 가능한 경험치인경우 추가 체크함.
                }

            }
            else
            {
                break;
            }
        }

        return isLvup;

    }

    // 현재 경험치 표시용
    public double GetNowExp()
    {
        double prevExp = 0;
        if (DataMgr.Instance.GameData.wardrobeLv >= 2)
        {
            prevExp = GameMgr.Table.GetWardrobeLvTable(DataMgr.Instance.GameData.wardrobeLv).ExpSum;
        }

        double nowExp = DataMgr.Instance.GameData.wardrobeExp - prevExp;

        return nowExp;
    }
    // 남은 경험치 표시용
    public double GetNextExp()
    {
        double prevExp = 0;
        if (DataMgr.Instance.GameData.wardrobeLv >= 2)
        {
            prevExp = GameMgr.Table.GetWardrobeLvTable(DataMgr.Instance.GameData.wardrobeLv).ExpSum;
        }

        double nextExp = prevExp;
        if (DataMgr.Instance.GameData.wardrobeLv < Const.MAX_WARDROBE_LEVEL)
        {
            nextExp = GameMgr.Table.GetWardrobeLvTable(DataMgr.Instance.GameData.wardrobeLv + 1).ExpSum - prevExp;
        }
        return nextExp;
    }
    // 현재 경험치표시 비율 반환
    public float GetExpPer(bool over = false)
    {
        float ret = (float)((double)GetNowExp() / GetNextExp());
        if (over)
        {
            return ret;
        }
        else
        {    // 최대1로 제한
            return ret > 1 ? 1 : ret;
        }
    }

    public int GetLv()
    {
        return DataMgr.Instance.GameData.wardrobeLv;
    }



    // 옷장 레벨 보유효과 반환
    public double GetWardrobeStat(string statType)
    {
        double value = 0;

        CommonLvTableData wardrobeLvData = GameMgr.Table.GetWardrobeLvTable(GetLv());

        if (wardrobeLvData == null)
        {
            return 0;
        }

        for (int i = 0; i < wardrobeLvData.StatCodeList.Count; i++)
        {
            if(wardrobeLvData.StatCodeList[i] == statType)
            {
                value += wardrobeLvData.StatValueList[i];
            }
        }

        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
        if(statInfoData == null)
        {
            return 0;
        }
        if (statInfoData.IsPer)
        {
            return value * 0.0001f;
        }
        else
        {
            return value;
        }
    }

    // 의상 세공 효과 반환
    public double GetAccStat(string statType)
    {
        double value = 0;

        foreach (var item in _accEqpDic)
        {
            if(item.Value != null)
            {
                AccData accData = item.Value;
                for (int i = 0; i < accData.GetStatCode().Count; i++)
                {
                    if(accData.GetStatCode()[i] == statType)
                    {
                        value += accData.GetStatValue()[i];
                    }
                }
            }
        }

        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
        if(statInfoData == null)
        {
            return 0;
        }
        if (statInfoData.IsPer)
        {
            return value * 0.0001f;
        }
        else
        {
            return value;
        }
    }


    // 수집품 스탯
    public double GetSupplyStatValue(string type)
    {
        double value = 0;
        
        // SupplyData supplyData 

        foreach (var item in _supplyDataDic)
        {
            SupplyData supplyData = item.Value;
            for (int i = 0; i < supplyData.statCode.Count; i++)
            {
                if(supplyData.statCode[i] == type)
                {
                    value += supplyData.statValue[i];
                }
            }
        }

        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(type);
        if(statInfoData == null)
        {
            return 0;
        }
        else
        {
            if (statInfoData.IsPer)
            {
                return value * 0.0001f;
            }
            else
            {
                return value;
            }
        }
    }

    // 수집품 소환 후 임시 저장
    public void MakeAllSupplyEqp(List<SummonTableData> summonTableDatas, bool isAuto = false)
    {
        for (int i = 0; i < summonTableDatas.Count; i++)
        {
            EqpTableData supplyTableData = GameMgr.Table.GetSupplyTableData(summonTableDatas[i].UniqueKey);
            SupplyData supplyData = MakeSupply(supplyTableData);

            DataMgr.Instance.GameData.supplyTempDataList.Add(supplyData);
        }

        // 정렬
        DataMgr.Instance.GameData.supplyTempDataList = DataMgr.Instance.GameData.supplyTempDataList.OrderByDescending(x => x.Table.Grade).ToList();

        // CheckSupplyEqp(isAuto);
        BotUI.Instance.ShowSupplySummonEff();
    }


    // 수집품 소환 후 실행내용 체크 (자동 장착 or 자동 분해 or 장비 교체 팝업 노출)
    public void CheckSupplyEqp(bool isAuto = false)
    {
        // 자동 분해
        List<DataMgr.PaymentData> paymentDatas = new();
        bool isSell = false;
        for (int i = 0; i < DataMgr.Instance.GameData.supplyTempDataList.Count; i++)
        {
            SupplyData supplyData = DataMgr.Instance.GameData.supplyTempDataList[i];
            // 등급 확인 후 처리 (자동 장착 or 자동 분해 or 장비 교체 팝업 노출)  
            if(isAuto && (!IsSupplyFilterGrade(supplyData) || !IsSupplyFilterOption(supplyData)))  // 필터에 걸리면 자동 분해
            {
                // 자동 분해
                List<DataMgr.PaymentData> tempPayments = SellSupplyEqp(supplyData, false);
                paymentDatas.AddRange(tempPayments);
                i--;
                isSell = true;
            }

            if(DataMgr.Instance.GameData.supplyTempDataList.Count == 0)
            {
                break;
            }
        }

        if(isSell)
        {
            if (isAuto)
            {
                DataMgr.Instance.SaveData(); // 자동소환중일 때는 로컬 저장
            } else {
                DataMgr.Instance.SaveData(true);
            }
            
            onUpdateSupplyData?.Invoke();
        }

        // 수집품 임시 데이터가 있을 경우 처리
        if(DataMgr.Instance.GameData.supplyTempDataList.Count > 1)
        {
            if(Const.IS_BATTLE_SCENE && DataMgr.Instance.nowDungeon == Const.DUNGEON_STAGE && !Const.IS_EVENT_MINIGAME)
            {
                PopupMgr.Instance.ShowPopup(PopupMgr.PopupSuppliesItemList);
            }
            else
            {
                BotUI.Instance.isAutoSummon = false;
            }
            onUpdateSupplyData?.Invoke();
        }
        else if(DataMgr.Instance.GameData.supplyTempDataList.Count > 0)
        {
            if(Const.IS_BATTLE_SCENE && DataMgr.Instance.nowDungeon == Const.DUNGEON_STAGE && !Const.IS_EVENT_MINIGAME && !PopupMgr.Instance.IsActive())
            {
                SupplyData nowEqpData = GetSupplyEqp(DataMgr.Instance.GameData.supplyTempDataList[0].Table.EqpType);
                PopupMgr.Instance.ShowPopup(PopupMgr.PopupSuppliesItem, nowEqpData, DataMgr.Instance.GameData.supplyTempDataList[0]);
            }
            else
            {
                BotUI.Instance.isAutoSummon = false;
            }
            onUpdateSupplyData?.Invoke();
        }
        else  // 전부 분해되어 데이터가 없을 경우 
        {
            if(isAuto)
            {
                // onUpdateSupplyData?.Invoke();
                // BotUI.Instance.StartAutoSummonDelay(2);  //StartAutoSummon();    
                BotUI.Instance.StartAutoSummon();
                // 분해 연출
                BotUI.Instance.ShowSellEff(paymentDatas);
            }
        }
    }

    public double GetNewSupplyKey()
    {
        DataMgr.Instance.GameData.supplyUniqueKey++;
        return DataMgr.Instance.GameData.supplyUniqueKey;
    }

    // 수집품 데이터 생성
    public SupplyData MakeSupply(EqpTableData eqpTableData)
    {
        double uniqueKey = GetNewSupplyKey();
        // CommonLvTableData lvTableData = GameMgr.Table.GetSupplyLvData(DataMgr.Instance.GetSupplyLv());
        CommonLvTableData lvTableData = GameMgr.Table.GetCharLevelTableData(DataMgr.Instance.GetLv()); 
        int lv = lvTableData.GetLv();

        SupplyData supplyData = new();
        // 유니크 키, 장비 코드 적용
        supplyData.uniqueKey = uniqueKey;
        supplyData.eqpCode = eqpTableData.EqpUnique;
        supplyData.lv = lv;
        // 스탯 적용
        AddSupplyStat(supplyData);        

        return supplyData;
    }
    // 수집품 스탯 생성
    public void AddSupplyStat(SupplyData supplyData)
    {
        CommonLvTableData lvStatTableData = GameMgr.Table.GetSupplyStatLvData(supplyData.lv);
        // 기본 공격 스탯
        double attackStat = lvStatTableData.GetStatRandomValue(Const.STAT_ATTACK_POWER)*supplyData.Table.GetStatValue(Const.STAT_ATTACK_POWER);
        supplyData.AddStat(Const.STAT_ATTACK_POWER, attackStat);
        // 기본 HP 스탯
        double hpStat = lvStatTableData.GetStatRandomValue(Const.STAT_HP)*supplyData.Table.GetStatValue(Const.STAT_HP);
        supplyData.AddStat(Const.STAT_HP, hpStat);

        // 특수 스탯
        for (int i = 0; i < supplyData.Table.SpecialCount; i++)
        {
            SupplyRandomStatTableData statData = GameMgr.Table.GetSupplyRandomStat(supplyData.Table.Grade);
            bool isSame = false;
            for (int j = 0; j < supplyData.statCode.Count; j++)
            {
                if(supplyData.statCode[j] == statData.StatCode)
                {
                    isSame = true;
                }
            }
            if(isSame && statData.Duplicate=="N")
            {
                i--;
            }
            else
            {
                supplyData.AddStat(statData.StatCode, statData.GetStatValue());
            }
            // supplyData.AddStat(Const.STAT_HP, hpStat);
        }
    }
    
    public void AddSupply(SupplyData supplyData)
    {

    }
    public void AddSupply(string uniqueKey)
    {
        EqpTableData supplyTableData = GameMgr.Table.GetSupplyTableData(uniqueKey);
        SupplyData supplyData = MakeSupply(supplyTableData);
    }

    public void EqpSupply(SupplyData supplyData)
    {
        string eqpType = supplyData.Table.EqpType;
        if(!_supplyDataDic.ContainsKey(eqpType))
        {
            _supplyDataDic[eqpType] = supplyData;
            DataMgr.Instance.GameData.supplyTempDataList.Remove(supplyData);
        }
        else
        {
            DataMgr.Instance.GameData.supplyTempDataList.Add(_supplyDataDic[eqpType]);
            DataMgr.Instance.GameData.supplyTempDataList.Remove(supplyData);
            _supplyDataDic[eqpType] = supplyData;
        }
        onUpdateSupplyData?.Invoke();
        GameMgr.Unit.CheckChangeBattlePower();
    }
    public SupplyData GetSupplyEqp(string eqpType)
    {
        if(_supplyDataDic.ContainsKey(eqpType))
        {
            return _supplyDataDic[eqpType];
        }
        return null;
    }

    public List<DataMgr.PaymentData> SellSupplyEqp(SupplyData supplyData, bool isUpdate = true)
    {
        CommonLvTableData lvStatTableData = GameMgr.Table.GetSupplyStatLvData(supplyData.lv);
        List<DataMgr.PaymentData> basePaymentDatas = lvStatTableData.GetSellPaymentDataList();
        List<DataMgr.PaymentData> paymentDatas = new();
        for (int i = 0; i < basePaymentDatas.Count; i++)
        {
            float multiValue = supplyData.Table.GetSellValue(basePaymentDatas[i].type.ToString());
            DataMgr.PaymentData paymentData;
            if(basePaymentDatas[i].type == Const.PaymentType.GOLD)
            {
                MonLevelData monLevelData = GameMgr.Table.GetMonLevelData(BattleMgr.Instance.GetNowMonsterLv());
                paymentData = DataMgr.Instance.GetPaymentData(basePaymentDatas[i].type, monLevelData.Gold*multiValue);
            }
            else
            {
                paymentData = DataMgr.Instance.GetPaymentData(basePaymentDatas[i].type, basePaymentDatas[i].value*multiValue);
            }
            paymentDatas.Add(paymentData);
        }

        // 판매 후 재화증가
        DataMgr.Instance.IncPaymentDatas(paymentDatas);
        // 판매 후 아이템 제거
        RemoveTempSupplyData(supplyData);
        GameMgr.Quest.IncQuestCount(Const.QUEST_SELL_SUPPLY, 1);
        
        if(isUpdate)
        {
            DataMgr.Instance.SaveData(true);
            onUpdateSupplyData?.Invoke();
        }
        return paymentDatas;
    }

    public void SellAllSupply()
    {
        List<DataMgr.PaymentData> paymentDatas = new();
        for (int i = 0; i < DataMgr.Instance.GameData.supplyTempDataList.Count; i++)
        {
            if(DataMgr.Instance.GameData.supplyTempDataList.Count > 0)
            {
                List<DataMgr.PaymentData> tempPayments = SellSupplyEqp(DataMgr.Instance.GameData.supplyTempDataList[i], false);
                paymentDatas.AddRange(tempPayments);
                i--;
            }
        }

        DataMgr.Instance.SaveData(true);
        onUpdateSupplyData?.Invoke();

        if(BotUI.Instance.isAutoSummon)
        {
            BotUI.Instance.StartAutoSummon();
        }
        // 분해 연출
        BotUI.Instance.ShowSellEff(paymentDatas);
    }

    public List<SupplyData> GetTempSupplyDataList()
    {
        return DataMgr.Instance.GameData.supplyTempDataList;
    }
    public int GetTempSupplyDataCount()
    {
        return DataMgr.Instance.GameData.supplyTempDataList.Count;
    }

    public void RemoveTempSupplyData(SupplyData supplyData)
    {
        for (int i = 0; i < DataMgr.Instance.GameData.supplyTempDataList.Count; i++)
        {
            if(supplyData.uniqueKey == DataMgr.Instance.GameData.supplyTempDataList[i].uniqueKey)
            {
                DataMgr.Instance.GameData.supplyTempDataList.Remove(DataMgr.Instance.GameData.supplyTempDataList[i]);
            }
        }
    }
    public void AddTempSupplyData()
    {

    }

    // 수집품 필터 등급
    public Const.GradeTypeCapital GetSupplyFilterGrade()
    {
        Const.GradeTypeCapital grade = JoyUtil.StringToEnum<Const.GradeTypeCapital>(GetSupplyFilterGradeStr());
        return grade;
    }
    public string GetSupplyFilterGradeStr()
    {
        return DataMgr.Instance.GameData.supplyGradeFilter;
    }
    public void SetSupplyFilterGradeStr(string grade)
    {
        DataMgr.Instance.GameData.supplyGradeFilter = grade;
    }
    public bool IsSupplyFilterGrade(SupplyData supplyData)
    {
        if(supplyData.Table.Grade >= GetSupplyFilterGrade())  // 필터 보다 등급이 낮으면 자동 분해
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 수집품 필터 스탯 타입
    public string GetSupplyFilterStatOne1()
    {
        return DataMgr.Instance.GameData.supplyOneFilter1;
    } 
    public void SetSupplyFilterStatOne1(string statType)
    {
        DataMgr.Instance.GameData.supplyOneFilter1 = statType;
    }
    public string GetSupplyFilterStatOne2()
    {
        return DataMgr.Instance.GameData.supplyOneFilter2;
    } 
    public void SetSupplyFilterStatOne2(string statType)
    {
        DataMgr.Instance.GameData.supplyOneFilter2 = statType;
    }
    public string GetSupplyFilterStatTwo1()
    {
        return DataMgr.Instance.GameData.supplyTwoFilter1;
    } 
    public void SetSupplyFilterStatTwo1(string statType)
    {
        DataMgr.Instance.GameData.supplyTwoFilter1 = statType;
    }
    public string GetSupplyFilterStatTwo2()
    {
        return DataMgr.Instance.GameData.supplyTwoFilter2;
    } 
    public void SetSupplyFilterStatTwo2(string statType)
    {
        DataMgr.Instance.GameData.supplyTwoFilter2 = statType;
    }

    public bool IsSupplyOneFilter()
    {
        return DataMgr.Instance.GameData.isSupplyOneFilter;
    }
    public void SetSupplyOneFilter(bool isActive)
    {
        DataMgr.Instance.GameData.isSupplyOneFilter = isActive;
    }
    public bool IsSupplyTwoFilter()
    {
        return DataMgr.Instance.GameData.isSupplyTwoFilter;
    }
    public void SetSupplyTwoFilter(bool isActive)
    {
        DataMgr.Instance.GameData.isSupplyTwoFilter = isActive;
    }

    public int GetSupplyFilterSummonCountIndex()
    {
        return DataMgr.Instance.GameData.supplySummonCountFilter;
    }
    public void SetSupplyFilterSummonCountIndex(int index)
    {
        DataMgr.Instance.GameData.supplySummonCountFilter = index;
    }
    public int GetSupplyFilterSummonCount()
    {
        SupplyAutoSummonTableData data = GameMgr.Table.GetSupplyAutoSummonData(GetSupplyFilterSummonCountIndex());
        if(data == null)
        {
            return 1;  // 소환 필터 데이터가 없을 경우 1 반환
        }
        else
        {
            return data.AutoCount;
        }
    }


    public bool IsSupplyFilterOption(SupplyData supplyData)
    {
        bool isStatOne1 = false;
        bool isStatOne2 = false;
        bool isStatTwo1 = false;
        bool isStatTwo2 = false;


        for (int i = 0; i < supplyData.statCode.Count; i++)
        {
            if(supplyData.statCode[i] == GetSupplyFilterStatOne1() || GetSupplyFilterStatOne1() == "")
            {
                isStatOne1 = true;
            }
            if(supplyData.statCode[i] == GetSupplyFilterStatOne2() || GetSupplyFilterStatOne2() == "")
            {
                isStatOne2 = true;
            }
            if(supplyData.statCode[i] == GetSupplyFilterStatTwo1() || GetSupplyFilterStatTwo1() == "")
            {
                isStatTwo1 = true;
            }
            if(supplyData.statCode[i] == GetSupplyFilterStatTwo2() || GetSupplyFilterStatTwo2() == "")
            {
                isStatTwo2 = true;
            }
        }

        bool isStatOne = !IsSupplyOneFilter() || (isStatOne1 || isStatOne2);  // 필터가 꺼져있으면 통과, 스탯 두개중 하나라도 있을 경우 통과
        bool isStatTwo = !IsSupplyTwoFilter() || (isStatTwo1 || isStatTwo2);  // 필터가 꺼져있으면 통과, 스탯 두개중 하나라도 있을 경우 통과
        
        if(isStatOne && isStatTwo)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    ////////// 장비 각성 ///////////
    
    // 장비의 각성 스탯 코드 반환
    public string GetEqpAwakeningStatCode(EqpData eqpData)
    {
        EvolInfoTableData evolInfoTableData = GameMgr.Table.GetEvolInfoTableData(eqpData.eqpUnique);
        return evolInfoTableData.statCodeList[0];

    }

    public EvolStatInfoTableData GetEqpEvolStatTableData(EqpData eqpData, int plusAwaken = 0)
    {
        EvolInfoTableData evolInfoTableData = GameMgr.Table.GetEvolInfoTableData(eqpData.eqpUnique);
        int awaken = eqpData.awaken;

        return GameMgr.Table.GetEvolStatInfoTableData(evolInfoTableData.EvolType, awaken + plusAwaken);
    }

    // 장비의 각성 효과 스탯 반환
    public double GetEqpAwakeningStatValue(EqpData eqpData, int plusAwaken = 0)
    {
        EvolInfoTableData evolInfoTableData = GameMgr.Table.GetEvolInfoTableData(eqpData.eqpUnique);
        int awaken = eqpData.awaken;


        EvolStatInfoTableData evolStatInfoTableData = GameMgr.Table.GetEvolStatInfoTableData(evolInfoTableData.EvolType, awaken + plusAwaken);
        if (evolStatInfoTableData == null)
        {
            return 0;
        } else {

            return evolInfoTableData.statMulValueList[0] * evolStatInfoTableData.statValueList[0];
        }
    }

    // 장비의 각성 강화 비용 반환
    public double GetEqpAwakeningPriceValue(EqpData eqpData)
    {
        EvolInfoTableData evolInfoTableData = GameMgr.Table.GetEvolInfoTableData(eqpData.eqpUnique);
        int awaken = eqpData.awaken;


        EvolStatInfoTableData evolStatInfoTableData = GameMgr.Table.GetEvolStatInfoTableData(evolInfoTableData.EvolType, awaken + 1);
        if (evolStatInfoTableData == null)
        {
            return 99999999999999999; // 여기까지 안와야 함
        }
        else
        {
            return evolInfoTableData.NeedMulValue * evolStatInfoTableData.NeedValue;
        }
    }


    // 장비 각성 가능 여부 체크
    public bool IsAwakeEqp(EqpData eqpData)
    {
        EvolStatInfoTableData evolInfoTableData = GameMgr.Eqp.GetEqpEvolStatTableData(eqpData, 1);

        if(evolInfoTableData == null)
        {
            return false;
        }

        bool lvPass = true;
        // if (eqpData.lv < evolInfoTableData.LimitLv)
        // {
        //     lvPass = false;
        // }
        // 각성 가능 여부.
        double needValue = GameMgr.Eqp.GetEqpAwakeningPriceValue(eqpData);
        if (DataMgr.Instance.GetPayment(Const.PaymentType.EQP_AWAKE_STONE) >= needValue && lvPass)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 모든 장비 각성 가능 여부 체크
    public bool IsAwakeEqpAll(string eqpType)
    {
        var eqpDataForType = _eqpDataDic.Where(x => x.Value.Table.EqpType == eqpType).Select(x => x.Value);  // 같은 타입의 장비 데이터 찾기

        foreach (var item in eqpDataForType)
        {
            bool isUpgrade = IsAwakeEqp(item);
            if(isUpgrade)
            {
                return true;
            }
        }
        return false;
    }





    // 룬 정보
    public RuneData GetRuneData(string code)
    {
        if(_runeDataDic.ContainsKey(code))
        {
            return _runeDataDic[code];
        }
        return null;
    }

    public List<RuneData> GetNowRuneDatas()
    {
        List<RuneData> list = new();
        for (int i = 0; i < DataMgr.Instance.GameData.runeEqpCodeList.Count; i++)
        {
            RuneData runeData = GetRuneData(DataMgr.Instance.GameData.runeEqpCodeList[i]);
            if(runeData != null)
            {
                list.Add(runeData);
            }
        }
        return list;
    }

    public void AddRuneData(string runeCode, int count = 1)
    {
        if (!_runeDataDic.ContainsKey(runeCode))
        {
            _runeDataDic[runeCode] = new RuneData(runeCode);
            _runeDataDic[runeCode].lv = 1;

            _runeDataDic[runeCode].AddOwn(count -1);
        }
        else
        {
            _runeDataDic[runeCode].AddOwn(count);
        }
        onUpdateRuneData?.Invoke();
    }



    public int GetMaxMyRuneLv()
    {
        int maxLv = 0;
        foreach (var item in _runeDataDic)
        {
            if(item.Value.lv > maxLv)
            {
                maxLv = item.Value.lv;
            }
        }
        return maxLv;
    }

    

    public List<string> GetRuneDeckList()
    {
        return DataMgr.Instance.GameData.runeEqpCodeList;
    }
    public string GetRuneDeckList(int index)
    {
        return DataMgr.Instance.GameData.runeEqpCodeList[index];
    }
    public void SetRuneDeckList(int index, string charCode)
    {
        DataMgr.Instance.GameData.runeEqpCodeList[index] = charCode;
        GameMgr.Unit.CheckChangeBattlePower();

        onUpdateRuneData?.Invoke();
    }
    public bool IsSelectedRune(string charCode)
    {
        List<string> deckList = GetRuneDeckList();
        for (int i = 0; i < deckList.Count; i++)
        {
            if(deckList[i] == charCode)
            {
                return true;
            }
        }
        return false;
    }

    // public int IsSelectedRune(int index)
    // {
    //     if(DataMgr.Instance.GameData.friendDeckList[index] == "")
    //     {
    //         return 0;
    //     }
    //     else
    //     {
    //         return 1;
    //     }
    // }

    public bool IsUpgradeRuneAll()
    {
        foreach (var item in _runeDataDic)
        {
            bool isUpgrade = IsUpgradeRune(item.Value);
            if(isUpgrade)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsUpgradeRune(RuneData runeData)
    {
        if(runeData.lv >= Const.MAX_RUNE_LV)
        {
            return false;
        }

        if(!IsUpgradeRuneForEvol(runeData))
        {
            return false;
        }

        int price = GameMgr.Table.GetRuneLvupNeedPiece(runeData.lv);
        int ableOwn = runeData.own;
        if (ableOwn < price)
        {
            return false;
        }
        return true;
    }

    public bool IsUpgradeRuneForEvol(RuneData runeData)
    {
        if(runeData.evol < Const.MAX_RUNE_EVOL)
        {
            CommonLvTableData runeEvolTable = GameMgr.Table.GetRuneEvolTable(runeData.evol+1);
            if(runeData.lv >= runeEvolTable.LimitLv)
            {
                return false;
            }
        }
        return true;
    }


    public List<UpgradeData> UpgradeRuneAll()
    {
        List<UpgradeData> upgradeDataList = new();

        var runeDataForType = _runeDataDic.OrderBy(x => x.Value.Table.Index);  // 같은 타입의 장비 데이터 찾기
        
        // bool isOpenProduct = false;
        // bool isOpenProduct2 = false;
        foreach (var item in runeDataForType)
        {
            UpgradeData tempUpgradeData = UpgradeRuneOne(item.Value, false);
            if(tempUpgradeData != null)
            {
                upgradeDataList.Add(tempUpgradeData);
                // if(!isOpenProduct)
                // {
                //     isOpenProduct = GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_EQP_A, item.lv, item.eqpUnique);
                // }
                // if(!isOpenProduct2)
                // {
                //     isOpenProduct2 = GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_EQP_LEVEL, item.lv, item.eqpUnique);
                // }
            }
        }

        onUpdateRuneData?.Invoke();
        GameMgr.Unit.CheckChangeBattlePower();
        // DataMgr.Instance.onUpdateAlbum?.Invoke();
        // GameMgr.Quest.IncQuestCount(Const.QUEST_ANY_UPGRADE_EQP, 1);

        return upgradeDataList;
    }


    public UpgradeData UpgradeRuneOne(RuneData runeData, bool isUpdate = true)
    {
        UpgradeData upgradeData = new();
        upgradeData.type = Const.TYPE_RUNE;
        upgradeData.uniqueKey = runeData.runeCode;
        upgradeData.beforeLv = runeData.lv;
        ActRuneUpgrade(runeData);
        upgradeData.afterLv = runeData.lv;

        if(upgradeData.beforeLv == upgradeData.afterLv)
        {
            return null;
        }

        if(isUpdate)
        {
            onUpdateRuneData?.Invoke();
            GameMgr.Unit.CheckChangeBattlePower();
            onUpdateRuneData?.Invoke();
            // GameMgr.Quest.IncQuestCount(Const.QUEST_ANY_UPGRADE_EQP, 1);
            // GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_FIRST_EQP_A, eqpData.lv, eqpData.eqpUnique);
            // GameMgr.Shop.CheckOpenTimeLimitProduct(ShopMgr.LIMIT_EQP_LEVEL, eqpData.lv, eqpData.eqpUnique);
        }

        return upgradeData;
    }


    public void ActRuneUpgrade(RuneData runeData)
    {
        int price = GameMgr.Table.GetRuneLvupNeedPiece(runeData.lv);
        int ableOwn = runeData.own;
        if (ableOwn < price)
        {
            return;
        }
        if (runeData.lv >= Const.MAX_RUNE_LV)
        {
            runeData.lv = Const.MAX_RUNE_LV;
            return;
        }
        if(!IsUpgradeRuneForEvol(runeData))
        {
            return;
        }

        runeData.own -= price;
        runeData.lv++;
        // GameMgr.Quest.IncQuestCount(Const.QUEST_EQP_LV_UP, 1);

        // onUpdateRuneData?.Invoke();

        ActRuneUpgrade(runeData);  // 가능한 레벨까지 업그레이드
    }

    public void EvolRune()
    {

    }

    public bool IsRuneEvol(RuneData runeData)
    {
        if(runeData.evol >= Const.MAX_RUNE_EVOL)
        {
            return false;  // 최대 진화면 안됨
        }

        CommonLvTableData runeEvolData = GameMgr.Table.GetRuneEvolTable(runeData.evol+1);
        if(runeData.lv >= runeEvolData.LimitLv)  // 제한 레벨 이상일때 진화 가능
        {
            return true;
        }

        return false;
    }

    // 진화 스탯값 반환. 
    public double GetRuneEvolStatValue(RuneData runeData, int plusEvol = 0)
    {
        int evol = runeData.evol+plusEvol;
        if(evol >= Const.MAX_RUNE_EVOL)
        {
            evol = Const.MAX_RUNE_EVOL;
        }
        CommonLvTableData runeEvolData = GameMgr.Table.GetRuneEvolTable(evol);
        return runeEvolData.StatValueList[0]*0.0001f;
    }
    // 진화 보유 효과 반환
    public double GetRuneEvolInventoryStatValue(RuneData runeData, int plusEvol = 0)
    {
        int evol = runeData.evol+plusEvol;
        if(evol >= Const.MAX_RUNE_EVOL)
        {
            evol = Const.MAX_RUNE_EVOL;
        }
        CommonLvTableData runeEvolData = GameMgr.Table.GetRuneEvolTable(evol);
        return runeEvolData.InventoryStatValueList[0]*0.0001f;
    }






    public double GetRuneEqpStat(string statType, RuneTableData runeTableData = null)
    {
        double value = 0;

        RuneData runeData = null;
        int lv = 1;
        if(runeTableData != null)
        {
            runeData = GetRuneData(runeTableData.RuneCode);
        }
        if(runeData != null)
        {
            lv = runeData.lv;
        }

        CommonLvTableData lvData = GameMgr.Table.GetRuneLvTable(lv);

        for (int i = 0; i < lvData.StatCodeList.Count; i++)
        {
            if(lvData.StatCodeList[i] == statType)
            {
                value += lvData.StatValueList[i];  //*runeTableData.StatValueList[i];
            }
        }

        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
        if(statInfoData == null)
        {
            return 0;
        }
        if (statInfoData.IsPer)
        {
            return value * 0.0001f;
        }
        else
        {
            return value;
        }
    }

    public double GetAllRuneEqpStat(string statType)
    {
        double value = 0;

        for (int i = 0; i < DataMgr.Instance.GameData.runeEqpCodeList.Count; i++)
        {
            RuneData runeData = GetRuneData(DataMgr.Instance.GameData.runeEqpCodeList[i]);
            if(runeData != null)
            {
                value += GetRuneEqpStat(statType, runeData.Table);
                value += GetRuneEvolStatValue(runeData);
                // value += GetRuneEvolInventoryStatValue(runeData);
            }
        }
        return value;
    }



    public double GetRuneInventoryStat(string statType, RuneTableData runeTableData = null)
    {
        double value = 0;

        RuneData runeData = null;
        int lv = 1;
        if(runeTableData != null)
        {
            runeData = GetRuneData(runeTableData.RuneCode);
        }
        if(runeData != null)
        {
            lv = runeData.lv;
        }

        CommonLvTableData lvData = GameMgr.Table.GetRuneLvTable(lv);

        for (int i = 0; i < lvData.InventoryStatCodeList.Count; i++)
        {
            if(lvData.InventoryStatCodeList[i] == statType)
            {
                value += lvData.InventoryStatValueList[i];  //*runeTableData.InventoryStatValueList[i];
            }
        }

        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
        if(statInfoData == null)
        {
            return 0;
        }
        if (statInfoData.IsPer)
        {
            return value * 0.0001f;
        }
        else
        {
            return value;
        }
    }

    public double GetRuneEvolAllInventoryStat(string statType)
    {
        double value = 0;

        foreach (var item in _runeDataDic)
        {
            RuneTableData runeTableData = item.Value.Table;
            CommonLvTableData lvData = GameMgr.Table.GetRuneLvTable(item.Value.lv);
            for (int i = 0; i < lvData.InventoryStatCodeList.Count; i++)
            {
                if(lvData.InventoryStatCodeList[i] == statType)
                {
                    value += lvData.InventoryStatValueList[i];  //*runeTableData.InventoryStatValueList[i];
                }
            }
        }

        StatInfoData statInfoData = GameMgr.Table.GetStatInfoData(statType);
        if(statInfoData == null)
        {
            return 0;
        }
        if (statInfoData.IsPer)
        {
            return value * 0.0001f;
        }
        else
        {
            return value;
        }
    }


    public double GetRuneAllInventoryStat(string statType)
    {
        double value = 0;

        foreach (var item in _runeDataDic)
        {
            RuneData runeData = item.Value;
            value += GetRuneInventoryStat(statType, runeData.Table);
            value += GetRuneEvolInventoryStatValue(runeData);
        }
        return value;
    }



}



