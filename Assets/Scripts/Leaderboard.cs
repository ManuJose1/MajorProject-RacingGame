using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

struct PlayerStats
{
    public string name;
    public int position;
    public float time;

    public PlayerStats(string n, int p, float t)
    {
        name = n;
        position = p;
        time = t;
    }
}

public class Leaderboard
{
    static Dictionary<int, PlayerStats> lb = new Dictionary<int, PlayerStats>();
    static int carsRegisered = -1;

    public static void Reset()
    {
        lb.Clear();
        carsRegisered = -1;
    }

    public static int RegisterCar(string name)
    {
        carsRegisered++;
        Debug.Log("Registering car with name: " + name);
        lb.Add(carsRegisered, new PlayerStats(name, 0, 0));
        return carsRegisered;
    }

    public static void SetPosition(int reg, int lap, int checkPoint, float time)
    {
        int position = lap * 1000 + checkPoint;
        lb[reg] = new PlayerStats(lb[reg].name, position, time);
    }

    public static string GetPosition(int reg)
    {
       int index = 0; 
       foreach(KeyValuePair<int, PlayerStats> pos in lb.OrderByDescending(key => key.Value.position).ThenBy(key => key.Value.time))
       {
            index++;
            if(pos.Key == reg)
                switch (index)
                {
                    case 1: return "1st";
                    case 2: return "2nd";
                    case 3: return "3rd";
                    default: return index + "th";
                }
       }
       return "Unknown";
    }

    public static List<string> GetPlaces()
    {
        List<string> places = new List<string>();
        foreach (KeyValuePair<int, PlayerStats> pos in lb.OrderByDescending(key => key.Value.position).ThenBy(key => key.Value.time))
        {
            places.Add(GetPosition(pos.Key) +" " +pos.Value.name);
        }
        return places;
    }
}