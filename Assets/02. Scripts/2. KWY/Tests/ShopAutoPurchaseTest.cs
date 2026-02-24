using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ShopAutoPurchaseTest
{
    const int LOOP_COUNT = 30;

    [UnityTest]
    public IEnumerator Shop_Purchase_30_Times()
    {
        // 씬 로드 (현재 메인 씬 이름으로 변경)
        SceneManager.LoadScene("2. KWY");
        yield return new WaitForSeconds(3f);

        // Shop 버튼
        var shopButton =
            GameObject.Find("MainCanvas/Canvas/MainUIButton/Shop Button")
            .GetComponent<Button>();

        Assert.NotNull(shopButton);

        for (int i = 0; i < LOOP_COUNT; i++)
        {
            Debug.Log($"구매 시도 {i + 1}");

            // 1. 샵 버튼 클릭
            shopButton.onClick.Invoke();
            yield return new WaitForSeconds(1f);

            // 2. 첫번째 슬롯 클릭
            var firstSlot =
                GameObject.Find("ShopSlot")
                .GetComponent<Button>();

            Assert.NotNull(firstSlot);
            firstSlot.onClick.Invoke();
            yield return new WaitForSeconds(1f);

            // 3. 구매 버튼 클릭
            var buyButton =
                GameObject.Find("Button")
                .GetComponent<Button>();

            Assert.NotNull(buyButton);
            buyButton.onClick.Invoke();

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("30회 구매 완료");
    }
}