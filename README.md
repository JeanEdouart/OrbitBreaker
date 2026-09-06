# Orbit Breaker

Prototype professionnel de jeu mobile 2D infini, jouable à un doigt et développé avec Unity.

Le joueur tourne automatiquement autour d'une ancre. Une pression le propulse selon la tangente de son orbite : il faut atteindre l'anneau d'une ancre située plus haut, éviter les *breakers* et enchaîner les captures les plus précises possible.

## État du prototype

- Boucle de jeu complète : orbite, propulsion, capture et défaite
- Génération procédurale infinie avec difficulté progressive
- Recyclage des ancres et obstacles par *object pooling*
- Score unique en mètres, validé uniquement à l'arrivée sur une orbite
- Multiplicateur de distance croissant pendant chaque vol, particulièrement rentable lors des skips
- Retour tactique vers les orbites visitées avec restauration du checkpoint de score
- Fusée animée avec propulseur progressif et jauge de carburant intégrée
- Six planètes et six variantes de débris spatiaux sélectionnées de façon déterministe
- Fond spatial dynamique avec nébuleuse et étoiles en parallaxe réversible
- Indicateurs animés montrant le sens de rotation de chaque orbite
- Dangers mobiles déterministes, télégraphiés et introduits progressivement
- Validateur de parcours échantillonnant les fenêtres de lancement avant d'accepter une orbite
- Portes de synchronisation calculées depuis une trajectoire réellement atteignable, optionnelles et progressivement plus précises
- Débris libres réservés aux corridors de skip N−2 → N réellement atteignables, avec bonus de frôlement en vol
- Motifs procéduraux et phases de respiration répartis sur des cycles de difficulté
- Quatre styles cosmétiques de traînée et propulseur débloqués par la distance cumulée
- Mission quotidienne locale et récapitulatif des performances de chaque partie
- Transitions progressives de couleur du décor tous les 500 UA de score, y compris avec les fonds du hangar ; notification de nouveau secteur.
- Distance affichée en UA (unités astronomiques, unité de jeu ; valeurs des sauvegardes et du classement inchangées).
- Difficulté linéaire jusqu'à 3 000 UA puis plafonnée : rotation de 158 à 222 degrés/s, immunité de capture de 0,80 à 0,52 s, débris possibles dès la quatrième orbite, fenêtres de sécurité et orbites de répit conservées. Les orbites déjà générées ne changent pas brutalement et un retour arrière ne réduit pas la difficulté atteinte.
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

- Des cristaux de matÃ©riaux de trois tailles apparaissent entre les orbites ; leur taille dÃ©termine une valeur de 1, 3 ou 7.
- Le portefeuille, les achats et les Ã©quipements sont sauvegardÃ©s localement avec `PlayerPrefs`.
- Le hangar contient 10 fusÃ©es, 4 propulsions, 3 packs de planÃ¨tes et 3 fonds spatiaux.
- Le catalogue comprend 100 dÃ©fis. Trois sont actifs Ã  la fois ; lorsque les trois rÃ©compenses sont rÃ©cupÃ©rÃ©es, un nouveau trio distinct remplace le prÃ©cÃ©dent.
- `MetaProgression.cs` centralise l'Ã©conomie, le catalogue cosmÃ©tique et les dÃ©fis.

## Build Android

Dans Unity :

1. Ouvrir **File > Build Profiles**.
2. Sélectionner Android.
3. Choisir `Assets/Scenes/Main.unity`.
4. Générer un APK pour les tests locaux ou un AAB signé pour Google Play.

Configuration actuelle :

- identifiant : `com.orbitbreaker.game`
- orientation : portrait
- version : `0.1.0`
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

## Prochaines étapes possibles

- sessions d'équilibrage sur appareil Android ;
- variantes d'ancres et obstacles mobiles ;
- défis quotidiens et missions ;
- skins et thèmes cosmétiques ;
- classement et succès ;
- paramètres audio et vibration ;
- préparation d'un AAB signé et d'une fiche Google Play.
