// ResourceExample.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using EasyTools;

public class ResourceExample : MonoBehaviour
{
    [Header("资源配置")]
    public string prefabLocation = "TestCube";
    public string textureLocation = "TestTexture";
    public string sceneLocation = "TestScene";

    [Header("UI")]
    public Text statusText;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) TestLoadAsset();
        if (Input.GetKeyDown(KeyCode.Alpha2)) StartCoroutine(TestLoadAssetAsync());
        if (Input.GetKeyDown(KeyCode.Alpha3)) ShowStatistics();
        if (Input.GetKeyDown(KeyCode.Alpha4)) UnloadUnused();
        if (Input.GetKeyDown(KeyCode.Alpha5)) TestReferenceCount();
    }

    void TestLoadAsset()
    {
        AssetHandle handle = EasyAsset.Instance.LoadAssetAsync(prefabLocation);
        StartCoroutine(WaitAndInstantiate(handle));
    }

    IEnumerator TestLoadAssetAsync()
    {
        AssetHandle handle = EasyAsset.Instance.LoadAssetAsync(prefabLocation);
        yield return handle.WaitForCompletion();

        if (handle.Status == EProviderStatus.Succeed)
        {
            GameObject obj = handle.InstantiateSync();
            obj.transform.position = Random.insideUnitSphere * 3f;
            UpdateStatus($"加载成功: {prefabLocation}");
            
            yield return new WaitForSeconds(3f);
            Destroy(obj);
            handle.Release();
            UpdateStatus("资源已释放");
        }
        else
        {
            UpdateStatus($"加载失败: {handle.LastError}");
            handle.Release();
        }
    }

    IEnumerator WaitAndInstantiate(AssetHandle handle)
    {
        yield return handle.WaitForCompletion();

        if (handle.Status == EProviderStatus.Succeed)
        {
            GameObject obj = handle.InstantiateSync();
            obj.transform.position = Random.insideUnitSphere * 3f;
            UpdateStatus($"同步加载成功");
            
            yield return new WaitForSeconds(3f);
            Destroy(obj);
            handle.Release();
        }
        else
        {
            handle.Release();
        }
    }

    void ShowStatistics()
    {
        EasyAsset.Instance.GetStatistics(out int providers, out int loaders, out int zeroRefProviders, out int zeroRefLoaders);
        UpdateStatus($"Provider: {providers} (零引用: {zeroRefProviders})\nLoader: {loaders} (零引用: {zeroRefLoaders})");
    }

    void UnloadUnused()
    {
        EasyAsset.Instance.UnloadUnusedAssets();
        UpdateStatus("已卸载未使用资源");
    }

    void TestReferenceCount()
    {
        StartCoroutine(TestRefCountCoroutine());
    }

    IEnumerator TestRefCountCoroutine()
    {
        // 第一次加载
        AssetHandle handle1 = EasyAsset.Instance.LoadAssetAsync(prefabLocation);
        yield return handle1.WaitForCompletion();
        UpdateStatus("加载1: RefCount应该=1");

        // 第二次加载（复用Provider）
        AssetHandle handle2 = EasyAsset.Instance.LoadAssetAsync(prefabLocation);
        yield return handle2.WaitForCompletion();
        UpdateStatus("加载2: RefCount应该=2（复用Provider）");

        yield return new WaitForSeconds(1f);

        // 释放第一个
        handle1.Release();
        UpdateStatus("释放1: RefCount应该=1");

        yield return new WaitForSeconds(1f);

        // 释放第二个
        handle2.Release();
        UpdateStatus("释放2: RefCount=0，但未卸载");

        yield return new WaitForSeconds(1f);

        // 调用卸载
        EasyAsset.Instance.UnloadUnusedAssets();
        UpdateStatus("调用UnloadUnusedAssets后，资源被卸载");
    }

    void UpdateStatus(string msg)
    {
        Debug.Log($"[Example] {msg}");
        if (statusText) statusText.text = msg;
    }
}