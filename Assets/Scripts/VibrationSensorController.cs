using System.Collections;
using UnityEngine;

/// <summary>
/// 振動センサのコントローラー
/// </summary>
public class VibrationSensorController : MonoBehaviour
{
    [SerializeField] private SystemModel systemModel;

    private Coroutine resetCountTimer;
    private WaitUntil waitUntil_SensorStateChanged;

    private bool newSensorState;

    private void Start()
    {
        systemModel = GetComponent<SystemModel>();
        waitUntil_SensorStateChanged = new WaitUntil(() => systemModel.CheckVibrationSensorChange(out newSensorState));

        StartCoroutine(CountKnock());
    }

    /// <summary>
    /// ノックの検知を行う
    /// </summary>
    IEnumerator CountKnock()
    {
        // システムのフェーズがWaitKnockでない場合は再起
        if (systemModel.currentPhase != SystemModel.SystemPhase.WaitKnock)
        {
            yield return null;
            StartCoroutine(CountKnock());
            yield break;
        }

        // 振動センサの状態を監視
        yield return waitUntil_SensorStateChanged;
        if(newSensorState)
        {
            systemModel.knockCount++;
            Debug.Log("ノック回数："+systemModel.knockCount);

            // 一定時間経過後にノックカウントをリセットする機能を更新
            if (resetCountTimer != null) StopCoroutine(resetCountTimer);
            resetCountTimer = StartCoroutine(ResetKnockCountTimer(systemModel.RepeatedKnockInterval));

            // 指定回数ノックされた場合は次のフェーズに移行
            if(systemModel.knockCount >= systemModel.KnockThreshold)
            {
                systemModel.ChangeNextSystemPhase();
                systemModel.knockCount = 0;
            }
        }

        StartCoroutine(CountKnock());
    }


    /// <summary>
    /// 指定時間以内にノックがない場合にノックカウントをリセットする
    /// </summary>
    /// <param name="waitTime">リセット時間</param>
    IEnumerator ResetKnockCountTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        systemModel.knockCount = 0;
    }
}
