# Implémentation de la review

## Passe catalogue, parcours quotidien et classements (2026-09-07)

- Le mode Entraînement est retiré en conservant les valeurs numériques des modes sauvegardés.
- Le parcours du jour est fini : graine UTC, niveau déterministe 1–5, 12–20 captures uniques, difficulté adaptée et claim unique de 90–320 matériaux.
- Trois fusées illustrées exclusives se débloquent aux 3e, 7e et 14e jours terminés ; le hangar interdit leur achat direct.
- Infini et Sprint 90 s ont des classements distincts. Les métadonnées publiées comprennent les deux records et le nombre de planètes découvertes. Aucun classement quotidien n'est créé.
- Les cartes stellaires affichent les compteurs global et par pack. Les sauvegardes existantes restent valides.
- Les nouvelles fusées, planètes et fonds utilisent des illustrations importées ; les traînées ont été agrandies sans changer leurs collisions.
- `meow_sad.mp3`, fourni et autorisé par le propriétaire du projet, est proposé dans l'onglet Musique du hangar.
- Validation : compilation sans erreur, 64/64 tests EditMode et 1/1 test PlayMode, puis lancement Play sans nouvelle erreur de console. Les builds Android et WebGL du 7 septembre 2026 sont réussis.

Le classement `orbit_breaker_sprint_90` est déployé dans Unity Services avec l'état `Deployed`.

Ce document distingue les changements source des vérifications encore nécessaires. Aucun résultat de test sur téléphone n'est déduit d'une simple lecture du code ou de la réussite d'un build.

## Fiabilité et sauvegardes

- `OnlineLeaderboard` sérialise initialisation, envoi et rafraîchissement. Une réponse ancienne ne supprime pas un meilleur score arrivé pendant l'envoi. Les erreurs laissent le record en attente. Le dernier classement chargé reste en mémoire après un échec, avec une date de rafraîchissement accessible à l'interface.
- Les synchronisations sur orbites revisitées, captures arrière et arrivées par trou de ver ne progressent pas les défis de synchronisation.
- Une récupération de défi écrit le solde, le statut récupéré et, au troisième défi, le nouveau trio avant une sauvegarde commune. Les anciennes sauvegardes avec trois défis déjà réclamés sont débloquées à la prochaine consultation.
- Une amélioration de bonus écrit désormais le débit et le niveau avant une sauvegarde commune.
- Les collectes modifient immédiatement le solde en mémoire et sont persistées au plus tard au prochain passage de la boucle après 0,5 seconde. Les pauses, pertes de focus, fins de partie, resets et fermetures normales forcent un flush. Les achats et récompenses ne sont pas retardés. La fenêtre théorique de perte en cas d'arrêt brutal est de 0,5 seconde de collectes tant que Unity continue à traiter ses frames ; un blocage de l'application peut l'allonger.
- La saturation du portefeuille empêche un dépassement d'entier de convertir une addition en solde négatif.

PlayerPrefs reste une sauvegarde locale modifiable. Cette passe n'ajoute ni validation serveur ni migration de service distant. Le classement mondial existant continue de recevoir les parties du mode Infini uniquement.

## Gameplay et modes

- La recherche des débris de skip considère jusqu'à quatre sources, de N−2 à N−5. Chaque trajectoire suit la tangente dans le sens réel de rotation et doit éviter la capture par toutes les autres orbites présentes. Les placements respectent leurs marges de collision ; ce n'est pas une recherche exhaustive de toute la carte.
- Entraînement, Parcours du jour et Sprint utilisent un inventaire temporaire copié au début de la partie. Ramasser ou utiliser un bonus dans ces modes ne modifie pas le stock persistant. Matériaux, défis, statistiques Infini et records mondiaux ne sont pas crédités par ces runs.
- Le Parcours du jour dérive sa graine de la date UTC. Le monde reste adapté au score et aux choix du joueur ; une graine commune ne signifie pas une géométrie immuable quels que soient les choix. Son record est local par jour.
- Le Sprint utilise 90 secondes de jeu actif, pauses exclues. Un trou de ver en cours termine sa transition avant le résultat pour éviter d'abandonner le joueur au milieu de la cinématique.
- Les statistiques locales enregistrent durée moyenne, meilleur skip, meilleure série et causes de mort des nouvelles parties Infini. Elles ne reconstituent pas l'historique précédant cette mise à jour.
- Le contraste renforcé des débris est une option purement visuelle, activable séparément des effets, sans changement de rayon de collision ou de vitesse.

