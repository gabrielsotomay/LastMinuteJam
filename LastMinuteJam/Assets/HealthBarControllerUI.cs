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
    public Sprite healthyPlayerIcon;
    public Sprite hurtPlayerIcon;
    public Sprite damagedPlayerIcon;
    public HorizontalLayoutGroup HorizontalLayoutGroup;
    public TMP_Text playerName;
    bool isLeft;
    float _oldHealth = 0f;

    Tween damageTween;
    public void Init(bool isLeft_, Sprite playerIcon_, Sprite hurtPlayerIcon_, Sprite damagedPlayerIcon_,  Sprite emptyHealthBar_, Sprite fullHealthBar_, Sprite damageHealthBar_, string playerName_)
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
        playerName.text = playerName_;
        healthyPlayerIcon = playerIcon_;
        hurtPlayerIcon = hurtPlayerIcon_;
        damagedPlayerIcon = damagedPlayerIcon_;
        playerIcon.sprite = playerIcon_;
        fullHealthBar.sprite = fullHealthBar_;
        emptyHealthBar.sprite = emptyHealthBar_;
        damageHealthBar.sprite = damageHealthBar_;

    }

    public void ShowDamage(object o, Health.PlayerHurtEventArgs e)
    {
        
        _oldHealth = e.oldHealth;

        damageHealthBar.fillAmount = e.oldHealth;
        damageTween = DOTween.To(() => fullHealthBar.fillAmount, x => fullHealthBar.fillAmount = x, e.newHealth, 0.5f);
        damageTween.SetEase(Ease.OutCubic);
    }

    public void ShowHealth(object o, Health.PlayerDamagedEventArgs e)
    {
        damageTween.Complete();
        damageTween = DOTween.To(() => _oldHealth, x => damageHealthBar.fillAmount = x, e.newHealth, 0.2f);
        damageTween.SetEase(Ease.OutCubic);
        //damageHealthBar.fillAmount = health;
        fullHealthBar.fillAmount = e.newHealth;
        _oldHealth = e.newHealth;
        UpdateIcon(e.newHealth);
    }

    public void UpdateIcon(float health)
    {
        if (health > 0.6)
        {
            playerIcon.sprite = healthyPlayerIcon;
        }
        else if (health > 0.2)
        {
            playerIcon.sprite = hurtPlayerIcon;
        }
        else
        {
            playerIcon.sprite = damagedPlayerIcon;
        }
    }
}
