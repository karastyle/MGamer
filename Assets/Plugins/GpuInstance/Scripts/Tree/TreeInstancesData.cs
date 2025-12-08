// TreeInstanceData.cs - 去掉bendFactor

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TreeInstancesData
{
    public Vector3 terrainPosition;
    public Vector3 terrainSize;
    public List<TreePrototypeInfo> prototypes;
    public List<TreeInstanceInfo> instances;
}

[System.Serializable]
public class TreePrototypeInfo
{
    public int index;
    public string prefabPath;
}

[System.Serializable]
public class TreeInstanceInfo
{
    public int prototypeIndex;
    public Vector3 position;
    public float rotation;
    public float widthScale;
    public float heightScale;
}