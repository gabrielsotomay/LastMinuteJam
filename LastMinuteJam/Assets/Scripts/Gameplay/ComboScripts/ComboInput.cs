using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;



    public class ComboInput : MonoBehaviour
    {
        private InputAction _up;
        private InputAction _down;
        private InputAction _right;
        private InputAction _left;

        [SerializeField] private TextMeshProUGUI textMesh;

        [SerializeField] private List<string> _combo;
        [SerializeField] private List<string> _currentCombo = new List<string>();

        private List<string> _comboOptions = new List<string> { "up", "down", "right", "left" };

        private int _easyDifficultyLength = 4;
        private int _mediumDifficultyLength = 9;
        private int _hardDifficultyLength = 14;
        [SerializeField] private int difficultyLevel;

        [SerializeField] private Sprite blueArrow;
        [SerializeField] private Sprite greenArrow;
        [SerializeField] private Sprite redArrow;
        private float _arrowAnimSpeed = 1000f;

        [SerializeField] private Transform spawnPosition;

        private List<GameObject> _comboImages = new List<GameObject>();
        private int counter = 0;
        private float offset = 0;
        private float scrollSpeed = 100f;

        [SerializeField] private GameObject _comboDestroyAnim;

        [SerializeField] private AudioSource _audioSource;

        [SerializeField] public bool _playComboEvent = true;
        
        

        void Start()
        {
            _up = InputSystem.actions.FindAction("Up");
            _down = InputSystem.actions.FindAction("Down");
            _right = InputSystem.actions.FindAction("Right");
            _left = InputSystem.actions.FindAction("Left");

        }

        // Update is called once per frame
        void Update()
        {

            if (_playComboEvent)
            {

                _up.performed += ctx => ProcessInput("up");
                _right.performed += ctx => ProcessInput("right");
                _left.performed += ctx => ProcessInput("left");
                _down.performed += ctx => ProcessInput("down");


            }

            //PositionAndScrollComboUI();
        }

        public void NoScrollUI()
        {
            foreach (var image in _comboImages)
            {
                if (image != null)
                {
                    image.transform.localPosition = new Vector3(image.transform.localPosition.x - 300f,
                        image.transform.localPosition.y, image.transform.localPosition.z);
                }

            }
        }

        // Scroll the combo UI
        public void PositionAndScrollComboUI()
        {

            {
                foreach (var image in _comboImages)
                {


                    if (_comboImages[_comboImages.Count() - 1] == null)
                    {
                        break;
                    }

                    else
                    {
                        image.transform.localPosition =
                            new Vector3(image.transform.localPosition.x - scrollSpeed * Time.deltaTime, 0, 0);

                    }

                }
            }

        }

        public void DisplayCombo()
        {
            foreach (var direction in _combo)
            {
                GameObject imageObject = new GameObject(direction + "Image");
                _comboImages.Add(imageObject);
                imageObject.transform.SetParent(spawnPosition);
                imageObject.transform.localScale = Vector3.one;
                imageObject.transform.localPosition = new Vector3(offset, 0, 0);
                offset += 300;
                Image image = imageObject.AddComponent<Image>();

                image.transform.localScale = new Vector3(2f, 2f, 2f);

                switch (direction)
                {
                    case "up":
                        image.sprite = blueArrow;
                        break;
                    case "down":
                        imageObject.transform.rotation = Quaternion.Euler(0, 0, 180);
                        image.sprite = blueArrow;

                        break;
                    case "right":
                        imageObject.transform.rotation = Quaternion.Euler(0, 0, 270);
                        image.sprite = blueArrow;
                        break;
                    case "left":
                        imageObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                        image.sprite = blueArrow;
                        break;
                }
                
            }
        }

        // take input of difficulty (1, 2 or 3) this will increase the length of combo
        public void GenerateRandomCombo(int difficulty)
        {
            switch (difficulty)
            {
                case 1:
                    for (int i = 0; i < _easyDifficultyLength; i++)
                    {
                        _combo.Add(_comboOptions[UnityEngine.Random.Range(0, _comboOptions.Count - 1)]);
                    }

                    break;

                case 2:
                    for (int i = 0; i < _mediumDifficultyLength; i++)
                    {
                        _combo.Add(_comboOptions[UnityEngine.Random.Range(0, _comboOptions.Count - 1)]);
                    }

                    break;

                case 3:
                    for (int i = 0; i < _hardDifficultyLength; i++)
                    {
                        _combo.Add(_comboOptions[UnityEngine.Random.Range(0, _comboOptions.Count - 1)]);
                    }

                    break;
            }

        }

        public void ProcessInput(String input)
        {
            _currentCombo.Add(input);

            if (_currentCombo[counter] == _combo[counter] && counter < _comboImages.Count)
            {
                _comboImages[counter].GetComponent<Image>().sprite = greenArrow;


                GameObject particleObject = Instantiate(
                    _comboDestroyAnim, _comboImages[counter].transform.position, Quaternion.identity);

                particleObject.transform.SetParent(_comboImages[counter].transform, false);

                particleObject.transform.localPosition = Vector3.zero;

                _audioSource.Play();
                NoScrollUI();
                if (counter != _comboImages.Count)
                {
                    StartCoroutine(ArrowFallAnimation(_comboImages[counter]));
                }

                counter++;
            }
            else
            {
                _comboImages[counter].GetComponent<Image>().sprite = redArrow;
            }

            Debug.Log(_comboImages.Count);
            if (counter == _combo.Count)
            {
                for (int i = 0; i < counter; i++)
                {
                    
                    Destroy(_comboImages[i]);
                }

                Reset();
                textMesh.text = "You completed the combo!";
            }
        }

        // reset the variables for next combo event
        public void Reset()
        {
            StopAllCoroutines();
            _combo.Clear();
            _currentCombo.Clear();
            _comboImages.Clear();
            counter = 0;
            offset = 0;
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

        public void FailAnimation()
        {

        }

    }

    