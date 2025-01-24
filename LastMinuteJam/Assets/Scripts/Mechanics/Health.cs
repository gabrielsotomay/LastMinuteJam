using System;
using Platformer.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Represebts the current vital statistics of some game entity.
    /// </summary>
    public class Health : MonoBehaviour
    {

        public class PlayerHurtEventArgs : EventArgs { public float oldHealth; public float newHealth; }
        public delegate void OnPlayerHurtEvent(object source, PlayerHurtEventArgs e);
        public event OnPlayerHurtEvent OnPlayerHurt;

        public class PlayerDamagedEventArgs : EventArgs {public float newHealth; }
        public delegate void OnPlayerDamagedEvent(object source, PlayerDamagedEventArgs e);
        public event OnPlayerDamagedEvent OnPlayerDamaged;



        /// <summary>
        /// The maximum hit points for the entity.
        /// </summary>
        public float maxHP = 1;

        /// <summary>
        /// Indicates if the entity should be considered 'alive'.
        /// </summary>
        public bool IsAlive => currentHP > 0;

        public float currentHP;

        float cumulativeDamage = 0;
        private Image healthBar;


        public void Start()
        {
            
        }

        /// <summary>
        /// Increment the HP of the entity.
        /// </summary>
        public void Increment()
        {
            currentHP = Mathf.Clamp(currentHP + 1, 0, maxHP);
        }

        /// <summary>
        /// Decrement the HP of the entity. Will trigger a HealthIsZero event when
        /// current HP reaches 0.
        /// </summary>
        /// 
        public void Hurt(float damage)
        {
            cumulativeDamage += damage;
            float newHp = Mathf.Clamp(currentHP - damage, 0, maxHP);
            OnPlayerHurt?.Invoke(this, new PlayerHurtEventArgs { oldHealth = currentHP / maxHP, newHealth = newHp / maxHP });
        }

        public void TakeDamage()
        {
            float newHp = Mathf.Clamp(currentHP - cumulativeDamage, 0, maxHP);
            OnPlayerDamaged?.Invoke(this, new PlayerDamagedEventArgs { newHealth = newHp / maxHP });
            currentHP = newHp;
            cumulativeDamage = 0;
        }
        public void Decrement()
        {
            float newHp = Mathf.Clamp(currentHP - 1, 0, maxHP);
            currentHP = newHp;
            if (currentHP == 0)
            {
                //var ev = Schedule<HealthIsZero>();
                //ev.health = this;
            }
        }

        public void DisplayHealthBar()
        {
            healthBar.fillAmount = currentHP / maxHP;
        }
        /// <summary>
        /// Decrement the HP of the entitiy until HP reaches 0.
        /// </summary>
        public void Die()
        {
            while (currentHP > 0) Decrement();

        }

        void Awake()
        {
            currentHP = maxHP;
        }

        

    }
}
