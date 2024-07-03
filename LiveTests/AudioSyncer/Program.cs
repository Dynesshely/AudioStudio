using NAudio.CoreAudioApi;
using NAudio.Wave;

var enumerator = new MMDeviceEnumerator();
var renderDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

Console.WriteLine("可用的播放设备:");
for (int i = 0; i < renderDevices.Count; i++)
{
    Console.WriteLine($"{i}: {renderDevices[i].FriendlyName}");
}

Console.Write("请选择播放设备的序号: ");
if (!int.TryParse(Console.ReadLine(), out int selectedIndex) || selectedIndex < 0 || selectedIndex >= renderDevices.Count)
{
    Console.WriteLine("无效的序号。");
    return;
}

var selectedDevice = renderDevices[selectedIndex];

selectedDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 1.0f;

var captureDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
var playbackDevice = selectedDevice;

var waveFormat = new WaveFormat(44100, 16, 2);

using var capture = new WasapiLoopbackCapture(captureDevice);

using var player = new WasapiOut(playbackDevice, AudioClientShareMode.Shared, true, 20);

capture.WaveFormat = waveFormat;

var bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
{
    BufferDuration = TimeSpan.FromMilliseconds(100),
    DiscardOnBufferOverflow = true
};

capture.DataAvailable += (s, a) =>
{
    bufferedWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded);
};

player.Init(bufferedWaveProvider);

capture.StartRecording();

player.Play();

Console.WriteLine("正在录制和播放声音，按任意键停止...");
Console.ReadKey();

capture.StopRecording();
