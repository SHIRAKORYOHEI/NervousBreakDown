using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public enum ResultType
{
    ThreeCard, OnePair, NoMatch, Waiting
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private Slider playerHpBar;
    [SerializeField] private Slider enemyHpBar;
    
    [SerializeField] private TextMeshProUGUI playerHpText;
    [SerializeField] private TextMeshProUGUI enemyHpText;
    
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI matchText;
    [SerializeField] private TextMeshProUGUI drawCountText;
    
    [SerializeField] private Color playerColor = Color.cyan;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color waitingColor = Color.black;

    [SerializeField] private Image bg;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject resultPanel;
    
    [SerializeField] private TextMeshProUGUI resultText;
    
    void Awake() => Instance = this;

    void Start()
    {
        playerHpBar.value = 1;
        enemyHpBar.value = 1;
        
        startPanel.SetActive(true);
        resultPanel.SetActive(false);
    }

    public void UpdateHP(int playerHp, int enemyHp, int maxHp)
    {
        playerHpBar.value = playerHp / (float)maxHp;
        enemyHpBar.value = enemyHp / (float)maxHp;
        
        playerHpText.text = $"{playerHp} / {maxHp}";
        enemyHpText.text  = $"{enemyHp} / {maxHp}";
    }

    public void UpdateTurn(bool isPlayerTurn)
    {
        turnText.color = isPlayerTurn ? playerColor : enemyColor;
        turnText.text = isPlayerTurn 
            ? ToVertical("PLAYER TURN") 
            : ToVertical("ENEMY TURN");
    }

    public void UpdateDraw(int drawCount)
    {
        drawCountText.text = ToVertical(drawCount.ToString());
    }
    
    public void UpdateMatch(ResultType resultType, bool isPlayerTurn)
    {
        matchText.color = isPlayerTurn ? playerColor : enemyColor;
        if (resultType == ResultType.Waiting) matchText.color = waitingColor;
        matchText.text = resultType switch
        {
            ResultType.ThreeCard => ToVertical("THREE CARD!"),
            ResultType.OnePair => ToVertical("ONE PAIR"),
            ResultType.NoMatch => ToVertical("NO MATCH..."),
            ResultType.Waiting => ToVertical("WAITING..."),
            _ => " "
        };
    }

    public void UpdateState(bool isLocked)
    {
        bg.DOColor(isLocked ? new Color(0.4f, 0.4f, 0.4f, 1f) : Color.white, 0.2f);
    }
    
    public void PlayDamageEffect(bool isPlayer)
    {
        var bar = isPlayer ? enemyHpBar : playerHpBar;
        var text = isPlayer ? enemyHpText : playerHpText;

        bar.transform.DOShakePosition(0.2f, 5f);
        text.transform.DOScale(1.2f, 0.1f).SetLoops(2, LoopType.Yoyo);
    }

    public void OnStartGame()
    {
        GameManager.Instance.StartGame();
        startPanel.SetActive(false);
    }
    
    public void OnChangeDifficulty(int index)
    {
        GameManager.Instance.SetDifficulty((GameManager.Difficulty) index);
    }

    public void ShowResult(bool isPlayerWin)
    {
        resultPanel.SetActive(true);
        resultText.color = isPlayerWin ? Color.green : Color.red;
        resultText.text = isPlayerWin ? "YOU WIN!!!" : "LOSE...";
    }

    public void OnRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    string ToVertical(string input)
    {
        return string.Join("\n", input.ToCharArray());
    }
}
