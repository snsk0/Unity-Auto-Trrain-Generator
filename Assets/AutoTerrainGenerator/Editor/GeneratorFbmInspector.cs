#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using AutoTerrainGenerator.HeightMapGenerators;
using AutoTerrainGenerator.Parameters;

namespace AutoTerrainGenerator.Editors
{
    [CustomEditor(typeof(GeneratorFbm))]
    public class GeneratorFbmInspector : Editor
    {
        private void Awake()
        {
            serializedObject.Update();

            string paramJson = EditorUserSettings.GetConfigValue(nameof(GeneratorFbmInspector));
            SerializedProperty paramProperty = serializedObject.FindProperty("_param");

            //Json������ꍇ
            if (!string.IsNullOrEmpty(paramJson))
            {
                //�f�V���A���C�Y�����s
                HeightMapGeneratorParam param = CreateInstance<HeightMapGeneratorParam>();
                JsonUtility.FromJsonOverwrite(paramJson, param);

                //���������ꍇ�I������
                if (param != null)
                {
                    paramProperty.objectReferenceValue = param;
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
            }

            //�f�V���A���C�Y�Ɏ��s�����ꍇ��������
            paramProperty.objectReferenceValue = CreateInstance<HeightMapGeneratorBase>();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDestroy()
        {
            //�V���A���C�Y�����s
            HeightMapGeneratorParam param = serializedObject.FindProperty("_param").objectReferenceValue as HeightMapGeneratorParam;
            EditorUserSettings.SetConfigValue(nameof(GeneratorFbmInspector), JsonUtility.ToJson(param));

            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //�p�����[�^�I�u�W�F�N�g���擾
            HeightMapGeneratorParam param = serializedObject.FindProperty("_param").objectReferenceValue as HeightMapGeneratorParam;

            param.seed = EditorGUILayout.IntField(new GUIContent("�V�[�h�l", "�V�[�h�l��ݒ肵�܂�"), param.seed);

            param.frequency = EditorGUILayout.FloatField(new GUIContent("���g��", "�g�p����m�C�Y�̎��g����ݒ肵�܂�"), param.frequency);
            MessageType type = MessageType.Info;
            if(param.frequency > 256)
            {
                type = MessageType.Warning;
            }
            EditorGUILayout.HelpBox("UnityEngine.Mathf.PerlinNoise�̎�����256�Ȃ���\n256�ȏ�̐��l�ɂ���Ɠ��l�̒n�`�������\��������܂�", type);

            param.isLinearScaling = EditorGUILayout.Toggle(new GUIContent("���`�X�P�[�����O", "���`�X�P�[�����O��L�������܂�"), param.isLinearScaling);

            if (!param.isLinearScaling)
            {
                param.amplitude = EditorGUILayout.Slider(new GUIContent("�U��", "��������HeightMap�̐U����ݒ肵�܂�"),
                    param.amplitude, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);
            }
            else
            {
                EditorGUILayout.MinMaxSlider(new GUIContent("�X�P�[���͈�", "��������HeightMap�̃X�P�[���͈͂�ݒ肵�܂�"),
                    ref param.minLinearScale, ref param.maxLinearScale, ATGMathf.MinTerrainHeight, ATGMathf.MaxTerrainHeight);

                GUI.enabled = false;
                EditorGUILayout.FloatField(new GUIContent("�Œ�l", "�U���̍Œ�l��\�����܂�"), param.minLinearScale);
                EditorGUILayout.FloatField(new GUIContent("�ō��l", "�U���̍ō��l��\�����܂�"), param.maxLinearScale);
                EditorGUILayout.FloatField(new GUIContent("�U��", "�U���̒l��\�����܂�"), param.maxLinearScale - param.minLinearScale);
                GUI.enabled = true;
            }

            if (param.octaves > 0 && param.maxLinearScale == ATGMathf.MaxTerrainHeight)
            {
                EditorGUILayout.HelpBox("�I�N�^�[�u�𗘗p����ꍇ�A�U����1�����ɐݒ肵�Ă�������\n�n�`����������������܂���\n0.5����������܂�", MessageType.Error);
            }

            param.octaves = EditorGUILayout.IntField(new GUIContent("�I�N�^�[�u", "�񐮐��u���E���^���𗘗p���ăI�N�^�[�u�̐��l�̉񐔃m�C�Y���d�˂܂�"), param.octaves);

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(new GUIContent("�ݒ�l���o�͂���", "�ݒ�l���A�Z�b�g�t�@�C���ɕۑ����܂�")))
            {
                string savePath = EditorUtility.SaveFilePanelInProject("Save", "parameters", "asset", "");
                if (!string.IsNullOrEmpty(savePath))
                {
                    //�l���R�s�[����
                    HeightMapGeneratorParam outputParam = Instantiate(param);

                    //�o�͂���
                    AssetDatabase.CreateAsset(outputParam, savePath);
                }
            }
        }
    }
}
#endif