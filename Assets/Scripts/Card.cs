using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    bool isFaceUp;
    public int id;
    public int suit => id / 10;
    public int number => id % 10 + 1;

    [SerializeField] private Sprite backSprite;
    public Sprite frontSprite;
    
    Image image;
    
    void Awake()
    {
        image = GetComponent<Image>();
        image.sprite = backSprite;
    }
    
    public void OnCardClicked()
    {
        GameManager.Instance.SelectCard(this);
    }

    private bool isFlipping = false;
    public void Flip()
    {
        if (isFlipping) return;
        isFlipping = true;

        transform.DOKill();
        
        transform.DORotate(new Vector3(0, 90, 0), 0.2f).OnComplete(() =>
        {
            isFaceUp = !isFaceUp;
            image.sprite = isFaceUp ? frontSprite : backSprite;

            transform.eulerAngles = new Vector3(0, -90, 0);

            transform.DORotate(Vector3.zero, 0.2f).OnComplete(() =>
            {
                isFlipping = false;
            });
        });
    }
}
