using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DBLink", menuName = "DatabaseLink", order = 1)]
public class PrefabList : ScriptableObject
{
    public List<GameObject> entityList, backgroundList, TilesetList;
}
