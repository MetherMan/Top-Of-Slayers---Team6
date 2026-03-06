using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;

public class AddressableManager : Singleton<AddressableManager>
{
    #region field
    private List<AsyncOperationHandle> loadedAssets = new List<AsyncOperationHandle>();
    public Dictionary<string, StageConfigSO> _stageSO = new Dictionary<string, StageConfigSO>();
    public Dictionary<string, SceneInstance> _stageScene = new Dictionary<string, SceneInstance>();
    public Dictionary<string, WaveRule> _ruleSO = new Dictionary<string, WaveRule>();
    public Dictionary<string, GameObject> _uI = new Dictionary<string, GameObject>();
    public Dictionary<string, EnemyConfigSO> _enemySO = new Dictionary<string, EnemyConfigSO>();
    public Dictionary<string, GameObject> _monsterPf = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> _vFX = new Dictionary<string, GameObject>();

    Slider loadingBar;
    TextMeshProUGUI loadingText;
    float visualProgress = 0f;
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    //비동기 매서드 실행하기 위해서 async 필요
    //로드 매서드를 실행하지만 Unity 생명주기대로 기다리지 않고 실행된다.
    private async void Start()
    {
        Progress<float> progressHandle = new Progress<float>( value =>
        {
            visualProgress = Mathf.Lerp(visualProgress, value, 0.1f);
            loadingBar.value = visualProgress;
            loadingText.text = $"{visualProgress * 100}%";
        });

        await LoadAllData(progressHandle);
    }

    #region method
    public void TakeObject(Slider bar, TextMeshProUGUI text)
    {
        loadingBar = bar;
        loadingText = text;
    }

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
        if (!handle.IsValid() || handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError($"[Addressable Error] 유효성 {handle.IsValid()}, 상태 {handle.Status}");
            return true;
        }

