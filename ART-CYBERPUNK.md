# Cyberpunk — artwork et résurrection 67

Les sept images de `OrbitBreaker/Assets/OrbitBreaker/Resources/Art/cyberpunk-*.png`
sont générées avec l'outil intégré de génération d'images, et non dessinées par
les générateurs de formes du jeu. Chaque planète est une image indépendante,
sans découpe d'atlas. Les identifiants, achats, prix, musique et traînée existants
sont conservés.

## Jeu de prompts / direction artistique

- Fusée : sprite vu du dessus, nez vers le haut, chasseur cyberpunk métallique
  sombre, cockpit cyan, accents magenta et jaunes, silhouette complète,
  sans moteur allumé, fond réellement transparent. Passe d'édition pour
  conserver le vaisseau et retirer le fond, avec marge autour de la silhouette.
- Fond : espace cyberpunk vertical, mégalopoles néon sur les côtés, corridor
  central sombre et lisible pour le gameplay, indigo/cyan/magenta, sans interface.
- Planète 0 : monde sphérique mégalopole cyan, constructions détaillées,
  silhouette complète isolée sur transparence.
- Planète 1 : monde urbain magenta, quartiers néon et voies lumineuses,
  silhouette complète isolée sur transparence.
- Planète 2 : monde forge orange, métal sombre et fissures incandescentes,
  silhouette complète isolée sur transparence.
- Planète 3 : monde de données violet, tours cristallines et anneau orbital,
  anneau complet, isolé sur transparence.
- Planète 4 : cité verte bio-technologique, dômes et jardins lumineux,
  silhouette complète isolée sur transparence.

## Résurrection

Une fois par partie, une mort à un score finissant par 67 déclenche une séquence
de huit secondes : arrêt, recentrage du vaisseau, traversées de 67 dans quatre
directions, retour sur la dernière orbite nettoyée de ses dangers orbitaux.
Le temps de jeu et les durées de bonus sont gelés pendant la séquence.
La reprise n'émet pas d'événement de capture et n'accorde pas de score gratuit.
Une protection de deux secondes accompagne la reprise. La fin des 90 secondes
et la réussite du parcours quotidien ne déclenchent pas de résurrection.

Aucun build ou commit n'est nécessaire pour tester ces changements dans Unity.
