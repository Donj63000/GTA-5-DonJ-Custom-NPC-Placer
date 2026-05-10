# Documentation développeur - DonJ Custom NPC Placer / GTA V Enhanced

## 1. Objectif du projet

Le projet est un mod solo pour GTA V Enhanced sur Windows x64.

Le mod s'appelle :

DonJ Custom NPC Placer

Le fichier livré au jeu s'appelle :

DonJCustomNpcPlacer.ENdll

Son but est de permettre au joueur de créer des scènes personnalisées en mode histoire :

- placement de PNJ ;
- placement de véhicules ;
- placement d'objets ;
- placement d'entrées/sorties d'intérieurs ;
- sauvegarde et chargement XML ;
- respawn automatique ;
- gardes alliés ;
- patrouilles ;
- appels téléphoniques Cartel ;
- attaques Ballas ;
- escorte haute sécurité avec limousine blindée ;
- gestion d'objets interactifs comme argent, soin, armure, munitions ;
- debug/logs ;
- tests de non-régression.

Le projet ne doit jamais être pensé pour GTA Online. Il est fait pour le mode histoire uniquement.

## 2. Contexte technique exact

Configuration cible actuelle :

Plateforme : Windows x64
Jeu : Grand Theft Auto V Enhanced version Steam
Exécutable : GTA5_Enhanced.exe
Version jeu sur le poste : 1.0.1013.34
Dossier jeu :
C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V Enhanced

Loader / runtime côté jeu :

ScriptHookV.dll 3788.0.1013.34
dinput8.dll 1.0.0.1
NIBScriptHookVDotNet.asi
NIBScriptHookVDotNet2.dll 2.11.6

Point critique :

Le projet cible l'API v2 via NIBScriptHookVDotNet2.dll.
Ne pas coder avec l'API ScriptHookVDotNet v3.
Ne pas supposer que ScriptHookVDotNet2.dll classique est présent.

Dossier scripts chargé par le jeu :

C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V Enhanced\Scripts

Fichier attendu dans Scripts :

DonJCustomNpcPlacer.ENdll

Fichier optionnel mais utile pour debug :

DonJCustomNpcPlacer.pdb

Mods déjà présents à connaître :

Menyoo.asi
NativeTrainer.asi
pc_trainer.asi
OpenRPF.asi
DirectStorageFix.asi
NIBMods.net.ENdll
IronmanV3EG.ENdll
Superman V2.ENdll
DonJEnemySpawner.ENdll

Logs utiles :

C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V Enhanced\NIBScriptHookVDotNet.log
C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V Enhanced\ScriptHookV.log
C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V Enhanced\menyooLog.txt
C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V Enhanced\Scripts\*.log

Stack build locale :

C#
.NET Framework 4.8
.NET SDK 9.0.312
MSBuild 17.14

## 3. Structure du dépôt

Structure importante :

GTA5modDEV.sln

AGENTS.md
README.md
LICENSE
crash-list.md

Mode-pour-jeu-ici\
  DonJCustomNpcPlacer.ENdll
  DonJCustomNpcPlacer.pdb
  INSTALLATION_SIMPLE.txt

src\
  DonJEnemySpawner\
    DonJEnemySpawner.cs
    DonJEnemySpawner.HighSecurityEscort.cs
    DonJEnemySpawner.Interiors.cs
    DonJEnemySpawner.Interiors.AdvancedLoading.cs
    DonJEnemySpawner.InteriorCatalog.cs
    DonJEnemySpawner.Logging.cs
    DonJEnemySpawner.csproj

tests\
  DonJEnemySpawner.Tests\
    DonJEnemySpawnerTests.cs
    SafetySimulationTests.cs
    BugLogCollectionTests.cs
    DonJEnemySpawner.Tests.csproj

tools\
  run-safety-checks.ps1
  collect-bug-logs.ps1

Rôle des fichiers source :

DonJEnemySpawner.cs

C'est le cœur du mod. Il contient :

- la classe principale DonJEnemySpawner : Script ;
- le menu F10 ;
- la logique de placement ;
- les modèles PNJ/véhicules/objets ;
- les armes ;
- les comportements PNJ ;
- les relations ;
- les blips ;
- les sauvegardes XML ;
- le Cartel ;
- les Ballas ;
- les objets interactifs ;
- une grande partie du runtime principal.

DonJEnemySpawner.HighSecurityEscort.cs

Contient toute la partie :

- limousine blindée ;
- convoi VIP ;
- appel avec L ;
- gardes haute sécurité ;
- formation véhicules ;
- trajet waypoint ;
- combat de convoi ;
- IA conducteur ;
- déblocage véhicules ;
- entrée du joueur dans la limousine ;
- nettoyage/retrait du convoi.

Quand on travaille sur la limousine, on modifie principalement ce fichier.

DonJEnemySpawner.Interiors.cs

