using System;
using System.Collections.Generic;
using System.Globalization;
using GTA;
using GTA.Math;
using GTA.Native;

public sealed partial class DonJEnemySpawner
{
    private const float AdvancedInteriorTeleportZLift = 0.45f;
    private const float AdvancedInteriorHdAreaRadius = 95.0f;
    private const float AdvancedInteriorNewSceneRadius = 70.0f;
    private const int AdvancedInteriorAssetLoadTimeoutMs = 5000;
    private const int AdvancedInteriorAssetPollMs = 75;
    private const int AdvancedInteriorViewportStabilizeMs = 1750;
    private const int AdvancedInteriorViewportPollMs = 75;
    private const int AdvancedInteriorMaintainIntervalMs = 250;

    /*
     * Je garde ici les hashs bruts pour rester compatible avec NIB/SHVDN v2
     * meme si l'enum Hash locale n'expose pas encore tous les noms utiles.
     */
    private const ulong AdvancedNativeOnEnterMp = 0x0888C3502DBBEEF5UL;
    private const ulong AdvancedNativeSetInstancePriorityMode = 0x9BAE5AD2508DF078UL;

    private const ulong AdvancedNativeSetFocusPosAndVel = 0xBB7454BAFF08FE25UL;
    private const ulong AdvancedNativeSetFocusEntity = 0x198F77705FA0931DUL;
    private const ulong AdvancedNativeClearFocus = 0x31B73D1EA9F01DA2UL;
    private const ulong AdvancedNativeSetHdArea = 0xB85F26619073E775UL;
    private const ulong AdvancedNativeClearHdArea = 0xCE58B1CFB9290813UL;
    private const ulong AdvancedNativeNewLoadSceneStart = 0x212A8D0D2BABFAC2UL;
    private const ulong AdvancedNativeNewLoadSceneStop = 0xC197616D221FF4A4UL;

    private const ulong AdvancedNativeGetInteriorAtCoords = 0xB0F7F8663821D9C3UL;
    private const ulong AdvancedNativeGetInteriorFromEntity = 0x2107BA504071A6BBUL;
    private const ulong AdvancedNativeIsValidInterior = 0x26B0E73D7EAAF4D3UL;
    private const ulong AdvancedNativePinInteriorInMemory = 0x2CA429C029CCF247UL;
    private const ulong AdvancedNativeIsInteriorReady = 0x6726BDCCC1932F0EUL;
    private const ulong AdvancedNativeActivateInteriorEntitySet = 0x55E86AF2712B36A1UL;
    private const ulong AdvancedNativeSetInteriorEntitySetTintIndex = 0xC1F1920BAF281317UL;
    private const ulong AdvancedNativeRefreshInterior = 0x41F37C3427C75AE0UL;

    private const ulong AdvancedNativeClearRoomForEntity = 0xB365FC0C4E27FFA7UL;
    private const ulong AdvancedNativeForceRoomForEntity = 0x52923C4710DD9907UL;
    private const ulong AdvancedNativeGetRoomKeyFromEntity = 0x47C2A06D4F5F424BUL;
    private const ulong AdvancedNativeGetKeyForEntityInRoom = 0x399685DB942336BCUL;
    private const ulong AdvancedNativeForceRoomForGameViewport = 0x920D853F3E17F1DAUL;
    private const ulong AdvancedNativeGetRoomKeyForGameViewport = 0xA6575914D2A0B450UL;
    private const ulong AdvancedNativeClearRoomForGameViewport = 0x23B59D8912F94246UL;

    private const ulong AdvancedNativeSetFollowPedCamViewMode = 0x5A4F9EDF1673F704UL;
    private const ulong AdvancedNativeGetFollowPedCamViewMode = 0x8D4D46230B2C353AUL;
    private const ulong AdvancedNativeSetGameplayCamRelativeHeading = 0xB4EC2312F4E5B1F1UL;
    private const ulong AdvancedNativeSetGameplayCamRelativePitch = 0x6D0858B8EDFD2B7DUL;

    private const ulong AdvancedNativeSetEntityCoordsNoOffset = 0x239A3351AC1DA385UL;
    private const ulong AdvancedNativeSetEntityLoadCollisionFlag = 0x0DC7CABAB1E9B67EUL;

    private bool _advancedInteriorMpMapLoadRequested;
    private int _advancedInteriorForcedInteriorId;
    private int _advancedInteriorForcedRoomKey;
    private int _advancedInteriorNextMaintainAt;

    private sealed class AdvancedInteriorEntitySetSpec
    {
        public string Name;
        public bool HasTint;
        public int TintIndex;
    }

