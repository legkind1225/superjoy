using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using UnityEngine.EventSystems; 
using UnityEngine.SceneManagement;
using LitJson;

public class KingdomBattleMapMgr : Singleton<KingdomBattleMapMgr>
{
    public KingdomBattleMapUI _kingdomBattleMapUI;
    public Camera mainCamera;
    public Camera UICamera;
    public CameraFollow cameraFollow;

    // 프리팹
    public GameObject tileObjPrefab;
    public GameObject monsterObjPrefab;
    public GameObject itemObjPrefab;

    // 주인공 오브젝트
    public KingdomPlayerObj playerObj;

    // 주인공 선택 타일 오브젝트
    public GameObject playerTile;
    public GameObject targetArrow;
    
    // 주인공 이동 방향 오브젝트
    public GameObject wayArrow; //전체 방향 오브젝트 부모
    public List<GameObject> wayArrowList;

    
    // 생성된 타일 및 오브젝트 부모
    public Transform mapBg;
    public Transform mapBlind;
    public Transform objParent;

    // 타일 관련 벡터
    public Vector2 startPos;  //타일 시작 위치 (0,0)의 좌표
    public Vector2 cellSize;  //타일 사이즈

    // 타일 오브젝트
    private List<List<TileObj>> bgTileObjList = new List<List<TileObj>>();
    private List<List<TileObj>> blindTileObjList = new List<List<TileObj>>();
    public List<TileMonsterObj> tileMonsterObjList = new List<TileMonsterObj>();
    public List<TileBoxObj> tileBoxObjList = new List<TileBoxObj>();

    // 타일 상태 데이터
    private List<List<int>> _bgTileDataList;
    private List<List<int>> _blindTileDataList = new List<List<int>>();
    private List<TileMapObjTableData> _objectDataList = new List<TileMapObjTableData>();
    private TileMapObjTableData _playerData = new TileMapObjTableData();
    private List<TileMapObjTableData> _boxDataList = new List<TileMapObjTableData>();
    private List<TileMapObjTableData> _monsterDataList = new List<TileMapObjTableData>();

    
    // 인접 타일 확인용 리스트
    private List<Vector2Int> _neighborOddVector = new();
    private List<Vector2Int> _neighborEvenVector = new();

    // 이동 가능 방향 타일 확인용 리스트
    private List<Vector3Int> _movePossiableVector = new();
    private List<Vector2Int> _movePossiableOddVector = new();
    private List<Vector2Int> _movePossiableEvenVector = new();


    List<TileObj> _movePath = new List<TileObj>();
    TileObj _targetTileObj = null;
    


    // 맵 드래그용 임시데이터
    private Vector3 startTouchPos;      // 터치 시작 위치
    public Vector3 nowTouchPos;        // 터치 현재 위치
    private bool _isTouchMove;          // 현재 드래그로 인해 맵이 움직이고 있는지

    private bool _isTouchPossible = true;

    // 자동 진행용 데이터
    KingdomBattleMapClearData _mapClearData = new();
    // 토벌전 클리어 경로
    public List<Vector2Int> _clearRoot = new();
    // 토벌전 진행 경로 저장(진행중인 경로)
    public List<Vector2Int> _playingRoot = new();
    
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    protected override void Awake()
    {
        // base.Awake();

        //임시
        InitMap();
    }
    
    void Start()
    {
        GameMgr.Instance.SetResolution();
        UICamera.rect = mainCamera.rect;
        SoundMgr.Instance.PlayBGMSound(SoundMgr.BGM_KINGDOM_BATTLE);

        // 튜토리얼 체크
        string productTuto = $"KINGDOMBATTLE_STARTBATTLE";
        string code = "ENTER_CONTENT";
        if (GameMgr.Story.CheckKingdomTutorial(code, productTuto))
        {
            GameMgr.Story.ProcStory();
        }

    }

    public void InitMap() {
        // 육각 타일의 주변 타일 체크용 데이터 초기화
        InitTileNeighborData();
        
        // 맵 데이터 불러오기
        LoadTileData();
        LoadObjectData();
        LoadClearData();  // 맵 클리어 데이터 불러오기

        // 카메라 기본 세팅
        InitCamera();

        // 맵 BG 초기화  (바닥 타일)
        InitBgTile();

        // 맵 Blind 초기화 (어두운 타일)
        InitBlindTile();

        //맵 BG타일의 이웃 타일 업데이트
        UpdateBgTileNeighborList();
        UpdateBlindTileNeighborList();

        // 맵 Object 초기화 (보물 상자, 몬스터, 성)
        UpdatePlayer();
        UpdateMonster();
        UpdateBox();

        // 지나온 맵 초기화
        InitBgBackTile();

        InitClearRootTile();

        // UI 초기화 (KingdomBattleMapUI에서 처리함)
        InitPlayerUI();
        // InitMonsterUI();
        InitBossUI();

        // 토벌전 오브젝트 상황 UI 업데이트
        _kingdomBattleMapUI.UpdateObjCount(true);

        _isTouchPossible = true;

        //// 토벌전 진입시 마지막 스테이지 클리어 되어 있을 경우 해당 성 클리어 보상 및 임시 저장된 재화 획득
        CheckLastStageClear();

        // 카메라 포지션 초기세팅
        Vector3 pos = new Vector3(playerObj.transform.position.x, playerObj.transform.position.y + cameraFollow.CameraFixY, cameraFollow.transform.position.z);
        cameraFollow.transform.position = new Vector3(Mathf.Clamp(pos.x, cameraFollow.minPos.x, cameraFollow.maxPos.x),
            Mathf.Clamp(pos.y, cameraFollow.minPos.y, cameraFollow.maxPos.y),
            pos.z);

    }

    private void InitTileNeighborData() {

        //근접 타일 체크용 (짝수)
        _neighborOddVector.Add(new Vector2Int(1,0));
        _neighborOddVector.Add(new Vector2Int(1,-1));
        _neighborOddVector.Add(new Vector2Int(0,-1));
        _neighborOddVector.Add(new Vector2Int(-1,-1));
        _neighborOddVector.Add(new Vector2Int(-1,0));
        _neighborOddVector.Add(new Vector2Int(0,1));

        //근접 타일 체크용 (홀수)
        _neighborEvenVector.Add(new Vector2Int(1,1));
        _neighborEvenVector.Add(new Vector2Int(1,0));
        _neighborEvenVector.Add(new Vector2Int(0,-1));
        _neighborEvenVector.Add(new Vector2Int(-1,0));
        _neighborEvenVector.Add(new Vector2Int(-1,1));
        _neighborEvenVector.Add(new Vector2Int(0,1));


        //이동 가능 방향 체크용
        _movePossiableVector.Add(new Vector3Int(0,-1,1));
        _movePossiableVector.Add(new Vector3Int(1,-1,0));
        _movePossiableVector.Add(new Vector3Int(1,0,-1));
        _movePossiableVector.Add(new Vector3Int(0,1,-1));

        //이동 가능 방향 체크용 (짝수)
        _movePossiableOddVector.Add(new Vector2Int(1,0));
        _movePossiableOddVector.Add(new Vector2Int(1,-1));
        _movePossiableOddVector.Add(new Vector2Int(0,-1));
        _movePossiableOddVector.Add(new Vector2Int(0,1));

        //이동 가능 방향 체크용 (홀수)
        _movePossiableEvenVector.Add(new Vector2Int(1,1));
        _movePossiableEvenVector.Add(new Vector2Int(1,0));
        _movePossiableEvenVector.Add(new Vector2Int(0,-1));
        _movePossiableEvenVector.Add(new Vector2Int(0,1));


    }

