게임의 메인 컨텐츠 중 마왕군 토벌전 타일맵 관련 로직입니다.

AStar.cs : AStar 알고리즘을 통해 목적지까지 최단거리 이동 경로 타일들을 얻기 위한 로직입니다.
KingdomBattleMapMgr.cs : 토벌전 맵을 관리하는 로직입니다.  토벌전 맵 생성, 플레이어의 이동 등을 관리합니다.
TileObj.cs : 타일 오브젝트를 관리하는 로직입니다.

KingdomBattleMapMgr -> InitMap 함수를 통해 맵 구성 로직을 찾아볼 수 있습니다.
AStarPathfinding -> FindPath 함수를 통해 최단 거리 경로 관련 로직을 찾아볼 수 있습니다.
