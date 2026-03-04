using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class BuyStressTest
{
    const string SCENE_NAME = "2. KWY";
    const string ROOT_NAME = "MainCanvas";
    const string SHOP_PATH = "Canvas/MainPanel/Shop";
    const string CONTENT_PATH = "Scroll View/Viewport/Content"; 

    [UnitySetUp]
    public IEnumerator Setup()
    {
        if (SceneManager.GetActiveScene().name != SCENE_NAME)
        {
            Debug.LogWarning("테스트 실행 전 씬을 강제로 로드 합니다.");
            SceneManager.LoadScene(SCENE_NAME);
        }

        // 씬/오브젝트/레이아웃 안정화
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        yield return new WaitUntil(() => GameObject.Find(ROOT_NAME) != null);
    }

    [UnityTest]
    public IEnumerator ClickShopSlot18()
    {
        // 1) MainCanvas 찾기
        var rootGO = GameObject.Find(ROOT_NAME);
        Assert.IsNotNull(rootGO, $"{ROOT_NAME} not found");

        // 2) Shop 찾기
        var shop = rootGO.transform.Find(SHOP_PATH);
        Assert.IsNotNull(shop, $"Shop not found. Tried: {ROOT_NAME}/{SHOP_PATH}");

        // 3) Content 찾기
        var content = shop.Find(CONTENT_PATH);
        Assert.IsNotNull(content, $"Content not found. Tried: {CONTENT_PATH}");

        // UI 생성/정렬 대기
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Assert.GreaterOrEqual(content.childCount, 18, "슬롯이 18개 미만입니다.");

        // 4) 18번째 슬롯 (index 17)
        var slot18 = content.GetChild(17);
        Assert.IsNotNull(slot18, "Slot18 not found (Content.GetChild(17))");

        // 5) 버튼 찾기 (비활성 포함)
        var btn = slot18.GetComponentInChildren<Button>(true);
        Assert.IsNotNull(btn, "Button not found in Slot18");

        // 6) 클릭
        btn.onClick.Invoke();
        Debug.Log(" 18번째 ShopSlot 버튼 클릭 성공");

        for (int i = 0; i < 100; i++)
        {
            btn.onClick.Invoke();
            Debug.Log($"{i + 1}번째 클릭");
            yield return null; // 프레임 대기 (중요)
        }

        yield return null;
    }
}