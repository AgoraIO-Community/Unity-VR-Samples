﻿using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Agora.Rtc;
using RingBuffer;

namespace Agora.Rtc.Extended
{
    /// <summary>
    /// The Custom AudioSink Player class receives audio frames from the
    /// Agora channel and applies the buffer to an AudioSource for playback.
    /// </summary>
    public class CustomAudioSinkPlayer : IAudioRenderManager
    {
        private IRtcEngine mRtcEngine = null;

        public int CHANNEL = 1;
        public int SAMPLE_RATE = 44100;
        public int PULL_FREQ_PER_SEC = 100;
        public bool DebugFlag = false;
        public int DebugCheck = 10;

        int SAMPLES;
        int FREQ;
        int BUFFER_SIZE;

        private int writeCount = 0;
        private int readCount = 0;

        private RingBuffer<float> _audioBuffer;
        private AudioClip _audioClip;
        private object _rtclock;

        private Thread _pullAudioFrameThread = null;
        private bool _pullAudioFrameThreadSignal = true;

        IntPtr BufferPtr { get; set; }

        // Start is called before the first frame update
        IEnumerator Start()
        {
            SAMPLES = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL;
            FREQ = 1000 / PULL_FREQ_PER_SEC;
            BUFFER_SIZE = SAMPLES * (int)BYTES_PER_SAMPLE.TWO_BYTES_PER_SAMPLE;

            var aud = GetComponent<AudioSource>();
            if (aud == null)
            {
                aud = gameObject.AddComponent<AudioSource>();
            }


            yield return new WaitWhile(() => mRtcEngine == null);
            KickStartAudio(aud, "externalClip");
        }

        public override void Init(IRtcEngine engine, object rtclock)
        {
            mRtcEngine = engine;
            _rtclock = rtclock;
            mRtcEngine.SetExternalAudioSink(true, SAMPLE_RATE, CHANNEL);

            // Increase playback overall volume
            mRtcEngine.AdjustPlaybackSignalVolume(200);
        }

        void KickStartAudio(AudioSource aud, string clipName)
        {
            var bufferLength = SAMPLES * 100; // 1-sec-length buffer

            // allow overflow to prevent edge case 
            _audioBuffer = new RingBuffer<float>(bufferLength, overflow: true);

            // Create and start the AudioClip playback, OnAudioRead will feed it
            _audioClip = AudioClip.Create(clipName,
                SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL, CHANNEL, SAMPLE_RATE, true,
                OnAudioRead);
            aud.clip = _audioClip;
            aud.loop = true;
            aud.Play();

            StartPullAudioThread();
        }

        void StartPullAudioThread()
        {
            if (_pullAudioFrameThread != null)
            {
                Debug.LogWarning("Stopping previous thread");
                _pullAudioFrameThread.Abort();
            }

            _pullAudioFrameThread = new Thread(PullAudioFrameThread);
            _pullAudioFrameThread.Start();
        }

        bool _paused = false;
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (DebugFlag)
                {
                    Debug.Log("Application paused. AudioBuffer length = " + _audioBuffer.Size);
                    Debug.Log("PullAudioFrameThread state = " + _pullAudioFrameThread.ThreadState + " signal =" + _pullAudioFrameThreadSignal);
                }

                // Invalidate the buffer
                _pullAudioFrameThread.Abort();
                _pullAudioFrameThread = null;
                _paused = true;
            }
            else
            {
                if (_paused) // had been paused, not from starting up
                {
                    Debug.Log("Resuming PullAudioThread");
                    _audioBuffer.Clear();
                    StartPullAudioThread();
                }
            }
        }


        void OnDestroy()
        {
            Debug.Log("OnApplicationQuit");
            _pullAudioFrameThreadSignal = false;
            _audioBuffer?.Clear();
            if (BufferPtr != IntPtr.Zero)
            {
                Debug.LogWarning("cleanning up IntPtr buffer");
                Marshal.FreeHGlobal(BufferPtr);
                BufferPtr = IntPtr.Zero;
            }
        }
        //get timestamp millisecond
        private double GetTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ts.TotalMilliseconds;
        }

        private void PullAudioFrameThread()
        {
            var avsync_type = 0;
            var bytesPerSample = 2;
            var type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
            var channels = CHANNEL;
            var samples = SAMPLE_RATE / PULL_FREQ_PER_SEC * CHANNEL;
            var samplesPerSec = SAMPLE_RATE;
            var buffer = new byte[samples * bytesPerSample];
            var freq = 1000 / PULL_FREQ_PER_SEC;

            // BufferPtr = Marshal.AllocHGlobal(BUFFER_SIZE);

            var tic = new TimeSpan(DateTime.Now.Ticks);

            var byteArray = new byte[BUFFER_SIZE];
            double startMillisecond = GetTimestamp();
            long tick = 0;

            AudioFrame audioFrame = new AudioFrame(
             type, samples, BYTES_PER_SAMPLE.TWO_BYTES_PER_SAMPLE, channels, samplesPerSec, buffer, 0, avsync_type);
            BufferPtr = Marshal.AllocHGlobal(samples * bytesPerSample * channels);
            audioFrame.buffer = BufferPtr;

            Debug.Log("[Agora] PullAudioFrameThread starts, audioFrame buffer = " + audioFrame.buffer);
            int loopcount = 0;
            while (_pullAudioFrameThreadSignal)
            {
                int nRet = -1;
                lock (_rtclock)
                {
                    if (mRtcEngine == null)
                    {
                        break;
                    }
                    Debug.Assert(audioFrame.buffer != null, "Audio Buffer is null!");
                    nRet = mRtcEngine.PullAudioFrame(audioFrame);
                }

                if (nRet == 0)
                {
                    Marshal.Copy((IntPtr)audioFrame.buffer, buffer, 0, buffer.Length);
                    var floatArray = ConvertByteToFloat16(buffer);
                    lock (_audioBuffer)
                    {
                        _audioBuffer.Put(floatArray);
                        writeCount += floatArray.Length;
                    }
                }
                if (++loopcount % DebugCheck == 0) Debug.Log("nRec = " + nRet);

                if (nRet == 0)
                {
                    tick++;
                    double nextMillisecond = startMillisecond + tick * freq;
                    double curMillisecond = GetTimestamp();
                    int sleepMillisecond = (int)Math.Ceiling(nextMillisecond - curMillisecond);
                    if (sleepMillisecond > 0)
                    {
                        Thread.Sleep(sleepMillisecond);
                    }
                }

            }

            if (BufferPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(BufferPtr);
                BufferPtr = IntPtr.Zero;
            }

            Debug.Log("Done running pull audio thread");
        }

        private static float[] ConvertByteToFloat16(byte[] byteArray)
        {
            var floatArray = new float[byteArray.Length / 2];
            for (var i = 0; i < floatArray.Length; i++)
            {
                floatArray[i] = BitConverter.ToInt16(byteArray, i * 2) / 32768f; // -Int16.MinValue
            }

            return floatArray;
        }

        // This Monobehavior method feeds data into the audio source
        private void OnAudioRead(float[] data)
        {
            for (var i = 0; i < data.Length; i++)
            {
                lock (_audioBuffer)
                {
                    if (_audioBuffer.Count > 0)
                    {
                        data[i] = _audioBuffer.Get();
                    }
                    else
                    {
                        // no data
                        data[i] = 0;
                    }
                }

                readCount += data.Length;
            }

        }
    }
}
