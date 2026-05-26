using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip bgm;
    [SerializeField] private AudioClip diceRollSFX;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource.clip = bgm;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayDiceRoll()
    {
        sfxSource.PlayOneShot(diceRollSFX);
    }
}