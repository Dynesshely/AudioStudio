using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace DesktopAudioCapture;

public class Program
{
    static void Main(string[] args)
    {
        var captureDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
        var playbackDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        var waveFormat = new WaveFormat(44100, 16, 2);

        using (var capture = new WasapiCapture(captureDevice))
        {
            using (var player = new WasapiOut(playbackDevice, AudioClientShareMode.Shared, true, 50))
            {
                capture.WaveFormat = waveFormat;
                var bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
                bufferedWaveProvider.DiscardOnBufferOverflow = true;

                capture.DataAvailable += (s, a) =>
                {
                    bufferedWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded);
                };

                player.Init(bufferedWaveProvider);

                capture.StartRecording();
                player.Play();

                Console.WriteLine("按任意键停止录制...");
                Console.ReadKey();

                capture.StopRecording();
            }
        }
    }
}
