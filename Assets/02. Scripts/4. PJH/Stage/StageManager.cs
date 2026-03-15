using System.Collections.Generic;
using UnityEngine;

/*
    !!! 0306 : 코드 엎음
  
    StageDatabase에서 StageSO를 가져온다
*/
public class StageManager : Singleton<StageManager>
{
    #region field
    public StageDatabase stageDB;
    public StageConfigSO selectDB;

    public System.Action LoadCompleted;

    private Dictionary<string, ItemSO> itemDict = new Dictionary<string, ItemSO>();
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    #region method
    public void GetData(StageConfigSO stageConfigSO, StageDatabase database)
    {
        stageDB = database;
        selectDB = stageConfigSO;
    }

    //AddressableManager가 데이터를 다 받아오고 난 뒤 실행
    public void LoadAllItemSO()
    {
        Dictionary<string, ItemSO> data = AddressableManager.Instance.GetAllItemSO();
        
        if (data != null && data.Count > 0)
        {
            foreach (ItemSO item in data.Values)
            {
                itemDict.Add(item.itemName, item);
            }
        }
        Debug.LogFormat("<color=cyan>StageManager: {0}개 아이템 SO 로드 완료</color>", data.Count);
    }

    public ItemSO GetItemByID(string id)
    {
        if (itemDict != null && itemDict.TryGetValue(id, out ItemSO data)) 
        {
            return data;
        } 
        
        Debug.LogWarningFormat("{0} : itemDict에 없는 데이터", id);
        return null;
    }
    #endregion
}
