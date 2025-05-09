using DG.Tweening;
using Netick;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Platformer.Mechanics;
public class HealthBarControllerUI : MonoBehaviour
{
    public Image fullHealthBar;
    public NetworkPlayer player;
    public Image emptyHealthBar;
    public Image damageHealthBar;
    public Image playerIcon;
    public HorizontalLayoutGroup HorizontalLayoutGroup;
    public TMP_Text playerName;
    bool isLeft;
    float _oldHealth = 0f;

    Tween damageTween;
    public void Init(bool isLeft_, Sprite playerIcon_, Sprite emptyHealthBar_, Sprite fullHealthBar_, Sprite damageHealthBar_)
    {
        if (!isLeft_)
        {
            HorizontalLayoutGroup.reverseArrangement = true;
            isLeft = isLeft_;
            damageHealthBar.fillOrigin = 0;
            fullHealthBar.fillOrigin = 1;
            damageHealthBar.transform.localScale = new Vector3(-damageHealthBar.transform.localScale.x, damageHealthBar.transform.localScale.y, damageHealthBar.transform.localScale.z);
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.pivot = Vector3.one;
            rectTransform.anchorMin = Vector2.one;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            HorizontalLayoutGroup.reverseArrangement = false;
            isLeft = isLeft_;
            damageHealthBar.fillOrigin = 0;
            fullHealthBar.fillOrigin = 0;
        }
        playerIcon.sprite = playerIcon_;
        fullHealthBar.sprite = fullHealthBar_;
        emptyHealthBar.sprite = emptyHealthBar_;
        damageHealthBar.sprite = damageHealthBar_;

    }

    public void ShowDamage(object o, Health.PlayerHurtEventArgs e)
    {
        
        float _oldHealth = e.oldHealth;

        damageHealthBar.fillAmount = e.oldHealth;
        damageTween = DOTween.To(() => e.oldHealth, x => fullHealthBar.fillAmount = x, e.newHealth, 0.5f);
        damageTween.SetEase(Ease.OutCubic);
    }

    public void ShowHealth(object o, Health.PlayerDamagedEventArgs e)
    {
        damageTween.Complete();
        damageTween = DOTween.To(() => _oldHealth, x => damageHealthBar.fillAmount = x, e.newHealth, 0.2f);
        damageTween.SetEase(Ease.OutCubic);
        //damageHealthBar.fillAmount = health;
        fullHealthBar.fillAmount = e.newHealth;
    }
}
