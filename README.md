# Orbit Breaker

Jeu mobile 2D à un doigt développé avec Unity, avec progression cosmétique et classement global Android/WebGL.

## État de cette mise à jour

Les modifications décrites ci-dessous sont intégrées au projet source et livrées en Android/WebGL. La version de configuration est `0.4.0` (Android code `4`). Les résultats de validation et de build de cette passe sont consignés dans `IMPLEMENTATION-REVIEW.md`.

- Le mode Entraînement a été retiré. Infini conserve la progression principale et son classement mondial ; Sprint possède désormais un classement mondial séparé.
- Le parcours quotidien utilise la date civile française comme graine et change exactement à minuit heure de Paris (été comme hiver) : 12 à 20 captures inédites selon un niveau quotidien de 1 à 5, puis une récompense unique de 90 à 320 matériaux. Une seule tentative est disponible par jour, consommée au premier décollage et signalée clairement sur l'écran principal après utilisation. Il ne possède volontairement aucun classement : l'objectif est de terminer le trajet du jour.
- Les 3e, 7e et 14e parcours quotidiens terminés débloquent chacun une fusée exclusive impossible à acheter. Le claim est idempotent et conserve les anciennes sauvegardes.
- Le Sprint dure 90 secondes de jeu actif. Une animation de trou de ver déjà lancée termine son arrivée avant d'afficher le résultat, puis le meilleur score est envoyé au classement Sprint.
- Les musiques s'achètent et s'équipent dans l'onglet Musique du hangar. Neon Orbit reste gratuite et sélectionnée par défaut ; Son conserve le mixage général/musique/effets.
- Les ajouts cosmétiques et musicaux utilisent des identifiants nouveaux, afin de conserver les achats et équipements existants.
- Une option de contraste renforcé ajoute un contour visible aux débris, sans modifier leurs collisions.

Le joueur tourne automatiquement autour d'une ancre. Une pression le propulse selon la tangente de son orbite : il faut atteindre l'anneau d'une ancre située plus haut, éviter les *breakers* et enchaîner les captures les plus précises possible.

## État du prototype

- Boucle de jeu complète : orbite, propulsion, capture et défaite
- Génération procédurale infinie avec difficulté progressive
- Recyclage des ancres et obstacles par *object pooling*
- Score unique en UA, validé uniquement à l'arrivée sur une orbite
- Multiplicateur de distance croissant pendant chaque vol, particulièrement rentable lors des skips
- Retour tactique vers les orbites visitées avec restauration du checkpoint de score
- Fusée animée avec propulseur progressif et jauge de carburant intégrée
- Packs de planètes équipables et variantes de débris spatiaux sélectionnées de façon déterministe
- Fond spatial dynamique avec nébuleuse et étoiles en parallaxe réversible
- Indicateurs animés montrant le sens de rotation de chaque orbite
- Dangers mobiles déterministes, télégraphiés et introduits progressivement
- Validateur de parcours échantillonnant les fenêtres de lancement avant d'accepter une orbite
- Portes de synchronisation calculées depuis une trajectoire réellement atteignable, optionnelles et progressivement plus précises
- Débris libres réservés aux corridors de skip réellement atteignables : recherche bornée depuis N−2 à N−5, sens de rotation respecté et trajet vérifié contre toutes les orbites présentes
- Motifs procéduraux et phases de respiration répartis sur des cycles de difficulté
- Cosmétiques de fusée, traînée, planètes, fond et musique débloqués contre des matériaux
- Trois défis actifs simultanément, récapitulatif de partie et écran Statistiques dédié : records Infini/Sprint, carrière, exploration, tentatives et parcours quotidiens terminés, causes de mort
- Replay local et skippable des trois dernières secondes après une mort, avant l’écran de résultats
- Cartes stellaires avec compteurs global et par pack
- Transitions progressives de couleur du décor tous les 500 UA de score, y compris avec les fonds du hangar ; notification de nouveau secteur.
- Distance affichée en UA (unités astronomiques, unité de jeu ; valeurs des sauvegardes et du classement inchangées).
- Rythme 0.4.0 : distance de base doublée, séries de skips avec bonus ×1,25 au deuxième skip puis +0,25 jusqu'à ×2,5. Une capture normale, un retour, un checkpoint revisité ou un trou de ver remet la série à zéro. Aucun changement aux records existants.
- Difficulté progressive jusqu'à 900 UA puis plafonnée : rotation de 158 à 230 degrés/s. Débris orbitaux à partir de 100 UA (cible 1/6, puis 40 % à 300 UA), débris de skip à partir de 300 UA (12 à 45 % des occasions jusqu'à 600 UA). Les placements dangereux restent exclus et les orbites de répit conservées ; les taux réellement observés dépendent des trajectoires sûres disponibles.
- Répartition absolue gauche/centre/droite des planètes pour éviter l'accumulation d'orbites contre un seul bord. Les orbites déjà générées ne changent pas brutalement et un retour arrière ne réduit pas la difficulté atteinte.
- Bonus persistants : 5 charges **par type**, soit 25 au total ; icônes dédiées et compteurs `0/5` à `5/5` dans le HUD et le menu.
- Musique chiptune originale en boucle et mixage séparé général/musique/effets
- Menu audio accessible depuis l'accueil et l'écran de fin
- Réglages persistants par onglets : son, aides visuelles, vibrations, modes 30/60/120 FPS, caméra stable et secousses indépendantes
- Écran de crédits intégré : création JeanEdouart © 2026 et transparence sur l'assistance IA au développement
- Identité visuelle Orbit Breaker dédiée pour l'icône Android et l'icône PWA sur l'écran d'accueil iOS
- Build WebGL installable comme web app plein écran, avec manifeste PWA et cache hors ligne
- Mise en pause tactile avec reprise immédiate
- Interface adaptée aux zones sûres des appareils mobiles
- Effets visuels, explosion, traînée, sons synthétisés et retours haptiques différenciés
- Contrôles tactiles, souris et clavier
- Build Android portrait avec IL2CPP
- Tests EditMode du réglage de difficulté, du calcul du score et de la synchronisation
- Kit graphique original stocké dans `Assets/OrbitBreaker/Resources/Art`

