using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class ComboUIController : MonoBehaviour
{

    [SerializeField] private Sprite promptArrow;
    [SerializeField] private Sprite pressedArrow;
    [SerializeField] private Sprite failArrow;

    [SerializeField] private Sprite promptLightAttack;
    [SerializeField] private Sprite pressedLightAttack;
    [SerializeField] private Sprite failLightAttack;

    [SerializeField] private Sprite promptHeavyAttack;
    [SerializeField] private Sprite pressedHeavyAttack;
    [SerializeField] private Sprite failHeavyAttack;

    [SerializeField] private Transform comboContainer;
    [SerializeField] private GameObject comboPanel;

    [SerializeField] List<GameObject> comboPrompts;

    [SerializeField] GameObject comboUIInputPrefab;

    [SerializeField] GameObject comboDestroyEffectPrefab;
    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioSource comboAudio;
    [SerializeField] Combo combo;

    [SerializeField] TMP_Text comboText;


    [SerializeField] AudioClip comboHit;
    [SerializeField] AudioClip comboFail;
    [SerializeField] AudioClip comboSuccess;

    int lastComboHit = -1;


    private float offset = 0;
    public float scrollSpeed = 100f;
    public float _arrowAnimSpeed = 1000f;

    Coroutine hidePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void DisplayCombo(Combo combo_)
    {
        combo = combo_;
        offset = 0;
        comboText.text = "";
        for (int i = 0; i < comboPrompts.Count; i++) 
        {
            if (comboPrompts[i] != null)
            {
                Destroy(comboPrompts[i]);
            }
        }
        comboPrompts.Clear();
        if (hidePanel != null)
        {
            StopCoroutine(hidePanel);
        }
        StopAllCoroutines();

        comboAudio.pitch = 1;
        lastComboHit = -1;
        comboPanel.SetActive(true);
        foreach (Combo.Input input in combo.sequence)
        {
            GameObject newInputObject = Instantiate(comboUIInputPrefab, comboContainer);


            switch (input)
            {
                case Combo.Input.Up:
                case Combo.Input.Left:
                case Combo.Input.Right:
                case Combo.Input.Down:
                    newInputObject.GetComponent<Image>().sprite = promptArrow;
                    break;
                case Combo.Input.LightAttack:
                    newInputObject.GetComponent<Image>().sprite = promptLightAttack;
                    break;
                case Combo.Input.HeavyAttack:
                    newInputObject.GetComponent<Image>().sprite = promptHeavyAttack;
                    break;
                case Combo.Input.None:
                default:
                    break;
            }
            switch (input)
            {
                case Combo.Input.Up:
                    break;
                case Combo.Input.Left:
                    newInputObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                    break;
                case Combo.Input.Right:
                    newInputObject.transform.rotation = Quaternion.Euler(0, 0, 270);
                    break;
                case Combo.Input.Down:
                    newInputObject.transform.rotation = Quaternion.Euler(0, 0, 180);
                    break;
                case Combo.Input.None:
                default:
                    break;
            }
            comboPrompts.Add(newInputObject);
            newInputObject.transform.localPosition = new Vector3(offset, 0, 0);
            offset += 300;
        }
    }
    public void OnComboHit(int index)
    {
        for (int i = lastComboHit+1; i < index + 1 ; i++)
        {
            comboPrompts[i].GetComponent<Image>().sprite = GetPressedSprite(combo.sequence[i]);
            GameObject particleObject = Instantiate(
            comboDestroyEffectPrefab, comboPrompts[i].transform.position, Quaternion.identity, comboPrompts[i].transform);

            particleObject.transform.localPosition = Vector3.zero;
            comboAudio.Stop();
            comboAudio.pitch += 0.1f;
            comboAudio.clip = comboHit;
            comboAudio.Play();
            UpdatePromptLocations();
            if (i < combo.sequence.Count)
            {
                StartCoroutine(ArrowFallAnimation(comboPrompts[i]));
            }
        }
        lastComboHit = index;
    }

    Sprite GetFailSprite(Combo.Input input)
    {
        switch (input)
        {
            case Combo.Input.Up:
            case Combo.Input.Left:
            case Combo.Input.Down:
            case Combo.Input.Right:
                return failArrow;
            case Combo.Input.LightAttack:
                return failLightAttack;
            case Combo.Input.HeavyAttack:
                return failHeavyAttack;
            default:
                return null;
        }
    }

    Sprite GetPressedSprite(Combo.Input input)
    {
        switch (input)
        {
            case Combo.Input.Up:
            case Combo.Input.Left:
            case Combo.Input.Down:
            case Combo.Input.Right:
                return pressedArrow;
            case Combo.Input.LightAttack:
                return pressedLightAttack;
            case Combo.Input.HeavyAttack:
                return pressedHeavyAttack;
            default:
                return null;
        }
    }
    public void UpdatePromptLocations()
    {
        foreach (GameObject prompt in comboPrompts)
        {
            if (prompt != null)
            {
                prompt.transform.localPosition = new Vector3(prompt.transform.localPosition.x - 300f,
                    prompt.transform.localPosition.y, prompt.transform.localPosition.z);
            }

        }
    }
    IEnumerator ArrowFallAnimation(GameObject target)
    {
        while (target.transform.localPosition.y > -1100f)
        {
            target.transform.position = new Vector3(target.transform.position.x - _arrowAnimSpeed * Time.deltaTime,
                target.transform.position.y - _arrowAnimSpeed * Time.deltaTime,
                target.transform.position.z);
            target.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 360),
                UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
            yield return null;
        }
    }

    IEnumerator DelayedHide(GameObject target, float delay)
    {
        float time = 0;
        while (time < delay)
        {
            time += Time.deltaTime;
            yield return null;
        }
        target.SetActive(false);
    }
    public void OnComboFail(int index)
    {
        comboPrompts[index].GetComponent<Image>().sprite = GetFailSprite(combo.sequence[index]);
        foreach( GameObject prompt in comboPrompts)
        {
            StartCoroutine(ArrowFallAnimation(prompt));
        }
        comboText.text = "You failed the combo :(";
        hidePanel = StartCoroutine(DelayedHide(comboPanel,2f));
        comboAudio.Stop();
        comboAudio.clip = comboFail;
        comboAudio.Play();
    }

    public void OnComboCompleted()
    {
        switch (combo.comboEffect.type)
        {
            case ComboEffect.Type.Speed:
                comboText.text = "SPEED BOOST";
                break;
            case ComboEffect.Type.Damage:
                comboText.text = "DAMAGE BOOST";
                break;
            case ComboEffect.Type.AttackSpeed:
                comboText.text = "ATTACK SPEED BOOST";
                break;

            case ComboEffect.Type.AttackSize:
                comboText.text = "ATTACK SIZE BOOST";
                break;

            default:
                break;
        }
        comboAudio.pitch = 1;
        comboAudio.clip = comboSuccess;
        comboAudio.Play();

        hidePanel = StartCoroutine(DelayedHide(comboPanel,2f));

    }
}
