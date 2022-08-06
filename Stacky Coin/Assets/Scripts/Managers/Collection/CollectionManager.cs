using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    public GameObject collectionHolder;
    public GameObject collectionLights;
    [SerializeField] private Transform title;
    [SerializeField] private Transform skinHolder;
    [SerializeField] private new Camera camera;
    [SerializeField] private Button backButton;
    [SerializeField] private Transform unseenSkinBubblePrefab;
    [SerializeField] private float skinSpinTime;
    [SerializeField] private float skinSpinTimeInBetween;
    [SerializeField] private float skinSpinDelay;

    private CollectionState state;
    [HideInInspector] public Skin[] skins;
    private Vector3[] skinsStartPositions;
    private Vector3 titleStartPosition;
    private int[] tempIntArray;
    private Transform unseenSkinBubble;
    private float shiftProgress, shiftAmount;
    private float screenWorldWidth;
    private WaitForSeconds skinSpinDelayWait;
    private Coroutine spinningSkinsRoutine;
    private Vector3 skinStartRotationCoin, skinStartRotationGem;
    private List<Transform> unseenSkinBubbles = new List<Transform>();
    private Skin skin;

    void Start()
    {
        EventManager.EntersCollection.AddListener(OnEntersCollection);
        EventManager.EnteringCollection.AddListener(OnEnteringCollection);
        EventManager.EnteredCollection.AddListener(OnEnteredCollection);
        EventManager.EnteringHome.AddListener(OnEnteringHome);
        EventManager.UnlockedNewCoinSkin.AddListener(OnUnlockedNewCoinSkin);
        backButton.onClick.AddListener(PressBackButton);

        skinSpinDelayWait = new WaitForSeconds(skinSpinDelay);

        screenWorldWidth = (camera.ScreenToWorldPoint(Vector3.zero) - camera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0))).magnitude;

        skins = skinHolder.GetComponentsInChildren<Skin>();

        skinStartRotationCoin = skins[0].visuals.localRotation.eulerAngles;
        skinStartRotationGem = skins[GameManager.I.coinSkinAmount].visuals.localRotation.eulerAngles;

        skinsStartPositions = new Vector3[skins.Length];
        for (int i = 0; i < skins.Length; i++)
        {
            skinsStartPositions[i] = skins[i].transform.position - skins[i].visuals.TransformDirection(Vector3.back * screenWorldWidth);
        }
        titleStartPosition = title.position - skins[0].visuals.TransformDirection(Vector3.back * screenWorldWidth);

        InstantiateSkinVisuals();

        foreach (Skin skin in skins)
        {
            skin.transform.position -= skin.visuals.TransformDirection(Vector3.back * screenWorldWidth);
        }

        if (!GameManager.I.testEnableAllSkins)
        {
            EnableOwnedSkins();
        }
        else
        {
            EnableAllSkins();
        }

        DisplayUnseenSkinBubbles();

        SetState(new CollectionDisabled(this));
    }

    private void OnEntersCollection()
    {
        collectionHolder.SetActive(true);
        collectionLights.SetActive(true);
    }

    private void OnEnteringCollection()
    {
        StartCoroutine(ShiftSkinsSideways(true));

        ResetSkinRotations();

        spinningSkinsRoutine = StartCoroutine(SpinningSkins());

        ToggleUnseenSkinBubbles(true);

        Data.RemoveAllUnseenNewSkins();
    }

    private void OnEnteredCollection()
    {
        SetState(new CollectionDefault(this));
    }

    private void OnEnteringHome()
    {
        StartCoroutine(ShiftSkinsSideways(false));

        ToggleUnseenSkinBubbles(false);

        unseenSkinBubbles.Clear();
    }

    private void OnUnlockedNewCoinSkin(int id)
    {
        EnableSkin(id);

        InstantiateUnseenSkinBubble(id);
    }

    private void PressBackButton()
    {
        SetState(new CollectionEnteringHome(this));
    }

    public void SetState(CollectionState state)
    {
        if (this.state != null) state.Exit();
        this.state = state;
        this.state.Enter();
    }

    private IEnumerator SpinningSkins()
    {
        float timeElapsed;
        float t;

        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            timeElapsed = 0;
            while (timeElapsed < (skins.Length - 1) * skinSpinTimeInBetween + skinSpinTime)
            {
                yield return null;

                timeElapsed += Time.deltaTime;

                for (int i = 0; i < skins.Length; i++)
                {
                    if (!skins[i].unlocked) continue;

                    t = Mathf.Clamp((timeElapsed - skinSpinTimeInBetween * i) / skinSpinTime, 0, 1);

                    skins[i].visuals.localEulerAngles = (i < GameManager.I.coinSkinAmount ? skinStartRotationCoin : skinStartRotationGem) + 
                                                        new Vector3(0, 0, -(Mathf.Cos(Mathf.PI * t) - 1) / 2 * 360);
                }
            }

            yield return skinSpinDelayWait;
        }
    }

    private void ResetSkinRotations()
    {
        if (spinningSkinsRoutine != null) StopCoroutine(spinningSkinsRoutine);

        for (int i = 0; i < skins.Length; i++)
        {
            skins[i].visuals.localEulerAngles = i < GameManager.I.coinSkinAmount ? skinStartRotationCoin : skinStartRotationGem;
        }
    }

    private void InstantiateSkinVisuals()
    {
        Coin coin;

        for (int i = 0, count = skins.Length; i < count; i++)
        {
            coin = Instantiate(i < GameManager.I.coinSkinAmount ? GameManager.I.coinPrefabs[i] : GameManager.I.gemPrefabs[i - GameManager.I.coinSkinAmount], skins[i].visuals);
            coin.transform.localPosition = Vector3.zero;
            coin.transform.localScale = new Vector3(1, 1, 1);
            coin.rb.isKinematic = true;
            coin.gameObject.layer = 23;
            coin.gameObject.isStatic = true;
            coin.gameObject.SetActive(false);
            skins[i].number = coin.number;
            Destroy(coin.collider);
            Destroy(coin.transform.GetChild(0).gameObject);
            Destroy(coin);

            skins[i].shadedVisual = coin.transform;
        }
    }

    private void EnableOwnedSkins()
    {
        tempIntArray = Data.ownedCoins;
        
        for (int i = 0; i < tempIntArray.Length; i++)
        {
            skin = GetCoinSkin(tempIntArray[i]);

            skin.shadedVisual.gameObject.SetActive(true);

            skin.unlocked = true;
        }

        tempIntArray = Data.ownedGems;

        for (int i = 0; i < tempIntArray.Length; i++)
        {
            skin = GetCoinSkin(tempIntArray[i]);

            skin.shadedVisual.gameObject.SetActive(true);

            skin.unlocked = true;
        }
    }

    private void EnableAllSkins()
    {
        foreach (Skin skin in skins)
        {
            skin.shadedVisual.gameObject.SetActive(true);
        }
    }

    private void EnableSkin(int id)
    {
        skin = GetCoinSkin(id);

        skin.shadedVisual.gameObject.SetActive(true);

        skin.unlocked = true;
    }

    private IEnumerator ShiftSkinsSideways(bool inOrOut)
    {
        shiftProgress = 0;
        Vector3 totalMove = skins[0].visuals.TransformDirection(Vector3.back * screenWorldWidth);
        
        while (shiftProgress < 1)
        {
            shiftProgress = Mathf.Min(shiftProgress + Time.deltaTime / GameManager.I.collectionHomeTransitionTime, 1);
            shiftAmount = shiftProgress < 0.5 ? 2 * shiftProgress * shiftProgress : 1 - Mathf.Pow(-2 * shiftProgress + 2, 2) / 2;

            for (int i = 0; i < skins.Length; i++)
            {
                skins[i].transform.position = skinsStartPositions[i] + totalMove * (inOrOut ? shiftAmount : 1 - shiftAmount)/*))*/;
            }

            title.position = titleStartPosition + totalMove * (inOrOut ? shiftAmount : 1 - shiftAmount);
            
            yield return null;
        }
    }

    private void DisplayUnseenSkinBubbles()
    {
        tempIntArray = Data.unseenNewSkins;
        
        for (int i = 0; i < tempIntArray.Length; i++)
        {
            InstantiateUnseenSkinBubble(tempIntArray[i]);
        }
    }

    private void InstantiateUnseenSkinBubble(int skinId)
    {
        unseenSkinBubble = Instantiate(unseenSkinBubblePrefab, (GetCoinSkin(skinId)).transform);
        unseenSkinBubble.localPosition = new Vector3(0.1109f, -0.0157999992f, -0.0759000033f);
        unseenSkinBubble.localEulerAngles = new Vector3(34.2640877f, 315.462067f, 331.01355f);
        unseenSkinBubble.localScale = new Vector3(0.047862187f, 0.0478621572f, 0.047862161f);
        unseenSkinBubbles.Add(unseenSkinBubble);
    }

    private void ToggleUnseenSkinBubbles(bool onOrOff)
    {
        foreach (Transform bubble in unseenSkinBubbles)
        {
            bubble.gameObject.SetActive(onOrOff ? true : false);
        }
    }

    public Skin GetCoinSkin(int id)
    {
        foreach (Skin skin in skins)
        {
            if (skin.number != id) continue;

            return skin;
        }

        return null;
    }
}
