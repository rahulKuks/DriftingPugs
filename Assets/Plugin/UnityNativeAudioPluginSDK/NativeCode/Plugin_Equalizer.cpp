#include "AudioPluginUtil.h"

namespace Equalizer
{
	enum Param
	{
		P_MasterGain,
		P_LowGain,
		P_MidGain,
		P_HighGain,
		P_LowFreq,
		P_MidFreq,
		P_HighFreq,
		P_LowQ,
		P_MidQ,
		P_HighQ,
		P_UseLogScale,
		P_ShowSpectrum,
		P_SpectrumDecay,
		P_NUM
	};

	struct EffectData
	{
		struct Data
		{
			float p[P_NUM];
			BiquadFilter FilterH[8];
			BiquadFilter FilterP[8];
			BiquadFilter FilterL[8];
			float sr;
			Random random;
#if !UNITY_PS3 && !UNITY_SPU
			FFTAnalyzer analyzer;
#endif
		};
		union
		{
			Data data;
			unsigned char pad[(sizeof(Data) + 15) & ~15]; // This entire structure must be a multiple of 16 bytes (and and instance 16 byte aligned) for PS3 SPU DMA requirements
		};
	};
    
#if !UNITY_SPU

	int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
	{
		int numparams = P_NUM;
		definition.paramdefs = new UnityAudioParameterDefinition [numparams];
		RegisterParameter(definition, "MasterGain", "dB", -100.0f, 100.0f, 0.0f, 1.0f, 1.0f, P_MasterGain);
		RegisterParameter(definition, "LowGain", "dB", -100.0f, 100.0f, 0.0f, 1.0f, 1.0f, P_LowGain);
		RegisterParameter(definition, "MidGain", "dB", -100.0f, 100.0f, 0.0f, 1.0f, 1.0f, P_MidGain);
		RegisterParameter(definition, "HighGain", "dB", -100.0f, 100.0f, 0.0f, 1.0f, 1.0f, P_HighGain);
		RegisterParameter(definition, "LowFreq", "Hz", 0.01f, 24000.0f, 800.0f, 1.0f, 3.0f, P_LowFreq);
		RegisterParameter(definition, "MidFreq", "Hz", 0.01f, 24000.0f, 4000.0f, 1.0f, 3.0f, P_MidFreq);
		RegisterParameter(definition, "HighFreq", "Hz", 0.01f, 24000.0f, 8000.0f, 1.0f, 3.0f, P_HighFreq);
		RegisterParameter(definition, "LowQ", "", 0.01f, 10.0f, 0.707f, 1.0f, 3.0f, P_LowQ);
		RegisterParameter(definition, "MidQ", "", 0.01f, 10.0f, 0.707f, 1.0f, 3.0f, P_MidQ);
		RegisterParameter(definition, "HighQ", "", 0.01f, 10.0f, 0.707f, 1.0f, 3.0f, P_HighQ);
		RegisterParameter(definition, "UseLogScale", "", 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, P_UseLogScale);
		RegisterParameter(definition, "ShowSpectrum", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, P_ShowSpectrum);
		RegisterParameter(definition, "SpectrumDecay", "dB/s", -50.0f, 0.0f, -10.0f, 1.0f, 1.0f, P_SpectrumDecay);
		return numparams;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		EffectData* effectdata = new EffectData;
		memset(effectdata, 0, sizeof(EffectData));
#if !UNITY_PS3
		effectdata->data.analyzer.spectrumSize = 4096;
#endif
		InitParametersFromDefinitions(InternalRegisterEffectDefinition, effectdata->data.p);
		state->effectdata = effectdata;
		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
	{
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
#if !UNITY_PS3
		data->analyzer.Cleanup();
#endif
		delete data;
		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
	{
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
		if(index >= P_NUM)
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		data->p[index] = value;
		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char *valuestr)
	{
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
		if(value != NULL)
			*value = data->p[index];
		if(valuestr != NULL)
			valuestr[0] = 0;
		return UNITY_AUDIODSP_OK;
	}

	int UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback (UnityAudioEffectState* state, const char* name, float* buffer, int numsamples)
	{
#if !UNITY_PS3
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;
		if(strcmp(name, "InputSpec") == 0)
			data->analyzer.ReadBuffer(buffer, numsamples, true);
		else if(strcmp(name, "OutputSpec") == 0)
			data->analyzer.ReadBuffer(buffer, numsamples, false);
		else if(strcmp(name, "Coeffs") == 0)
		{
			data->FilterL[0].StoreCoeffs(buffer);
			data->FilterP[0].StoreCoeffs(buffer);
			data->FilterH[0].StoreCoeffs(buffer);
		}
		else
#endif
			memset(buffer, 0, sizeof(float) * numsamples);
		return UNITY_AUDIODSP_OK;
	}

#endif

#if !UNITY_PS3 || UNITY_SPU

#if UNITY_SPU
	EffectData	g_EffectData __attribute__((aligned(16)));
	extern "C"
#endif
	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
	{
		EffectData::Data* data = &state->GetEffectData<EffectData>()->data;

#if UNITY_SPU
		UNITY_PS3_CELLDMA_GET(&g_EffectData, state->effectdata, sizeof(g_EffectData));
		data = &g_EffectData.data;
#endif

		float sr = (float)state->samplerate;
		for(int i = 0; i < inchannels; i++)
		{
			data->FilterH[i].SetupHighShelf(data->p[P_HighFreq], sr, data->p[P_HighGain], data->p[P_HighQ]);
			data->FilterP[i].SetupPeaking(data->p[P_MidFreq], sr, data->p[P_MidGain], data->p[P_MidQ]);
			data->FilterL[i].SetupLowShelf(data->p[P_LowFreq], sr, data->p[P_LowGain], data->p[P_LowQ]);
		}

#if !UNITY_PS3 && !UNITY_SPU
		float specDecay = powf(10.0f, 0.05f * data->p[P_SpectrumDecay] * length / sr);
		bool calcSpectrum = (data->p[P_ShowSpectrum] >= 0.5f);
		if(calcSpectrum)
			data->analyzer.AnalyzeInput(inbuffer, inchannels, length, specDecay);
#endif

		const float masterGain = powf(10.0f, data->p[P_MasterGain] * 0.05f);
		for(unsigned int n = 0; n < length; n++)
		{
			for(int i = 0; i < outchannels; i++)
			{
				float killdenormal = (float)(data->random.Get() & 255) * 1.0e-9f;
				float y = inbuffer[n * inchannels + i] + killdenormal;
				y = data->FilterH[i].Process(y);
				y = data->FilterP[i].Process(y);				
				y = data->FilterL[i].Process(y);
				outbuffer[n * outchannels + i] = y * masterGain;
			}
		}

#if !UNITY_PS3 && !UNITY_SPU
		if(calcSpectrum)
			data->analyzer.AnalyzeOutput(outbuffer, outchannels, length, specDecay);
#endif

#if UNITY_SPU
		UNITY_PS3_CELLDMA_PUT(&g_EffectData, state->effectdata, sizeof(g_EffectData));
#endif

		return UNITY_AUDIODSP_OK;
	}

#endif
}
