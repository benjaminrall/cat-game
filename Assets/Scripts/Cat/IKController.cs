using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
    public float stepSpeed;
    public float maxRestDistance;

    public AnimationCurve stepCurve;

    public Transform FL;
    public Leg FLLeg;

    public Transform FR;
    public Leg FRLeg;

    public Transform BL;
    public Leg BLLeg;

    public Transform BR;
    public Leg BRLeg;

    public List<Leg> Legs;

    void Start()
    {
        FLLeg = new Leg(this.transform, FL, maxRestDistance, stepCurve, true, 0);
        FRLeg = new Leg(this.transform, FR, maxRestDistance, stepCurve, false, 0);

        BLLeg = new Leg(this.transform, BL, maxRestDistance, stepCurve, true, 1);
        BRLeg = new Leg(this.transform, BR, maxRestDistance, stepCurve, false, 1);

        Legs = new List<Leg>();

        Legs.Add(FLLeg);
        Legs.Add(FRLeg);
        Legs.Add(BLLeg);
        Legs.Add(BRLeg);
    }

    void FixedUpdate()
    {
        foreach (Leg leg in Legs)
        {
            leg.Update();
        }
    }
}

public class Leg
{
    public Transform parent;

    public Transform leg; // Leg Bone Transform
    public bool Left; // True if LeftSide
    public int legIndex; // Index of leg from front of Cat


    //public Vector3 currentLegPos; // Current Snapped Leg Position
    public Vector3 restPos; // Idle Leg Position

    public float maxDistFromRest; // Max Distance the leg can travel from Idle Position before it Steps
    public bool isStepping; // True if leg is currently stepping
    public Vector3 liftOffParentPosition;
    public Vector3 liftOffLegPosition;
    public Vector3 liftOffLegTargetPosition;
    public AnimationCurve stepCurve;
    public Leg(Transform parent, Transform leg, float maxRestDist, AnimationCurve stepCurve, bool isLeft, int legIndex)
    {
        // Passed
        this.parent = parent;

        this.leg = leg;
        this.maxDistFromRest = maxRestDist;

        this.Left = isLeft;
        this.legIndex = legIndex;
        this.restPos = this.leg.transform.position - this.parent.transform.position;
        this.stepCurve = stepCurve;

        // Defaults
        this.isStepping = false;

        // Leg Offset depending on the Leg
        float legOffset = 0;
        if (this.Left)
        {
            if (legIndex == 0) legOffset = this.maxDistFromRest;
            else if (legIndex == 1) legOffset = this.maxDistFromRest / 2;
        }
        else
        {
            if (legIndex == 0) legOffset = (-this.maxDistFromRest) / 2;
            else if (legIndex == 1) legOffset = -this.maxDistFromRest;
        }
        this.liftOffLegTargetPosition = new Vector3(this.leg.transform.position.x + legOffset,
                                             this.leg.transform.position.y,
                                             this.leg.transform.position.z);
    }

    public void Update()
    {
        float distance = this.leg.position.x - (this.parent.transform.position.x + this.restPos.x);

        if (Mathf.Abs(distance) > this.maxDistFromRest && !this.isStepping)
        {
            this.isStepping = true;

            this.liftOffLegPosition = this.leg.position;
            this.liftOffLegTargetPosition = new Vector3((this.parent.transform.position.x + this.restPos.x) + (1.5f * this.maxDistFromRest), 
                                                        this.leg.position.y,
                                                        this.leg.position.z);

            this.liftOffParentPosition = this.parent.position;
        }

        if (this.isStepping)
        {
            float percentage = (this.parent.position.x - this.liftOffParentPosition.x) / this.maxDistFromRest;

            Vector3 newPos = this.liftOffLegPosition + ((this.liftOffLegTargetPosition - this.liftOffLegPosition) * percentage);
            this.leg.position = new Vector3(newPos.x, newPos.y + this.stepCurve.Evaluate(percentage), newPos.z);

            if (percentage >= 1f)
            {
                this.isStepping = false;
                this.leg.position = this.liftOffLegTargetPosition;
            }
        }
        else
        {
            this.leg.position = this.liftOffLegTargetPosition;
        }
    }
}
