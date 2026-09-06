# Collection Aurore et ajustements du 6 septembre 2026

## Gameplay

Version 0.2.0 : difficulté linéaire selon la distance maximale atteinte pendant la partie, de 0 à 3 000 UA, puis plafonnée. Rotation de 158 à 222 degrés/s ; immunité de capture de 0,80 à 0,52 seconde. Débris possibles dès la quatrième orbite avec une densité progressivement accrue. Les exclusions de sécurité et les orbites de répit restent actives. Les orbites déjà générées ne changent pas brutalement.

Le secteur dépend du score affiché : 0–499, 500–999, 1000–1499, etc. Les six teintes tournent progressivement, sans remplacer le fond équipé. Revenir en arrière rétablit la teinte correspondante. Une nouvelle partie repart du secteur initial.

Hyperspace : les traits rectilignes ont été remplacés par des nappes cyan/violet en perspective, animées en tunnel. Les planètes restent visibles. Effet procédural sans post-traitement plein écran, atténué par le réglage des effets renforcés.

## Nouveaux cosmétiques

| Objet | Matériaux |
| --- | ---: |
| Fusée Aurore | 1800 |
| Sillage boréal | 750 |
| Flux améthyste | 1100 |
| Mondes d’Aurore (quatre planètes) | 1900 |
| Voile boréal | 1500 |

Les identifiants existants sont conservés pour préserver les achats sauvegardés.

## Assets générés par IA

Dans `OrbitBreaker/Assets/OrbitBreaker/Resources/Art/` :

- `aurora-rocket.png` : fusée originale blanche nacrée, ailes turquoise, cockpit violet, accents dorés, vue du dessus orientée vers le haut, sans flamme, silhouette entière sur fond transparent.
- `aurora-planets.png` : atlas 2 × 2 de quatre mondes originaux, glace turquoise, monde violet, géante gazeuse pâle et planète fissurée violet/vert. Silhouettes complètes, cellules séparées avec marges, fond transparent.
- `aurora-background.png` : fond spatial portrait, centre sombre lisible, nébuleuses émeraude/turquoise sur les bords et touches violettes, sans planète ni texte.

Ces descriptions documentent la direction des prompts utilisés ; aucun asset de Star Citizen ou No Man’s Sky n’a été repris.

## Vérification

Tests EditMode couvrant les secteurs, le stockage indépendant des cinq types de bonus, les trajectoires et la difficulté. Menu et compteurs vérifiés en Play Mode. Les distances sont affichées en UA sans conversion des valeurs sauvegardées. Les résultats de livraison sont consignés dans le compte rendu de version.
