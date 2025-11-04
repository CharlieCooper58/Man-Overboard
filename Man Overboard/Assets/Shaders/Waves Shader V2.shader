Shader "Custom/WavesShaderV2"
{
    Properties
    {

        _WaveCount("Wave Count", Int) = 4
        _WaveSeed("Wave Seed", Float) = 1.0
        _WaveSeedIter("Wave Seed Iter", Float) = 0.5
        _WaveSpeed("Wave Speed", Float) = 1.0
        _WaveSpeedRamp("Wave Speed Ramp", Float) = 1.0
        _BaseFrequency("Base Frequency", Float) = 0.2
        _BaseAmplitude("Base Amplitude", Float) = 0.5
        _BrownianFrequencyMult("Brownian Frequency Mult", Float) = 2.0
        _BrownianAmplitudeMult("Brownian Amplitude Mult", Float) = 0.5
        _MaxPeak("Max Peak", Float) = 1.0
        _PeakOffset("Peak Offset", Float) = 0.0
        _WaveDrag("Wave Drag", Float) = 0.2
        _WaveHeight("Wave Height", Float) = 1.0
        _NormalStrength("Normal Strength", Float) = 1.0

        _Ambient("Ambient Color", Color) = (0.2, 0.2, 0.2, 1.0)
        _DiffuseReflectance("Diffuse Reflectance", Color) = (1, 1, 1, 1)

        _SpecularNormalStrength("Specular Normal Strength", Float) = 1.0
        _SpecularReflectance("Specular Reflectance", Color) = (1, 1, 1, 1)
        _FresnelColor("Fresnel Color", Color) = (1, 1, 1, 1)
        _TipColor("Tip Color", Color) = (1, 1, 1, 1) 
        _LightColor("Light Color", Color) = (1, 1, 1, 1)

        _LightDir("Light Direction", Vector) = (-0.277, -0.5547, 0.832, 0)

        _Shininess("Shininess", Float) = 50
        _FresnelBias("Fresnel Bias", Float) = 0.1
        _FresnelNormalStrength("Fresnel Normal Strength", Float) = 1.0
        _FresnelStrength("Fresnel Strength", Float) = 1
        _FresnelShininess("Fresnel Shininess", Float) = 5
        _TipAttenuation("Tip Attenuation", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };


            struct Wave{
                float2 direction;
                float frequency;
                float amplitude;
                float phase;
                };
            struct v2f{
                float4 pos: SV_POSITION;
                float3 normal: TEXCOORD1;
                float3 worldPos: TEXCOORD2;
                float heightOffset: TEXCOORD3;
                };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                int _WaveCount;
                float _WaveSeed, _WaveSeedIter, _WaveSpeed, _WaveSpeedRamp, _BaseFrequency, _BaseAmplitude, _BrownianFrequencyMult, _BrownianAmplitudeMult, _MaxPeak, _PeakOffset, _WaveDrag, _WaveHeight, _NormalStrength, _FresnelNormalStrength, _SpecularNormalStrength;
                float4 _LightColor, _Ambient, _FresnelColor, _TipColor, _DiffuseReflectance, _SpecularReflectance;
                float3 _LightDir;//, _LightColor, _Ambient, _DiffuseReflectance, _SpecularReflectance, _FresnelColor, _TipColor;
                float _Shininess, _FresnelBias, _FresnelStrength, _FresnelShininess, _TipAttenuation;
            CBUFFER_END

            float CalculateOffset(float3 v, float2 direction, float frequency, float amplitude, float phase)
            {
                float waveCoord = v.x*direction.x+v.z*direction.y;
                return amplitude*exp(_MaxPeak * sin(waveCoord*frequency + phase*_Time.y));
            }

            float2 CalculateNormal(float3 v, float2 direction, float frequency, float amplitude, float phase, float offset)
            {
                float waveCoord = v.x*direction.x+v.z*direction.y;
                float2 n = _MaxPeak*frequency*cos(waveCoord*frequency + phase*_Time.y) * offset * direction;
                return float2(n.x, n.y);
            }

            float3 vertexFBM(float3 v){
                float f = _BaseFrequency;
                float a = _BaseAmplitude;
                float seed = _WaveSeed;
                float speed = _WaveSpeed;

                float3 p = v;

                float h = 0.0f;
                float2 n = 0.0f;

                float amplitudeSum = 0.0f;

                for(int wi = 0; wi < _WaveCount; ++wi){
                    float2 d = normalize(float2(cos(seed), sin(seed)));

                    float wave = CalculateOffset(p, d, f, a, speed);
                    float2 dx = CalculateNormal(p, d, f, a, speed, wave);
                
                    h += wave;
                    n += dx;
                    p.xz += -dx*a*_WaveDrag;

                    amplitudeSum += a;
                    f *= _BrownianFrequencyMult;
                    a *= _BrownianAmplitudeMult;
                    speed *= _WaveSpeedRamp;
                    seed += _WaveSeedIter;
                }
                float3 output = float3(h, n.x, n.y)/amplitudeSum;
                output.x *= _WaveHeight;

                return output;
                
            }
            v2f vert(Attributes v)
            {
                v2f i;
                i.worldPos = mul(float4(v.positionOS.xyz, 1.0), unity_ObjectToWorld);
                float3 fbm = vertexFBM(i.worldPos);
                i.heightOffset = fbm.x;


                //float4 newPos = v.positionOS + float4(float3(0, fbm.x, 0), 0.0f);
                i.worldPos += float3(0, fbm.x, 0);//;mul(newPos, unity_ObjectToWorld);
                i.pos = TransformWorldToHClip(i.worldPos);

                i.normal = TransformObjectToWorldNormal(normalize(float3(-fbm.y, 1.0f, -fbm.z)));

                return i;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfwayDir = (_LightDir + viewDir);

                float3 normal = i.normal;
                float height = i.heightOffset;
                normal.xz *= _NormalStrength;
                normal = normalize(normal);

                float ndotl = saturate(dot(_LightDir, normal));
                float3 diffuseReflectance = _DiffuseReflectance/PI;


                float3 diffuse = _LightColor * ndotl * diffuseReflectance;

                float3 fresnelNormal = normal;
                fresnelNormal.xz *= _FresnelNormalStrength;
                fresnelNormal = normalize(fresnelNormal);
                float base = 1 - dot(viewDir, fresnelNormal);
                float exponential = pow(base, _FresnelShininess);
                float R = exponential + _FresnelBias*(1.0f - exponential);
                R *= _FresnelStrength;

                float3 fresnel = _FresnelColor * R;

                float3 specularReflectance = _SpecularReflectance;
				float3 specNormal = normal;
				specNormal.xz *= _SpecularNormalStrength;
				specNormal = normalize(specNormal);
				float spec = pow(saturate(dot(specNormal, halfwayDir)), _Shininess) * ndotl;
                float3 specular = _LightColor.rgb * specularReflectance * spec;

				// Schlick Fresnel but again for specular
				base = 1 - saturate(dot(viewDir, halfwayDir));
				exponential = pow(base, 5.0f);
				R = exponential + _FresnelBias * (1.0f - exponential);

				specular *= R;
				


				float3 tipColor = _TipColor * pow(saturate(height/_WaveHeight), _TipAttenuation);

				float3 output = _Ambient + diffuse + specular + fresnel + tipColor;
                return float4(output, 1.0f);
            }
            ENDHLSL
        }
    }
}
