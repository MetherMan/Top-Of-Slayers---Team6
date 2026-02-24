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
            Debug.LogError("❌ GachaManager 없음");
            yield break;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("❌ InventoryManager 없음");
            yield break;
        }

        if (gacha.TicketItem == null)
        {
            Debug.LogError("❌ TicketItem 연결 안됨");
            yield break;
        }

        // 테스트용 티켓 지급
        InventoryManager.Instance.AddItem(gacha.TicketItem, 1000);
        Debug.Log("✅ 가차 티켓 1000장 지급");

        int legend = 0;
        int epic = 0;
        int normal = 0;

        int total = 1000;

        for (int i = 0; i < total; i++)
        {
            ItemSO item = gacha.RollOne();

            if (item == null)
            {
                Debug.LogError($"❌ {i}번째 Draw null");
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

        Debug.Log($"🎯 결과 → Legend:{legend}, Epic:{epic}, Normal:{normal}");

        // 확률 검증
        if (legend < 30 || legend > 70)
            Debug.LogError("❌ Legend 확률 이상");

        if (epic < 120 || epic > 180)
            Debug.LogError("❌ Epic 확률 이상");

        if (normal < 750 || normal > 850)
            Debug.LogError("❌ Normal 확률 이상");

        Debug.Log("✅ 가차 확률 자동 테스트 종료");
    }
}