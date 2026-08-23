using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ButtonTemplate : MonoBehaviour
{
    [Tooltip("How far the underline will expand")]
    public float underlineExpansionAmount = 100f;
    [Tooltip("How long the underline expand/collapse animation takes, in seconds")]
    public float underlineAnimationDuration = 0.2f;
    public RawImage highlight;
    public RawImage underline;
    public TMP_Text text;

    [ContextMenu("hover")]
    void Hovered()
    {
        StopAllCoroutines();
        StartCoroutine(UnderlineExpand(0f, underlineExpansionAmount, underlineAnimationDuration));
    }

    [ContextMenu("unhover")]
    void Unhovered()
    {
        StopAllCoroutines();
        // Ease from wherever the underline actually currently is, not an assumed value
        StartCoroutine(UnderlineExpand(underline.rectTransform.sizeDelta.x, 0f, underlineAnimationDuration));
    }


    private IEnumerator UnderlineExpand(float startWidth, float targetWidth, float duration)
    {
        float elapsedTime = 0f;

        // Snap to the explicit start width immediately so the animation always begins from a known state
        Vector2 startSize = underline.rectTransform.sizeDelta;
        startSize.x = startWidth;
        underline.rectTransform.sizeDelta = startSize;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float linearT = Mathf.Clamp01(elapsedTime / duration);
            float easeOutT = 1f - (1f - linearT) * (1f - linearT);

            float currentWidth = Mathf.Lerp(startWidth, targetWidth, easeOutT);

            Vector2 temporarySize = underline.rectTransform.sizeDelta;
            temporarySize.x = currentWidth;
            underline.rectTransform.sizeDelta = temporarySize;

            yield return null;
        }

        Vector2 finalSize = underline.rectTransform.sizeDelta;
        finalSize.x = targetWidth;
        underline.rectTransform.sizeDelta = finalSize;
    }
}