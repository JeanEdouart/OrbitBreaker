using UnityEngine;
using UnityEngine.UI;

namespace OrbitBreaker
{
    public sealed partial class OrbitHud
    {
        private GameObject modeButton, modePanel, journalButton, journalPanel;
        private Text modeLabel, runModeLabel, journalText, journalTitle, journalStats;
        private Text modeSelectionTitle, modeSelectionDescription, modeSelectionDetails, modeConfirmLabel;
        private readonly Image[] modeCardImages = new Image[3];
        private RunMode pendingRunMode;
        private Image journalPlanet;
        private int journalPackIndex, journalVariant;
        private GameBootstrap game;
        private float leaderboardViewportHeight;
        private int lastDistance, lastRecord;
        private Text tipsContent;
        private int shownTip = -1;
        private static readonly string[] Tips = {
            "VISE LA PORTE COLORÉE DANS SON SENS POUR UNE SYNCHRO\nELLE EST OPTIONNELLE : TOUTE L'ORBITE PEUT TE CAPTURER",
            "ENCHAÎNE LES SKIPS POUR AUGMENTER TON BONUS DE DISTANCE\nUNE CAPTURE NORMALE INTERROMPT LA SÉRIE",
            "APRÈS UNE CAPTURE, LE BOUCLIER TE PROTÈGE BRIÈVEMENT\nSON CLIGNOTEMENT ANNONCE LA FIN DE L'IMMUNITÉ",
            "REVENIR EN ARRIÈRE RESTAURE LE SCORE DE L'ORBITE\nLA DIFFICULTÉ GARDE LE NIVEAU MAXIMAL DÉJÀ ATTEINT",
            "UN FRÔLEMENT DE DÉBRIS LIBRE AUGMENTE TON MULTIPLICATEUR\nLE RISQUE RESTE OPTIONNEL : TU PEUX CHOISIR UN AUTRE VOL",
            "TES BONUS SONT CONSERVÉS ENTRE LES PARTIES : 5 PAR TYPE\nLES MODES LOCAUX N'UTILISENT PAS TON STOCK PERSISTANT"
        };

