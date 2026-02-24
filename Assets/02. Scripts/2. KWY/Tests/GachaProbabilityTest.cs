using UnityEditor;
using UnityEngine;
using System.Linq;

public class GachaProbabilityEditorTest
{
    const int SIM = 20000;

    [MenuItem("QA/Test Gacha Probability")]
    static void RunTest()
    {
        string guid = AssetDatabase.FindAssets("t:GachaDataSO").FirstOrDefault();
        if (guid == null)
        {
            Debug.LogError("[QA] GachaDataSO not found");
            return;
        }

        var data = AssetDatabase.LoadAssetAtPath<GachaDataSO>(
            AssetDatabase.GUIDToAssetPath(guid));

        int legend = 0;
        int epic = 0;
        int normal = 0;

        for (int i = 0; i < SIM; i++)
        {
            var item = Roll(data);

            if (item.grade == Grade.Legend) legend++;
            else if (item.grade == Grade.Epic) epic++;
            else if (item.grade == Grade.Normal) normal++;
        }

        float legendRate = (float)legend / SIM * 100f;
        float epicRate = (float)epic / SIM * 100f;
        float normalRate = (float)normal / SIM * 100f;

        Debug.Log(
            $"[QA] Result ({SIM} Rolls)\n" +
            $"Legend : {legendRate:F2}% (Expected {data.legendChance}%)\n" +
            $"Epic   : {epicRate:F2}% (Expected {data.epicChance}%)\n" +
            $"Normal : {normalRate:F2}% (Expected {data.normalChance}%)\n" +
            $"Total  : {(legendRate + epicRate + normalRate):F2}%"
        );
    }

    static ItemSO Roll(GachaDataSO d)
    {
        float r = Random.Range(0f, 100f);

        if (r < d.legendChance)
            return d.legendItems[Random.Range(0, d.legendItems.Count)];

        r -= d.legendChance;

        if (r < d.epicChance)
            return d.epicItems[Random.Range(0, d.epicItems.Count)];

        return d.normalItems[Random.Range(0, d.normalItems.Count)];
    }
}