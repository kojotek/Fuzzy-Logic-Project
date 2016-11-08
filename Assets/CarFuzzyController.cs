using System;
using UnityEngine;
using System.Collections;
using FuzzyFramework.Dimensions;
using FuzzyFramework.Sets;
using FuzzyFramework;
using FuzzyFramework.Defuzzification;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using UnityEditor;

namespace UnityStandardAssets.Vehicles.Car {
    [RequireComponent(typeof(CarController))]
    public class CarFuzzyController : MonoBehaviour {

        private CarController m_Car;

        float memberLeft;
        float memberRight;
        float memberCenter;

        float memberFar;
        float memberClose;
        float memberDangerouslyClose;

        float memberHighSpeed;
        float memberLowSpeed;
        float memberZeroSpeed;

        float crispSteeringWheel;
        float crispGas;

        public SimpleRaycaster rightRaycaster;
        public SimpleRaycaster leftRaycaster;
        public SimpleRaycaster frontRaycaster;

        private ContinuousDimension inputSpeed;
        private ContinuousDimension inputSideBoundDistance;
        private ContinuousDimension inputFrontBoundDistance;
        private ContinuousDimension outputGas;
        private ContinuousDimension outputBrakes;
        private DiscreteDimension outputAction;
        private ContinuousDimension outputSteeringWheel;

        private FuzzySet backwardSpeed;
        private FuzzySet zeroSpeed;
        private FuzzySet lowSpeed;
        //private FuzzySet medSpeed;
        private FuzzySet highSpeed;

        private FuzzySet dangerousLeft;
        private FuzzySet dangerousRight;
        private FuzzySet safe;

        private FuzzySet far;
        private FuzzySet close;
        private FuzzySet dangerousClose;

        private FuzzySet pedalToMetal;
        private FuzzySet clutch;
        private FuzzySet brake;

        private FuzzySet turnRight;
        private FuzzySet turnLeft;
        private FuzzySet forward;

        private FuzzyRelation simpleSteeringRules;
        private FuzzyRelation simpleGasRules;

        private void Awake() {
            m_Car = GetComponent<CarController>();

            inputSpeed = new ContinuousDimension("", "", "", -100, 180);
                backwardSpeed = new RightQuadraticSet(inputSpeed, "", -100, -10, 0);
                zeroSpeed = new BellSet(inputSpeed, "", 0, 5, 10);
                lowSpeed = new QuadraticSet(inputSpeed, "", 10, 40, 0, 50, 5, 45);
                //private FuzzySet medSpeed;
                highSpeed = new LeftQuadraticSet(inputSpeed, "", 40, 45, 50);


            inputSideBoundDistance = new ContinuousDimension("", "", "", -10, 10);
                dangerousRight = new LeftQuadraticSet(inputSideBoundDistance, "", 0, 9, 10);
                dangerousLeft = new RightQuadraticSet(inputSideBoundDistance, "", -10, -9, 0);
                safe = new TrapezoidalSet(inputSideBoundDistance, "", -5, 5, -7, 7);


            inputFrontBoundDistance = new ContinuousDimension("", "", "", 0, 150);
                far = new LeftLinearSet(inputFrontBoundDistance, "", 20, 40);
                close = new TrapezoidalSet(inputFrontBoundDistance, "", 15, 20, 10, 25);
                dangerousClose = new RightLinearSet(inputFrontBoundDistance, "", 10, 15);


            outputGas = new ContinuousDimension("", "", "", -100, 100);
                pedalToMetal = new LeftQuadraticSet(outputGas, "", 0, 80, 100);
                clutch = new SingletonSet(outputGas, "", 0);
                brake = new RightLinearSet(outputGas,"", -100, 0);


            outputBrakes = new ContinuousDimension("", "", "", 0, 1);


            outputAction = new DiscreteDimension("","");


            outputSteeringWheel = new ContinuousDimension("", "", "", -100, 100);
                turnLeft = new RightLinearSet(outputSteeringWheel, "", -100, 0);
                turnRight = new LeftLinearSet(outputSteeringWheel, "", 0, 100);
                forward = new TriangularSet(outputSteeringWheel, "", 0, -10, 10);


            simpleSteeringRules =
                (dangerousLeft & turnRight) |
                (dangerousRight & turnLeft) |
                (safe & forward);

            simpleGasRules =
                (far & pedalToMetal) |
                ((close & lowSpeed) & clutch) |
                ((close & highSpeed) & brake) |
                ((close & zeroSpeed) & brake) |
                (dangerousClose & brake);


        }


        private void FixedUpdate() {
            /*
            double zero = zeroSpeed.IsMember((decimal)m_Car.CurrentSpeed);
            double low = lowSpeed.IsMember((decimal)m_Car.CurrentSpeed);
            double high = highSpeed.IsMember((decimal)m_Car.CurrentSpeed);
            */
            //Debug.Log("zero: " + zero + ", low: " + low + ", high: " + high);

            float steeringValue = ComputeSteering(
                leftRaycaster.GetDistance(),
                rightRaycaster.GetDistance(),
                simpleSteeringRules);

            float gasValue = ComputeGas(
                 frontRaycaster.GetDistance(),
                 simpleGasRules);



            float h = steeringValue/100.0f; //CrossPlatformInputManager.GetAxis("Horizontal");
            float v = gasValue / 100.0f;   //CrossPlatformInputManager.GetAxis("Vertical");

            float handbrake = CrossPlatformInputManager.GetAxis("Jump");

            m_Car.Move(h, v, v, handbrake);
        }

