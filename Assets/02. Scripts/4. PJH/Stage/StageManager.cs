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
        itemDict = AddressableManager.Instance.GetAllItemSO();
    }

    public ItemSO GetItemByID(string id)
    {
        if (itemDict.ContainsKey(id)) 
        {
            return itemDict[id];
        } 
        else
        {
            Debug.LogWarningFormat("{0} : itemDict에 없는 데이터", id);
        }
        return null;
    }
    #endregion
}
