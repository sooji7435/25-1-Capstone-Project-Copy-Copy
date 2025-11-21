using System;
using UnityEngine;

/// <summary>
/// Adaptive Fuzzy System
/// - 입력: 거리(dist), 보스HP(bossHP), 플레이어HP(playerHP)
/// - 출력: Aggressive / Defensive / Balanced (float 스코어)
/// - 퍼지 함수: Gaussian
/// - 규칙: 9개 기본 규칙 기반 (거리 × 보스HP × 플레이어HP 조합)
/// </summary>
[Serializable]
public class Fuzzy
{
    // ----------- 퍼지 파라미터 (학습 or 수동 조정 가능) -----------
    [Header("Distance MF Parameters")]
    public float muNear = 1.0f;
    public float sigmaNear = 1.0f;
    public float muMid = 4.0f;
    public float sigmaMid = 1.5f;
    public float muFar = 7.0f;
    public float sigmaFar = 2.0f;

    [Header("HP MF Parameters")]
    public float muLow = 0.2f;
    public float muMidHP = 0.5f;
    public float muHigh = 0.8f;
    public float sigmaHP = 0.15f;

    [Header("Rule Weights")]
    public float wAggressive = 1.0f;
    public float wDefensive = 1.0f;
    public float wBalanced = 1.0f;

    // ---------------------------------------------------------------
    private float G(float x, float mu, float sigma)
    {
        sigma = Mathf.Max(sigma, 0.1f); // NaN 방지
        return Mathf.Exp(-Mathf.Pow(x - mu, 2) / (2 * sigma * sigma));
    }

    /// <summary>
    /// 입력(dist, bossHP, playerHP)을 퍼지화 후 세 행동에 대한 스코어 반환
    /// </summary>
    public Vector3 Evaluate(float dist, float bossHP, float playerHP)
    {
        // --- 1. 퍼지 멤버십 계산 ---
        float near = G(dist, muNear, sigmaNear);
        float mid = G(dist, muMid, sigmaMid);
        float far = G(dist, muFar, sigmaFar);

        float bossLow = G(bossHP, muLow, sigmaHP);
        float bossMid = G(bossHP, muMidHP, sigmaHP);
        float bossHigh = G(bossHP, muHigh, sigmaHP);

        float playerLow = G(playerHP, muLow, sigmaHP);
        float playerMid = G(playerHP, muMidHP, sigmaHP);
        float playerHigh = G(playerHP, muHigh, sigmaHP);

        // --- 2. 퍼지 규칙 평가 ---
        // 규칙 예시:
        // ① 보스HP 높고, 거리 가깝고, 플레이어HP 낮으면 → 공격적
        // ② 보스HP 낮고, 거리 멀면 → 방어적
        // ③ 그 외는 밸런스형

        float r1 = near * bossHigh * playerLow;    // Aggressive
        float r2 = mid * bossHigh * playerMid;    // Aggressive
        float r3 = far * bossHigh * playerHigh;   // Defensive (원거리 견제)
        float r4 = far * bossLow * playerHigh;   // Defensive
        float r5 = near * bossLow * playerLow;    // Balanced (위기상황 근거리)
        float r6 = mid * bossMid * playerMid;    // Balanced
        float r7 = far * bossMid * playerLow;    // Aggressive (추격)
        float r8 = near * bossLow * playerHigh;   // Defensive (후퇴)
        float r9 = mid * bossHigh * playerLow;    // Aggressive

        float aggressiveScore = (r1 + r2 + r7 + r9) * wAggressive;
        float defensiveScore = (r3 + r4 + r8) * wDefensive;
        float balancedScore = (r5 + r6) * wBalanced;

        // --- 3. 정규화 ---
        float total = aggressiveScore + defensiveScore + balancedScore + 1e-6f;
        aggressiveScore /= total;
        defensiveScore /= total;
        balancedScore /= total;

        return new Vector3(aggressiveScore, defensiveScore, balancedScore);
    }

    /// <summary>
    /// 퍼지 출력값(세 행동 중 최대값)을 문자열로 반환 (디버깅용)
    /// </summary>
    public string GetDominantBehavior(float dist, float bossHP, float playerHP)
    {
        Vector3 eval = Evaluate(dist, bossHP, playerHP);
        if (eval.x > eval.y && eval.x > eval.z)
            return "Aggressive";
        else if (eval.y > eval.x && eval.y > eval.z)
            return "Defensive";
        else
            return "Balanced";
    }

    public void ResetParameters()
    {
        muNear = 1f;
        sigmaNear = 1f;
        muFar = 6f;
        sigmaFar = 2f;
        wAggressive = 1f;
        wDefensive = 1f;
    }
}
