using System.Text;
using System.Threading;
using _NM.Core.Item;
using _NM.Core.Item.Implementaion;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemAcquireLogObject : MonoBehaviour
{
    [SerializeField] private Image acquiredItemImage;
    [SerializeField] private TextMeshProUGUI acquiredItemText;
    [SerializeField] private CanvasGroup objectFade;
    [SerializeField] private CancellationTokenSource fadeCts;
    [SerializeField] private StringBuilder logTextBuilder;
    [SerializeField] private bool hided;
    [field:SerializeField] private Sequence startSequence;
    [SerializeField] private Sequence endSequence;
    [SerializeField] public float YPosDest;
    

    private void OnValidate()
    {
        acquiredItemImage = transform.GetChild(0).GetComponent<Image>();
        acquiredItemText = GetComponentInChildren<TextMeshProUGUI>();
        objectFade = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        YPosDest = 0f;
        startSequence = DOTween.Sequence();
        endSequence = DOTween.Sequence();
        hided = true;
        objectFade.alpha = 0f;
        logTextBuilder = new();
        fadeCts = new();
        InitializeTween();
    }

    private void Update()
    {
        if (objectFade && objectFade.alpha <= 0f)
        {
            hided = true;
        }
    }

    public void ShowItemAcquireLog(Item item,int count)
    {
        startSequence.Pause();
        hided = false;
        logTextBuilder.Clear();
        logTextBuilder.Append(item.ItemInfoData.ItemName);
        logTextBuilder.Append(" x ");
        logTextBuilder.Append(count);
        acquiredItemText.text = logTextBuilder.ToString();
        acquiredItemImage.sprite = ItemIconTable.Local.GetItemSprite(item.ItemInfoData.ItemIconName,ItemIconTable.SpriteSize.Small);
        startSequence.Restart();

    }
    

    private void InitializeTween()
    {
        startSequence = DOTween.Sequence().SetAutoKill(false).
            OnRewind(() =>
        {
            objectFade.alpha = 0f;
        }).OnPlay(() =>
        {
            objectFade.DOFade(1f, 0.5f);
        }).PrependInterval(2.5f)
            .OnComplete(() =>
        {
            objectFade.alpha = 1f;
            objectFade.DOFade(0f, 0.5f);

        });
        
        startSequence.Rewind();

    }
    
    


}
