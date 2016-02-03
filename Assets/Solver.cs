using UnityEngine;
using System.Collections;

public class Solver : MonoBehaviour {
    public enum ViewModeEnum { Null = 0, Velocity, Density }

    public const string KW_VIEW_VELOCITY = "VIEW_VELOCITY";
    public const string KW_VIEW_DENSITY  = "VIEW_DENSITY";

    public const string PROP_FIELD_TEX = "_MainTex";
    public const string PROP_BOUNDARY_TEX = "_BoundaryTex";
    public const string PROP_FORCE_TEX = "_ForceTex";
    public const string PROP_DT = "_Dt";
    public const string PROP_K_VIS = "_KVis";
    public const string PROP_S = "_S";
    public const string PROP_FORCE_POWER = "_ForcePower";

    public const float rho0 = 1f;
    public const float dx = 1f;

    public Shader solverShader;
    public KeyCode forceKey = KeyCode.Space;
    public Texture2D forceTex;

    public ViewModeEnum viewMode;
    public int width = 256;
    public int height = 256;

    public float k = 0.12f;
    public float vis = 1.8e-5f;
    public float forcePower = 0.1f;

    public Material[] outputMats;

    Texture2D _initTex;
    Texture2D _boundaryTex;
    RenderTexture _fieldTex0;
    RenderTexture _fieldTex1;
    Material _solverMat;
    float _forceThrottle = 0f;

	void Start () {
        InitSolver();
	}
	void Update () {
        UpdateKeyword();
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
        Destroy(_fieldTex0);
        Destroy(_fieldTex1);
    }
    void InitSolver() {
        ReleaseSolver();

        _solverMat = new Material(solverShader);
        _initTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        _boundaryTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        _fieldTex0 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        _fieldTex1 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        _fieldTex0.wrapMode = _fieldTex1.wrapMode = _initTex.wrapMode = TextureWrapMode.Clamp;
        _fieldTex0.filterMode = _fieldTex1.filterMode = _initTex.filterMode = FilterMode.Bilinear;

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
        Graphics.Blit(_initTex, _fieldTex0);
    }
    void UpdateSolver(float dt) {
        var kvis = vis / rho0;
        var s = k * (dx * dx) / (dt * rho0);
        var f = forcePower * _forceThrottle;

        _solverMat.SetTexture(PROP_FIELD_TEX, _fieldTex0);
        _solverMat.SetTexture(PROP_BOUNDARY_TEX, _boundaryTex);
        _solverMat.SetTexture(PROP_FORCE_TEX, forceTex);
        _solverMat.SetFloat(PROP_DT, dt);
        _solverMat.SetFloat(PROP_K_VIS, kvis);
        _solverMat.SetFloat(PROP_S, s);
        _solverMat.SetFloat(PROP_FORCE_POWER, f);
        Graphics.Blit(_fieldTex0, _fieldTex1, _solverMat);
        Swap();
    }
    void NotifyResult() {
        foreach (var mat in outputMats)
            mat.mainTexture = _fieldTex0;
    }
    void Swap() {
        var tmpField = _fieldTex0; _fieldTex0 = _fieldTex1; _fieldTex1 = tmpField;
    }

    void UpdateKeyword() {
        Shader.DisableKeyword(KW_VIEW_VELOCITY);
        Shader.DisableKeyword(KW_VIEW_DENSITY);
        switch (viewMode) {
        case ViewModeEnum.Velocity:
            Shader.EnableKeyword(KW_VIEW_VELOCITY);
            break;
        case ViewModeEnum.Density:
            Shader.EnableKeyword(KW_VIEW_DENSITY);
            break;
        }
    }

}
