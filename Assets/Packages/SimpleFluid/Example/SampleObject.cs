using UnityEngine;
using System.Collections;

namespace SimpleFluid {

    public class SampleObject : MonoBehaviour {
        public const string PROP_FLUID_TEX = "_FluidTex";
        public const string PROP_DT = "_Dt";

        public Solver solver;
        public Material advectMat;
        public Texture2D imageTex;

        public float timeScale = 1f;

        public Material[] outputMats;

    	RenderTexture _imageTex0;
    	RenderTexture _imageTex1;
        int _width = -1;
        int _height = -1;

        public int Width { get { return _width; }}
        public int Height { get { return _height; }}

        public void Start() {
            
    	}
    	void Update () {
            solver.Solve(Time.deltaTime * timeScale, imageTex.width, imageTex.height);
            #if true
    		NotifyResult(_imageTex0);
            #else
    		NotifyResult(_fluidTex0);
            #endif
    	}
        void OnDestroy() {
            Release();
        }

        void Release() {
    		Destroy (_imageTex0);
    		Destroy (_imageTex1);
        }
    	void NotifyResult(Texture outputTex) {
            foreach (var mat in outputMats)
    			mat.mainTexture = outputTex;
        }
    	void UpdateImage (float dt) {
    		advectMat.SetFloat (PROP_DT, dt);
            Graphics.Blit (_imageTex0, _imageTex1, advectMat);
    		Solver.Swap (ref _imageTex0, ref _imageTex1);
    	}
    }
}
