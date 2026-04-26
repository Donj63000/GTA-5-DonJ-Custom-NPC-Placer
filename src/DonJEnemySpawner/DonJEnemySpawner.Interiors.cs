using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Xml;
using GTA;
using GTA.Math;
using GTA.Native;

public sealed partial class DonJEnemySpawner
{
    private const float InteriorPortalMarkerRadius = 1.15f;
    private const float InteriorPortalActivationRadius = 1.05f;
    private const float InteriorPortalActivationZTolerance = 2.25f;
    private const float InteriorPortalDrawDistance = 180.0f;
    private const float InteriorPortalExitPlacementMaxDistance = 550.0f;
    private const int InteriorPortalCooldownMs = 1800;
    private const int InteriorPortalHintCooldownMs = 2500;

    private readonly List<InteriorCategory> _interiorCategories = BuildInteriorCategories();
    private readonly List<PlacedInteriorPortal> _placedInteriorPortals = new List<PlacedInteriorPortal>();

    private int _selectedInteriorCategoryIndex;
    private int _selectedInteriorIndexInCategory;
    private ActiveInteriorSession _activeInteriorSession;
    private int _nextInteriorPortalUseAllowedAt;
    private int _nextInteriorPortalHintAt;

    private enum InteriorPortalKind
    {
        Entrance,
        Exit
    }

    private sealed class InteriorCategory
    {
        public string Name;
        public List<InteriorOption> Options;
    }

    private sealed class InteriorOption
    {
        public string Id;
        public string Category;
        public string DisplayName;
        public Vector3 Position;
        public float Heading;
        public List<string> Ipls;
    }

    private sealed class PlacedInteriorPortal
    {
        public string Id;
        public InteriorPortalKind Kind;
        public InteriorOption Interior;
        public Vector3 Position;
        public float Heading;
        public Vector3 FallbackReturnPosition;
        public float FallbackReturnHeading;
        public string SourceEntranceId;
    }

    private sealed class ActiveInteriorSession
    {
        public string EntrancePortalId;
        public string InteriorId;
        public InteriorOption Interior;
        public Vector3 ReturnPosition;
        public float ReturnHeading;
        public int StartedAt;
    }

    private InteriorCategory CurrentInteriorCategory()
    {
        if (_interiorCategories == null || _interiorCategories.Count == 0)
        {
            return new InteriorCategory
            {
                Name = "Aucun interieur",
                Options = new List<InteriorOption>()
            };
        }

        _selectedInteriorCategoryIndex = Clamp(_selectedInteriorCategoryIndex, 0, _interiorCategories.Count - 1);
        return _interiorCategories[_selectedInteriorCategoryIndex];
    }

    private InteriorOption CurrentInteriorOption()
    {
        InteriorCategory category = CurrentInteriorCategory();

        if (category.Options == null || category.Options.Count == 0)
        {
            return new InteriorOption
            {
                Id = "bunker_generic",
                Category = category.Name,
                DisplayName = "Bunker interieur",
                Position = new Vector3(899.5518f, -3246.038f, -98.04907f),
                Heading = 0.0f,
                Ipls = new List<string>()
            };
        }

        _selectedInteriorIndexInCategory = Wrap(_selectedInteriorIndexInCategory, category.Options.Count);
        return category.Options[_selectedInteriorIndexInCategory];
    }

    private void ChangeInteriorCategory(int direction)
    {
        if (_interiorCategories == null || _interiorCategories.Count == 0)
        {
            return;
        }

        _selectedInteriorCategoryIndex = Wrap(_selectedInteriorCategoryIndex + direction, _interiorCategories.Count);
        _selectedInteriorIndexInCategory = 0;
        DeletePlacementPreview();
    }

    private void ChangeInterior(int direction)
    {
        InteriorCategory category = CurrentInteriorCategory();

        if (category.Options == null || category.Options.Count == 0)
        {
            return;
        }

        _selectedInteriorIndexInCategory = Wrap(_selectedInteriorIndexInCategory + direction, category.Options.Count);
        DeletePlacementPreview();
    }

