using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class AddressableManager : Singleton<AddressableManager>
{
    #region field
    private Dictionary<string, StageConfigSO> _stageDB = new Dictionary<string, StageConfigSO>();
    private Dictionary<string, SceneInstance> _scene = new Dictionary<string, SceneInstance>();
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    #region method
    //유효성 && 완료 && 성공 체크 메서드
    private bool IsSucceeded(AsyncOperationHandle handle)
    {
        if (handle.IsValid() && handle.IsDone && handle.Status == AsyncOperationStatus.Succeeded)
        {
            return true;
        }
        return false;
    }

    private bool IsFailed(AsyncOperationHandle handle)
    {
        if (!handle.IsValid())
        {
            Debug.LogError("유효성 검사 실패");
            return true;
        }

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("Status.Failed");
            return true;
        }
        return false;
    }
    #region Load
    //StageConfigSO
    public async Task LoadAllStageDB()
    {
        AsyncOperationHandle<IList<StageConfigSO>> handle 
            = Addressables.LoadAssetsAsync<StageConfigSO>("StageSO", null);

        await handle.Task;

        if (IsSucceeded(handle))
        {
            foreach (StageConfigSO data in handle.Result)
            {
                //중복 키 방지
                if (!_stageDB.ContainsKey(data.stageKey.ToString()))
                {
                    _stageDB.Add(data.stageKey.ToString(), data);
                }
            }
        }
        else if (IsFailed(handle))
        {
            Debug.LogError("LoadAllStageDB : Failed");
            return;
        }
    }

    //SceneInstance
    //LoadAssetsAsync<T> 사용할 경우 메모리 부담
    public void test()
    {
        
    }

    //RuleSO
    //MonsterSO
    //MonsterPrefab
    //ItemSO
    //ItemPrefab
    //VFX
    //SFX
    #endregion

    #region Get
    //stageNum T int -> string으로 형변환
    public StageConfigSO GetData(string stageNum)
    {
        if (_stageDB.TryGetValue(stageNum, out StageConfigSO data))
        {
            return data;
        }
        return null;
    }
    #endregion
    #endregion
}