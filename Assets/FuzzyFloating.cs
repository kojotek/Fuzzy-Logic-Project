using UnityEngine;
using System.Collections;
using FuzzyFramework.Dimensions;
using FuzzyFramework.Sets;
using FuzzyFramework;
using FuzzyFramework.Defuzzification;
using System.Collections.Generic;

public class FuzzyFloating : MonoBehaviour {

    public Rigidbody _rigidbody;

    ContinuousDimension altitude;// = new ContinuousDimension("Height", "Personal height", "cm", 100, 250);
    ContinuousDimension velocity;// = new ContinuousDimension("Suitability for basket ball", "0 = not good, 5 = very good", "grade", 0, 5);

    FuzzySet upSpeed;// = new LeftLinearSet(height, "Tall person", 170, 185);
    FuzzySet downSpeed;// = new LeftLinearSet(height, "Tall person", 170, 185);
    FuzzySet noSpeed;// = new LeftLinearSet(height, "Tall person", 170, 185);

    FuzzySet high;// = new LeftLinearSet(height, "Tall person", 170, 185);
    FuzzySet low;// = new LeftLinearSet(height, "Tall person", 170, 185);
    FuzzySet mid;// = new LeftLinearSet(height, "Tall person", 170, 185);

 
    FuzzyRelation term;// = (lanky & goodForBasket) | (!lanky & !goodForBasket);
    FuzzyRelation lowUp;
    FuzzyRelation highDown;
    FuzzyRelation midno;

    ContinuousDimension Adim;
    ContinuousDimension Bdim;
    ContinuousDimension Cdim;

    FuzzySet A;
    FuzzySet B;
    FuzzySet C;

    FuzzyRelation AlarsB;

    void Start () {
        _rigidbody = GetComponent<Rigidbody>();

        altitude = new ContinuousDimension("Altitude", "Altitude", "units", -100, 100);
        velocity = new ContinuousDimension("Velocity", "Velocity", "units/s", -50, 50);

        Adim = new ContinuousDimension("", "", "", 0, 10);
        Bdim = new ContinuousDimension("", "", "", 0, 10);
        Cdim = new ContinuousDimension("", "", "", 0, 10);

        A = new TriangularSet(Adim, "", 2, 0, 4);
        B = new TriangularSet(Bdim, "", 4, 3, 5);
        C = new TriangularSet(Cdim, "", 5, 0, 10);


        AlarsB = (A&B) / C;

        var projection = AlarsB.Project(new Dictionary<IDimension, decimal> { { Adim, 3 }, { Bdim, 4 } });

        for (decimal i = 0; i < 10; i += 1) {
            Debug.Log(i + "  " + projection.IsMember(i));
        }

        upSpeed = new LeftLinearSet(velocity, "up speed", 5, 30);
        downSpeed = new RightLinearSet(velocity, "down speed", -30, -5);
        noSpeed = new TriangularSet(velocity, "no speed", 0, -10, 10);
        high = new LeftLinearSet(altitude, "high", 5, 20);
        low = new RightLinearSet(altitude, "l", -20, -5);
        mid = new TriangularSet(altitude, "l", 0, -10, 10);

        term =  (high / downSpeed) %
                (low / upSpeed) %
                (mid / noSpeed);
/*
        lowUp = (low * upSpeed);
        highDown = (high * downSpeed);
        midno = mid * noSpeed;

        for (decimal x = -30; x<= 30; x++) {
            for (decimal y = -30; y <= 30 ; y++) {
                Debug.Log("low(" + x + ") && upSpeed(" + y + ") = " + lowUp.IsMember(new Dictionary<IDimension, decimal> { {altitude, x}, {velocity, y} }));
            }
           
        }
        */

    }
	
	// Update is called once per frame
	void Update () {

        Defuzzification result = new CenterOfGravity(
            term,
            new Dictionary<IDimension, decimal>{
                    { altitude, (decimal)transform.position.y}
            }
        );

        _rigidbody.AddForce(Vector3.up * (float)result.CrispValue);
        //Debug.Log((float)result.CrispValue);
    }        

}