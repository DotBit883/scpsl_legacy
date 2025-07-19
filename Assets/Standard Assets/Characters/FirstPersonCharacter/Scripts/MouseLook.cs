using System;
using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;

        public float sensitivityMultiplier = 1f;

        public bool isOpenEq;

        public bool scp106_eq;

        private Quaternion m_CharacterTargetRot;

        private Quaternion m_CameraTargetRot;

        private Transform charact;

        private Transform cam;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }


        public void SetRotation(Quaternion rot)
        {
            LookRotation(charact, cam, rot.eulerAngles.y);
        }
        
        public void LookRotation(Transform character, Transform camera, float setPos = 0f, float overrideY = 0f, float overrideX = 0f, bool isRecoil = false)
        {
            charact = character;
            cam = camera;
            float num = 0f;
            float num2 = 0f;
            if (isRecoil)
            {
                num2 = overrideX;
                num = overrideY;
            }
            else
            {
                num = sensitivityMultiplier * Input.GetAxis("Mouse X") * XSensitivity * Sensitivity.sens * (float)((Cursor.lockState == CursorLockMode.Locked) ? 1 : 0);
                num2 = sensitivityMultiplier * Input.GetAxis("Mouse Y") * YSensitivity * Sensitivity.sens * (float)((Cursor.lockState == CursorLockMode.Locked) ? 1 : 0);
            }
            m_CharacterTargetRot *= Quaternion.Euler(0f, num + setPos, 0f);
            m_CameraTargetRot *= Quaternion.Euler(0f - num2, 0f, 0f);
            if (clampVerticalRotation)
            {
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
            }
            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, smoothTime * Time.fixedDeltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot, smoothTime * Time.fixedDeltaTime);
                return;
            }
            character.localRotation = m_CharacterTargetRot;
            if (!float.IsNaN(m_CameraTargetRot.x))
            {
                camera.localRotation = m_CameraTargetRot;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}