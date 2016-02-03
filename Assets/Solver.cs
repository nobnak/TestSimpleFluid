using UnityEngine;
using System.Collections;

public class Solver : MonoBehaviour {
    public const int SOLVER_FLUID = 0;
    public const int SOLVER_ADVECT = 1;

    public const string PROP_FLUID_TEX = "_FluidTex";
    public const string PROP_BOUNDARY_TEX = "_BoundaryTex";
    public const string PROP_FORCE_TEX = "_ForceTex";
    public const string PROP_VISUAL_TEX = "_VisualTex";
    public const string PROP_DT = "_Dt";
    public const string PROP_K_VIS = "_KVis";
    public const string PROP_S = "_S";
    public const string PROP_FORCE_POWER = "_ForcePower";

    public const float rho0 = 1f;
    public const float dx = 1f;

    public Shader solverShader;
    public KeyCode forceKey = KeyCode.Space;
    public Texture2D forceTex;

    public int width = 256;
    public int height = 256;

    public float k = 0.12f;
    public float vis = 1.8e-5f;
    public Vector2 forcePower = new Vector2(0.1f, 0.1f);

    public Material[] outputMats;

    Texture2D _initTex;
    Texture2D _boundaryTex;
    RenderTexture _fluidTex0;
    RenderTexture _fluidTex1;
    Material _solverMat;
    float _forceThrottle = 0f;

	void Start () {
        InitSolver();
	}
	void Update () {
        _forceThrottle = (Input.GetKey(forceKey) ? 1f : 0f);
	}
    void FixedUpdate() {
        UpdateSolver(Time.fixedDeltaTime);
        NotifyResult();
    }
    void OnDestroy() {
        ReleaseSolver();
    }

    void ReleaseSolver() {
        Destroy(_solverMat);
        Destroy(_initTex);
        Destroy(_boundaryTex);
        Destroy(_fluidTex0);
        Destroy(_fluidTex1);
    }
    void InitSolver() {
        ReleaseSolver();

        _solverMat = new Material(solverShader);
        _initTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        _boundaryTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        _fluidTex0 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        _fluidTex1 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        _fluidTex0.wrapMode = _fluidTex1.wrapMode = _initTex.wrapMode = TextureWrapMode.Clamp;
        _fluidTex0.filterMode = _fluidTex1.filterMode = _initTex.filterMode = FilterMode.Bilinear;

        var initData = _initTex.GetPixels();
        var boundaryData = _boundaryTex.GetPixels();
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var i = x + y * width;
                initData[i] = new Color(0f, 0f, 0f, rho0);
                boundaryData[i] = new Color(
                    (x == 0 || x == (width-1)) ? 0f : 1f,
                    (y == 0 || y == (height-1)) ? 0f : 1f,
                    1f, 1f);
            }
        }
        _initTex.SetPixels(initData);
        _boundaryTex.SetPixels(boundaryData);
        _initTex.Apply();
        _boundaryTex.Apply();
        Graphics.Blit(_initTex, _fluidTex0);
    }
    void UpdateSolver(float dt) {
        var kvis = vis / rho0;
        var s = k * (dx * dx) / (dt * rho0);
        var f = (Vector4)(forcePower * _forceThrottle);

        _solverMat.SetTexture(PROP_FLUID_TEX, _fluidTex0);
        _solverMat.SetTexture(PROP_FLUID_TEX, _fluidTex0);
        _solverMat.SetTexture(PROP_BOUNDARY_TEX, _boundaryTex);
        _solverMat.SetTexture(PROP_FORCE_TEX, forceTex);
        _solverMat.SetFloat(PROP_DT, dt);
        _solverMat.SetFloat(PROP_K_VIS, kvis);
        _solverMat.SetFloat(PROP_S, s);
        _solverMat.SetVector(PROP_FORCE_POWER, f);
        Graphics.Blit(null, _fluidTex1, _solverMat, SOLVER_FLUID);
        Swap();
    }
    void NotifyResult() {
        foreach (var mat in outputMats)
            mat.mainTexture = _fluidTex0;
    }
    void Swap() {
        var tmpField = _fluidTex0; _fluidTex0 = _fluidTex1; _fluidTex1 = tmpField;
    }

}