    private bool PrepareInteriorForTeleportSafe(InteriorOption interior)
    {
        if (interior == null)
        {
            return false;
        }

        if (ShouldLoadMultiplayerMapSafe(interior))
        {
            EnsureMultiplayerInteriorMapLoadedSafe();
        }

        BeginInteriorStreamingFocusSafe(interior.Position);
        RequestInteriorAssetsSafe(interior);

        int interiorId = 0;
        bool needsReadyInterior = ShouldWaitForInteriorReadySafe(interior);
        int deadline = Game.GameTime + AdvancedInteriorAssetLoadTimeoutMs;

        while (Game.GameTime < deadline)
        {
            RequestInteriorAssetsSafe(interior);
            BeginInteriorStreamingFocusSafe(interior.Position);
            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, interior.Position.X, interior.Position.Y, interior.Position.Z);

            interiorId = GetInteriorIdAtCoordsSafe(interior.Position);

            if (interiorId != 0 && IsValidInteriorIdSafe(interiorId))
            {
                PinInteriorSafe(interiorId);
                ActivateDefaultInteriorEntitySetsSafe(interiorId, interior);
                RefreshInteriorSafe(interiorId);

                if (!needsReadyInterior || IsInteriorReadySafe(interiorId))
                {
                    return true;
                }
            }
            else if (!needsReadyInterior)
            {
                return true;
            }

            Wait(AdvancedInteriorAssetPollMs);
        }

        if (interiorId != 0 && IsValidInteriorIdSafe(interiorId))
        {
            ActivateDefaultInteriorEntitySetsSafe(interiorId, interior);
            RefreshInteriorSafe(interiorId);
            return true;
        }