    private string PlacementSlotCategoryLabel()
    {
        switch (_selectedPlacementType)
        {
            case PlacementEntityType.Entrance:
                return "Categorie interieur";

            case PlacementEntityType.Exit:
                return "Sortie active";

            case PlacementEntityType.Object:
            case PlacementEntityType.Vehicle:
            case PlacementEntityType.Npc:
            default:
                return "Categorie objet";
        }
    }

    private string PlacementSlotCategoryValue()
    {
        switch (_selectedPlacementType)
        {
            case PlacementEntityType.Entrance:
                return CurrentInteriorCategory().Name;

            case PlacementEntityType.Exit:
                return _activeInteriorSession != null && _activeInteriorSession.Interior != null
                    ? _activeInteriorSession.Interior.DisplayName
                    : "Aucune entree active";

            case PlacementEntityType.Object:
            case PlacementEntityType.Vehicle:
            case PlacementEntityType.Npc:
            default:
                return CurrentObjectCategory().Name;
        }
    }

    private string PlacementSlotOptionLabel()
    {
        switch (_selectedPlacementType)
        {
            case PlacementEntityType.Entrance:
                return "Interieur";

            case PlacementEntityType.Exit:
                return "Destination sortie";

            case PlacementEntityType.Object:
            case PlacementEntityType.Vehicle:
            case PlacementEntityType.Npc:
            default:
                return "Objet";
        }
    }

    private string PlacementSlotOptionValue()
    {
        switch (_selectedPlacementType)
        {
            case PlacementEntityType.Entrance:
                return CurrentInteriorOption().DisplayName;

            case PlacementEntityType.Exit:
                return _activeInteriorSession != null
                    ? "Retour au marqueur d'entree"
                    : "Entre d'abord par une Entree";

            case PlacementEntityType.Object:
            case PlacementEntityType.Vehicle:
            case PlacementEntityType.Npc:
            default:
                return CurrentObjectDisplayName();
        }
    }

    private void ChangePlacementSlotCategory(int direction)
    {
        if (_selectedPlacementType == PlacementEntityType.Entrance)
        {
            ChangeInteriorCategory(direction);
            return;
        }

        if (_selectedPlacementType == PlacementEntityType.Exit)
        {
            ShowStatus("La sortie utilise automatiquement l'interieur dans lequel tu es entre.", 2500);
            return;
        }

        ChangeObjectCategory(direction);
    }

    private void ChangePlacementSlotOption(int direction)
    {
        if (_selectedPlacementType == PlacementEntityType.Entrance)
        {
            ChangeInterior(direction);
            return;
        }

        if (_selectedPlacementType == PlacementEntityType.Exit)
        {
            ShowStatus("Place la sortie dans l'interieur actif: elle ramenera au point d'entree.", 3000);
            return;
        }

        ChangeObject(direction);
    }

    private bool TryPlaceInteriorEntrance(Vector3 requestedPosition, Vector3 surfaceNormal, bool precise, bool hasHeadingOverride, float headingOverride)
    {
        Ped player = Game.Player.Character;

        if (!Entity.Exists(player) || player.IsDead)
        {
            ShowStatus("Entree annulee: joueur invalide.", 3000);
            return false;
        }

        InteriorOption interior = CurrentInteriorOption();

        if (!IsInteriorOptionValid(interior))
        {
            ShowStatus("Interieur invalide: selectionne un interieur valide.", 3500);
            return false;
        }

        Vector3 position = AdjustInteriorPortalSpawnPosition(requestedPosition, surfaceNormal, precise);
        float heading = hasHeadingOverride
            ? NormalizeHeading(headingOverride)
            : HeadingFromTo(position, player.Position);

        RegisterInteriorEntrance(position, heading, interior, true);
        return true;
    }

