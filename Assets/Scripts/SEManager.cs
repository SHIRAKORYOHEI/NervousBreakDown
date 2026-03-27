using UnityEngine;

public class SEManager : MonoBehaviour
{
    public static SEManager Instance;

    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip flipSE;

    [SerializeField] AudioClip[] comboSE; // 4段階

    [SerializeField] AudioClip attackSE;
    
    [SerializeField] AudioClip winSE;
    [SerializeField] AudioClip loseSE;

    void Awake()
    {
        Instance = this;
    }

    public void PlayFlip()
    {
        audioSource.PlayOneShot(flipSE);
    }

    public void PlayCombo(int comboLevel)
    {
        comboLevel = Mathf.Clamp(comboLevel, 0, comboSE.Length - 1);
        audioSource.PlayOneShot(comboSE[comboLevel]);
    }

    public void PlayAttack()
    {
        audioSource.PlayOneShot(attackSE);
    }

    public void PlayerWin(bool isPlayerWin)
    {
        audioSource.PlayOneShot(isPlayerWin ? winSE : loseSE);
    }
}