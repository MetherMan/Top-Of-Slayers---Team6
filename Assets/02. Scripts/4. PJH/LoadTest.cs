using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadTest : MonoBehaviour
{
    public AssetLabelReference monsterSO;
    public AssetLabelReference stageSO;

    void Awake()
    {
        //"MonsterSO" 라벨이 붙은 모든 ScriptableObject를 리스트로 로드
        Addressables.LoadAssetsAsync<ScriptableObject>(monsterSO, (data) =>
        {
            //각 에셋이 로드될 때마다 실행될 콜백
            Debug.Log($"{data.name} MonsterSO 로딩 중...");
        }).Completed += OnAllMonstersLoaded;

        //"StageSO" 라벨이 붙은 모든 ScriptalbeObject를 리스트로 로드
        Addressables.LoadAssetsAsync<ScriptableObject>(stageSO, (data) =>
        {
            Debug.Log($"{data.name} StageSO 로딩 중 ...");
        }).Completed += OnAllStagesLoaded;
    }

    private void OnAllMonstersLoaded(AsyncOperationHandle<IList<ScriptableObject>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            //로드된 모든 데이터를 리스트 형태로 반환
            IList<ScriptableObject> monsterList = handle.Result;
            Debug.Log($"총 {monsterList.Count}개의 몬스터 데이터를 서버에서 가져옴");
        }
    }

    private void OnAllStagesLoaded(AsyncOperationHandle<IList<ScriptableObject>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            IList<ScriptableObject> stageList = handle.Result;
            Debug.Log($"총 {stageList.Count}개의 스테이지 데이터를 서버에서 가져옴");
        }
    }
}
