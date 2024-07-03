using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniPTG.Parameters;

namespace UniPTG.HeightmapGenerators
{
    internal abstract class DefaultHeightmapGeneratorBase : HeightmapGeneratorBase
    {
        [SerializeField]
        private protected HeightmapGenerationParam _param;

        [SerializeField]
        private protected HeightmapGenerationParam _inputParam;

        private void OnEnable()
        {
            //インスタンス化
            _param = CreateInstance<HeightmapGenerationParam>();

            //専用フォルダを取得
            string path = Application.dataPath.Replace("Assets", "UserSettings/") + "UniPTG";

            //ない場合作る
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //ファイル名を追加
            path += "/" + GetType().FullName + ".json";


            //ファイルがないなら作成する
            if (!File.Exists(path))
            {
                Save(path);
            }

            //jsonを取得する
            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            reader.Close();

            //jsonがある場合上書きする
            if (!string.IsNullOrEmpty(json))
            {
                JsonUtility.FromJsonOverwrite(json, _param);
            }

            //永続化する
            _param.hideFlags = HideFlags.DontSave;

            Debug.Log(_param.GetInstanceID());
        }

        private void OnDisable()
        {
            //セーブする
            Save(Application.dataPath.Replace("Assets", "UserSettings/") + "UniPTG/" + GetType().FullName + ".json");

            //paramを破棄する
            DestroyImmediate(_param);
        }

        private void Save(string path)
        {
            //Jsonに変換して書き込み
            string json = JsonUtility.ToJson(_param);
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(json);
            writer.Close();
        }

        public override void Generate(float[,] heightmap, int size, INoiseReader noiseReader)
        {
            //入力値がある場合はそちらを使用する
            HeightmapGenerationParam param = _param;
            if (_inputParam != null)
            {
                param = _inputParam;
            }

            float frequency = param.frequency;
            float amplitude = param.amplitude;

            if (param.isLinearScaling)
            {
                amplitude = Mathf.MaxTerrainHeight;
            }

            for (int i = 0; i <= param.octaves; i++)
            {
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        float xvalue = (float)x / size * frequency;
                        float yvalue = (float)y / size * frequency;
                        heightmap[x, y] += CalculateHeight(amplitude, noiseReader.GetValue(xvalue, yvalue));
                    }
                }

                frequency *= Mathf.FBmFrequencyRate;
                amplitude *= Mathf.FBmPersistence;

                //ノイズ状態の更新
                noiseReader.UpdateState();
            }

            //高さの最大値と最小値を取得する
            IEnumerable<float> heightEnum = heightmap.Cast<float>();
            float minHeight = heightEnum.Min();
            float maxHeight = heightEnum.Max();

            //スケーリング
            if (param.isLinearScaling)
            {
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        heightmap[x, y] = Mathf.LinearScaling(heightmap[x, y], minHeight, maxHeight, param.minLinearScale, param.maxLinearScale);
                    }
                }
            }

            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    if (param.isLinearScaling)
                    {
                        heightmap[x, y] = HeightProcessing(param.minLinearScale, param.maxLinearScale, heightmap[x, y]);
                    }
                    else
                    {
                        heightmap[x, y] = HeightProcessing(minHeight, maxHeight, heightmap[x, y]);
                    }
                }
            }
        }

        private protected abstract float CalculateHeight(float currentAmplitude, float value);

        private protected virtual float HeightProcessing(float minAmplitude, float maxAmplitude, float value)
        {
            return value;
        }
    }
}