    private void InitCamera() {
        // cameraFollow = mainCamera.GetComponent<CameraFollow>();
        //cameraFollow.SetKingdomCameraZoom(mainCamera, _bgTileDataList[0].Count*cellSize.x+startPos.x);


        // if(Const.IS_KINGDOM_BATTLE_MAP_LIGHT) {
        //     cameraFollow.moveFlag = false;
        // }

        cameraFollow.SetKingdomCameraZoom(mainCamera, _bgTileDataList[0].Count * cellSize.x + startPos.x);
        cameraFollow.moveFlag = false;

    }

    private void CreateTile(List<List<TileObj>> tileList, Transform parent, int x, int y) {
        GameObject slot = Instantiate(tileObjPrefab);
        slot.transform.SetParent(parent, false);
        if(x >= tileList.Count) {
            List<TileObj> tempList = new List<TileObj>();
            tileList.Add(tempList); 
        }
        if(y >= tileList[x].Count) {
            TileObj tempTileObj = slot.GetComponent<TileObj>();
            tileList[x].Add(tempTileObj);
        }

        //// 위가 뾰족한 육각 타일일때 좌표
        // float startX = startPos.x + ((x+1)%2)*cellSize.x*0.5f;
        // float posX = startX + cellSize.x*(y);
        // float posY = startPos.y - cellSize.y*(x);

        //// 옆이 뾰족한 육각 타일일때 좌표
        float startY = startPos.y + (y%2)*cellSize.y*0.5f;
        float posX = startPos.x + cellSize.x*(y);
        float posY = startY - cellSize.y*(x);

        tileList[x][y].transform.localPosition = new Vector3(posX, posY, 0);
    }

    private GameObject CreateObj(GameObject prefab, Transform parent) {
        GameObject slot = Instantiate(prefab);
        slot.transform.SetParent(parent, false);
        return slot;
    }



    public void LoadTileData() {
        //TODO : 난이도 별로 다른 타일맵 불러옴
        _bgTileDataList = GameMgr.Table.GetKingdomTileMapList(DataMgr.Instance.GetKingdomWorldDifficulty(), DataMgr.Instance.GetKingdomWorldSelectIndex());

        //바닥 타일 있는 위치 전부 어두운 타일 데이터 넣어줌 (0: 어두움, 1: 밝혀짐)
        //TODO : 이후 해당 값을 저장했다가 불러와야함
        if(Const.IS_KINGDOM_BATTLE_MAP_LIGHT) {
            for (int i = 0; i < _bgTileDataList.Count; i++)
            {
                List<int> tempList = new List<int>();
                for (int j = 0; j < _bgTileDataList[i].Count; j++)
                {
                    tempList.Add(1);
                }
                _blindTileDataList.Add(tempList);
            }
        } else {
            if(DataMgr.Instance.GetKingdomBattleBlindMap() != null) {
                _blindTileDataList = DataMgr.Instance.GetKingdomBattleBlindMap();
            } else {
                for (int i = 0; i < _bgTileDataList.Count; i++)
                {
                    List<int> tempList = new List<int>();
                    for (int j = 0; j < _bgTileDataList[i].Count; j++)
                    {
                        tempList.Add(0);
                    }
                    _blindTileDataList.Add(tempList);
                }
            }
        }
        
    }

    public void LoadObjectData() {
        //TODO : 난이도 별로 다른 오브젝트 데이터 불러옴
        _objectDataList =  GameMgr.Table.GetKingdomTileMapObjList(DataMgr.Instance.GetKingdomWorldDifficulty(), DataMgr.Instance.GetKingdomWorldSelectIndex());
        for (int i = 0; i < _objectDataList.Count; i++)
        {
            switch (_objectDataList[i].ObjectType)
            {
                case Const.KINGDOM_BATTLE_PLAYER:
                    _playerData = _objectDataList[i];
                    break;
                case Const.KINGDOM_BATTLE_MONSTER:
                    _monsterDataList.Add(_objectDataList[i]);
                    break;
                case Const.KINGDOM_BATTLE_BOX:
                case Const.KINGDOM_BATTLE_RARE_BOX:
                case Const.KINGDOM_BATTLE_RECOVERY_WELL:
                case Const.KINGDOM_BATTLE_RESOURCE:
                    _boxDataList.Add(_objectDataList[i]);
                    break;
                default:
                    break;
            }
        }
    }

    public void LoadClearData()
    {
        _mapClearData = DataMgr.Instance.GetKingdomBattleClearData(DataMgr.Instance.GetKingdomWorldDifficulty(), DataMgr.Instance.GetKingdomWorldSelectIndex());
        _clearRoot = _mapClearData.clearRoot;
        _playingRoot = _mapClearData.playingRoot;
    }

    public bool IsClear()
    {
        return _clearRoot.Count > 0;
    }

    public void InitBgTile() {
        for (int i = 0; i < _bgTileDataList.Count; i++)
        {
            if(i >= bgTileObjList.Count) {
                List<TileObj> tempList = new List<TileObj>();
                bgTileObjList.Add(tempList); 
            }
            for (int j = 0; j < _bgTileDataList[i].Count; j++)
            {
                if(j >= bgTileObjList[i].Count) {
                    CreateTile(bgTileObjList, mapBg, i, j);
                }
            }
        }

        // 타일 데이터 넣기 및 기본 세팅
        for (int i = 0; i < bgTileObjList.Count; i++)
        {
            if(i < _bgTileDataList.Count) {
                for (int j = 0; j < bgTileObjList[i].Count; j++)
                {
                    if(j < _bgTileDataList[i].Count) {
                        bgTileObjList[i][j].SetTileMapType(Const.TileMapType.BATTLEMAP);
                        bgTileObjList[i][j].SetTileSprite(_bgTileDataList[i][j], new Vector2Int(j,i));
                    }
                }
            }
        }
    }

