# Catalogue cosmétique — extension

Les 50 propositions d'IDEES-COSMETIQUES.txt sont intégrées au catalogue, auxquelles s'ajoutent le pack Galaxie Gen-Z, le fond Scroll cosmique et la Fusée coquine. Les 12 musiques comprennent la piste par défaut, les 3 pistes précédentes et les 8 nouvelles compositions originales. Total : 90 entrées dans le hangar, catalogue historique inclus.

Les indices et identifiants historiques restent inchangés pour préserver les achats existants. Chaque catégorie possède un équipement gratuit d'indice 0. Ces contenus ne changent aucune collision, vitesse, durée de vol ou récompense.

## Réalisation et budget

Les nouveaux visuels sont des illustrations originales générées par code, à silhouettes et palettes distinctes, sans images de personnages ou marques tierces. Ils sont créés à la demande puis mémorisés. Les fusées et planètes utilisent des textures RGBA 256×256, les motifs de traînées 64×64 et les fonds 384×768. Les nouvelles planètes sont toutes des disques complets, avec marge transparente et sans découpage d'atlas. Chaque nouveau pack comprend cinq variantes.

Les traînées ajoutent des motifs correspondant à leur thème (notes, cristaux, bulles, circuits, etc.), avec un pool fixe de 18 éléments maximum. Les effets réduits divisent par deux la fréquence d'émission. Leur animation utilise le temps de simulation du joueur : la pause ne continue pas à produire de particules. Les fonds utilisent le défilement et la parallaxe du jeu, avec motifs de faible luminosité.

Les motifs Gen-Z sont des créations originales : 67, personnage de bois, morceau de poulet croustillant, NPC et BRUH. Le fond comporte des inscriptions et symboles originaux, pas d'images de memes récupérées sur Internet. La fusée humoristique est une silhouette stylisée non réaliste.

Les prix suivent les ordres de grandeur du catalogue existant : 180–1 400 matériaux pour les traînées, 420–1 800 pour les nouvelles fusées, 1 100–2 600 pour les packs de cinq planètes, 700–2 300 pour les fonds, 250–1 300 pour la musique. Ce sont des valeurs initiales à ajuster avec des observations de gains en partie, pas une estimation prétendument mesurée du temps d'acquisition.

## Vaisseaux

| Indice | Nom | Matériaux |
|---|---|---:|
| 11 | Luciole | 420 |
| 12 | Origami | 550 |
| 13 | Sardine cosmique | 600 |
| 14 | Scarabée | 1 200 |
| 15 | Théière orbitale | 850 |
| 16 | Taxi lunaire | 650 |
| 17 | Manta | 1 500 |
| 18 | Comète vintage | 750 |
| 19 | Méduse | 1 800 |
| 20 | Cactus | 950 |
| 21 | Sous-marin | 1 250 |
| 22 | Géode | 1 700 |
| 23 | Skate spatial | 1 100 |
| 24 | Abeille | 800 |
| 25 | Capsule arctique | 1 450 |
| 26 | Fusée coquine | 1 650 |

## Traînées

| Indice | Nom | Matériaux |
|---|---|---:|
| 6 | Poussière lunaire | 180 |
| 7 | Ruban pastel | 400 |
| 8 | Pixels perdus | 350 |
| 9 | Miel solaire | 450 |
| 10 | Aurore polaire | 950 |
| 11 | Étincelles de forge | 600 |
| 12 | Bulles cosmiques | 500 |
| 13 | Encre stellaire | 650 |
| 14 | Notes de vol | 900 |
| 15 | Cristaux de givre | 750 |
| 16 | Circuit imprimé | 1 000 |
| 17 | Queue de paon | 1 400 |
| 18 | Confettis | 850 |
| 19 | Étoiles filantes | 1 150 |
| 20 | Onde sonar | 700 |

## Packs de planètes

| Indice | Nom | Matériaux |
|---|---|---:|
| 4 | Jardin céleste | 1 100 |
| 5 | Archipel océan | 1 300 |
| 6 | Atelier mécanique | 1 700 |
| 7 | Confiserie | 1 500 |
| 8 | Cristallines | 2 200 |
| 9 | Civilisations miniatures | 2 400 |
| 10 | Saisons | 1 200 |
| 11 | Papier découpé | 1 600 |
| 12 | Volcans endormis | 1 900 |
| 13 | Mondes champignons | 2 100 |
| 14 | Galaxie Gen-Z | 2 600 |

## Fonds

| Indice | Nom | Matériaux |
|---|---|---:|
| 4 | Mer de nébuleuses | 800 |
| 5 | Nuit polaire | 950 |
| 6 | Archives stellaires | 1 000 |
| 7 | Poussière d'or | 1 300 |
| 8 | Océan cosmique | 1 200 |
| 9 | Cité lointaine | 1 900 |
| 10 | Crépuscule binaire | 1 500 |
| 11 | Rêve monochrome | 700 |
| 12 | Cathédrale de glace | 1 800 |
| 13 | Festival orbital | 1 600 |
| 14 | Scroll cosmique | 2 300 |

## Musiques

| Indice | Nom | Matériaux |
|---|---|---:|
| 0 | Neon Orbit | Gratuit |
| 1 | Starlight | 250 |
| 2 | Void Runner | 450 |
| 3 | Cosmic Disco | 600 |
| 4 | Lunar Lounge | 350 |
| 5 | Asteroid Funk | 700 |
| 6 | Binary Chase | 900 |
| 7 | Aurora Dream | 800 |
| 8 | Rusty Station | 500 |
| 9 | Solar Carnival | 1 100 |
| 10 | Deep Blue | 550 |
| 11 | Pocket Galaxy | 1 300 |

Les compositions, leur durée et leur génération sont documentées dans ORIGINAL-MUSIC.md. Le fichier YouTube demandé n'est pas inclus dans cette liste de compositions originales.

## Vérifications prévues

CosmeticCatalogTests vérifie l'unicité des identifiants, la continuité des indices, la conservation de tous les anciens identifiants/indices et l'existence de sprites distincts pour chaque nouveau vaisseau, traînée, fond et variante de planète. Il vérifie aussi que les sprites utilisent la texture complète plutôt qu'un rectangle d'atlas recadré. Les résultats d'exécution sont à consulter dans le compte rendu global : ce document ne remplace pas une validation visuelle ou un test sur appareil.
