using UnityEngine;
using System.Collections;

namespace SimpleFluid {
    [RequireComponent(typeof(Camera))]
    public class FluidEffect : MonoBehaviour {
        public const string PROP_FLUID_TEX = "_FluidTex";
        public const string PROP_IMAGE_TEX = "_ImageTex";
		public const string PROP_REF_TEX = "_RefTex";
        public const string PROP_DT = "_Dt";

		public Solver solver;

        public Material advectMat;
        public Material lerpMat;
        public Material fluidEffectMat;

		public int lod = 1;

        Camera _attachedCamera;
    	RenderTexture _imageTex0;
		RenderTexture _imageTex1;

        public static void Clear(RenderTexture target, Color bg) {
            var active = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear (true, true, bg);
            RenderTexture.active = active;
        }

        void Start() {
            _attachedCamera = GetComponent<Camera> ();
            Init ();
        }
        void Update() {
            var dt = solver.DeltaTime;
            Init ();
            solver.Solve(dt);
            UpdateImage (dt);
        }
		void OnRenderImage(RenderTexture src, RenderTexture dst) {
			lerpMat.SetTexture (PROP_REF_TEX, src);
			Graphics.Blit (_imageTex0, _imageTex1, lerpMat);
			Solver.Swap (ref _imageTex0, ref _imageTex1);

            fluidEffectMat.SetTexture (PROP_IMAGE_TEX, _imageTex0);
            Graphics.Blit (src, dst, fluidEffectMat);
		}
        void OnDestroy() {
            Release();
        }

		void Release() {
			ReleaseImage ();
        }

        void Init () {
            var width = _attachedCamera.pixelWidth;
            var height = _attachedCamera.pixelHeight;
            solver.SetSize (width >> lod, height >> lod);
            InitOrResizeImage (width, height);
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
		void UpdateImage (float dt) {
			solver.SetProperties (advectMat, PROP_FLUID_TEX);
    		advectMat.SetFloat (PROP_DT, dt);
            Graphics.Blit (_imageTex0, _imageTex1, advectMat);
    		Solver.Swap (ref _imageTex0, ref _imageTex1);
    	}
        void ReleaseImage () {
            Destroy (_imageTex0);
            Destroy (_imageTex1);
        }
    }
}
