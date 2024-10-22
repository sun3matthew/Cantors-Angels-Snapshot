using System;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using UnityEngine.SocialPlatforms.Impl;

public class Backend : MonoBehaviour
{
    public Text text;

    // current date
    public static DateTime Origin => new(2023, 5, 8, 0, 0, 0, DateTimeKind.Utc);
    DateTime LastUpdate;
    private SteamLeaderboard_t LeaderboardHandle;
    void Start()
    {
        LastUpdate = DateTime.Now;
        UpdateText();
    }

    void Update()
    {
        DateTime now = DateTime.Now;
        // new day
        if (now.Date != LastUpdate.Date)
        {
            LastUpdate = now;
            // new week
            // if (now.DayOfWeek == DayOfWeek.Monday)
            // {

            // }
        }
        UpdateText();
    }
    public void CreateNewDayLeaderBoard()
    {
        string name = NameOfDailyLeaderBoard(DateTime.Now);
        // result
        SteamAPICall_t handle = SteamUserStats.FindOrCreateLeaderboard(name, ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending, ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
        CallResult<LeaderboardFindResult_t> callResult = new CallResult<LeaderboardFindResult_t>();
        callResult.Set(handle, (result, failure) =>
        {
            if (failure)
            {
                Debug.LogError("Failed to create leaderboard: " + name);
                return;
            }
            LeaderboardHandle = result.m_hSteamLeaderboard;
            Debug.Log("Leaderboard created: " + name);
        });
    }
    // public static SteamLeaderboard_t GetLeaderboardHandle(string name)
    // {
    //     SteamAPICall_t handle = SteamUserStats.FindLeaderboard(name);
    //     CallResult<LeaderboardFindResult_t> callResult = new CallResult<LeaderboardFindResult_t>();
    // }
    public static string NameOfDailyLeaderBoard(DateTime date)
    {
        int days = (int)(date - Origin).TotalDays;
        days++;
        return "Daily Leaderboard: Day - " + days;
    }
    public static string NameOfWeeklyLeaderBoard(DateTime date)
    {
        int days = (int)(date - Origin).TotalDays;
        int weeks = days / 7;
        weeks++;
        return "Weekly Leaderboard: Week - " + weeks;
    }

    public void UpdateText()
    {
        // next update in
        // text.text = "Next update in: " + (60 - counter).ToString("0.00");

    }
}