Contient :

- portails d'entrée ;
- portails de sortie ;
- session intérieure active ;
- téléportation entrée/sortie ;
- sauvegarde des portails.

DonJEnemySpawner.Interiors.AdvancedLoading.cs

Contient :

- chargement avancé IPL/intérieurs ;
- pin interior ;
- focus zone ;
- HD area ;
- room forcing ;
- entity sets ;
- stabilisation caméra/viewport.

DonJEnemySpawner.InteriorCatalog.cs

Contient :

- catalogue des intérieurs disponibles ;
- noms affichés ;
- coordonnées ;
- configurations d'entités intérieures.

DonJEnemySpawner.Logging.cs

Contient :

- logger runtime ;
- écriture dans DonJCustomNpcPlacer.log ;
- fallback vers dossiers accessibles ;
- sanitation des noms de fichiers ;
- protections pour ne jamais crasher à cause du logger.

## 4. Build et déploiement

Le projet principal cible :

net48

La sortie assembly est :

DonJCustomNpcPlacer.dll

Mais le livrable réellement chargé par NIB est :

DonJCustomNpcPlacer.ENdll

Commande build normale :

dotnet build GTA5modDEV.sln -c Release

Commande test normale :

dotnet test GTA5modDEV.sln -c Release

Commande build avec dossier GTA forcé :

dotnet build GTA5modDEV.sln -c Release /p:GtaRoot="C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V Enhanced"

Commande validation complète :

.\tools\run-safety-checks.ps1

Si PowerShell bloque :

powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\run-safety-checks.ps1

Si l'API GTA locale n'est pas disponible mais qu'on veut tester avec stub :

.\tools\run-safety-checks.ps1 -UseStubApi

Sorties attendues après build :

src\DonJEnemySpawner\bin\Release\DonJCustomNpcPlacer.dll
src\DonJEnemySpawner\bin\Release\DonJCustomNpcPlacer.ENdll
src\DonJEnemySpawner\bin\Release\DonJCustomNpcPlacer.pdb

En Release, le .csproj peut déployer automatiquement vers :

$(GtaRoot)\Scripts\DonJCustomNpcPlacer.ENdll

Le .csproj nettoie aussi les anciens noms pour éviter qu'un vieux script se charge en double :

DonJEnemySpawner.dll
DonJEnemySpawner.ENdll
DonJEnemySpawner.pdb

Règle importante : ne jamais livrer seulement le .dll. Le jeu/NIB charge ici le .ENdll.

## 5. Architecture runtime

La classe principale est :

public sealed partial class DonJEnemySpawner : Script

Elle est découpée en plusieurs fichiers avec partial.

Le runtime repose sur :

Tick += OnTick;
KeyDown += OnKeyDown;
Aborted += OnAborted;

OnTick est le cœur vivant du mod. Il doit rester léger. Toute logique coûteuse doit être cadencée.

À éviter absolument dans OnTick :

- scanner tous les PNJ du monde à chaque frame ;
- envoyer TASK_* à chaque frame ;
- créer des allocations LINQ inutiles ;
- appeler ClearAllTasks en boucle ;
- forcer des téléportations visibles ;
- créer/supprimer des entités en masse sans budget ;
- manipuler des entités sans Entity.Exists ;
- écrire dans un fichier à chaque frame.

Les systèmes importants utilisent déjà :

Game.GameTime
intervalles en millisecondes
jitter par handle
dictionnaires de cache
budgets max par tick

Ce pattern doit être conservé.

Exemple conceptuel :

if (Game.GameTime < _nextSomethingAt)
{
    return;
}

_nextSomethingAt = Game.GameTime + IntervalMs;

Pour un groupe de PNJ, on évite de tous les mettre à jour au même moment. On utilise un curseur ou un jitter.

## 6. Règles générales C# / ScriptHook

Le code doit rester compatible :

C#
.NET Framework 4.8
ScriptHookVDotNet API v2
NIBScriptHookVDotNet2.dll

Ne pas utiliser :

- API v3 ;
- async/await qui touche le monde GTA ;
- thread de fond qui manipule Ped/Vehicle/Prop/Blip ;
- classes/méthodes inexistantes en API v2 ;
- dépendances NuGet inutiles ;
- refactor massif hors sujet ;
- renommage de fichiers ou classes sans nécessité.

Chaque accès à une entité doit être protégé :

if (!Entity.Exists(ped) || ped.IsDead)
{
    return;
}

Pour un véhicule :

if (!Entity.Exists(vehicle) || !vehicle.IsDriveable)
{
    return;
}

Pour un handle stocké :

Ped ped = FindPedByHandle(handle);

if (!Entity.Exists(ped))
{
    // nettoyer la référence interne
}

Ne jamais faire confiance à un handle stocké. GTA peut supprimer des entités à tout moment.

## 7. Natives GTA

