using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class AddressableManager : Singleton<AddressableManager>
{
    #region field
    private List<AsyncOperationHandle> loadedAssets = new List<AsyncOperationHandle>();
    private Dictionary<string, StageConfigSO> _stageDB = new Dictionary<string, StageConfigSO>();
    private Dictionary<string, SceneInstance> _stageScene = new Dictionary<string, SceneInstance>();
    #endregion

    protected override void Awake()
    {
        base.Awake();
        LoadAllData();
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

    public async Task LoadAllData()
    {
        //호출 리스트
        Task stageDBTask = LoadAllStageDB();
        //RuleSO
        //MonsterSO
        //MonsterPrefab
        //ItemSO
        //ItemPrefab
        //VFX
        //SFX

        await Task.WhenAll(stageDBTask);

        Debug.Log("모든 데이터 로드 완료");
    }

    #region Load
    //StageConfigSO : 게임을 종료할 때까지 가지고 있는다.
    public async Task LoadAllStageDB()
    {
        AsyncOperationHandle<IList<StageConfigSO>> stageDBhandle 
            = Addressables.LoadAssetsAsync<StageConfigSO>("StageSO", null);
        loadedAssets.Add(stageDBhandle);

        await stageDBhandle.Task;

        if (IsSucceeded(stageDBhandle))
        {
            foreach (StageConfigSO data in stageDBhandle.Result)
            {
                //중복 키 방지
                if (!_stageDB.ContainsKey(data.stageKey))
                {
                    _stageDB.Add(data.stageKey, data);
                }
            }
        }
        else if (IsFailed(stageDBhandle))
        {
            Debug.LogError("LoadAllStageDB : Failed");
            return;
        }
    }

    //SceneInstance
    //LoadAssetsAsync<T> 사용할 경우 메모리 부담 / 데이터 파일로 취급
    //스테이지 씬 이동 : SO의 키값을 입력하면 해당되는 씬을 불러와 실행한다.
    public void RequestStageScene(string key)
    {
        StageConfigSO data = AddressableManager.Instance.GetData(key);
        if (data == null) return;

        //AsyncOperationHandle<T> 타입을 명시하지 않으면 handle.Result는 object 타입으로 반환한다.
        AsyncOperationHandle<SceneInstance> loadOp = Addressables.LoadSceneAsync(data.sceneReference);

        loadOp.Completed += (handle) =>
        {
            if (IsSucceeded(handle))
            {
                //RuntimeKey를 제외할 경우 엉뚱한 값이 나올 수 있다
                string address = data.sceneReference.RuntimeKey.ToString();
                AddSceneSafely(address, handle.Result);
            }
            else if (IsFailed(handle))
            {
                Debug.LogError("RequestScene : Failed");
                return;
            }
        };
    }

    //중복방지 메서드
    public void AddSceneSafely(string address, SceneInstance instance)
    {
        if (_stageScene.ContainsKey(address))
        {
            // '==' 기호를 사용하려면 오버로드를 해줘야 사용할 수 있다.
            //.Equals는 object에 내장되어있는 비교함수이다.
            if (_stageScene[address].Equals(instance))
            {
                Debug.Log("RequestScene 중복 데이터");
                return;
            }
        }
        _stageScene[address] = instance;
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
    private void OnDestroy()
    {
        foreach (AsyncOperationHandle handle in loadedAssets)
        {
            //핸들이 유효한지 확인 (실패한 핸들도 유효; 대신 handle.Status = faild로 구분됨
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
    }
    #endregion
}