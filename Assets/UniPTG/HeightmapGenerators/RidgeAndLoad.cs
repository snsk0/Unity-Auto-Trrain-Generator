namespace UniPTG.HeightmapGenerators
{
    internal class RidgeAndLoad : GeneratorRidge
    {
        private protected override float CalculateHeight(float currentAmplitude, float value)
        {
            value = base.CalculateHeight(currentAmplitude, value);

            //一定値以下の場合圧縮を行う
            float threshold = 0.25f;

            if (value < threshold)
            {
                value = Mathf.LinearScaling(value, 0, 1, 0.25f, 0.3f);
            }
            return value;
        }
    }
}