## Contrôles

| Plateforme | Action |
| --- | --- |
| Mobile | Toucher l'écran |
| Éditeur | Clic gauche |
| Clavier | Espace ou Entrée |

Une pression pendant une orbite libère la bille. Pendant le vol, le multiplicateur augmente de `x0,1` toutes les `0,12 s`, jusqu'à `x6`. La distance n'est ajoutée au score qu'après une capture réussie : un vol ambitieux peut donc rapporter beaucoup, mais une chute ne rapporte rien. Après une défaite, une nouvelle pression relance immédiatement une partie.

## Prérequis

- Unity `6000.5.0f1`
- Universal 2D / URP `17.5.0`
- Input System `1.19.0`
- Pour Android : Android Build Support, SDK/NDK Tools et OpenJDK installés depuis Unity Hub

## Lancer le projet

1. Ouvrir le dossier `OrbitBreaker` avec Unity Hub.
2. Charger `Assets/Scenes/Main.unity`.
3. Vérifier que la cible active est Android dans **File > Build Profiles**.
4. Entrer en mode Play.

La scène contient volontairement peu d'objets persistants. `GameBootstrap` instancie les systèmes et les éléments visuels au lancement ; les sprites originaux sont chargés depuis `Resources/Art`.

## Architecture

```text
OrbitBreaker/
├── Assets/
│   ├── OrbitBreaker/
│   │   ├── Runtime/
│   │   │   ├── GameBootstrap.cs
│   │   │   ├── GameTuning.cs
│   │   │   ├── OrbitPlayer.cs
│   │   │   ├── OrbitWorld.cs
│   │   │   ├── OrbitPresentation.cs
│   │   │   └── RuntimeAssets.cs
│   │   └── Tests/EditMode/
│   │       └── GameTuningTests.cs
│   └── Scenes/Main.unity
├── Packages/
└── ProjectSettings/
```

Responsabilités principales :

