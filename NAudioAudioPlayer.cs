using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CreatorCoffinTracker;

/// <summary>
/// Plays one local OGG, WAV, or MP3 file at a time through NAudio.
/// Keep one instance alive for the application lifetime, then call <see cref="Play"/>
/// from any UI or application code.
/// </summary>
public sealed class NAudioAudioPlayer : IDisposable
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ogg",
        ".wav",
        ".mp3"
    };

    private readonly object Sync = new();
    private WaveOut? CurrentOutput;
    private WaveStream? CurrentReader;
    private bool IsDisposed;

    /// <summary>
    /// Stops the current sound, if any, and begins playing the sound.
    /// </summary>
    /// <param name="FilePath">Path to an existing OGG, WAV, or MP3 file.</param>
    /// <param name="Volume">Linear output volume from 0.0 (silent) through 1.0 (normal).</param>
    public void Play(string FilePath, float Volume = 1.0f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(FilePath);

        string FullPath = Path.GetFullPath(FilePath);
        if (!File.Exists(FullPath))
            throw new FileNotFoundException("The audio file does not exist.", FullPath);

        if (!SupportedExtensions.Contains(Path.GetExtension(FullPath)))
            throw new NotSupportedException("Only OGG, WAV, and MP3 audio files are supported.");

        if (!float.IsFinite(Volume) || Volume < 0.0f || Volume > 1.0f)
            throw new ArgumentOutOfRangeException(nameof(Volume), "Volume must be a finite value from 0.0 through 1.0.");

        WaveStream Reader = CreateReader(FullPath);
        var Output = new WaveOut
        {
            BufferMilliseconds = 100,
            NumberOfBuffers = 2
        };

        try
        {
            var AmplifiedAudio = new VolumeSampleProvider
                (Reader.ToSampleProvider()) { Volume = Volume };

            Output.Init(AmplifiedAudio.ToWaveProvider());
            Output.Volume = Volume;
            Output.PlaybackStopped += Output_PlaybackStopped;

            lock (Sync)
            {
                ThrowIfDisposed();

                // Restart on new play
                StopCurrentLocked();
                CurrentReader = Reader;
                CurrentOutput = Output;
                Output.Play();
            }
        }
        catch
        {
            Output.PlaybackStopped -= Output_PlaybackStopped;
            Output.Dispose();
            Reader.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Stops the current sound, if any, and begins playing the sound.
    /// </summary>
    /// <param name="FilePath">Path to an existing OGG, WAV, or MP3 file.</param>
    /// <param name="Volume">Linear output volume from 0.0 (silent) through 1.0 (normal).</param>
    /// <param name="Pitch">Linear output pitch from 0.0 (deep af) through 1.0 (normal).</param>
    public void Play(string FilePath, float Volume = 1.0f, float Pitch = 1.0f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(FilePath);

        string FullPath = Path.GetFullPath(FilePath);
        if (!File.Exists(FullPath))
            throw new FileNotFoundException("The audio file does not exist.", FullPath);

        if (!SupportedExtensions.Contains(Path.GetExtension(FullPath)))
            throw new NotSupportedException("Only OGG, WAV, and MP3 audio files are supported.");

        if (!float.IsFinite(Volume) || Volume < 0.0f || Volume > 1.0f)
            throw new ArgumentOutOfRangeException(nameof(Volume), "Volume must be a finite value from 0.0 through 1.0.");

        if (!float.IsFinite(Pitch) || Pitch <= 0.0f)
            throw new ArgumentOutOfRangeException(nameof(Pitch), "Volume must be a finite value above 0.0.");

        WaveStream Reader = CreateReader(FullPath);
        var Output = new WaveOut
        {
            BufferMilliseconds = 100,
            NumberOfBuffers = 2
        };

        try
        {
            var PitchedAudio = new SmbPitchShiftingSampleProvider
                (Reader.ToSampleProvider()) { PitchFactor = Pitch };

            var AmplifiedAudio = new VolumeSampleProvider
                (PitchedAudio) { Volume = Volume };

            Output.Init(AmplifiedAudio.ToWaveProvider());
            Output.PlaybackStopped += Output_PlaybackStopped;

            lock (Sync)
            {
                ThrowIfDisposed();

                // Restart on new play
                StopCurrentLocked();
                CurrentReader = Reader;
                CurrentOutput = Output;
                Output.Play();
            }
        }
        catch
        {
            Output.PlaybackStopped -= Output_PlaybackStopped;
            Output.Dispose();
            Reader.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Stops playback and releases the current file decoder immediately.
    /// </summary>
    public void Stop()
    {
        lock (Sync)
        {
            if (!IsDisposed)
                StopCurrentLocked();
        }
    }

    /// <summary>
    /// Stops playback and releases all NAudio resources. Call this once at application shutdown.
    /// </summary>
    public void Dispose()
    {
        lock (Sync)
        {
            if (IsDisposed)
                return;

            StopCurrentLocked();
            IsDisposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private static WaveStream CreateReader(string FullPath)
    {
        return Path.GetExtension(FullPath).ToLowerInvariant() switch
        {
            ".ogg" => new VorbisWaveReader(FullPath),
            ".wav" or ".mp3" => new AudioFileReader(FullPath),
            _ => throw new NotSupportedException("Only OGG, WAV, and MP3 audio files are supported.")
        };
    }

    private void Output_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (sender is not WaveOut Output)
            return;

        WaveStream? Reader;
        lock (Sync)
        {
            if (CurrentOutput != Output)
                return;

            CurrentOutput = null;
            Reader = CurrentReader;
            CurrentReader = null;
        }

        Output.PlaybackStopped -= Output_PlaybackStopped;
        Output.Dispose();
        Reader?.Dispose();
    }

    private void StopCurrentLocked()
    {
        var Output = CurrentOutput;
        var Reader = CurrentReader;
        CurrentOutput = null;
        CurrentReader = null;

        if (Output is not null)
        {
            Output.PlaybackStopped -= Output_PlaybackStopped;
            Output.Stop();
            Output.Dispose();
        }

        Reader?.Dispose();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }
}
