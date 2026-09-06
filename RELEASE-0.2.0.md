# Orbit Breaker 0.2.0

## Changements

- Difficulté affine jusqu'à 3 000 UA, puis plafonnée. Rotation de 158 à 222 degrés/s, immunité de capture de 0,80 à 0,52 s. Les garanties de trajectoire et les orbites de répit restent actives.
- Unités astronomiques dans les scores, records, défis et bonus ; aucune conversion des sauvegardes.
- Cinq charges persistantes pour chacun des cinq bonus ; icônes distinctes, stocks lisibles et accès dédié aux améliorations.
- Tunnel d'hyperespace, nouvelle collection Aurore, transitions du décor tous les 500 UA.
- Cache PWA et URLs des fichiers WebGL versionnés pour cette livraison.

## Validation

- 45 tests EditMode réussis.
- Build APK Android réussi sans erreur ; signature identique à l'APK précédent, version 0.2.0 / code 2. Les deux avertissements portent sur l'absence volontaire des outils Pipeline dans le Player et sur les symboles de diagnostic des rapports de crash.
- Build WebGL réussi, aucune erreur de build. L'avertissement Pipeline indique seulement que les outils de contrôle de l'éditeur ne sont pas inclus dans le jeu.
- Chargement WebGL vérifié avec Edge en viewport mobile 430 × 932, jusqu'à l'écran de choix du pseudo ; aucune exception JavaScript ni requête échouée. Le verrouillage d'orientation n'est pas pris en charge dans ce navigateur de bureau, sans empêcher le chargement.
- Ces contrôles ne remplacent pas un test sur un téléphone Android et un iPhone physiques.

## Artefacts locaux

- APK : `Builds/Android/OrbitBreaker.apk` (version Android 0.2.0, code 2).
- WebGL : `Builds/Web/`, publié depuis la branche `gh-pages`.
- Les builds et les sauvegardes locales des joueurs ne sont pas ajoutés à la branche des sources.