- `GameBootstrap` : cycle de partie, entrées, score et coordination
- `OrbitPlayer` : états orbital, en vol et détruit
- `OrbitWorld` : génération infinie, obstacles et pools d'objets
- `OrbitPresentation` : caméra, HUD, zone sûre, audio et feedbacks
- `GameTuning` : multiplicateur, score de distance et courbes de difficulté testables
- `RuntimeAssets` : formes, matériaux et sons générés à l'exécution

## Tests

Dans Unity, ouvrir **Window > General > Test Runner**, sélectionner **EditMode**, puis lancer tous les tests.

Les tests couvrent notamment :

- la monotonie et les bornes de la difficulté ;
- les limites des vitesses orbitale et de propulsion ;
- la progression et le plafonnement du multiplicateur ;
- la validation de la distance à l'atterrissage ;
- la phase d'apprentissage sans danger ;
- les patterns déterministes des dangers.

## Progression et hangar

- Des cristaux de matériaux de trois tailles apparaissent entre les orbites ; leur taille détermine une valeur de 1, 3 ou 7, avant les éventuels effets des bonus.
- Le portefeuille, les achats et les équipements sont sauvegardés localement avec `PlayerPrefs`, également sur WebGL via le stockage du navigateur. Il n'y a pas de synchronisation du portefeuille entre appareils.
- Les collectes de matériaux sont regroupées sur des fenêtres de 0,5 seconde. Pause, perte de focus, fin de partie et fermeture normale forcent la sauvegarde. Une fermeture brutale sans callback peut perdre les dernières collectes non sauvegardées ; achats et récompenses restent enregistrés immédiatement.
- Le hangar comprend 30 fusées (dont 3 exclusives quotidiennes), 21 traînées, 15 packs de planètes, 15 fonds et 13 musiques, en comptant les choix par défaut. Les 50 propositions de `IDEES-COSMETIQUES.txt` sont complétées par le pack Gen-Z, le fond Scroll cosmique et la fusée Coquine.
- Les nouveaux vaisseaux, packs de planètes et fonds utilisent des planches illustrées générées pour la direction artistique du jeu ; les anciens dessins de secours ne sont utilisés qu'en cas d'asset manquant.
- Le catalogue comprend 112 défis, dont 12 nouveaux défis de trajectoire/collecte. Trois sont actifs à la fois ; lorsque les trois récompenses sont récupérées, un nouveau trio distinct remplace le précédent.
- `MetaProgression.cs` centralise l'économie et les défis ; `ExpandedCosmetics.cs` ajoute le catalogue cosmétique. Les prix sont exprimés en matériaux.
- La récompense d'un défi et son état récupéré sont persistés ensemble ; le débit d'une amélioration de bonus et son niveau aussi. Les envois de record sont sérialisés, et un rafraîchissement raté conserve le dernier classement en mémoire.
- Les onglets du classement sont contextuels : Infini n'affiche que son record, Sprint n'affiche le libellé 90 s que dans son propre onglet.

## Build Android

Dans Unity :

1. Ouvrir **File > Build Profiles**.
2. Sélectionner Android.
3. Choisir `Assets/Scenes/Main.unity`.
4. Générer un APK pour les tests locaux ou un AAB signé pour Google Play.

Configuration actuelle :

- identifiant : `com.orbitbreaker.game`
- orientation : portrait
- version : `0.4.0` ; code Android : `4`
- backend : IL2CPP
- API Android minimale : 26

Le dossier `Builds/` est ignoré par Git.

## Ajuster le gameplay

Les valeurs principales se trouvent dans `GameTuning.cs` :

- vitesse orbitale ;
- vitesse de propulsion ;
- largeur de capture ;
- durée maximale d'un vol ;
- espacement des ancres ;
- évolution de la difficulté ;
- calcul du score et du combo.

Modifier ces valeurs par petites étapes et tester sur un téléphone physique : la lisibilité et la sensation du toucher sont plus importantes que la difficulté brute.

## Validation avant la prochaine livraison

- sessions d'équilibrage et profilage sur appareil Android et navigateur mobile ;
- vérification tactile des menus, textes simultanés et aperçus de tous les cosmétiques ;
- comparaison du poids audio, de la mémoire et du démarrage après génération des builds autorisés ;
- préparation d'un AAB signé et d'une fiche Google Play lorsque le contenu est validé.