Quand une fonction n'est pas exposée par l'API v2, le projet utilise des natives GTA.

Pattern existant :

private const ulong NativeSomething = 0x123456789ABCDEF0UL;

Puis appel :

Function.Call((Hash)NativeSomething, arg1, arg2, arg3);

Ou avec retour :

bool result = Function.Call<bool>((Hash)NativeSomething, arg1, arg2);

Règles :

- garder les constantes native en haut du fichier concerné ;
- utiliser ulong ;
- caster vers Hash au moment de l'appel ;
- entourer les natives risquées avec try/catch si elles peuvent varier selon version ;
- ne pas spammer une native lourde à chaque frame ;
- utiliser NativeDB Enhanced pour vérifier les signatures ;
- commenter en français l'intention gameplay.

Exemple de commentaire attendu :

// Je force une route propre vers le waypoint sans réassigner l'ordre à chaque frame.

Style commentaire du projet : en français, souvent à la première personne avec "Je".

## 8. Menu principal F10

Le menu est ouvert/fermé avec :

F10

Constantes stables :

TrainerTitle = "DonJ Custom NPC Placer"
TrainerSubtitle = "Placement propre pour NPC, vehicules et objets"
MenuToggleKey = Keys.F10
MenuToggleKeyLabel = "F10"

Sections principales :

- Placement type ;
- NPC ;
- Vehicle ;
- Object ;
- Interior ;
- Save ;
- Cleanup.

Contrôles menu :

F10 : ouvrir/fermer
Haut/Bas ou NumPad 8/2 : naviguer
Gauche/Droite ou NumPad 4/6 : modifier valeur
Entrée ou NumPad 5 : valider
PageUp/PageDown : scroll rapide
Home/End : début/fin
Esc/Backspace/NumPad 0 : fermer ou retour
T : saisir un modèle custom si le modèle sélectionné est Custom

Types de placement :

private enum PlacementEntityType
{
    Npc,
    Vehicle,
    Object,
    Entrance,
    Exit
}

Comportements PNJ :

private enum NpcBehavior
{
    Static,
    Attacker,
    Neutral,
    Ally,
    Bodyguard,
    NeutralPatrol,
    HostilePatrol,
    AllyPatrol
}

Objets interactifs :

private enum ObjectInteractionKind
{
    None,
    Cash,
    Health,
    Armor,
    Ammo
}

Quand on ajoute une nouvelle option menu :

1. Ajouter l'entrée dans MainMenuAction si nécessaire.
2. Ajouter l'affichage dans BuildMainMenuEntries().
3. Gérer les changements dans ChangeMainMenuValue().
4. Gérer l'action dans ActivateMainMenuItem().
5. Garder la sélection normalisée.
6. Ajouter un test si la constante ou le contrat devient stable.

## 9. Placement PNJ

Le placement PNJ passe par :

TrySpawnNpc
CreatePedFromModelIdentity
RegisterSpawnedNpc
ConfigureSpawnedPed
StartNpcRuntimeBehavior

Un PNJ placé peut avoir :

- modèle ;
- arme ;
- attachments ;
- teinte ;
- munitions ;
- santé ;
- armure ;
- comportement ;
- rayon de patrouille ;
- respawn automatique ;
- blip ;
- relation avec joueur et groupes.

Bornes stables :

Santé : 1 à 5000
Armure : 0 à 200
Distance placement : 25 à 2500, pas de 25
Rayon patrouille : 5 à 500, pas de 5

Règle importante : les modèles doivent être demandés et vérifiés avant création.

Pattern :

Model model = identity.ToModel();

if (!model.IsValid || !model.IsPed)
{
    return null;
}

model.Request(timeout);

if (!model.IsLoaded)
{
    return null;
}

Après création, relâcher le modèle si le code existant le fait dans ce flux :

model.MarkAsNoLongerNeeded();

## 10. Placement véhicules

Le placement véhicule passe par :

TrySpawnVehicle
CreateVehicleFromIdentity
RegisterPlacedVehicle
ConfigurePlacedVehicleEntity

Un véhicule placé peut être :

- persisté dans la scène ;
- sauvegardé XML ;
- respawn automatiquement ;
- nettoyé via menu ;
- utilisé par des bodyguards selon logique IA.

Règles véhicules :

- poser au sol correctement ;
- régler heading ;
- nettoyer proprement les blips ;
- éviter le spam d'upgrades ;
- éviter SetOnGroundProperly en boucle ;
- protéger IsDriveable ;
- ne pas contrôler deux fois le même véhicule avec deux systèmes différents.

Pour un véhicule de convoi, ne jamais envoyer une tâche conducteur chaque frame. Utiliser une cadence :

CartelVehicleOrderIntervalMs
HighSecurityEscortVehicleOrderIntervalMs
EnemyRaidVehicleOrderIntervalMs

## 11. Placement objets

