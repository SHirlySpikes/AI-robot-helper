using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System;

public class ElevenLabsTTS : MonoBehaviour
{
    private const string ElevenLabsApiUrl = "https://api.elevenlabs.io/v1/text-to-speech/pMsXgVXv3BLzUgSXRplE?optimize_streaming_latency=0&output_format=pcm_22050";

    private const string  ElevenLabsApiKey = "sk_213d53c1c3bad57da213832e5c9821f49f2db8dcb6fb21c2"; 
    private AudioSource audioSource;

    void Start()
    {
        // add an audiosource component to the gameobject if not already present
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public async Task PlayResponseAudio(string responseText)
    {
        byte[] audioData = await GetAudioFromText(responseText);

        if (audioData != null)
        {
            // sample rate and channels (adjust based on your chosen output format)
            int sampleRate = 22050; // matches pcm_22050
            int channels = 1; // mono audio

            try
            {
                AudioClip clip = ConvertPCMToAudioClip(audioData, sampleRate, channels);
                if (clip != null)
                {
                    Debug.Log("Playing converted PCM audio...");
                    audioSource.clip = clip;
                    audioSource.Play();
                }
                else
                {
                    Debug.LogWarning("Failed to create AudioClip from PCM data.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error converting PCM to AudioClip: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Received null audio data.");
        }
    }
    private AudioClip ConvertPCMToAudioClip(byte[] pcmData, int sampleRate, int channels)
    {
        // calculate the total number of samples
        int totalSamples = pcmData.Length / sizeof(short); // assuming 16-bit PCM (2 bytes per sample)
        float[] audioData = new float[totalSamples];

        // convert pcm bytes to float samples
        for (int i = 0; i < totalSamples; i++)
        {
            short sample = System.BitConverter.ToInt16(pcmData, i * sizeof(short));
            audioData[i] = sample / 32768.0f; // normalize to -1 to 1
        }

        // create the AudioClip
        AudioClip audioClip = AudioClip.Create(
            "GeneratedClip",
            totalSamples / channels,
            channels,
            sampleRate,
            false
        );
        audioClip.SetData(audioData, 0);

        return audioClip;
    }
    private async Task<byte[]> GetAudioFromText(string text)
    {
        using (HttpClient client = new HttpClient())
        {
            // set authorization header (replace with your api key)
            Debug.Log($"Using API key: {ElevenLabsApiKey}");

            client.DefaultRequestHeaders.Add("xi-api-key", $"{ElevenLabsApiKey}");


            // construct the json payload
            var requestBody = new
            {
                text = text,
                voice_settings = new
                {
                    stability = 0.5,
                    similarity_boost = 0.75,
                    style = 0
                }
            };

// serialize with newtonsoft
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // send the request
            Debug.Log("Sending request to ElevenLabs...");
            var response = await client.PostAsync(ElevenLabsApiUrl, jsonContent);

            // handle response
            if (response.IsSuccessStatusCode)
            {
                Debug.Log("Successfully received audio response.");
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                Debug.LogError($"Error calling ElevenLabs API: {response.StatusCode}");
                Debug.LogError($"Response details: {await response.Content.ReadAsStringAsync()}");
                return null;
            }
        }
    }
}