    private bool TryPlaceInteriorExit(Vector3 requestedPosition, Vector3 surfaceNormal, bool precise, bool hasHeadingOverride, float headingOverride)
    {
        Ped player = Game.Player.Character;

        if (!Entity.Exists(player) || player.IsDead)
        {
            ShowStatus("Sortie annulee: joueur invalide.", 3000);
            return false;
        }

        if (!CanPlaceInteriorExit(player.Position, true))
        {
            return false;
        }

        Vector3 position = AdjustInteriorPortalSpawnPosition(requestedPosition, surfaceNormal, precise);

        if (!IsPointInsideActiveInterior(position))
        {
            ShowStatus("Sortie refusee: place-la dans l'interieur actif, pas dehors.", 3500);
            return false;
        }

        float heading = hasHeadingOverride
            ? NormalizeHeading(headingOverride)
            : HeadingFromTo(position, player.Position);

        RegisterInteriorExit(position, heading, true);
        return true;
    }

    private void ConfirmInteriorEntrancePlacementSpawn()
    {
        TryPlaceInteriorEntrance(_placementSpawnPoint, _placementSurfaceNormal, true, true, _placementHeading);
    }

    private void ConfirmInteriorExitPlacementSpawn()
    {
        TryPlaceInteriorExit(_placementSpawnPoint, _placementSurfaceNormal, true, true, _placementHeading);
    }

    private Vector3 AdjustInteriorPortalSpawnPosition(Vector3 requestedPosition, Vector3 surfaceNormal, bool precise)
    {
        if (!precise)
        {
            return AdjustDistanceSpawnPosition(requestedPosition);
        }

        Vector3 normal = Normalize(surfaceNormal);

        if (normal.Length() < 0.001f)
        {
            normal = new Vector3(0.0f, 0.0f, 1.0f);
        }

        if (normal.Z > 0.35f)
        {
            return requestedPosition + new Vector3(0.0f, 0.0f, 0.05f);
        }

        return requestedPosition + normal * 0.15f;
    }

    private PlacedInteriorPortal RegisterInteriorEntrance(Vector3 position, float heading, InteriorOption interior, bool showStatus)
    {
        PlacedInteriorPortal portal = new PlacedInteriorPortal
        {
            Id = Guid.NewGuid().ToString("N"),
            Kind = InteriorPortalKind.Entrance,
            Interior = CloneInteriorOption(interior),
            Position = position,
            Heading = NormalizeHeading(heading),
            FallbackReturnPosition = BuildReturnPositionFromEntrance(position, heading),
            FallbackReturnHeading = NormalizeHeading(heading),
            SourceEntranceId = string.Empty
        };

        _placedInteriorPortals.Add(portal);

        if (showStatus)
        {
            ShowStatus("Entree placee: " + interior.DisplayName + ". Marche sur le marqueur bleu pour entrer.", 4500);
        }

        return portal;
    }

    private PlacedInteriorPortal RegisterInteriorExit(Vector3 position, float heading, bool showStatus)
    {
        if (_activeInteriorSession == null || _activeInteriorSession.Interior == null)
        {
            if (showStatus)
            {
                ShowStatus("Sortie impossible: entre d'abord par une Entree.", 3500);
            }

            return null;
        }

        PlacedInteriorPortal portal = new PlacedInteriorPortal
        {
            Id = Guid.NewGuid().ToString("N"),
            Kind = InteriorPortalKind.Exit,
            Interior = CloneInteriorOption(_activeInteriorSession.Interior),
            Position = position,
            Heading = NormalizeHeading(heading),
            FallbackReturnPosition = _activeInteriorSession.ReturnPosition,
            FallbackReturnHeading = _activeInteriorSession.ReturnHeading,
            SourceEntranceId = _activeInteriorSession.EntrancePortalId ?? string.Empty
        };

        _placedInteriorPortals.Add(portal);

        if (showStatus)
        {
            ShowStatus("Sortie placee: retour vers le point d'entree actif.", 4500);
        }

        return portal;
    }

    private bool CanPlaceInteriorExit(Vector3 currentPlayerPosition, bool showStatus)
    {
        if (_activeInteriorSession == null || _activeInteriorSession.Interior == null)
        {
            if (showStatus)
            {
                ShowStatus("Sortie impossible: entre d'abord par une Entree puis place la sortie dans cet interieur.", 4500);
            }

            return false;
        }

        if (!IsPointInsideActiveInterior(currentPlayerPosition))
        {
            if (showStatus)
            {
                ShowStatus("Sortie impossible: tu n'es plus dans l'interieur actif.", 3500);
            }

            return false;
        }

        return true;
    }

