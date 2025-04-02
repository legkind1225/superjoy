using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TileObj : MonoBehaviour
{
    public SpriteRenderer tileSprite;
    public SpriteRenderer tileUnderSprite;
    public SpriteRenderer backMask;
    public SpriteRenderer nowStateTileSprite;

    //테스트용 좌표 표기
    public TextMeshPro offsetPosText;
    public TextMeshPro cubePosText;

    public int _tileNum = 0;
    public int _blindValue = 0;

    public Vector2Int _position;


    /// <summary>
    /// Sum of G and H.
    /// </summary>
    public int F => g + h;

    /// <summary>
    /// Cost from start tile to this tile.
    /// </summary>
    public int g;

    /// <summary>
    /// Estimated cost from this tile to destination tile.
    /// </summary>
    public int h;

    /// <summary>
    /// Tile's coordinates.
    /// </summary>
    public Vector3Int position;
	
    /// <summary>
    /// References to all adjacent tiles.
    /// </summary>
    public List<TileObj> adjacentTiles = new List<TileObj>();  //인접 타일
    public List<TileObj> movePossiableTiles = new List<TileObj>(); // 이동 가능한 방향 타일
    public List<TileObj> threeTiles = new List<TileObj>(); // 3타일 건물용 인접 타일
	
    /// <summary>
    /// If true - Tile is an obstacle impossible to pass.
    /// </summary>
    public bool isObstacle;

    private bool isLightUp = false;

    private const int BASE_TILE_HEIGHT = 286;

    public TileMapObjTableData _tileObjData = null;

    // 타일 타입 (bg, blind)
    public Const.TileType tiletype = Const.TileType.BG;
    // 타일맵 타입 (월드맵, 토벌맵)
    Const.TileMapType _tileMapType = Const.TileMapType.BATTLEMAP;

    public void SetTileMapType(Const.TileMapType tileMapType) {
        _tileMapType = tileMapType;
    }

    public void SetTileSprite(int tileNum, Vector2Int pos) {
        tileSprite.sprite = GameMgr.Resource.LoadTileImage(tileNum);
        tileUnderSprite.sprite = GameMgr.Resource.LoadUnderTileImage(DataMgr.Instance.GetKingdomWorldDifficulty());
        tiletype = Const.TileType.BG;
        _tileNum = tileNum;
        _position = pos;
        GetCubeCoordinates();
        isObstacle = tileNum>=100;  //100보다 타일의 값이 크거나 같을 경우 이동 불가 타일임.  TODO : 추후 몬스터가 있을 경우도 (경로탐색시) 이동 불가 타일이 되어야함

        float spriteHeight = tileSprite.sprite.rect.height;
        tileSprite.transform.position += new Vector3(0,(spriteHeight-BASE_TILE_HEIGHT)*0.005f*0.85f,0);  // 타일 이미지 위치 조정 (위로 긴 타일의 경우 위치 조절됨)
        
        int addOrder = 0;
        if(isObstacle) { // 이동 불가 타일일 경우 위로 조금 올려줌
            tileSprite.transform.position += new Vector3(0, 0.3f, 0);
            tileUnderSprite.transform.position += new Vector3(0, 0.3f, 0);
            addOrder = 3;
        }

        // tileSprite.sortingOrder = -1;
        tileSprite.sortingOrder = _position.y*2 + addOrder;
        tileUnderSprite.sortingOrder = tileSprite.sortingOrder;
        backMask.sortingOrder = tileSprite.sortingOrder+1;
        nowStateTileSprite.sortingOrder = tileSprite.sortingOrder+3;

        nowStateTileSprite.gameObject.SetActive(false);
        nowStateTileSprite.sprite = null;
    }

    public void SetBlindTile(int value, Vector2Int pos) {
        _position = pos;

        TileObj tileObj = null;
        if(_tileMapType == Const.TileMapType.BATTLEMAP) {
            tileObj = KingdomBattleMapMgr.Instance.GetTileObj(_position.y, _position.x);
        } else if(_tileMapType == Const.TileMapType.WORLDMAP){
            tileObj = KingdomWorldMapMgr.Instance.GetTileObj(_position.y, _position.x);
        }
        // TileObj tileObj = KingdomBattleMapMgr.Instance.GetTileObj(_position.y, _position.x);
        tileSprite.sprite = tileObj.tileSprite.sprite;

        tileSprite.color = new Color(0,0,0,0.5f);

        float spriteHeight = tileSprite.sprite.rect.height;
        tileSprite.transform.position += new Vector3(0,(spriteHeight-BASE_TILE_HEIGHT)*0.005f*0.85f,0);  // 타일 이미지 위치 조정 (위로 긴 타일의 경우 위치 조절됨)
        int addOrder = 0;
        if(tileObj.isObstacle) { // 이동 불가 타일일 경우 위로 조금 올려줌
            tileSprite.transform.position += new Vector3(0, 0.3f, 0);
            addOrder = 2;
        }

        UpdateBlindTile(value);

        tiletype = Const.TileType.BLIND;
        
        GetCubeCoordinates();

        // tileSprite.sortingOrder = 0;
        tileSprite.sortingOrder = _position.y*2+2 + addOrder;
    }


    // 해당 타일맵에 타일 오브젝트 데이터 넣음
    public void SetTileObjData(TileMapObjTableData tileMapObjTableData, int nextNum = 0) {
        _tileObjData = tileMapObjTableData;
        if(nextNum > 0) {  //
            nextNum -= 1;
            for (int i = 0; i < threeTiles.Count; i++)
            {
                if(_tileObjData.ObjectType != Const.KINGDOM_BATTLE_MINE)
                {
                    threeTiles[i].SetTileObjData(tileMapObjTableData, nextNum);   
                }
            }
        }
        UpdateStateTile(tileMapObjTableData);
    }
    public void SetTileObjDataNeighbor(TileMapObjTableData tileMapObjTableData, int nextNum = 0) {
        _tileObjData = tileMapObjTableData;
        if(nextNum > 0) {  //
            nextNum -= 1;
            for (int i = 0; i < adjacentTiles.Count; i++)
            {
                adjacentTiles[i].SetTileObjData(tileMapObjTableData, nextNum);
            }
        }
        //TODO : 해당 타일맵 오브젝트 데이터로 타일 변경되는 부분 처리
        UpdateStateTile(tileMapObjTableData);
    }

    public void UpdateStateTile(TileMapObjTableData tileMapObjTableData) {
        if(tileMapObjTableData.ObjectType == Const.KINGDOM_BATTLE_PLAYER) {
            nowStateTileSprite.sprite = GameMgr.Resource.LoadKingdomImage("map_tile_clear");
            // if(isNeighborTile) {
            //     if(nowStateTileSprite.sprite != null) {
            //         nowStateTileSprite.sprite = GameMgr.Resource.LoadKingdomImage("map_tile_move");
            //     }
            // } else {
            //     nowStateTileSprite.sprite = GameMgr.Resource.LoadKingdomImage("map_tile_clear");
            //     for (int i = 0; i < adjacentTiles.Count; i++)
            //     {
            //         adjacentTiles[i].UpdateStateTile(tileMapObjTableData, true);
            //     }
            // }
        } 
        else if(tileMapObjTableData.ObjectType == Const.KINGDOM_BATTLE_MINE) 
        {
            nowStateTileSprite.sprite = GameMgr.Resource.LoadKingdomImage("map_tile_select");
        }
        else {
            bool isClear = DataMgr.Instance.IsClearKingdomBattleMap(tileMapObjTableData.MapDifficulty, tileMapObjTableData.ObjectValue);
            if(isClear) {
                nowStateTileSprite.sprite = GameMgr.Resource.LoadKingdomImage("map_tile_clear");
            } else {
                nowStateTileSprite.sprite = GameMgr.Resource.LoadKingdomImage("map_tile_boss");
            }
        }
    }
    // 토벌전 월드맵 성 클리어 주변 타일
    public void UpdateClearTile(int nextNum = 0) {
        if(_tileObjData == null) {
            if(nowStateTileSprite.sprite == null && !isObstacle) {
                nowStateTileSprite.gameObject.SetActive(true);
                nowStateTileSprite.sprite = GameMgr.Resource.LoadKingdomImage("map_tile_move");
            }
        } else {
            bool isClear = DataMgr.Instance.IsClearKingdomBattleMap(_tileObjData.MapDifficulty, _tileObjData.ObjectValue);
            if(_tileObjData.ObjectType == Const.KINGDOM_BATTLE_PLAYER) {
                isClear = true;
            }
            else if(_tileObjData.ObjectType == Const.KINGDOM_BATTLE_MINE)
            {
                isClear = false;
            }
            if(isClear && nextNum > 0) {
                nextNum--;
                for (int i = 0; i < adjacentTiles.Count; i++)
                {
                    adjacentTiles[i].UpdateClearTile(nextNum);
                }
            }
        }
    }

    // 토벌전 전투맵에서 이전 클리어 루트 보여주는 타일
    public void UpdateClearRootTile(bool isActive)
    {
        if(isActive)
        {
            nowStateTileSprite.gameObject.SetActive(true);
            nowStateTileSprite.sprite = GameMgr.Resource.LoadKingdomImage("map_tile_clear");
        }
        else 
        {
            nowStateTileSprite.gameObject.SetActive(false);
        }
    }

    // 상태 타일 활성화 여부 업데이트
    public void UpdateStateTileActive() {
        TileObj blindTile = KingdomWorldMapMgr.Instance.GetBlindTileObj(_position.y, _position.x);

        if(_tileObjData != null) {
            switch (_tileObjData.ObjectType)
            {
                case Const.KINGDOM_BATTLE_ENEMY_BIG:
                case Const.KINGDOM_BATTLE_ENEMY_GIANT:
                case Const.KINGDOM_BATTLE_MINE:
                    nowStateTileSprite.gameObject.SetActive(true);
                    break;
                default:
                     nowStateTileSprite.gameObject.SetActive(!blindTile.gameObject.activeSelf);
                    break;
            }
        }
        
        // nowStateTileSprite.gameObject.SetActive(!blindTile.gameObject.activeSelf && _tileObjData != null);
    }

    // 플레이어 타일보다 뒤에 있는 타일 처리
    public void CheckBackMask() {
        //블라인드인 곳은 제외
        TileObj blindTile = KingdomBattleMapMgr.Instance.GetBlindTileObj(_position.y, _position.x);
        if(blindTile.gameObject.activeSelf) {  
            return;
        }

        if(isObstacle) {  //장애물은 제외
            return;
        }

        Vector2Int playerPosition = KingdomBattleMapMgr.Instance.GetPlayerPosition();
        backMask.gameObject.SetActive(playerPosition.x > _position.x);

        // CheckBlindBackMask();
    }

    private void CheckBlindBackMask() {
        // 지나온 어두운 타일 강제로 밝히기 (붉은색 처리됨)
        TileObj blindTile = KingdomBattleMapMgr.Instance.GetBlindTileObj(_position.y, _position.x);
        if(backMask.gameObject.activeSelf) {
            blindTile.UpdateBlindTile(1);
        }
    }


    public void UpdateBlindTile(int value) {
        _blindValue = value;

        if(_blindValue == 0) {
            gameObject.SetActive(true);
        } else if(_blindValue == 1) {
            gameObject.SetActive(false);
        }
    }

    // 좌표계 변환
    public void GetCubeCoordinates() {
        position = AStarPathfinding.OffsetEvenQToCube(_position);


        //// 좌표 테스트용
        // offsetPosText.gameObject.SetActive(true);
        // cubePosText.gameObject.SetActive(true);
        // offsetPosText.text = "(" + _position.x + ", " + _position.y + ")";
        // cubePosText.text = "(" + position.x + ", " + position.y + ", " + position.z + ")";
    }

    // 이웃 타일 설정
    public void SetNeighborList() {
        adjacentTiles.Clear();
        if(_tileMapType == Const.TileMapType.BATTLEMAP) 
        {
            adjacentTiles = KingdomBattleMapMgr.Instance.GetNeighborTileObj(_position);
            movePossiableTiles = KingdomBattleMapMgr.Instance.GetMovePossiableTileObj(_position);
        } 
        else if(_tileMapType == Const.TileMapType.WORLDMAP)
        {
            adjacentTiles = KingdomWorldMapMgr.Instance.GetNeighborTileObj(_position);
            threeTiles = KingdomWorldMapMgr.Instance.GetThreeBuildingTileObj(_position);
        }
    }


    public void ClickBtn(TileObj startTile) {
        switch (_tileMapType)
        {
            case Const.TileMapType.BATTLEMAP:
                ActBattleMapClick(startTile);
                break;
            case Const.TileMapType.WORLDMAP:
                ActWorldMapClick(startTile);
                break;
        }

        
    }

    

    // 토벌전 월드맵에서의 타일 터치
    public void ActWorldMapClick(TileObj startTile) {
        // 광산의 경우 이동 없이 즉시 입장
        if(_tileObjData != null && _tileObjData.ObjectType == Const.KINGDOM_BATTLE_MINE)
        {
            int stageNo = DataMgr.Instance.GetChangeStageNo(_tileObjData.MapDifficulty, 0, _tileObjData.ObjectValue);
            StageData nowStageData = GameMgr.Table.GetDungeonMineInfo(stageNo);

            // 광산과 연결된 성을 클리어해야 진입 가능.
            //if (DataMgr.Instance.IsClearKingdomBattleMap(_tileObjData.MapDifficulty, _tileObjData.MineLimit))
            if (DataMgr.Instance.IsClearKingdomBattleMap(_tileObjData.MapDifficulty, nowStageData.KingdomObjectValue))
            {
                // DataMgr.Instance.isMineFromKingdomBattle = true; // 토벌전에서 광산 진입
                // DataMgr.Instance.CheckEnterNewMine(stageNo);
                DataMgr.Instance.isMineFromKingdomBattle = true; // 토벌전에서 광산 진입
                if (!DataMgr.Instance.GameData.isMineOpen)
                {
                    DataMgr.Instance.GameData.isMineOpen = true;
                    if(DataMgr.Instance.GameData.mineKey <= Const.MAX_MINE_KEY)
                        DataMgr.Instance.GameData.mineKey = Const.MAX_MINE_KEY;
                    DataMgr.Instance.SaveData();
                }
                PopupMgr.Instance.ShowPopup(PopupMgr.PopupMine, stageNo);
                SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);
            } else {
                int monsterIndex = GameMgr.Table.GetKingdomTileMapObjMaxCount(_tileObjData.MapDifficulty, nowStageData.KingdomObjectValue, Const.KINGDOM_BATTLE_MONSTER);  // 해당 토벌전맵의 마지막 몬스터 기준
                int stageNo2 = DataMgr.Instance.GetChangeStageNo(_tileObjData.MapDifficulty, nowStageData.KingdomObjectValue, monsterIndex);
                StageData stageData = GameMgr.Table.GetDungeonKingdomBattleInfo(stageNo2);

                // 진입 불가 경고 문구
                PopupMgr.Instance.ShowAlert(PopupMgr.AlertLimitMine, stageData.MonLv.ToString());  // Lv. {1} 성을 점령해야 이용할 수 있습니다.
                SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_CANCEL);
            }
            
            return;
        }

        if(!DataMgr.Instance.IsFirstKingdomBattleMap()) {  // 이미 전투중인 경우
            if(_tileObjData != null) {  // 오브젝트 데이터 있을 경우
                if(DataMgr.Instance.IsSameKingdomBattleMap(_tileObjData.MapDifficulty, _tileObjData.ObjectValue)) {
                    GameObject.FindObjectOfType<ScreenFader>().FadeOut(Const.SCENE_KINGDOM_BATTLEMAP);
                    return;
                }
            }

            int remainGiveupCount = Const.KINGDOM_BATTLE_MAX_GIVEUP - DataMgr.Instance.GetDailyKingdomBattleGiveup();
            if(remainGiveupCount <= 0) 
            {
                PopupMgr.Instance.ShowAlert(PopupMgr.AlertKingdomBattleGiveUp);
                SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_CANCEL);
                return;
            }
            string giveUpCount = remainGiveupCount.ToString() + "/" + Const.KINGDOM_BATTLE_MAX_GIVEUP.ToString();
            PopupMgr.Instance.ShowPopup(PopupMgr.PopupMsg, PopupMgr.MsgType.KINGDOM_MAP_CHANGE, giveUpCount);
            PopupMgr.Instance.GetPopup(PopupMgr.PopupMsg).GetComponent<PopupMsg>().addOkEvent(ActResetMap);
            PopupMgr.Instance.GetPopup(PopupMgr.PopupMsg).GetComponent<PopupMsg>().addCancelEvent(ActChangeCancel);
            SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_ALERT2);
            return;
        }

        if(!IsDontMove()) {  // 장애물이 아니고 이동 가능한 방향이면
            List<TileObj> movePath = AStarPathfinding.FindPath(startTile, this);
            if(movePath.Count > 0) {
                movePath.RemoveAt(0); // 출발 지점은 제거
            }
            KingdomWorldMapMgr.Instance.MovePlayer(movePath);
        } else {
            // 경고 : 이동 불가능 위치
            PopupMgr.Instance.ShowAlert(PopupMgr.AlertDontMovePosition);
            SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_TOUCH);
        }
    }

    public bool ActResetMap() {
        DataMgr.Instance.SetPlayKingdomMap(0,0);
        DataMgr.Instance.AddDailyKingdomBattleGiveup();
        DataMgr.Instance.ResetKingdomBattleMap();
        PopupMgr.Instance.ShowAlert(PopupMgr.AlertKingdomBattleGiveupComp);  // 토벌전 포기됨

        KingdomWorldMapMgr.Instance.UpdateCastleUI();
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);
        return true;
    }

    public bool ActChangeCancel() {
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_CANCEL);
        return true;
    }

    public void ActBattleMapEnter() {
        PopupMgr.Instance.ShowPopup(PopupMgr.PopupKDBattleInfo, _tileObjData);
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_BTN0);


        // // 전투맵 진입 관리
        // if(DataMgr.Instance.IsFirstKingdomBattleMap()) {  // 첫 진입 맵일 경우
        //     ActChangeEnemy();
        // } else if(DataMgr.Instance.IsSameKingdomBattleMap(DataMgr.Instance.GetKingdomWorldDifficulty(), _tileObjData.ObjectValue)) {  // 기존에 진행중인 맵인 경우
        //     // 씬 이동
        //     SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_CLEAR);
        //     GameObject.FindObjectOfType<ScreenFader>().FadeOut(Const.SCENE_KINGDOM_BATTLEMAP);
        // } else {  // 새로운 맵에 진입할 경우
        //     PopupMgr.Instance.ShowPopup(PopupMgr.PopupMsg, PopupMgr.MsgType.KINGDOM_MAP_CHANGE);
        //     PopupMgr.Instance.GetPopup(PopupMgr.PopupMsg).GetComponent<PopupMsg>().addOkEvent(ActChangeEnemy);
        //     PopupMgr.Instance.GetPopup(PopupMgr.PopupMsg).GetComponent<PopupMsg>().addCancelEvent(ActChangeCancel);
        // }
    }

    // public bool ActChangeEnemy() {
    //     SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_CLEAR);
    //     DataMgr.Instance.SetKingdomWorldSelectIndex(_tileObjData.ObjectValue);
    //     // 씬 이동
    //     GameObject.FindObjectOfType<ScreenFader>().FadeOut(Const.SCENE_KINGDOM_BATTLEMAP);
    //     return true;
    // }
    // public bool ActChangeCancel() {
    //     SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_CANCEL);
    //     return true;
    // }



    // 토벌전 배틀맵에서의 타일 터치
    public void ActBattleMapClick(TileObj startTile) {
        switch (tiletype)
        {
            case Const.TileType.BG:
                if(_position.x < startTile._position.x) {
                    // 경고 : 이전 타일로는 갈수 없음
                    PopupMgr.Instance.ShowAlert(PopupMgr.AlertDontMoveBack);
                    SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_TOUCH);
                    return;
                }
                if(!IsObstacle() && KingdomBattleMapMgr.Instance.IsMovePossiable(position-startTile.position)) {  // 장애물이 아니고 이동 가능한 방향이면
                    int difficulty = DataMgr.Instance.GetKingdomWorldDifficulty();
                    int needActionPoint = GameMgr.Table.GetKingdomMovePoint(difficulty);
                    if(KingdomBattleMapMgr.Instance.GetTargetTileMapObjData() != null) {
                        TileMapObjTableData tile = KingdomBattleMapMgr.Instance.GetTargetTileMapObjData();
                        switch (tile.ObjectType)
                        {
                            case Const.KINGDOM_BATTLE_BOX:
                                {
                                    TileMapBoxRewardTableData tileMapBoxRewardTableData = GameMgr.Table.GetKingdomTileMapBoxReward(tile.MapDifficulty, tile.MapIndex, tile.ObjectType + tile.ObjectValue);

                                    if (tileMapBoxRewardTableData.RewardType == Const.PaymentType.PRI_EQP_GROUP_1.ToString())
                                        needActionPoint += GameMgr.Table.GetKingdomRandEqpPoint(difficulty);
                                    else
                                        needActionPoint += GameMgr.Table.GetKingdomRandBoxPoint(difficulty);
                                }
                                break;
                            //case Const.KINGDOM_BATTLE_RARE_BOX:
                            //    break;
                            //case Const.KINGDOM_BATTLE_RECOVERY_WELL:
                            //    break;
                            //case Const.KINGDOM_BATTLE_RESOURCE:
                            //    break;
                            case Const.KINGDOM_BATTLE_MONSTER:
                                needActionPoint += GameMgr.Table.GetKingdomMonsterPoint(difficulty);
                                break;
                        }
                    }
                    if(DataMgr.Instance.getPayment(Const.PaymentType.KINGDOM_ACTION_POINT) >= needActionPoint) {  // 행동력 있을 때만 맵 밝히기 가능
                        bool isFull = DataMgr.Instance.getPayment(Const.PaymentType.KINGDOM_ACTION_POINT) >= DataMgr.Instance.GetAirShipMaxMagicPower();
                        
                        DataMgr.Instance.incPayment(Const.PaymentType.KINGDOM_ACTION_POINT, -needActionPoint);
                        // 토벌전 행동력 소모 퀘스트
                        GameMgr.Quest.IncQuestCount(Const.QUEST_KINGDOM_ACTIONPOINT_COUNT, needActionPoint);
                        
                        if(isFull && DataMgr.Instance.getPayment(Const.PaymentType.KINGDOM_ACTION_POINT) < DataMgr.Instance.GetAirShipMaxMagicPower()) {  // 행동력이 최대치 이상이었다가 최대치 미만으로 내려갈 경우에만
                            // 행동력 회복 시작
                            DataMgr.Instance.SetActionPointTime(TimeMgr.Instance.GetSvrNowT());
                        }
                        
                        List<TileObj> movePath = AStarPathfinding.FindPath(startTile, this);
                        KingdomBattleMapMgr.Instance.MovePlayer(movePath, needActionPoint);
                    } else {
                        // 경고 : 마법력 부족
                        PopupMgr.Instance.ShowAlertNotEnoughtPayment(Const.PaymentType.KINGDOM_ACTION_POINT);
                        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_TOUCH);

                        GameMgr.Shop.CheckOpenTimeLimitProduct("NEED_ACTION_POTION");
                        GameMgr.Shop.ShowOpenTimeLimitProduct();
                        
                        // 마법력 부족시 auto 켜져있으면 꺼줌
                        DataMgr.Instance.SetKingdomBattleAutoOff();
                        KingdomBattleMapMgr.Instance._kingdomBattleMapUI.UpdateRepeatBtn();
                    }
                } else {
                    // 경고 : 이동 불가능 위치
                    PopupMgr.Instance.ShowAlert(PopupMgr.AlertDontMovePosition);
                    SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_TOUCH);
                }
                break;
            case Const.TileType.BLIND:
                // 경고 : 이동 불가능 위치
                PopupMgr.Instance.ShowAlert(PopupMgr.AlertDontMovePosition);
                SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_TOUCH);
                break;
        }
    }

    int _nextNum = 0;
    // 타일 밝히기
    public void LightUp(TileMapObjTableData tileObjData = null, int nextNum = 0) {
        if(_nextNum >= nextNum) {
            return;
        } 
        
        _nextNum = nextNum;

        if(_tileMapType == Const.TileMapType.BATTLEMAP) {
            KingdomBattleMapMgr.Instance.LightUpBlindTileData(_position.y, _position.x);
        } else if(_tileMapType == Const.TileMapType.WORLDMAP){
            KingdomWorldMapMgr.Instance.LightUpBlindTileData(_position.y, _position.x);
            UpdateStateTileActive();
        }
        nextNum--;

        // if(_tileObjData != null && _tileObjData == tileObjData && !isLightUp) {
        //     nextNum++;
        // }
        // isLightUp = true;

        if(nextNum >= 0) {
            LightUpNeighbors(_tileObjData, nextNum);
        }
    }
    public void LightUpNeighbors(TileMapObjTableData tileObjData = null, int nextNum = 0) {
        for (int i = 0; i < adjacentTiles.Count; i++)
        {
            adjacentTiles[i].LightUp(tileObjData, nextNum);
        }
    }


    public Const.TileType GetTileType() {
        return tiletype;
    }

    public bool IsDontMove() {
        return isObstacle || IsBlindObj();   
    }

    // 장애물 체크 true면 뭔가 있는것!
    public bool IsObstacle() {
        return isObstacle || IsBlindObj() || IsExistObj(); // 
    }

    // 해당 타일 위에 오브젝트 있는지 확인
    public bool IsExistObj() {
        if(_tileMapType == Const.TileMapType.BATTLEMAP) {
            return false; //KingdomBattleMapMgr.Instance.IsExistObj(_position);
        } else if(_tileMapType == Const.TileMapType.WORLDMAP){
            return _tileObjData != null;
        }
        return false;
    }
    
    // 해당 타일 위에 블라인드 타일이 있는지 확인
    public bool IsBlindObj() {
        if(_tileMapType == Const.TileMapType.BATTLEMAP) {
            return (KingdomBattleMapMgr.Instance.GetBlindTileData(_position.y, _position.x) == 0);
        } else if(_tileMapType == Const.TileMapType.WORLDMAP){
            return (KingdomWorldMapMgr.Instance.GetBlindTileData(_position.y, _position.x) == 0);
        }
        return false;
    }
}
