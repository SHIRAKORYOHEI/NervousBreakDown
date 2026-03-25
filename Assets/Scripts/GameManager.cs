using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private CanvasGroup boardCanvasGroup;
    
    //難易度ごとにAIが記憶できる枚数
    public enum Difficulty { Easy, Normal, Hard }
    [SerializeField] private Difficulty difficulty = Difficulty.Normal;

    private int AiMemoryLimit => difficulty switch
    {
        Difficulty.Easy => 1,
        Difficulty.Normal => 3,
        Difficulty.Hard => 8,
        _ => 3
    };
    
    const int MaxFlippedCards = 3;
    List<Card> selectedCards = new();
    
    int playerHP = 200;
    int enemyHP = 200;
    
    bool isPlayerTurn = true;
    bool isSelectLock = false;
    Dictionary<int, Card> aiMemory = new();
    
    List<Card> allCards = new();

    private const int InitialDraw = 3;
    private int remainingDraw = 0;
    bool isResolving = false;
    bool isLocked     = false;

    void Awake() => Instance = this;

    public void RegisterCards(List<Card> cards)
    {
        allCards = new List<Card>(cards);
        StartPlayerTurn();
    }
    
    void Start() => DOVirtual.DelayedCall(0.5f, () => Debug.Log($"allCards起動時: {allCards.Count}"));
    
    public void SelectCard(Card card)
    {
        if (!isPlayerTurn || isLocked || remainingDraw <= 0) return;
        if (selectedCards.Contains(card)) return;
        
        FlipAndSelect(card);
        EventSystem.current.SetSelectedGameObject(null);

        if (selectedCards.Count >= MaxFlippedCards)
        {
            isLocked = true;
            DOVirtual.DelayedCall(1f, ResolveTurn);
        }
        
    }
    
    void FlipAndSelect(Card card)
    {
        selectedCards.Add(card);
        card.Flip();
        RememberCard(card);
        remainingDraw--;
    }

    void ResolveTurn()
    {
        boardCanvasGroup.blocksRaycasts = false;
        
        var groups = selectedCards.GroupBy(c => c.number).ToList();
        int sum = selectedCards.Sum(c => c.number);
        int damage;
        int addDraw = 0;
        
        switch (groups.Count)
        {
            case 1: // スリーカード：2枚削除、1枚戻す、+3ドロー
                damage = sum * 5;
                addDraw = 3;
                
                var c1 = selectedCards[0];
                var c2 = selectedCards[1];
                var c3 = selectedCards[2];

                RemoveCard(c1);
                RemoveCard(c2);

                c3.Flip();
                Debug.Log($"スリーカード {damage}");
                break;
            
            case 2: // ワンペア
                damage = sum * 3;
                addDraw = 1;
                
                var pairedCards = groups.First(g => g.Count() == 2).ToList();
                var oddCard = groups.First(g => g.Count() == 1).First();
                foreach (var c in pairedCards)
                    RemoveCard(c);
                
                oddCard.Flip();
                selectedCards.Remove(oddCard);
                Debug.Log($"ワンペア {damage}");
                break;
            
            default: // 役無し
                damage = sum;
                foreach (var c in selectedCards)
                    c.Flip();
                Debug.Log($"役無し {damage}");
                break;
        }
        
        selectedCards.Clear();
        ApplyDamage(damage);
        
        if( CheckGameOver()) return;

        if (addDraw > 0)
        {
            remainingDraw = addDraw;
            isLocked = false;
            if (!isPlayerTurn) StartCoroutine(AIContinue());
            else boardCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            EndTurn();
        }
    }

    void StartPlayerTurn()
    {
        isPlayerTurn  = true;
        isLocked      = false;
        remainingDraw = InitialDraw;
        boardCanvasGroup.blocksRaycasts = true;
    }
    
    void EndTurn()
    {
        selectedCards.Clear();
        isPlayerTurn = !isPlayerTurn;
        isLocked     = false;
 
        if (isPlayerTurn)
            StartPlayerTurn();
        else
        {
            boardCanvasGroup.blocksRaycasts = false;
            DOVirtual.DelayedCall(1f, StartAITurn);
        }
    }
    
    // ----- AI ターン ----

    void StartAITurn()
    {
        remainingDraw = InitialDraw;
        StartCoroutine(AITurn());
    }
    
    IEnumerator AIContinue()
    {
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(AITurn());
    }
    

    IEnumerator AITurn()
    {
        var faceDown = allCards.Where(c => !c.isFaceUp).ToList();
        if (faceDown.Count == 0 || remainingDraw <= 0) { EndTurn(); yield break; }
 
        // ペア候補を記憶から探す
        var best = aiMemory.Values
            .Where(c => !c.isFaceUp)
            .GroupBy(c => c.number)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault(g => g.Count() >= 2);
 
        var chosen = best != null ? best.Take(2).ToList() : new List<Card>();
        chosen.AddRange(faceDown.Except(chosen)
            .OrderBy(_ => Random.value)
            .Take(remainingDraw - chosen.Count));
 
        foreach (var c in chosen)
        {
            yield return new WaitForSeconds(0.5f);
            FlipAndSelect(c);
        }
 
        yield return new WaitForSeconds(1f);
        ResolveTurn();
        // 追加ドローがある場合はAIContinueがResolveTurn内から呼ばれる
    }

    void EndAITurn()
    {
        Debug.Log("end");
        isSelectLock = false;
        boardCanvasGroup.blocksRaycasts = true;
        EndTurn();
    }

    void RememberCard(Card card)
    {
        if(aiMemory.ContainsKey(card.id)) return;
        if (aiMemory.Count >= AiMemoryLimit) 
            aiMemory.Remove(aiMemory.Keys.First());
        aiMemory[card.id] = card;
    }

    void RemoveCard(Card card)
    {
        selectedCards.Remove(card);
        allCards.Remove(card);
        aiMemory.Remove(card.id);
        card.GetComponent<UnityEngine.UI.Image>().DOFade(0, 0.5f);
        card.transform.DOScale(0, 0.5f);
    }

    void ApplyDamage(int damage)
    {
        if (isPlayerTurn)
        {
            enemyHP = Mathf.Max(enemyHP - damage, 0);
            Debug.Log($"敵HP: {enemyHP}");
        }
        else
        {
            playerHP = Mathf.Max(playerHP - damage, 0);
            Debug.Log($"プレイヤーHP: {playerHP}");
        }
    }

    bool CheckGameOver()
    {
        if (playerHP <= 0) { Debug.Log("敗北"); /* TODO: UI */ return true; }
        if (enemyHP  <= 0) { Debug.Log("勝利"); /* TODO: UI */ return true; }
        return false;
    }
}