    private bool IsPointInsideActiveInterior(Vector3 position)
    {
        if (_activeInteriorSession == null || _activeInteriorSession.Interior == null)
        {
            return false;
        }

        return position.DistanceTo(_activeInteriorSession.Interior.Position) <= InteriorPortalExitPlacementMaxDistance;
    }

    private void UpdateInteriorPortals()
    {
        if (_placedInteriorPortals.Count == 0)
        {
            return;
        }

        Ped player = Game.Player.Character;

        if (!Entity.Exists(player) || player.IsDead)
        {
            return;
        }

        MaintainActiveInteriorVisualsSafe(player);

        Vector3 playerPosition = player.Position;

        for (int i = _placedInteriorPortals.Count - 1; i >= 0; i--)
        {
            PlacedInteriorPortal portal = _placedInteriorPortals[i];

            if (portal == null || portal.Interior == null)
            {
                _placedInteriorPortals.RemoveAt(i);
                continue;
            }

            DrawInteriorPortalMarker(portal, playerPosition);
        }

        if (Game.GameTime < _nextInteriorPortalUseAllowedAt)
        {
            return;
        }

        for (int i = 0; i < _placedInteriorPortals.Count; i++)
        {
            PlacedInteriorPortal portal = _placedInteriorPortals[i];

            if (portal == null || !IsPlayerInsideInteriorPortal(playerPosition, portal))
            {
                continue;
            }

            if (player.IsInVehicle())
            {
                ShowInteriorPortalHint("Descends du vehicule pour utiliser ce marqueur.", 2500);
                _nextInteriorPortalUseAllowedAt = Game.GameTime + 650;
                break;
            }

            if (portal.Kind == InteriorPortalKind.Entrance)
            {
                EnterInteriorPortal(portal, player);
            }
            else
            {
                ExitInteriorPortal(portal, player);
            }

            break;
        }
    }

    private void DrawInteriorPortalMarker(PlacedInteriorPortal portal, Vector3 playerPosition)
    {
        if (portal.Position.DistanceTo(playerPosition) > InteriorPortalDrawDistance)
        {
            return;
        }

        Color color = portal.Kind == InteriorPortalKind.Entrance
            ? Color.FromArgb(175, 40, 150, 255)
            : Color.FromArgb(185, 255, 190, 40);

        World.DrawMarker(
            MarkerType.VerticalCylinder,
            portal.Position + new Vector3(0.0f, 0.0f, 0.04f),
            Vector3.Zero,
            Vector3.Zero,
            new Vector3(InteriorPortalMarkerRadius, InteriorPortalMarkerRadius, 0.30f),
            color);

        World.DrawMarker(
            MarkerType.DebugSphere,
            portal.Position + new Vector3(0.0f, 0.0f, 0.82f),
            Vector3.Zero,
            Vector3.Zero,
            new Vector3(0.16f, 0.16f, 0.16f),
            Color.FromArgb(230, 255, 255, 255));
    }

    private bool IsPlayerInsideInteriorPortal(Vector3 playerPosition, PlacedInteriorPortal portal)
    {
        float dx = playerPosition.X - portal.Position.X;
        float dy = playerPosition.Y - portal.Position.Y;
        float dz = Math.Abs(playerPosition.Z - portal.Position.Z);

        return dx * dx + dy * dy <= InteriorPortalActivationRadius * InteriorPortalActivationRadius &&
               dz <= InteriorPortalActivationZTolerance;
    }

