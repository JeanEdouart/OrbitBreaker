# Original chiptune music

`Tools/generate-music.cjs` produces the twelve mono 44.1 kHz, 16-bit WAV files in `OrbitBreaker/Assets/Resources/OrbitMusic`. Run from the repository root:

```powershell
node Tools/generate-music.cjs OrbitBreaker/Assets/Resources/OrbitMusic
```

All arrangements are generated locally from original note/rhythm data with synthesized oscillators and percussion. No downloaded recordings or sound samples are used. Neon Orbit, Starlight, Void Runner and Cosmic Disco preserve the earlier game's note sequences, tempi and arrangements; the offline JavaScript rendering is not bit-identical to Unity's floating-point procedural renderer. A 4 ms boundary fade avoids a discontinuity when looping.

| Index | Resource | BPM | Character |
|---|---|---|---|
| 0 | neon_orbit | 132 | Existing default arcade arrangement |
| 1 | starlight | 116 | Existing bright, softer lead |
| 2 | void_runner | 148 | Existing minor-key driving bass |
| 3 | cosmic_disco | 124 | Existing accented groove |
| 4 | lunar_lounge | 108 | Spacious major lead and triangle bass |
| 5 | asteroid_funk | 126 | Syncopated bass and displaced kick |
| 6 | binary_chase | 152 | Minor arpeggios and octave-response bridge |
| 7 | aurora_dream | 118 | Suspended arpeggios and triangle lead |
| 8 | rusty_station | 112 | Metallic FM lead and sparse percussion |
| 9 | solar_carnival | 136 | Bouncing bass and bright call/response melody |
| 10 | deep_blue | 100 | Sparse low pulse and spacious minor melody |
| 11 | pocket_galaxy | 128 | Short major hook with arpeggiated bridge |
| 12 | meow_sad | — | Ballade « Meow » fournie par le propriétaire du projet |

The twelve generated tracks last 64 beats (25–38 seconds). The eight new generated tracks are leveled to RMS 0.036, approximately the original default's 0.0358, leaving room for gameplay effects. Measured peaks across those generated tracks are below 0.357; first and last PCM samples are zero. `meow_sad.mp3` is a separate recording supplied by the project owner, who confirmed the project has the rights needed to use it; it is not produced by `Tools/generate-music.cjs`.

## Import and platform considerations

Import defaults: Vorbis quality 0.8, Decompress On Load, mono, background loading, audio-data preload disabled. Load only the selected/preview clip, wait for audio-data loading before playback, and release old decoded audio when no longer used. A decoded track needs about 4.5–6.8 MB; retaining all twelve is unnecessary.

Unity's Web target uses its browser audio backend and platform-specific encoding; WAV source format does not imply uncompressed WAV delivery. Decompress On Load is supported on Web and gives low playback CPU cost on Android. Browser playback still requires a user gesture; do not assume audio will autoplay on iPhone. These settings are an import audit, not a claim of device testing or a new build.

References: [Unity Web audio](https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-audio.html), [AudioClip load types](https://docs.unity3d.com/ScriptReference/AudioClipLoadType.html), [background audio loading](https://docs.unity3d.com/ScriptReference/AudioImporter-loadInBackground.html).