    public void InitBgBackTile() {
        // 타일 데이터 넣기 및 기본 세팅
        for (int i = 0; i < bgTileObjList.Count; i++)
        {
            if(i < _bgTileDataList.Count) {
                for (int j = 0; j < bgTileObjList[i].Count; j++)
                {
                    if(j < _bgTileDataList[i].Count) {
                        bgTileObjList[i][j].CheckBackMask();
                    }
                }
            }
        }
    }

    // 클리어 루트 타일 표기
    public void InitClearRootTile()
    {
        if(IsSameRoot())
        {
            for (int i = 0; i < _clearRoot.Count; i++)
            {
                GetTileObj(_clearRoot[i].y, _clearRoot[i].x).UpdateClearRootTile(true);
            }
        }
    }
    public void RemoveClearRootTile()
    {
        for (int i = 0; i < _clearRoot.Count; i++)
        {
            GetTileObj(_clearRoot[i].y, _clearRoot[i].x).UpdateClearRootTile(false);
        }
    }

    public void UpdateBgTileNeighborList() {
        //타일의 인접 타일 찾아주기
        for (int i = 0; i < bgTileObjList.Count; i++)
        {
            if(i < _bgTileDataList.Count) {
                for (int j = 0; j < bgTileObjList[i].Count; j++)
                {
                    if(j < _bgTileDataList[i].Count) {
                        bgTileObjList[i][j].SetNeighborList();
                    }
                }
            }
        }
    }
    public void UpdateBlindTileNeighborList() {
        //타일의 인접 타일 찾아주기
        for (int i = 0; i < blindTileObjList.Count; i++)
        {
            if(i < _blindTileDataList.Count) {
                for (int j = 0; j < blindTileObjList[i].Count; j++)
                {
                    if(j < _blindTileDataList[i].Count) {
                        blindTileObjList[i][j].SetNeighborList();
                    }
                }
            }
        }
    }

    public void InitBlindTile() {
        for (int i = 0; i < _blindTileDataList.Count; i++)
        {
            if(i >= blindTileObjList.Count) {
                List<TileObj> tempList = new List<TileObj>();
                blindTileObjList.Add(tempList); 
            }
            for (int j = 0; j < _blindTileDataList[i].Count; j++)
            {
                if(j >= blindTileObjList[i].Count) {
                    CreateTile(blindTileObjList, mapBlind, i, j);
                }
            }
        }

        for (int i = 0; i < blindTileObjList.Count; i++)
        {
            if(i < _blindTileDataList.Count) {
                for (int j = 0; j < blindTileObjList[i].Count; j++)
                {
                    if(j < _blindTileDataList[i].Count) {
                        blindTileObjList[i][j].SetTileMapType(Const.TileMapType.BATTLEMAP);
                        blindTileObjList[i][j].SetBlindTile(_blindTileDataList[i][j], new Vector2Int(j,i));
                    }
                }
            }
        }
    }

    public void UpdatePlayer() {
        Vector2Int playerPos = DataMgr.Instance.GetPlayerPos();
        playerObj.SetTileMapType(Const.TileMapType.BATTLEMAP);
        if(playerPos != new Vector2Int(0,0)) {
            playerObj.SetStartPos(playerPos.x, playerPos.y);
        } else {
            playerObj.SetStartPos(_playerData.PositionX, _playerData.PositionY);
        }
        //플레이어의 위치 데이터가 없을경우 테이블의 플레이어 위치값 받아옴
        // playerObj.transform.position = GetTilePosition(_playerData.PositionY, _playerData.PositionX);
        playerObj.gameObject.SetActive(true);
        playerObj.InitObj(_playerData);
        // playerObj.UpdateCharSpine();
        UpdatePlayerTile();
    }

    public void UpdatePlayerTile() {
        playerTile.SetActive(true);
        wayArrow.SetActive(true);
        // 플레이어 위치 표기 타일 위치 변경
        playerTile.transform.position = playerObj.transform.position;
        wayArrow.transform.position = playerObj.transform.position;

        // 플레이어 위치 기준으로 타일 오픈
        TileObj tileObj = GetTileObj(playerObj.PosY, playerObj.PosX);
        LightUpNeighbors(tileObj);

        // 갈 수 있는 방향 표기
        for (int j = 0; j < wayArrowList.Count; j++)
        {
            bool isMove = false;
            for (int i = 0; i < tileObj.movePossiableTiles.Count; i++)
            {
                if(!tileObj.movePossiableTiles[i].isObstacle) {  // 장애물이 아닌지 확인
                    if(tileObj.movePossiableTiles[i].position-tileObj.position == _movePossiableVector[j]) {  // 이웃 타일 해당 방향이 맞는지 확인
                        isMove = true;
                    }
                }
            }
            wayArrowList[j].SetActive(isMove);
        }
    }

    public void LightUpNeighbors(TileObj tileObj) {
        tileObj.LightUp(null, 2);
        // tileObj.LightUpNeighbors();
    }

    public void UpdateMonster() {
        for (int i = 0; i < _monsterDataList.Count; i++)
        {
            if(i >= tileMonsterObjList.Count) {
                GameObject tempObj = CreateObj(monsterObjPrefab, objParent);
                tileMonsterObjList.Add(tempObj.GetComponent<TileMonsterObj>());
            }
        }

        for (int i = 0; i < tileMonsterObjList.Count; i++)
        {
            if(i < _monsterDataList.Count) {
                tileMonsterObjList[i].SetMonster(_monsterDataList[i]);
                tileMonsterObjList[i].transform.position = GetTilePosition(_monsterDataList[i].PositionY, _monsterDataList[i].PositionX);
            }
        }
    }


    public void UpdateBox() {
        for (int i = 0; i < _boxDataList.Count; i++)
        {
            if(i >= tileBoxObjList.Count) {
                GameObject tempObj = CreateObj(itemObjPrefab, objParent);
                tileBoxObjList.Add(tempObj.GetComponent<TileBoxObj>());
            }
        }

        for (int i = 0; i < tileBoxObjList.Count; i++)
        {
            if(i < _boxDataList.Count) {
                tileBoxObjList[i].SetBox(_boxDataList[i]);
                tileBoxObjList[i].transform.position = GetTilePosition(_boxDataList[i].PositionY, _boxDataList[i].PositionX);
            }
        }
    }

    public void InitPlayerUI() {
        _kingdomBattleMapUI.InitPlayerUI(playerObj);
    }
    public void UpdateActionPointUI(int actionPoint) {
        _kingdomBattleMapUI.UpdateActionPointUI(actionPoint);
    }

    public void ShowRewardUI(DataMgr.PaymentData paymentData)
    {
        _kingdomBattleMapUI.ShowBoxRewardUI(paymentData);
    }

    public void InitMonsterUI() {
        _kingdomBattleMapUI.UpdateMonsterUI(tileMonsterObjList);
    }

    public void UpdateMonsterUI() {
        _kingdomBattleMapUI.UpdateUIActive();
    }

