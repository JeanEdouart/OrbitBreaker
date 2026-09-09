using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitBreaker
{
    public sealed partial class OrbitHud
    {
        private GameObject dailyRewardButton, dailyRewardPanel, chestRewardRows;
        private Image chestArt, chestGlow, chestSeal, chestBadge;
        private Text chestTitle, chestTimer, chestHint, chestMaterials, chestCosmetic, chestConversion, chestActionLabel;
        private Button chestAction, chestClose;
        private GameObject chestOddsPanel, chestReel;
        private readonly Image[] reelImages = new Image[9];
        private readonly Text[] reelLabels = new Text[9];
        private readonly Image[] reelCards = new Image[9];

        private Sprite RewardSprite(DailyRewardResult reward)
        {
            if(reward.Cosmetic.HasValue)return CosmeticPreview(reward.Cosmetic.Value);
            if(reward.TotalGadgets>0)return RuntimeAssets.GetPowerUpIcon((PowerUpType)Array.FindIndex(reward.Gadgets,n=>n>0));
            return RuntimeAssets.MaterialCrystalSprite;
        }
        private string RewardLabel(DailyRewardResult reward)
        {
            if(reward.Cosmetic.HasValue)return reward.Cosmetic.Value.Name;
            if(reward.TotalGadgets>0)return reward.TotalGadgets+" GADGETS";
            return (reward.Materials+reward.CosmeticCompensation)+" MATÉRIAUX";
        }
#if UNITY_EDITOR
        private Button chestEditorReset;
#endif
        private RectTransform chestStage;
        private readonly Text[] chestStockLabels = new Text[5];
        private readonly Image[] chestStars = new Image[24];
        private Coroutine dailyRewardAnimation;
        private DailyRewardResult displayedChest;
        private bool chestOpening;
        private int chestSecond = -1;
        private Sprite chestSprite;
        private Sprite openChestSprite;
        private AudioSource chestSound;
        private AudioClip chestTick;
        private readonly AudioClip[] chestChimes = new AudioClip[5];

        private Text ChestText(Transform parent, string name, string content, int size, float x0, float y0, float x1, float y1, Color color)
        {
            Text label = CreateText(parent, name, content, size, TextAnchor.MiddleCenter, FontStyle.Bold);
            label.color = color; label.resizeTextForBestFit = true;
            label.resizeTextMinSize = Mathf.Max(12, size - 8); label.resizeTextMaxSize = size;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(label.rectTransform, new Vector2(x0,y0), new Vector2(x1,y1), Vector2.zero,Vector2.zero);
            return label;
        }

        private void CreateDailyRewardUi(Transform safe)
        {
            DailyRewards.RecoverPending();
            Texture2D texture = Resources.Load<Texture2D>("Art/daily-chest");
            chestSprite = texture != null ? Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), Vector2.one*.5f, texture.width) : RuntimeAssets.DailyChestIcon;
            Texture2D opened = Resources.Load<Texture2D>("Art/daily-chest-open");
            openChestSprite = opened != null ? Sprite.Create(opened,new Rect(0,0,opened.width,opened.height),Vector2.one*.5f,opened.width) : chestSprite;
            chestSound=gameObject.AddComponent<AudioSource>();chestSound.playOnAwake=false;
            chestTick=RuntimeAssets.CreateTone("Chest lock",520f,.045f,.15f);
            dailyRewardButton = CreateIconButton(safe,"Daily Reward Button",chestSprite,ToggleDailyReward);
            SetSquareRect(dailyRewardButton.GetComponent<RectTransform>(),new Vector2(.91f,.215f),92f);
            dailyRewardButton.transform.Find("Icon").GetComponent<Image>().color = Color.white;
            chestBadge=CreateImage(dailyRewardButton.transform,"Ready",new Color(.3f,1f,.6f)); chestBadge.sprite=RuntimeAssets.CircleSprite;
            chestBadge.raycastTarget=false; SetSquareRect(chestBadge.rectTransform,new Vector2(.84f,.84f),20f);
            dailyRewardPanel=CreateImage(safe,"Daily Reward Panel",new Color(.003f,.009f,.026f,.98f)).gameObject;
            SetRect(dailyRewardPanel.GetComponent<RectTransform>(),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            Image card=CreateImage(dailyRewardPanel.transform,"Reward Card",new Color(.013f,.037f,.073f)); ApplyRounded(card);
            SetRect(card.rectTransform,new Vector2(.045f,.035f),new Vector2(.955f,.965f),Vector2.zero,Vector2.zero);
            ChestText(card.transform,"Eyebrow","TRANSMISSION QUOTIDIENNE",28,.06f,.93f,.94f,.97f,new Color(.4f,.75f,.85f));
            chestTitle=ChestText(card.transform,"Title","TON COFFRE STELLAIRE",52,.06f,.865f,.94f,.93f,Color.white);
            chestTimer=ChestText(card.transform,"Timer","",36,.06f,.80f,.94f,.857f,new Color(.4f,1f,.75f));
            chestHint=ChestText(card.transform,"Hint","",30,.07f,.745f,.93f,.795f,new Color(.55f,.75f,.84f));
            chestStage=CreateImage(card.transform,"Chest Stage",Color.clear).rectTransform;
            chestStage.GetComponent<Image>().raycastTarget=false;
            SetRect(chestStage,new Vector2(.12f,.44f),new Vector2(.88f,.74f),Vector2.zero,Vector2.zero);
            chestGlow=CreateImage(chestStage,"Aura",new Color(.15f,.7f,1f,.25f)); chestGlow.sprite=CreateChestGlow(); chestGlow.preserveAspect=true;
            SetRect(chestGlow.rectTransform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            for(int i=0;i<chestStars.Length;i++)
            {
                Image star=CreateImage(chestStage,"Spark "+i,Color.clear); star.sprite=RuntimeAssets.CircleSprite; star.raycastTarget=false;
                SetSquareRect(star.rectTransform,Vector2.one*.5f,5f+i%3*3); chestStars[i]=star;
            }
            chestArt=CreateImage(chestStage,"Chest",Color.white); chestArt.sprite=chestSprite; chestArt.preserveAspect=true;
            SetRect(chestArt.rectTransform,new Vector2(.06f,.02f),new Vector2(.94f,.98f),Vector2.zero,Vector2.zero);
            Button artButton=chestArt.gameObject.AddComponent<Button>();artButton.transition=Selectable.Transition.None;artButton.onClick.AddListener(BeginDailyReward);
            chestSeal=CreateImage(chestStage,"Reveal Bloom",Color.clear); chestSeal.sprite=RuntimeAssets.CircleSprite; chestSeal.preserveAspect=true; chestSeal.raycastTarget=false;
            SetRect(chestSeal.rectTransform,new Vector2(.3f,.3f),new Vector2(.7f,.7f),Vector2.zero,Vector2.zero);
            chestMaterials=ChestText(card.transform,"Materials","",46,.06f,.385f,.94f,.44f,Color.white);
            chestCosmetic=ChestText(card.transform,"Cosmetic","",32,.06f,.335f,.94f,.383f,new Color(1f,.79f,.36f));
            chestRewardRows=CreateImage(card.transform,"Gadget Rewards",Color.clear).gameObject;
            SetRect(chestRewardRows.GetComponent<RectTransform>(),new Vector2(.07f,.235f),new Vector2(.93f,.33f),Vector2.zero,Vector2.zero);
            for(int i=0;i<5;i++)
            {
                Image row=CreateImage(chestRewardRows.transform,"Gadget "+i,new Color(.035f,.09f,.145f));ApplyRounded(row);
                SetRect(row.rectTransform,new Vector2(i*.2f+.01f,0),new Vector2(i*.2f+.19f,1),Vector2.zero,Vector2.zero);
                Image icon=CreateImage(row.transform,"Icon",PowerUpProgression.Definition((PowerUpType)i).Color);icon.sprite=RuntimeAssets.GetPowerUpIcon((PowerUpType)i);icon.preserveAspect=true;
                SetRect(icon.rectTransform,new Vector2(.24f,.4f),new Vector2(.76f,.92f),Vector2.zero,Vector2.zero);
                chestStockLabels[i]=ChestText(row.transform,"Count","",34,.03f,.03f,.97f,.39f,Color.white);
            }
            chestConversion=ChestText(card.transform,"Conversion","",26,.06f,.187f,.94f,.232f,new Color(.55f,.76f,.84f));
            chestAction=CreateButton(card.transform,"Open Daily Chest","OUVRIR",new Color(.035f,.43f,.52f),BeginDailyReward).GetComponent<Button>();
            SetRect(chestAction.GetComponent<RectTransform>(),new Vector2(.12f,.116f),new Vector2(.88f,.18f),Vector2.zero,Vector2.zero);
            chestActionLabel=chestAction.GetComponentInChildren<Text>(); chestActionLabel.resizeTextForBestFit=true;chestActionLabel.resizeTextMinSize=24;chestActionLabel.resizeTextMaxSize=36;chestActionLabel.verticalOverflow=VerticalWrapMode.Truncate;
            string[] odds={"60 %","24 %","11 %","4,2 %","0,8 %"};
            for(int i=0;i<5;i++) ChestText(card.transform,"Rarity "+i,DailyRewards.RarityName((DailyChestRarity)i)+"\n"+odds[i],28,.06f+i*.176f,.061f,.225f+i*.176f,.108f,DailyRewards.RarityColor((DailyChestRarity)i));
            chestClose=CreateButton(card.transform,"Close Daily Reward","FERMER",new Color(.035f,.09f,.14f),ToggleDailyReward).GetComponent<Button>();
            SetRect(chestClose.GetComponent<RectTransform>(),new Vector2(.3f,.012f),new Vector2(.7f,.055f),Vector2.zero,Vector2.zero);
            chestClose.GetComponentInChildren<Text>().fontSize=32;
            var probabilities=CreateButton(card.transform,"Reward probabilities","CONTENU ET PROBABILITÉS",new Color(.08f,.15f,.23f),()=>{if(!chestOpening){chestOddsPanel.SetActive(true);card.gameObject.SetActive(false);}});
            SetRect(probabilities.GetComponent<RectTransform>(),new Vector2(.12f,.695f),new Vector2(.88f,.74f),Vector2.zero,Vector2.zero);
            SetRect(chestStage,new Vector2(.12f,.44f),new Vector2(.88f,.69f),Vector2.zero,Vector2.zero);
            probabilities.GetComponentInChildren<Text>().fontSize=28;
            chestOddsPanel=CreateImage(dailyRewardPanel.transform,"Probabilities",new Color(.015f,.025f,.07f,.99f)).gameObject;
            SetRect(chestOddsPanel.GetComponent<RectTransform>(),new Vector2(.04f,.04f),new Vector2(.96f,.96f),Vector2.zero,Vector2.zero);
            ChestText(chestOddsPanel.transform,"Title","UNE OUVERTURE = UN LOT",40,.06f,.88f,.94f,.96f,Color.cyan);
            ChestText(chestOddsPanel.transform,"Categories","MATÉRIAUX 50 %   ·   GADGETS 30 %\nCOSMÉTIQUE 20 %",32,.06f,.77f,.94f,.87f,Color.white);
            string[] ranges={"1–500 MAT  /  1–3 gadgets  /  skin 1–500 MAT","500–1000 MAT  /  3–5 gadgets  /  skin 501–1000 MAT","1000–1500 MAT  /  5–10 gadgets  /  skin 1001–1500 MAT","1500–3000 MAT  /  10–25 gadgets  /  skin 1501–3000 MAT","3000–5000 MAT  /  plein (25)  /  skin ≥ 3000 MAT"};
            for(int i=0;i<5;i++)ChestText(chestOddsPanel.transform,"Tier "+i,DailyRewards.RarityName((DailyChestRarity)i)+" · "+odds[i]+"\n"+ranges[i],28,.05f,.66f-i*.105f,.95f,.76f-i*.105f,DailyRewards.RarityColor((DailyChestRarity)i));
            ChestText(chestOddsPanel.transform,"Rules","La rareté est tirée indépendamment de la catégorie.\nQuantités équiprobables dans chaque tranche.\nCosmétiques non possédés : même chance par objet.\nExclusivités quotidiennes exclues.\nTranche déjà possédée : compensation au plafond\n(Rainbow : 3000 MAT). Gadget en surplus : 25 MAT.",24,.06f,.09f,.94f,.235f,Color.white);
            var oddsClose=CreateButton(chestOddsPanel.transform,"Close probabilities","RETOUR",new Color(.04f,.3f,.4f),()=>{chestOddsPanel.SetActive(false);card.gameObject.SetActive(true);});
            SetRect(oddsClose.GetComponent<RectTransform>(),new Vector2(.2f,.02f),new Vector2(.8f,.08f),Vector2.zero,Vector2.zero);
            chestOddsPanel.SetActive(false);
            chestReel=CreateImage(chestStage,"Reward Reel",new Color(.015f,.015f,.055f)).gameObject;
            SetRect(chestReel.GetComponent<RectTransform>(),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            chestReel.AddComponent<RectMask2D>();
            for(int i=0;i<9;i++)
            {
                reelCards[i]=CreateImage(chestReel.transform,"Reward "+i,Color.white);ApplyRounded(reelCards[i]);
                reelImages[i]=CreateImage(reelCards[i].transform,"Preview",Color.white);reelImages[i].preserveAspect=true;
                SetRect(reelImages[i].rectTransform,new Vector2(.12f,.25f),new Vector2(.88f,.9f),Vector2.zero,Vector2.zero);
                reelLabels[i]=ChestText(reelCards[i].transform,"Name","",26,.04f,.02f,.96f,.24f,Color.white);
            }
            var marker=CreateImage(chestReel.transform,"Selection marker",Color.cyan);marker.raycastTarget=false;
            SetRect(marker.rectTransform,new Vector2(.497f,0),new Vector2(.503f,1),Vector2.zero,Vector2.zero);
            chestReel.SetActive(false);
#if UNITY_EDITOR
            SetRect(chestClose.GetComponent<RectTransform>(),new Vector2(.07f,.012f),new Vector2(.38f,.055f),Vector2.zero,Vector2.zero);
            chestEditorReset=CreateButton(card.transform,"Reset Daily Chest (Editor)","RESET COFFRE (ÉDITEUR)",new Color(.12f,.16f,.22f),ResetDailyChestForEditor).GetComponent<Button>();
            SetRect(chestEditorReset.GetComponent<RectTransform>(),new Vector2(.42f,.012f),new Vector2(.94f,.055f),Vector2.zero,Vector2.zero);
            var resetLabel=chestEditorReset.GetComponentInChildren<Text>();
            resetLabel.resizeTextForBestFit=true;
            resetLabel.resizeTextMinSize=18;
            resetLabel.resizeTextMaxSize=26;
            resetLabel.verticalOverflow=VerticalWrapMode.Truncate;
#endif
            dailyRewardPanel.SetActive(false);
        }

#if UNITY_EDITOR
        private void ResetDailyChestForEditor()
        {
            if(chestOpening)return;
            DailyRewards.ResetCooldownForEditor();
            displayedChest=null;
            chestSecond=-1;
            RefreshChest();
        }
#endif

        private void UpdateDailyRewardUi(bool menuVisible)
        {
#if UNITY_EDITOR
            if(chestEditorReset!=null)chestEditorReset.interactable=!chestOpening;
#endif
            if(dailyRewardButton==null)return;
            dailyRewardButton.SetActive(menuVisible&&!SettingsOpen);
            int second=(int)Time.unscaledTime;
            if(second!=chestSecond)
            {
                chestSecond=second;
                bool ready=DailyRewards.IsReady(DateTime.UtcNow); chestBadge.gameObject.SetActive(ready);
                if(dailyRewardPanel.activeSelf&&!chestOpening)RefreshChest();
            }
            if(dailyRewardPanel.activeSelf&&!chestOpening)
            {
                chestArt.rectTransform.anchoredPosition=new Vector2(0,Mathf.Sin(Time.unscaledTime*1.5f)*5f);
                if(displayedChest!=null&&displayedChest.Rarity==DailyChestRarity.Rainbow)
                {Color c=Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime*.1f,1f),.6f,1f);chestTitle.color=c;c.a=.2f;chestGlow.color=c;}
            }
        }

        private void ToggleDailyReward()
        {
            if(chestOpening)return;
            bool opening=!dailyRewardPanel.activeSelf;
            if(opening)
            {
                hudFeedback.StopMusicPreview();
                settingsPanel.SetActive(false);creditsPanel.SetActive(false);missionsPanel.SetActive(false);hangarPanel.SetActive(false);
                leaderboardPanel.SetActive(false);powerUpPanel.SetActive(false);statisticsPanel.SetActive(false);modePanel.SetActive(false);journalPanel.SetActive(false);
                displayedChest=DailyRewards.IsReady(DateTime.UtcNow)?null:DailyRewards.LastResult();
                dailyRewardPanel.transform.SetAsLastSibling(); RefreshChest();
            }
            dailyRewardPanel.SetActive(opening);gameOverPanel.SetActive(!opening&&gameOverVisible);
        }

        private void RefreshChest()
        {
            TimeSpan left=DailyRewards.Remaining(DateTime.UtcNow);bool ready=left==TimeSpan.Zero;
            if(ready)displayedChest=null;
            chestTimer.text=ready?"COFFRE PRÊT À ÊTRE OUVERT":"PROCHAIN COFFRE  "+FormatCooldown(left);
            chestAction.interactable=ready;chestArt.GetComponent<Button>().interactable=ready;
            chestActionLabel.text=ready?"OUVRIR MON COFFRE":"RÉCOMPENSE RÉCUPÉRÉE";
            if(displayedChest!=null){ShowChestResult(displayedChest);return;}
            chestTitle.text="TON COFFRE STELLAIRE";chestTitle.color=Color.white;
            chestArt.sprite=chestSprite;
            chestHint.text=ready?"Une surprise gratuite, toutes les 24 heures.":"Ton prochain coffre est en préparation.";
            chestMaterials.text=ready?"QUE CONTIENT-IL ?":"À BIENTÔT, PILOTE";
            chestCosmetic.text="MATÉRIAUX 50 % · GADGETS 30 % · SKIN 20 %";
            chestRewardRows.SetActive(false);chestConversion.text="Les exclusivités du parcours du jour restent exclusives.";
            chestArt.color=ready?Color.white:new Color(.6f,.7f,.8f);
        }

        private void BeginDailyReward()
        {
            if(chestOpening||!dailyRewardPanel.activeSelf)return;
            DailyRewardResult result=DailyRewards.TryClaim(DateTime.UtcNow);
            if(result==null){RefreshChest();return;}
            chestOpening=true;chestClose.interactable=false;chestAction.interactable=false;chestArt.GetComponent<Button>().interactable=false;
            dailyRewardAnimation=StartCoroutine(PlayDailyReward(result));
        }

        private IEnumerator PlayDailyReward(DailyRewardResult result)
        {
            chestReel.SetActive(true);
            var previewRandom=new System.Random();
            for(int i=0;i<9;i++)
            {
                var preview=i==7?result:DailyRewards.Generate(previewRandom);
                reelImages[i].sprite=RewardSprite(preview);reelLabels[i].text=RewardLabel(preview);
                Color tint=DailyRewards.RarityColor(preview.Rarity);reelCards[i].color=new Color(tint.r*.25f,tint.g*.25f,tint.b*.25f,1);
            }
            chestRewardRows.SetActive(false);chestCosmetic.text="";chestConversion.text="";chestTimer.text="DÉVERROUILLAGE";
            chestHint.text="Une seule récompense sera sélectionnée";chestMaterials.text="";chestActionLabel.text="OUVERTURE EN COURS";
            Color color=DailyRewards.RarityColor(result.Rarity);float motion=GamePreferences.EnhancedEffects?1f:.2f;
            int beat=-1;
            for(float time=0;time<3.2f;time+=Time.unscaledDeltaTime)
            {
                float t=time/3.2f;int next=Mathf.FloorToInt(t*13);
                float travel=7f*(1-Mathf.Pow(1-t,3));
                for(int i=0;i<9;i++)SetRect(reelCards[i].rectTransform,new Vector2(.5f+(i-travel)*.65f-.3f,.08f),new Vector2(.5f+(i-travel)*.65f+.3f,.92f),Vector2.zero,Vector2.zero);
                if(next!=beat){beat=next;chestSound.pitch=.8f+t;chestSound.volume=hudFeedback.EffectsVolume;chestSound.PlayOneShot(chestTick);}
                chestArt.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(time*24)*t*4*motion);
                chestArt.rectTransform.localScale=Vector3.one*(1f+t*.12f);
                chestGlow.color=new Color(.2f,.85f,1f,.12f+t*.18f);
                for(int i=0;i<chestStars.Length;i++)
                {
                    float angle=i*Mathf.PI*2/chestStars.Length+time;float radius=(1-t)*chestStage.rect.width*.44f;
                    chestStars[i].rectTransform.anchoredPosition=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                    chestStars[i].color=new Color(.4f,.9f,1f,t*.8f);
                }
                yield return null;
            }
            chestReel.SetActive(false);
            displayedChest=result;PlayChestChime(result.Rarity);ShowChestResult(result);
            chestTimer.text="RÉCOMPENSES AJOUTÉES À TA COLLECTION";chestActionLabel.text="C'EST À TOI !";
            for(float time=0;time<1.4f;time+=Time.unscaledDeltaTime)
            {
                float t=time/1.4f;chestArt.rectTransform.localRotation=Quaternion.identity;chestArt.rectTransform.localScale=Vector3.one*(1f+.14f*(1-t));
                Color glow=color;glow.a=.2f;chestGlow.color=glow;
                Color bloom=color;bloom.a=(1-t)*.4f*motion;chestSeal.color=bloom;chestSeal.rectTransform.localScale=Vector3.one*(1+t*3);
                for(int i=0;i<chestStars.Length;i++)
                {
                    float angle=i*Mathf.PI*2/chestStars.Length;float radius=t*chestStage.rect.width*.5f;
                    chestStars[i].rectTransform.anchoredPosition=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                    Color spark=result.Rarity==DailyChestRarity.Rainbow?Color.HSVToRGB(i/24f,.6f,1):color;spark.a=1-t;chestStars[i].color=spark;
                }
                yield return null;
            }
            chestSeal.color=Color.clear;
            foreach(var star in chestStars)star.color=Color.clear;
            chestOpening=false;dailyRewardAnimation=null;chestClose.interactable=true;RefreshChest();RefreshMetaPanels();
        }

        private void ShowChestResult(DailyRewardResult result)
        {
            chestTitle.text="COFFRE "+DailyRewards.RarityName(result.Rarity);chestTitle.color=DailyRewards.RarityColor(result.Rarity);
            chestHint.text="Ta dernière récompense · conservée dans ta collection";
            chestMaterials.text="+"+(result.Materials+result.OverflowMaterials+result.CosmeticCompensation)+" MATÉRIAUX";
            chestCosmetic.text=result.Cosmetic.HasValue?"DÉBLOQUÉ : "+result.Cosmetic.Value.Name:result.CosmeticCompensation>0?"Collection possédée : compensation en matériaux":"BONUS AJOUTÉS À TON INVENTAIRE";
            chestRewardRows.SetActive(result.TotalGadgets>0);
            if(result.Kind!=DailyRewardKind.Legacy)
            {
                chestMaterials.text=RewardLabel(result);
                chestCosmetic.text=result.Cosmetic.HasValue?"COSMÉTIQUE DÉBLOQUÉ":result.CosmeticCompensation>0?"Collection complète : lot converti en matériaux":result.TotalGadgets>0?"LOT DE GADGETS":"MATÉRIAUX AJOUTÉS";
            }
            for(int i=0;i<5;i++)chestStockLabels[i].text="+"+result.AddedGadgets[i];
            chestConversion.text=result.OverflowMaterials>0?"Stock plein : "+result.OverflowMaterials+" matériaux de compensation":"Chaque bonus peut être conservé jusqu'à 5/5.";
            chestArt.color=Color.white;
            chestArt.sprite=result.Kind==DailyRewardKind.Legacy?openChestSprite:RewardSprite(result);
            Color glow=DailyRewards.RarityColor(result.Rarity);glow.a=.35f;chestGlow.color=glow;
        }

        private static Sprite CreateChestGlow()
        {
            const int size=128;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {float d=Vector2.Distance(new Vector2(x,y),Vector2.one*(size-1)*.5f)/(size*.5f);float a=Mathf.Pow(Mathf.Clamp01(1f-d),2);pixels[y*size+x]=new Color(1,1,1,a);}
            texture.SetPixels(pixels);texture.Apply(false,true);return Sprite.Create(texture,new Rect(0,0,size,size),Vector2.one*.5f,size);
        }

        private void PlayChestChime(DailyChestRarity rarity)
        {
            int tier=(int)rarity;
            if(chestChimes[tier]==null)
            {
                const int rate=22050;float duration=1.1f+tier*.24f;var samples=new float[Mathf.CeilToInt(rate*duration)];
                int[] notes={0,4,7,12,16,19,24};int count=3+tier;
                for(int s=0;s<samples.Length;s++)
                {
                    float time=s/(float)rate;float value=0;
                    for(int n=0;n<count;n++)
                    {float age=time-n*.09f;if(age<0)continue;float frequency=330f*Mathf.Pow(2f,notes[n]/12f);float phase=age*frequency*Mathf.PI*2;
                        value+=(Mathf.Sin(phase)+.18f*Mathf.Sign(Mathf.Sin(phase*2)))*Mathf.Exp(-age*4)*Mathf.Min(1,age*120)*.13f;}
                    samples[s]=Mathf.Clamp(value,-.8f,.8f)*Mathf.Clamp01((duration-time)*12);
                }
                chestChimes[tier]=AudioClip.Create("Chest "+rarity,samples.Length,1,rate,false);chestChimes[tier].SetData(samples,0);
            }
            chestSound.Stop();chestSound.pitch=1;chestSound.volume=hudFeedback.EffectsVolume;chestSound.PlayOneShot(chestChimes[tier]);
            hudFeedback.DailyRewardHaptic(rarity);
        }

        private void OnDestroy()
        {
            if(chestSprite!=null&&chestSprite!=RuntimeAssets.DailyChestIcon)Destroy(chestSprite);
            if(openChestSprite!=null&&openChestSprite!=chestSprite)Destroy(openChestSprite);
            if(chestGlow!=null&&chestGlow.sprite!=null){Destroy(chestGlow.sprite.texture);Destroy(chestGlow.sprite);}
            if(chestTick!=null)Destroy(chestTick);
            foreach(var clip in chestChimes)if(clip!=null)Destroy(clip);
        }

        private static string FormatCooldown(TimeSpan value)
        { long seconds=(long)Math.Ceiling(value.TotalSeconds);return (seconds/3600).ToString("00")+":"+(seconds/60%60).ToString("00")+":"+(seconds%60).ToString("00"); }
    }
}
