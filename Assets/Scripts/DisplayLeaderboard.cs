using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayLeaderboard : MonoBehaviour
{
    public TextMeshProUGUI first;
    public TextMeshProUGUI second;
    public TextMeshProUGUI third;
    public TextMeshProUGUI fourth;

    void Start()
    {
        Leaderboard.Reset();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        List<string> places = Leaderboard.GetPlaces();

        // Safely update the leaderboard display
        first.text = places.Count > 0 ? places[0] : "";
        second.text = places.Count > 1 ? places[1] : "";
        third.text = places.Count > 2 ? places[2] : "";
        fourth.text = places.Count > 3 ? places[3] : "";
    }
}
