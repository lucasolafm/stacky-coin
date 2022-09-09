using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HomeSprites : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private Camera cameraCoinTube;
    [SerializeField] private SpriteRenderer coinTubeSprite;
    [SerializeField] private SpriteRenderer[] chestSlots;
    
    private Transform[] sprites;
    private Vector3[] startPositions;

    void Awake()
    {
        sprites = transform.GetComponentsInChildren<Transform>().Where(t => t.parent == transform).ToArray();
        startPositions = sprites.Select(t => t.position).ToArray();

        EventManager.EnteringCollection.AddListener(OnEnteringCollection);
        EventManager.EnteringHome.AddListener(OnEnteringHome);
    }

    public void Initialize()
    {
        float coinTubeWidth = coinTubeSprite.bounds.size.x;
        float slotWidth = chestSlots[0].bounds.size.x;
        float spacing = (1.182324f - coinTubeWidth - slotWidth * chestSlots.Length) / (chestSlots.Length + 1);
        Vector3 edgeOfScreen = camera.ScreenToWorldPoint(new Vector3(0, 0, 10));

        for (int i = 0; i < chestSlots.Length; i++)
        {
            chestSlots[i].transform.position = edgeOfScreen + chestSlots[i].transform.right *
                (slotWidth * (i + 0.5f) +
                spacing * (i + 1)) +
                chestSlots[i].transform.up * (spacing + chestSlots[i].bounds.size.z / 2);
        }
    }

    private void OnEnteringCollection()
    {
        StartCoroutine(ShiftSideways(false));
    }

    private void OnEnteringHome()
    {
        StartCoroutine(ShiftSideways(true));
    }
    
    private IEnumerator ShiftSideways(bool inOrOut)
    {
        float t = 0;
        float shiftAmount;
        Vector3 totalMove = sprites[0].TransformDirection(Vector3.right * 1.182324f);
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / GameManager.I.collectionHomeTransitionTime, 1);
            shiftAmount = t < 0.5 ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

            for (var i = 0; i < sprites.Length; i++)
            {
                sprites[i].position = startPositions[i] + totalMove * (inOrOut ? 1 - shiftAmount : shiftAmount);
            }

            yield return null;
        }
    }

}
