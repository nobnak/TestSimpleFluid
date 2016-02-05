using UnityEngine;
using System.Collections;

namespace SimpleFluid {

    public class ForceField : MonoBehaviour {
        public const string PROP_DIR_AND_CENTER = "_DirAndCenter";
        public const string PROP_INV_RADIUS = "_InvRadius";

        public int width;
        public int height;
        public Collider viewCollider;
        public Material forceFieldMat;
        public float radius = 10f;

        float _forceThrottle = 0f;
        Vector3 _mousePos;
        RenderTexture _forceFieldTex;
    	
    	void Update () {
            if (_forceFieldTex == null || _forceFieldTex.width != width || _forceFieldTex.height != height) {
                Release();
                _forceFieldTex = new RenderTexture(width, height, 0, RenderTextureFormat.RGFloat);
            }

            var mousePos = Input.mousePosition;
            var dx = UpdateMousePos(mousePos);
            var forceVector = Vector2.zero;
            var uv = Vector2.zero;

            RaycastHit hit;
            if (Input.GetMouseButton (0)
                && viewCollider.Raycast (Camera.main.ScreenPointToRay (mousePos), out hit, float.MaxValue)) {
                forceVector = Vector2.ClampMagnitude ((Vector2)dx, 1f);
                uv = hit.textureCoord;
            }

            forceFieldMat.SetVector(PROP_DIR_AND_CENTER, 
                new Vector4(forceVector.x, forceVector.y, uv.x, uv.y));
            forceFieldMat.SetFloat(PROP_INV_RADIUS, 1f / radius);
            Graphics.Blit(null, _forceFieldTex, forceFieldMat);
        }
        void OnDestroy() {
            Release();
        }

        Vector3 UpdateMousePos (Vector3 mousePos) {
            var dx = mousePos - _mousePos;
            _mousePos = mousePos;
            return dx;
        }

        void Release() {
            Destroy(_forceFieldTex);
        }
    }
}