    public void InitBossUI() {
        _kingdomBattleMapUI.UpdateBossMonsterUI(tileMonsterObjList[tileMonsterObjList.Count-1]);
    }
    public void UpdateBossUI() {

    }



    public Vector2Int GetPlayerPosition() {
        return new Vector2Int(playerObj.PosX, playerObj.PosY);
    }

    public Vector3 GetTilePosition(int x, int y) {
        return bgTileObjList[x][y].transform.position;
    }

    public TileObj GetTileObj(int x, int y) {
        if(x < 0  || x >= bgTileObjList.Count)
        {
            return null;
        }
        if(y < 0 || y >= bgTileObjList[x].Count)
        {
            return null;
        }

        return bgTileObjList[x][y];
    }
    public TileObj GetBlindTileObj(int x, int y) {
        return blindTileObjList[x][y];
    }
    public int GetBlindTileData(int x, int y) {
        return _blindTileDataList[x][y];
    }

    public List<List<TileObj>> GetAllTileObj() {
        return bgTileObjList;
    }
    public List<List<TileObj>> GetAllBlindTileObj() {
        return blindTileObjList;
    }



    // 블라인드 타일 밝히기
    public void LightUpBlindTileData(int x, int y) {
        _blindTileDataList[x][y] = 1;
        blindTileObjList[x][y].UpdateBlindTile(1);
        DataMgr.Instance.SetKingdomBattleBlindMap(_blindTileDataList);
        DataMgr.Instance.SaveData();

        TileObj tempTargetTileObj = GetTileObj(x, y);
        if(IsExistMonsterObj(tempTargetTileObj._position)) {   //밝혀진 타일에 몬스터가 있을 경우 몬스터 보여줌
            GetMonsterObj(tempTargetTileObj._position).UpdateMonsterActive();
            UpdateMonsterUI();
        } else if(IsExistBoxObj(tempTargetTileObj._position)) {    //밝혀진 타일에 보물 상자가 있을 경우 보물상자 보여줌
            GetBoxObj(tempTargetTileObj._position).UpdateObjActive();
        } else {    //밝혀진 타일이 빈타일일 경우 이동
            // TileObj tempPlayerTile = GetTileObj(playerObj.PosY, playerObj.PosX);
            // tempTargetTileObj.ClickBtn(tempPlayerTile);
        }


        // if(_targetTileObj != null) {
        //     TileObj tempTargetTileObj = GetTileObj(_targetTileObj._position.y, _targetTileObj._position.x);
        //     if(IsExistMonsterObj(tempTargetTileObj._position)) {   //밝혀진 타일에 몬스터가 있을 경우 몬스터 보여줌
        //         GetMonsterObj(tempTargetTileObj._position).UpdateMonsterActive();
        //         UpdateMonsterUI();
        //     } else if(IsExistBoxObj(tempTargetTileObj._position)) {    //밝혀진 타일에 보물 상자가 있을 경우 보물상자 보여줌
        //         GetBoxObj(tempTargetTileObj._position).UpdateObjActive();
        //     } else {    //밝혀진 타일이 빈타일일 경우 이동
        //         // TileObj tempPlayerTile = GetTileObj(playerObj.PosY, playerObj.PosX);
        //         // tempTargetTileObj.ClickBtn(tempPlayerTile);
        //     }
        // }
    }

    // 해당 타일에 있는 몬스터 가져오기
    public TileMonsterObj GetMonsterObj(Vector2Int position) {
        for (int i = 0; i < tileMonsterObjList.Count; i++)
        {
            if(tileMonsterObjList[i].GetObjectPosition() == position) {
                return tileMonsterObjList[i];
            }
        }
        return null;
    }
    // 해당 타일에 획득형 오브젝트 가져오기 (보물상자, 희귀 보물상자, 회복 샘)
    public TileBoxObj GetBoxObj(Vector2Int position) {
        for (int i = 0; i < tileBoxObjList.Count; i++)
        {
            if(tileBoxObjList[i].GetObjectPosition() == position) {
                return tileBoxObjList[i];
            }
        }
        return null;
    }


    // 해당 타일에 몬스터 있는지 확인
    public bool IsExistMonsterObj(Vector2Int position) {
        for (int i = 0; i < tileMonsterObjList.Count; i++)
        {
            if(tileMonsterObjList[i].IsActive() && tileMonsterObjList[i].GetObjectPosition() == position) {
                return true;
            }
        }
        return false;
    }

    // 해당 타일에 획득형 오브젝트가 있는지 확인 (보물상자, 희귀 보물상자, 회복 샘)
    public bool IsExistBoxObj(Vector2Int position) {
        for (int i = 0; i < tileBoxObjList.Count; i++)
        {
            if(tileBoxObjList[i].IsActive() && tileBoxObjList[i].GetObjectPosition() == position) {
                return true;
            }
        }
        return false;
    }

    // 해당 타일에 몬스터, 보물상자 있는지 확인
    public bool IsExistObj(Vector2Int position) {
        bool isExistMoster = IsExistMonsterObj(position);
        bool isExistBox = IsExistBoxObj(position);
        
        return isExistMoster || isExistBox;
    }


    // 이동 가능한지 확인
    public bool IsMovePossiable(Vector3Int vec3) {
        for (int i = 0; i < _movePossiableVector.Count; i++)
        {
            if(_movePossiableVector[i] == vec3) {
                return true;
            }
        }
        return false;
    }



    // 해당 타일이 블라인드 상태인지 확인
    public bool IsBlindObj(TileObj targetObj) {
        if(GetBlindTileData(targetObj._position.y, targetObj._position.x) == 0) {
            return true;
        } else {
            return false;
        }
    }

    // 이웃 타일 가져오기
    public List<TileObj> GetNeighborTileObj(Vector2Int vec2)
    {
        List<TileObj> tileObjList = new();
        List<Vector2Int> tempVector = new();
        if(vec2.x % 2 == 0)
        {
            tempVector = _neighborEvenVector;
        }
        else
        { 
            tempVector = _neighborOddVector;
        }

        for (int i = 0; i < tempVector.Count; i++)
        {
            Vector2Int tileVec = tempVector[i] + vec2;
            TileObj tileObj = GetTileObj(tileVec.y, tileVec.x);
            if(tileObj != null)
            {
                tileObjList.Add(tileObj);
            }
        }
        return tileObjList;
    }

    // 이웃 타일 가져오기
    public List<TileObj> GetMovePossiableTileObj(Vector2Int vec2)
    {
        List<TileObj> tileObjList = new();
        List<Vector2Int> tempVector = new();
        if(vec2.x % 2 == 0)
        {
            tempVector = _movePossiableEvenVector;
        }
        else
        { 
            tempVector = _movePossiableOddVector;
        }

        for (int i = 0; i < tempVector.Count; i++)
        {
            Vector2Int tileVec = tempVector[i] + vec2;
            TileObj tileObj = GetTileObj(tileVec.y, tileVec.x);
            if(tileObj != null)
            {
                tileObjList.Add(tileObj);
            }
        }
        return tileObjList;
    }



