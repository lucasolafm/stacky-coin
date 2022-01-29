using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinTubeManager : MonoBehaviour
{
    public HomeManager homeManager;
    [SerializeField] private MiniCoinManager miniCoinManager;

    [SerializeField] private Transform coinTube;
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
    [HideInInspector] public int firstCoinOnScreenIndex;
    [SerializeField] private float counterPointerOffset;
    [SerializeField] private int coinsOnScreenStart;

    private CoinTubeState state;
    private float offsetTopScreen;
    private float cameraMoveAmount;
    [HideInInspector] public Vector3 coinTubeStartPosition;
    [HideInInspector] public float cameraPositionMin;
    private float shiftProgress, shiftAmount;
    private int pointerCount;

    void Start()
    {
        EventManager.SpawningNewMiniCoins.AddListener(OnSpawningNewMiniCoins);
        EventManager.LastCoinOnScreenPaid.AddListener(OnLastCoinOnScreenPaid);
        EventManager.CoinTubeCameraRepositioned.AddListener(OnCoinTubeCameraRepositioned);
        EventManager.MiniCoinAddedToTube.AddListener(OnMiniCoinAddedToTube);    
        EventManager.MiniCoinRemovedFromTube.AddListener(OnMiniCoinRemovedFromTube);
        EventManager.EnteringCollection.AddListener(OnEnteringCollection);
        EventManager.EnteringHome.AddListener(OnEnteringHome);

        coinTubeStartPosition = coinTube.position;
        cameraPositionMin = cameraTransform.position.y;
    }

    void Update()
    {
        state.Update();
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
        firstCoinOnScreenIndex = GetFirstCoinOnScreenIndex(bottomRightOfScreen.y);
    }

    private void OnMiniCoinAddedToTube()
    {
        UpdateCounterPointer(1);
    }

    private void OnMiniCoinRemovedFromTube(MiniCoin miniCoin, int paidCount, float percentageOfPaidThisFrame)
    {
        UpdateCounterPointer(-1);

        //MoveCameraDown();
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

        cameraMoveAmount = (topOfScreen.y - bottomRightOfScreen.y) * cameraMoveScreenPercent;

        // Get half the lenght of the counter pointer
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

        firstCoinOnScreenIndex = GetFirstCoinOnScreenIndex(bottomRightOfScreen.y);

        InitializeCounterPointer();
    }

    private void MoveCameraDown()
    {
        cameraTransform.position -= new Vector3(0, miniCoinManager.inTubeSpacing, 0);  

        EventManager.CoinTubeCameraRepositioned.Invoke();      
    }

    private void InitializeCounterPointer()
    {
        // Move the counter pointer up above the top most mini coin
        counterPointer.position = new Vector3(bottomRightOfScreen.x - homeManager.offSetSideCoinTube, 
                                                bottomOfCoinTube.y + homeManager.offSetBottomCoinTube + 
                                                (homeManager.startOriginalMiniCoins.Length) * miniCoinManager.inTubeSpacing + counterPointerOffset, 0);
                                                
        pointerCount = homeManager.startOriginalMiniCoins.Length;

        counterPointerText.text = pointerCount.ToString();
    }

    private void UpdateCounterPointer(int shift)
    {
        counterPointer.position += new Vector3(0, miniCoinManager.inTubeSpacing * shift, 0);

        pointerCount += shift;
        counterPointerText.text = pointerCount.ToString();

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

            coinTube.position = 
                coinTubeStartPosition + coinTube.TransformDirection(Vector3.right * 
                (homeManager.screenWorldWidth * (inOrOut == true ? 1 - shiftAmount : shiftAmount)));

            yield return null;
        }
    }

    public int GetFirstCoinOnScreenIndex(float bottomOfScreen)
    {
        return Mathf.FloorToInt(Mathf.Max(bottomOfScreen - (bottomOfCoinTube.y + homeManager.offSetBottomCoinTube), 0) / miniCoinManager.inTubeSpacing);
    }

    public void GetCameraWorldBoundaries()
    {
        topOfScreen = camera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
        bottomRightOfScreen = camera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
    }
}
