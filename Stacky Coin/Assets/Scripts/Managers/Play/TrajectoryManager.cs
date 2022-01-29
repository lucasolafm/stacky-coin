using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class TrajectoryManager : MonoBehaviour
{
	[SerializeField] private HandManager handManager;

	[SerializeField] private Transform trajectoryPointPrefab;
	[SerializeField] private Transform trajectoryHolder;

	public float fadeInTime;
	public float fadeOutTime;
	public float lifeTime;


    private List<List<Vector3>> trajPointPositions = new List<List<Vector3>>();
	private List<List<Vector3>> trajPointPositionsHeavy = new List<List<Vector3>>();
	private List<List<float>> trajPointTransparency = new List<List<float>>();
	private List<List<float>> trajPointTransparencyHeavy = new List<List<float>>();
	private List<List<float>> trajPointScale = new List<List<float>>();
	private List<List<float>> trajPointScaleHeavy = new List<List<float>>();

	private List<Transform> trajPointsMoving = new List<Transform>();
	private List<Renderer> trajPointsMovingRenderers = new List<Renderer>();

    private bool shouldMoveSpheres;
    private Coroutine moveSpheresRoutine;
    private Vector3 originalFlipPos = new Vector3(2.22f, -0.02489293f, 2.356181f);
	private int trajPointsMovingCount, positionsCount;
	private float fade;
	private int index;
	private float startTime;
	private float scale;
	private float transparency;
	private TextAsset data;
	private StreamReader reader;

    void Start()
    {
		EventManager.HandCharges.AddListener(Enable);
		EventManager.HandStopsCharge.AddListener(Disable);

        GetTrajectoryData();
        SpawnSpheres();

		trajPointsMovingCount = trajPointsMoving.Count;
    }

    private void Enable(Coin coin)
    {
        shouldMoveSpheres = true;

        if (moveSpheresRoutine != null) StopCoroutine(moveSpheresRoutine);
        moveSpheresRoutine = StartCoroutine(MoveSpheres(coin));
    }

	public void Disable()
    {
        shouldMoveSpheres = false;
    }

    private IEnumerator MoveSpheres(Coin coin)
	{
        startTime = Time.time;

		//Deactivate spheres
		for (int i = 0; i < trajPointsMovingCount; i++)
		{
			trajPointsMoving[i].position = Vector3.zero;
		}

		positionsCount = (coin.type == CoinType.Coin ? trajPointPositions : trajPointPositionsHeavy).Count;

		fade = 0;
		while (shouldMoveSpheres || fade < fadeInTime)  //Loop while charging or while fade has not completed
		{
			//Point lists are based on time steps of 0.016
			//Get the amount of steps
			index = Mathf.Min(Mathf.RoundToInt((Time.time - startTime) / 0.016f), positionsCount - 2);

			//Move spheres & set transparency
			for (int i = 0, count = (coin.type == CoinType.Coin ? trajPointPositions : trajPointPositionsHeavy)[index].Count; i < count; i++)
			{
				//Position and scale
				if (shouldMoveSpheres)
				{
					trajPointsMoving[i].position = (coin.type == CoinType.Coin ? trajPointPositions : trajPointPositionsHeavy)[index][i] + handManager.hand.position + new Vector3(0, 1, 0) - originalFlipPos;
					scale = (coin.type == CoinType.Coin ? trajPointScale : trajPointScaleHeavy)[index][i];
					trajPointsMoving[i].localScale = new Vector3(scale, scale, scale);
				}

				//Fade in transparency
				if (fade < fadeInTime)
				{
					fade = Mathf.Min(fade + Time.deltaTime, fadeInTime);
					transparency = (coin.type == CoinType.Coin ? trajPointTransparency : trajPointTransparencyHeavy)[index][i] * (fade / fadeInTime);
				}
				else
				{
					transparency = (coin.type == CoinType.Coin ? trajPointTransparency : trajPointTransparencyHeavy)[index][i];
				}
				trajPointsMovingRenderers[i].material.color = new Color(1, 1, 1, transparency);
			}

			yield return null;;
		}

		yield return new WaitForSeconds(lifeTime);

		fade = 0;
		while (fade < fadeOutTime)
		{
			fade = Mathf.Min(fade + Time.deltaTime, fadeOutTime);
			for (int i = 0, count = (coin.type == CoinType.Coin ? trajPointTransparency : trajPointTransparencyHeavy)[index].Count; i < count; i++)
			{
				trajPointsMovingRenderers[i].material.color = new Color(1, 1, 1, 
					(coin.type == CoinType.Coin ? trajPointTransparency : trajPointTransparencyHeavy)[index][i] * (1 - fade / fadeOutTime));
			}

			yield return null;
		}
		
		/*
		//Deactivate spheres
		for (int i = 0; i < trajPointsMovingCount; i++)
		{
			trajPointsMoving[i].position = Vector3.zero;
		}
		*/
	}

    private void GetTrajectoryData()
    {
        trajPointPositions.Add(new List<Vector3>());
		trajPointTransparency.Add(new List<float>());
		trajPointScale.Add(new List<float>());
		trajPointPositionsHeavy.Add(new List<Vector3>());
		trajPointTransparencyHeavy.Add(new List<float>());
		trajPointScaleHeavy.Add(new List<float>());

		CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
		ci.NumberFormat.CurrencyDecimalSeparator = ".";

		string line;
		string[] values;

		for (int i = 0; i < 2; i++)
		{
			int positionsCount = (i == 0 ? trajPointPositions : trajPointPositionsHeavy).Count;

			data = Resources.Load("Data/" + (i == 0 ? "TrajectorySpheres" : "TrajectorySpheresHeavy")) as TextAsset;
			reader = new StreamReader(new MemoryStream(data.bytes));

			int count = 0;
			while (!reader.EndOfStream) //Continue until end of file
			{
				count++;
				line = reader.ReadLine(); //Read the next line

				if (line != "")  //Check if not empty line (no sphere)
				{
					values = line.Split(',');

					//Position
					(i == 0 ? trajPointPositions : trajPointPositionsHeavy)[(i == 0 ? trajPointPositions : trajPointPositionsHeavy).Count - 1].
						Add(new Vector3(float.Parse(values[0], NumberStyles.Any, ci), float.Parse(values[1], NumberStyles.Any, ci), 2.356181f));

					//Transparency
					(i == 0 ? trajPointTransparency : trajPointTransparencyHeavy)[(i == 0 ? trajPointTransparency : trajPointTransparencyHeavy).Count - 1].
						Add(float.Parse(values[2], NumberStyles.Any, ci));

					//Scale
					(i == 0 ? trajPointScale : trajPointScaleHeavy)[(i == 0 ? trajPointScale : trajPointScaleHeavy).Count - 1].
						Add(float.Parse(values[3], NumberStyles.Any, ci));
				}

				//When end of current trajectory data is reached (10th line)
				//Save lists containing trajectory data in general data list
				if (count == 10)
				{
					(i == 0 ? trajPointPositions : trajPointPositionsHeavy).Add(new List<Vector3>());
					(i == 0 ? trajPointTransparency : trajPointTransparencyHeavy).Add(new List<float>());
					(i == 0 ? trajPointScale : trajPointScaleHeavy).Add(new List<float>());

					count = 0;
				}
			}
		}
    }

    private void SpawnSpheres()
    {
        for (int i = 0; i < 10; i++)
		{
			Transform point = Instantiate(trajectoryPointPrefab, trajectoryHolder);
			trajPointsMoving.Add(point);
			trajPointsMovingRenderers.Add(point.GetComponent<Renderer>());
		}
    }
}
