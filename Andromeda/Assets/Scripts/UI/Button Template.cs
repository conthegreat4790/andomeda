using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ButtonTemplate : MonoBehaviour
{
    [Tooltip("How far the underline will expand (also scales the highlight)")]
    public float underlineExpansionAmount = 100f;
    [Tooltip("How long the underline expand/collapse animation takes, in seconds")]
    public float underlineAnimationDuration = 0.2f;
    [Tooltip("Alpha value of the highlight when hovered")]
    public float highlightHoverAlpha = 0.3f;
    [Tooltip("Alpha value of the highlight when not hovered")]
    public float highlightIdleAlpha = 0f;
    [Tooltip("Uniform scale of the text when hovered")]
    public float textHoverScale = 1.1f;
    [Tooltip("Uniform scale of the text when not hovered")]
    public float textIdleScale = 1f;
    public RawImage highlight;
    public RawImage underline;
    public TMP_Text text;

    [ContextMenu("hover")]
    public void Hovered()
    {
        StopAllCoroutines();
        StartCoroutine(UnderlineExpand(underline.rectTransform.sizeDelta.x, underlineExpansionAmount, underlineAnimationDuration));
        StartCoroutine(HighlightFade(highlight.color.a, highlightHoverAlpha, underlineAnimationDuration));
        StartCoroutine(TextScale(text.rectTransform.localScale.x, textHoverScale, underlineAnimationDuration));
        highlight.rectTransform.sizeDelta = new Vector2(underlineExpansionAmount, highlight.rectTransform.sizeDelta.y);
    }

    [ContextMenu("unhover")]
    public void Unhovered()
    {
        StopAllCoroutines();
        StartCoroutine(UnderlineExpand(underline.rectTransform.sizeDelta.x, 0f, underlineAnimationDuration));
        StartCoroutine(HighlightFade(highlight.color.a, highlightIdleAlpha, underlineAnimationDuration));
        StartCoroutine(TextScale(text.rectTransform.localScale.x, textIdleScale, underlineAnimationDuration));
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
            float easeOutT = linearT * linearT * (3f - 2f * linearT);

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

    private IEnumerator HighlightFade(float startAlpha, float targetAlpha, float duration)
    {
        float elapsedTime = 0f;

        Color startColor = highlight.color;
        startColor.a = startAlpha;
        highlight.color = startColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float linearT = Mathf.Clamp01(elapsedTime / duration);
            float easeOutT = linearT * linearT * (3f - 2f * linearT);

            Color currentColor = highlight.color;
            currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, easeOutT);
            highlight.color = currentColor;

            yield return null;
        }

        Color finalColor = highlight.color;
        finalColor.a = targetAlpha;
        highlight.color = finalColor;
    }

    private IEnumerator TextScale(float startScale, float targetScale, float duration)
    {
        float elapsedTime = 0f;

        Vector3 startSize = Vector3.one * startScale;
        text.rectTransform.localScale = startSize;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float linearT = Mathf.Clamp01(elapsedTime / duration);
            float easeOutT = linearT * linearT * (3f - 2f * linearT);

            float currentScale = Mathf.Lerp(startScale, targetScale, easeOutT);
            text.rectTransform.localScale = Vector3.one * currentScale;

            yield return null;
        }

        text.rectTransform.localScale = Vector3.one * targetScale;
    }
}