Le placement objet passe par :

TrySpawnObject
CreatePropFromIdentity
RegisterPlacedObject
ConfigurePlacedObjectEntity

Catégories :

Sécurité
Couverture
Argent / butin
Matériel tactique
Soin / survie
Bureau / informatique
Atelier / outils
Mobilier
Caisse / stockage
Décoration
Lumière
Extérieur
Divers

Interactions possibles :

None
Cash
Health
Armor
Ammo

Quand le joueur s'approche d'un objet interactif :

- marker / hint ;
- touche E ;
- gain ou effet ;
- suppression ou désactivation selon logique ;
- sauvegarde compatible si nécessaire.

## 12. Relations et IA

Relations importantes :

RelationshipCompanion = 0
RelationshipNeutral = 3
RelationshipDislike = 4
RelationshipHate = 5

Le projet initialise plusieurs groupes relationnels :

- joueur ;
- alliés ;
- neutres ;
- hostiles ;
- Cartel ;
- Ballas ;
- escortes.

Règle critique : ne pas créer une haine globale contre des groupes GTA ambiants sans garde-fou.

Bon comportement :

- identifier une menace réelle ;
- vérifier distance ;
- vérifier relation ;
- vérifier agression ou tir ;
- appliquer la relation seulement sur les groupes concernés ;
- limiter les refresh relationnels.

Le mod distingue :

- PNJ statique hostile ;
- attaquant ;
- neutre ;
- allié ;
- bodyguard ;
- patrouille neutre ;
- patrouille hostile ;
- patrouille alliée.

Les alliés ne doivent pas spammer TASK_COMBAT_PED. Le mod utilise des caches de menace et des intervalles.

## 13. Système de sauvegarde XML

Dossier principal :

Grand Theft Auto V Enhanced\Scripts\DonJEnemySpawnerSaves

Fallbacks possibles :

Documents\Rockstar Games\GTA V Enhanced\DonJEnemySpawnerSaves
%LOCALAPPDATA%\DonJEnemySpawner\Saves

Variable d'environnement possible :

DONJ_ENEMY_SPAWNER_SAVE_DIR

Fichier marqueur dernière sauvegarde :

_last_save.txt

Les XML contiennent :

- PNJ ;
- modèles custom ;
- armes ;
- attachments ;
- comportements ;
- santé ;
- armure ;
- véhicules ;
- objets ;
- entrées/sorties d'intérieurs ;
- options de respawn ;
- positions ;
- headings ;
- données compatibles anciennes versions.

Règles XML :

- garder CultureInfo.InvariantCulture pour les nombres ;
- ne pas casser les anciens fichiers ;
- accepter les attributs manquants ;
- garder les .bak si le système les utilise ;
- sanitariser les noms de sauvegarde ;
- éviter les chemins arbitraires non contrôlés.

## 14. Respawn automatique

Le respawn automatique permet de recréer :

- PNJ ;
- véhicules ;
- objets.

Constantes importantes :

AutoRespawnCheckIntervalMs = 1000
AutoRespawnMinDelayMs = 6000
AutoRespawnRetryDelayMs = 15000
AutoRespawnMaxPerTick = 3
AutoRespawnLeaveDistance = 220.0f
AutoRespawnNearSafetyDistance = 70.0f

Règles gameplay :

- ne jamais respawn sous les yeux du joueur ;
- attendre que le joueur soit assez loin ;
- éviter de respawn trop proche ;
- limiter le nombre de respawns par tick ;
- réessayer plus tard si le spawn échoue ;
- préserver position, rotation, modèle et options.

## 15. Contacts téléphone

Le téléphone ajoute trois systèmes gameplay :

C : Cartel allié
R : attaque Ballas hostile
L : escorte haute sécurité

Ces touches doivent être actives uniquement dans le contexte prévu, principalement quand le téléphone joueur est ouvert ou quand le système d'escorte demande une validation route.

Le mod vérifie l'état téléphone avec native :

NativeIsPedRunningMobilePhoneTask = 0x2AFE52F782F25775UL

Règles :

- utiliser des latches pour éviter plusieurs appels sur une pression ;
- garder des cooldowns courts mais réels ;
- ne pas relancer une équipe active sans gérer son retrait ;
- afficher un statut clair au joueur ;
- ne pas bloquer les autres systèmes.

## 16. Système Cartel

Contact :

Cartel

Touche :

C

Rôle :

Appeler une équipe alliée de protection.

Configuration actuelle :

11 gardes
3 véhicules
500 santé
200 armure
Service Carbine + Machine Pistol
spawn entre 68 m et 118 m
conduite professionnelle
combat drive-by ou à pied
retrait propre si rappel

Le Cartel :

- protège le joueur ;
- suit à pied si joueur à pied ;
- suit en véhicule si joueur en véhicule ;
- engage les menaces réelles ;
- peut tirer depuis les véhicules ;
- descend si menace proche ;
- rejoint les véhicules si nécessaire ;
- se retire proprement quand rappelé.

