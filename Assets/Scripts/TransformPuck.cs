using UnityEngine;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using DG.Tweening;
using System.Collections;
using System.Linq;
//using static UnityEditor.Rendering.CameraUI;
//using static UnityEditor.Rendering.CameraUI;

public class TransformPuck : MonoBehaviour
{
    private DataDisk currentData;
    int count = 0;
    private bool deleteLastMarks = true;

    private List<Vector2> lastPositions = new List<Vector2>();
    private List<Vector2> currentPositions = new List<Vector2>();
    [SerializeField] private List<GameObject> discTempHolder = new List<GameObject>();

    [System.Serializable]
    public class DataDisk
    {
        public int tur;
        public List<List<int>> pos;
        public int? pto;
        public bool tim;

        public List<Vector2> GetPositions()
        {
            List<Vector2> vectorPositions = new List<Vector2>();
            if (pos != null)
            {
                foreach (var item in pos)
                {
                    if (item.Count == 2)
                        vectorPositions.Add(new Vector2(item[0], item[1]));
                }
            }
            return vectorPositions;
        }
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(DataManager.instance.data))
        {
            try
            {
                GameObject discDelayScore = null;
                if (DataManager.instance.data == "{\"clear\": true}")
                {
                    Debug.Log("Clear signal");
                    GameObject[] discs = GameObject.FindGameObjectsWithTag("Disc");
                    foreach (GameObject disc in discs)
                    {
                        Destroy(disc);
                    }

                    if (discTempHolder.Count > 0)
                    {
                        discTempHolder.Clear();
                    }

                    // Clear positions
                    currentPositions.Clear();

                    DataManager.instance.data = null;
                    return;
                }

                if (GameMechanics.Instance.canThrow == false)
                {
                    Debug.Log("Ignoring JSON because canThrow is false");
                    DataManager.instance.data = null;
                    return;
                }

                GameMechanics.Instance.display1Canvas.gameObject.SetActive(false);

                currentData = JsonConvert.DeserializeObject<DataDisk>(DataManager.instance.data);
                lastPositions = currentPositions;
                List<Vector2> positions = currentData.GetPositions();
                currentPositions = positions;

                // It results that the positions are not in any particular order, but changing ALL
                // the code so the cases work without any order in particular is a pain
                // So I'm going to sort the currentPositions by the recieving order
                // This is a workaround, but it works...
                // currentPositions = [newPos, oldPos] and so on. They can be in ANY order when recieved, so we need to have track of which ones are news and which ones are olds
                // We can't just sort the list because we need to know which ones are the new ones and which ones are the old ones
                // Also, we need to account for a little margin of error, so we can't just compare the lists
                // We need to compare the lists with a margin of error of 5 pixels

                // This code gets creazier everytime I visit it but somehow works just as intended

                float marginOfError = 5f;
                List<Vector2> newPositions = new List<Vector2>();

                foreach (Vector2 pos in currentPositions)
                {
                    bool isOld = false;

                    foreach (Vector2 lastPos in lastPositions)
                    {
                        if (Vector2.Distance(pos, lastPos) <= marginOfError)
                        {
                            isOld = true;
                            break;
                        }
                    }

                    if (!isOld)
                    {
                        newPositions.Add(pos);
                    }
                }

                // Move the old positions forward in the list and add the new ones at the beginning
                List<Vector2> sortedPositions = new List<Vector2>(newPositions);
                foreach (Vector2 lastPos in lastPositions)
                {
                    if (currentPositions.Any(p => Vector2.Distance(p, lastPos) <= marginOfError))
                    {
                        sortedPositions.Add(lastPos);
                    }
                }

                currentPositions = sortedPositions;
                // Log sorted position before assign
                string sortedPosStrings = "Sorted Positions: ";
                for (int i = 0; i < sortedPositions.Count; i++)
                {
                    sortedPosStrings += sortedPositions[i].ToString() + " ";
                }
                //Debug.Log(sortedPosStrings);

                // Check for equal positions in shots
                bool areEquals = AreListEqual(lastPositions, currentPositions);
                //Debug.Log("Are original equals: " + areEquals);
                // Check for equal positions in shots without last shot
                List<Vector2> newPositionsWithoutLastOne = new List<Vector2>(currentPositions);

                string newPosStrings = "New Positions Without LO: ";
                string currentPosStrings = "Current Positions: ";
                if (newPositionsWithoutLastOne.Count > 0)
                {
                    newPositionsWithoutLastOne.RemoveAt(0);
                }

                for (int i = 0; i < currentPositions.Count; i++)
                {
                    currentPosStrings += currentPositions[i].ToString() + " ";
                }
                //Debug.Log(currentPosStrings);
                for (int i = 0; i < newPositionsWithoutLastOne.Count; i++)
                {
                    newPosStrings += newPositionsWithoutLastOne[i].ToString() + " ";
                }
                //Debug.Log(newPosStrings);

                bool areEqualsWithoutLastOne = AreListEqual(lastPositions, newPositionsWithoutLastOne);
                //Debug.Log("Are without lastone equals: " + areEqualsWithoutLastOne);

                // IF THESE CASES ARE TRUE:
                // CASE: 0. THERE'S ONLY ONE SHOT AND NOT EQUAL = MAKE A PARABOLA TOWARDS THE SHOT AND THEN GO NEXT TURN BECAUSE NOTHING CHANGES
                // CASE: 0.5. THERE'S ONLY ONE SHOT AND THERE'S A POINT = MAKE A PARABOLA TO THE HOLE AND KEEP EVERYTHING THE SAME
                // CASE: 1. ARE LIST EQUALS = DON'T DO ANYTHING, JUST GO NEXT TURN BECAUSE NOTHING CHANGES
                // CASE: 2. ARE LIST EQUALS AND THERE'S A POINT = MAKE A PARABOLA TOWARDS THE POINT HOLE AND THEN GO NEXT TURN BECAUSE NOTHING CHANGES EXCEPT THE POINT SHOT
                // CASE: 3. ARE LIST EQUALS WITHOUT LAST ONE AND THERE'S NO POINT = MAKE A PARABOLA IN THE LAST ONE AND THEN GO NEXT TURN BECAUSE NOTHING CHANGES EXCEPT THE LAST SHOT
                // CASE: 4. COLLISIONS AND EDGE CASES.
                if (currentPositions.Count == 1 && !areEquals && currentData.pto == null)
                {
                    Vector3 output = FindObjectOfType<TextureToPlane>().PixelToWorldPosition((int)currentPositions[0].x, (int)currentPositions[0].y);
                    GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, GameMechanics.Instance.discSpawnPosition.position, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                    discTempHolder.Insert(0, disc);
                    Disc discScript = disc.GetComponent<Disc>();
                    StartCoroutine(discScript.MoveAlongParabola(output));
                    Debug.Log("0");
                }
                else if (currentPositions.Count == 1 && !areEquals && currentData.pto != null)
                {
                    Hole holePoint = GetHoleFromIndex(currentData.pto.Value);
                    GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, GameMechanics.Instance.discSpawnPosition.position, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                    //discTempHolder.Add(disc);
                    Disc discScript = disc.GetComponent<Disc>();
                    StartCoroutine(discScript.MoveAlongParabola(holePoint.transform.position));
                    Debug.Log("0.5");
                }
                else if (areEquals && currentData.pto == null)
                {
                    //Debug.Log("Son iguales, no se harán eliminaciones");
                    Debug.Log("1");
                    //Debug.Log($"lastPositions: {lastPositions.ToString()} is equal to currentPositions: {currentPositions.ToString()}");
                }
                else if (areEquals && currentData.pto != null)
                {
                    Hole holePoint = GetHoleFromIndex(currentData.pto.Value);
                    GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, GameMechanics.Instance.discSpawnPosition.position, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                    discDelayScore = disc;
                    StartCoroutine(ScorePointDelay(GetHoleFromIndex(currentData.pto.Value), disc, 0.75f));
                    //discTempHolder.Add(disc);
                    Disc discScript = disc.GetComponent<Disc>();
                    StartCoroutine(discScript.MoveAlongParabola(holePoint.transform.position));
                    Debug.Log("2");
                }
                else if (areEqualsWithoutLastOne && currentData.pto == null)
                {
                    Vector3 output = FindObjectOfType<TextureToPlane>().PixelToWorldPosition((int)currentPositions[0].x, (int)currentPositions[0].y);
                    GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, GameMechanics.Instance.discSpawnPosition.position, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                    discTempHolder.Insert(0, disc);
                    Disc discScript = disc.GetComponent<Disc>();
                    StartCoroutine(discScript.MoveAlongParabola(output));
                    Debug.Log("3");
                }
                else
                {
                    // Edge cases: A shot disc doesn't scores a point but causes some of the discs to move
                    // There should be 1 more disc but everything should be different
                    // Make a parabola towards the last position and move all the discs
                    // My solution is to move those discs from lastPositions to currentPositions using the same indexes
                    if (lastPositions.Count + 1 == currentPositions.Count && currentData.pto == null && !areEquals && !areEqualsWithoutLastOne)
                    {
                        // Make a parabola towards the last position
                        Vector3 output = FindObjectOfType<TextureToPlane>().PixelToWorldPosition((int)currentPositions[0].x, (int)currentPositions[0].y);
                        GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, GameMechanics.Instance.discSpawnPosition.position, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                        Disc discScript = disc.GetComponent<Disc>();
                        StartCoroutine(discScript.MoveAlongParabola(output));
                        StartCoroutine(CollisionMovementDelay(newPositionsWithoutLastOne, 0.65f, disc));

                        Debug.Log("<color=green>Edge case: Collision and new disk in table but no point</color>");
                    }
                    // Edge cases: A shot disc scores a point and causes some of the discs to move
                    // Make a parabola and move all the discs
                    // Pick de closest disc to the hole from the lastPositions and move it towards the hole
                    else if (lastPositions.Count == currentPositions.Count && currentData.pto != null && !areEquals && !areEqualsWithoutLastOne)
                    {
                        // Make a parabola towards the last position
                        Vector3 output = FindObjectOfType<TextureToPlane>().PixelToWorldPosition((int)currentPositions[0].x, (int)currentPositions[0].y);
                        GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, GameMechanics.Instance.discSpawnPosition.position, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                        discTempHolder.Insert(0, disc);
                        Disc discScript = disc.GetComponent<Disc>();
                        StartCoroutine(discScript.MoveAlongParabola(output));

                        // Delete the disc who inserted from discTempHolder
                        GameObject insertedDisc = PickClosestFromHole(GetHoleFromIndex(currentData.pto.Value));
                        if (discTempHolder.Contains(insertedDisc))
                            discTempHolder.Remove(insertedDisc);
                        // Move the disc towards the hole

                        discDelayScore = insertedDisc;

                        Vector3 targetPosition = GetHoleFromIndex(currentData.pto.Value).transform.position;
                        targetPosition.y = 0.395f;
                        StartCoroutine(DiscMovementDelay(insertedDisc, targetPosition, 0.65f));
                        StartCoroutine(ScorePointDelay(GetHoleFromIndex(currentData.pto.Value), insertedDisc, 1.1f));

                        Debug.Log("<color=green>Edge case: Collision and new disk in table and a point</color>");
                    }
                    // Edge cases: A shot disc somehow affects the other discs but doesn't score a point and the number of discs is the same
                    // Somehow this is possible:
                    // {"tur": 5, "pos": [[400, 280], [300, 350], [190, 410]], "pto": null, "tim": false}
                    // {"tur": 6, "pos": [[390, 270], [300, 350], [190, 410]], "pto": null, "tim": false}
                    // Move every disk, because the count is the same, we move every index equally
                    else if (lastPositions.Count == currentPositions.Count && currentData.pto == null && !areEquals && !areEqualsWithoutLastOne)
                    {
                        for (int i = 0; i < currentPositions.Count; i++)
                        {
                            Vector3 targetPosition = FindObjectOfType<TextureToPlane>().PixelToWorldPosition((int)currentPositions[i].x, (int)currentPositions[i].y);

                            if (discTempHolder[i] != null)
                                discTempHolder[i].transform.DOMove(targetPosition, 0.5f).SetEase(Ease.OutExpo);

                        }
                    }
                    // Else, just delete everything and reset the discs
                    else
                    {
                        GameObject[] marks = GameObject.FindGameObjectsWithTag("Mark");
                        foreach (GameObject mark in marks)
                        {
                            Destroy(mark);
                        }

                        // Delete the last disc
                        GameObject[] discs = GameObject.FindGameObjectsWithTag("Disc");
                        foreach (GameObject disc in discs)
                        {
                            Destroy(disc);
                        }

                        // Spawn new disc
                        for (int i = 0; i < currentPositions.Count; i++)
                        {
                            Vector3 output = FindObjectOfType<TextureToPlane>().PixelToWorldPosition((int)currentPositions[i].x, (int)currentPositions[i].y);
                            GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, output, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                            disc.transform.position = new Vector3(disc.transform.position.x, disc.transform.position.y + 20, disc.transform.position.z);
                            disc.transform.DOMove(output, 0.5f).SetEase(Ease.InOutBounce);
                            discTempHolder.Insert(0, disc);
                        }

                        Debug.Log("<color=red>Couldn't catch one case</color>");
                    }
                }

                //bool shouldUseParabola = true;

                // There was a point and that disc that didn't moved anything else
                //if (AreListEqual(currentPositions, lastPositions) && currentData.pto != null)
                //{
                //    GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, GameMechanics.Instance.discSpawnPosition.position, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                //    Disc discScript = disc.GetComponent<Disc>();
                //    Hole holePoint = GetHoleFromIndex(currentData.pto.Value);
                //    StartCoroutine(discScript.MoveAlongParabola(holePoint.transform.position));
                //    shouldUseParabola = false;
                //}
                //if (shouldUseParabola && currentPositions.Count > lastPositions.Count)
                //{
                //    GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, GameMechanics.Instance.discSpawnPosition.position, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                //    Disc discScript = disc.GetComponent<Disc>();
                //    Vector3 output = FindObjectOfType<TextureToPlane>().PixelToWorldPosition((int)positions[currentPositions.Count-1].x, (int)positions[currentPositions.Count-1].y);
                //    StartCoroutine(discScript.MoveAlongParabola(output));
                //    shouldUseParabola = false;
                //}
                //
                //for (int i = 0; i < positions.Count; i++)
                //{
                //    if (areEquals)
                //    {
                //        break;
                //    }
                //    Vector3 output = FindObjectOfType<TextureToPlane>().PixelToWorldPosition((int)positions[i].x, (int)positions[i].y);
                //
                //    //GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //    // Make sphere unlit
                //    //mark.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));
                //    //mark.transform.position = output;
                //    //mark.transform.localScale = new Vector3(0.125f, 0.125f, 0.125f);
                //    //mark.GetComponent<Renderer>().material.color = Color.green;
                //    //mark.tag = "Mark";
                //
                //    if (i == positions.Count - 1 && shouldUseParabola)
                //    {
                //        GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, GameMechanics.Instance.discSpawnPosition.position, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                //        Disc discScript = disc.GetComponent<Disc>();
                //        StartCoroutine(discScript.MoveAlongParabola(output));
                //        //disc.transform.position = new Vector3(disc.transform.position.x, disc.transform.position.y + 20, disc.transform.position.z);
                //        //disc.transform.DOMove(output, 0.5f).SetEase(Ease.InOutBounce);
                //    }
                //    else
                //    {
                //        if (i != positions.Count - 1)
                //        {
                //            GameObject disc = Instantiate(GameMechanics.Instance.gameDiscPrefab, output, GameMechanics.Instance.gameDiscPrefab.transform.rotation);
                //            disc.transform.position = new Vector3(disc.transform.position.x, disc.transform.position.y + 20, disc.transform.position.z);
                //            disc.transform.DOMove(output, 0.5f).SetEase(Ease.InOutBounce);
                //        }              
                //    }
                //}

                if (currentData.pto != null)
                {
                    // Search for a hole with the respective number and call ScorePoint
                    Hole[] tempHoles = GameMechanics.Instance.GetHoles();
                    foreach (Hole hole in tempHoles)
                    {
                        if (hole.GetHoleIndex() == currentData.pto)
                        {
                            if (discDelayScore == null)
                            {
                                StartCoroutine(ScorePointDelay(hole, 0.65f));
                            }
                            else
                            {
                                //StartCoroutine(ScorePointDelay(hole, discDelayScore, 0.65f));
                            }
                        }
                    }
                }
                else
                {
                    // GPT FIX
                    GameMechanics.Instance.FailedShot();
                }

                if (currentData.tim == true)
                {
                    GameMechanics.Instance.isTimeout = true;
                }
                else
                {
                    GameMechanics.Instance.isTimeout = false;
                }

                int previousTurn = GameMechanics.Instance.jsonTurn;
                GameMechanics.Instance.jsonTurn = currentData.tur;

                // Debug log to track turn changes
                if (currentData.tur != previousTurn)
                {
                    Debug.Log($"[v0] Turn updated from {previousTurn} to {currentData.tur}");
                }
                else
                {
                    Debug.Log($"[v0] Same turn ({currentData.tur}) - duplicate JSON received");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"JSON Parsing Error: {e.Message} /////// {e.Source} /////// {e.Data}");
            }

            DataManager.instance.data = null;
            count++;
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            // Find all objects with the tag "Mark"
            //GameObject[] marks = GameObject.FindGameObjectsWithTag("Disc");
            // Destroy all objects
            //foreach (GameObject mark in marks)
            //{
            //    Destroy(mark);
            //}
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            //deleteLastMarks = !deleteLastMarks;
            //Debug.LogError("Delete last marks is now: " + deleteLastMarks);
        }
    }

    private IEnumerator CollisionMovementDelay(List<Vector2> newPositionsWithoutLastOne, float delay, GameObject discToInsertAfter)
    {
        yield return new WaitForSeconds(delay);

        int n = Mathf.Min(lastPositions.Count, newPositionsWithoutLastOne.Count, discTempHolder.Count);

        for (int i = 0; i < n; i++)
        {
            Vector3 targetPosition = FindObjectOfType<TextureToPlane>()
                .PixelToWorldPosition((int)newPositionsWithoutLastOne[i].x, (int)newPositionsWithoutLastOne[i].y);

            if (discTempHolder[i] != null)
                discTempHolder[i].transform.DOMove(targetPosition, 0.5f).SetEase(Ease.OutExpo);
        }

        if (discToInsertAfter != null)
            discTempHolder.Insert(0, discToInsertAfter);
    }

    private IEnumerator DiscMovementDelay(GameObject insertedDisc, Vector3 targetPos, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (insertedDisc != null)
            insertedDisc.transform.DOMove(targetPos, 0.65f).SetEase(Ease.OutExpo);
    }
    private IEnumerator ScorePointDelay(Hole hole, float delay)
    {
        //Debug.Log("SP1");
        yield return new WaitForSeconds(delay);
        hole.ScorePoint(Instantiate(GameMechanics.Instance.gameDiscPrefab));
    }
    private IEnumerator ScorePointDelay(Hole hole, GameObject disc, float delay)
    {
        //Debug.Log("SP2");
        yield return new WaitForSeconds(delay);
        hole.ScorePoint(disc);
    }
    public GameObject PickClosestFromHole(Hole hole)
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < discTempHolder.Count; i++)
        {
            if (Vector3.Distance(discTempHolder[i].transform.position, hole.transform.position) < closestDistance)
            {
                closestDistance = Vector3.Distance(discTempHolder[i].transform.position, hole.transform.position);
                closestIndex = i;
            }
        }

        return discTempHolder[closestIndex];
    }
    public Hole GetHoleFromIndex(int index)
    {
        foreach (var hole in GameMechanics.Instance.GetHoles())
        {
            if (hole.GetHoleIndex() == index)
            {
                return hole;
            }
        }

        return null;
    }

    public bool AreListEqual(List<Vector2> list1, List<Vector2> list2)
    {
        return list1.Count == list2.Count && list1.TrueForAll(e => list2.Contains(e));
    }
    private string FormatPositions(List<Vector2> positions)
    {
        if (positions == null || positions.Count == 0) return "None";
        return string.Join(", ", positions);
    }
}
