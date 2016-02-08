using UnityEngine;
using System.Collections;

namespace SimpleFluid {

    public class FluidEffect : MonoBehaviour {
        public const string PROP_FLUID_TEX = "_FluidTex";
		public const string PROP_REFERENCE_TEX = "_RefTex";
        public const string PROP_DT = "_Dt";

		public const string PROP_DIR_AND_CENTER = "_DirAndCenter";
		public const string PROP_INV_RADIUS = "_InvRadius";

		public Solver solver;

		public Collider viewCollider;
		public Material forceFieldMat;
		public float radius = 10f;

        public Material advectMat;
        public Texture2D imageTex;

		public int lod = 1;
        public float timeScale = 1f;

        public Material[] outputMats;

    	RenderTexture _imageTex0;
		RenderTexture _imageTex1;

		Vector3 _mousePos;
		RenderTexture _forceFieldTex;

    	void Update () {
			var dt = Time.deltaTime * timeScale;
			var width = imageTex.width >> lod;
			var height = imageTex.height >> lod;

			UpdateForceField (width, height);
			solver.forceTex = _forceFieldTex;
            solver.Solve(dt, width, height);
			UpdateImage (dt);
		}
        void OnDestroy() {
            Release();
        }

		void Release() {
			ReleaseForceField ();
			ReleaseImage ();
        }

		void ReleaseForceField () {
			Destroy (_forceFieldTex);
		}
		void ReleaseImage () {
			Destroy (_imageTex0);
			Destroy (_imageTex1);
		}
		void UpdateImage (float dt) {
			InitImage ();
			solver.SetProperties (advectMat, PROP_FLUID_TEX);
			advectMat.SetTexture (PROP_REFERENCE_TEX, imageTex);
    		advectMat.SetFloat (PROP_DT, dt);
            Graphics.Blit (_imageTex0, _imageTex1, advectMat);
    		Solver.Swap (ref _imageTex0, ref _imageTex1);
    	}
		void InitImage(int width, int height) {
			if (_imageTex0 == null || _imageTex0.width != width || _imageTex0.height != height) {
				ReleaseImage();
				_imageTex0 = new RenderTexture (width, height, 0, RenderTextureFormat.ARGBFloat);
				_imageTex1 = new RenderTexture (width, height, 0, RenderTextureFormat.ARGBFloat);
				_imageTex0.filterMode = _imageTex1.filterMode = FilterMode.Bilinear;
				_imageTex0.wrapMode = _imageTex1.wrapMode = TextureWrapMode.Clamp;
				var active = RenderTexture.active;
				RenderTexture.active = _imageTex0;
				GL.Clear (true, true, Color.clear);
				RenderTexture.active = active;
			}
		}
		void UpdateForceField(int width, int height) {
			if (_forceFieldTex == null || _forceFieldTex.width != width || _forceFieldTex.height != height) {
				ReleaseForceField();
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
		Vector3 UpdateMousePos (Vector3 mousePos) {
			var dx = mousePos - _mousePos;
			_mousePos = mousePos;
			return dx;
		}
    }
}