Règle performance :

Le Cartel ne doit pas scanner tout le monde trop souvent.
Il utilise des caches de menace et un nombre limité de scans par passe.

## 17. Système Ballas

Contact :

Ballas

Touche :

R

Rôle :

Créer une attaque hostile dynamique autour du joueur.

Configuration actuelle :

4 à 12 ennemis par appel
max 36 ennemis actifs
max 4 véhicules
100 santé
100 armure
spawn entre 72 m et 130 m
arrivée en véhicules
drive-by puis combat à pied
nettoyage post-combat
restauration après mort du joueur

Les Ballas :

- sont hostiles ;
- arrivent en véhicules ;
- tirent en drive-by ;
- descendent pour combattre ;
- sont nettoyés après combat ;
- peuvent être reconstruits après mort du joueur si GTA les supprime.

Règles :

- ne pas laisser des blips rouges véhicules après combat terminé ;
- ne pas supprimer un véhicule visible immédiatement ;
- nettoyer quand joueur s'éloigne ou ne regarde plus ;
- limiter le nombre d'actifs.

## 18. Système escorte haute sécurité / limousine blindée

Fichier principal :

src\DonJEnemySpawner\DonJEnemySpawner.HighSecurityEscort.cs

Contact :

Escorte haute sécurité

Touche :

L

Rôle :

Créer un convoi VIP allié avec limousine blindée et véhicules d'escorte.

Configuration actuelle de base :

1 limousine blindée
4 Baller noirs
gardes Cartel renforcés
500 santé
200 armure
Service Carbine + Machine Pistol
IA dédiée convoi
trajet waypoint
combat de protection
retrait propre

Modes internes :

HighSecurityEscortModeNone
HighSecurityEscortModeArriving
HighSecurityEscortModeStandby
HighSecurityEscortModeConvoyRoute
HighSecurityEscortModeFootFollow
HighSecurityEscortModePlayerVehicleFollow
HighSecurityEscortModeDismissing

Rôles véhicules :

HighSecurityEscortVehicleRoleLimousine = -100
HighSecurityEscortVehicleRoleFrontLeft = 0
HighSecurityEscortVehicleRoleFrontRight = 1
HighSecurityEscortVehicleRoleRearLeft = 2
HighSecurityEscortVehicleRoleRearRight = 3

Même si les noms historiques disent "FrontLeft/FrontRight", la logique peut être adaptée pour faire une file propre derrière la limousine.

Flux gameplay :

1. Le joueur ouvre le téléphone.
2. Il appuie sur L.
3. Le convoi spawn hors champ, si possible sur route.
4. La limousine arrive près du joueur.
5. Le joueur monte à l'arrière avec F.
6. Le joueur place un waypoint.
7. Le joueur appuie sur L dans la limousine.
8. Le convoi part vers le waypoint.
9. Les Baller suivent et protègent.
10. En cas d'attaque, les gardes réagissent.
11. À destination, le convoi repasse en standby.
12. Si le joueur rappelle/rejette, le convoi se retire.

Règles importantes pour travailler sur la limousine :

- ne pas casser la conduite existante ;
- ne pas spammer TASK_VEHICLE_DRIVE_TO_COORD ;
- garder les véhicules route-based ;
- éviter les spawns visibles ;
- éviter les téléportations visibles ;
- garder la place joueur libre ;
- protéger le chauffeur ;
- garder l'entrée F assistée ;
- bloquer les bugs où la limousine écrase le joueur ;
- ne pas envoyer tout le convoi sur la même coordonnée ;
- faire des offsets propres en file ou formation ;
- garder un fallback si aucun node route n'est trouvé.

Bon pattern convoi :

- trouver un point de route hors champ ;
- calculer une direction d'approche ;
- placer la limousine devant ;
- placer les Baller derrière avec un spacing stable ;
- snapper sur vehicle nodes si possible ;
- garder une hauteur Z sûre ;
- donner un heading cohérent ;
- enregistrer chaque véhicule avec son rôle ;
- donner des ordres de conduite cadencés.

Combat escorte :

- menace scannée avec cache ;
- passagers tirent depuis véhicule si possible ;
- descente seulement si menace proche ou véhicule bloqué ;
- limousine garde priorité route si joueur dedans ;
- Baller peuvent devenir plus agressifs ;
- conducteurs reçoivent un style adapté ;
- les gardes reviennent au véhicule si situation calme.

Déblocage véhicule :

- soft unstuck après quelques secondes bloqué ;
- petite marche arrière possible ;
- hard rescue seulement si très loin ou hors champ ;
- jamais de téléportation visible ;
- ne pas repositionner une entité que le joueur regarde de près.

## 19. Conduite IA GTA

