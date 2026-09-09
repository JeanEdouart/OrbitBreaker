# Coffres quotidiens

Un coffre gratuit toutes les 24 heures après la dernière ouverture. Le premier coffre est disponible immédiatement. Le stockage repose sur PlayerPrefs (Android et stockage local du navigateur WebGL), avec l'horloge UTC de l'appareil : il ne constitue pas une validation serveur contre la modification de l'heure ou l'effacement des données.

Un seul lot par ouverture : matériaux (50 %), gadgets (30 %) OU cosmétique (20 %). La rareté est indépendante de la catégorie. Les quantités sont équiprobables dans leur tranche ; les cosmétiques éligibles ont chacun la même chance. Aucun cumul des trois catégories. Un lot de gadgets peut être partiellement converti si les stocks sont pleins.

| Rareté | Probabilité | Matériaux | Charges de bonus | Valeur du cosmétique |
|---|---:|---:|---:|---:|
| Vert | 60 % | 1–500 | 1–3 | 1–500 |
| Bleu | 24 % | 500–1 000 | 3–5 | 501–1 000 |
| Violet | 11 % | 1 000–1 500 | 5–10 | 1 001–1 500 |
| Doré | 4,2 % | 1 500–3 000 | 10–25 | 1 501–3 000 |
| Rainbow | 0,8 % | 3 000–5 000 | 5 de chaque type | ≥ 3 000 |

Les cosmétiques sont choisis parmi les objets non possédés de la tranche de prix correspondante. Rainbow sélectionne les objets à partir de 3 000 matériaux. Les fusées exclusives du parcours quotidien sont toujours exclues. Si la tranche est entièrement possédée, le tirage cosmétique est remplacé par des matériaux (maximum de la tranche, ou 3 000 pour Rainbow). Les doublons de bonus excédant le stock de cinq sont convertis à 25 matériaux chacun. Le panneau distingue les charges effectivement ajoutées et leur compensation.

## Fiabilité

L'attribution enregistre d'abord un journal contenant les soldes et stocks finaux, puis applique ces valeurs et le reçu. Une récupération au prochain lancement rejoue les valeurs absolues, sans doubler le gain. Le dernier reçu est consultable après fermeture/réouverture. Le compteur ne réinitialise plus l'écran de résultat. Le menu bloque les actions de jeu, et les bonus reçus sont synchronisés avant la première propulsion en mode Infini.

Tests : seuil exact des 24 h, double clic, horloge reculée, timestamp invalide, stocks pleins, reçu persistant, récupération du journal et 20 000 tirages avec limites/exclusions.

## Présentation

Assets générés avec l'outil intégré de génération d'images :

- `OrbitBreaker/Assets/OrbitBreaker/Resources/Art/daily-chest.png`
- `OrbitBreaker/Assets/OrbitBreaker/Resources/Art/daily-chest-open.png`

Prompt fermé : coffre spatial premium, titane bleu nuit et armure argentée, joints lumineux cyan, fermoir diamant, vue trois quarts, objet entier isolé sur fond transparent, sans texte.

Prompt ouvert : conserver le même coffre et sa perspective, ouvrir le couvercle vers le haut/arrière, cœur énergétique cyan/blanc visible, fond transparent, aucun objet ni texte supplémentaire.

L'animation fait défiler des cartes de lots, ralentit vers la récompense sauvegardée et révèle son visuel, puis disperse les particules avec la couleur de rareté. Les cartes voisines sont décoratives, sans effet sur les probabilités. Le panneau « Contenu et probabilités » explique les règles. Le Rainbow utilise un halo multicolore. Les cinq carillons respectent le volume des effets et le retour haptique respecte le réglage existant.

Contrôle visuel dans la vue Game à 1080×1920, 768×1024 et 360×800 ; validation sur appareils Android/iPhone à effectuer lors de la prochaine distribution.
