using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class AddressableManager : Singleton<AddressableManager>
{
    #region field
    private List<AsyncOperationHandle> loadedAssets = new List<AsyncOperationHandle>();
    private Dictionary<string, StageConfigSO> _stageSO = new Dictionary<string, StageConfigSO>();
    private Dictionary<string, StageDatabase> _database = new Dictionary<string, StageDatabase>();
    private Dictionary<string, SceneInstance> _stageScene = new Dictionary<string, SceneInstance>();
    private Dictionary<string, WaveRule> _ruleSO = new Dictionary<string, WaveRule>();
    private Dictionary<string, GameObject> _uI = new Dictionary<string, GameObject>();
    private Dictionary<string, EnemyConfigSO> _enemySO = new Dictionary<string, EnemyConfigSO>();
    private Dictionary<string, GameObject> _monsterPf = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _vFX = new Dictionary<string, GameObject>();
    private Dictionary<string, ItemSO> _itemSO = new Dictionary<string, ItemSO>();
    //private Dictionary<string, Material> _uIMat = new Dictionary<string, Material>();
    //private Dictionary<string, TMP_FontAsset> _uIAsset = new Dictionary<string, TMP_FontAsset>();
    //private Dictionary<string, Sprite> _uISprite = new Dictionary<string, Sprite>();
    //private Dictionary<string, Font> _uIFont = new Dictionary<string, Font>();
    //private Dictionary<string, Shader> _uIShader = new Dictionary<string, Shader>();

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

        bool isSuccess = await LoadAllData(progressHandle);

        //ItemSO 다 가져오기 : 계정 로그인 시 활성화 될 메서드에 필요한 데이터
        if (isSuccess)
        {
            StageManager.Instance.LoadAllItemSO();
        }
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

    private async UniTask<bool> LoadAllData(IProgress<float> progress)
    {
        try
        {
            await CheckUpdate("Preload", progress);
            await UniTask.NextFrame();

            await DownloadWithCapacityUI("Preload", progress);
            await UniTask.NextFrame();

            //호출 리스트
            UniTask databaseTask = LoadDatabase();
            UniTask stageSOTask = LoadAllStageSO();
            UniTask ruleSOTask = LoadAllRule();
            UniTask uITask = LoadAllUI();
            UniTask monsterSOTask = LoadAllMonsterSO();
            UniTask monsterPfTask = LoadAllMonsterPf();
            UniTask itemSOTask = LoadAllItemSO();
            //ItemPrefab
            //UniTask uIResourceTask = LoadAllUIResource();
            UniTask vFXTask = LoadAllVFX();
            //SFX

            List<UniTask> tasks = new List<UniTask> 
            { 
                databaseTask, stageSOTask, ruleSOTask,
                uITask, monsterSOTask, monsterPfTask,
                itemSOTask, vFXTask
            };

            await UniTask.WhenAll(tasks);
            progress.Report(1.0f);
            visualProgress = 1.0f;

            await UniTask.Delay(2000);
            LoginUI.Instance.CompleteLoding();
            Debug.Log("<color=white>모든 데이터 로드 완료</color>");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"로드중 에러 발생 :" + ex.Message);
            return false;
        }
    }

    private async UniTask DownloadWithCapacityUI(object key, IProgress<float> progress)
    {
        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(key);
        long totalBytes = await sizeHandle.Task;

        if (totalBytes > 0)
        {
            float totalMB = totalBytes / (1024f * 1024f);

            //정밀도 손상? Mathf -> Math
            LoginUI.Instance.downloadText.text
                = $"필수 리소스 {Math.Ceiling(totalMB * 100) / 100}MB \n" +
                    $"다운로드가 필요합니다";
            LoginUI.Instance.DownloadUIOpen();

            await UniTask.WaitUntil(() => LoginSceneManager.Instance.confirmDownload);


            IProgress<float> progressProvider = Progress.Create<float>(p =>
            {
                float currentMB = totalMB * p;

                loadingText.text = $"{Math.Ceiling(currentMB * 100) / 100}" +
                    $" / {Math.Ceiling(totalMB * 100) / 100} MB";
            });

            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(key, true);

            try
            {
                await downloadHandle.ToUniTask(progress: progressProvider, autoReleaseWhenCanceled: false);

                if (IsSucceeded(downloadHandle))
                {
                    Debug.Log("다운로드 완료");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"다운로드 실패 : {downloadHandle.OperationException}");
                LoginUI.Instance.confirmText.text = downloadHandle.OperationException.ToString();
                LoginUI.Instance.ConfirmUIOpen();
            }
        }

        if (sizeHandle.IsValid())
        {
            Addressables.Release(sizeHandle);
        }

        progress.Report(0.5f);
        Debug.Log("다운로드 프로세스 종료");
    }

    private async UniTask CheckUpdate(object key, IProgress<float> progress)
    {
        AsyncOperationHandle<List<string>> updateHandle
            = Addressables.CheckForCatalogUpdates(false);
        await updateHandle.Task;

        if (updateHandle.Result.Count > 0)
        {
            await Addressables.UpdateCatalogs(updateHandle.Result).Task;
            Debug.Log("카탈로그 업데이트");
        }
        else
        {
            Debug.Log("업데이트 없음");
        }

        progress.Report(0.2f);
        Addressables.Release(updateHandle);
    }

    #region Load
    //StageConfigSO : 게임을 종료할 때까지 가지고 있는다.
    private async UniTask LoadAllStageSO()
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

        //foreach (KeyValuePair<string, StageConfigSO> item in _stageSO)
        //{
        //    Debug.Log($"stageSO Key : {item.Key} value : {item.Value.name}");
        //}

        Debug.Log("LoadAllStageSO : Completed");
    }

    private async UniTask LoadDatabase()
    {
        AsyncOperationHandle<IList<IResourceLocation>> loadResourceLocationsHandle
            = Addressables.LoadResourceLocationsAsync("Database" , typeof(StageDatabase));

        await loadResourceLocationsHandle.Task;

        if (IsFailed(loadResourceLocationsHandle))
            Debug.LogError("loadResourceLocationhandle : failed");

        List<AsyncOperationHandle> opList = new List<AsyncOperationHandle>();

        foreach (IResourceLocation location in loadResourceLocationsHandle.Result)
        {
            AsyncOperationHandle<StageDatabase> loadAssetHandle
                = Addressables.LoadAssetAsync<StageDatabase>(location);

            loadAssetHandle.Completed += op => 
            { 
                if (IsSucceeded(op))
                {
                    if (!_database.ContainsKey(location.PrimaryKey))
                    {
                        _database.Add(location.PrimaryKey, op.Result);
                    }
                    else
                    {
                        Debug.LogWarning($"database 중복 : {location.PrimaryKey}");
                    }
                }
                else
                {
                    Debug.LogError("LoadDatabase : Failed");
                }
            };

            opList.Add(loadAssetHandle);
        }

        AsyncOperationHandle<IList<AsyncOperationHandle>> opGroup
            = Addressables.ResourceManager.CreateGenericGroupOperation(opList);

        await opGroup.Task;
        loadedAssets.Add(opGroup);

        Addressables.Release(loadResourceLocationsHandle);

        //foreach (KeyValuePair<string, StageDatabase> item in _database)
        //{
        //    Debug.LogFormat($"StageDatabase key : {0}, value : {1}", item.Key, item.Value);
        //}

        Debug.Log("LoadDatabase : Completed");
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
                Debug.LogFormat("<color=yellow> 씬 runtimeKey : {0}</color>", runtimeKey);
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
    private async UniTask LoadAllRule()
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

        //foreach (KeyValuePair<string, WaveRule> item in _ruleSO)
        //{
        //    Debug.Log($"ruleSO Key : {item.Key} value : {item.Value.name}");
        //}

        Debug.Log("LoadAllRule : Completed");
    }

    //UI
    private async UniTask LoadAllUI()
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

        //foreach (KeyValuePair<string, GameObject> item in _uI)
        //{
        //    Debug.Log($"UI Key : {item.Key} value : {item.Value.name}");
        //}

        Debug.Log("LoadAllUI : Completed");
    }

    //MonsterSO
    private async UniTask LoadAllMonsterSO()
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

        Addressables.Release(loadResourceLocationHandle);

        //foreach (KeyValuePair<string, EnemyConfigSO> item in _enemySO)
        //{
        //    Debug.Log($"MonsterSO Key : {item.Key} value : {item.Value.name}");
        //}

        Debug.Log("LoadAllMonsterSO : Completed");
    }

    //MonsterPrefab
    private async UniTask LoadAllMonsterPf()
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

        //foreach (KeyValuePair<string, GameObject> item in _monsterPf)
        //{
        //    Debug.Log($"MonsterPf Key : {item.Key} value : {item.Value.name}");
        //}

        Debug.Log("LoadAllMonsterPf : Completed");
    }

    //ItemSO
    private async UniTask LoadAllItemSO()
    {
        AsyncOperationHandle<IList<IResourceLocation>> loadResourceLocationHandle
            = Addressables.LoadResourceLocationsAsync("ItemSO", typeof(ItemSO));

        await loadResourceLocationHandle.Task;

        if (IsFailed(loadResourceLocationHandle)) Debug.LogError("loadResourceLocationHandle : Failed");

        List<AsyncOperationHandle> ItemSOOpList = new List<AsyncOperationHandle>();

        foreach (IResourceLocation location in loadResourceLocationHandle.Result)
        {
            AsyncOperationHandle<ItemSO> loadAssetHandle
                = Addressables.LoadAssetAsync<ItemSO>(location);

            loadAssetHandle.Completed += (op) =>
            {
                if (IsSucceeded(op))
                {
                    if (!_itemSO.ContainsKey(location.PrimaryKey))
                    {
                        _itemSO.Add(location.PrimaryKey, op.Result);
                    }
                    else
                    {
                        Debug.LogWarning($"ItemSO 중복 : {location.PrimaryKey}");
                    }
                }
                else if (IsFailed(op))
                {
                    Debug.LogError("LoadAllItemSO : Failed");
                }
            };
            ItemSOOpList.Add(loadAssetHandle);
        }

        AsyncOperationHandle ItemSOGroup
            = Addressables.ResourceManager.CreateGenericGroupOperation(ItemSOOpList);

        await ItemSOGroup.Task;
        loadedAssets.Add(ItemSOGroup);

        Addressables.Release(loadResourceLocationHandle);

        //foreach (KeyValuePair<string, ItemSO> item in _itemSO)
        //{
        //    Debug.Log($"ItemSO Key : {item.Key} value : {item.Value.name}");
        //}

        Debug.Log("LoadAllItemSO : Completed");
    }

    ////ItemPrefab
    //public async Task LoadAllItemPf()
    //{
    //프리팹 추가로 코드 구현
    //}

    //VFX
    private async UniTask LoadAllVFX()
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

        //foreach (KeyValuePair<string, GameObject> item in _vFX)
        //{
        //    Debug.Log($"VFX Key : {item.Key} value : {item.Value.name}");
        //}

        Debug.Log("LoadAllVFX : Completed");
    }

    //SFX
    //public async Task LoadAllSFX()
    //{

    //}

    //UIResource
    //private async UniTask LoadAllUIResource()
    //{
    //    AsyncOperationHandle<IList<IResourceLocation>> handle
    //        = Addressables.LoadResourceLocationsAsync("UIResource");

    //    await handle.Task;
    //    if (IsFailed(handle)) Debug.LogError("LoadAllUIResource : Failed");

    //    List<AsyncOperationHandle> opList = new List<AsyncOperationHandle>();

    //    foreach (IResourceLocation location in handle.Result)
    //    {
    //        string path = location.InternalId;
    //        if (path.EndsWith(".mat"))
    //        {
    //            AsyncOperationHandle<Material> loadLocationHandle
    //                = Addressables.LoadAssetAsync<Material>(location);
    //            opList.Add(loadLocationHandle);

    //            await loadLocationHandle.Task;

    //            if (IsFailed(loadLocationHandle)) Debug.LogError("LoadUIResource : Failed");

    //            loadLocationHandle.Completed += (loadHandle) =>
    //            {
    //                if (!_uIMat.ContainsKey(location.PrimaryKey))
    //                {
    //                    _uIMat.Add(location.PrimaryKey, loadHandle.Result);
    //                }
    //                else
    //                {
    //                    Debug.LogWarningFormat("UIResource 중복 : {0}", location.PrimaryKey);
    //                }
    //            };
    //        }
    //        else if (path.EndsWith(".asset"))
    //        {
    //            AsyncOperationHandle<TMPro.TMP_FontAsset> loadLocationHandle
    //                = Addressables.LoadAssetAsync<TMPro.TMP_FontAsset>(location);
    //            opList.Add(loadLocationHandle);

    //            await loadLocationHandle.Task;

    //            if (IsFailed(loadLocationHandle)) Debug.LogError("LoadUIResource : Failed");

    //            loadLocationHandle.Completed += (loadHandle) =>
    //            {
    //                if (!_uIAsset.ContainsKey(location.PrimaryKey))
    //                {
    //                    _uIAsset.Add(location.PrimaryKey, loadHandle.Result);
    //                }
    //                else
    //                {
    //                    Debug.LogWarningFormat("UIResource 중복 : {0}", location.PrimaryKey);
    //                }
    //            };
    //        }
    //        else if (path.EndsWith(".png"))
    //        {
    //            AsyncOperationHandle<Sprite> loadLocationHandle
    //                = Addressables.LoadAssetAsync<Sprite>(location);
    //            opList.Add(loadLocationHandle);

    //            await loadLocationHandle.Task;

    //            if (IsFailed(loadLocationHandle)) Debug.LogError("LoadUIResource : Failed");

    //            loadLocationHandle.Completed += (loadHandle) =>
    //            {
    //                if (!_uISprite.ContainsKey(location.PrimaryKey))
    //                {
    //                    _uISprite.Add(location.PrimaryKey, loadHandle.Result);
    //                }
    //                else
    //                {
    //                    Debug.LogWarningFormat("UIResource 중복 : {0}", location.PrimaryKey);
    //                }
    //            };
    //        }
    //        else if (path.EndsWith(".ttf"))
    //        {
    //            AsyncOperationHandle<Font> loadLocationHandle
    //                = Addressables.LoadAssetAsync<Font>(location);
    //            opList.Add(loadLocationHandle);

    //            await loadLocationHandle.Task;

    //            if (IsFailed(loadLocationHandle)) Debug.LogError("LoadUIResource : Failed");

    //            loadLocationHandle.Completed += (loadHandle) =>
    //            {
    //                if (!_uIFont.ContainsKey(location.PrimaryKey))
    //                {
    //                    _uIFont.Add(location.PrimaryKey, loadHandle.Result);
    //                }
    //                else
    //                {
    //                    Debug.LogWarningFormat("UIResource 중복 : {0}", location.PrimaryKey);
    //                }
    //            };
    //        }
    //        else if (path.EndsWith(".shader"))
    //        {
    //            AsyncOperationHandle<Shader> loadLocationHandle
    //                = Addressables.LoadAssetAsync<Shader>(location);
    //            opList.Add(loadLocationHandle);

    //            await loadLocationHandle.Task;

    //            if (IsFailed(loadLocationHandle)) Debug.LogError("LoadUIResource : Failed");

    //            loadLocationHandle.Completed += (loadHandle) =>
    //            {
    //                if (!_uIShader.ContainsKey(location.PrimaryKey))
    //                {
    //                    _uIShader.Add(location.PrimaryKey, loadHandle.Result);
    //                }
    //                else
    //                {
    //                    Debug.LogWarningFormat("UIResource 중복 : {0}", location.PrimaryKey);
    //                }
    //            };

    //        }
    //    };

    //    AsyncOperationHandle opListGroup
    //        = Addressables.ResourceManager.CreateGenericGroupOperation(opList);

    //    await opListGroup.Task;

    //    loadedAssets.Add(opListGroup);

    //    Addressables.Release(handle);

    //    foreach (KeyValuePair<string, TMP_FontAsset> item in _uIAsset)
    //    {
    //        Debug.Log($"<color=blue>uIAsset Key : {item.Key} value : {item.Value.name}</color>");
    //    }

    //    foreach (KeyValuePair<string, Material> item in _uIMat)
    //    {
    //        Debug.Log($"<color=blue>uIMat Key : {item.Key} value : {item.Value.name}</color>");
    //    }

    //    foreach (KeyValuePair<string, Font> item in _uIFont)
    //    {
    //        Debug.Log($"<color=blue>uIFont Key : {item.Key} value : {item.Value.name}</color>");
    //    }

    //    foreach (KeyValuePair<string, Shader> item in _uIShader)
    //    {
    //        Debug.Log($"<color=blue>uIShader Key : {item.Key} value : {item.Value.name}</color>");
    //    }

    //    foreach (KeyValuePair<string, Sprite> item in _uISprite)
    //    {
    //        Debug.Log($"<color=blue>uISprite Key : {item.Key} value : {item.Value.name}</color>");
    //    }

    //    //RefreshUI();

    //    Debug.Log("LoadUIResource : Completed");
    //}

    //private void RefreshUI()
    //{
    //    //(true) 비활성화 객체 포함 UI SetActive:False되있는 것도 포함시킬 때 쓰는 용도랑 동일
    //    TextMeshProUGUI[] allTexts = FindObjectsOfType<TMPro.TextMeshProUGUI>(true);
    //    foreach (TextMeshProUGUI text in allTexts)
    //    {
    //        if (text.font != null && _uIAsset.TryGetValue(text.font.name, out TMP_FontAsset loadedFont))
    //        {
    //            text.font = loadedFont; //폰트 강제로 재할당
    //            text.fontSharedMaterial = loadedFont.material; //기본 재질 강제로 재할당
    //        }
    //    }

    //    TMP_SubMeshUI[] subMeshes = FindObjectsOfType<TMP_SubMeshUI>(true);
    //    foreach (TMP_SubMeshUI sub in subMeshes)
    //    {

    //    }

    //    Debug.Log("Font 갱신");
    //}
    #endregion

    #region Get
    public StageDatabase GetDatabase(string addressableName)
    {
        if (_database.TryGetValue(addressableName, out StageDatabase data))
        {
            return data;
        }
        else
        {
            Debug.LogWarning("AddressableManager에 존재하지 않는 데이터");
        }
        return null;
    }

    public StageConfigSO GetStageData(string addressableName)
    {
        if (_stageSO.TryGetValue(addressableName, out StageConfigSO data))
        {
            return data;
        }
        else
        {
            Debug.LogWarning("AddressableManager에 존재하지 않는 데이터");
        }
        return null;
    }

    public WaveRule GetRuleData(string addressableName)
    {
        if (_ruleSO.TryGetValue(addressableName, out WaveRule rule))
        {
            return rule;
        }
        else
        {
            Debug.LogWarning("AddressableManager에 존재하지 않는 데이터");
        }
        return null;
    }

    public GameObject GetUI(string addressableName)
    {
        if (_uI.TryGetValue(addressableName, out GameObject ui))
        {
            return ui;
        }
        else
        {
            Debug.LogWarning("AddressableManager에 존재하지 않는 데이터");
        }
        return null;
    }

    public EnemyConfigSO GetEnemyData(string addressableName)
    {
        if (_enemySO.TryGetValue(addressableName, out EnemyConfigSO data))
        {
            return data;
        }
        else
        {
            Debug.LogWarning("AddressableManager에 존재하지 않는 데이터");
        }
        return null;
    }

    public GameObject GetEnemyPf(string addressableName)
    {
        if (_monsterPf.TryGetValue(addressableName, out GameObject data))
        {
            return data;
        }
        else
        {
            Debug.LogWarning("AddressableManager에 존재하지 않는 데이터");
        }
        return null;
    }

    public GameObject GetVFX(string addressableName)
    {
        if (_vFX.TryGetValue(addressableName, out GameObject data))
        {
            return data;
        }
        else
        {
            Debug.LogWarning("AddressableManager에 존재하지 않는 데이터");
        }
        return null;
    }

    public Dictionary<string, ItemSO> GetAllItemSO()
    {
        Dictionary<string, ItemSO> data = new Dictionary<string, ItemSO>();
        foreach (KeyValuePair<string, ItemSO> item in _itemSO)
        {
            data.Add(item.Key, item.Value);
        }

        return data;
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