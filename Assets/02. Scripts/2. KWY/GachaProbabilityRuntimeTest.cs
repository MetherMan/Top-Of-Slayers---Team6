using UnityEngine;
using System.Collections;

public class GachaProbabilityRuntimeTest : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);

        var gacha = FindObjectOfType<GachaManager>();

        if (gacha == null)
        {
            Debug.LogError("GachaManager 없음");
            yield break;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager 없음");
            yield break;
        }

        if (gacha.TicketItem == null)
        {
            Debug.LogError("❌ TicketItem 연결 안됨");
            yield break;
        }

        // 테스트용 티켓 지급
        InventoryManager.Instance.AddItem(gacha.TicketItem, 1000);
        Debug.Log("가차 티켓 1000장 지급");

        int legend = 0;
        int epic = 0;
        int normal = 0;

        int total = 1000;

        for (int i = 0; i < total; i++)
        {
            ItemSO item = gacha.RollOne();

            if (item == null)
            {
                Debug.LogError($"{i}번째 Draw null");
                yield break;
            }

            switch (item.grade)
            {
                case Grade.Legend:
                    legend++;
                    break;

                case Grade.Epic:
                    epic++;
                    break;

                case Grade.Normal:
                    normal++;
                    break;
            }
        }

        // ===== 실제 확률 계산 =====
        float legendRate = legend / (float)total; //0.049
        float epicRate = epic / (float)total;
        float normalRate = normal / (float)total;

        Debug.Log($" 결과");
        Debug.Log($"Legend: {legend} ({legendRate:P2})");
        Debug.Log($"Epic:   {epic} ({epicRate:P2})");
        Debug.Log($"Normal: {normal} ({normalRate:P2})");

        // ===== 목표 확률 (GachaDataSO 기준) =====
        float targetLegend = 0.05f;
        float targetEpic = 0.15f;
        float targetNormal = 0.80f;

        // ===== 허용 오차 =====
        float legendTolerance = 0.02f; // ±2%
        float epicTolerance = 0.03f; // ±3%
        float normalTolerance = 0.03f; // ±3%

        bool legendOK = Mathf.Abs(legendRate - targetLegend) < legendTolerance;
        bool epicOK = Mathf.Abs(epicRate - targetEpic) < epicTolerance;
        bool normalOK = Mathf.Abs(normalRate - targetNormal) < normalTolerance;

        if (!legendOK) Debug.LogError(" Legend 확률 이상");
        if (!epicOK) Debug.LogError(" Epic 확률 이상");
        if (!normalOK) Debug.LogError(" Normal 확률 이상");

        if (legendOK && epicOK && normalOK)
            Debug.Log(" 가차 확률 자동 테스트 통과");

        Debug.Log(" 가차 확률 자동 테스트 종료");
    }
}