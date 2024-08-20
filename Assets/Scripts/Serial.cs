using UnityEngine;
using System.IO.Ports;
using System.Collections;
//エラーが出る場合はProjectSettingsで「.NET Framework」が選択されているか確認

public class Serial : MonoBehaviour
{
    [SerializeField] private SystemModel systemModel;

    //COMポート番号とポートレートをArduinoと合わせる
    [SerializeField, Tooltip("マイコンのCOMポート番号")]
    private int comPortNum = 3; 
    [SerializeField, Tooltip("マイコンとの通信ボーレート")]
    private int bandRate = 9600; 

    [SerializeField] 
    private bool isDebugSerial = false;

    private SerialPort serialPort;
    private bool isDoneFirstReceive;


    void Start()
    {
        serialPort = new SerialPort("COM" + comPortNum.ToString(), bandRate);  
        serialPort.Open();
        isDoneFirstReceive = false;

        StartCoroutine(Receive());
    }

    void Update()
    {
        if(systemModel.CheckPhaseChange())  ClearBuffer();
    }

    /// <summary>
    /// データを受信するメソッド
    /// </summary>
    IEnumerator Receive()
    {
        if (serialPort.IsOpen && serialPort.BytesToRead > 0)
        {
            string receivedData = serialPort.ReadLine();  //通信読み取り（改行区切り）

            if (isDoneFirstReceive)
            {
                if (isDebugSerial)
                {
                    Debug.Log(receivedData);
                }
                
                ReadData(receivedData);
            }
            else
            {
                isDoneFirstReceive = true;
            }
        }

        yield return null;
        StartCoroutine(Receive());
    }

    /// <summary>
    /// 受信したデータを解析する関数
    /// </summary>
    /// <param name="originalData"> 受信データ文字列 </param>
    void ReadData(string originalData)
    {
        string[] separatedData = originalData.Split(" ");
        for(int i = 0; i < separatedData.Length; i++)
        {
            string[] nameAndValue = separatedData[i].Split(":");
            if(nameAndValue.Length != 2 || !(nameAndValue[1] == "0" || nameAndValue[1] == "1"))
            {
                Debug.LogError("シリアル通信で0,1以外のデータを受信しました．");
                return;
            }
            
            switch (nameAndValue[0])
            {
                case "V":
                    systemModel.vibrationSensorCurrentState = nameAndValue[1] == "1"? true:false;
                    break;
                case "T":
                    systemModel.touchSensorCurrentState = nameAndValue[1] == "1" ? true : false;
                    break;
                default:
                    Debug.LogError("シリアル通信で未対応の接頭辞を受信しました．");
                    break;
            }
        }
    }

    /// <summary>
    /// バッファ削除
    /// </summary>
    void ClearBuffer()
    {
        try
        {
            serialPort.DiscardInBuffer();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    void OnApplicationQuit()
    {
        serialPort.Close();
    }
}