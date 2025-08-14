using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class TileUI : MonoBehaviour
{
    public float speed = 300f;
    public float perfectWindow = 0.1f;
    public float goodWindow = 0.25f;

    private float targetTime;
    private RectTransform rt;
    private bool isHit = false;
    private bool hasTriggeredLose = false; // NEW
    private TilePool pool;
    private AudioSource music;
    private RectTransform hitLine;
    private RectTransform bottomLine;
    private CanvasGroup canvasGroup;

    // Difficulty-based timing windows
    private float effectivePerfectWindow;
    private float effectiveGoodWindow;

    public void Init(float targetBeatTime, float fallSpeed, TilePool poolRef, AudioSource musicSource,
                     RectTransform hitLineRef, RectTransform bottomLineRef, DifficultyLevel difficulty = null)
    {
        targetTime = targetBeatTime;
        speed = fallSpeed;
        pool = poolRef;
        music = musicSource;
        hitLine = hitLineRef;
        bottomLine = bottomLineRef;
        isHit = false;
        hasTriggeredLose = false;

        // Apply difficulty-based timing windows
        if (difficulty != null)
        {
            effectivePerfectWindow = perfectWindow * difficulty.timingWindowMultiplier;
            effectiveGoodWindow = goodWindow * difficulty.timingWindowMultiplier;
        }
        else
        {
            effectivePerfectWindow = perfectWindow;
            effectiveGoodWindow = goodWindow;
        }

        gameObject.SetActive(true);

        // Reset visual state
        rt.localScale = Vector3.one;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnTap);
        }
    }

    void Update()
    {
        // Move toward negative Y (falling) - speed already adjusted by difficulty
        rt.anchoredPosition -= new Vector2(0, speed * Time.deltaTime);

        if (music == null) return;

        // Use music time adjusted for pitch/speed
        float songTime = music.time;

        // Miss if outside timing window
        if (!isHit && songTime > targetTime + effectiveGoodWindow)
        {
            ScoreEvents.Miss();
            ReturnToPool();
        }
        // Lose check:
        if (bottomLine != null && rt.position.y < bottomLine.position.y) // or a specific bottom line
        {
            if (!GameManager.Instance.hasLost) // NEW: Only if game not lost yet
            {
                hasTriggeredLose = true;
                GameManager.Instance.ShowLoseGame();
                pool.ReturnTile(this);
            }
        }
    }

    void OnTap()
    {
        if (isHit || music == null) return;

        isHit = true;
        float songTime = music.time;
        float timeDiff = Mathf.Abs(songTime - targetTime);

        // Use difficulty-adjusted timing windows
        if (timeDiff <= effectivePerfectWindow)
            ScoreEvents.Perfect();
        else if (timeDiff <= effectiveGoodWindow)
            ScoreEvents.Good();
        else
            ScoreEvents.Miss();

        // Animate: scale up + fade out, then return to pool
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOScale(1.3f, 0.15f).SetEase(Ease.OutQuad));
        seq.Join(canvasGroup.DOFade(0f, 0.15f));
        seq.OnComplete(ReturnToPool);
    }

    void ReturnToPool()
    {
        // Reset transform state so next reuse starts clean
        rt.localScale = Vector3.one;
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        gameObject.SetActive(false);
        pool.ReturnTile(this);
    }
}