    public TileMonsterObj GetTileMonsterObj(Vector2Int position) {
        for (int i = 0; i < tileMonsterObjList.Count; i++)
        {
            if(tileMonsterObjList[i].IsActive() && tileMonsterObjList[i].GetObjectPosition() == position) {
                return tileMonsterObjList[i];
            }
        }
        return null;
    }
    public TileBoxObj GetTileBoxObj(Vector2Int position) {
        for (int i = 0; i < tileBoxObjList.Count; i++)
        {
            if(tileBoxObjList[i].IsActive() && tileBoxObjList[i].GetObjectPosition() == position) {
                return tileBoxObjList[i];
            }
        }
        return null;
    }


    // 선택된 타일의 타일맵 오브젝트 데이터 가져오기 (몬스터 , 상자 등)
    public TileMapObjTableData GetTargetTileMapObjData() {
        TileMonsterObj tileMonsterObj = GetTileMonsterObj(_targetTileObj._position);
        if(tileMonsterObj != null) {
            return tileMonsterObj.GetTileMapObjTableData();
        }

        TileBoxObj tileBoxObj = GetTileBoxObj(_targetTileObj._position);
        if (tileBoxObj != null)
            return tileBoxObj.GetTileMapObjTableData();

        return null;
    }

    public void UpdateQuickBattleEnd() {
        UpdateTargetObj(); // 토벌전 몬스터 업데이트
        UpdatePlayerTile(); // 토벌전 플레이어 타일 업데이트

        CheckBackMask();     // 플레이어 타일보다 뒤에 있는 타일 처리

        cameraFollow.moveFlag = false;
        _targetTileObj = null;
        _isTouchPossible = true;
        
        // 자동 진행 체크
        CheckAutoPlay();
    }

    public void UpdateTargetObj() {
        TileMonsterObj tileMonsterObj = GetTileMonsterObj(_targetTileObj._position);
        tileMonsterObj.UpdateClear();
        tileMonsterObj.UpdateMonsterActive();
    }

    public void StartBattle() {
        TileMapObjTableData tileMapObjTableData = GetTargetTileMapObjData();

        int difficulty = DataMgr.Instance.GetKingdomWorldDifficulty();
        int mapIndex = DataMgr.Instance.GetKingdomWorldSelectIndex();
        int monsterIndex = tileMapObjTableData.ObjectValue;
        int stageNo = DataMgr.Instance.GetChangeStageNo(difficulty, mapIndex, monsterIndex);

        DataMgr.Instance.nowDungeon = Const.DUNGEON_KINGDOM_BATTLE;
        DataMgr.Instance.nowDungeonStageNo = stageNo;

        GameMgr.Quest.IncQuestCount(Const.QUEST_KINGDOM_BATTLE_CHALLENGE, 1);  // 토벌전 전투 도전 퀘스트

        DataMgr.Instance.SaveData();

        SoundMgr.Instance.PlayBattleStartSound();

        // 맵씬으로 이동
        GameObject.FindObjectOfType<ScreenFader>().FadeOut(Const.SCENE_INGAME);
    }

    private void ShowBattleAni() {
        if(DataMgr.Instance.IsKingdomQuickBattle() && DataMgr.Instance.getPayment(Const.PaymentType.QUICK_KINGDOM_BATTLE_TICKET) > 0) {
            playerObj.ShowAirshipBomb();
        } else {
            _kingdomBattleMapUI.ShowBattleAni();
        }
    }

