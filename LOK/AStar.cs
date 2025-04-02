using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    // List<Vector2> evenr_direction_differences = new List<Vector2>();


    public void InitAstar() {
        // evenr_direction_differences.Add(new Vector2(1,0));
        // evenr_direction_differences.Add(new Vector2(1,0));
    }


    //좌표계 변환 ( offset(x,y) -> cube(x,y,z) )
    public static Vector3Int TransOffsetToCube(Vector2Int hex) {
        int q = hex.x - (hex.y + (hex.x&1)) / 2;
        int r = hex.y;
        return new Vector3Int(q, r, -q-r);
    }
    //좌표계 변환 ( cube(x,y,z) -> offset(x,y) )
    public static Vector2Int TransCubeToOffset(Vector3Int hex) {
        int col = (int)hex.x + ((int)hex.y + ((int)hex.z&1)) / 2;
        int row = (int)hex.y;
        return new Vector2Int(col, row);
    }



    // public bool isNeighbors() {

    // }

    // public List<Vector2> GetNeighborList(Vector2 pos) {
    //     List<Vector2> neighbors = new List<Vector2>();

    // }

}