        private void CreateExtraMenus(Transform safe)
        {
            modeButton = CreateButton(safe, "Run Mode", "MODE : INFINI", new Color(0.03f,0.16f,0.24f), ToggleModePanel);
            SetRect(modeButton.GetComponent<RectTransform>(), new Vector2(0.27f,0.27f), new Vector2(0.73f,0.315f), Vector2.zero,Vector2.zero);
            modeLabel = modeButton.transform.Find("Label").GetComponent<Text>(); modeLabel.fontSize = 20;
            modeLabel.resizeTextForBestFit = true; modeLabel.resizeTextMinSize = 13; modeLabel.resizeTextMaxSize = 20;
            journalButton = CreateIconButton(safe,"Planet Journal",RuntimeAssets.PlanetIcon,ToggleJournal);
            SetSquareRect(journalButton.GetComponent<RectTransform>(),new Vector2(0.07f,0.215f),92f);
            CreateStatisticsUi(safe);
            runModeLabel = CreateText(safe,"Run Mode Status",string.Empty,18,TextAnchor.MiddleCenter,FontStyle.Bold);
            SetRect(runModeLabel.rectTransform,new Vector2(0.18f,0.885f),new Vector2(0.8f,0.925f),Vector2.zero,Vector2.zero);
            journalPanel = new GameObject("Journal Panel",typeof(RectTransform),typeof(Image));
            journalPanel.transform.SetParent(safe,false);
            SetRect(journalPanel.GetComponent<RectTransform>(),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            journalPanel.GetComponent<Image>().color=new Color(0.005f,0.02f,0.045f,0.99f);
            journalTitle=CreateText(journalPanel.transform,"Title","CARTES STELLAIRES",32,TextAnchor.MiddleCenter,FontStyle.Bold);
            SetRect(journalTitle.rectTransform,new Vector2(.08f,.85f),new Vector2(.92f,.95f),Vector2.zero,Vector2.zero);
            journalStats=CreateText(journalPanel.transform,"Statistics",string.Empty,21,TextAnchor.MiddleCenter,FontStyle.Normal);
            SetRect(journalStats.rectTransform,new Vector2(.08f,.63f),new Vector2(.92f,.85f),Vector2.zero,Vector2.zero);
            journalPlanet=CreateImage(journalPanel.transform,"Planet",Color.white);journalPlanet.preserveAspect=true;
            SetRect(journalPlanet.rectTransform,new Vector2(.28f,.35f),new Vector2(.72f,.60f),Vector2.zero,Vector2.zero);
            journalText=CreateText(journalPanel.transform,"Planet Description",string.Empty,22,TextAnchor.MiddleCenter,FontStyle.Bold);
            SetRect(journalText.rectTransform,new Vector2(.1f,.19f),new Vector2(.9f,.35f),Vector2.zero,Vector2.zero);
            var prev=CreateButton(journalPanel.transform,"Previous Planet","PRÉCÉDENTE",new Color(.04f,.25f,.32f),()=>MoveJournal(-1));
            var next=CreateButton(journalPanel.transform,"Next Planet","SUIVANTE",new Color(.04f,.25f,.32f),()=>MoveJournal(1));
            SetRect(prev.GetComponent<RectTransform>(),new Vector2(.06f,.1f),new Vector2(.46f,.17f),Vector2.zero,Vector2.zero);
            SetRect(next.GetComponent<RectTransform>(),new Vector2(.54f,.1f),new Vector2(.94f,.17f),Vector2.zero,Vector2.zero);
            var close=CreateButton(journalPanel.transform,"Close","FERMER",new Color(.04f,.2f,.3f),ToggleJournal);
            SetRect(close.GetComponent<RectTransform>(),new Vector2(.3f,.025f),new Vector2(.7f,.085f),Vector2.zero,Vector2.zero);
            journalPanel.SetActive(false);
            CreateModePanel(safe);
        }

        private void CreateModePanel(Transform safe)
        {
            modePanel = new GameObject("Mode Selection Panel", typeof(RectTransform), typeof(Image));
            modePanel.transform.SetParent(safe, false);
            SetRect(modePanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            modePanel.GetComponent<Image>().color = new Color(0.005f, 0.015f, 0.045f, 0.97f);
            Image card = CreateImage(modePanel.transform, "Mode Card", new Color(0.025f, 0.075f, 0.14f, 0.995f)); ApplyRounded(card);
            SetRect(card.rectTransform, new Vector2(.045f,.08f), new Vector2(.955f,.92f), Vector2.zero, Vector2.zero);
            Text heading=CreateText(card.transform,"Heading","CHOISIR UN MODE",39,TextAnchor.MiddleCenter,FontStyle.Bold);
            heading.color=new Color(.76f,.98f,1f);SetRect(heading.rectTransform,new Vector2(.06f,.88f),new Vector2(.94f,.97f),Vector2.zero,Vector2.zero);
            RunMode[] modes={RunMode.Endless,RunMode.Daily,RunMode.Sprint};
            for(int i=0;i<modes.Length;i++)
            {
                int index=i; GameObject option=CreateButton(card.transform,"Mode "+modes[i],ModeName(modes[i]),new Color(.035f,.16f,.24f),()=>SelectPendingMode(modes[index]));
                SetRect(option.GetComponent<RectTransform>(),new Vector2(.07f+i*.30f,.75f),new Vector2(.33f+i*.30f,.84f),Vector2.zero,Vector2.zero);
                modeCardImages[i]=option.GetComponent<Image>();
                Text label=option.transform.Find("Label").GetComponent<Text>();label.fontSize=18;label.resizeTextForBestFit=true;label.resizeTextMinSize=12;
            }
            modeSelectionTitle=CreateText(card.transform,"Selected Mode",string.Empty,34,TextAnchor.MiddleCenter,FontStyle.Bold);
            modeSelectionTitle.color=new Color(1f,.75f,.24f);SetRect(modeSelectionTitle.rectTransform,new Vector2(.08f,.62f),new Vector2(.92f,.72f),Vector2.zero,Vector2.zero);
            Image descriptionPlate=CreateImage(card.transform,"Description Plate",new Color(.015f,.055f,.105f,.98f));ApplyRounded(descriptionPlate);
            SetRect(descriptionPlate.rectTransform,new Vector2(.07f,.30f),new Vector2(.93f,.62f),Vector2.zero,Vector2.zero);
            modeSelectionDescription=CreateText(descriptionPlate.transform,"Description",string.Empty,23,TextAnchor.MiddleCenter,FontStyle.Bold);
            modeSelectionDescription.color=new Color(.78f,.94f,1f);modeSelectionDescription.resizeTextForBestFit=true;modeSelectionDescription.resizeTextMinSize=16;
            SetRect(modeSelectionDescription.rectTransform,new Vector2(.07f,.43f),new Vector2(.93f,.91f),Vector2.zero,Vector2.zero);
            modeSelectionDetails=CreateText(descriptionPlate.transform,"Details",string.Empty,18,TextAnchor.MiddleCenter,FontStyle.Normal);
            modeSelectionDetails.color=new Color(.48f,.77f,.9f);modeSelectionDetails.resizeTextForBestFit=true;modeSelectionDetails.resizeTextMinSize=13;
            SetRect(modeSelectionDetails.rectTransform,new Vector2(.07f,.08f),new Vector2(.93f,.44f),Vector2.zero,Vector2.zero);
            GameObject confirm=CreateButton(card.transform,"Confirm Mode","CHANGER LE MODE",new Color(.08f,.48f,.58f),ConfirmRunMode);
            modeConfirmLabel=confirm.transform.Find("Label").GetComponent<Text>();SetRect(confirm.GetComponent<RectTransform>(),new Vector2(.18f,.17f),new Vector2(.82f,.27f),Vector2.zero,Vector2.zero);
            GameObject close=CreateButton(card.transform,"Close Mode","FERMER",new Color(.04f,.2f,.3f),ToggleModePanel);
            SetRect(close.GetComponent<RectTransform>(),new Vector2(.3f,.055f),new Vector2(.7f,.135f),Vector2.zero,Vector2.zero);
            modePanel.SetActive(false);
        }

        private void ToggleModePanel()
        {
            if(game==null)game=FindFirstObjectByType<GameBootstrap>(); bool opening=!modePanel.activeSelf;
            settingsPanel.SetActive(false);creditsPanel.SetActive(false);missionsPanel.SetActive(false);hangarPanel.SetActive(false);powerUpPanel.SetActive(false);leaderboardPanel.SetActive(false);journalPanel.SetActive(false);statisticsPanel.SetActive(false);
            modePanel.SetActive(opening);gameOverPanel.SetActive(!opening&&gameOverVisible);
            if(opening)SelectPendingMode(game!=null?game.CurrentRunMode:RunMode.Endless);
        }

        private void SelectPendingMode(RunMode mode)
        {
            pendingRunMode=mode;
            int selectedIndex=mode==RunMode.Endless?0:mode==RunMode.Daily?1:2;
            for(int i=0;i<modeCardImages.Length;i++)modeCardImages[i].color=i==selectedIndex?new Color(.08f,.4f,.5f):new Color(.035f,.16f,.24f);
            modeSelectionTitle.text=ModeName(mode);
            modeSelectionDescription.text=mode switch{RunMode.Daily=>"UN PARCOURS UNIQUE À TERMINER EN UNE SEULE TENTATIVE.",RunMode.Sprint=>"90 SECONDES POUR PARCOURIR LA PLUS GRANDE DISTANCE.",_=>"VA LE PLUS LOIN POSSIBLE ET BATS TON RECORD."};
            if(mode==RunMode.Daily)
            {
                DailyCourseDefinition d=DailyCourse.ForDate(System.DateTime.UtcNow);
                modeSelectionDetails.text="NIVEAU "+d.Tier+" / 5  ·  "+d.RequiredCaptures+" ORBITES  ·  "+d.MaterialReward+" MAT\nLe niveau règle la longueur, la densité des débris et la récompense du parcours.";
            }
            else modeSelectionDetails.text=mode==RunMode.Sprint?"CLASSEMENT MONDIAL SÉPARÉ · BONUS TEMPORAIRES FOURNIS":"PROGRESSION, MATÉRIAUX ET BONUS PERSISTANTS";
            bool current=game!=null&&game.CurrentRunMode==mode;modeConfirmLabel.text=current?"MODE ACTUEL":"CHANGER LE MODE";
            modeConfirmLabel.transform.parent.GetComponent<Button>().interactable=!current;
        }

        private void ConfirmRunMode(){if(game==null)game=FindFirstObjectByType<GameBootstrap>();if(game!=null&&game.SetRunMode(pendingRunMode))ToggleModePanel();}
        private static string ModeName(RunMode mode) => mode switch { RunMode.Daily=>"PARCOURS DU JOUR",RunMode.Sprint=>"SPRINT 90 S",_=>"INFINI" };

        private void LateUpdate()
        {
            if(game==null)game=FindFirstObjectByType<GameBootstrap>();
            bool menu=(tutorialTips!=null&&tutorialTips.activeSelf)||gameOverVisible;
            int tip = (int)(Time.unscaledTime / 7f) % Tips.Length;
            if (tipsContent != null && tutorialTips.activeSelf && tip != shownTip)
            { shownTip = tip; tipsContent.text = Tips[tip]; }
            if(modeButton!=null)modeButton.SetActive(menu && !SettingsOpen);
            if(journalButton!=null)journalButton.SetActive(menu && !SettingsOpen);
            if(statisticsButton!=null)statisticsButton.SetActive(menu && !SettingsOpen);
            if(game!=null&&modeLabel!=null)
            {
                modeLabel.text="MODE : "+ModeName(game.CurrentRunMode);
                runModeLabel.enabled = !SettingsOpen;
                runModeLabel.text=game.CurrentRunMode==RunMode.Endless
                    ? (lastRecord>0&&lastDistance<lastRecord&&lastRecord-lastDistance<=50?"RECORD À "+(lastRecord-lastDistance)+" UA":string.Empty)
                    : game.CurrentRunMode==RunMode.Sprint&&!menu ? "SPRINT · "+Mathf.CeilToInt(game.SprintRemainingSeconds)+" S"
                    : game.CurrentRunMode==RunMode.Daily ? "PARCOURS · "+game.DailyCaptures+" / "+game.DailyTarget+" ORBITES · NIVEAU "+game.DailyTier
                    : ModeName(game.CurrentRunMode);
            }
            if(hangarPanel!=null&&hangarPanel.activeSelf&&hangarPreview!=null)
                hangarPreview.rectTransform.localEulerAngles=new Vector3(0,0,Mathf.Sin(Time.unscaledTime*1.4f)*4f);
            if(leaderboardPanel!=null&&leaderboardPanel.activeSelf&&leaderboardScroll!=null)
            {
                float height=leaderboardScroll.viewport.rect.height;
                if(Mathf.Abs(height-leaderboardViewportHeight)>1f)
                {leaderboardViewportHeight=height; RenderLeaderboardRows(onlineLeaderboard.Filter(leaderboardSearchInput.text));}
            }
        }

        private System.Collections.Generic.List<CosmeticDefinition> JournalPacks()
        { var packs=new System.Collections.Generic.List<CosmeticDefinition>();foreach(var item in MetaProgression.Catalog)if(item.Kind==CosmeticKind.PlanetPack)packs.Add(item);return packs; }
        private void ToggleJournal()
        {
            bool opening=!journalPanel.activeSelf; journalPanel.SetActive(opening);
            if(opening){hudFeedback.StopMusicPreview(); settingsPanel.SetActive(false);hangarPanel.SetActive(false);missionsPanel.SetActive(false);creditsPanel.SetActive(false);powerUpPanel.SetActive(false);leaderboardPanel.SetActive(false);statisticsPanel.SetActive(false);RefreshJournal();}
        }
        private void MoveJournal(int delta)
        {
            var packs=JournalPacks();journalVariant+=delta;
            if(journalVariant>=PlanetJournal.VariantCount(packs[journalPackIndex].VisualIndex)){journalVariant=0;journalPackIndex=(journalPackIndex+1)%packs.Count;}
            if(journalVariant<0){journalPackIndex=(journalPackIndex+packs.Count-1)%packs.Count;journalVariant=PlanetJournal.VariantCount(packs[journalPackIndex].VisualIndex)-1;}
            RefreshJournal();
        }
        private void RefreshJournal()
        {
            var pack=JournalPacks()[journalPackIndex];bool seen=PlanetJournal.Has(pack.VisualIndex,journalVariant);
            journalPlanet.sprite=RuntimeAssets.GetPlanetPackSprite(pack.VisualIndex,journalVariant);
            journalPlanet.color=seen?Color.white:new Color(.12f,.17f,.22f,1f);
            int packFound=PlanetJournal.PackDiscovered(pack.VisualIndex),totalFound=PlanetJournal.TotalDiscovered();
            journalText.text=pack.Name+" · MONDE "+(journalVariant+1)+" / "+PlanetJournal.VariantCount(pack.VisualIndex)+"\n"+(seen?PlanetJournal.Description(pack.VisualIndex):"SILHOUETTE INCONNUE · À DÉCOUVRIR EN MODE INFINI");
            journalStats.text="COLLECTION GLOBALE  "+totalFound+" / "+PlanetJournal.TotalPlanets()+"\n"+pack.Name+"  "+packFound+" / "+PlanetJournal.VariantCount(pack.VisualIndex)+"\nMEILLEUR SKIP "+LocalRunStats.BestSkip+" · SÉRIE "+LocalRunStats.BestChain;
        }
    }

    public static class PlanetJournal
    {
        private static readonly string[] Descriptions = {
            "Des mondes inconnus sur les routes de l'Orbiter.", "Nos voisines du Système solaire, réimaginées en miniature.",
            "Des terres fantastiques aux océans et ciels insolites.", "Des aurores colorent ces horizons lointains.",
            "Des jardins entiers s'accrochent à ces petits mondes.", "Des archipels dérivent dans un océan sans frontières.",
            "Engrenages et mécanismes rythment ces planètes-ateliers.", "Des mondes sucrés, à regarder sans les croquer.",
            "Des cristaux géants réfléchissent la lumière stellaire.", "Des villes minuscules éclairent la face nocturne.",
            "Printemps, été, automne et hiver se partagent l'espace.", "Des couches de papier composent ces reliefs fragiles.",
            "Sous la roche sombre, les volcans dorment encore.", "Des forêts de champignons brillent dans le silence.",
            "67, personnage de bois et poulet croustillant : l'espace a scrollé trop loin.",
            "Herbes stellaires, résine sombre, glace festive et rythmes reggae en orbite."
        };
        public static string Description(int pack) => Descriptions[Mathf.Clamp(pack,0,Descriptions.Length-1)];
        public static int VariantCount(int pack)=>pack==0?6:pack>=4?5:4;
        public static bool Has(int pack,int variant)=>PlayerPrefs.GetInt("OrbitBreaker.Discovery."+pack+"."+variant,0)==1;
        public static int PackDiscovered(int pack){int count=0;for(int i=0;i<VariantCount(pack);i++)if(Has(pack,i))count++;return count;}
        public static int TotalDiscovered(){int count=0;for(int pack=0;pack<16;pack++)count+=PackDiscovered(pack);return count;}
        public static int TotalPlanets(){int count=0;for(int pack=0;pack<16;pack++)count+=VariantCount(pack);return count;}
        public static void Record(int pack,int sequence)
        {int variant=Mathf.Abs(sequence%VariantCount(pack));if(!Has(pack,variant))PlayerPrefs.SetInt("OrbitBreaker.Discovery."+pack+"."+variant,1);}
    }
}
