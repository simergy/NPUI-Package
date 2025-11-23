using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.UI;

namespace NP_UI
{
    public class CubesManagerMenu : NpGenericMenu
    {
        public Generator GeneratorManager;
        private NP_Label counterLable;
        private LabelData counterLabelData;
        private LabelData SliderValueLabel;
        private LabelData SliderValueRaduisLabel;
        private SliderData SliderValueSliderData;
        private SliderData SliderDataRadius;
        private string numberOfCubesText = "Number of Cubes: ";
        private int numberOfObjects = 0;
        private LabelData SeperatorLabel;

        private Dictionary<DynamicObjectsTests, ButtonData> _dynamicObjectsTestsMap;

        public override void CreateMenuItems()
        {
            counterLabelData = new LabelData(numberOfCubesText);
            
            SliderValueSliderData = new SliderData(1,1,100);
            SliderValueLabel = new LabelData("0"); 
            SliderDataRadius = new SliderData(1,1,100);
            SliderValueRaduisLabel = new LabelData("0");

            ButtonData StartGeneratorButton = new ButtonData(StartGenerator, "Start Generator", Color.green);
            ButtonData StopGeneratorButton = new ButtonData(StopGenerator, "Stop Generator", Color.yellow);
            
            genericUIDatas =
                counterLabelData + SliderValueLabel + SliderValueSliderData + SliderValueRaduisLabel +
                SliderDataRadius + StartGeneratorButton + StopGeneratorButton;
            _dynamicObjectsTestsMap = new Dictionary<DynamicObjectsTests, ButtonData>();
        }

        public override void StartAfterCreation()
        {
            InitializeFields();
            AddListener();
        }

        private void UpdateUI()
        {
            ClearUI(false);
            AddElementsByDataToMenu(genericUIDatas, _elementsSystem);
            InitializeFields();
            UpdateCount(numberOfObjects);
        }

        private void UpdateCount(int i)
        {
             ((NP_Label)counterLabelData.GetUIElement()).SetText(numberOfCubesText + i.ToString());
        }

        private void InitializeFields()
        {
            counterLable = counterLabelData.GetUIElement() as NP_Label;
            NP_Slider slider = SliderValueSliderData.GetUIElement() as NP_Slider;
            NP_Label label = SliderValueLabel.GetUIElement() as NP_Label;
            
            NP_Slider sliderRadius = SliderDataRadius.GetUIElement() as NP_Slider;
            NP_Label labelRadius = SliderValueRaduisLabel.GetUIElement() as NP_Label;
            
            slider.SetOnClick(value => label.SetText(value.ToString()));
            sliderRadius.SetOnClick(value => labelRadius.SetText(value.ToString()));
            
            GeneratorManager = FindFirstObjectByType<Generator>(FindObjectsInactive.Include);
        }

        private void StartGenerator()
        {
            int numberOfCubes = 0;
            GeneratorManager.gameObject.SetActive(true);
            NP_Slider numberOfCubesSlider = SliderValueSliderData.GetUIElement() as NP_Slider;
            if (numberOfCubesSlider != null)
            {
                numberOfCubes = (int)numberOfCubesSlider.GetValue();
            }

            int radius = 0;
            NP_Slider sliderRadius = SliderDataRadius.GetUIElement() as NP_Slider;
            if (sliderRadius != null)
            {
                radius = (int)sliderRadius.GetValue();
            }

            CreateSeperatorIfNeeded();

            StartGeneratorByData(numberOfCubes, radius);

            UpdateUI();
        }

        private void CreateSeperatorIfNeeded()
        {
            string seperatorText = "Created Objects";
            SeperatorLabel = new LabelData(seperatorText, 27);
            Func<GenericUIData, bool> isSeperator = (x => x is LabelData && ((LabelData)x).Text == seperatorText);
            if (genericUIDatas.FirstOrDefault(isSeperator) == null)
            {
                genericUIDatas += SeperatorLabel;
            }
        }

        private void StartGeneratorByData(int numberOfCubes, int radius)
        {
            GeneratorManager.numberOfSpheres = numberOfCubes;
            GeneratorManager.radius = radius;
            GeneratorManager.CreateAllDynamicObjects();
        }

        private void StopGenerator()
        {
            Generator.DestroyEvent.Invoke();
            if (SeperatorLabel != null)
            {
                genericUIDatas -= SeperatorLabel;
            }
        }

        private void AddListener()
        {
            Generator.NewObjectAddedEvent.AddListener(NewObjectAddedEvent);
            Generator.ObjectDestroyedEvent.AddListener(ObjectRemovedEvent);
        }

        private void NewObjectAddedEvent(DynamicObjectsTests dynamicObjectsTests)
        {
            counterLable.SetText(numberOfCubesText + ++numberOfObjects);
            ButtonData newLabelData = new ButtonData(dynamicObjectsTests.DestroySelf, "Destroy Object", Color.red);
            genericUIDatas += newLabelData;
            if (!_dynamicObjectsTestsMap.ContainsKey(dynamicObjectsTests))
            {
                _dynamicObjectsTestsMap.Add(dynamicObjectsTests, newLabelData);
            }
        }

        private void ObjectRemovedEvent(DynamicObjectsTests dynamicObjectsTests)
        {
            counterLable.SetText(numberOfCubesText + --numberOfObjects);

            if (_dynamicObjectsTestsMap.ContainsKey(dynamicObjectsTests))
            {
                genericUIDatas -= _dynamicObjectsTestsMap[dynamicObjectsTests];
                NP_Button npButton = _dynamicObjectsTestsMap[dynamicObjectsTests].GetUIElement() as NP_Button;
                if (npButton != null)
                {
                    Destroy(npButton.gameObject);
                }

                _dynamicObjectsTestsMap.Remove(dynamicObjectsTests);
            }
        }

        public override void CloseMenu()
        {
            ClearUI(true);
            MenuGameObject.SetActive(false);
        }

        public override void OpenMenu()
        {
            CreateUI();
            MenuGameObject.SetActive(true);
        }

    }
    
}