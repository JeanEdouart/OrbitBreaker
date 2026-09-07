# Review du jeu — 7 septembre 2026

> Mise à jour : cette review initiale a été traitée dans la passe suivante. Voir [IMPLEMENTATION-REVIEW.md](IMPLEMENTATION-REVIEW.md) pour les corrections réellement intégrées, les validations et les limites. La sélection musicale se trouve désormais dans le **hangar**, pas dans Son. Aucun build ni commit n'a été réalisé pour cette passe.

Lecture statique du README et des systèmes score, génération, joueur, interface, économie, classement et audio. Les scénarios ci-dessous sont des risques déduits du code, pas tous des bugs reproduits sur téléphone. Les règles existantes n'ont pas été modifiées dans cette passe.

## Points prioritaires

1. **Envois simultanés du record** — `OnlineLeaderboard.SubmitBestScoreAsync` efface la clé PendingLeaderboardScore à la fin de chaque requête. Si un second score supérieur est mis en attente pendant la première, une réponse plus ancienne peut supprimer ce second score. Sérialiser les envois et n'effacer que la valeur réellement confirmée.
2. **Synchronisations sur checkpoints revisités** — `GameBootstrap.HandleCaptured` exclut les revisites des récompenses de skip, mais incrémente runSynchronizations sur toute capture synchronisée non arrière. Un retour puis une nouvelle capture avant sur une ancre connue peut donc faire progresser les défis. Décider explicitement si c'est voulu; sinon appliquer le même filtre que pour les skips.
3. **Sauvegarde d'une récompense de défi en deux étapes** — `MetaProgression.ClaimChallenge` appelle AddMaterials (qui sauvegarde), puis marque le défi récupéré et sauvegarde à nouveau. Une fermeture entre les deux peut permettre une seconde récupération. Écrire les deux valeurs avant une sauvegarde commune.
4. **Classement et confiance client** — le score envoyé provient du client et les données économiques sont dans PlayerPrefs. C'est suffisant pour un test entre amis; ce n'est pas une protection contre les scores ou matériaux modifiés. Une validation distante serait un chantier séparé.
5. **README partiellement périmé** — mentions de mètres, version Android 0.1.0, mission quotidienne, déblocage des traînées par distance et ancien catalogue coexistent avec les systèmes UA, matériaux et version 0.3.0. Le document doit distinguer l'état réel des idées futures.

## Optimisations sans changement de règles

- `MetaProgression.AddMaterials` sauvegarde à chaque cristal : regrouper les écritures avec sauvegarde en pause, fin de partie et sortie, en définissant la tolérance à une fermeture brutale. Cela réduit les accès disque mais nécessite de préserver la robustesse des achats.
- Le HUD cherche des composants et reformate des textes régulièrement (`UpdateActivePowerUps`, `UpdateFlightDisplay`). Mémoriser les composants et ne modifier les libellés qu'au changement de valeur; conserver les mises à jour nécessaires aux animations.
- La musique est synthétisée avec de nombreux Sin/Pow par échantillon. Exporter les pistes en assets audio au développement supprimerait ce calcul au lancement/changement; comparer ensuite taille, compression et mémoire sur Android et WebGL.
- La génération recycle déjà les objets : conserver cette approche, profiler les pics de génération avant d'ajouter des pools ou caches supplémentaires.
- Scinder `OrbitPresentation.cs` en fichiers caméra, HUD, réglages et audio. Cela rend le code plus court à lire, sans changer son comportement; supprimer uniquement les champs réellement inutilisés après recherche globale.
- Mesurer les allocations et le temps CPU sur appareil avant/après. Une compilation sans erreur ne valide ni la fluidité ni l'absence de chevauchement visuel.

## Cohérence et scénarios à tester

- Génération des débris libres : le code commence par une source N−2. Vérifier les autres sources réellement explorées avant d'annoncer une recherche exhaustive de tous les skips possibles.
- Vérifier le classement avec 0, 1, 10, 11 et 100 joueurs, recherche pendant chargement, fermeture/réouverture et rotation/redimensionnement de fenêtre. Sa géométrie est recalculée lors du rendu des résultats.
- Le chargement du classement vide le cache avant une requête. En cas de réseau absent, l'ancien classement disparaît. Conserver les derniers résultats avec une indication de fraîcheur améliorerait l'expérience.
- Tester les textes simultanés (score long, matériaux, skip, série, frôlement), surtout près des bords et avec les animations de taille.
- Les retours restaurent des checkpoints de score tandis que la difficulté conserve son maximum : cohérent avec le README récent, mais à expliquer dans les tips.

## Idées de nouveautés / modifications

1. Défi de parcours commun via une graine quotidienne, avec classement distinct.
2. Écran de fin montrant le meilleur enchaînement et une seule suggestion utile pour la prochaine partie.
3. Aperçu animé des cosmétiques avant achat, avec simulation du fond équipé.
4. Mode entraînement sans classement pour comprendre les tangentes et la synchronisation.
5. Défis variés de trajectoire (retour tactique, skip long, collecte précise) plutôt que seulement du volume.
6. Indicateur discret de proximité du record personnel.
7. Option contraste renforcé pour les débris, indépendante de leur difficulté.
8. Statistiques personnelles : durée moyenne, meilleur skip, fréquence des causes de mort.
9. Album de planètes découvertes avec descriptions courtes.
10. Ambiances musicales sélectionnables et aperçu immédiat dans Son (ajouté dans cette passe).
11. Petites variations décoratives de secteur qui ne ressemblent jamais à un obstacle.
12. Mode trajet court optionnel de 90 secondes, avec classement séparé; à évaluer uniquement si l'objectif devient de raccourcir les parties.

## Ajout musical de cette passe

Son propose quatre pistes : Neon Orbit (originale, défaut), Starlight (116 BPM, lumineuse), Void Runner (148 BPM, sombre) et Cosmic Disco (124 BPM, syncopée). Le choix est local et persistant. Les trois nouvelles compositions utilisent le synthétiseur 8-bit existant avec mélodies, basses et mixages distincts. Les clips sont créés à la demande et réutilisés; aucun achat ni effet sur le gameplay.
