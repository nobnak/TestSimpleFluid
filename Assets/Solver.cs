using UnityEngine;
using System.Collections;

public class Solver : MonoBehaviour {
    public const int SOLVER_FLUID = 0;
    public const int SOLVER_ADVECT = 1;

    public const string PROP_FLUID_TEX = "_FluidTex";
    public const string PROP_BOUNDARY_TEX = "_BoundaryTex";
    public const string PROP_FORCE_TEX = "_ForceTex";
	public const string PROP_IMAGE_TEX = "_ImageTex";
    public const string PROP_DT = "_Dt";
    public const string PROP_K_VIS = "_KVis";
    public const string PROP_S = "_S";
    public const string PROP_FORCE_POWER = "_ForcePower";

    public const float rho0 = 1f;
    public const float dx = 1f;

    public Shader solverShader;
    public KeyCode forceKey = KeyCode.Space;
    public Texture2D forceTex;
	public Texture2D imageTex;
	public Collider viewCollider;

    public int width = 256;
    public int height = 256;

    public float k = 0.12f;
    public float vis = 1.8e-5f;
	public float forcePower = 1f;
	public float timeStep = 0.1f;

    public Material[] outputMats;

    Texture2D _initTex;
    Texture2D _boundaryTex;
    RenderTexture _fluidTex0;
    RenderTexture _fluidTex1;
	RenderTexture _imageTex0;
	RenderTexture _imageTex1;
    Material _solverMat;
    float _forceThrottle = 0f;
	Vector2 _forceVector;
	Vector2 _forceTexOffset = Vector2.zero;
	Vector3 _mousePos;

	void Start () {
        InitSolver();
	}
	void Update () {
		var mousePos = Input.mousePosition;
		var dx = UpdateMousePos(mousePos);
        var dt = timeStep;
		_forceThrottle = 0f;

		RaycastHit hit;
		if (Input.GetMouseButton (0)
				&& viewCollider.Raycast (Camera.main.ScreenPointToRay (mousePos), out hit, float.MaxValue)) {
			_forceThrottle = 1f;
			_forceVector = forcePower * Vector2.ClampMagnitude ((Vector2)dx, 1f);
			_forceTexOffset = -hit.textureCoord + new Vector2 (0.5f, 0.5f);
		}

        UpdateSolver(dt);
        UpdateImage(dt);
        #if true
		NotifyResult(_imageTex0);
        #else
		NotifyResult(_fluidTex0);
        #endif
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
		Destroy (_imageTex0);
		Destroy (_imageTex1);
    }
    void InitSolver() {
        ReleaseSolver();

        _solverMat = new Material(solverShader);
        _initTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        _boundaryTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        _fluidTex0 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        _fluidTex1 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
		_imageTex0 = new RenderTexture (imageTex.width, imageTex.height, 0, RenderTextureFormat.ARGB32);
		_imageTex1 = new RenderTexture (imageTex.width, imageTex.height, 0, RenderTextureFormat.ARGB32);
		_fluidTex0.wrapMode = _fluidTex1.wrapMode = _imageTex0.wrapMode = _imageTex1.wrapMode
			= _initTex.wrapMode = TextureWrapMode.Clamp;
		_fluidTex0.filterMode = _fluidTex1.filterMode = _imageTex0.filterMode = _imageTex1.filterMode
			= _initTex.filterMode = FilterMode.Bilinear;

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
		Graphics.Blit (imageTex, _imageTex0);
    }
    void UpdateSolver(float dt) {
        var kvis = vis / rho0;
        var s = k * (dx * dx) / (dt * rho0);
		var f = (Vector4)(_forceVector * _forceThrottle);

        _solverMat.SetTexture(PROP_FLUID_TEX, _fluidTex0);
        _solverMat.SetTexture(PROP_BOUNDARY_TEX, _boundaryTex);
        _solverMat.SetTexture(PROP_FORCE_TEX, forceTex);
		_solverMat.SetTextureOffset (PROP_FORCE_TEX, _forceTexOffset);
        _solverMat.SetFloat(PROP_DT, dt);
        _solverMat.SetFloat(PROP_K_VIS, kvis);
        _solverMat.SetFloat(PROP_S, s);
        _solverMat.SetVector(PROP_FORCE_POWER, f);
		Graphics.Blit (null, _fluidTex1, _solverMat, SOLVER_FLUID);
		Swap (ref _fluidTex0, ref _fluidTex1);
    }
	void NotifyResult(Texture outputTex) {
        foreach (var mat in outputMats)
			mat.mainTexture = outputTex;
    }
	void Swap<T>(ref T t0, ref T t1) { var tmp = t0; t0 = t1; t1 = tmp; }

	Vector3 UpdateMousePos (Vector3 mousePos) {
		var dx = mousePos - _mousePos;
		_mousePos = mousePos;
		return dx;
	}

	void UpdateImage (float dt) {
		var fluidStepSize = new Vector4 (1f / width, 1f / height, 0f, 0f);

		_solverMat.SetTexture (PROP_FLUID_TEX, _fluidTex0);
		_solverMat.SetTexture (PROP_IMAGE_TEX, _imageTex0);
		_solverMat.SetFloat (PROP_DT, dt);
		Graphics.Blit (null, _imageTex1, _solverMat, SOLVER_ADVECT);
		Swap (ref _imageTex0, ref _imageTex1);
	}
}
