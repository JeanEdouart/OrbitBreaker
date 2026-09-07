using UnityEngine;

namespace OrbitBreaker
{
    // Small fixed pool, world-space motifs: never affects capture or collision geometry.
    public sealed class CosmeticTrailEmitter : MonoBehaviour
    {
        const int Capacity = 28;
        readonly SpriteRenderer[] sprites = new SpriteRenderer[Capacity];
        readonly float[] age = new float[Capacity];
        readonly Vector3[] positions = new Vector3[Capacity];
        OrbitPlayer player;
        Transform pool;
        Sprite motif;
        Color accent;
        int style = -1, cursor;
        float emission;
        public void Configure(OrbitPlayer owner, int selected)
        {
            player = owner;
            if (selected == style) return;
            style = selected;
            Clear();
            if(style < 6) return;
            motif = RuntimeAssets.GetTrailSprite(style);
            accent = GameProgression.TrailColor(style);
            if(pool != null) return;
            pool = new GameObject("Cosmetic Trail Motifs").transform;
            for(int i=0;i<Capacity;i++)
            {
                var obj=new GameObject("Motif "+i);
                obj.transform.SetParent(pool,false);
                sprites[i]=obj.AddComponent<SpriteRenderer>();
                sprites[i].sharedMaterial=RuntimeAssets.SpriteMaterial;
                sprites[i].sortingOrder=9;
                sprites[i].enabled=false;
                age[i]=1f;
            }
        }
        public void Clear(){emission=0;for(int i=0;i<Capacity;i++){age[i]=1f;if(sprites[i]!=null)sprites[i].enabled=false;}}
        public void Tick(float dt)
        {
            if(pool==null||style<6||player==null)return;
            if(player.State==PlayerOrbitState.Dead){Clear();return;}
            emission+=dt;
            if(player.State==PlayerOrbitState.Flying&&emission>=(GamePreferences.EnhancedEffects ? .065f : .13f))
            {
                emission=0;
                int i=cursor++%Capacity;
                age[i]=0;
                positions[i]=transform.position-transform.up*.25f;
                sprites[i].sprite=motif;
                sprites[i].enabled=true;
                sprites[i].transform.rotation=Quaternion.Euler(0,0,(style==14||style==20)?0:cursor*43f);
            }
            else if(player.State!=PlayerOrbitState.Flying)emission=0;
            for(int i=0;i<Capacity;i++)
            {
                if(age[i]>=1)continue;
                age[i]=Mathf.Min(1,age[i]+dt/.65f);
                float t=age[i],spread=Mathf.Sin(i*2.3f+t*4)*.055f;
                sprites[i].transform.position=positions[i]+new Vector3(spread*t,0,0);
                float size=style==21 ? Mathf.Lerp(.48f,.18f,t) : style==20 ? .38f+t*.25f : style==12 ? .24f+t*.16f : Mathf.Lerp(.31f,.07f,t);
                sprites[i].transform.localScale=Vector3.one*size;
                Color color=accent;
                if(style==7||style==17||style==18)color=Color.Lerp(accent,Color.HSVToRGB((i*.17f)%1,.42f,1),.65f);
                color.a=(1-t)*.92f;sprites[i].color=color;
                if(t>=1)sprites[i].enabled=false;
            }
        }
        void OnDisable(){Clear();}
        void OnDestroy(){if(pool!=null)Destroy(pool.gameObject);}
    }
}
