using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : Singleton<AddressableManager>
{
    private Dictionary<string, Object> assetCache = new Dictionary<string, Object>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void LoadAsset<T>(string address, System.Action<T> onComplete) where T : Object
    {
        if (assetCache.ContainsKey(address))
        {
            onComplete?.Invoke(assetCache[address] as T);
            return;
        }

        Addressables.LoadAssetAsync<T>(address).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                assetCache[address] = handle.Result;
                onComplete?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogError($"로드 실패: {address}");
            }
        };
    }
}
