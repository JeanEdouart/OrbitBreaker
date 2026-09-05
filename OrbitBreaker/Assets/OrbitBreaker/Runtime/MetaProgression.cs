using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    public enum ChallengeKind { Distance, Orbits, Skips, Synchronizations, NearMisses, Materials, Multiplier, Runs }
    public enum CosmeticKind { Rocket, Trail, PlanetPack, Background }

    public readonly struct ChallengeDefinition
    {
        public ChallengeDefinition(int id, ChallengeKind kind, int target, int reward)
        { Id=id; Kind=kind; Target=target; Reward=reward; }
        public int Id { get; }
        public ChallengeKind Kind { get; }
        public int Target { get; }
        public int Reward { get; }
        public string Label => Kind switch {
            ChallengeKind.Distance => "PARCOURIR " + Target + " m",
            ChallengeKind.Orbits => "CAPTURER " + Target + " ORBITES",
            ChallengeKind.Skips => "RÉUSSIR " + Target + " SKIPS",
            ChallengeKind.Synchronizations => "RÉUSSIR " + Target + " SYNCHRONISATIONS",
            ChallengeKind.NearMisses => "RÉUSSIR " + Target + " FRÔLEMENTS",
            ChallengeKind.Materials => "RÉCUPÉRER " + Target + " MATÉRIAUX",
            ChallengeKind.Multiplier => "ATTEINDRE UN MULTIPLICATEUR x" + (Target/10f).ToString("0.0"),
            _ => "TERMINER " + Target + " PARTIES" };
    }

    public readonly struct CosmeticDefinition
    {
        public CosmeticDefinition(string id,string name,CosmeticKind kind,int price,int visualIndex)
        { Id=id;Name=name;Kind=kind;Price=price;VisualIndex=visualIndex; }
        public string Id { get; }
        public string Name { get; }
        public CosmeticKind Kind { get; }
        public int Price { get; }
        public int VisualIndex { get; }
    }

    public static class MetaProgression
    {
        private const string P="OrbitBreaker.Meta.";
        public static readonly CosmeticDefinition[] Catalog = {
            new("rocket_default","ORBITER",CosmeticKind.Rocket,0,0), new("rocket_interceptor","INTERCEPTOR",CosmeticKind.Rocket,180,1),
            new("rocket_miner","FOREUSE",CosmeticKind.Rocket,260,2), new("rocket_retro","RÉTRO",CosmeticKind.Rocket,320,3),
            new("rocket_crystal","CRISTAL",CosmeticKind.Rocket,520,4), new("rocket_bio","BIOSHIP",CosmeticKind.Rocket,680,5),
            new("rocket_banana","BANANA",CosmeticKind.Rocket,850,6), new("rocket_stealth","FANTÔME",CosmeticKind.Rocket,1100,7),
            new("rocket_lander","LUNAIRE",CosmeticKind.Rocket,1450,8), new("rocket_gold","SOLARIS",CosmeticKind.Rocket,2400,9),
            new("trail_cyan","ION CYAN",CosmeticKind.Trail,0,0), new("trail_plasma","PLASMA ROSE",CosmeticKind.Trail,280,1),
            new("trail_toxic","COMÈTE VERTE",CosmeticKind.Trail,520,2), new("trail_solar","FEU SOLAIRE",CosmeticKind.Trail,900,3),
            new("planets_default","ORBIT BREAKER",CosmeticKind.PlanetPack,0,0), new("planets_solar","SYSTÈME SOLAIRE",CosmeticKind.PlanetPack,900,1),
            new("planets_anime","MONDES DU DRAGON",CosmeticKind.PlanetPack,1600,2),
            new("background_default","ESPACE PROFOND",CosmeticKind.Background,0,0), new("background_ion","NÉBULEUSE ION",CosmeticKind.Background,700,1),
            new("background_inferno","TEMPÊTE SOLAIRE",CosmeticKind.Background,1300,2)
        };

        public static int Materials => PlayerPrefs.GetInt(P+"Materials",0);
        public static int Selected(CosmeticKind kind)=>PlayerPrefs.GetInt(P+"Selected."+kind,0);
        public static bool Owned(CosmeticDefinition item)=>item.Price==0||PlayerPrefs.GetInt(P+"Owned."+item.Id,0)==1;
        public static bool BuyOrEquip(CosmeticDefinition item)
        {
            if(!Owned(item)) { if(Materials<item.Price)return false; PlayerPrefs.SetInt(P+"Materials",Materials-item.Price); PlayerPrefs.SetInt(P+"Owned."+item.Id,1); }
            PlayerPrefs.SetInt(P+"Selected."+item.Kind,item.VisualIndex); PlayerPrefs.Save(); return true;
        }
        public static void AddMaterials(int amount){if(amount<=0)return;PlayerPrefs.SetInt(P+"Materials",Materials+amount);PlayerPrefs.Save();}

        public static ChallengeDefinition Challenge(int id)
        {
            int normalized=Mathf.Abs(id)%100; ChallengeKind kind=(ChallengeKind)(normalized%8); int tier=normalized/8;
            int target=kind switch { ChallengeKind.Distance=>350+tier*100, ChallengeKind.Orbits=>6+tier*2, ChallengeKind.Skips=>2+tier,
                ChallengeKind.Synchronizations=>2+tier, ChallengeKind.NearMisses=>2+tier, ChallengeKind.Materials=>8+tier*2,
                ChallengeKind.Multiplier=>18+tier*2, _=>2+tier };
            return new ChallengeDefinition(normalized,kind,target,35+tier*10+(int)kind*3);
        }
        public static int ActiveChallengeId(int slot){EnsureChallenges();return PlayerPrefs.GetInt(P+"Challenge."+slot,slot);}
        public static int ChallengeProgress(int slot){EnsureChallenges();return PlayerPrefs.GetInt(P+"Progress."+slot,0);}
        public static bool ChallengeClaimed(int slot){return PlayerPrefs.GetInt(P+"Claimed."+slot,0)==1;}

        public static void RecordRun(int distance,int orbits,int skips,int sync,int nearMiss,int collected,float maxMultiplier)
        {
            EnsureChallenges();
            for(int slot=0;slot<3;slot++){
                ChallengeDefinition c=Challenge(ActiveChallengeId(slot)); int add=c.Kind switch {
                    ChallengeKind.Distance=>distance,ChallengeKind.Orbits=>orbits,ChallengeKind.Skips=>skips,
                    ChallengeKind.Synchronizations=>sync,ChallengeKind.NearMisses=>nearMiss,ChallengeKind.Materials=>collected,
                    ChallengeKind.Multiplier=>Mathf.RoundToInt(maxMultiplier*10f),_=>1};
                int value=c.Kind==ChallengeKind.Multiplier?Mathf.Max(ChallengeProgress(slot),add):ChallengeProgress(slot)+add;
                PlayerPrefs.SetInt(P+"Progress."+slot,Mathf.Min(c.Target,value));
            }
            PlayerPrefs.Save();
        }
        public static int ProjectedProgress(int slot,int distance,int orbits,int skips,int sync,int nearMiss,int collected,float maxMultiplier)
        {
            ChallengeDefinition c=Challenge(ActiveChallengeId(slot));int current=ChallengeProgress(slot);int runValue=c.Kind switch{
                ChallengeKind.Distance=>distance,ChallengeKind.Orbits=>orbits,ChallengeKind.Skips=>skips,
                ChallengeKind.Synchronizations=>sync,ChallengeKind.NearMisses=>nearMiss,ChallengeKind.Materials=>collected,
                ChallengeKind.Multiplier=>Mathf.RoundToInt(maxMultiplier*10f),_=>0};
            int value=c.Kind==ChallengeKind.Multiplier?Mathf.Max(current,runValue):current+runValue;
            return Mathf.Min(c.Target,value);
        }
        public static bool Claim(int slot)
        {
            ChallengeDefinition c=Challenge(ActiveChallengeId(slot)); if(ChallengeProgress(slot)<c.Target||ChallengeClaimed(slot))return false;
            AddMaterials(c.Reward);PlayerPrefs.SetInt(P+"Claimed."+slot,1);PlayerPrefs.Save();
            if(ChallengeClaimed(0)&&ChallengeClaimed(1)&&ChallengeClaimed(2))RollChallenges(); return true;
        }
        private static void EnsureChallenges(){if(PlayerPrefs.HasKey(P+"Challenge.0"))return;RollChallenges();}
        private static void RollChallenges(){int generation=PlayerPrefs.GetInt(P+"Generation",0)+1;var used=new HashSet<int>();var previous=new HashSet<int>();if(PlayerPrefs.HasKey(P+"Challenge.0"))for(int i=0;i<3;i++)previous.Add(PlayerPrefs.GetInt(P+"Challenge."+i,-1));for(int i=0;i<3;i++){int id=UnityEngine.Random.Range(0,100);while(used.Contains(id)||previous.Contains(id))id=(id+UnityEngine.Random.Range(1,17))%100;used.Add(id);PlayerPrefs.SetInt(P+"Challenge."+i,id);PlayerPrefs.SetInt(P+"Progress."+i,0);PlayerPrefs.SetInt(P+"Claimed."+i,0);}PlayerPrefs.SetInt(P+"Generation",generation);PlayerPrefs.Save();}
    }
}
