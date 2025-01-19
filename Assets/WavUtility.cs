using UnityEngine;
using System.IO;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavData, string clipName = "GeneratedClip")
    {
        using (var memoryStream = new MemoryStream(wavData))
        using (var reader = new BinaryReader(memoryStream))
        {
            // read the RIFF header
            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF")
                throw new System.Exception("Invalid WAV file. Missing RIFF header.");

            reader.ReadInt32(); // file size
            string wave = new string(reader.ReadChars(4));
            if (wave != "WAVE")
                throw new System.Exception("Invalid WAV file. Missing WAVE header.");

            // read format chunk
            string fmt = new string(reader.ReadChars(4));
            if (fmt != "fmt ")
                throw new System.Exception("Invalid WAV file. Missing fmt chunk.");

            int fmtChunkSize = reader.ReadInt32();
            int audioFormat = reader.ReadInt16();
            int numChannels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            reader.ReadInt32(); // byte rate
            reader.ReadInt16(); // block align
            int bitsPerSample = reader.ReadInt16();
            reader.BaseStream.Seek(fmtChunkSize - 16, SeekOrigin.Current); // skip extra format bytes

            // read data chunk
            string data = new string(reader.ReadChars(4));
            if (data != "data")
                throw new System.Exception("Invalid WAV file. Missing data chunk.");

            int dataSize = reader.ReadInt32();
            byte[] audioData = reader.ReadBytes(dataSize);

            // create audio clip
            AudioClip audioClip = AudioClip.Create(
                clipName,
                dataSize / (bitsPerSample / 8) / numChannels,
                numChannels,
                sampleRate,
                false
            );

            // convert audio data
            float[] floatData = new float[dataSize / (bitsPerSample / 8)];
            if (bitsPerSample == 16)
            {
                for (int i = 0; i < floatData.Length; i++)
                    floatData[i] = System.BitConverter.ToInt16(audioData, i * 2) / 32768f;
            }
            else if (bitsPerSample == 8)
            {
                for (int i = 0; i < floatData.Length; i++)
                    floatData[i] = (audioData[i] - 128) / 128f;
            }

            audioClip.SetData(floatData, 0);
            return audioClip;
        }
    }
}