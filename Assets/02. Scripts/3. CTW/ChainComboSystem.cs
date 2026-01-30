using UnityEngine;
using System;

public class ChainComboSystem : MonoBehaviour
{
    [SerializeField] private float chainRemain = 2f;
    private float chainTimer;

    private bool isChain;
    private int currentChain;

    public event Action<int> OnChainChanged;
    public event Action<int> OnChainEnded;

    void Update()
    {
        if (!isChain) return;
        chainTimer -= Time.deltaTime;

        if (chainTimer <= 0)
        {
            EndChain();
        }
    }

    //에너미 관련 스크립트에서 죽었을 때 메서드 적용
    public void ChainUp()
    {
        if (!isChain)
        {
            StartChain();
        }

        currentChain++;
        chainTimer = chainRemain;

        OnChainChanged?.Invoke(currentChain);

        Debug.Log($"현재체인: {currentChain}");
        //체인 수에 따라 캐릭터 주변 오오라도 추가하
    }

    private void StartChain()
    {
        isChain = true;
        currentChain = 0;
    }

    public void EndChain()
    {
        if (!isChain) return;

        int finalChain = currentChain;

        isChain = false;
        currentChain = 0;
        chainTimer = 0f;

        OnChainEnded?.Invoke(finalChain);
        Debug.Log("체인 끝");
    }
}
