using UnityEngine;
using System.Collections;
using TMPro;

public class Laps : MonoBehaviour {

    private const string COLOR_RED = "<color=#ff0000>";
    private const string COLOR_GREEN = "<color=#90ee90>";

    // These Static Variables are accessed in "checkpoint" Script
    public Transform[] checkpoints;
    public float[] segmentTimes, bestSegmentTimes;
    public int nextCheckpoint;
    public int currentLap;
    public float timer = 0;

    //TIMER
    private int minutes, seconds, milliseconds;

    void Start() {
        nextCheckpoint = 0;
        currentLap = 0;
        segmentTimes = new float[checkpoints.Length];
        bestSegmentTimes = new float[checkpoints.Length];
    }

    void FixedUpdate() {
        updateTimer();
    }

    private void updateTimer() {
        timer += Time.deltaTime * 1000;
    }

    public float getTimer() {
        return timer;
    }

    private void setSegmentTimes() {
        if(currentLap != 0) {
            segmentTimes[nextCheckpoint] = timer;
            if (bestSegmentTimes[nextCheckpoint] == 0 || segmentTimes[nextCheckpoint] < bestSegmentTimes[nextCheckpoint]) {
                bestSegmentTimes[nextCheckpoint] = segmentTimes[nextCheckpoint];
            }
        }
    }

    private string getTextColor(int index) {
        if(bestSegmentTimes[index] < segmentTimes[index]) {
            return COLOR_RED;
        }
        else {
            return COLOR_GREEN;
        }
    }

    public string getCheckpointTimes(TextMeshPro n, TextMeshPro nMinus, TextMeshPro nPlus, TextMeshPro currTime) {
        int index;
        //nMinus.text = bestSegmentTimes[nextCheckpoint]
        currTime.text = getTimeString(timer);

        index = nextCheckpoint;

        n.text = getTimeString(bestSegmentTimes[index]) + "\n\n" + getTextColor(index) + getTimeString(segmentTimes[index]) + "</color>";

        index = nextCheckpoint - 1 >= 0 ? nextCheckpoint - 1 : bestSegmentTimes.Length - 1;
        
        nMinus.text = getTimeString(bestSegmentTimes[index])  + "\n\n" + getTextColor(index) + getTimeString(segmentTimes[index]) + "</color>";

        index = nextCheckpoint + 1 < bestSegmentTimes.Length - 1 ? nextCheckpoint + 1 : 0;

        nPlus.text = getTimeString(bestSegmentTimes[index]) + "\n\n" + getTextColor(index) + getTimeString(segmentTimes[index]) + "</color>";


        string res = "";
        foreach (float s in bestSegmentTimes) {
            res += getTimeString(s) + "\n";
        }
        return res;
    }

    public string getTimeString(float t) {
        minutes = (int)t / 60000;
        seconds = (int)t / 1000 - 60 * minutes;
        milliseconds = (int)t - minutes * 60000 - 1000 * seconds;
        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    public void passedCheckpoint(Transform checkpoint) {
          if (checkpoint == checkpoints[nextCheckpoint]) {
            setSegmentTimes();
            if (nextCheckpoint == 0) {
                timer = 0;
                currentLap++;
            }
            if (nextCheckpoint + 1 >= checkpoints.Length) {
                nextCheckpoint = 0;
            }
            else {
                nextCheckpoint++;
            }
          }  
    }
}