        private float ComputeSteering(float distToLeftBound, float distToRightBound, FuzzyRelation relation) {

            float r = Mathf.Clamp(distToRightBound, 0.0f, 20.0f);
            float l = Mathf.Clamp(distToLeftBound, 0.0f, 20.0f);
            float middle = (l+r) / 2;

            float fCarPosition;

            if (r < middle) {
                fCarPosition = 10.0f - (r / middle) * 10.0f;
            }
            else {
                fCarPosition = -10.0f + (l / middle) * 10.0f;
            }

            decimal carPosition = (decimal)fCarPosition;

            memberLeft = (float)dangerousLeft.IsMember(carPosition);
            memberRight = (float)dangerousRight.IsMember(carPosition);
            memberCenter = (float)safe.IsMember(carPosition);

            Defuzzification result = new CenterOfGravity(relation, new Dictionary<IDimension, decimal>()
            {
                {inputSideBoundDistance, carPosition }

            });

            crispSteeringWheel = (float)result.CrispValue;

            return (float)result.CrispValue;
        }


        private float ComputeGas(float distanceToFrontBound, FuzzyRelation relation) {

            decimal distance = (decimal)Mathf.Clamp(distanceToFrontBound, 0.0f, 150.0f);

            memberDangerouslyClose = (float)dangerousClose.IsMember(distance);
            memberClose = (float)close.IsMember(distance);
            memberFar = (float)far.IsMember(distance);

            memberZeroSpeed = (float)zeroSpeed.IsMember((decimal)m_Car.CurrentSpeed);
            memberLowSpeed = (float)lowSpeed.IsMember((decimal)m_Car.CurrentSpeed);
            memberHighSpeed = (float)highSpeed.IsMember((decimal)m_Car.CurrentSpeed);


            /*
            FuzzySet projection = simpleSteeringRules.Project(new Dictionary<IDimension, decimal>()
            {
                {inputSideBoundDistance, distance }
            });

            string str = "";

            for (decimal i = -100; i <= 100; i += 20) {
                str += i + ": " + projection.IsMember(i) + ",  ";
            }
            Debug.Log(str);
            */

            Defuzzification result = new CenterOfGravity(relation, new Dictionary<IDimension, decimal>()
            {
                {inputFrontBoundDistance, distance},
                {inputSpeed, (decimal)m_Car.CurrentSpeed }
            });

            //Debug.Log("distance: " + distance + ", gas: " + result.CrispValue);

            crispGas = (float)result.CrispValue;
            return (float)result.CrispValue;
        }


        void OnGUI() {

            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(10, 40, 150, 20), "Distance to side bounds:", style);

            style.normal.textColor = new Color(1.0f - memberLeft, memberLeft, 0.0f);
            GUI.Label(new Rect(10, 60, 150, 20), "LEFT - " + memberLeft, style);

            style.normal.textColor = new Color(1.0f - memberCenter, memberCenter, 0.0f);
            GUI.Label(new Rect(10, 80, 150, 20), "CENTER - " + memberCenter, style);

            style.normal.textColor = new Color(1.0f - memberRight, memberRight, 0.0f);
            GUI.Label(new Rect(10, 100, 150, 20), "RIGHT - " + memberRight, style);



            style.normal.textColor = Color.white;
            GUI.Label(new Rect(280, 40, 150, 20), "Distance to front bound:", style);

            style.normal.textColor = new Color(1.0f - memberDangerouslyClose, memberDangerouslyClose, 0.0f);
            GUI.Label(new Rect(280, 60, 150, 20), "VERY CLOSE - " + memberDangerouslyClose, style);

            style.normal.textColor = new Color(1.0f - memberClose, memberClose, 0.0f);
            GUI.Label(new Rect(280, 80, 150, 20), "CLOSE - " + memberClose, style);

            style.normal.textColor = new Color(1.0f - memberFar, memberFar, 0.0f);
            GUI.Label(new Rect(280, 100, 150, 20), "FAR - " + memberFar, style);




            style.normal.textColor = Color.white;
            GUI.Label(new Rect(540, 40, 150, 20), "Speedometer:", style);

            style.normal.textColor = new Color(1.0f - memberZeroSpeed, memberZeroSpeed, 0.0f);
            GUI.Label(new Rect(540, 60, 150, 20), "ZERO SPEED - " + memberDangerouslyClose, style);

            style.normal.textColor = new Color(1.0f - memberLowSpeed, memberLowSpeed, 0.0f);
            GUI.Label(new Rect(540, 80, 150, 20), "LOW SPEED - " + memberLowSpeed, style);

            style.normal.textColor = new Color(1.0f - memberHighSpeed, memberHighSpeed, 0.0f);
            GUI.Label(new Rect(540, 100, 150, 20), "HIGH SPEED - " + memberHighSpeed, style);




            style.normal.textColor = new Color(crispSteeringWheel / 100.0f, 1.0f - crispSteeringWheel/100.0f, 0.0f);
            string str = crispSteeringWheel < 0 ? "LEFT " + Mathf.Abs(crispSteeringWheel) : "RIGHT " + Mathf.Abs(crispSteeringWheel);
            GUI.Label(new Rect(50, Screen.height - 50, 150, 20), "STEERING WHEEL - " + str + "%", style);

            style.normal.textColor = new Color(1.0f - Mathf.Abs(crispGas) / 100.0f, Mathf.Abs(crispGas) / 100.0f, 0.0f);
            str = crispGas >= 0 ? "GO " + Mathf.Abs(crispGas) : "BRAKE " + Mathf.Abs(crispGas);
            GUI.Label(new Rect(500, Screen.height - 50, 150, 20), "GAS - " + crispGas + "%", style);
        }
    }
}