Le projet utilise des styles de conduite numériques.

Constante générale :

private const int ProfessionalDrivingStyle = 786603;

Dans l'escorte :

HighSecurityEscortFastTaxiDrivingStyle = 786469
HighSecurityEscortCombatDrivingStyle = 2883621

Principe :

- conduite normale : propre, route, évitement, trafic ;
- conduite rapide : taxi pressé, dépassements, moins de respect feux ;
- conduite combat : plus agressive mais à limiter aux menaces.

GTA ne donne pas toujours une lecture propre des panneaux de vitesse via ScriptHookVDotNet v2. Pour simuler une conduite respectueuse, il faut régler :

- driving style ;
- vitesse max ;
- fréquence de retask ;
- distance d'arrivée ;
- target route node ;
- comportement proche destination.

Ne pas utiliser des vitesses délirantes. Dans GTA, une vitesse scriptée trop haute rend les véhicules violents et instables.

Bonnes pratiques :

- arrivée convoi : vitesse modérée ;
- route VIP normale : vitesse taxi réaliste ;
- urgence : vitesse plus haute mais pas projectile ;
- combat : vitesse plus agressive mais toujours contrôlée ;
- proche destination : limiter vitesse ;
- proche joueur : limiter fortement.

## 20. Intérieurs

Les intérieurs sont gérés dans :

DonJEnemySpawner.Interiors.cs
DonJEnemySpawner.Interiors.AdvancedLoading.cs
DonJEnemySpawner.InteriorCatalog.cs

Le mod peut placer :

- une entrée ;
- une sortie ;
- un couple entrée/sortie ;
- des portails persistants sauvegardés XML.

Le chargement avancé peut utiliser :

- SET_FOCUS_POS_AND_VEL ;
- SET_HD_AREA ;
- NEW_LOAD_SCENE_START ;
- GET_INTERIOR_AT_COORDS ;
- PIN_INTERIOR_IN_MEMORY ;
- ACTIVATE_INTERIOR_ENTITY_SET ;
- REFRESH_INTERIOR ;
- FORCE_ROOM_FOR_ENTITY ;
- FORCE_ROOM_FOR_GAME_VIEWPORT.

Règles :

- téléporter avec une petite marge Z ;
- stabiliser collision et viewport ;
- éviter d'enfermer le joueur dans un intérieur non prêt ;
- garder un cooldown portail ;
- sauvegarder les portails ;
- nettoyer focus/HD area quand session terminée.

## 21. Logging runtime

Fichier log runtime :

DonJCustomNpcPlacer.log

Le logger doit :

- ne jamais crasher le mod ;
- sanitariser les noms ;
- trouver un dossier writable ;
- écrire des messages courts ;
- garder stack trace utile en cas d'exception ;
- être utilisé pour erreurs importantes, pas pour chaque frame.

Le fichier :

DonJEnemySpawner.Logging.cs

contient les helpers.

Règle :

Si une erreur peut être utile pour debug mais ne doit pas casser le jeu, logger puis continuer proprement.

## 22. Collecte de bugs

Script :

tools\collect-bug-logs.ps1

Il collecte :

- logs GTA ;
- logs NIB ;
- logs ScriptHookV ;
- logs Scripts ;
- DirectStorageFix.log ;
- menyooLog.txt ;
- MapEditor.log ;
- événements Application Windows ;
- état Git ;
- résumé ;
- manifest JSON ;
- entrée prête pour crash-list.md.

Commande type :

powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\collect-bug-logs.ps1 -Title "bug-limousine" -SinceHours 24

Avec dossier GTA forcé :

powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\collect-bug-logs.ps1 -Title "bug-limousine" -SinceHours 24 -GtaRoot "C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V Enhanced"

Chaque bug/crash doit être documenté dans :

crash-list.md

Chaque entrée doit contenir :

- date ;
- statut ;
- contexte ;
- symptôme ;
- sources vérifiées ;
- extraits utiles ;
- analyse/hypothèse ;
- action menée ;
- vérification ;
- résolution.

## 23. Tests

Projet tests :

tests\DonJEnemySpawner.Tests

Types de tests :

DonJEnemySpawnerTests.cs

Teste surtout :

- constantes stables ;
- contrats menu ;
- contrats Cartel ;
- contrats Ballas ;
- contrats escorte ;
- XML ;
- compatibilité hash ;
- structure attendue.

SafetySimulationTests.cs

Teste :

- scénarios simulés hors jeu ;
- comportements menu ;
- garde-fous ;
- logique testable sans lancer GTA.

BugLogCollectionTests.cs

Teste :

- collecteur de logs ;
- génération bug-reports ;
- fallback logger ;
- scripts PowerShell.

Règle :

Quand on change une constante stable, il faut mettre à jour le test correspondant.
Quand on ajoute une fonctionnalité testable hors jeu, il faut ajouter un test.
Ne jamais supprimer un test juste pour faire passer la suite.

