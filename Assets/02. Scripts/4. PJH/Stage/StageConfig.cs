/*
    *싱글톤
    StageConfigSO 데이터 저장
    StageManager 연동
*/

public class StageConfig
{
    #region field
    static StageConfig instance;
    public static StageConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new StageConfig();
            }
            return instance;
        }
    }

    StageConfig() { }

    public StageConfigSO stageConfigSO;
    #endregion

    #region method
    public void AddData()
    {

    }

    public void RemoveData()
    {

    }
    #endregion
}