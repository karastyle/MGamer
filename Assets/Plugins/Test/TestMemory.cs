using System.Collections;
using System.Collections.Generic;
using EasyTools;
using UnityEngine;

public class TestMemory : MonoBehaviour
{

    void Start() { }

    void Update() { }

    public void Test1()
    {
        StartCoroutine(TestAllocation());
    }
    
    public void Test2()
    {
        StartCoroutine(TestAllocation2());
    }
    
    public IEnumerator TestAllocation()
    {
        var atlasName = "testAAA";
        AssetHandle handle = EasyAsset.Instance.LoadAssetAsync(atlasName);
        yield return handle.WaitForCompletion();

        var atlas = handle.AssetObject<GameObject>();

        Debug.Log("test1");
    }
    
    public IEnumerator TestAllocation2()
    {
        var atlasName = "testAAA";
        AssetHandle handle = EasyAsset.Instance.LoadAssetAsync(atlasName);
        yield return handle.WaitForCompletion();

        var atlas = handle.AssetObject<GameObject>();

        var root = GameObject.Find("Persistent/Root");
        var obj = Instantiate(atlas, root.transform);
        obj.transform.localPosition = Vector3.zero;

        
        Debug.Log("test2");
    }

}