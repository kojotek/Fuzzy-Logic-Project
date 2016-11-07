using System;
using UnityEngine;
using System.Collections;
using FuzzyFramework.Dimensions;
using FuzzyFramework.Sets;
using FuzzyFramework;
using FuzzyFramework.Defuzzification;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car {
    [RequireComponent(typeof(CarController))]
    public class CarFuzzyController : MonoBehaviour {

        private CarController m_Car;

        public SimpleRaycaster rightRaycaster;
        public SimpleRaycaster leftRaycaster;

        private ContinuousDimension inputSpeed;
        private ContinuousDimension inputSideBoundDistance;
        private ContinuousDimension inputFrontBoundDistance;
        private ContinuousDimension outputGas;
        private ContinuousDimension outputBrakes;
        private DiscreteDimension outputAction;
        private ContinuousDimension outputSteeringWheel;

        private FuzzySet zeroSpeed;
        private FuzzySet lowSpeed;
        //private FuzzySet medSpeed;
        private FuzzySet highSpeed;

        private FuzzySet dangerousLeft;
        private FuzzySet dangerousRight;
        private FuzzySet safe;

        private FuzzySet turnRight;
        private FuzzySet turnLeft;
        private FuzzySet forward;

        private FuzzyRelation simpleSteeringRules;

        private void Awake() {
            m_Car = GetComponent<CarController>();

            inputSpeed = new ContinuousDimension("", "", "", -100, 180);
            zeroSpeed = new BellSet(inputSpeed, "", 0, 3, 5);
            lowSpeed = new QuadraticSet(inputSpeed, "", 10, 40, 0, 50, 5, 45);
            //private FuzzySet medSpeed;
            highSpeed = new LeftQuadraticSet(inputSpeed, "", 40, 45, 50);


            inputSideBoundDistance = new ContinuousDimension("", "", "", -10, 10);
            dangerousRight = new LeftLinearSet(inputSideBoundDistance, "", 4, 7);
            dangerousLeft = new RightLinearSet(inputSideBoundDistance, "", -7, -4);
            safe = new TrapezoidalSet(inputSideBoundDistance, "", -3, 3, -6, 6);


            inputFrontBoundDistance = new ContinuousDimension("", "", "", 0, 150);


            outputGas = new ContinuousDimension("", "", "", 0, 1);


            outputBrakes = new ContinuousDimension("", "", "", 0, 1);


            outputAction = new DiscreteDimension("","");


            outputSteeringWheel = new ContinuousDimension("", "", "", -1, 1);
            turnLeft = new RightLinearSet(outputSteeringWheel, "", -1, 0);
            turnRight = new RightLinearSet(outputSteeringWheel, "", -1, 0);

            simpleSteeringRules = (dangerousLeft / turnRight) % (dangerousRight / turnLeft);
        }


        private void FixedUpdate() {
            double zero = zeroSpeed.IsMember((decimal)m_Car.CurrentSpeed);
            double low = lowSpeed.IsMember((decimal)m_Car.CurrentSpeed);
            double high = highSpeed.IsMember((decimal)m_Car.CurrentSpeed);

            //Debug.Log("zero: " + zero + ", low: " + low + ", high: " + high);

            decimal r = (decimal)rightRaycaster.GetDistance();
            decimal l = (decimal)leftRaycaster.GetDistance();


            decimal carPosition = l;

            Debug.Log(carPosition);

            Defuzzification result = new CenterOfGravity(simpleSteeringRules, new Dictionary<IDimension, decimal>()
            {
                {inputSideBoundDistance, carPosition }

            });

            
            float h = (float)result.CrispValue;//CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");

            float handbrake = 0.0f;
            m_Car.Move(h, v, v, handbrake);
        }
    }
}
