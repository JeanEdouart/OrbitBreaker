using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace OrbitBreaker.Tests
{
    public sealed class MusicAssetTests
    {
        [UnityTest]
        public IEnumerator AllMusic_LoadsAndContainsFiniteNonSilentAudio()
        {
            var unique = new HashSet<AudioClip>();
            for (int index = 0; index < 12; index++)
            {
                var clip = MusicLibrary.Load(index);
                Assert.That(clip, Is.Not.Null, "Track " + index);
                Assert.That(unique.Add(clip), Is.True);
                Assert.That(clip.channels, Is.EqualTo(1));
                Assert.That(clip.length, Is.InRange(25f, 39f));
                Assert.That(clip.LoadAudioData(), Is.True);
                float deadline = Time.realtimeSinceStartup + 15f;
                while (clip.loadState == AudioDataLoadState.Loading && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(clip.loadState, Is.EqualTo(AudioDataLoadState.Loaded), clip.name);
                var samples = new float[8192];
                Assert.That(clip.GetData(samples, clip.samples / 2), Is.True);
                double energy = 0;
                foreach (float value in samples)
                {
                    Assert.That(float.IsNaN(value) || float.IsInfinity(value), Is.False);
                    Assert.That(Mathf.Abs(value), Is.LessThan(.9f));
                    energy += value * value;
                }
                Assert.That(energy / samples.Length, Is.GreaterThan(.00001), clip.name);
                bool inUse = false;
                foreach (var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                    if (source.clip == clip) { inUse = true; break; }
                if (!inUse) clip.UnloadAudioData();
            }
        }
    }
}
