﻿using System.Collections.Generic;
using MotionGenerator;
using RLCreature.BodyGenerator.Manipulatables;
using UnityEngine;

namespace RLCreature.Sample.DesignedCreatures
{
    public class Joint : ManipulatableBase
    {
        const int maximumForce = 10000;
        const float positionDamper = 20;
        const float positionSpring = 1000;
        public float TargetForce;

        float targetForce;
        public List<float> targetAngle;
        int consumedFrames = 0;
        int manipulatableId;

        ConfigurableJoint joint;
        MotionSequence sequence = new MotionSequence();

        public void Start()
        {
            targetAngle = new List<float> {0, 0, 0};
            joint = GetComponent<ConfigurableJoint>();
        }

        public override void Manipulate(MotionSequence sequence)
        {
            consumedFrames = 0;
            this._isMoving = true;
            this.sequence = new MotionSequence(sequence);
        }

        void UpdateJointMotor(List<float> targetAngle)
        {
            if (targetAngle.Count != 3)
                throw new System.ArgumentException("need 3D input");
            targetForce = TargetForce;
            joint.targetRotation = Quaternion.Euler(
                targetAngle[0] * (joint.highAngularXLimit.limit - joint.lowAngularXLimit.limit)
                + joint.lowAngularXLimit.limit,
                (targetAngle[1] * 2f - 1f) * joint.angularYLimit.limit,
                (targetAngle[2] * 2f - 1f) * joint.angularZLimit.limit);

            JointDrive jd = joint.angularXDrive;
            jd.maximumForce = maximumForce;
            jd.positionDamper = positionDamper * targetForce;
            jd.positionSpring = positionSpring * targetForce;
            joint.angularXDrive = jd;
            JointDrive jdYZ = joint.angularYZDrive;
            jdYZ.maximumForce = maximumForce;
            jdYZ.positionDamper = positionDamper * targetForce;
            jdYZ.positionSpring = positionSpring * targetForce;
            joint.angularYZDrive = jdYZ;

            joint.projectionMode = JointProjectionMode.PositionAndRotation;
        }

        public void FixedUpdate()
        {
            if (sequence.Sequence.Count > 0)
            {
                this.targetAngle = sequence[0].value;
                if (sequence[0].time < consumedFrames)
                {
                    sequence.Sequence.RemoveAt(0);
                }
            }
            else
            {
                this._isMoving = false;
            }

            UpdateJointMotor(this.targetAngle);
            consumedFrames += 1;
        }

        public override int GetManipulatableDimention()
        {
            return 3;
        }
    }
}