    private void GetBoxReward() {
        TileBoxObj tileBoxObj = GetTileBoxObj(_targetTileObj._position);
        TileMapObjTableData tileMapObjTableData = tileBoxObj.GetTileMapObjTableData();

        TileMapBoxRewardTableData tileMapBoxRewardTableData = GameMgr.Table.GetKingdomTileMapBoxReward(tileMapObjTableData.MapDifficulty, tileMapObjTableData.MapIndex, tileMapObjTableData.ObjectType+tileMapObjTableData.ObjectValue);
        List<DataMgr.PaymentData> paymentDataList = tileMapBoxRewardTableData.GetPaymentDataList();
        
        switch (tileMapObjTableData.ObjectType)
        {
            case Const.KINGDOM_BATTLE_BOX:
                DataMgr.Instance.AddGetKingdomBox(tileMapObjTableData.ObjectValue);
                bool isRandPiece = false;
                if (paymentDataList[0].type == Const.PaymentType.PRI_EQP_GROUP_1)
                    isRandPiece = true;

                ShowRewardUI(paymentDataList[0]);
                paymentDataList = DataMgr.Instance.incPaymentDatas(paymentDataList, true);
                // PopupMgr.Instance.ShowPopup(PopupMgr.PopupReward, paymentDataList);
                if(isRandPiece)
                {
                    PopupMgr.Instance.ShowPopup(PopupMgr.PopupReward, paymentDataList);
                    SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_GET_REWARD);
                }
                else
                {
                    SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_TILE_GET_BOX);
                }
                break;
            case Const.KINGDOM_BATTLE_RARE_BOX:
                DataMgr.Instance.AddGetKingdomRareBox(tileMapObjTableData.MapDifficulty, tileMapObjTableData.MapIndex, tileMapObjTableData.ObjectValue);
                paymentDataList = DataMgr.Instance.incPaymentDatas(paymentDataList, true);
                PopupMgr.Instance.ShowPopup(PopupMgr.PopupReward, paymentDataList);
                SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_TILE_GET_BOX);
                break;
            case Const.KINGDOM_BATTLE_RECOVERY_WELL:
                DataMgr.Instance.AddGetKingdomRecoveryWell(tileMapObjTableData.ObjectValue);
                paymentDataList = DataMgr.Instance.incPaymentDatas(paymentDataList, false);
                // PopupMgr.Instance.ShowPopup(PopupMgr.PopupReward, paymentDataList);
                ShowRewardUI(paymentDataList[0]);
                SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_EVOL_COMBINE);
                break;
            case Const.KINGDOM_BATTLE_RESOURCE:
                DataMgr.Instance.AddGetKingdomResource(tileMapObjTableData.ObjectValue);
                for (int i = 0; i < paymentDataList.Count; i++)
                {
                    switch (paymentDataList[i].type)
                    {
                        case Const.PaymentType.WOOD:
                            DataMgr.Instance.SetTempSavedWood(DataMgr.Instance.GetTempSavedWood() + paymentDataList[i].value);
                            break;
                        case Const.PaymentType.FOOD:
                            DataMgr.Instance.SetTempSavedFood(DataMgr.Instance.GetTempSavedFood() + paymentDataList[i].value);
                            break;
                        case Const.PaymentType.IRON:
                            DataMgr.Instance.SetTempSavedIron(DataMgr.Instance.GetTempSavedIron() + paymentDataList[i].value);
                            break;
                    }
                    _kingdomBattleMapUI.ShowMovePayment(paymentDataList[i].type, paymentDataList[i].value, tileBoxObj.transform.position);
                    ShowRewardUI(paymentDataList[i]);
                }
                SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_TILE_GET_MATERIAL);
                break;
            default:
                break;
        }



        _kingdomBattleMapUI.UpdateObjCount();

        //DataMgr.Instance.SaveData();
        // 호출하는 부분에서 저장함.

        tileBoxObj.UpdateObjActive();

        
    }

    public bool IsTouchPossible()
    {
        return _isTouchPossible;
    }
    
    // 플레이어 이동 시작
    public void MovePlayer(List<TileObj> movePath, int actionPoint) {
        _movePath = movePath;
        cameraFollow.moveFlag = true;
        CheckNextMove();
        playerTile.SetActive(false);
        wayArrow.SetActive(false);
        UpdateActionPointUI(actionPoint);

        _isTouchPossible = false;
    }

    public void CheckNextMove() {
        if(_movePath.Count > 0) {
            bool isLast = _movePath.Count == 1;
            playerObj.MovePos(_movePath[0], _movePath.Count, isLast);
            _movePath.RemoveAt(0);
        } else {
            // TODO : 이동 끝.  관련 로직 있을 경우 추가 
            
            // 선택된 타일이 블라인드 타일일 경우
            if(IsBlindObj(_targetTileObj)) {
                TileObj tempPlayerTile = GetTileObj(playerObj.PosY, playerObj.PosX);
                _targetTileObj.ClickBtn(tempPlayerTile);
            } else if(IsExistMonsterObj(_targetTileObj._position)) { // 선택된 타일에 몬스터가 있을 경우
                DataMgr.Instance.SetTempPlayerPos(_targetTileObj._position);
                // StartBattle();
                ShowBattleAni();
                return;
            } else if(IsExistBoxObj(_targetTileObj._position)) { // 선택된 타일에 보물 상자가 있을 경우
                GetBoxReward();
            }

            // 이동 위치 저장
            DataMgr.Instance.SetPlayerPos(_targetTileObj._position);
            // 전투맵일 경우 다음 도착 지점을 저장
            _playingRoot.Add(_targetTileObj._position);
            DataMgr.Instance.SaveData(true);
            // ConnMgr.Instance.UpdateUserData(DataMgr.Instance.GetSaveStr());

            // 플레이어 도착 위치로 플레이어 타일 옮겨줌
            UpdatePlayerTile();
            CheckBackMask();

            cameraFollow.moveFlag = false;
            _targetTileObj = null;
            _isTouchPossible = true;

            // 자동 진행 체크
            CheckAutoPlay();
        }
    }

    // 플레이어 타일보다 뒤에 있는 타일 처리
    private void CheckBackMask() {
        for (int i = 0; i < bgTileObjList.Count; i++)
        {
            if(playerObj.PosX-1 >=0) {
                bgTileObjList[i][playerObj.PosX-1].CheckBackMask();
            }
        }
    }

    public void ShowTargetArrow() {
        targetArrow.SetActive(true);
        targetArrow.transform.position = _targetTileObj.transform.position;
        targetArrow.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "touch", false);
        CancelInvoke("EndTargetArrow");
        Invoke("EndTargetArrow", 1f);
    }
    public void EndTargetArrow() {
        targetArrow.SetActive(false);
    }
    



    //// 마지막 스테이지 클리어 체크
    public void CheckLastStageClear() {
        if(DataMgr.Instance.IsClearLastKingdomMonster(DataMgr.Instance.GetKingdomWorldDifficulty(), DataMgr.Instance.GetKingdomWorldSelectIndex())) {
            ActRewardLastStage();
        }
    }

    private void ActRewardLastStage() {
        List<DataMgr.PaymentData> paymentDataList = new List<DataMgr.PaymentData>();

        List<TileMapObjTableData> tileMapObjTableList = GameMgr.Table.GetKingdomTileMapObjListToType(DataMgr.Instance.GetKingdomWorldDifficulty(), DataMgr.Instance.GetKingdomWorldSelectIndex(), Const.KINGDOM_BATTLE_RARE_BOX);
        TileMapObjTableData tileMapObjTableData = tileMapObjTableList[tileMapObjTableList.Count-1];  // 마지막 레어 보물은 보스 클리어시 획득
        TileMapBoxRewardTableData tileMapBoxRewardTableData = GameMgr.Table.GetKingdomTileMapBoxReward(tileMapObjTableData.MapDifficulty, tileMapObjTableData.MapIndex, tileMapObjTableData.ObjectType+tileMapObjTableData.ObjectValue);
        if(DataMgr.Instance.IsGetKingdomRareBox(DataMgr.Instance.GetKingdomWorldDifficulty(), DataMgr.Instance.GetKingdomWorldSelectIndex(), tileMapBoxRewardTableData.ObjectValue)) {
            //이미 보상 받은 경우 클리어 보상 추가하지 않음
        } else {
            //아직 보상 안받은 경우, 보상 받은 내역 추가
            paymentDataList.AddRange(tileMapBoxRewardTableData.GetPaymentDataList());  // = tileMapBoxRewardTableData.GetPaymentDataList();
            DataMgr.Instance.AddGetKingdomRareBox(DataMgr.Instance.GetKingdomWorldDifficulty(), DataMgr.Instance.GetKingdomWorldSelectIndex(), tileMapBoxRewardTableData.ObjectValue);
        }
        
        // 쌓아놓은 임시 재화 습득함
        long tempWoodCount = DataMgr.Instance.GetTempSavedWood();
        long tempFoodCount = DataMgr.Instance.GetTempSavedFood();
        long tempIronCount = DataMgr.Instance.GetTempSavedIron();

        if(tempWoodCount > 0) 
        {
            DataMgr.Instance.AddPaymentData(ref paymentDataList, Const.PaymentType.WOOD, tempWoodCount);
        }
        if(tempFoodCount > 0) 
        {
            DataMgr.Instance.AddPaymentData(ref paymentDataList, Const.PaymentType.FOOD, tempFoodCount);
        }
        if(tempIronCount > 0) 
        {
            DataMgr.Instance.AddPaymentData(ref paymentDataList, Const.PaymentType.IRON, tempIronCount);
        }

        // 재화 실제 지급 및 데이터 처리
        paymentDataList = DataMgr.Instance.incPaymentDatas(paymentDataList);
        // 임시 재료값 0으로 변경해줌
        DataMgr.Instance.SetTempSavedWood(0);
        DataMgr.Instance.SetTempSavedFood(0);
        DataMgr.Instance.SetTempSavedIron(0);

        // 연출 성 index 저장
        DataMgr.Instance.SetNowKingdomBattleClearAniIndex(DataMgr.Instance.GetKingdomWorldSelectIndex());
        if(DataMgr.Instance.GetNowKingdomBattleClearAniComp())
            DataMgr.Instance.SetNowKingdomBattleClearAniIndex(-1);

        // 클리어 경로 저장
        _clearRoot.Clear();
        for (int i = 0; i < _playingRoot.Count; i++)
        {
            _clearRoot.Add(_playingRoot[i]);
        }
        _playingRoot.Clear();

        //선택된 토벌전 초기화
        DataMgr.Instance.SetPlayKingdomMap(0,0);
        DataMgr.Instance.ResetKingdomBattleMap();

        // 클리어로그 추가
        int stageNo = DataMgr.Instance.GetStageNo(DataMgr.Instance.GetKingdomWorldDifficulty(), DataMgr.Instance.GetKingdomWorldSelectIndex(), 0);
        JsonData logJson = GameMgr.Log.GetLogObject(LogMgr.LOG_KINGDOM_CASTLE_CLEAR, stageNo, paymentDataList);
        string logdata = logJson.ToJson();

        DataMgr.Instance.SaveData(true, logdata);
        // ConnMgr.Instance.UpdateUserData(DataMgr.Instance.GetSaveStr(), logdata);

        // 보상 팝업
        string nameStr = LocaleMgr.GetString(8903, "{1}", DataMgr.Instance.GetKingdomWorldSelectIndex().ToString());
        
        PopupMgr.Instance.ShowPopup(PopupMgr.PopupRewardMapclear, paymentDataList, LocaleMgr.GetString(8909, "{1}", nameStr));
        PopupMgr.Instance.GetPopup(PopupMgr.PopupRewardMapclear).GetComponent<PopupReward>().addOkEvent(ActChangeScene);
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_TILE_GET_BOX);
        
        // PopupMgr.Instance.ShowPopup(PopupMgr.PopupReward, paymentDataList, LocaleMgr.GetString(8909, "{1}", nameStr));
        // PopupMgr.Instance.GetPopup(PopupMgr.PopupReward).GetComponent<PopupReward>().addOkEvent(ActChangeScene);
    }

    public bool ActChangeScene() {
        SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_CLEAR);
        if (DataMgr.Instance.GetKingdomWorldSelectIndex() == GameMgr.Table.GetKingdomMapIndexMaxCount(DataMgr.Instance.GetKingdomWorldDifficulty())
            && !DataMgr.Instance.GetNowKingdomBattleClearAniComp())
        {
            // 난이도 클리어 연출 팝업 추가
            PopupMgr.Instance.ShowPopup(PopupMgr.PopupWorldMapClear);
        }
        else
        {
            // 씬 이동
            GameObject.FindObjectOfType<ScreenFader>().FadeOut(Const.SCENE_KINGDOM_WORLDMAP);
        }
        return true;
    }


    public KingdomBattleMapUI GetKingdomBattleMapUI() {
        return _kingdomBattleMapUI;
    }






    void Update()
    {
        if(!_isTouchPossible) // 터치 불가능이면 리턴
        {
            return;
        }
        if(DataMgr.Instance.IsAutoNextDungeon(Const.DUNGEON_KINGDOM_BATTLE))  // 자동 전투중 터치 불가
        {
            return;
        }

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)   // pc
//#if UNITY_ANDROID
        // 마우스 입력
        if (Input.GetMouseButtonDown(0))
        {  // 마우스 클릭 시작
            if(!EventSystem.current.IsPointerOverGameObject()) // UI 터치 체크
            {  
                nowTouchPos = Input.mousePosition;
                ActTouchDown();
            }
        }
        else if (Input.GetMouseButton(0))
        {  //마우스 드래그
            if(!EventSystem.current.IsPointerOverGameObject()) // UI 터치 체크
            {
                nowTouchPos = Input.mousePosition;
                ActTouchMove();
                // if(Const.IS_KINGDOM_BATTLE_MAP_LIGHT) {
                    
                // }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {  //마우스 클릭 끝
            if(!EventSystem.current.IsPointerOverGameObject()) // UI 터치 체크
            {
                nowTouchPos = Input.mousePosition;
                cameraFollow.EndMovePos();
                if (!_isTouchMove)
                {
                    ActTouchUp();
                }
            }
        }

#else
        // 터치 시
        if (Input.touchCount == 1) {
            if(!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) // UI 터치 체크
            {  
                if(Input.touches[0].phase == TouchPhase.Began) {
                    nowTouchPos = Input.GetTouch(0).position;
                    ActTouchDown();
                } else if (Input.touches[0].phase == TouchPhase.Moved) {
                    nowTouchPos = Input.GetTouch(0).position;
                    ActTouchMove();
                    // if(Const.IS_KINGDOM_BATTLE_MAP_LIGHT) {
                        
                    // }
                } else if (Input.touches[0].phase == TouchPhase.Ended) {
                    nowTouchPos = Input.GetTouch(0).position;
                    cameraFollow.EndMovePos();
                    if(!_isTouchMove) {
                        ActTouchUp();
                    }
                }
            }
        }
#endif
    }

    // 드래그 거리에 따라 드래그로 움직일 수 있는지 체크
    private void CheckTouchMove() {
        if(Vector3.Distance(nowTouchPos, startTouchPos) > 10f) {
            _isTouchMove = true;
        }
    }

    // 터치 누를 때 실행
    private void ActTouchDown() {
        startTouchPos = nowTouchPos;
        _isTouchMove = false;
        // 카메라에서 스크린에 마우스 클릭 위치를 통과하는 광선을 반환합니다.
        // Ray ray = Camera.main.ScreenPointToRay(nowTouchPos);
        // RaycastHit hitInformation;
        // Physics.Raycast(ray, out hitInformation);


        float cameraDepth = -mainCamera.transform.position.z;  // 카메라 떨어진 거리
        float rayRange = 10f;
        Vector3 MousePosition = mainCamera.ScreenToWorldPoint(new Vector3(nowTouchPos.x, nowTouchPos.y, cameraDepth));
        RaycastHit2D hitInformation = Physics2D.Raycast(MousePosition, transform.forward, rayRange);

        if (hitInformation.collider != null)
        {
            GameObject touchedObject = hitInformation.transform.gameObject;
            if (touchedObject.TryGetComponent(out TileMonsterObj _tileMonsterObj)) {

            } else if(touchedObject.TryGetComponent(out TileObj _tileObj)) {
                if(_tileObj.GetTileType() == Const.TileType.BG) {
                    _targetTileObj = _tileObj;
                } else if(_tileObj.GetTileType() == Const.TileType.BLIND){
                    _targetTileObj = _tileObj;
                }
            }
        }
    }

    // 터치 드래그 중 일 때 실행
    private void ActTouchMove() {
        if(!cameraFollow.moveFlag) {
            CheckTouchMove();
            if(_isTouchMove) {  // 맵 드래그 기능.
                Vector3 moveVector = (startTouchPos-nowTouchPos)*0.02f;  //움직인 거리 벡터
                cameraFollow.UpdateMovePos(moveVector);
                cameraFollow.moveFlag = false;
            }
        }
    }

    // 터치 땔 때 실행
    public  void ActTouchUp() {
        // 카메라에서 스크린에 마우스 클릭 위치를 통과하는 광선을 반환합니다.
        // Ray ray = Camera.main.ScreenPointToRay(nowTouchPos);
        // RaycastHit hitInformation;
        // Physics.Raycast(ray, out hitInformation);

        float cameraDepth = -mainCamera.transform.position.z;  // 카메라 떨어진 거리
        float rayRange = 10f;
        Vector3 MousePosition = mainCamera.ScreenToWorldPoint(new Vector3(nowTouchPos.x, nowTouchPos.y, cameraDepth));
        RaycastHit2D hitInformation = Physics2D.Raycast(MousePosition, transform.forward, rayRange);
        
        if (hitInformation.collider != null)
        {
            GameObject touchedObject = hitInformation.transform.gameObject;
            if (touchedObject.TryGetComponent(out TileMonsterObj _tileMonsterObj)) {
                _tileMonsterObj.ClickBtn();
            } else if(touchedObject.TryGetComponent(out TileObj _tileObj)) {
                // _targetTileObj = _tileObj;
                TileObj tempPlayerTile = GetTileObj(playerObj.PosY, playerObj.PosX);
                if(_targetTileObj == _tileObj && _tileObj != tempPlayerTile) {
                    if(IsSameRoot() && _targetTileObj._position != _clearRoot[_playingRoot.Count])
                    {
                        PopupMgr.Instance.ShowPopup(PopupMgr.PopupMsg, PopupMgr.MsgType.KINGDOM_BATTLE_ROOT);
                        PopupMgr.Instance.GetPopup(PopupMgr.PopupMsg).GetComponent<PopupMsg>().addOkEvent(ActDifferentRoot);
                    } 
                    else
                    {
                        ActClickTile();
                    }
                    SoundMgr.Instance.PlaySFXSound(SoundMgr.SFX_TILE_CLICK);
                }                
            }
        }
    }

    public bool ActDifferentRoot()
    {
        if(!_targetTileObj.IsDontMove())
            RemoveClearRootTile();
        ActClickTile();
        return true;
    }

    public bool ActClickTile()
    {
        ShowTargetArrow();
        TileObj tempPlayerTile = GetTileObj(playerObj.PosY, playerObj.PosX);
        _targetTileObj.ClickBtn(tempPlayerTile);

        return true;
    }

    public void ActTargetBoxObj(int index)
    {
        _targetTileObj = tileBoxObjList[index].GetTileObj();
        TileObj tempPlayerTile = GetTileObj(playerObj.PosY, playerObj.PosX);
        ShowTargetArrow();
        _targetTileObj.ClickBtn(tempPlayerTile);
    }

    public void ActTargetMonsterObj(int index)
    {
        _targetTileObj = tileMonsterObjList[index].GetTileObj();
        TileObj tempPlayerTile = GetTileObj(playerObj.PosY, playerObj.PosX);
        ShowTargetArrow();
        _targetTileObj.ClickBtn(tempPlayerTile);
    }



    //// 자동 진행 관련 로직
    
    


    // 다음 경로 찾기
    public Vector2Int GetNextPosition()
    {
        Vector2Int nextPosition = new(0,0);
        if(playerObj)
        if(_playingRoot.Count == 0)
        {
            nextPosition = _clearRoot[0];
        }
        else if(_playingRoot.Count == _clearRoot.Count)
        {

        }
        else 
        {
            for (int i = _playingRoot.Count-1; i < _clearRoot.Count; i++)
            {
                if(_clearRoot[i] == _playingRoot[^1])
                {
                    nextPosition = _clearRoot[i+1];
                    break;
                }
            }
        }
        return nextPosition;
    }
    // 저장된 경로 자동 진행
    public void AutoNextMove()
    {
        Vector2Int nextPosition = GetNextPosition();
        if(nextPosition.x == 0 && nextPosition.y == 0) 
        {
            return;
        }
        // 자동이동 중 마지막보상을 받을 경우 다시 시작함
        if(PopupMgr.Instance.IsActive(PopupMgr.PopupRewardMapclear))
        {
            Invoke("AutoRePlay", 2);
            return;
        }
        _targetTileObj = GetTileObj(nextPosition.y, nextPosition.x); 
        TileObj tempPlayerTile = GetTileObj(playerObj.PosY, playerObj.PosX);
        ShowTargetArrow();
        _targetTileObj.ClickBtn(tempPlayerTile);
    }

    public void ClickTestAuto()
    {
        if(_clearRoot.Count > 0)
        {
            AutoNextMove();
        }
    }

    // 해당 맵 마지막 보상 받은 후 자동전투 진행중일 경우 다시 시작
    public void AutoRePlay()
    {
        // 클리어 한 경우 다시 해당 맵 선택한 이후 다시 시작
        DataMgr.Instance.SetKingdomWorldSelectIndex(DataMgr.Instance.GetKingdomWorldSelectIndex());
        GameObject.FindObjectOfType<ScreenFader>().FadeOut(Const.SCENE_KINGDOM_BATTLEMAP);
    }

    // 자동 진행인지 확인후 자동전투이면 일정 시간 후 특이사항 체크
    public void CheckAutoPlay()
    {
        if (DataMgr.Instance.IsAutoNextDungeon(Const.DUNGEON_KINGDOM_BATTLE))
        {
            Invoke("AutoNextStep", 0.5f);
        }
    }
    // 특이사항 없으면 다음 타일로 이동시켜줌
    public void AutoNextStep()
    {
        // 팝업이 있으면 전부 닫아줌
        if(PopupMgr.Instance.IsActive())
        {
            PopupMgr.Instance.HideAllPopup();
        }

        AutoNextMove();
    }

    // 기존 루트와 같은지 체크
    public bool IsSameRoot()
    {
        // 기존 루트가 없다면 항상 다른 루트임
        if(_clearRoot.Count == 0)
        {
            return false;
        }

        bool isSame = true;
        for (int i = 0; i < _playingRoot.Count; i++)
        {
            if(_playingRoot[i] != _clearRoot[i])
            {
                isSame = false;
                break;
            }
        }
        return isSame;
    }
}
