# DoorKnock

## 概要
このプロジェクトは、異世界の住人との会話を通じて、自己開示や他人との価値観の共有をテーマにしたインタラクティブアート作品です。プロジェクト詳細は以下のリンクよりご確認ください。

[プロジェクト詳細はこちら](https://yagetas-portfolio9.cms.webnode.jp/doorknock/)

## システム概要
この作品は、ノックを検出する振動センサーやドアノブのタッチを検出する静電容量センサーをArduinoによって管理し、UnityとArduino間で通信することでインタラクティブ性を実現しています。このレポジトリには、Unityプロジェクトのみが含まれ、作品のメインシステムとなる音声制御部分のソースコードを記載しています。

### 各クラスの役割
- **Serial.cs**: Arduinoとの通信を担当
- **VibrationSensorController.cs**: 振動センサーの値を監視
- **RecordingController.cs**: 録音の制御を担当
- **AudioController.cs**: システム全体の音声を制御
- **AudioDataBase.cs**: 音声データベースの管理
- **AudioLoader.cs**: 音声データの読み込み
- **RecordingSystem.cs**: 体験者の音声の録音を制御
- **SystemModel.cs**: システム全体のモデルクラス

### その他
- **Audioフォルダ**: 体験者の音声が記録されています。

---

