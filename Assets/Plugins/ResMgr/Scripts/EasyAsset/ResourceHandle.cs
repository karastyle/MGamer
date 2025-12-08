namespace EasyTools
{
// ResourceHandle.cs (添加CreateTime存储为DateTime)
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public abstract class HandleBase
{
    protected ProviderBase _provider;
    protected bool _isReleased = false;
    protected DateTime _createTime;
    private int _handleRefCount = 1;

    public string Location => _provider?.Location;
    public EProviderStatus Status => _provider?.Status ?? EProviderStatus.None;
    public string LastError => _provider?.LastError;
    public bool IsDone => _provider?.IsDone ?? false;
    public DateTime CreateTime => _createTime;
    public int HandleRefCount => _handleRefCount;
    public ProviderBase Provider => _provider;

    internal void SetProvider(ProviderBase provider)
    {
        _provider = provider;
        _createTime = DateTime.Now;
        if (_provider != null)
        {
            _provider.Retain();
            _provider.AddHandle(this);
        }
    }

    public IEnumerator WaitForCompletion()
    {
        if (_provider != null)
        {
            yield return new WaitUntil(() => _provider.IsDone);
        }
    }

    public void Retain()
    {
        _handleRefCount++;
    }

    public void Release()
    {
        if (_isReleased) return;

        _handleRefCount--;
        if (_handleRefCount <= 0)
        {
            _isReleased = true;

            if (_provider != null)
            {
                _provider.RemoveHandle(this);
                _provider.Release();
                _provider = null;
            }
        }
    }
}

public class AssetHandle : HandleBase
{
    public Object Asset => (_provider as AssetProvider)?.AssetObject;
    public T AssetObject<T>() where T : Object => Asset as T;
    public GameObject InstantiateSync() => Object.Instantiate(Asset) as GameObject;
    public GameObject InstantiateSync(Transform parent) => Object.Instantiate(Asset, parent) as GameObject;
}

public class SubAssetsHandle : HandleBase
{
    public Object[] AllAssets => (_provider as SubAssetsProvider)?.AllAssets;

    public T[] GetSubAssets<T>() where T : Object
    {
        var assets = AllAssets;
        if (assets == null) return null;

        int count = 0;
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is T) count++;
        }

        T[] result = new T[count];
        int index = 0;
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is T t)
            {
                result[index++] = t;
            }
        }
        return result;
    }
}

public class SceneHandle : HandleBase
{
    public string SceneName => (_provider as SceneProvider)?.SceneName;
    public float Progress => (_provider as SceneProvider)?.Progress ?? 0f;

    public bool ActivateScene()
    {
        return (_provider as SceneProvider)?.ActivateScene() ?? false;
    }

    public bool IsActiveScene()
    {
        return (_provider as SceneProvider)?.IsActiveScene() ?? false;
    }

    public IEnumerator UnloadAsync()
    {
        var sceneProvider = _provider as SceneProvider;
        if (sceneProvider != null)
        {
            yield return sceneProvider.UnloadAsync();
        }
    
        Release();

        if (sceneProvider != null)
        {
            //场景卸载后，provider就没用了，无法复用，因此要直接销毁
            EasyAsset.Instance.RemoveProviderInternal(sceneProvider.Location);
        }
    }
}

public class RawFileHandle : HandleBase
{
    public byte[] Data => (_provider as RawFileProvider)?.Data;
    public string FilePath => (_provider as RawFileProvider)?.FilePath;
    public string GetText() => Data != null ? System.Text.Encoding.UTF8.GetString(Data) : null;
}
}