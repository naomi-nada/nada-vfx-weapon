using UnityEngine;

namespace NADA.VFX.Weapon.Editor
{
    /// <summary>
    /// Editor-only character preview camera.
    ///
    /// The normal GameCamera update is suppressed while the NADA editor is
    /// open. This controller temporarily owns only the camera presentation
    /// during that window and restores the captured pose when editing ends.
    /// </summary>
    internal static class NadaVfxEditorPreviewCamera
    {
        private const float FocusHeight = 1.25f;

        private const float DefaultDistance = 4f;
        private const float MinimumDistance = 1.75f;
        private const float MaximumDistance = 7.5f;

        private const float MinimumPitch = -20f;
        private const float MaximumPitch = 70f;

        private const float OrbitSensitivity = 3.5f;
        private const float ZoomSensitivity = 1.25f;

        private static Camera _camera;

        private static bool _active;

        private static Vector3 _capturedPosition;
        private static Quaternion _capturedRotation;

        private static float _yaw;
        private static float _pitch;
        private static float _distance;

        internal static bool IsActive =>
            _active;

        internal static void Tick()
        {
            if (!NadaVfxEditor.IsOpen)
            {
                Deactivate();
                return;
            }

            if (!_active)
            {
                if (!TryActivate())
                    return;
            }

            if (_camera == null ||
                Player.m_localPlayer == null)
            {
                Deactivate();
                return;
            }

            ReadOrbitInput();
            ApplyCameraPose();
        }

        internal static void Deactivate()
        {
            if (!_active)
                return;

            if (_camera != null)
            {
                try
                {
                    _camera.transform.position =
                        _capturedPosition;

                    _camera.transform.rotation =
                        _capturedRotation;
                }
                catch
                {
                }
            }

            _camera =
                null;

            _active =
                false;

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorPreviewCamera] deactivated.");
        }

        private static bool TryActivate()
        {
            global::Player player =
                Player.m_localPlayer;

            if (player == null)
                return false;

            Camera camera =
                Camera.main;

            if (camera == null)
                return false;

            _camera =
                camera;

            _capturedPosition =
                camera.transform.position;

            _capturedRotation =
                camera.transform.rotation;

            Vector3 focusPoint =
                ResolveFocusPoint(
                    player);

            Vector3 offset =
                camera.transform.position -
                focusPoint;

            float capturedDistance =
                offset.magnitude;

            if (capturedDistance <=
                0.01f)
            {
                _distance =
                    DefaultDistance;

                _yaw =
                    player.transform.eulerAngles.y +
                    180f;

                _pitch =
                    15f;
            }
            else
            {
                Vector3 direction =
                    offset /
                    capturedDistance;

                _distance =
                    Mathf.Clamp(
                        capturedDistance,
                        MinimumDistance,
                        MaximumDistance);

                _yaw =
                    Mathf.Atan2(
                        direction.x,
                        direction.z) *
                    Mathf.Rad2Deg;

                _pitch =
                    Mathf.Asin(
                        Mathf.Clamp(
                            direction.y,
                            -1f,
                            1f)) *
                    Mathf.Rad2Deg;

                _pitch =
                    Mathf.Clamp(
                        _pitch,
                        MinimumPitch,
                        MaximumPitch);
            }

            _active =
                true;

            ApplyCameraPose();

            Plugin.Log?.LogInfo(
                $"{Plugin.ModName}: [EditorPreviewCamera] " +
                $"activated " +
                $"distance={_distance:0.00} " +
                $"yaw={_yaw:0.0} " +
                $"pitch={_pitch:0.0}.");

            return true;
        }

        private static void ReadOrbitInput()
        {
            if (!Input.GetMouseButton(1))
                return;

            float mouseX =
                Input.GetAxis(
                    "Mouse X");

            float mouseY =
                Input.GetAxis(
                    "Mouse Y");

            _yaw +=
                mouseX *
                OrbitSensitivity;

            _pitch -=
                mouseY *
                OrbitSensitivity;

            _pitch =
                Mathf.Clamp(
                    _pitch,
                    MinimumPitch,
                    MaximumPitch);

            float scroll =
                Input.GetAxis(
                    "Mouse ScrollWheel");

            if (!Mathf.Approximately(
                    scroll,
                    0f))
            {
                _distance -=
                    scroll *
                    ZoomSensitivity;

                _distance =
                    Mathf.Clamp(
                        _distance,
                        MinimumDistance,
                        MaximumDistance);
            }
        }

        private static void ApplyCameraPose()
        {
            if (_camera == null ||
                Player.m_localPlayer == null)
            {
                return;
            }

            Vector3 focusPoint =
                ResolveFocusPoint(
                    Player.m_localPlayer);

            float yawRadians =
                _yaw *
                Mathf.Deg2Rad;

            float pitchRadians =
                _pitch *
                Mathf.Deg2Rad;

            float horizontalLength =
                Mathf.Cos(
                    pitchRadians);

            Vector3 direction =
                new Vector3(
                    horizontalLength *
                    Mathf.Sin(
                        yawRadians),

                    Mathf.Sin(
                        pitchRadians),

                    horizontalLength *
                    Mathf.Cos(
                        yawRadians));

            Vector3 cameraPosition =
                focusPoint +
                direction *
                _distance;

            Vector3 lookDirection =
                focusPoint -
                cameraPosition;

            if (lookDirection.sqrMagnitude <=
                0.0001f)
            {
                return;
            }

            _camera.transform.position =
                cameraPosition;

            _camera.transform.rotation =
                Quaternion.LookRotation(
                    lookDirection.normalized,
                    Vector3.up);
        }

        private static Vector3 ResolveFocusPoint(
            global::Player player)
        {
            if (player == null)
                return Vector3.zero;

            return
                player.transform.position +
                Vector3.up *
                FocusHeight;
        }
    }
}