        /*
         * Je ne bloque pas indefiniment les interieurs legacy qui n'exposent
         * pas toujours un interior id detectable.
         */
        return !needsReadyInterior;
    }

    private void RequestInteriorAssetsSafe(InteriorOption interior)
    {
        if (interior == null)
        {
            return;
        }

        List<string> ipls = BuildEffectiveInteriorIplList(interior);

        for (int i = 0; i < ipls.Count; i++)
        {
            string ipl = ipls[i];

            if (string.IsNullOrWhiteSpace(ipl))
            {
                continue;
            }

            try
            {
                Function.Call(Hash.REQUEST_IPL, ipl.Trim());
            }
            catch
            {
                // Je garde ce chargement tolerant: un IPL inconnu ne doit jamais casser le mod complet.
            }
        }

        Function.Call(Hash.REQUEST_COLLISION_AT_COORD, interior.Position.X, interior.Position.Y, interior.Position.Z);
    }

    private void ApplyInteriorEntitySetsSafe(InteriorOption interior)
    {
        if (interior == null)
        {
            return;
        }

        int interiorId = GetInteriorIdAtCoordsSafe(interior.Position);

        if (interiorId == 0 || !IsValidInteriorIdSafe(interiorId))
        {
            return;
        }

        ActivateDefaultInteriorEntitySetsSafe(interiorId, interior);
        RefreshInteriorSafe(interiorId);
    }

    private void EnsureMultiplayerInteriorMapLoadedSafe()
    {
        if (_advancedInteriorMpMapLoadRequested)
        {
            return;
        }

        _advancedInteriorMpMapLoadRequested = true;

        try
        {
            Function.Call((Hash)AdvancedNativeOnEnterMp);
        }
        catch
        {
        }

        try
        {
            Function.Call((Hash)AdvancedNativeSetInstancePriorityMode, 1);
        }
        catch
        {
        }
    }

    private static bool ShouldLoadMultiplayerMapSafe(InteriorOption interior)
    {
        if (interior == null)
        {
            return false;
        }

        string id = (interior.Id ?? string.Empty).ToLowerInvariant();
        string category = (interior.Category ?? string.Empty).ToLowerInvariant();

        if (category.Contains("online") ||
            category.Contains("ceo") ||
            category.Contains("mc") ||
            category.Contains("business") ||
            category.Contains("casino") ||
            category.Contains("entrepot"))
        {
            return true;
        }

        if (id.Contains("bunker") ||
            id.Contains("facility") ||
            id.Contains("server_farm") ||
            id.Contains("smugglers") ||
            id.Contains("hangar") ||
            id.Contains("submarine") ||
            id.Contains("avenger") ||
            id.Contains("nightclub") ||
            id.Contains("terrorbyte") ||
            id.StartsWith("apt_", StringComparison.OrdinalIgnoreCase) ||
            id.StartsWith("arcadius_", StringComparison.OrdinalIgnoreCase) ||
            id.StartsWith("maze_", StringComparison.OrdinalIgnoreCase) ||
            id.StartsWith("lom_", StringComparison.OrdinalIgnoreCase) ||
            id.StartsWith("clubhouse_", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (interior.Ipls != null)
        {
            for (int i = 0; i < interior.Ipls.Count; i++)
            {
                string ipl = interior.Ipls[i] ?? string.Empty;

                if (ipl.StartsWith("apa_", StringComparison.OrdinalIgnoreCase) ||
                    ipl.StartsWith("bkr_", StringComparison.OrdinalIgnoreCase) ||
                    ipl.StartsWith("ex_", StringComparison.OrdinalIgnoreCase) ||
                    ipl.StartsWith("imp_", StringComparison.OrdinalIgnoreCase) ||
                    ipl.StartsWith("gr_", StringComparison.OrdinalIgnoreCase) ||
                    ipl.StartsWith("xm_", StringComparison.OrdinalIgnoreCase) ||
                    ipl.StartsWith("sm_", StringComparison.OrdinalIgnoreCase) ||
                    ipl.StartsWith("vw_", StringComparison.OrdinalIgnoreCase) ||
                    ipl.StartsWith("ba_", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool ShouldWaitForInteriorReadySafe(InteriorOption interior)
    {
        if (interior == null)
        {
            return false;
        }

        string id = (interior.Id ?? string.Empty).ToLowerInvariant();

        return id.Contains("bunker") ||
               id.Contains("facility") ||
               id.Contains("server_farm") ||
               id.Contains("smugglers") ||
               id.Contains("hangar") ||
               id.Contains("submarine") ||
               id.Contains("avenger") ||
               id.Contains("nightclub") ||
               id.Contains("terrorbyte") ||
               id.Contains("warehouse") ||
               id.Contains("clubhouse") ||
               id.Contains("casino") ||
               id.StartsWith("apt_", StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> BuildEffectiveInteriorIplList(InteriorOption interior)
    {
        List<string> result = new List<string>();

        if (interior != null && interior.Ipls != null)
        {
            for (int i = 0; i < interior.Ipls.Count; i++)
            {
                AddUniqueInteriorString(result, interior.Ipls[i]);
            }
        }

        string id = interior != null ? (interior.Id ?? string.Empty).ToLowerInvariant() : string.Empty;

        if (id.Contains("bunker"))
        {
            AddUniqueInteriorStrings(result, BunkerInteriorIplsSafe());
        }

        if (id == "facility" || id == "iaa_facility" || id == "server_farm" || id == "submarine" || id == "avenger")
        {
            AddUniqueInteriorStrings(result, DoomsdayInteriorIplsSafe());
        }

        if (id == "smugglers_hangar")
        {
            AddUniqueInteriorStrings(result, SmugglersRunInteriorIplsSafe());
        }

        if (id == "nightclub" || id == "nightclub_warehouse" || id == "casino_nightclub")
        {
            AddUniqueInteriorStrings(result, NightclubInteriorIplsSafe());
        }

        if (id == "terrorbyte")
        {
            AddUniqueInteriorStrings(result, TerrorbyteInteriorIplsSafe());
        }

        if (id.StartsWith("clubhouse_", StringComparison.OrdinalIgnoreCase) ||
            id == "meth_lab" ||
            id == "weed_farm" ||
            id == "cocaine_lockup" ||
            id == "counterfeit_cash" ||
            id == "document_forgery")
        {
            AddUniqueInteriorStrings(result, BikerBusinessBaseIplsSafe());
        }

        if (id.StartsWith("warehouse_", StringComparison.OrdinalIgnoreCase))
        {
            AddUniqueInteriorString(result, "ex_exec_warehouse_placement");
        }

        if (id == "vehicle_warehouse")
        {
            AddUniqueInteriorString(result, "imp_impexp_interior_placement");
        }

        if (id.StartsWith("apt_", StringComparison.OrdinalIgnoreCase))
        {
            AddUniqueInteriorString(result, "apa_v_mp_h_01_a");
        }

        return result;
    }

    private static void AddUniqueInteriorStrings(List<string> target, string[] values)
    {
        if (target == null || values == null)
        {
            return;
        }

        for (int i = 0; i < values.Length; i++)
        {
            AddUniqueInteriorString(target, values[i]);
        }
    }

    private static void AddUniqueInteriorString(List<string> target, string value)
    {
        if (target == null || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        string clean = value.Trim();

        for (int i = 0; i < target.Count; i++)
        {
            if (string.Equals(target[i], clean, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        target.Add(clean);
    }

    private void ActivateDefaultInteriorEntitySetsSafe(int interiorId, InteriorOption interior)
    {
        List<AdvancedInteriorEntitySetSpec> entitySets = BuildDefaultInteriorEntitySetsSafe(interior);

        for (int i = 0; i < entitySets.Count; i++)
        {
            AdvancedInteriorEntitySetSpec set = entitySets[i];

            if (set == null || string.IsNullOrWhiteSpace(set.Name))
            {
                continue;
            }

            try
            {
                Function.Call((Hash)AdvancedNativeActivateInteriorEntitySet, interiorId, set.Name);

                if (set.HasTint)
                {
                    Function.Call((Hash)AdvancedNativeSetInteriorEntitySetTintIndex, interiorId, set.Name, set.TintIndex);
                }
            }
            catch
            {
            }
        }
    }

    private static List<AdvancedInteriorEntitySetSpec> BuildDefaultInteriorEntitySetsSafe(InteriorOption interior)
    {
        List<AdvancedInteriorEntitySetSpec> result = new List<AdvancedInteriorEntitySetSpec>();

        if (interior == null)
        {
            return result;
        }

        string id = (interior.Id ?? string.Empty).ToLowerInvariant();

        if (id == "facility" || id == "iaa_facility")
        {
            AddInteriorEntitySetSafe(result, "set_int_02_decal_01", 1);
            AddInteriorEntitySetSafe(result, "set_int_02_lounge1", 1);
            AddInteriorEntitySetSafe(result, "set_int_02_cannon", 1);
            AddInteriorEntitySetSafe(result, "set_int_02_clutter1", 1);
            AddInteriorEntitySetSafe(result, "set_int_02_crewemblem", null);
            AddInteriorEntitySetSafe(result, "set_int_02_shell", 1);
            AddInteriorEntitySetSafe(result, "set_int_02_security", 1);
            AddInteriorEntitySetSafe(result, "set_int_02_sleep", 1);
            AddInteriorEntitySetSafe(result, "set_int_02_trophy1", 1);
            AddInteriorEntitySetSafe(result, "set_int_02_paramedic_complete", 1);
            AddInteriorEntitySetSafe(result, "set_Int_02_outfit_paramedic", 1);
            AddInteriorEntitySetSafe(result, "set_Int_02_outfit_serverfarm", 1);
        }
        else if (id == "server_farm")
        {
            AddInteriorEntitySetSafe(result, "set_int_02_shell", 1);
            AddInteriorEntitySetSafe(result, "set_Int_02_outfit_serverfarm", 1);
            AddInteriorEntitySetSafe(result, "set_int_02_security", 1);
        }
        else if (id == "smugglers_hangar")
        {
            AddInteriorEntitySetSafe(result, "set_lighting_hangar_a", null);
            AddInteriorEntitySetSafe(result, "set_tint_shell", 1);
            AddInteriorEntitySetSafe(result, "set_bedroom_tint", 1);
            AddInteriorEntitySetSafe(result, "set_crane_tint", 1);
            AddInteriorEntitySetSafe(result, "set_modarea", 1);
            AddInteriorEntitySetSafe(result, "set_lighting_tint_props", 1);
            AddInteriorEntitySetSafe(result, "set_floor_1", null);
            AddInteriorEntitySetSafe(result, "set_floor_decal_1", 1);
            AddInteriorEntitySetSafe(result, "set_bedroom_modern", null);
            AddInteriorEntitySetSafe(result, "set_office_modern", null);
            AddInteriorEntitySetSafe(result, "set_bedroom_blinds_open", null);
            AddInteriorEntitySetSafe(result, "set_lighting_wall_tint01", null);
        }
        else if (id.Contains("bunker"))
        {
            AddInteriorEntitySetSafe(result, "standard_bunker_set", null);
            AddInteriorEntitySetSafe(result, "interior_basic", null);
            AddInteriorEntitySetSafe(result, "office_blocker_set", null);
            AddInteriorEntitySetSafe(result, "gun_wall_blocker", null);
            AddInteriorEntitySetSafe(result, "gun_range_lights", null);
            AddInteriorEntitySetSafe(result, "gun_locker_upgrade", null);
            AddInteriorEntitySetSafe(result, "Gun_schematic_set", null);
        }

        return result;
    }

    private static void AddInteriorEntitySetSafe(List<AdvancedInteriorEntitySetSpec> target, string name, int? tintIndex)
    {
        if (target == null || string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        target.Add(new AdvancedInteriorEntitySetSpec
        {
            Name = name.Trim(),
            HasTint = tintIndex.HasValue,
            TintIndex = tintIndex.HasValue ? tintIndex.Value : 0
        });
    }

    private static string[] BunkerInteriorIplsSafe()
    {
        return new[]
        {
            "grdlc_int_01_shell",
            "gr_grdlc_int_01",
            "gr_grdlc_int_02",
            "gr_entrance_placement",
            "gr_grdlc_interior_placement",
            "gr_grdlc_interior_placement_interior_0_grdlc_int_01_milo_",
            "gr_grdlc_interior_placement_interior_1_grdlc_int_02_milo_"
        };
    }

    private static string[] DoomsdayInteriorIplsSafe()
    {
        return new[]
        {
            "xm_x17dlc_int_placement",
            "xm_x17dlc_int_placement_interior_0_x17dlc_int_base_ent_milo_",
            "xm_x17dlc_int_placement_interior_1_x17dlc_int_base_loop_milo_",
            "xm_x17dlc_int_placement_interior_2_x17dlc_int_bse_tun_milo_",
            "xm_x17dlc_int_placement_interior_3_x17dlc_int_base_milo_",
            "xm_x17dlc_int_placement_interior_4_x17dlc_int_facility_milo_",
            "xm_x17dlc_int_placement_interior_5_x17dlc_int_facility2_milo_",
            "xm_x17dlc_int_placement_interior_6_x17dlc_int_silo_01_milo_",
            "xm_x17dlc_int_placement_interior_7_x17dlc_int_silo_02_milo_",
            "xm_x17dlc_int_placement_interior_8_x17dlc_int_sub_milo_",
            "xm_x17dlc_int_placement_interior_9_x17dlc_int_01_milo_",
            "xm_x17dlc_int_placement_interior_10_x17dlc_int_tun_straight_milo_",
            "xm_x17dlc_int_placement_interior_11_x17dlc_int_tun_slope_flat_milo_",
            "xm_x17dlc_int_placement_interior_12_x17dlc_int_tun_flat_slope_milo_",
            "xm_x17dlc_int_placement_interior_13_x17dlc_int_tun_30d_r_milo_",
            "xm_x17dlc_int_placement_interior_14_x17dlc_int_tun_30d_l_milo_",
            "xm_x17dlc_int_placement_interior_15_x17dlc_int_tun_straight_milo_",
            "xm_x17dlc_int_placement_interior_16_x17dlc_int_tun_straight_milo_",
            "xm_x17dlc_int_placement_interior_17_x17dlc_int_tun_slope_flat_milo_",
            "xm_x17dlc_int_placement_interior_18_x17dlc_int_tun_slope_flat_milo_",
            "xm_x17dlc_int_placement_interior_19_x17dlc_int_tun_flat_slope_milo_",
            "xm_x17dlc_int_placement_interior_20_x17dlc_int_tun_flat_slope_milo_",
            "xm_x17dlc_int_placement_interior_21_x17dlc_int_tun_30d_r_milo_",
            "xm_x17dlc_int_placement_interior_22_x17dlc_int_tun_30d_r_milo_",
            "xm_x17dlc_int_placement_interior_23_x17dlc_int_tun_30d_r_milo_",
            "xm_x17dlc_int_placement_interior_24_x17dlc_int_tun_30d_r_milo_",
            "xm_x17dlc_int_placement_interior_25_x17dlc_int_tun_30d_l_milo_",
            "xm_x17dlc_int_placement_interior_26_x17dlc_int_tun_30d_l_milo_",
            "xm_x17dlc_int_placement_interior_27_x17dlc_int_tun_30d_l_milo_",
            "xm_x17dlc_int_placement_interior_28_x17dlc_int_tun_30d_l_milo_",
            "xm_x17dlc_int_placement_interior_29_x17dlc_int_tun_30d_l_milo_",
            "xm_x17dlc_int_placement_interior_30_v_apart_midspaz_milo_",
            "xm_x17dlc_int_placement_interior_31_v_studio_lo_milo_",
            "xm_x17dlc_int_placement_interior_32_v_garagem_milo_",
            "xm_x17dlc_int_placement_interior_33_x17dlc_int_02_milo_",
            "xm_x17dlc_int_placement_interior_34_x17dlc_int_lab_milo_",
            "xm_x17dlc_int_placement_interior_35_x17dlc_int_tun_entry_milo_",
            "xm_x17dlc_int_placement_strm_0",
            "xm_bunkerentrance_door",
            "xm_hatch_closed",
            "xm_hatches_terrain",
            "xm_hatches_terrain_lod",
            "xm_mpchristmasadditions",
            "xm_siloentranceclosed_x17"
        };
    }

    private static string[] SmugglersRunInteriorIplsSafe()
    {
        return new[]
        {
            "sm_smugdlc_interior_placement",
            "sm_smugdlc_interior_placement_interior_0_smugdlc_int_01_milo_"
        };
    }

    private static string[] BikerBusinessBaseIplsSafe()
    {
        return new[]
        {
            "bkr_biker_interior_placement",
            "bkr_biker_interior_placement_interior_0_biker_dlc_int_01_milo",
            "bkr_biker_interior_placement_interior_1_biker_dlc_int_02_milo",
            "bkr_biker_interior_placement_interior_2_biker_dlc_int_ware01_milo",
            "bkr_biker_interior_placement_interior_3_biker_dlc_int_ware02_milo",
            "bkr_biker_interior_placement_interior_4_biker_dlc_int_ware03_milo",
            "bkr_biker_interior_placement_interior_5_biker_dlc_int_ware04_milo",
            "bkr_biker_interior_placement_interior_6_biker_dlc_int_ware05_milo"
        };
    }

    private static string[] NightclubInteriorIplsSafe()
    {
        return new[]
        {
            "ba_int_placement_ba",
            "ba_int_placement_ba_interior_0_dlc_int_01_ba_milo_",
            "ba_int_placement_ba_interior_1_dlc_int_02_ba_milo_",
            "ba_int_placement_ba_interior_2_dlc_int_03_ba_milo_"
        };
    }

    private static string[] TerrorbyteInteriorIplsSafe()
    {
        return new[]
        {
            "xm_x17dlc_int_placement",
            "xm_x17dlc_int_placement_interior_32_v_garagem_milo_"
        };
    }

    private int GetInteriorIdAtCoordsSafe(Vector3 position)
    {
        try
        {
            return Function.Call<int>((Hash)AdvancedNativeGetInteriorAtCoords, position.X, position.Y, position.Z);
        }
        catch
        {
            return 0;
        }
    }

    private int GetInteriorFromEntitySafe(Entity entity)
    {
        if (!Entity.Exists(entity))
        {
            return 0;
        }

        try
        {
            return Function.Call<int>((Hash)AdvancedNativeGetInteriorFromEntity, entity.Handle);
        }
        catch
        {
            return 0;
        }
    }

    private bool IsValidInteriorIdSafe(int interiorId)
    {
        if (interiorId == 0)
        {
            return false;
        }

        try
        {
            return Function.Call<bool>((Hash)AdvancedNativeIsValidInterior, interiorId);
        }
        catch
        {
            return true;
        }
    }

    private void PinInteriorSafe(int interiorId)
    {
        if (interiorId == 0)
        {
            return;
        }

        try
        {
            Function.Call((Hash)AdvancedNativePinInteriorInMemory, interiorId);
        }
        catch
        {
        }
    }

    private bool IsInteriorReadySafe(int interiorId)
    {
        if (interiorId == 0)
        {
            return false;
        }

        try
        {
            return Function.Call<bool>((Hash)AdvancedNativeIsInteriorReady, interiorId);
        }
        catch
        {
            return true;
        }
    }

    private void RefreshInteriorSafe(int interiorId)
    {
        if (interiorId == 0)
        {
            return;
        }

        try
        {
            Function.Call((Hash)AdvancedNativeRefreshInterior, interiorId);
        }
        catch
        {
        }
    }

    private void BeginInteriorStreamingFocusSafe(Vector3 position)
    {
        try
        {
            Function.Call((Hash)AdvancedNativeSetHdArea, position.X, position.Y, position.Z, AdvancedInteriorHdAreaRadius);
        }
        catch
        {
        }

        try
        {
            Function.Call((Hash)AdvancedNativeSetFocusPosAndVel, position.X, position.Y, position.Z, 0.0f, 0.0f, 0.0f);
        }
        catch
        {
        }

        try
        {
            Function.Call((Hash)AdvancedNativeNewLoadSceneStart,
                position.X,
                position.Y,
                position.Z,
                0.0f,
                0.0f,
                0.0f,
                AdvancedInteriorNewSceneRadius,
                0);
        }
        catch
        {
        }
    }

    private void KeepInteriorFocusOnPlayerSafe(Ped player)
    {
        if (!Entity.Exists(player))
        {
            return;
        }

        try
        {
            Function.Call((Hash)AdvancedNativeSetFocusEntity, player.Handle);
        }
        catch
        {
        }

        try
        {
            Function.Call((Hash)AdvancedNativeSetFocusPosAndVel, player.Position.X, player.Position.Y, player.Position.Z, 0.0f, 0.0f, 0.0f);
        }
        catch
        {
        }

        try
        {
            Function.Call((Hash)AdvancedNativeSetHdArea, player.Position.X, player.Position.Y, player.Position.Z, AdvancedInteriorHdAreaRadius);
        }
        catch
        {
        }
    }

    private void ClearInteriorRenderingFocusSafe(Ped player)
    {
        if (Entity.Exists(player))
        {
            try
            {
                Function.Call((Hash)AdvancedNativeClearRoomForEntity, player.Handle);
            }
            catch
            {
            }
        }

        try
        {
            Function.Call((Hash)AdvancedNativeClearRoomForGameViewport);
        }
        catch
        {
        }

        try
        {
            Function.Call((Hash)AdvancedNativeClearFocus);
        }
        catch
        {
        }

        try
        {
            Function.Call((Hash)AdvancedNativeClearHdArea);
        }
        catch
        {
        }

        try
        {
            Function.Call((Hash)AdvancedNativeNewLoadSceneStop);
        }
        catch
        {
        }

        _advancedInteriorForcedInteriorId = 0;
        _advancedInteriorForcedRoomKey = 0;
        _advancedInteriorNextMaintainAt = 0;
    }

    private bool IsTargetInsideCurrentActiveInterior(Vector3 targetPosition)
    {
        if (_activeInteriorSession == null || _activeInteriorSession.Interior == null)
        {
            return false;
        }

        return targetPosition.DistanceTo(_activeInteriorSession.Interior.Position) <= 12.0f;
    }

    private void TeleportPlayerWithFadeSafe(Ped player, Vector3 targetPosition, float heading)
    {
        if (!Entity.Exists(player))
        {
            return;
        }

        bool targetIsInterior = IsTargetInsideCurrentActiveInterior(targetPosition);
        InteriorOption targetInterior = targetIsInterior ? _activeInteriorSession.Interior : null;
        Vector3 safeTarget = targetPosition + new Vector3(0.0f, 0.0f, AdvancedInteriorTeleportZLift);
        bool oldFrozen = player.FreezePosition;
        bool oldInvincible = player.IsInvincible;
        bool oldCanRagdoll = player.CanRagdoll;

        Function.Call(Hash.DO_SCREEN_FADE_OUT, 250);
        Wait(300);

        try
        {
            player.IsInvincible = true;
            player.FreezePosition = true;
            player.CanRagdoll = false;

            if (targetIsInterior && targetInterior != null)
            {
                BeginInteriorStreamingFocusSafe(targetInterior.Position);
                RequestInteriorAssetsSafe(targetInterior);
                ApplyInteriorEntitySetsSafe(targetInterior);
            }
            else
            {
                ClearInteriorRenderingFocusSafe(player);
            }

            Function.Call(Hash.REQUEST_COLLISION_AT_COORD, safeTarget.X, safeTarget.Y, safeTarget.Z);
            SetEntityLoadCollisionFlagSafe(player, true);
            Wait(120);

            for (int i = 0; i < 5; i++)
            {
                if (targetIsInterior && targetInterior != null)
                {
                    BeginInteriorStreamingFocusSafe(targetInterior.Position);
                    RequestInteriorAssetsSafe(targetInterior);
                }

                Function.Call(Hash.REQUEST_COLLISION_AT_COORD, safeTarget.X, safeTarget.Y, safeTarget.Z);
                SetEntityCoordsNoOffsetSafe(player, safeTarget);
                player.Heading = NormalizeHeading(heading);
                Function.Call(Hash.SET_ENTITY_VELOCITY, player.Handle, 0.0f, 0.0f, 0.0f);
                Wait(AdvancedInteriorViewportPollMs);
            }

            if (targetIsInterior && targetInterior != null)
            {
                StabilizeInteriorViewportAfterTeleportSafe(player, targetInterior, heading);
            }
            else
            {
                ClearInteriorRenderingFocusSafe(player);
            }
        }
        finally
        {
            player.FreezePosition = oldFrozen;
            player.IsInvincible = oldInvincible;
            player.CanRagdoll = oldCanRagdoll;
        }

        Function.Call(Hash.DO_SCREEN_FADE_IN, 350);
    }

    private void StabilizeInteriorViewportAfterTeleportSafe(Ped player, InteriorOption interior, float heading)
    {
        if (!Entity.Exists(player) || interior == null)
        {
            return;
        }

        int oldViewMode = GetFollowPedCamViewModeSafe();
        bool changedViewMode = false;

        int interiorId = ResolveInteriorIdForPlayerSafe(player, interior);
        if (interiorId != 0 && IsValidInteriorIdSafe(interiorId))
        {
            PinInteriorSafe(interiorId);
            ActivateDefaultInteriorEntitySetsSafe(interiorId, interior);
            RefreshInteriorSafe(interiorId);
        }

        int deadline = Game.GameTime + AdvancedInteriorViewportStabilizeMs;

        while (Game.GameTime < deadline)
        {
            KeepInteriorFocusOnPlayerSafe(player);
            RequestInteriorAssetsSafe(interior);

            interiorId = ResolveInteriorIdForPlayerSafe(player, interior);

            if (interiorId != 0 && IsValidInteriorIdSafe(interiorId))
            {
                PinInteriorSafe(interiorId);
                ActivateDefaultInteriorEntitySetsSafe(interiorId, interior);
                RefreshInteriorSafe(interiorId);

                int roomKey = ResolveRoomKeyForPlayerOrViewportSafe(player);

                if (roomKey != 0)
                {
                    ForceInteriorRoomAndViewportSafe(player, interiorId, roomKey);
                    player.Heading = NormalizeHeading(heading);
                    Function.Call(Hash.SET_ENTITY_VELOCITY, player.Handle, 0.0f, 0.0f, 0.0f);

                    if (changedViewMode && oldViewMode >= 0)
                    {
                        SetFollowPedCamViewModeSafe(oldViewMode);
                    }

                    return;
                }
            }

            if (!changedViewMode && Game.GameTime > deadline - AdvancedInteriorViewportStabilizeMs + 450)
            {
                // Je pulse temporairement la vue 1ere personne pour forcer le calcul du roomKey.
                SetFollowPedCamViewModeSafe(4);
                SetGameplayCameraFacingSafe(0.0f, 0.0f);
                changedViewMode = true;
            }

            Wait(AdvancedInteriorViewportPollMs);
        }

        interiorId = ResolveInteriorIdForPlayerSafe(player, interior);
        int fallbackRoomKey = ResolveRoomKeyForPlayerOrViewportSafe(player);

        if (interiorId != 0 && fallbackRoomKey != 0)
        {
            ForceInteriorRoomAndViewportSafe(player, interiorId, fallbackRoomKey);
        }

        if (changedViewMode && oldViewMode >= 0)
        {
            // Je restaure la vue apres le force viewport pour ne pas laisser le joueur bloque en 1ere personne.
            SetFollowPedCamViewModeSafe(oldViewMode);
        }
    }

    private void MaintainActiveInteriorVisualsSafe(Ped player)
    {
        if (_activeInteriorSession == null || _activeInteriorSession.Interior == null)
        {
            return;
        }

        if (!Entity.Exists(player) || player.IsDead)
        {
            return;
        }

        if (!IsPointInsideActiveInterior(player.Position))
        {
            return;
        }

        if (Game.GameTime < _advancedInteriorNextMaintainAt)
        {
            return;
        }

        _advancedInteriorNextMaintainAt = Game.GameTime + AdvancedInteriorMaintainIntervalMs;

        InteriorOption interior = _activeInteriorSession.Interior;
        int interiorId = ResolveInteriorIdForPlayerSafe(player, interior);

        if (interiorId == 0 || !IsValidInteriorIdSafe(interiorId))
        {
            BeginInteriorStreamingFocusSafe(interior.Position);
            return;
        }

        KeepInteriorFocusOnPlayerSafe(player);
        PinInteriorSafe(interiorId);
        ActivateDefaultInteriorEntitySetsSafe(interiorId, interior);

        int roomKey = ResolveRoomKeyForPlayerOrViewportSafe(player);

        if (roomKey != 0)
        {
            ForceInteriorRoomAndViewportSafe(player, interiorId, roomKey);
        }
    }

    private int ResolveInteriorIdForPlayerSafe(Ped player, InteriorOption interior)
    {
        int interiorId = GetInteriorFromEntitySafe(player);

        if (interiorId != 0)
        {
            return interiorId;
        }

        if (interior != null)
        {
            interiorId = GetInteriorIdAtCoordsSafe(interior.Position);

            if (interiorId != 0)
            {
                return interiorId;
            }
        }

        return Entity.Exists(player) ? GetInteriorIdAtCoordsSafe(player.Position) : 0;
    }

    private int ResolveRoomKeyForPlayerOrViewportSafe(Ped player)
    {
        int roomKey = GetRoomKeyFromEntitySafe(player);

        if (roomKey != 0)
        {
            return roomKey;
        }

        roomKey = GetKeyForEntityInRoomSafe(player);

        if (roomKey != 0)
        {
            return roomKey;
        }

        roomKey = GetRoomKeyForGameViewportSafe();

        if (roomKey != 0)
        {
            return roomKey;
        }

        return _advancedInteriorForcedRoomKey;
    }

    private int GetRoomKeyFromEntitySafe(Entity entity)
    {
        if (!Entity.Exists(entity))
        {
            return 0;
        }

        try
        {
            return Function.Call<int>((Hash)AdvancedNativeGetRoomKeyFromEntity, entity.Handle);
        }
        catch
        {
            return 0;
        }
    }

    private int GetKeyForEntityInRoomSafe(Entity entity)
    {
        if (!Entity.Exists(entity))
        {
            return 0;
        }

        try
        {
            return Function.Call<int>((Hash)AdvancedNativeGetKeyForEntityInRoom, entity.Handle);
        }
        catch
        {
            return 0;
        }
    }

    private int GetRoomKeyForGameViewportSafe()
    {
        try
        {
            return Function.Call<int>((Hash)AdvancedNativeGetRoomKeyForGameViewport);
        }
        catch
        {
            return 0;
        }
    }

    private void ForceInteriorRoomAndViewportSafe(Ped player, int interiorId, int roomKey)
    {
        if (interiorId == 0 || roomKey == 0)
        {
            return;
        }

        _advancedInteriorForcedInteriorId = interiorId;
        _advancedInteriorForcedRoomKey = roomKey;

        if (Entity.Exists(player))
        {
            try
            {
                Function.Call((Hash)AdvancedNativeForceRoomForEntity, player.Handle, interiorId, roomKey);
            }
            catch
            {
            }
        }

        try
        {
            Function.Call((Hash)AdvancedNativeForceRoomForGameViewport, interiorId, roomKey);
        }
        catch
        {
        }
    }

    private int GetFollowPedCamViewModeSafe()
    {
        try
        {
            return Function.Call<int>((Hash)AdvancedNativeGetFollowPedCamViewMode);
        }
        catch
        {
            return -1;
        }
    }

    private void SetFollowPedCamViewModeSafe(int viewMode)
    {
        try
        {
            Function.Call((Hash)AdvancedNativeSetFollowPedCamViewMode, viewMode);
        }
        catch
        {
        }
    }

    private void SetGameplayCameraFacingSafe(float heading, float pitch)
    {
        try
        {
            Function.Call((Hash)AdvancedNativeSetGameplayCamRelativeHeading, heading);
        }
        catch
        {
        }

        try
        {
            Function.Call((Hash)AdvancedNativeSetGameplayCamRelativePitch, pitch, 1.0f);
        }
        catch
        {
        }
    }

    private void SetEntityCoordsNoOffsetSafe(Entity entity, Vector3 position)
    {
        if (!Entity.Exists(entity))
        {
            return;
        }

        try
        {
            Function.Call((Hash)AdvancedNativeSetEntityCoordsNoOffset,
                entity.Handle,
                position.X,
                position.Y,
                position.Z,
                false,
                false,
                false);
        }
        catch
        {
            entity.Position = position;
        }
    }

    private void SetEntityLoadCollisionFlagSafe(Entity entity, bool enabled)
    {
        if (!Entity.Exists(entity))
        {
            return;
        }

        try
        {
            Function.Call((Hash)AdvancedNativeSetEntityLoadCollisionFlag, entity.Handle, enabled, true);
        }
        catch
        {
        }
    }

    private void CleanAllInteriorPortals()
    {
        int deleted = _placedInteriorPortals.Count;
        Ped player = Game.Player.Character;

        DeletePlacementPreview();
        _placedInteriorPortals.Clear();
        _activeInteriorSession = null;
        _nextInteriorPortalUseAllowedAt = 0;
        _nextInteriorPortalHintAt = 0;
        ClearInteriorRenderingFocusSafe(player);

        ShowStatus("Nettoyage entrees/sorties: " + deleted.ToString(CultureInfo.InvariantCulture) + " repere(s) supprime(s).", 4500);
    }
}
