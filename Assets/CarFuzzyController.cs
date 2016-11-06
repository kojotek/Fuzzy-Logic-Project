using System;
using UnityEngine;
using System.Collections;
using FuzzyFramework.Dimensions;
using FuzzyFramework.Sets;
using FuzzyFramework;
using FuzzyFramework.Defuzzification;
using System.Collections.Generic;

namespace UnityStandardAssets.Vehicles.Car {
    [RequireComponent(typeof(CarController))]
    public class CarFuzzyController : MonoBehaviour {

        private CarController m_Car;

        ContinuousDimension inputSpeed;
        ContinuousDimension inputRightBoundDistance;
        ContinuousDimension inputLeftBoundDistance;
        ContinuousDimension inputFrontBoundDistance;
        ContinuousDimension outputAction;
        ContinuousDimension outputSteeringWheel;

        FuzzySet zeroSpeed;
        FuzzySet lowSpeed;
        FuzzySet medSpeed;
        FuzzySet highSpeed;



        FuzzyRelation AlarsB;

        private void Awake() {
            m_Car = GetComponent<CarController>();
        }


        private void FixedUpdate() {
            
            

            float h = 0.5f;//CrossPlatformInputManager.GetAxis("Horizontal");
            float v = -0.3f;//CrossPlatformInputManager.GetAxis("Vertical");

            float handbrake = 0.0f;
            m_Car.Move(h, v, v, handbrake);
        }
    }
}
