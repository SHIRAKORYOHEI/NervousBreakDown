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
        Flip();
        GameManager.Instance.SelectCard(this);
    }

    void Flip()
    {
        isFaceUp = !isFaceUp;
        image.sprite = isFaceUp ? frontSprite : backSprite;
    }
}
