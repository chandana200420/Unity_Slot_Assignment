
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ReelSlot : MonoBehaviour
{
    [SerializeField] private Image symbolImage;

    private RectTransform rect;

    public SymbolData Symbol { get; private set; }

    public RectTransform Rect =>
        rect != null ? rect : (rect = (RectTransform)transform);

    private void Awake()
    {
        if (symbolImage == null)
            symbolImage = transform.Find("SymbolImage")?.GetComponent<Image>();
    }

    public void SetSymbol(SymbolData symbol)
    {
        Symbol = symbol;

        if (symbolImage == null)
            symbolImage = transform.Find("SymbolImage")?.GetComponent<Image>();

        if (symbolImage == null)
        {
            Debug.LogError("SymbolImage is missing from ReelSlot.", this);
            return;
        }

        symbolImage.sprite = symbol != null ? symbol.Sprite : null;
        symbolImage.preserveAspect = true;
        symbolImage.type = Image.Type.Simple;

        float scale = symbol != null ? symbol.DisplayScale : 1f;
        symbolImage.rectTransform.localScale = Vector3.one * scale;
    }
}