## Contenu et interface

Le catalogue source est étendu avec les 50 cosmétiques proposés, les extras Gen-Z et la fusée Coquine, ainsi que 12 sélections musicales au total. Les identifiants existants sont conservés. Les musiques sont équipées et achetées au hangar ; l'onglet Son conserve les trois volumes. Les prix et indices sont définis dans `ExpandedCosmetics`.

Les ajouts d'interface (modes, statistiques, album, aperçu, contraste et musique) font l'objet de la validation visuelle commune. La présence de leur code ne remplace pas la vérification du rendu aux résolutions mobiles.

## Vérifications

### Contrôles réalisés dans Unity 6000.5.0f1

- Compilation réussie après intégration et découpage des scripts de présentation.
- Suite EditMode finale : **64 tests réussis, 0 échec, 0 ignoré** (catalogue, géométrie, progression, sauvegardes, nouveaux défis et achat musical).
- Test PlayMode audio : **1 test réussi, 0 échec**, couvrant les douze ressources distinctes, mono, chargées, non silencieuses, sans échantillons non finis ni saturation au point de contrôle (0,89 seconde).
- Lecture en jeu confirmée pour Neon Orbit ; préécoute et retour à la piste équipée contrôlés. Le test de chargement audio a été déplacé d'EditMode à PlayMode après un blocage du chargement asynchrone hors simulation ; son exécution en PlayMode réussit.
- Contrôle en mode Play du passage entre les quatre modes, de la collecte sans gain persistant en entraînement et du ramassage de bonus sans modification du stock sauvegardé.
- Contrôle local de l'interface du classement avec 0, 1, 10, 11 et 100 entrées : hauteur exacte par ligne, défilement borné et absence de duplication après deux rendus identiques. Ces données de test n'ont pas été envoyées au classement mondial.
- Captures inspectées du hangar musical au format 540 × 960 et d'une planche des nouvelles fusées et variantes de planètes. Un texte sortant de la boîte de la fusée Sardine a été réduit. Les noms des cartes ont été agrandis et séparés des images.
- Nettoyage des anciens menus inaccessibles et du synthétiseur musical inutilisé. Caméra, fond, audio, options et éléments communs du HUD sont répartis dans des fichiers dédiés.

### Écart explicite par rapport aux demandes externes

La piste YouTube n'est pas embarquée. Le fichier `meow_sad.mp3` fourni par le propriétaire du projet est intégré, et les autres pistes livrées sont les compositions originales décrites dans `ORIGINAL-MUSIC.md`. Les variantes Gen-Z sont des dessins originaux, pas des images téléchargées de personnages/mèmes. Aucun backend supplémentaire n'a été ajouté en dehors des classements Unity Services existants et du classement Sprint déployé pendant cette livraison.

Des tests de régression source ont été ajoutés pour la récupération unique des défis, le renouvellement du trio, la graine quotidienne, la géométrie reproductible et le solde des collectes après dépense/flush. Ils restaurent les clés de sauvegarde qu'ils utilisent. Le résultat effectif du Test Runner doit être consigné après l'exécution commune.

Restent nécessaires avant une livraison :

- inspection visuelle des aperçus, des textes simultanés et des onglets sur plusieurs formats ;
- test tactile et réseau du classement sur téléphone, recherche et rafraîchissements répétés (géométrie et rendus répétés déjà contrôlés localement à 0, 1, 10, 11 et 100 joueurs) ;
- vérification en jeu des quatre modes, notamment fin de Sprint pendant un trou de ver et passage d'un mode local à Infini ;
- écoute des pistes sur téléphone et vérification des boucles ;
- profilage réel CPU/mémoire/allocations Android et WebGL, en comparant une scène et une durée identiques ;
- vérification du démarrage et du comportement des builds sur de vrais appareils Android et iOS/WebGL.

Builds de livraison du 7 septembre 2026 :

- Android : réussi, 0 erreur, APK de 74,41 Mio ;
- WebGL : réussi, 0 erreur et 3 avertissements Unity ;
- publication WebGL préparée pour la branche `gh-pages`.

Les optimisations plus larges du rendu, les nouveaux pools et toute validation distante restent subordonnés à une mesure ou à un chantier séparé. Aucune promesse de protection contre la triche ni de gain chiffré de performances n'est faite ici.
