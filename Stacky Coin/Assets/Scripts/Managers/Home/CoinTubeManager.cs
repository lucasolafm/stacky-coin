using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinTubeManager : MonoBehaviour
{
    public HomeManager homeManager;
    [SerializeField] private MiniCoinManager miniCoinManager;

    [SerializeField] private Transform coinTube;
    public SpriteRenderer coinTubeVisual;
    [SerializeField] private SpriteRenderer coinTubeFloorFront;
    [SerializeField] private Transform coinTubeFloorBack;
    public new Camera camera;
    public Transform cameraTransform;
    [SerializeField] private Transform counterPointer;
    [SerializeField] private TextMeshPro counterPointerText;
    [SerializeField] private MeshFilter counterPointerMeshFilter;

    public float cameraMoveScreenPercent, cameraAscendTime, cameraDescendTime;
    public float cameraDescendRandomness;
    public float cameraDescendIncrease;
    public float cameraMoveToCenterTime;
    [HideInInspector] public Vector3 topOfScreen, bottomRightOfScreen, bottomOfCoinTube;
    [SerializeField] private float counterPointerOffset;
    [SerializeField] private int coinsOnScreenStart;

    private int valueInTube;
    private CoinTubeState state;
    private float offsetTopScreen;
    [HideInInspector] public Vector3 coinTubeStartPosition;
    [HideInInspector] public float cameraPositionMin;
    private float shiftProgress, shiftAmount;
    private float visualMinHeight;
    private float visualOffsetCamera;
    private Transform visualTransform;
    private bool visualAttachedCamera;
    private float topOfFloor;

    void Start()
    {
        EventManager.SpawningNewMiniCoins.AddListener(OnSpawningNewMiniCoins);
        EventManager.LastCoinOnScreenPaid.AddListener(OnLastCoinOnScreenPaid);
        EventManager.CoinTubeCameraRepositioned.AddListener(OnCoinTubeCameraRepositioned);
        EventManager.MiniCoinAddedToTube.AddListener(OnMiniCoinAddedToTube);    
        EventManager.MiniCoinRemovedFromTube.AddListener(OnMiniCoinRemovedFromTube);
        EventManager.EnteringCollection.AddListener(OnEnteringCollection);
        EventManager.EnteringHome.AddListener(OnEnteringHome);

        coinTubeStartPosition = coinTube.transform.position;
        cameraPositionMin = cameraTransform.position.y;
    }

    void Update()
    {
        state.Update();

        visualTransform.position = new Vector3(visualTransform.position.x,
            Mathf.Max(camera.transform.position.y, visualMinHeight), visualTransform.position.z);
    }

    private void OnSpawningNewMiniCoins()
    {
        GetCameraWorldBoundaries();
    }

    private void OnLastCoinOnScreenPaid()
    {
        SetState(new CoinTubeDescending(this));
    }

    private void OnCoinTubeCameraRepositioned()
    {
        GetCameraWorldBoundaries();
    }

    private void OnMiniCoinAddedToTube(CoinType type)
    {
        valueInTube += type == CoinType.Gem ? GameManager.I.gemBonusAmount : 1;
        UpdateCounterPointer(1);
    }

    private void OnMiniCoinRemovedFromTube(MiniCoin miniCoin, int paidCount, float percentageOfPaidThisFrame)
    {
        valueInTube -= miniCoin.GetCoinType() == CoinType.Gem ? GameManager.I.gemBonusAmount : 1;
        UpdateCounterPointer(-1);
    }

    private void OnEnteringCollection()
    {
        StartCoroutine(ShiftSideways(false));
    }

    private void OnEnteringHome()
    {
        StartCoroutine(ShiftSideways(true));
    }

    public void SetState(CoinTubeState state)
    {
        if (this.state != null) this.state.Exit();
        this.state = state;
        this.state.Enter();
    }

    public void Initialize()
    {
        SetState(new CoinTubeDefault(this));

        valueInTube = GetInitialValueInTube();
        
        SetCoinTubeStartPosition();
        visualTransform = coinTubeVisual.transform;
        topOfFloor = coinTubeFloorFront.transform.position.y + coinTubeFloorFront.bounds.size.y / 2;

        // Get half the length of the counter pointer
        offsetTopScreen = (counterPointerMeshFilter.transform.TransformPoint(counterPointerMeshFilter.mesh.vertices[2]).y - 
                            counterPointerMeshFilter.transform.TransformPoint(counterPointerMeshFilter.mesh.vertices[0]).y) / 2;

        bottomOfCoinTube = bottomRightOfScreen;
        if (homeManager.startOriginalMiniCoins.Length > 0)
        {
            // Put the camera on the initial position
            cameraTransform.position = new Vector3(cameraTransform.position.x,
                                        cameraTransform.position.y + 
                                        Mathf.Max(((bottomRightOfScreen.y + homeManager.offSetBottomCoinTube + 
                                        (homeManager.startOriginalMiniCoins.Length - coinsOnScreenStart) * miniCoinManager.inTubeSpacing - bottomRightOfScreen.y)), 0),
                                        cameraTransform.position.z);

            // Get the new camera boundaries
            GetCameraWorldBoundaries();
        }

        InitializeCounterPointer();
    }
    
    private void SetCoinTubeStartPosition()
    {
        Vector3 tubeMin = coinTubeVisual.bounds.min;
        Vector3 tubeMax = coinTubeVisual.bounds.max;
 
        Vector3 screenMin = camera.WorldToScreenPoint(tubeMin);
        Vector3 screenMax = camera.WorldToScreenPoint(tubeMax);

        float worldPositionX = camera.ScreenToWorldPoint(new Vector2(Screen.width - (screenMax.x - screenMin.x) / 2, 0)).x;
        
        Vector3 floorMin = coinTubeFloorFront.bounds.min;
        Vector3 floorMax = coinTubeFloorFront.bounds.max;

        coinTubeFloorFront.transform.position = 
            new Vector3(worldPositionX, camera.ScreenToWorldPoint(Vector2.zero).y +
                (floorMax.y - floorMin.y) / 2 - 0.005f, coinTubeVisual.transform.position.z);

        coinTubeFloorBack.position = coinTubeFloorFront.transform.position + new Vector3(0, 0, 1);
        
        visualMinHeight = coinTubeFloorFront.transform.position.y + (floorMax.y - floorMin.y) / 2 + (tubeMax.y - tubeMin.y) / 2;

        coinTubeVisual.transform.position = new Vector3(worldPositionX, 
            Mathf.Max(camera.transform.position.y, visualMinHeight),
            coinTubeVisual.transform.position.z);
    }

    private void InitializeCounterPointer()
    {
        // Move the counter pointer up above the top most mini coin
        counterPointer.position = new Vector3(coinTubeVisual.transform.position.x, 
                                                bottomOfCoinTube.y + homeManager.offSetBottomCoinTube + 
                                                (homeManager.startOriginalMiniCoins.Length) * miniCoinManager.inTubeSpacing + counterPointerOffset, 0);

        counterPointerText.text = valueInTube.ToString();
    }

    private void UpdateCounterPointer(int shift)
    {
        counterPointer.position += new Vector3(0, miniCoinManager.inTubeSpacing * shift, 0);

        counterPointerText.text = valueInTube.ToString();

        // Move the camera up if the pointer is hitting the top of the screen
        if (counterPointer.position.y + offsetTopScreen > topOfScreen.y)
        {
            state.MiniCoinsReachedTopOfScreen();
        }
    }

    private IEnumerator ShiftSideways(bool inOrOut)
    {
        shiftProgress = 0;
        while (shiftProgress < 1)
        {
            shiftProgress = Mathf.Min(shiftProgress + Time.deltaTime / GameManager.I.collectionHomeTransitionTime, 1);
            shiftAmount = shiftProgress < 0.5 ? 2 * shiftProgress * shiftProgress : 1 - Mathf.Pow(-2 * shiftProgress + 2, 2) / 2;

            coinTube.transform.position = 
                coinTubeStartPosition + coinTube.transform.TransformDirection(Vector3.right * 
                                                                              (1.18228f * (inOrOut == true ? 1 - shiftAmount : shiftAmount)));

            yield return null;
        }
    }

    private int GetInitialValueInTube()
    {
        return homeManager.startOriginalMiniCoinsValue;
    }
    
    public void GetCameraWorldBoundaries()
    {
        topOfScreen = camera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
        bottomRightOfScreen = camera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
    }
}
