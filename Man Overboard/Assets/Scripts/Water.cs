using Unity.Mathematics;
using UnityEngine;

public class Water : MonoBehaviour
{
    public static Water instance;
    [SerializeField] Material waterMat;

    [Header("Wave Composite Settings")]
    [SerializeField] int _WaveCount = 16;
    [SerializeField] float _WaveSeed = 4;
    [SerializeField] float _WaveSeedIter = 1;
    [SerializeField] float _WaveSpeed = 1.5f;
    [SerializeField] float _WaveSpeedRamp = 1.07f;
    [SerializeField] float _BaseFrequency = 0.2f;
    [SerializeField] float _BaseAmplitude = 2f;
    [SerializeField] float _BrownianFrequencyMult = 1.14f;
    [SerializeField] float _BrownianAmplitudeMult = 0.83f;
    [SerializeField] float _MaxPeak = 2.1f;
    [SerializeField] float _PeakOffset = 1.14f;
    [SerializeField] float _WaveDrag = 0.5f;
    [SerializeField] float _WaveHeight = 1.48f;

    Vector2[] waveDirections;
    private void Awake()
    {
        instance = this;

        waterMat.SetInt("_WaveCount", _WaveCount);
        waterMat.SetFloat("_WaveSeed", _WaveSeed);
        waterMat.SetFloat("_WaveSeedIter", _WaveSeedIter);
        waterMat.SetFloat("_WaveSpeed", _WaveSpeed);
        waterMat.SetFloat("_WaveSpeedRamp", _WaveSpeedRamp);
        waterMat.SetFloat("_BaseFrequency", _BaseFrequency);
        waterMat.SetFloat("_BaseAmplitude", _BaseAmplitude);
        waterMat.SetFloat("_BrownianFrequencyMult", _BrownianFrequencyMult);
        waterMat.SetFloat("_BrownianAmplitudeMult", _BrownianAmplitudeMult);
        waterMat.SetFloat("_MaxPeak", _MaxPeak);
        waterMat.SetFloat("_PeakOffset", _PeakOffset);
        waterMat.SetFloat("_WaveDrag", _WaveDrag);
        waterMat.SetFloat("_WaveHeight", _WaveHeight);

        waveDirections = new Vector2[_WaveCount];
        for(int wi = 0; wi < _WaveCount; ++wi)
        {
            waveDirections[wi] = new Vector2(Mathf.Cos(_WaveSeed), Mathf.Sin(_WaveSeed));
            _WaveSeed += _WaveSeedIter;
        }
    }
    public static Vector3 StaticGetWaterBehaviorAtPoint(Vector3 coord)
    {
        return instance.GetWaterBehaviorAtPoint(coord);
    }
    public Vector3 GetWaterBehaviorAtPoint(Vector3 coord)
    {

        float time = Time.time;

        float CalculateOffset(Vector2 v, float2 direction, float frequency, float amplitude, float phase)
        {
            float waveCoord = v.x * direction.x + v.y * direction.y;
            return amplitude * Mathf.Exp(_MaxPeak * Mathf.Sin(waveCoord * frequency + phase * time) - _PeakOffset);
        }

        float2 CalculateNormal(Vector2 v, float2 direction, float frequency, float amplitude, float phase, float offset)
        {
            float waveCoord = v.x * direction.x + v.y * direction.y;
            return _MaxPeak * frequency * Mathf.Cos(waveCoord * frequency + phase * time) * offset * direction;
            
        }
        Vector3 vertexFBM(Vector3 v)
        {
            float f = _BaseFrequency;
            float a = _BaseAmplitude;
            float speed = _WaveSpeed;

            Vector2 p = new Vector2(v.x, v.z);

            float h = 0.0f;
            Vector2 n = new Vector2();

            float amplitudeSum = 0.0f;

            for (int wi = 0; wi < _WaveCount; ++wi)
            {
                float2 d = waveDirections[wi];

                float wave = CalculateOffset(p, d, f, a, speed);
                Vector2 dx = CalculateNormal(p, d, f, a, speed, wave);

                h += wave;
                n += dx;
                p += -dx * a * _WaveDrag;

                amplitudeSum += a;
                f *= _BrownianFrequencyMult;
                a *= _BrownianAmplitudeMult;
                speed *= _WaveSpeedRamp;

            }
            Vector3 output = new Vector3(h, n.x, n.y) / amplitudeSum;
            output.x *= _WaveHeight;
            return output;

        }
        return vertexFBM(coord);

    }

}
