using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
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

    [SerializeField] List<GameObject> comboPrompts;

    [SerializeField] GameObject comboUIInputPrefab;

    [SerializeField] GameObject comboDestroyEffectPrefab;
    [SerializeField] AudioSource audioSource;

    [SerializeField] Combo combo;

    [SerializeField] TMP_Text comboText;

    private float offset = 0;
    public float scrollSpeed = 100f;
    public float _arrowAnimSpeed = 1000f;


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
        for (int i = 0; i < comboPrompts.Count; i++) 
        {
            if (comboPrompts[i] != null)
            {
                Destroy(comboPrompts[i]);
            }
        }
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
        comboPrompts[index].GetComponent<Image>().sprite = GetPressedSprite(combo.sequence[index]);
        GameObject particleObject = Instantiate(
        comboDestroyEffectPrefab, comboPrompts[index].transform.position, Quaternion.identity, comboPrompts[index].transform);
        
        particleObject.transform.localPosition = Vector3.zero;

        audioSource.Play();
        UpdatePromptLocations();
        if (index < combo.sequence.Count)
        {
            StartCoroutine(ArrowFallAnimation(comboPrompts[index]));
        }
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
        Destroy(target);
    }

    public void OnComboFail(int index)
    {
        comboPrompts[index].GetComponent<Image>().sprite = GetFailSprite(combo.sequence[index]);
        foreach( GameObject prompt in comboPrompts)
        {
            StartCoroutine(ArrowFallAnimation(prompt));
        }
        comboText.text = "You failed the combo :(";
    }

    public void OnComboCompleted()
    {
        comboText.text = "You completed the combo!";

    }
}