## 24. Performance et stabilité

Le mod tourne en jeu. Il faut donc penser FPS et stabilité.

Règles strictes :

- OnTick léger.
- Pas de scans monde permanents.
- Pas d'ordres IA chaque frame.
- Pas de LINQ dans les chemins très fréquents si évitable.
- Pas de création d'objets inutiles à chaque tick.
- Pas d'écriture disque à chaque tick.
- Pas de thread qui touche GTA.
- Pas de téléportation visible sauf cas assumé et hors champ.
- Pas de suppression visible brutale si le joueur regarde.
- Toujours nettoyer handles, blips, dictionnaires.

Bon pattern :

if (Game.GameTime < nextUpdateAt)
{
    return;
}

nextUpdateAt = Game.GameTime + intervalMs + jitter;

Pour les groupes :

- curseur de scan ;
- max N entités par passe ;
- cache menace ;
- cache dernière position ;
- cache dernière tâche ;
- retask seulement si target a changé ou délai expiré.

## 25. Blips et UI

Les blips doivent être :

- créés seulement si nécessaire ;
- supprimés au cleanup ;
- refresh avec intervalle ;
- pas recréés chaque frame ;
- cohérents par couleur/type.

Les messages joueur utilisent :

ShowStatus(...)

Règle :

Un message doit expliquer une action gameplay, pas spammer.

Exemples corrects :

Escorte haute sécurité appelée.
Monte à l'arrière avec F.
Waypoint introuvable.
Convoi en route.
Mode urgence activé.

## 26. Ce que le mod peut faire techniquement

Avec ScriptHook/NIBScriptHookVDotNet v2 et les natives GTA, le mod peut faire beaucoup de choses en solo :

Créer des PNJ.
Créer des véhicules.
Créer des objets.
Placer des entités précisément.
Donner des armes.
Ajouter des composants d'armes.
Changer santé/armure.
Créer des relations entre groupes.
Créer des blips.
Afficher des markers.
Afficher du texte/status.
Gérer des menus.
Lire les touches clavier.
Détecter téléphone ouvert.
Détecter waypoint.
Téléporter joueur/entités.
Demander modèles.
Supprimer entités.
Commander IA : marcher, suivre, combattre, entrer véhicule, conduire.
Créer drive-by.
Faire patrouiller.
Faire garder une zone.
Forcer des scénarios simples.
Sauvegarder/charger XML.
Lire/écrire fichiers.
Charger certains intérieurs.
Appeler des natives non exposées.
Gérer respawn automatique.
Créer des convois.
Nettoyer les entités hors champ.
Collecter logs côté dev.

Ce que le mod ne doit pas faire ou ne peut pas garantir proprement :

Fonctionner en GTA Online.
Utiliser des API v3 inexistantes côté runtime actuel.
Lire parfaitement tous les panneaux de vitesse.
Garantir une pathfinding parfaite dans tous les cas.
Contrôler la circulation GTA à 100 %.
Empêcher tous les bugs d'IA natifs.
Téléporter sous les yeux du joueur sans casser l'immersion.
Manipuler le monde GTA depuis un thread externe.
Faire confiance aux handles stockés.

## 27. Guide de modification pour Codex

Avant chaque modification :

1. Lire AGENTS.md.
2. Vérifier git status --short.
3. Ne jamais écraser les changements utilisateur.
4. Lire les fichiers concernés.
5. Identifier précisément le système touché.
6. Faire un changement limité au besoin.
7. Ajouter tests si possible.
8. Relire le code.
9. Lancer build/tests/safety si environnement disponible.
10. Signaler clairement ce qui n'a pas pu être vérifié.

Commandes utiles :

git status --short
rg "HighSecurityEscort" src
rg "Cartel" src
rg "EnemyRaid" src
rg "ShowStatus" src
dotnet build GTA5modDEV.sln -c Release
dotnet test GTA5modDEV.sln -c Release
.\tools\run-safety-checks.ps1

Pour un changement limousine :

Fichier principal :
src\DonJEnemySpawner\DonJEnemySpawner.HighSecurityEscort.cs

Lire avant modification :

- constantes HighSecurityEscort ;
- SpawnHighSecurityEscortConvoy ;
- UpdateHighSecurityEscortState ;
- OrderHighSecurityEscortArrivalToPlayer ;
- OrderHighSecurityConvoyToDestination ;
- CalculateHighSecurityFormationTarget ;
- CalculateHighSecurityArrivalTarget ;
- UpdateHighSecurityEscortCombat ;
- CommandHighSecurityEscortGuardEnterAssignedVehicle ;
- CleanupHighSecurityEscort ;

À protéger :

- place joueur dans limousine ;
- chauffeur ;
- mode arrivée ;
- mode route ;
- mode dismiss ;
- caches d'ordres véhicules ;
- handles gardes/véhicules ;
- blips ;
- anti-spam téléphone ;
- logs ;
- compatibilité API v2.

