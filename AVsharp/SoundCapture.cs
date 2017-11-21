using CSCore;
using CSCore.SoundIn;
using CSCore.Codecs.WAV;
using CSCore.DSP;
using CSCore.Streams;
using System;
using CSCore.Utils;
using System.Collections.Generic;
using System.Linq;

public class SoundCapture {
    WasapiLoopbackCapture capture = null;     // system sound
    //WasapiCapture capture = null;               // default sound in (mic)
    SoundInSource Source;
    IWaveSource convertedSource;
    FftProvider fft;
    WaveWriter ww = null;
    public byte[] buffer;
    //Complex[] fftBuff;
    public Single[] fftBuff;
    public List<float> fftData;
    public FftSize FFT_RES = FftSize.Fft128;// FftSize.Fft64;
    //public int FFT_CHUNK = 64;
    int read = 0;

    public SoundCapture() {
        fftBuff = new Single[(int)FFT_RES];

        capture = new WasapiLoopbackCapture(100, new WaveFormat(44100, 16, 1)); // speed up capture
        //capture = new WasapiCapture(); // speed up capture
        capture.Initialize();

        Source = new SoundInSource(capture) { FillWithZeros = false }; ;
        convertedSource = Source
            //.ToStereo() //2 channels (for example)
            .ChangeSampleRate(44100) // 41.1kHz sample rate
            .ToSampleSource()
            .ToWaveSource(16); //16 bit pcm


        fft = new FftProvider(convertedSource.WaveFormat.Channels, FFT_RES);
        buffer = new byte[convertedSource.WaveFormat.BytesPerSecond / 2];
        Source.DataAvailable += (s, e) => {
            int read = 0;

            //while ((read = convertedSource.Read(buffer, 0, buffer.Length)) > 0) { }
            //convertedSource.Read(buffer, 0, buffer.Length);
            buffer = e.Data;

            fft.Add(BitConverter.ToSingle(buffer, 0), read);
            //Console.WriteLine(BitConverter.ToSingle(buffer, 0));
            fft.GetFftData(fftBuff);
            //foreach(var point in buffer) {
            //    Console.WriteLine(point);
            //}
        };

        //fft = new FftProvider(capture.WaveFormat.Channels, FFT_RES);
        //buffer = new byte[capture.WaveFormat.BytesPerSecond / 2];
        //capture.DataAvailable += (s, e) => {
        //    buffer = e.Data;
        //    fft.Add(BitConverter.ToSingle(buffer, 44), read);
        //    fft.GetFftData(fftBuff);
        //};

        capture.Start();
    }

    public void Stop() {
        if (ww != null && capture != null) {
            capture.Stop();
            ww.Dispose();
            ww = null;
            capture.Dispose();
            capture = null;
        }
    }

    public void StopAlt() {
        capture.Stop();
        //ww.Dispose();
        Source.Dispose();
        capture.Dispose();
    }

}