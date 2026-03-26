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
    List<Card> keptCards = new();
    
    int playerHP = 200;
    int enemyHP = 200;
    
    bool isPlayerTurn = true;
    Dictionary<int, Card> aiMemory = new();
    
    List<Card> allCards = new();

    private bool firstPair = true;
    private const int InitialDraw = 3;
    private int remainingDraw = 0;
    bool isLocked     = false;
    const int MaxKeptCards = 1;
    
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
        if (selectedCards.Contains(card) || keptCards.Contains(card)) return;
        if(card.isFaceUp) return;
        
        FlipAndSelect(card);
        EventSystem.current.SetSelectedGameObject(null);

        if (remainingDraw <= 0)
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
        var cards = selectedCards.ToList();
        cards.AddRange(keptCards);
        selectedCards.Clear();
        keptCards.Clear();
        
        var groups = cards.GroupBy(c => c.number).ToList();
        int sum = cards.Sum(c => c.number);
        int damage;
        int addDraw = 0;
        int maxSame = groups.Any() ? groups.Max(g => g.Count()) : 0;
        
        if (maxSame >= 3)
        {
            // スリーカード：2枚削除、+3ドロー
            damage = sum * 5;
            addDraw = 3;
            ApplyMatch(cards, groups.First(g => g.Count() >= 3).Take(2).ToList());
            Debug.Log($"スリーカード {damage}");
        }
        else if (maxSame == 2)
        {
            // ワンペア：2枚削除、+1ドロー、二回目以降+2ドロー
            damage = sum * 3;
            addDraw = firstPair ? 1 : 2;
            firstPair = false;
            ApplyMatch(cards, groups.First(g => g.Count() >= 2).Take(2).ToList());
            Debug.Log($"ワンペア {damage}");
        }
        else
        {
            // 役無し
            damage = sum / 2;
            foreach (var c in cards) c.Flip();
            Debug.Log($"役無し {damage}");
        }
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

    //役の共通処理
    void ApplyMatch(List<Card> all, List<Card> toRemove)
    {
        foreach (var c in toRemove) RemoveCard(c);
        foreach (var c in all.Except(toRemove)) KeepCard(c);
    }

    void KeepCard(Card card)
    {
        keptCards.Add(card);
        while (keptCards.Count > 1)
        {
            var oldest = keptCards[0];
            keptCards.RemoveAt(0);
            oldest.Flip();
        }
    }

    void StartPlayerTurn()
    {
        isPlayerTurn  = true;
        isLocked      = false;
        remainingDraw = InitialDraw;
        firstPair     = true;
        boardCanvasGroup.blocksRaycasts = true;
    }
    
    void EndTurn()
    {
        foreach (var c in selectedCards) c.Flip();
        selectedCards.Clear();
        keptCards.Clear();
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
        firstPair     = true;
        StartCoroutine(AITurn());
        selectedCards.Clear();
        keptCards.Clear();
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
 
        var chosen = new List<Card>();

        //持ち越しカードがあればペアを狙う
        if (keptCards.Count > 0)
        {
            var keptNumbers = keptCards.Select(c => c.number).ToHashSet();
            
            var matchFromMemory = aiMemory.Values
                .Where(c => !c.isFaceUp && keptNumbers.Contains(c.number))
                .Take(remainingDraw)
                .ToList();
            chosen.AddRange(matchFromMemory);
        }

        //記憶からペアを探す
        if (chosen.Count < remainingDraw)
        {
            var best = aiMemory.Values
                .Where(c => !c.isFaceUp && !chosen.Contains(c))
                .GroupBy(c => c.number)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault(g => g.Count() >= 2);
            
            if (best != null) chosen.AddRange(best.Take(remainingDraw - chosen.Count));
        }
        
        //残りはランダム
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
        card.transform.DOKill();
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
