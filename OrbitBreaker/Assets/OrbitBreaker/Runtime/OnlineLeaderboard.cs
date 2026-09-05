using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace OrbitBreaker
{
    public readonly struct OrbitLeaderboardEntry
    {
        public readonly int Rank;
        public readonly string PlayerName;
        public readonly int Score;
        public readonly bool IsLocalPlayer;

        public OrbitLeaderboardEntry(int rank, string playerName, int score, bool isLocalPlayer)
        {
            Rank = rank; PlayerName = playerName; Score = score; IsLocalPlayer = isLocalPlayer;
        }
    }

    public sealed class OnlineLeaderboard : MonoBehaviour
    {
        public const string LeaderboardId = "orbit_breaker_distance";
        private const string LocalNameKey = "OrbitBreaker.PlayerName";
        private const string PendingScoreKey = "OrbitBreaker.PendingLeaderboardScore";
        private const int PageSize = 100;
        private readonly List<OrbitLeaderboardEntry> cachedEntries = new List<OrbitLeaderboardEntry>();

        public bool IsReady { get; private set; }
        public bool IsBusy { get; private set; }
        public string LastError { get; private set; }
        public string PlayerName => PlayerPrefs.GetString(LocalNameKey, string.Empty);
        public bool NeedsPlayerName => string.IsNullOrWhiteSpace(PlayerName);
        public IReadOnlyList<OrbitLeaderboardEntry> CachedEntries => cachedEntries;

        public async Task InitializeAsync()
        {
            if (IsReady || IsBusy) return;
            IsBusy = true; LastError = string.Empty;
            try
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized) await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
                IsReady = true;
                if (!NeedsPlayerName)
                {
                    await TrySynchronizePlayerNameAsync(PlayerName);
                    int pending = PlayerPrefs.GetInt(PendingScoreKey, 0);
                    if (pending > 0) await SubmitBestScoreAsync(pending);
                }
            }
            catch (Exception exception)
            {
                LastError = FriendlyError(exception);
                Debug.LogWarning("Orbit Breaker online services unavailable: " + exception.Message);
            }
            finally { IsBusy = false; }
        }

        public async Task<bool> SavePlayerNameAsync(string requestedName)
        {
            string cleanName = SanitizeName(requestedName);
            if (cleanName.Length < 3)
            {
                LastError = "LE PSEUDO DOIT CONTENIR AU MOINS 3 CARACTÈRES";
                return false;
            }
            PlayerPrefs.SetString(LocalNameKey, cleanName); PlayerPrefs.Save(); LastError = string.Empty;
            if (!IsReady) return true;
            return await TrySynchronizePlayerNameAsync(cleanName);
        }

        public async Task SubmitBestScoreAsync(int score)
        {
            if (score <= 0) return;
            int pending = Mathf.Max(score, PlayerPrefs.GetInt(PendingScoreKey, 0));
            PlayerPrefs.SetInt(PendingScoreKey, pending); PlayerPrefs.Save();
            if (!IsReady || NeedsPlayerName) return;
            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, pending);
                PlayerPrefs.DeleteKey(PendingScoreKey); PlayerPrefs.Save();
            }
            catch (Exception exception)
            {
                LastError = FriendlyError(exception);
                Debug.LogWarning("Leaderboard score queued for retry: " + exception.Message);
            }
        }

        public async Task<IReadOnlyList<OrbitLeaderboardEntry>> RefreshAsync(string search = "")
        {
            cachedEntries.Clear(); LastError = string.Empty;
            if (!IsReady) { await InitializeAsync(); if (!IsReady) return cachedEntries; }
            IsBusy = true;
            try
            {
                LeaderboardScoresPage page = await LeaderboardsService.Instance.GetScoresAsync(LeaderboardId, new GetScoresOptions { Offset = 0, Limit = PageSize });
                string playerId = AuthenticationService.Instance.PlayerId;
                foreach (LeaderboardEntry entry in page.Results)
                    cachedEntries.Add(new OrbitLeaderboardEntry(entry.Rank + 1, StripDiscriminator(entry.PlayerName), Mathf.RoundToInt((float)entry.Score), entry.PlayerId == playerId));
            }
            catch (Exception exception)
            {
                LastError = FriendlyError(exception);
                Debug.LogWarning("Unable to refresh leaderboard: " + exception.Message);
            }
            finally { IsBusy = false; }
            return Filter(search);
        }

        public IReadOnlyList<OrbitLeaderboardEntry> Filter(string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return cachedEntries;
            string query = search.Trim();
            return cachedEntries.Where(entry => entry.PlayerName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        private async Task<bool> TrySynchronizePlayerNameAsync(string cleanName)
        {
            try { await AuthenticationService.Instance.UpdatePlayerNameAsync(cleanName); LastError = string.Empty; return true; }
            catch (Exception exception)
            {
                LastError = FriendlyError(exception);
                Debug.LogWarning("Player name will be synchronized later: " + exception.Message);
                return false;
            }
        }

        private static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return new string(value.Trim().Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').Take(16).ToArray());
        }

        private static string StripDiscriminator(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "PILOTE";
            int separator = value.LastIndexOf('#');
            return separator > 0 ? value.Substring(0, separator) : value;
        }

        private static string FriendlyError(Exception exception)
        {
            string message = exception.Message ?? string.Empty;
            if (message.IndexOf("leaderboard", StringComparison.OrdinalIgnoreCase) >= 0 && message.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0)
                return "CLASSEMENT PAS ENCORE DÉPLOYÉ";
            return Application.internetReachability == NetworkReachability.NotReachable ? "CONNEXION INTERNET INDISPONIBLE" : "SERVICE EN LIGNE TEMPORAIREMENT INDISPONIBLE";
        }
    }
}