    private void EnterInteriorPortal(PlacedInteriorPortal portal, Ped player)
    {
        if (!IsInteriorOptionValid(portal.Interior))
        {
            ShowStatus("Entree invalide: interieur absent.", 3500);
            _nextInteriorPortalUseAllowedAt = Game.GameTime + InteriorPortalCooldownMs;
            return;
        }

        _activeInteriorSession = new ActiveInteriorSession
        {
            EntrancePortalId = portal.Id,
            InteriorId = portal.Interior.Id,
            Interior = CloneInteriorOption(portal.Interior),
            ReturnPosition = BuildReturnPositionFromEntrance(portal.Position, portal.Heading),
            ReturnHeading = NormalizeHeading(portal.Heading),
            StartedAt = Game.GameTime
        };

        bool prepared = PrepareInteriorForTeleportSafe(portal.Interior);

        if (!prepared)
        {
            ShowStatus("Interieur charge partiellement: TP quand meme, mais certains assets peuvent encore arriver.", 4500);
        }

        TeleportPlayerWithFadeSafe(player, portal.Interior.Position, portal.Interior.Heading);
        ApplyInteriorEntitySetsSafe(portal.Interior);

        _nextInteriorPortalUseAllowedAt = Game.GameTime + InteriorPortalCooldownMs;
        ShowStatus("Interieur: " + portal.Interior.DisplayName + ". Place une Sortie ici si besoin.", 5000);
    }

    private void ExitInteriorPortal(PlacedInteriorPortal portal, Ped player)
    {
        if (_activeInteriorSession == null || _activeInteriorSession.Interior == null)
        {
            ShowInteriorPortalHint("Sortie inactive: entre d'abord par une Entree de ce mod.", 3500);
            _nextInteriorPortalUseAllowedAt = Game.GameTime + 900;
            return;
        }

        if (!string.Equals(portal.Interior.Id, _activeInteriorSession.InteriorId, StringComparison.OrdinalIgnoreCase))
        {
            ShowInteriorPortalHint("Cette sortie appartient a un autre interieur actif.", 3500);
            _nextInteriorPortalUseAllowedAt = Game.GameTime + 900;
            return;
        }

        Vector3 returnPosition = _activeInteriorSession.ReturnPosition;
        float returnHeading = _activeInteriorSession.ReturnHeading;

        if (IsZeroVector(returnPosition))
        {
            returnPosition = portal.FallbackReturnPosition;
            returnHeading = portal.FallbackReturnHeading;
        }

        if (IsZeroVector(returnPosition))
        {
            ShowStatus("Sortie impossible: point de retour invalide.", 3500);
            _nextInteriorPortalUseAllowedAt = Game.GameTime + 900;
            return;
        }

        TeleportPlayerWithFadeSafe(player, returnPosition, returnHeading);
        _activeInteriorSession = null;
        _nextInteriorPortalUseAllowedAt = Game.GameTime + InteriorPortalCooldownMs;
        ShowStatus("Retour au point d'entree.", 3500);
    }

    private void RequestInteriorAssets(InteriorOption interior)
    {
        if (interior == null)
        {
            return;
        }

        if (interior.Ipls != null)
        {
            for (int i = 0; i < interior.Ipls.Count; i++)
            {
                string ipl = interior.Ipls[i];

                if (string.IsNullOrWhiteSpace(ipl))
                {
                    continue;
                }

                Function.Call(Hash.REQUEST_IPL, ipl.Trim());
            }
        }

        Function.Call(Hash.REQUEST_COLLISION_AT_COORD, interior.Position.X, interior.Position.Y, interior.Position.Z);
    }

    private void TeleportPlayerWithFade(Ped player, Vector3 targetPosition, float heading)
    {
        if (!Entity.Exists(player))
        {
            return;
        }

        Function.Call(Hash.DO_SCREEN_FADE_OUT, 250);
        Wait(300);

        Function.Call(Hash.REQUEST_COLLISION_AT_COORD, targetPosition.X, targetPosition.Y, targetPosition.Z);
        Wait(80);

        player.Position = targetPosition;
        player.Heading = NormalizeHeading(heading);
        Function.Call(Hash.SET_ENTITY_VELOCITY, player.Handle, 0.0f, 0.0f, 0.0f);
        Function.Call(Hash.REQUEST_COLLISION_AT_COORD, targetPosition.X, targetPosition.Y, targetPosition.Z);
        Wait(150);

        Function.Call(Hash.DO_SCREEN_FADE_IN, 350);
    }

    private static Vector3 BuildReturnPositionFromEntrance(Vector3 entrancePosition, float entranceHeading)
    {
        Vector3 direction = HeadingToDirection(entranceHeading);
        return entrancePosition + direction * 1.45f + new Vector3(0.0f, 0.0f, 0.35f);
    }

