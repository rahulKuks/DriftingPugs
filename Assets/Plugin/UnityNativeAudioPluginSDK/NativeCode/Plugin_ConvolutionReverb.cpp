#include "AudioPluginUtil.h"

namespace ConvolutionReverb
{
	const float MAXLENGTH = 15.0f;

	enum Param
	{
		P_WET,
		P_GAIN,
		P_TIME,
		P_DECAY,
		P_DIFFUSION,
		P_STEREO,
		P_REVERSE,
		P_NUM
	};

	struct Channel
	{
		UnityComplexNumber** h;
		UnityComplexNumber** x;
		float* impulse;
		float* s;
	};

	struct EffectData
	{
		Mutex* mutex;
		float p[P_NUM];
		int numchannels;
		int numpartitions;
		int fftsize;
		int hopsize;
		int bufferindex;
		int writeoffset;
		int blocksize;
		float lastparams[P_NUM];
		UnityComplexNumber* tmpoutput;
		Channel* channels;
	};
        
	int InternalRegisterEffectDefinition(UnityAudioEffectDefinition& definition)
	{
		int numparams = P_NUM;
		definition.paramdefs = new UnityAudioParameterDefinition [numparams];
		RegisterParameter(definition, "Wet", "%", 0.0f, 100.0f, 30.0f, 1.0f, 1.0f, P_WET);
		RegisterParameter(definition, "Gain", "dB", -50.0f, 50.0f, 0.0f, 1.0f, 1.0f, P_GAIN);
		RegisterParameter(definition, "Time", "s", 0.01f, MAXLENGTH, 2.0f, 1.0f, 3.0f, P_TIME);
		RegisterParameter(definition, "Decay", "%", 0.01f, 100.0f, 50.0f, 1.0f, 3.0f, P_DECAY);
		RegisterParameter(definition, "Diffusion", "%", 0.0f, 100.0f, 100.0f, 1.0f, 1.0f, P_DIFFUSION);
		RegisterParameter(definition, "StereoSpread", "%", 0.0f, 100.0f, 30.0f, 1.0f, 1.0f, P_STEREO);
		RegisterParameter(definition, "Reverse", "", 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, P_REVERSE);
		return numparams;
	}

