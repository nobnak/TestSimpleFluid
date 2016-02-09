using UnityEngine;
using System.Collections;

namespace SimpleFluid {
    [RequireComponent(typeof(Camera))]
    public class FluidEffect : MonoBehaviour {
        public const string PROP_FLUID_TEX = "_FluidTex";
        public const string PROP_IMAGE_TEX = "_ImageTex";
        public const string PROP_DT = "_Dt";

		public const string PROP_DIR_AND_CENTER = "_DirAndCenter";
		public const string PROP_INV_RADIUS = "_InvRadius";

		public Solver solver;

        public Material advectMat;
        public Material lerpMat;
		public Material forceFieldMat;
        public Material fluidEffectMat;
		public float forceRadius = 0.05f;

		public int lod = 1;
        public float timeScale = 10f;

        public Material forceFieldViewerMat;

        Camera _attachedCamera;
        Vector3 _mousePos;
    	RenderTexture _imageTex0;
		RenderTexture _imageTex1;
        RenderTexture _forceFieldTex;

        public static void Clear(RenderTexture target, Color bg) {
            var active = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear (true, true, bg);
            RenderTexture.active = active;
        }

        void Start() {
            _attachedCamera = GetComponent<Camera> ();
        }
        void Update() {
            var dt = Time.deltaTime * timeScale;
            var width = _attachedCamera.pixelWidth;
            var height = _attachedCamera.pixelHeight;
            var lodWidth = width >> lod;
            var lodHeight = height >> lod;

            InitOrResizeForceField (lodWidth, lodHeight);
            InitOrResizeImage(width, height);

            UpdateForceField();
            solver.forceTex = _forceFieldTex;
            solver.Solve(dt, lodWidth, lodHeight);
            UpdateImage (dt);
        }
		void OnRenderImage(RenderTexture src, RenderTexture dst) {
            Graphics.Blit (src, _imageTex0, lerpMat);

            #if true
            Graphics.Blit (_imageTex0, dst);
            //Graphics.Blit(_forceFieldTex, dst, forceFieldViewerMat);
            #else

            fluidEffectMat.SetTexture (PROP_IMAGE_TEX, _imageTex0);
            Graphics.Blit (src, dst, fluidEffectMat);
            #endif
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
			solver.SetProperties (advectMat, PROP_FLUID_TEX);
    		advectMat.SetFloat (PROP_DT, dt);
            Graphics.Blit (_imageTex0, _imageTex1, advectMat);
    		Solver.Swap (ref _imageTex0, ref _imageTex1);
    	}
        void InitOrResizeImage(int width, int height) {
			if (_imageTex0 == null || _imageTex0.width != width || _imageTex0.height != height) {
				ReleaseImage();
				_imageTex0 = new RenderTexture (width, height, 0, RenderTextureFormat.ARGBFloat);
				_imageTex1 = new RenderTexture (width, height, 0, RenderTextureFormat.ARGBFloat);
				_imageTex0.filterMode = _imageTex1.filterMode = FilterMode.Bilinear;
				_imageTex0.wrapMode = _imageTex1.wrapMode = TextureWrapMode.Clamp;
                Clear(_imageTex0, Color.clear);
			}
		}
		void UpdateForceField() {
			var mousePos = Input.mousePosition;
			var dx = UpdateMousePos(mousePos);
			var forceVector = Vector2.zero;
			var uv = Vector2.zero;

			if (Input.GetMouseButton (0)) {
				uv = Camera.main.ScreenToViewportPoint (mousePos);
				forceVector = Vector2.ClampMagnitude ((Vector2)dx, 1f);
			}

			forceFieldMat.SetVector(PROP_DIR_AND_CENTER, 
				new Vector4(forceVector.x, forceVector.y, uv.x, uv.y));
			forceFieldMat.SetFloat(PROP_INV_RADIUS, 1f / forceRadius);
			Graphics.Blit(null, _forceFieldTex, forceFieldMat);
		}

        void InitOrResizeForceField (int width, int height) {
            if (_forceFieldTex == null || _forceFieldTex.width != width || _forceFieldTex.height != height) {
                ReleaseForceField ();
                _forceFieldTex = new RenderTexture (width, height, 0, RenderTextureFormat.RGFloat);
            }
        }

		Vector3 UpdateMousePos (Vector3 mousePos) {
			var dx = mousePos - _mousePos;
			_mousePos = mousePos;
			return dx;
		}
    }
}