## 28. Style de code attendu

Commentaires :

- français ;
- clairs ;
- pas de roman inutile ;
- expliquer l'intention gameplay ;
- style existant : "Je ...".

Exemple :

// Je cadence l'ordre conducteur pour éviter de casser l'IA native à chaque frame.

Noms :

- garder les préfixes existants ;
- HighSecurityEscort... pour la limousine ;
- Cartel... pour le Cartel ;
- EnemyRaid... pour Ballas ;
- Interior... pour portails ;
- AdvancedInterior... pour chargement avancé.

Ne pas mélanger les systèmes. Exemple : ne pas mettre une logique limousine dans DonJEnemySpawner.cs si elle appartient à DonJEnemySpawner.HighSecurityEscort.cs.

## 29. Checklist avant livraison

Avant de livrer un patch :

- Le changement répond exactement à la demande.
- Pas de refactor hors sujet.
- Pas de fichier généré modifié inutilement.
- Pas de binaire modifié sauf demande explicite.
- Pas de vieux DonJEnemySpawner.ENdll réintroduit.
- Le build produit DonJCustomNpcPlacer.ENdll.
- Les tests passent ou l'échec est expliqué.
- Les limites sont honnêtement indiquées.
- Les risques en jeu sont expliqués.

Commandes finales idéales :

dotnet build GTA5modDEV.sln -c Release
dotnet test GTA5modDEV.sln -c Release
.\tools\run-safety-checks.ps1

Si l'environnement n'a pas GTA/NIB local :

.\tools\run-safety-checks.ps1 -UseStubApi

## 30. Prompt prêt à donner à Codex

Tu peux donner ce bloc à Codex avec le projet :

Tu travailles sur le projet GTA5modDEV / DonJ Custom NPC Placer.

Lis d'abord AGENTS.md, README.md et les fichiers source concernés avant toute modification.

Contexte cible :
- GTA V Enhanced Steam Windows x64.
- GTA5_Enhanced.exe 1.0.1013.34.
- ScriptHookV.dll 3788.0.1013.34.
- Runtime .NET côté jeu : NIBScriptHookVDotNet.asi + NIBScriptHookVDotNet2.dll 2.11.6.
- API cible : ScriptHookVDotNet API v2 via NIBScriptHookVDotNet2.dll.
- Ne pas utiliser API v3.
- Projet C# .NET Framework 4.8.
- Livrable chargé par le jeu : DonJCustomNpcPlacer.ENdll.
- Dossier scripts GTA : Grand Theft Auto V Enhanced\Scripts.

Architecture :
- src/DonJEnemySpawner/DonJEnemySpawner.cs : cœur du mod, menu, placements, sauvegardes, PNJ, véhicules, objets, Cartel/Ballas.
- src/DonJEnemySpawner/DonJEnemySpawner.HighSecurityEscort.cs : limousine blindée, convoi haute sécurité, trajet VIP, combat convoi.
- src/DonJEnemySpawner/DonJEnemySpawner.Interiors.cs : portails d'intérieurs.
- src/DonJEnemySpawner/DonJEnemySpawner.Interiors.AdvancedLoading.cs : chargement avancé des intérieurs.
- src/DonJEnemySpawner/DonJEnemySpawner.InteriorCatalog.cs : catalogue intérieur.
- src/DonJEnemySpawner/DonJEnemySpawner.Logging.cs : logs runtime.
- tests/DonJEnemySpawner.Tests : tests MSTest.
- tools/run-safety-checks.ps1 : validation build/tests/livrable.
- tools/collect-bug-logs.ps1 : collecte logs bug/crash.

Règles :
- Ne jamais modifier hors sujet.
- Ne jamais écraser les changements utilisateur.
- Ne jamais manipuler Ped/Vehicle/Prop/Blip sans Entity.Exists.
- Ne jamais spammer TASK_* dans OnTick.
- Cadencer les IA avec Game.GameTime.
- Garder OnTick léger.
- Ne pas téléporter sous les yeux du joueur.
- Garder les commentaires en français.
- Ajouter ou ajuster les tests si un contrat stable change.
- Après modification, lancer dotnet build, dotnet test et si possible tools/run-safety-checks.ps1.
- Si une commande ne peut pas être lancée, expliquer précisément pourquoi.

Pour la limousine/convoi :
- Travailler principalement dans DonJEnemySpawner.HighSecurityEscort.cs.
- Garder la place joueur libre.
- Garder la conduite route-based.
- Spawn hors champ et propre.
- Pas de TP visible.
- Pas d'ordres véhicules à chaque frame.
- Préserver la conduite actuelle si elle fonctionne.
- Toute conduite agressive doit rester contrôlée et limitée au mode urgence/combat.
