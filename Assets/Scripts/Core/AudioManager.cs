using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Manages adaptive audio: synth-orchestra + rising-pitch multiplier SFX (Var 11).
    /// Intensity scales with swarm count and game state.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private AudioClip cityTheme;
        [SerializeField] private AudioClip swarmTheme;
        [SerializeField] private AudioClip raidTheme;

        [Header("SFX")]
        [SerializeField] private AudioClip gateActivateSFX;
        [SerializeField] private AudioClip shatterSFX;
        [SerializeField] private AudioClip lootSFX;
        [SerializeField] private AudioClip buildCompleteSFX;

        [Header("Adaptive Settings")]
        [SerializeField] private float basePitch = 1f;
        [SerializeField] private float maxPitch = 1.5f;
        [SerializeField] private float pitchRampSpeed = 0.5f;

        private AudioSource musicSource;
        private AudioSource sfxSource;
        private float targetPitch = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = GetComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void Update()
        {
            // Rising-pitch multiplier: pitch increases with swarm count
            musicSource.pitch = Mathf.MoveTowards(musicSource.pitch, targetPitch, pitchRampSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Update adaptive audio pitch based on swarm multiplier count.
        /// </summary>
        public void SetSwarmIntensity(int swarmCount)
        {
            float normalized = Mathf.Clamp01(swarmCount / 500f);
            targetPitch = Mathf.Lerp(basePitch, maxPitch, normalized);
        }

        /// <summary>
        /// Play a one-shot sound effect.
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void PlayGateActivate() => PlaySFX(gateActivateSFX);
        public void PlayShatter() => PlaySFX(shatterSFX);
        public void PlayLoot() => PlaySFX(lootSFX);
        public void PlayBuildComplete() => PlaySFX(buildCompleteSFX);

        private void HandleStateChanged(GameManager.GameState from, GameManager.GameState to)
        {
            AudioClip clip = to switch
            {
                GameManager.GameState.City => cityTheme,
                GameManager.GameState.Swarm => swarmTheme,
                GameManager.GameState.Raid => raidTheme,
                _ => null
            };

            if (clip != null && musicSource.clip != clip)
            {
                musicSource.clip = clip;
                musicSource.Play();
                targetPitch = basePitch;
            }
        }
    }
}