    private static Vector3 HeadingToDirection(float heading)
    {
        float radians = heading * (float)Math.PI / 180.0f;
        return new Vector3(-(float)Math.Sin(radians), (float)Math.Cos(radians), 0.0f);
    }

    private static bool IsInteriorOptionValid(InteriorOption interior)
    {
        return interior != null &&
               !string.IsNullOrWhiteSpace(interior.Id) &&
               !string.IsNullOrWhiteSpace(interior.DisplayName) &&
               !IsZeroVector(interior.Position);
    }

    private static InteriorOption CloneInteriorOption(InteriorOption source)
    {
        if (source == null)
        {
            return null;
        }

        return new InteriorOption
        {
            Id = source.Id ?? string.Empty,
            Category = source.Category ?? string.Empty,
            DisplayName = source.DisplayName ?? string.Empty,
            Position = source.Position,
            Heading = NormalizeHeading(source.Heading),
            Ipls = source.Ipls != null ? new List<string>(source.Ipls) : new List<string>()
        };
    }

    private void ShowInteriorPortalHint(string text, int milliseconds)
    {
        if (Game.GameTime < _nextInteriorPortalHintAt)
        {
            return;
        }

        _nextInteriorPortalHintAt = Game.GameTime + InteriorPortalHintCooldownMs;
        ShowStatus(text, milliseconds);
    }