	static void SetupImpulse(EffectData* data, int numchannels, int blocksize, float samplerate)
	{
		MutexScopeLock mutexScope(*data->mutex);

		Random random;

		// if no parameters have changed, there's no need to recalculate the impulse
		if(	data->numchannels == numchannels &&
			data->blocksize == blocksize &&
			data->lastparams[P_TIME] == data->p[P_TIME] &&
			data->lastparams[P_DECAY] == data->p[P_DECAY] &&
			data->lastparams[P_DIFFUSION] == data->p[P_DIFFUSION] &&
			data->lastparams[P_STEREO] == data->p[P_STEREO] &&
			data->lastparams[P_REVERSE] == data->p[P_REVERSE]
		) return;

		// delete old buffers (can be avoided if numchannels, numpartitions and fftsize stay the same)
		for(int i = 0; i < data->numchannels; i++)
		{
			Channel& c = data->channels[i];
			for(int k = 0; k < data->numpartitions; k++)
			{
				delete[] c.h[k];
				delete[] c.x[k];
			}
			delete[] c.h;
			delete[] c.x;
			delete[] c.s;
			delete[] c.impulse;
		}
		delete[] data->channels;
		delete[] data->tmpoutput;

		memcpy(data->lastparams, data->p, sizeof(data->p));

		// reinitialize data
		data->bufferindex = 0;
		data->writeoffset = 0;
		data->numchannels = numchannels;
		data->blocksize = blocksize;
		data->hopsize = blocksize;
		data->fftsize = data->hopsize * 2;
		data->tmpoutput = new UnityComplexNumber [data->fftsize];
		data->channels = new Channel [data->numchannels];

		memset(data->tmpoutput, 0, sizeof(UnityComplexNumber) * data->fftsize);

		// calculate length of impulse in samples
		int reallength = (int)ceilf(samplerate * data->p[P_TIME]);

		// calculate length of impulse in samples as a multiple of the number of partitions processed
		data->numpartitions = 0;
		while(data->numpartitions * data->hopsize < reallength)
			data->numpartitions++;
		int impulsesamples = data->numpartitions * data->hopsize;

		// calculate individual impulse responses per channel
		bool reverse = data->p[P_REVERSE] > 0.5f;
		int n_offs = reverse ? (impulsesamples - 1) : 0, n_dir = reverse ? -1 : 1;
		for(int i = 0; i < data->numchannels; i++)
		{
			Channel& c = data->channels[i];
			c.impulse = new float [impulsesamples];
			c.s = new float [data->fftsize];
			memset(c.s, 0, sizeof(float) * data->fftsize);

			// calculate the impulse response as decaying white noise
			float decayconst = (data->p[P_STEREO] * random.GetFloat(0.0f, 0.01f) - 1.0f) / (reallength * 0.01f * data->p[P_DECAY]);
			float rms = 0.0f, d = 10.0f - 0.09f * data->p[P_DIFFUSION];
			for(int n = 0; n < impulsesamples; n++)
			{
				int i = n_offs + n_dir * n;
				c.impulse[n] = expf(decayconst * i) * powf(random.GetFloat(0.1f, 1.0f), d) * random.GetFloat(-1.0f, 1.0f);
				rms += c.impulse[n] * c.impulse[n];
			}

			// scale to unity gain
			float scale = 1.0f / sqrtf(rms);
			for(int n = 0; n < impulsesamples; n++)
				c.impulse[n] *= scale;

			// partition the impulse response
			c.h = new UnityComplexNumber* [data->numpartitions];
			c.x = new UnityComplexNumber* [data->numpartitions];
			for(int k = 0; k < data->numpartitions; k++)
			{
				c.h[k] = new UnityComplexNumber [data->fftsize];
				c.x[k] = new UnityComplexNumber [data->fftsize];
				memset(c.x[k], 0, sizeof(UnityComplexNumber) * data->fftsize);
				for(int n = 0; n < data->hopsize; n++)
				{
					int i = n + k * data->hopsize;
					c.h[k][n].Set((i < impulsesamples) ? c.impulse[i] : 0.0f, 0.0f);
				}
				memset(c.h[k] + data->hopsize, 0, sizeof(UnityComplexNumber) * (data->fftsize - data->hopsize));
				FFT::Forward(c.h[k], data->fftsize);
			}

			// integrate impulse for later resampling via box-filtering in GUI
			float sum = 0.0f;
			for(int n = 0; n < impulsesamples; n++)
			{
				sum += c.impulse[n];
				c.impulse[n] = sum;
			}
		}
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(UnityAudioEffectState* state)
	{
		EffectData* data = new EffectData;
		memset(data, 0, sizeof(EffectData));
		data->mutex = new Mutex();
		state->effectdata = data;
		InitParametersFromDefinitions(InternalRegisterEffectDefinition, data->p);
		SetupImpulse(data, 2, 1024, (float)state->samplerate); // Assuming stereo and 1024 sample block size
		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(UnityAudioEffectState* state)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		delete data->mutex;
		delete data;
		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(UnityAudioEffectState* state, float* inbuffer, float* outbuffer, unsigned int length, int inchannels, int outchannels)
	{
		EffectData* data = state->GetEffectData<EffectData>();

		// this should be done on a separate thread to avoid cpu spikes
		SetupImpulse(data, outchannels, (int)length, (float)state->samplerate);

		int writeoffset; // set inside loop

		for(int i = 0; i < inchannels; i++)
		{
			Channel& c = data->channels[i];

			// feed new data to input buffer s
			float* s = c.s;
			const int mask = data->fftsize - 1;
			writeoffset = data->writeoffset;
			for(int n = 0; n < data->hopsize; n++)
			{
				s[writeoffset] = inbuffer[n * inchannels + i];
				writeoffset = (writeoffset + 1) & mask;
			}

			// calculate X=FFT(s)
			UnityComplexNumber* x = c.x[data->bufferindex];
			for(int n = 0; n < data->fftsize; n++)
			{
				x[n].Set(s[writeoffset], 0.0f);
				writeoffset = (writeoffset + 1) & mask;
			}
			FFT::Forward(x, data->fftsize);

			// calculate y=IFFT(sum(convolve(H_k, X_k), k=1..numpartitions))
			UnityComplexNumber* y = data->tmpoutput;
			memset(y, 0, sizeof(UnityComplexNumber) * data->fftsize);
			for(int k = 0; k < data->numpartitions; k++)
			{
				UnityComplexNumber* h = c.h[k];
				UnityComplexNumber* x = c.x[(k + data->bufferindex) % data->numpartitions];
				for(int n = 0; n < data->fftsize; n++)
					y[n] = y[n] + h[n] * x[n];
			}
			FFT::Backward(y, data->fftsize);

			// overlap-save readout
			const float wet = data->p[P_WET] * 0.01f;
			const float gain = powf(10.0f, 0.05f * data->p[P_GAIN]);
			int readoffset = data->fftsize - data->hopsize;
			for(int n = 0; n < data->hopsize; n++)
			{
				float input = inbuffer[n * outchannels + i];
				outbuffer[n * outchannels + i] = input + (gain * y[n + readoffset].re - input) * wet;
			}

			if(--data->bufferindex < 0)
				data->bufferindex = data->numpartitions - 1;
		}

		data->writeoffset = writeoffset;

		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK SetFloatParameterCallback(UnityAudioEffectState* state, int index, float value)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		if(index >= P_NUM)
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		data->p[index] = value;
		return UNITY_AUDIODSP_OK;
	}

	UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK GetFloatParameterCallback(UnityAudioEffectState* state, int index, float* value, char *valuestr)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		if(index >= P_NUM)
			return UNITY_AUDIODSP_ERR_UNSUPPORTED;
		if(value != NULL)
			*value = data->p[index];
		if(valuestr != NULL)
			valuestr[0] = 0;
		return UNITY_AUDIODSP_OK;
	}

	int UNITY_AUDIODSP_CALLBACK GetFloatBufferCallback (UnityAudioEffectState* state, const char* name, float* buffer, int numsamples)
	{
		EffectData* data = state->GetEffectData<EffectData>();
		MutexScopeLock mutexScope(*data->mutex);
		if(strncmp(name, "Impulse", 7) == 0)
		{
			int index = name[7] - '0';
			if(index >= data->numchannels)
				return UNITY_AUDIODSP_OK;
			const float* src = data->channels[index].impulse;
			float scale = (float)(data->hopsize * data->numpartitions - 2) / (float)(numsamples - 1);
			float prev_val = 0.0f, time_scale = 1.0f / scale;
			for(int n = 1; n < numsamples; n++)
			{
				// resample pre-integrated curve via box-filtering: f(x) = (F(x+dx)-F(x)) / dx
				float next_time = n * scale;
				int i = (int)next_time;
				float next_val = src[i] + (src[i + 1] - src[i]) * (next_time - i);
				buffer[n] = (next_val - prev_val) * time_scale;
				prev_val = next_val;
			}
		}
		return UNITY_AUDIODSP_OK;
	}
}
