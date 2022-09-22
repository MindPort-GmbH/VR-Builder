// Copyright (c) 2016 Unity Technologies. MIT license - license_unity.txt
// #NVJOB Water Shaders. MIT license - license_nvjob.txt
// #NVJOB Water Shaders v2.0 - https://nvjob.github.io/unity/nvjob-water-shaders-v2
// #NVJOB Nicholas Veselov - https://nvjob.github.io
// Support this asset - https://nvjob.github.io/donate


using UnityEngine;
using System.Collections;

[HelpURL("https://nvjob.github.io/unity/nvjob-water-shaders-v2")]
[AddComponentMenu("#NVJOB/Water Shaders V2")]


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


[ExecuteInEditMode]
public class NVWaterShaders : MonoBehaviour
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public Vector2 rotateSpeed = new Vector2(0.4f, 0.4f);
    public Vector2 rotateDistance = new Vector2(2.0f, 2.0f);
    public bool depthTextureModeOn = true;
    public bool waterSyncWind;
    public Transform windZone;
    public bool mirrorOn = false;
    public bool mirrorBackSide = false;
    public int textureSize = 1024;
    public Vector3 clipPlaneOffset = new Vector3(0.1f, 0.1f, 10);
    public LayerMask reflectLayers = -1;
    public bool garbageCollection = false;

    //--------------

    Transform thisTransform;
    Vector2 wVectorX, wVectorY;
    Vector3 ccLastpos;
    Renderer thisRenderer;
    Camera currentCamera, reflectionCamera;
    Hashtable reflectionCameras = new Hashtable();
    RenderTexture reflectionTexture = null;
    int oldReflectionTextureSize;
    static bool insideRendering = false;


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void OnEnable()
    {
        //--------------

        thisTransform = transform;
        thisRenderer = GetComponent<Renderer>();
        wVectorX = Vector2.zero;
        wVectorY = Vector2.zero;
        if (depthTextureModeOn) Camera.main.depthTextureMode = DepthTextureMode.Depth;

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void OnWillRenderObject()
    {
        //--------------

        if (mirrorOn == true) MirrorReflection();

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void OnDisable()
    {
        //--------------

        if (mirrorOn == true)
        {
            if (reflectionTexture)
            {
                DestroyImmediate(reflectionTexture);
                reflectionTexture = null;
            }
            foreach (DictionaryEntry kvp in reflectionCameras) DestroyImmediate(((Camera)kvp.Value).gameObject);
            reflectionCameras.Clear();
        }

        //--------------

        if (garbageCollection == true)
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void LateUpdate()
    {
        //--------------

        if (waterSyncWind == false)
        {
            wVectorX = Quaternion.AngleAxis(Time.time * rotateSpeed.x, Vector3.forward) * Vector2.one * rotateDistance.x;
            wVectorY = Quaternion.AngleAxis(Time.time * rotateSpeed.y, Vector3.forward) * Vector2.one * rotateDistance.y;
            if (windZone != null) windZone.rotation = Quaternion.LookRotation(new Vector3(wVectorY.x, 0, wVectorY.y), Vector3.zero) * Quaternion.Euler(0, -90, 0);
        }
        else
        {
            if (windZone != null)
            {
                Quaternion windQ = new Quaternion(windZone.rotation.x, windZone.rotation.z, windZone.rotation.y, -windZone.rotation.w);
                Vector3 windV = windQ * Vector3.up * 0.2f;
                wVectorX = windV * Time.time * rotateSpeed.x;
                wVectorY = windV * Time.time * rotateSpeed.y;
            }
        }

        Shader.SetGlobalVector("_NvWatersMovement", new Vector4(wVectorX.x, wVectorX.y, wVectorY.x, wVectorY.y));

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void MirrorReflection()
    {
        //--------------

        if (!enabled || !thisRenderer.enabled || !thisRenderer || !thisRenderer.sharedMaterial) return;
        currentCamera = Camera.current;
        if (!currentCamera) return;
        if (insideRendering) return;
        insideRendering = true;
        Transform ccTransform = currentCamera.transform;
        ccLastpos = ccTransform.position;
        CreateMirrorObjects(currentCamera, out reflectionCamera);
        Vector3 pos = thisTransform.position;
        Vector3 normal = thisTransform.up;
        if (mirrorBackSide == true) normal = -normal;
        UpdateCamera(currentCamera, reflectionCamera);
        float d = -Vector3.Dot(normal, pos) - clipPlaneOffset.x;
        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, new Vector4(normal.x, normal.y, normal.z, d));
        Vector3 newpos = reflection.MultiplyPoint(ccLastpos);
        reflectionCamera.worldToCameraMatrix = currentCamera.worldToCameraMatrix * reflection;
        Matrix4x4 projection = currentCamera.projectionMatrix;
        CalculateObliqueMatrix(ref projection, ClipPlane(reflectionCamera, pos, normal, 1.0f, clipPlaneOffset.x));
        reflectionCamera.projectionMatrix = projection;
        reflectionCamera.cullingMask = ~(1 << 4) & reflectLayers.value;
        reflectionCamera.targetTexture = reflectionTexture;
        GL.invertCulling = true;
        reflectionCamera.transform.position = newpos;
        Vector3 euler = ccTransform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);
        reflectionCamera.Render();
        reflectionCamera.transform.position = ccLastpos;
        GL.invertCulling = false;
        Material[] materials = thisRenderer.sharedMaterials;
        foreach (Material mat in materials) if (mat.HasProperty("_MirrorReflectionTex")) mat.SetTexture("_MirrorReflectionTex", reflectionTexture);
        insideRendering = false;

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void CreateMirrorObjects(Camera currentCamera, out Camera reflectionCamera)
    {
        //--------------

        reflectionCamera = null;

        if (!reflectionTexture || oldReflectionTextureSize != textureSize)
        {
            if (reflectionTexture) DestroyImmediate(reflectionTexture);
            reflectionTexture = new RenderTexture(textureSize, textureSize, 16) { name = "__MirrorReflection" + GetInstanceID(), isPowerOfTwo = true, hideFlags = HideFlags.DontSave };
            oldReflectionTextureSize = textureSize;
        }

        reflectionCamera = reflectionCameras[currentCamera] as Camera;
        GameObject go = new GameObject("Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
        reflectionCamera = go.GetComponent<Camera>();
        reflectionCamera.enabled = false;
        reflectionCamera.transform.SetPositionAndRotation(thisTransform.position, thisTransform.rotation);
        reflectionCamera.gameObject.AddComponent<FlareLayer>();
        go.hideFlags = HideFlags.HideAndDontSave;
        reflectionCameras[currentCamera] = reflectionCamera;

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void UpdateCamera(Camera src, Camera dest)
    {
        //--------------

        if (dest == null) return;

        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;

        //--------------

        if (src.clearFlags == CameraClearFlags.Skybox)
        {
            Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
            Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
            if (!sky || !sky.material)
            {
                mysky.enabled = false;
            }
            else
            {
                mysky.enabled = true;
                mysky.material = sky.material;
            }
        }

        //--------------

        dest.farClipPlane = clipPlaneOffset.z;
        dest.nearClipPlane = clipPlaneOffset.y;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
        dest.renderingPath = src.renderingPath;

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////    


    static Vector4 ClipPlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign, float clipPlaneOffset)
    {
        //--------------

        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
    {
        //--------------

        Vector4 q = projection.inverse * new Vector4(Sgn(clipPlane.x), Sgn(clipPlane.y), 1.0f, 1.0f);
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    static float Sgn(float a)
    {
        //--------------

        float sign = 0.0f;
        if (a > 0.0f) sign = 1.0f;
        if (a < 0.0f) sign = -1.0f;
        return sign;

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        //--------------

        float p0 = 2 * plane[0];
        float p1 = 2 * plane[1];
        float p2 = 2 * plane[2];
        float p3 = 2 * plane[3];
        reflectionMat.m00 = 1 - p0 * plane[0];
        reflectionMat.m01 = -p0 * plane[1];
        reflectionMat.m02 = -p0 * plane[2];
        reflectionMat.m03 = -p3 * plane[0];
        reflectionMat.m10 = -p1 * plane[0];
        reflectionMat.m11 = 1 - p1 * plane[1];
        reflectionMat.m12 = -p1 * plane[2];
        reflectionMat.m13 = -p3 * plane[1];
        reflectionMat.m20 = -p2 * plane[0];
        reflectionMat.m21 = -p2 * plane[1];
        reflectionMat.m22 = 1 - p2 * plane[2];
        reflectionMat.m23 = -p3 * plane[2];
        reflectionMat.m30 = reflectionMat.m31 = reflectionMat.m32 = 0;
        reflectionMat.m33 = 1;

        //--------------
    }


    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