        return false;
    }

    private async Task LoadAllData(IProgress<float> progress)
    {
        await CheckUpdate(progress);
        await DownloadWithCapacityUI("Preload", progress);

        //호출 리스트
        Task stageSOTask = LoadAllStageSO();
        Task ruleSOTask = LoadAllRule();
        Task uITask = LoadAllUI();
        Task monsterSOTask = LoadAllMonsterSO();
        Task monsterPfTask = LoadAllMonsterPf();
        //ItemSO
        //ItemPrefab
        Task vFXTask = LoadAllVFX();
        //SFX

        List<Task> tasks = new List<Task> 
        { stageSOTask, ruleSOTask, uITask, monsterSOTask, monsterPfTask, vFXTask };

        //로딩

        await Task.WhenAll(tasks);

        progress.Report(1.0f);
        visualProgress = 1.0f;

        await Task.Delay(2000);
        LoginUI.Instance.CompleteLoding();
        Debug.Log("모든 데이터 로드 완료");
    }

    private async Task DownloadWithCapacityUI(object key, IProgress<float> progress)
    {
        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(key);
        long totalBytes = await sizeHandle.Task;
        if (totalBytes > 0)
        {
            float totalMB = totalBytes / (1024f * 1024f);
            Debug.Log($"총 다운로드 용량 : {Math.Ceiling(totalMB * 100) / 100} MB");

            AsyncOperationHandle downloadHandle
                = Addressables.DownloadDependenciesAsync(key, true);
            /*
            UnityEngine.AddressableAssets.Utility.ResourceManagerDiagnostics.GenerateCompletedOperationDisplayName
            오류발생으로 핸들 유효성 체크 추가
            */
            while (downloadHandle.IsValid() && !downloadHandle.IsDone)
            {
                float currentMB = totalMB * downloadHandle.PercentComplete;

                loadingText.text = $"{Math.Ceiling(currentMB * 100) / 100}" +
                    $" / {Math.Ceiling(totalMB * 100) / 100} MB";

                await Task.Delay(100); // 무한 루프 방지 (Yield보다 cpu 점유율이 낮다)
            }

            if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("다운로드 완료");
            }
        }

        if (sizeHandle.IsValid())
        {
            Addressables.Release(sizeHandle);
        }

        progress.Report(0.5f);
        Debug.Log("다운로드 프로세스 종료");
    }

    private async Task CheckUpdate(IProgress<float> progress)
    {
        AsyncOperationHandle<List<string>> updateHandle
            = Addressables.CheckForCatalogUpdates(false);
        await updateHandle.Task;

        if (updateHandle.Result.Count > 0)
        {
            await Addressables.UpdateCatalogs(updateHandle.Result).Task;
            Debug.Log("카탈로그 업데이트");
        }
        Addressables.Release(updateHandle);
        progress.Report(0.2f);
    }

    #region Load
    //StageConfigSO : 게임을 종료할 때까지 가지고 있는다.
    private async Task LoadAllStageSO()
    {
        AsyncOperationHandle<IList<IResourceLocation>> loadResourceLocationHandle
            = Addressables.LoadResourceLocationsAsync("StageSO", typeof(StageConfigSO));

        await loadResourceLocationHandle.Task;

        if (IsFailed(loadResourceLocationHandle)) 
            Debug.LogError("LoadAllStageSO LoadResourceLocationsAsync : Failed");

        List<AsyncOperationHandle> stageSOOpList = new List<AsyncOperationHandle>();

        foreach (IResourceLocation location in loadResourceLocationHandle.Result)
        {
            AsyncOperationHandle<StageConfigSO> loadAssetHandle
                = Addressables.LoadAssetAsync<StageConfigSO>(location);

            loadAssetHandle.Completed += stageSOOp =>
            {
                if (IsSucceeded(stageSOOp))
                {
                    if (!_stageSO.ContainsKey(location.PrimaryKey))
                    {
                        _stageSO.Add(location.PrimaryKey, stageSOOp.Result);
                    }
                    else
                    {
                        Debug.LogWarning($"StageConfigSO 중복 : {location.PrimaryKey}");
                    }
                }
            };

            stageSOOpList.Add(loadAssetHandle);
        }

        AsyncOperationHandle stageSOGroupOp
            = Addressables.ResourceManager.CreateGenericGroupOperation(stageSOOpList);

        await stageSOGroupOp.Task;
        loadedAssets.Add(stageSOGroupOp);

        foreach (KeyValuePair<string, StageConfigSO> item in _stageSO)
        {
            Debug.Log($"stageSO Key : {item.Key} value : {item.Value.name}");
        }

        Debug.Log("LoadAllStageSO : Completed");
    }

    //SceneInstance
    //LoadAssetsAsync<T> 사용할 경우 메모리 부담 / 데이터 파일로 취급
    //스테이지 씬 이동 : SO의 키값을 입력하면 해당되는 씬을 불러와 실행한다.
    public AsyncOperationHandle<SceneInstance> RequestStageScene(string addressableName)
    {
        StageConfigSO data = GetStageData(addressableName);
        if (data == null) return default;

        return LoadSceneInternal(data.sceneReference);
    }

    public AsyncOperationHandle<SceneInstance> RequestScene(string addressableName)
    {
        return LoadSceneInternal(addressableName);
    }

    private AsyncOperationHandle<SceneInstance> LoadSceneInternal(object runtimeKey)
    {
        AsyncOperationHandle<SceneInstance> handle
            = Addressables.LoadSceneAsync(runtimeKey);

        handle.Completed += (h) =>
        {
            if (h.Status == AsyncOperationStatus.Succeeded)
            {
                AddSceneSafely(runtimeKey.ToString(), h.Result);
            }
            else
            {
                Debug.LogError($"Scene Load Failed : {runtimeKey}");
            }
        };

        return handle;
    }

    //중복방지 메서드
    private void AddSceneSafely(string address, SceneInstance instance)
    {
        if (_stageScene.ContainsKey(address))
        {
            Debug.Log("RequestScene 중복 데이터");
            return;
        }
        _stageScene.Add(address, instance);
    }

    //RuleSO
    private async Task LoadAllRule()
    {
        AsyncOperationHandle<IList<IResourceLocation>> loadResourceLocationHandle
            = Addressables.LoadResourceLocationsAsync("RuleSO", typeof(WaveRule));
        
        await loadResourceLocationHandle.Task;

        if (IsFailed(loadResourceLocationHandle)) 
            Debug.LogError("LoadAllRule LoadResourceLocation Failed");

        List<AsyncOperationHandle> ruleOpList = new List<AsyncOperationHandle>();

        foreach (IResourceLocation location in loadResourceLocationHandle.Result)
        {
            AsyncOperationHandle<WaveRule> loadAssetHandle
                = Addressables.LoadAssetAsync<WaveRule>(location);

            loadAssetHandle.Completed += op =>
            {
                if (IsSucceeded(op))
                {
                    if (!_ruleSO.ContainsKey(location.PrimaryKey))
                    {
                        _ruleSO.Add(location.PrimaryKey, op.Result);
                    }
                    else
                    {
                        Debug.LogWarning($"RuleSO 중복 : {location.PrimaryKey}");
                    }
                }
                else if (IsFailed(op))
                {
                    Debug.LogError("LoadAllRule Failed");
                }
            };
            ruleOpList.Add(loadAssetHandle);
        }

        AsyncOperationHandle ruleGroupOp
            = Addressables.ResourceManager.CreateGenericGroupOperation(ruleOpList);

        await ruleGroupOp.Task;
        loadedAssets.Add(ruleGroupOp);

        Addressables.Release(loadResourceLocationHandle);

        foreach (KeyValuePair<string, WaveRule> item in _ruleSO)
        {
            Debug.Log($"ruleSO Key : {item.Key} value : {item.Value.name}");
        }

        Debug.Log("LoadAllRule : Completed");
    }

    //UI
    private async Task LoadAllUI()
    {
        AsyncOperationHandle<IList<IResourceLocation>> loadResourceLocationHandle
            = Addressables.LoadResourceLocationsAsync("UI", typeof(GameObject));

        await loadResourceLocationHandle.Task;

        if (IsFailed(loadResourceLocationHandle)) Debug.LogError("loadResourceLocationHandle : Failed");

        List<AsyncOperationHandle> uIOpList = new List<AsyncOperationHandle>();

        foreach (IResourceLocation location in loadResourceLocationHandle.Result)
        {
            AsyncOperationHandle<GameObject> loadAssetHandle
                = Addressables.LoadAssetAsync<GameObject>(location); //타입은 동일하게 매개변수 참조를 location

            loadAssetHandle.Completed += op =>
            {
                if (IsSucceeded(op))
                {
                    if (!_uI.ContainsKey(location.PrimaryKey))
                    {
                        _uI.Add(location.PrimaryKey, op.Result);
                    }
                    else
                    {
                        Debug.LogWarning($"UI 중복 : {location.PrimaryKey}");
                    }
                }
                else if (IsFailed(op))
                {
                    Debug.LogError("LoadAllMonsterPf : Failed");
                }
            };

            uIOpList.Add(loadAssetHandle);
        }

        AsyncOperationHandle uIGroupOp
            = Addressables.ResourceManager.CreateGenericGroupOperation(uIOpList);

        await uIGroupOp.Task;
        loadedAssets.Add(uIGroupOp);

        Addressables.Release(loadResourceLocationHandle);

        foreach (KeyValuePair<string, GameObject> item in _uI)
        {
            Debug.Log($"UI Key : {item.Key} value : {item.Value.name}");
        }

        Debug.Log("LoadAllUI : Completed");
    }

    //MonsterSO
    private async Task LoadAllMonsterSO()
    {
        AsyncOperationHandle<IList<IResourceLocation>> loadResourceLocationHandle
            = Addressables.LoadResourceLocationsAsync("MonsterSO", typeof(EnemyConfigSO));

        await loadResourceLocationHandle.Task;

        if (IsFailed(loadResourceLocationHandle)) 
            Debug.LogError("LoadAllMonsterSO LoadResourceLocationsAsync IsValid() : Failed");

        List<AsyncOperationHandle> MonsterSOOpList = new List<AsyncOperationHandle>();

        foreach (IResourceLocation location in loadResourceLocationHandle.Result)
        {
            AsyncOperationHandle<EnemyConfigSO> loadAssetHandle
                = Addressables.LoadAssetAsync<EnemyConfigSO>(location);

            loadAssetHandle.Completed += op =>
            {
                if (IsSucceeded(op))
                {
                    if (!_enemySO.ContainsKey(location.PrimaryKey))
                    {
                        _enemySO.Add(location.PrimaryKey, op.Result);
                    }
                    else
                    {
                        Debug.LogWarning($"LoadAllMonsterSO 중복 : {location.PrimaryKey}");
                    }
                }
                else if (IsFailed(op))
                {
                    Debug.LogError("LoadAllMonesterSO : Failed");
                }
            };

            MonsterSOOpList.Add(loadAssetHandle);
        }

        AsyncOperationHandle MonsterSOGroupOp
            = Addressables.ResourceManager.CreateGenericGroupOperation(MonsterSOOpList);

        await MonsterSOGroupOp.Task;
        loadedAssets.Add(MonsterSOGroupOp);

        foreach (KeyValuePair<string, EnemyConfigSO> item in _enemySO)
        {
            Debug.Log($"MonsterSO Key : {item.Key} value : {item.Value.name}");
        }

        Debug.Log("LoadAllMonsterSO : Completed");
    }

    //MonsterPrefab
    private async Task LoadAllMonsterPf()
    {
        AsyncOperationHandle<IList<IResourceLocation>> loadResourceLocationHandle
            = Addressables.LoadResourceLocationsAsync("MonsterPrefab", typeof(GameObject));

        await loadResourceLocationHandle.Task;

        if (IsFailed(loadResourceLocationHandle)) Debug.LogError("loadResourceLocationHandle : Failed");

        List<AsyncOperationHandle> monsterPfOpList = new List<AsyncOperationHandle>();

        foreach (IResourceLocation location in loadResourceLocationHandle.Result)
        {
            AsyncOperationHandle<GameObject> loadAssetHandle
                = Addressables.LoadAssetAsync<GameObject>(location);

            loadAssetHandle.Completed += op =>
            {
                if (IsSucceeded(op))
                {
                    if (!_monsterPf.ContainsKey(location.PrimaryKey))
                    {
                        _monsterPf.Add(location.PrimaryKey, op.Result);
                    }
                    else
                    {
                        Debug.LogWarning($"MonsterPf 중복 : {location.PrimaryKey}");
                    }
                }
                else if (IsFailed(op))
                {
                    Debug.LogError("LoadAllMonsterPf : Failed");
                }

            };

            monsterPfOpList.Add(loadAssetHandle);
        }

        //create a GroupOperation to wait on all the above loads at once.
        AsyncOperationHandle monsterPfGroupOp
            = Addressables.ResourceManager.CreateGenericGroupOperation(monsterPfOpList);

        await monsterPfGroupOp.Task;
        loadedAssets.Add(monsterPfGroupOp);

        //ResourceLocation 위치 정보이기에 메모리를 지워도 데이터가 사라지지 않는다.
        Addressables.Release(loadResourceLocationHandle);

        foreach (KeyValuePair<string, GameObject> item in _monsterPf)
        {
            Debug.Log($"MonsterPf Key : {item.Key} value : {item.Value.name}");
        }

        Debug.Log("LoadAllMonsterPf : Completed");
    }

    //ItemSO
    //public async Task LoadAllItemSO()
    //{

    //}

    ////ItemPrefab
    //public async Task LoadAllItemPf()
    //{

    //}

    //VFX
    private async Task LoadAllVFX()
    {
        AsyncOperationHandle<IList<IResourceLocation>> loadResourceLocationHandle
            = Addressables.LoadResourceLocationsAsync("VFX", typeof(GameObject));

        await loadResourceLocationHandle.Task;

        if (IsFailed(loadResourceLocationHandle)) Debug.LogError("VFXHandle : Failed");

        List<AsyncOperationHandle> vFXOpList = new List<AsyncOperationHandle>();

        foreach (IResourceLocation location in loadResourceLocationHandle.Result)
        {
            AsyncOperationHandle<GameObject> loadLocationHandle
                = Addressables.LoadAssetAsync<GameObject>(location);

            loadLocationHandle.Completed += vFXOp =>
            {
                if (IsSucceeded(vFXOp))
                {
                    if (!_vFX.ContainsKey(location.PrimaryKey))
                    {
                        _vFX.Add(location.PrimaryKey, vFXOp.Result);
                    }
                    else
                    {
                        Debug.LogWarning($"VFX 중복 : {location.PrimaryKey}");
                    }
                }
                else if (IsFailed(vFXOp))
                {
                    Debug.LogError("LoadAllVFX : Failed");
                }
            };

            vFXOpList.Add(loadLocationHandle);
        }

        AsyncOperationHandle vFXGroupOp
            = Addressables.ResourceManager.CreateGenericGroupOperation(vFXOpList);
        
        await vFXGroupOp.Task;
        loadedAssets.Add(vFXGroupOp);

        Addressables.Release(loadResourceLocationHandle);

        foreach (KeyValuePair<string, GameObject> item in _vFX)
        {
            Debug.Log($"VFX Key : {item.Key} value : {item.Value.name}");
        }

        Debug.Log("LoadAllVFX : Completed");
    }

    //SFX
    //public async Task LoadAllSFX()
    //{

    //}
    #endregion

    #region Get
    public StageConfigSO GetStageData(string addressableName)
    {
        if (_stageSO.TryGetValue(addressableName, out StageConfigSO data))
        {
            return data;
        }
        return null;
    }

    public WaveRule GetRuleData(string addressableName)
    {
        if (_ruleSO.TryGetValue(addressableName, out WaveRule rule))
        {
            return rule;
        }
        return null;
    }

    public GameObject GetUI(string addressableName)
    {
        if (_uI.TryGetValue(addressableName, out GameObject ui))
        {
            return ui;
        }
        return null;
    }

    public EnemyConfigSO GetEnemyData(string addressableName)
    {
        if (_enemySO.TryGetValue(addressableName, out EnemyConfigSO data))
        {
            return data;
        }
        return null;
    }

    public GameObject GetEnemyPf(string addressableName)
    {
        if (_monsterPf.TryGetValue(addressableName, out GameObject data))
        {
            return data;
        }
        return null;
    }

    public GameObject GetVFX(string addressableName)
    {
        if (_vFX.TryGetValue(addressableName, out GameObject data))
        {
            return data;
        }
        return null;
    }
    #endregion

    #region Release
    //전체 메모리 할당 해제
    public void AllRelease()
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

    //개별 메모리 할당 해제
    #endregion
    #endregion
}