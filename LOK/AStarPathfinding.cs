using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Implementation of A* pathfinding algorithm.
/// </summary>
public class AStarPathfinding
{
    /// <summary>
    /// Finds path from given start point to end point. Returns an empty list if the path couldn't be found.
    /// </summary>
    /// <param name="startPoint">Start tile.</param>
    /// <param name="endPoint">Destination tile.</param>
    public static List<TileObj> FindPath(TileObj startPoint, TileObj endPoint)
    {
        List<TileObj> openPathTiles = new List<TileObj>();
        List<TileObj> closedPathTiles = new List<TileObj>();

        // Prepare the start tile.
        TileObj currentTile = startPoint;

        currentTile.g = 0;
        currentTile.h = GetEstimatedPathCost(startPoint.position, endPoint.position);

        // Add the start tile to the open list.
        openPathTiles.Add(currentTile);

        while (openPathTiles.Count != 0)
        {
            // Sorting the open list to get the tile with the lowest F.
            openPathTiles = openPathTiles.OrderBy(x => x.F).ThenByDescending(x => x.g).ToList();
            currentTile = openPathTiles[0];

            // Removing the current tile from the open list and adding it to the closed list.
            openPathTiles.Remove(currentTile);
            closedPathTiles.Add(currentTile);

            int g = currentTile.g + 1;

            // If there is a target tile in the closed list, we have found a path.
            if (closedPathTiles.Contains(endPoint))
            {
                break;
            }

            // Investigating each adjacent tile of the current tile.
            foreach (TileObj adjacentTile in currentTile.adjacentTiles)
            {
                // Ignore not walkable adjacent tiles.
                if (endPoint != adjacentTile && adjacentTile.IsObstacle())
                {
                    continue;
                }

                // Ignore the tile if it's already in the closed list.
                if (closedPathTiles.Contains(adjacentTile))
                {
                    continue;
                }

                // If it's not in the open list - add it and compute G and H.
                if (!(openPathTiles.Contains(adjacentTile)))
                {
                    adjacentTile.g = g;
                    adjacentTile.h = GetEstimatedPathCost(adjacentTile.position, endPoint.position);
                    openPathTiles.Add(adjacentTile);
                }
                // Otherwise check if using current G we can get a lower value of F, if so update it's value.
                else if (adjacentTile.F > g + adjacentTile.h)
                {
                    adjacentTile.g = g;
                }
            }
        }

        List<TileObj> finalPathTiles = new List<TileObj>();

        // Backtracking - setting the final path.
        // if (closedPathTiles.Contains(endPoint))
        // {
        //     currentTile = endPoint;
        //     finalPathTiles.Add(currentTile);

        //     for (int i = endPoint.g - 1; i >= 0; i--)
        //     {
        //         currentTile = closedPathTiles.Find(x => x.g == i && currentTile.adjacentTiles.Contains(x));
        //         finalPathTiles.Add(currentTile);
        //     }

        //     finalPathTiles.Reverse();
        // }

        if(endPoint.IsObstacle()) {  // 도착지점이 막힌 땅, 몬스터인 경우 거기까지 이동
            for (int i = endPoint.g - 1; i >= 0; i--)
            {
                currentTile = closedPathTiles.Find(x => x.g == i && currentTile.adjacentTiles.Contains(x));
                finalPathTiles.Add(currentTile);
            }
            finalPathTiles.Reverse();
            if(endPoint.IsExistObj()) {
                finalPathTiles.Add(endPoint);
            }
        } else {
            if(endPoint.g != 0) {  // 아예 갈 수 없는 곳은 막는 기능
                currentTile = endPoint;
                finalPathTiles.Add(currentTile);

                for (int i = endPoint.g - 1; i >= 0; i--)
                {
                    currentTile = closedPathTiles.Find(x => x.g == i && currentTile.adjacentTiles.Contains(x));
                    finalPathTiles.Add(currentTile);
                }

                finalPathTiles.Reverse();
            }
        }

        return finalPathTiles;
    }

    /// <summary>
    /// Returns estimated path cost from given start position to target position of hex tile using Manhattan distance.
    /// </summary>
    /// <param name="startPosition">Start position.</param>
    /// <param name="targetPosition">Destination position.</param>
    protected static int GetEstimatedPathCost(Vector3Int startPosition, Vector3Int targetPosition)
    {
        return Mathf.Max(Mathf.Abs(startPosition.z - targetPosition.z), Mathf.Max(Mathf.Abs(startPosition.x - targetPosition.x), Mathf.Abs(startPosition.y - targetPosition.y)));
    }


    //좌표계 변환 ( offset-EvenR(x,y) -> cube(x,y,z) )
    public static Vector3Int OffsetEvenRToCube(Vector2Int hex) {
        int q = hex.x - (hex.y + (hex.y&1)) / 2;
        int r = hex.y;
        return new Vector3Int(q, r, -q-r);
    }
    //좌표계 변환 ( offset-EvenQ(x,y) -> cube(x,y,z) )
    public static Vector3Int OffsetEvenQToCube(Vector2Int hex) {
        int q = hex.x;
        int r = hex.y - (hex.x + (hex.x&1)) / 2;
        return new Vector3Int(q, r, -q-r);
    }

    //좌표계 변환 ( cube(x,y,z) -> offset(x,y) )
    public static Vector2Int CubeToOffset(Vector3Int hex) {
        int col = (int)hex.x + ((int)hex.y + ((int)hex.z&1)) / 2;
        int row = (int)hex.y;
        return new Vector2Int(col, row);
    }
}