    private int WriteInteriorPortalsXml(XmlWriter writer)
    {
        int savedPortals = 0;

        writer.WriteStartElement("InteriorPortals");

        for (int i = 0; i < _placedInteriorPortals.Count; i++)
        {
            PlacedInteriorPortal portal = _placedInteriorPortals[i];

            if (portal == null || portal.Interior == null)
            {
                continue;
            }

            writer.WriteStartElement("Portal");
            writer.WriteAttributeString("id", portal.Id ?? string.Empty);
            writer.WriteAttributeString("kind", portal.Kind.ToString());
            writer.WriteAttributeString("sourceEntranceId", portal.SourceEntranceId ?? string.Empty);

            writer.WriteAttributeString("interiorId", portal.Interior.Id ?? string.Empty);
            writer.WriteAttributeString("interiorCategory", portal.Interior.Category ?? string.Empty);
            writer.WriteAttributeString("interiorName", portal.Interior.DisplayName ?? string.Empty);
            writer.WriteAttributeString("interiorX", portal.Interior.Position.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("interiorY", portal.Interior.Position.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("interiorZ", portal.Interior.Position.Z.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("interiorHeading", portal.Interior.Heading.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("interiorIpls", JoinInteriorIpls(BuildEffectiveInteriorIplList(portal.Interior)));

            writer.WriteAttributeString("x", portal.Position.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("y", portal.Position.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("z", portal.Position.Z.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("heading", portal.Heading.ToString(CultureInfo.InvariantCulture));

            writer.WriteAttributeString("returnX", portal.FallbackReturnPosition.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("returnY", portal.FallbackReturnPosition.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("returnZ", portal.FallbackReturnPosition.Z.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("returnHeading", portal.FallbackReturnHeading.ToString(CultureInfo.InvariantCulture));

            writer.WriteEndElement();
            savedPortals++;
        }

        writer.WriteEndElement();
        return savedPortals;
    }

    private int LoadInteriorPortalsFromXml(XmlDocument doc)
    {
        _placedInteriorPortals.Clear();
        _activeInteriorSession = null;
        _nextInteriorPortalUseAllowedAt = 0;
        _nextInteriorPortalHintAt = 0;

        if (doc == null)
        {
            return 0;
        }

        XmlNodeList portalNodes = doc.SelectNodes("/DonJEnemySpawnerSave/InteriorPortals/Portal");

        if (portalNodes == null || portalNodes.Count == 0)
        {
            return 0;
        }

        int loaded = 0;

        foreach (XmlNode node in portalNodes)
        {
            InteriorPortalKind kind = ReadEnumAttribute(node, "kind", InteriorPortalKind.Entrance);
            string interiorId = ReadStringAttribute(node, "interiorId", string.Empty);
            InteriorOption interior = ResolveInteriorOptionById(interiorId);

            if (interior == null)
            {
                interior = ReadInteriorOptionFromPortalXml(node);
            }
            else
            {
                interior = CloneInteriorOption(interior);
            }

            Vector3 position = new Vector3(
                ReadFloatAttribute(node, "x", 0.0f),
                ReadFloatAttribute(node, "y", 0.0f),
                ReadFloatAttribute(node, "z", 0.0f));

            if (!IsInteriorOptionValid(interior) || IsZeroVector(position))
            {
                continue;
            }

            PlacedInteriorPortal portal = new PlacedInteriorPortal
            {
                Id = ReadStringAttribute(node, "id", Guid.NewGuid().ToString("N")),
                Kind = kind,
                SourceEntranceId = ReadStringAttribute(node, "sourceEntranceId", string.Empty),
                Interior = interior,
                Position = position,
                Heading = NormalizeHeading(ReadFloatAttribute(node, "heading", 0.0f)),
                FallbackReturnPosition = new Vector3(
                    ReadFloatAttribute(node, "returnX", 0.0f),
                    ReadFloatAttribute(node, "returnY", 0.0f),
                    ReadFloatAttribute(node, "returnZ", 0.0f)),
                FallbackReturnHeading = NormalizeHeading(ReadFloatAttribute(node, "returnHeading", 0.0f))
            };

            if (portal.Kind == InteriorPortalKind.Entrance && IsZeroVector(portal.FallbackReturnPosition))
            {
                portal.FallbackReturnPosition = BuildReturnPositionFromEntrance(portal.Position, portal.Heading);
                portal.FallbackReturnHeading = portal.Heading;
            }

            _placedInteriorPortals.Add(portal);
            loaded++;
        }

        return loaded;
    }

    private InteriorOption ResolveInteriorOptionById(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || _interiorCategories == null)
        {
            return null;
        }

        for (int i = 0; i < _interiorCategories.Count; i++)
        {
            InteriorCategory category = _interiorCategories[i];

            if (category == null || category.Options == null)
            {
                continue;
            }

            for (int j = 0; j < category.Options.Count; j++)
            {
                InteriorOption option = category.Options[j];

                if (option != null && string.Equals(option.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return option;
                }
            }
        }

        return null;
    }

    private static InteriorOption ReadInteriorOptionFromPortalXml(XmlNode node)
    {
        return new InteriorOption
        {
            Id = ReadStringAttribute(node, "interiorId", "custom_interieur"),
            Category = ReadStringAttribute(node, "interiorCategory", "Charge depuis XML"),
            DisplayName = ReadStringAttribute(node, "interiorName", "Interieur charge"),
            Position = new Vector3(
                ReadFloatAttribute(node, "interiorX", 0.0f),
                ReadFloatAttribute(node, "interiorY", 0.0f),
                ReadFloatAttribute(node, "interiorZ", 0.0f)),
            Heading = NormalizeHeading(ReadFloatAttribute(node, "interiorHeading", 0.0f)),
            Ipls = SplitInteriorIpls(ReadStringAttribute(node, "interiorIpls", string.Empty))
        };
    }

    private static string JoinInteriorIpls(List<string> ipls)
    {
        if (ipls == null || ipls.Count == 0)
        {
            return string.Empty;
        }

        List<string> clean = new List<string>();

        for (int i = 0; i < ipls.Count; i++)
        {
            string value = ipls[i];

            if (!string.IsNullOrWhiteSpace(value))
            {
                clean.Add(value.Trim());
            }
        }

        return string.Join("|", clean.ToArray());
    }

    private static List<string> SplitInteriorIpls(string value)
    {
        List<string> result = new List<string>();

        if (string.IsNullOrWhiteSpace(value))
        {
            return result;
        }

        string[] parts = value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length; i++)
        {
            string ipl = parts[i];

            if (!string.IsNullOrWhiteSpace(ipl))
            {
                result.Add(ipl.Trim());
            }
        }

        return result;
    }
}
