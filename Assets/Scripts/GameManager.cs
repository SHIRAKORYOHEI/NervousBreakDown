using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private CanvasGroup boardCanvasGroup;

    
    List<Card> selectedCards = new ();
    const int MaxFlippedCards = 3;
    bool isSelectLock = false;

    void Awake()
    {
        Instance = this;
    }
    
    public void SelectCard(Card card)
    {
        if (isSelectLock || selectedCards.Count >= MaxFlippedCards || selectedCards.Contains(card)) return;
        selectedCards.Add(card);
        card.Flip();
        
        EventSystem.current.SetSelectedGameObject(null);

        if (selectedCards.Count == MaxFlippedCards)
        {
            isSelectLock = true;
            DOVirtual.DelayedCall(1f, ResolveTurn);
        }
            
    }

    void ResolveTurn()
    {
        boardCanvasGroup.blocksRaycasts = false;
        var groups = selectedCards.GroupBy(c => c.number).ToList();

        int sum = selectedCards.Sum(c => c.number);
        int damage;
        
        switch (groups.Count)
        {
            case 1: // スリーカード
                damage = sum * 5;
                
                RemoveCard(selectedCards[0]);
                RemoveCard(selectedCards[1]);
                
                Debug.Log($"スリーカード {damage}");
                break;
            
            case 2: // ワンペア
                damage = sum * 3;
                
                var pairedCards = groups.First(g => g.Count() == 2).ToList();

                foreach (var c in pairedCards)
                    RemoveCard(c);
                Debug.Log($"ワンペア {damage}");
                break;
            
            default: // 役無し
                damage = sum;
                foreach (var c in selectedCards)
                    c.Flip();
                //次ターンへの処理
                Debug.Log($"役無し {damage}");
                break;
        }
    }

    void RemoveCard(Card card)
    {
        card.GetComponent<UnityEngine.UI.Image>().DOFade(0, 0.5f);
        card.transform.DOScale(0, 0.5f);
    }
}
