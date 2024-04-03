using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Camera pictureCamera;
    [SerializeField] private float zoomSensitivity;
    [SerializeField, Range(5, 20)] private float minimumFieldOfView = 5f;
    [SerializeField, Range(40, 80)] private float maximumFieldOfView = 60f;

    [Tooltip("Event that carries the edited sensitivity based on the original zoom and current zoom")]
    [SerializeField] private UnityEvent<float> onZoomLevelChanged;
    [SerializeField] private Slider camZoomSlider;

    [SerializeField] private PagePicture picturePrefab;

    [SerializeField] private PictureStorage storage;
    [SerializeField] private float defaultMaxScanDistance = 20f;
    [SerializeField] private LayerMask ignoredPhotoLayers;
    [SerializeField] private TMPro.TMP_Text exceptionText;

    [SerializeField, Range(1, 100)] private int photoAccuracy = 50;

    [Header("Effects")]
    [SerializeField] private AudioClip shutterSound;
    [SerializeField] private AudioClip storageFullSound;
    [field: SerializeField] private AudioClip zoomInSound, zoomOutSound;
    [SerializeField] private Animator shutterTop, shutterBottom;

    [SerializeField] private SoundPlayer soundPlayer;

    private float originalZoom;

    private float effectiveScanDistance;

    private PlayerInput input;

    private bool snapping;

    private string path;

    private void OnValidate()
    {
        if (pictureCamera == null)
        {
            pictureCamera = Camera.main;
        }
    }

    private void Awake()
    {
        if (soundPlayer == null)
            soundPlayer = GetComponent<SoundPlayer>();
        if (soundPlayer == null)
            soundPlayer = GetComponentInParent<SoundPlayer>();


        originalZoom = pictureCamera.fieldOfView;

        camZoomSlider.minValue = minimumFieldOfView;
        camZoomSlider.maxValue = maximumFieldOfView;
        camZoomSlider.value = originalZoom;

        effectiveScanDistance = maximumFieldOfView / pictureCamera.fieldOfView * defaultMaxScanDistance;
        
        // TODO: don't make this happed every time a quest is completed?;
        StaticQuestHandler.OnShrineCompleted += () => storage.AddStorageCapacity();

        input = GetComponent<PlayerInput>();
        if (Application.isEditor)
        {
            path = Application.dataPath;
            return;
        }
        path = Application.dataPath;
    }

    public void CameraClose()
    {
        pictureCamera.fieldOfView = originalZoom;
        camZoomSlider.value = originalZoom;
        onZoomLevelChanged?.Invoke(1);
    }

    public void ZoomCamera(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            if (soundPlayer != null && !soundPlayer.AlreadyPlaying())
            {
                soundPlayer.PlaySound(callbackContext.ReadValue<Vector2>().y > 0 ? zoomInSound:zoomOutSound, true); 
            }

            pictureCamera.fieldOfView -= callbackContext.ReadValue<Vector2>().y * zoomSensitivity;
            pictureCamera.fieldOfView = Mathf.Clamp(pictureCamera.fieldOfView, minimumFieldOfView, maximumFieldOfView);
            camZoomSlider.value = pictureCamera.fieldOfView;

            effectiveScanDistance = maximumFieldOfView / pictureCamera.fieldOfView * defaultMaxScanDistance;
            onZoomLevelChanged?.Invoke(pictureCamera.fieldOfView / originalZoom);
        }
    }

    public void SnapPicture(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            if (!storage.StorageIsFull() && !snapping)
            {
                //shutterTop.SetTrigger("Snap");
                //shutterBottom.SetTrigger("Snap");
                StartCoroutine(Snap());
            } else if (storage.StorageIsFull() && !snapping && soundPlayer != null)
            {
                soundPlayer.PlaySound(storageFullSound, true);
            }
        }
    }

    public void DeleteCameraRoll() => storage.DeleteStorage();
    
    private IEnumerator Snap()
    {
        if (soundPlayer != null)
        {
            soundPlayer.PlaySound(shutterSound, true);
        }

        if (pictureCamera != Camera.main)
            pictureCamera.gameObject.SetActive(true);
        snapping = true;

        yield return new WaitForEndOfFrame();

        RenderTexture screenTexture = new RenderTexture(Screen.height, Screen.height, 16);
        pictureCamera.targetTexture = screenTexture;
        RenderTexture.active = screenTexture;
        pictureCamera.Render();

        PictureInfo pictureInfo = new PictureInfo(AnalyzeSubjects(), transform.position);

        Texture2D renderedTexture = new Texture2D(Screen.height, Screen.height);
        renderedTexture.ReadPixels(new Rect(0, 0, Screen.height, Screen.height), 0, 0);
        RenderTexture.active = null;

        try
        {
            byte[] byteArray = renderedTexture.EncodeToPNG();

            // Android doesn't use resources folder
#if PLATFORM_ANDROID
            string savingPath = Path.Combine(Application.persistentDataPath, System.DateTime.UtcNow.Day + System.DateTime.UtcNow.Minute + System.DateTime.UtcNow.Second + ".png");
#else
            string savingPath = path + "/Resources/snap" + System.DateTime.UtcNow.Day + System.DateTime.UtcNow.Minute + System.DateTime.UtcNow.Second + ".png";
#endif

            File.WriteAllBytes(savingPath, byteArray);
            pictureInfo.PicturePath = savingPath;

            // TODO: After implementing saving, make sure to not remove the pictures that are still used by the player.
            Application.quitting += () => RemovePicturesFromSystem(savingPath);

            Texture2D png = LoadTexture(savingPath);
            Sprite spr = Sprite.Create(png, new Rect(0, 0, png.width, png.height), Vector2.one * 0.5f);

            PagePicture newPagePicture = Instantiate(picturePrefab);
            newPagePicture.SetPicture(spr);
            newPagePicture.LinkPictureInformation(pictureInfo);

            EvaluateProgress.EvaluatePictureProgress(pictureInfo);

            storage.CreatePictureFromCamera(newPagePicture);
        }
        catch (System.Exception exception)
        {
            if (exceptionText != null)
            {
                exceptionText.text = exception.ToString();
            }
        }

        if (pictureCamera != Camera.main)
            pictureCamera.gameObject.SetActive(false);
        else
            pictureCamera.targetTexture = null;

        snapping = false;

    }
    private void OnDrawGizmos()
    {
        float camStep = pictureCamera.pixelHeight / photoAccuracy;
        float xStart = (pictureCamera.pixelWidth - pictureCamera.pixelHeight) * 0.5f;

        for (int x = 0; x <= photoAccuracy; x++)
        {
            for (int y = 0; y <= photoAccuracy; y++)
            {
                Ray ray = pictureCamera.ScreenPointToRay(new Vector3(xStart + x * camStep, y * camStep));
                Gizmos.DrawRay(ray.origin, ray.direction * effectiveScanDistance);
            }
        }
    }

    private List<QuestableObject> AnalyzeSubjects()
    {
        List<QuestableObject> result = new();

        float camStep = pictureCamera.pixelHeight / photoAccuracy;
        float xStart = (pictureCamera.pixelWidth - pictureCamera.pixelHeight) * 0.5f;

        for (int x = 0; x <= photoAccuracy; x++)
        {
            for (int y = 0; y <= photoAccuracy; y++)
            {
                Ray ray = pictureCamera.ScreenPointToRay(new Vector3(xStart + x * camStep, y * camStep));
                if(Physics.Raycast(ray, out RaycastHit hit, effectiveScanDistance, ~ignoredPhotoLayers))
                {
                    QuestableObject[] questables = hit.transform.GetComponents<QuestableObject>();
                    if (questables.Length >0)
                    {
                        foreach (QuestableObject questableObject in questables)
                        {
                            if (!result.Contains(questableObject))
                            {
                                EvaluateProgress.UpdateTrackedProgress(questableObject.QuestObjectID);
                                result.Add(questableObject);
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

    private void RemovePicturesFromSystem(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            if(File.Exists(path + ".meta"))
            {
                File.Delete(path + ".meta");
            }
        }
    }

    private Texture2D LoadTexture(string FilePath)
    {
        Texture2D Tex2D;
        byte[] FileData;
        /*
#if PLATFORM_ANDROID
        FileData = Resources.Load<TextAsset>(FilePath).bytes;

        Tex2D = new Texture2D(2, 2);
        if (Tex2D.LoadImage(FileData))
        {
            return Tex2D;
        }
#else
        */
        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);
            if (Tex2D.LoadImage(FileData))
            {
                return Tex2D;                
            }          
        }
//#endif
        return null;                    
    }
}
