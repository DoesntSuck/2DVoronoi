using UnityEngine;
using System.Collections.Generic;
using Graph2D;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace Assets
{
    public class ClickToBreakSpeedTest : MonoBehaviour
    {
        public GameObject ClickToBreakPrefab;
        public int ChunksFrom;
        public int ChunksTo;
        public float Radius;
        public int NTests;

        private Stopwatch stopWatch;

        void Start()
        {
            stopWatch = new Stopwatch();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Test();
        }

        public void Test()
        {
            using (StreamWriter streamWriter = File.AppendText(@"C:\Users\Stephen\Documents\GitHub\2DVoronoi\MeshBreakSpeedTest.txt"))
            {
                for (int i = ChunksFrom; i < ChunksTo; i += 5)
                {
                    double averageTime = TestNChunks(i, NTests);
                    streamWriter.WriteLine(i + ", " + averageTime);
                }
            }
            
            print("Done");
        }

        double TestNChunks(int nChunks, int nTests)
        {
            // Track the time and location of smashing
            List<double> elapsedTimes = new List<double>();
            List<Vector2> impactPoints = new List<Vector2>();
            for (int j = 0; j < nTests; j++)
            {
                // Create a new thing to be smashed
                GameObject obj = Instantiate(ClickToBreakPrefab, Vector3.zero, Quaternion.identity, transform) as GameObject;
                ClickToBreak clickToBreak = obj.GetComponent<ClickToBreak>();
                clickToBreak.ChunkCount = nChunks;
                clickToBreak.Radius = Radius;

                // Generate a random point within the square
                float x = Random.Range(-0.5f, 0.5f);
                float y = Random.Range(-0.5f, 0.5f);

                // Convert to Vector, remeber it.
                Vector2 point = new Vector2(x, y);
                impactPoints.Add(point);

                // Break whilst keeping track of time
                stopWatch.Start();
                List<GameObject> chunks = clickToBreak.BreakAbout(point);
                stopWatch.Stop();

                // Remeber time
                elapsedTimes.Add(stopWatch.Elapsed.TotalMilliseconds);
                stopWatch.Reset();

                // Clean up
                foreach (GameObject chunk in chunks)
                    DestroyImmediate(chunk);
                DestroyImmediate(clickToBreak);
            }

            // Print results
            double sum = elapsedTimes.Sum();

            double averageTime = sum / (double)elapsedTimes.Count;
            return averageTime;
        }
    }
}