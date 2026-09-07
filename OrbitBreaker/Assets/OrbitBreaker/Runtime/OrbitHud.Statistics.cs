using System;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitBreaker
{
    public sealed partial class OrbitHud
    {
        private GameObject statisticsButton, statisticsPanel;
        private Text statisticsPerformance, statisticsCareer, statisticsExploration, statisticsDailyBadge;

        private void CreateStatisticsUi(Transform safe)
        {
            statisticsButton = CreateIconButton(safe, "Statistics Button", RuntimeAssets.StatisticsIcon, ToggleStatistics);
            SetSquareRect(statisticsButton.GetComponent<RectTransform>(), new Vector2(0.91f, 0.215f), 92f);

            statisticsPanel = new GameObject("Statistics Panel", typeof(RectTransform), typeof(Image));
            statisticsPanel.transform.SetParent(safe, false);
            SetRect(statisticsPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            statisticsPanel.GetComponent<Image>().color = new Color(0.005f, 0.015f, 0.045f, 0.97f);

            Image card = CreateImage(statisticsPanel.transform, "Statistics Card", new Color(0.025f, 0.075f, 0.14f, 0.99f));
            ApplyRounded(card);
            SetRect(card.rectTransform, new Vector2(0.025f, 0.025f), new Vector2(0.975f, 0.975f), Vector2.zero, Vector2.zero);

            Image iconPlate = CreateImage(card.transform, "Icon Plate", new Color(0.06f, 0.27f, 0.36f));
            ApplyRounded(iconPlate);
            SetRect(iconPlate.rectTransform, new Vector2(0.07f, 0.865f), new Vector2(0.2f, 0.94f), Vector2.zero, Vector2.zero);
            Image icon = CreateImage(iconPlate.transform, "Icon", Color.white);
            icon.sprite = RuntimeAssets.StatisticsIcon; icon.preserveAspect = true; icon.raycastTarget = false;
            SetRect(icon.rectTransform, new Vector2(0.2f, 0.16f), new Vector2(0.8f, 0.84f), Vector2.zero, Vector2.zero);

            Text title = CreateText(card.transform, "Title", "STATISTIQUES", 40, TextAnchor.MiddleLeft, FontStyle.Bold);
            title.color = new Color(0.76f, 0.98f, 1f);
            SetRect(title.rectTransform, new Vector2(0.23f, 0.86f), new Vector2(0.93f, 0.95f), Vector2.zero, Vector2.zero);

            statisticsDailyBadge = CreateText(card.transform, "Daily Badge", string.Empty, 16, TextAnchor.MiddleCenter, FontStyle.Bold);
            statisticsDailyBadge.color = new Color(1f, 0.75f, 0.24f);
            SetRect(statisticsDailyBadge.rectTransform, new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.855f), Vector2.zero, Vector2.zero);

            statisticsPerformance = CreateStatisticsCard(card.transform, "Performance", "PERFORMANCE", 0.57f, 0.79f);
            statisticsCareer = CreateStatisticsCard(card.transform, "Career", "CARRIÈRE", 0.335f, 0.555f);
            statisticsExploration = CreateStatisticsCard(card.transform, "Exploration", "EXPLORATION", 0.10f, 0.32f);

            GameObject close = CreateButton(card.transform, "Close", "FERMER", new Color(0.06f, 0.26f, 0.36f), ToggleStatistics);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.3f, 0.025f), new Vector2(0.7f, 0.085f), Vector2.zero, Vector2.zero);
            statisticsPanel.SetActive(false);
        }

        private Text CreateStatisticsCard(Transform parent, string name, string heading, float bottom, float top)
        {
            Image plate = CreateImage(parent, name + " Plate", new Color(0.015f, 0.055f, 0.105f, 0.98f));
            ApplyRounded(plate);
            SetRect(plate.rectTransform, new Vector2(0.07f, bottom), new Vector2(0.93f, top), Vector2.zero, Vector2.zero);
            Text label = CreateText(plate.transform, "Heading", heading, 17, TextAnchor.MiddleLeft, FontStyle.Bold);
            label.color = new Color(0.25f, 0.92f, 1f);
            SetRect(label.rectTransform, new Vector2(0.05f, 0.76f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero);
            Text value = CreateText(plate.transform, "Values", string.Empty, 18, TextAnchor.UpperLeft, FontStyle.Bold);
            value.color = new Color(0.82f, 0.94f, 1f); value.lineSpacing = 1.15f;
            value.resizeTextForBestFit = true; value.resizeTextMinSize = 12; value.resizeTextMaxSize = 18;
            SetRect(value.rectTransform, new Vector2(0.05f, 0.04f), new Vector2(0.95f, 0.77f), Vector2.zero, Vector2.zero);
            return value;
        }

        private void ToggleStatistics()
        {
            bool opening = !statisticsPanel.activeSelf;
            settingsPanel.SetActive(false); creditsPanel.SetActive(false); missionsPanel.SetActive(false);
            hangarPanel.SetActive(false); powerUpPanel.SetActive(false); leaderboardPanel.SetActive(false); journalPanel.SetActive(false);
            statisticsPanel.SetActive(opening);
            if (opening) RefreshStatistics();
            gameOverPanel.SetActive(!opening && gameOverVisible);
        }

        private void RefreshStatistics()
        {
            int sprintBest = LocalRunStats.Best(RunMode.Sprint, 0);
            string average = TimeSpan.FromSeconds(LocalRunStats.AverageDuration).ToString(@"mm\:ss");
            statisticsPerformance.text = "RECORD INFINI        " + PlayerPrefs.GetInt("OrbitBreaker.BestScore", 0) + " UA\n"
                + "RECORD SPRINT        " + sprintBest + " UA\n"
                + "MEILLEUR SKIP        " + LocalRunStats.BestSkip + " ORBITES\n"
                + "MEILLEURE SÉRIE      x" + LocalRunStats.BestChain;
            statisticsCareer.text = "PARTIES INFINIES     " + LocalRunStats.CompletedRuns + "\n"
                + "TEMPS MOYEN          " + average + "\n"
                + "DISTANCE CUMULÉE     " + GameProgression.LifetimeDistance + " UA\n"
                + "MATÉRIAUX ACTUELS    " + MetaProgression.Materials + "\n"
                + "BONUS EN STOCK       " + PowerUpProgression.TotalStored();
            statisticsExploration.text = "MONDES DÉCOUVERTS    " + PlanetJournal.TotalDiscovered() + " / " + PlanetJournal.TotalPlanets() + "\n"
                + "ESSAIS QUOTIDIENS    " + DailyCourse.AttemptedDays + "\n"
                + "PARCOURS TERMINÉS    " + DailyCourse.CompletedDays + "\n"
                + "EXPLOSIONS           " + LocalRunStats.Deaths(DeathReason.Breaker) + "\n"
                + "PERDUS DANS L'ESPACE " + LocalRunStats.Deaths(DeathReason.LostInSpace);

            DailyCourseDefinition today = DailyCourse.ForDate(DateTime.UtcNow);
            statisticsDailyBadge.text = DailyCourse.IsClaimed(today.DayKey) ? "AUJOURD'HUI · PARCOURS TERMINÉ"
                : DailyCourse.IsAttempted(today.DayKey) ? "AUJOURD'HUI · ESSAI EFFECTUÉ"
                : "AUJOURD'HUI · ESSAI DISPONIBLE";
        }
    }
}
