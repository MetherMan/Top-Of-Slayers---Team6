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

    float completeCount;
    Slider loadingBar;
    TextMeshProUGUI loadingText;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        TakeObject();
    }

    //비동기 매서드 실행하기 위해서 async 필요
    //로드 매서드를 실행하지만 Unity 생명주기대로 기다리지 않고 실행된다.
    private async void Start()
    {
        Progress<float> progressHandle = new Progress<float>( value =>
        {
            loadingBar.value = value;
        });

        await LoadAllData(progressHandle);
    }

    #region method
    private void TakeObject()
    {
        loadingBar = GameObject.Find("Canvas/Background/LoadingBar")
            .GetComponent<Slider>();
        loadingText = GameObject.Find("Canvas/Background/LoadingBar/LoadingText")
            .GetComponent<TextMeshProUGUI>();
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
        while (completeCount < tasks.Count)
        {
            loadingText.text = $"{completeCount / tasks.Count}%";
            progress.Report(completeCount);
            await Task.WhenAll(tasks);
        }

        Debug.Log("모든 데이터 로드 완료");
    }

    #region Load
    //StageConfigSO : 게임을 종료할 때까지 가지고 있는다.
    public async Task LoadAllStageSO()
    {
        AsyncOperationHandle<IList<StageConfigSO>> stageSOHandle 
            = Addressables.LoadAssetsAsync<StageConfigSO>("StageSO", null);
        loadedAssets.Add(stageSOHandle);

        await stageSOHandle.Task;

        if (IsSucceeded(stageSOHandle))
        {
            foreach (StageConfigSO data in stageSOHandle.Result)
            {
                //중복 키 방지
                if (!_stageSO.ContainsKey(data.stageKey))
                {
                    _stageSO.Add(data.stageKey, data);
                    Debug.Log($"stageConfigSO 등록완료 : {data.stageKey}");
                }
                else
                {
                    Debug.LogWarning($"동일한 이름의 stageSO가 존재합니다. : {data.stageKey}");
                }
            }
        }
        else if (IsFailed(stageSOHandle))
        {
            Debug.LogError("LoadAllStageDB : Failed");
            return;
        }
        completeCount++;
    }

    //SceneInstance
    //LoadAssetsAsync<T> 사용할 경우 메모리 부담 / 데이터 파일로 취급
    //스테이지 씬 이동 : SO의 키값을 입력하면 해당되는 씬을 불러와 실행한다.
    public void RequestStageScene(string key)
    {
        StageConfigSO data = GetStageData(key);
        if (data == null) return;

        //AsyncOperationHandle<T> 타입을 명시하지 않으면 handle.Result는 object 타입으로 반환한다.
        AsyncOperationHandle<SceneInstance> sceneLoadHandle
            = Addressables.LoadSceneAsync(data.sceneReference);

        /*
            씬인스턴스 핸들은 별도로 관리하지 않는다.
            - single로만 전환하기 때문에 자동으로 sceneLoadHandle(핸들)을 Relases 해준다.
            - InValid()로 검사를 해서 오류가 발생하지 않지만 불필요한 행동이다.
            - 클라이언트가 정리될 경우 시스템에서 메모리를 전부 해제하므로 로딩중 클라이언트가
                + 강제 종료되는 상황은 상정하지 않아도 된다.

            람다식으로 이벤트를 구독하기에 매개변수명으로 사용한 handle은 추가하는 순간 메모리에서 삭제된다.
            즉 -= 구독해제 구문을 작성할 필요가 없다 할 수도 없고
        */ //리스트에 따로 .Add 하지 않는 이유
        /*
            Completed 이벤트는 매개변수 1개만 사용한다.
            작업이 끝나면 해당 작업의 결과정보(handle)를 통째로 넘겨주자 라고 정의되어 있기 때문이다.
        */ //.Completed
        sceneLoadHandle.Completed += (handle) =>
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
            Debug.Log("RequestScene 중복 데이터");
            return;
        }
        _stageScene.Add(address, instance);
    }

    //RuleSO
    public async Task LoadAllRule()
    {
        AsyncOperationHandle<IList<WaveRule>> ruleDbHandle
            = Addressables.LoadAssetsAsync<WaveRule>("RuleSO", null);
        loadedAssets.Add(ruleDbHandle);
        
        await ruleDbHandle.Task;

        if (IsSucceeded(ruleDbHandle))
        {
            foreach (WaveRule rule in ruleDbHandle.Result)
            {
                if (!_ruleSO.ContainsKey(rule.RuleType))
                {
                    _ruleSO.Add(rule.RuleType, rule);
                    Debug.Log($"rule 등록 완료 : {rule.RuleType}");
                }
                else
                {
                    Debug.LogWarning($"동일한 이름의 rule이 존재합니다. : {rule.RuleType}");
                }
            }
        }
        else if (IsFailed(ruleDbHandle))
        {
            Debug.LogError("LoadAllRule : Failed");
            return;
        }

        completeCount++;
    }

    //UI
    public async Task LoadAllUI()
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
                    _uI.Add(location.PrimaryKey, op.Result);
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

        completeCount++;
    }

    //MonsterSO
    public async Task LoadAllMonsterSO()
    {
        AsyncOperationHandle<IList<EnemyConfigSO>> MonsterSOHandle
            = Addressables.LoadAssetsAsync<EnemyConfigSO>("MonsterSO", null);
        loadedAssets.Add(MonsterSOHandle);

        await MonsterSOHandle.Task;

        if (IsSucceeded(MonsterSOHandle))
        {
            foreach (EnemyConfigSO monsterSO in MonsterSOHandle.Result)
            {
                if (!_enemySO.ContainsKey(monsterSO.MonsterName))
                {
                    _enemySO.Add(monsterSO.MonsterName, monsterSO);
                    Debug.Log($"monsterConfigSO 등록완료 : {monsterSO.MonsterName}");
                }
                else
                {
                    Debug.LogWarning($"동일한 이름의 monsterSO가 존재합니다. : {monsterSO.MonsterName}");
                }
            }
        }
        else if (IsFailed(MonsterSOHandle))
        {
            Debug.LogError("LoadAllMonsterSO : Failed");
            return;
        }

        completeCount++;
    }

    //MonsterPrefab
    public async Task LoadAllMonsterPf()
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
                    _monsterPf.Add(location.PrimaryKey, op.Result);
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

        foreach (var item in _monsterPf)
        {
            Debug.Log(item.Key + " - " + item.Value.name);
        }

        completeCount++;
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
    public async Task LoadAllVFX()
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
                    _vFX.Add(location.PrimaryKey, vFXOp.Result);
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

        Addressables.Release(loadResourceLocationHandle);

        completeCount++;
    }

    //SFX
    //public async Task LoadAllSFX()
    //{

    //}
    #endregion

    #region Get
    public StageConfigSO GetStageData(string stageKey)
    {
        if (_stageSO.TryGetValue(stageKey, out StageConfigSO data))
        {
            return data;
        }
        return null;
    }

    public WaveRule GetRuleData(string ruleType)
    {
        if (_ruleSO.TryGetValue(ruleType, out WaveRule rule))
        {
            return rule;
        }
        return null;
    }

    public GameObject GetUI(string uIname)
    {
        if (_uI.TryGetValue(uIname, out GameObject ui))
        {
            return ui;
        }
        return null;
    }

    public EnemyConfigSO GetEnemyData(string monsterName)
    {
        if (_enemySO.TryGetValue(monsterName, out EnemyConfigSO data))
        {
            return data;
        }
        return null;
    }

    public GameObject GetEnemyPf(string monsterName)
    {
        if (_monsterPf.TryGetValue(monsterName, out GameObject data))
        {
            return data;
        }
        return null;
    }

    public GameObject GetVFX(string vFXName)
    {
        if (_vFX.TryGetValue(vFXName, out GameObject data))
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