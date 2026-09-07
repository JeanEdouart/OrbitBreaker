# Orbit Breaker 0.3.0 — rythme et séries de skips

- Gain de distance de base ×2 ; le multiplicateur de vol est conservé.
- Séries de vrais skips vers de nouvelles orbites : ×1 au premier, ×1,25 au deuxième, ×1,5 au troisième, puis +0,25 jusqu'à ×2,5. Ce facteur multiplie la récompense du vol ; ce n'est pas un second score.
- Une capture normale, un retour, un checkpoint revisité ou un trou de ver casse la série. Les checkpoints conservent le score déjà enregistré, sans nouvelle récompense.
- Rotation de 158 à 230 degrés/s avec une progression plafonnée à 900 UA.
- Débris orbitaux : aucun avant 100 UA ; fréquence cible de 1/6 à 40 % entre 100 et 300 UA. Un petit budget reporté permet de conserver de la densité sans sacrifier les contrôles de visibilité, de trajectoire et de répit.
- Débris de skip : aucun avant 300 UA ; chance par occasion valide de 12 à 45 % entre 300 et 600 UA. Les contrôles de sens de rotation, de capture intermédiaire et d'éloignement des orbites restent actifs.
- Planètes distribuées sur des positions latérales équilibrées, avec une variation aléatoire limitée et validation de chaque transfert.
- Compteur de série séparé des notifications de capture, matériaux et frôlements ; explication ajoutée aux tips.

Validation : 48 tests EditMode ; scénario runtime vérifiant le bonus du deuxième skip, le retour au score précédent et l'absence de gain supplémentaire sur checkpoint revisité. Rendu portrait du compteur contrôlé dans Unity. L'équilibrage reste à confronter aux retours humains ; aucune garantie d'engagement ne découle des tests techniques.

Livraison prévue : APK `Builds/Android/OrbitBreaker.apk` (version 0.3.0, code 3), WebGL `Builds/Web/`, cache et URLs versionnés. Les sauvegardes existantes et le classement ne sont pas réinitialisés ; les nouveaux scores bénéficient des nouvelles règles.
