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
        public readonly int Rank, Score, EndlessBest, SprintBest;
        public readonly string PlayerName;
        public readonly bool IsLocalPlayer;
        public OrbitLeaderboardEntry(int rank, string playerName, int score, int endlessBest, int sprintBest, bool local)
        { Rank=rank; PlayerName=playerName; Score=score; EndlessBest=endlessBest; SprintBest=sprintBest; IsLocalPlayer=local; }
    }

    public sealed class OnlineLeaderboard : MonoBehaviour
    {
        public const string LeaderboardId="orbit_breaker_distance", SprintLeaderboardId="orbit_breaker_sprint_90";
        const string NameKey="OrbitBreaker.PlayerName", PendingEndless="OrbitBreaker.PendingLeaderboardScore", PendingSprint="OrbitBreaker.PendingSprintLeaderboardScore";
        const int PageSize=100;
        [Serializable] sealed class Profile { public int endless, sprint; }
        readonly Dictionary<RunMode,List<OrbitLeaderboardEntry>> caches=new() { {RunMode.Endless,new()}, {RunMode.Sprint,new()} };
        readonly System.Threading.SemaphoreSlim submitGate=new(1,1), refreshGate=new(1,1), initGate=new(1,1);
        public DateTime? LastRefreshUtc { get; private set; }
        public RunMode ActiveMode { get; private set; }=RunMode.Endless;
        public bool IsReady { get; private set; }
        public bool IsBusy { get; private set; }
        public string LastError { get; private set; }
        public string PlayerName=>PlayerPrefs.GetString(NameKey,string.Empty);
        public bool NeedsPlayerName=>string.IsNullOrWhiteSpace(PlayerName);
        public IReadOnlyList<OrbitLeaderboardEntry> CachedEntries=>caches[ActiveMode];
        public void SelectBoard(RunMode mode)=>ActiveMode=mode==RunMode.Sprint?RunMode.Sprint:RunMode.Endless;

        public async Task InitializeAsync()
        {
            if(IsReady)return; await initGate.WaitAsync(); if(IsReady){initGate.Release();return;} IsBusy=true;LastError="";
            try { if(UnityServices.State==ServicesInitializationState.Uninitialized)await UnityServices.InitializeAsync(); if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync(); IsReady=true; if(!NeedsPlayerName){await SyncName(PlayerName);await RetryPending();} }
            catch(Exception e){LastError=FriendlyError(e);Debug.LogWarning("Orbit Breaker online services unavailable: "+e.Message);}
            finally{IsBusy=false;initGate.Release();}
        }
        public async Task<bool> SavePlayerNameAsync(string requested)
        {
            string clean=Sanitize(requested); if(clean.Length<3){LastError="LE PSEUDO DOIT CONTENIR AU MOINS 3 CARACTÈRES";return false;}
            PlayerPrefs.SetString(NameKey,clean);PlayerPrefs.Save();LastError="";if(!IsReady)return true;bool ok=await SyncName(clean);if(ok)await RetryPending();return ok;
        }
        public Task SubmitBestScoreAsync(int score)=>QueueSubmit(RunMode.Endless,score);
        public Task SubmitSprintScoreAsync(int score)=>QueueSubmit(RunMode.Sprint,score);
        async Task QueueSubmit(RunMode mode,int score)
        {
            if(score<=0||mode==RunMode.Daily)return;string key=PendingKey(mode);PlayerPrefs.SetInt(key,Mathf.Max(score,PlayerPrefs.GetInt(key,0)));PlayerPrefs.Save();if(!IsReady||NeedsPlayerName)return;
            await submitGate.WaitAsync();try{int pending;while((pending=PlayerPrefs.GetInt(key,0))>0){var meta=new Profile{endless=Mathf.Max(PlayerPrefs.GetInt("OrbitBreaker.BestScore",0),mode==RunMode.Endless?pending:0),sprint=Mathf.Max(LocalRunStats.Best(RunMode.Sprint,0),mode==RunMode.Sprint?pending:0)};await LeaderboardsService.Instance.AddPlayerScoreAsync(Board(mode),pending,new AddPlayerScoreOptions{Metadata=meta});if(PlayerPrefs.GetInt(key,0)<=pending){PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();}}}
            catch(Exception e){LastError=FriendlyError(e);Debug.LogWarning("Leaderboard score queued for retry: "+e.Message);}finally{submitGate.Release();}
        }
        async Task RetryPending(){int a=PlayerPrefs.GetInt(PendingEndless,0),b=PlayerPrefs.GetInt(PendingSprint,0);if(a>0)await QueueSubmit(RunMode.Endless,a);if(b>0)await QueueSubmit(RunMode.Sprint,b);}
        public async Task<IReadOnlyList<OrbitLeaderboardEntry>> RefreshAsync(string search="")
        {
            if(!await refreshGate.WaitAsync(0))return Filter(search);try{LastError="";if(!IsReady){await InitializeAsync();if(!IsReady)return Filter(search);}IsBusy=true;RunMode requested=ActiveMode;LeaderboardScoresPage page=await LeaderboardsService.Instance.GetScoresAsync(Board(requested),new GetScoresOptions{Offset=0,Limit=PageSize,IncludeMetadata=true});string id=AuthenticationService.Instance.PlayerId;var replacement=new List<OrbitLeaderboardEntry>(page.Results.Count);foreach(LeaderboardEntry entry in page.Results){Profile p=Parse(entry.Metadata);int score=Mathf.RoundToInt((float)entry.Score);replacement.Add(new OrbitLeaderboardEntry(entry.Rank+1,Strip(entry.PlayerName),score,Mathf.Max(p.endless,requested==RunMode.Endless?score:0),Mathf.Max(p.sprint,requested==RunMode.Sprint?score:0),entry.PlayerId==id));}caches[requested].Clear();caches[requested].AddRange(replacement);LastRefreshUtc=DateTime.UtcNow;}
            catch(Exception e){LastError=FriendlyError(e);Debug.LogWarning("Unable to refresh leaderboard: "+e.Message);}finally{IsBusy=false;refreshGate.Release();}return Filter(search);
        }
        public IReadOnlyList<OrbitLeaderboardEntry> Filter(string search){IReadOnlyList<OrbitLeaderboardEntry> source=caches[ActiveMode];if(string.IsNullOrWhiteSpace(search))return source;string q=search.Trim();return source.Where(e=>e.PlayerName.IndexOf(q,StringComparison.OrdinalIgnoreCase)>=0).ToList();}
        public static string FormatRow(OrbitLeaderboardEntry entry, RunMode mode)
        {
            // Le contexte est déjà affiché par l'onglet actif. Une seule ligne évite tout
            // chevauchement sur les écrans étroits et conserve dix rangs visibles.
            return "#" + entry.Rank.ToString("000") + "  " + entry.PlayerName.ToUpperInvariant()
                + "     " + entry.Score + " UA";
        }
        static Profile Parse(string json){if(string.IsNullOrWhiteSpace(json))return new Profile();try{return JsonUtility.FromJson<Profile>(json)??new Profile();}catch{return new Profile();}}
        async Task<bool> SyncName(string clean){try{await AuthenticationService.Instance.UpdatePlayerNameAsync(clean);LastError="";return true;}catch(Exception e){LastError=FriendlyError(e);Debug.LogWarning("Player name will be synchronized later: "+e.Message);return false;}}
        static string Board(RunMode mode)=>mode==RunMode.Sprint?SprintLeaderboardId:LeaderboardId;
        static string PendingKey(RunMode mode)=>mode==RunMode.Sprint?PendingSprint:PendingEndless;
        static string Sanitize(string value)=>string.IsNullOrWhiteSpace(value)?string.Empty:new string(value.Trim().Where(c=>char.IsLetterOrDigit(c)||c=='_'||c=='-').Take(16).ToArray());
        static string Strip(string value){if(string.IsNullOrWhiteSpace(value))return "PILOTE";int i=value.LastIndexOf('#');return i>0?value.Substring(0,i):value;}
        static string FriendlyError(Exception e){string m=e.Message??"";if(m.IndexOf("leaderboard",StringComparison.OrdinalIgnoreCase)>=0&&m.IndexOf("not found",StringComparison.OrdinalIgnoreCase)>=0)return "CLASSEMENT PAS ENCORE DÉPLOYÉ";return Application.internetReachability==NetworkReachability.NotReachable?"CONNEXION INTERNET INDISPONIBLE":"SERVICE EN LIGNE TEMPORAIREMENT INDISPONIBLE";}
    }
}
