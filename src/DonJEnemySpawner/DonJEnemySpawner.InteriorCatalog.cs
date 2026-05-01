using System.Collections.Generic;
using GTA.Math;

public sealed partial class DonJEnemySpawner
{
    private static List<InteriorCategory> BuildInteriorCategories()
    {
        return new List<InteriorCategory>
        {
            Cat("Online - bases criminelles",
                I("bunker_generic", "Bunker interieur generique", 892.6384f, -3245.8664f, -98.2645f, 180.0f),
                I("facility", "Doomsday Facility", 483.2006f, 4810.5405f, -58.91929f, 18.04706f),
                I("iaa_facility", "IAA Facility", 2151.137f, 2921.3303f, -61.90187f, 85.82783f),
                I("server_farm", "IAA Server Farm", 2158.1184f, 2920.9382f, -81.07539f, 270.48007f),
                I("smugglers_hangar", "Hangar Smuggler's Run", -1266.9995f, -3014.6135f, -49.51799f, 359.93738f),
                I("avenger", "Avenger", 520.0f, 4750.0f, -70.0f, 0.0f),
                I("submarine", "Sous-marin / Kosatka", 514.29266f, 4885.8706f, -62.58986f, 180.25909f),
                I("terrorbyte", "Terrorbyte", -1421.015f, -3012.587f, -80.0f, 0.0f),
                I("nightclub", "Nightclub", -1604.664f, -3012.583f, -78.0f, 0.0f),
                I("nightclub_warehouse", "Nightclub Warehouse", -1505.783f, -3012.587f, -80.0f, 0.0f),
                I("casino_nightclub", "Casino Nightclub", 1550.0f, 250.0f, -48.0f, 0.0f)
            ),

            Cat("Appartements online IPL",
                I("apt_modern_1", "Modern 1 Apartment", -786.8663f, 315.7642f, 217.6385f, 0.0f, "apa_v_mp_h_01_a"),
                I("apt_modern_2", "Modern 2 Apartment", -786.9563f, 315.6229f, 187.9136f, 0.0f, "apa_v_mp_h_01_c"),
                I("apt_modern_3", "Modern 3 Apartment", -774.0126f, 342.0428f, 196.6864f, 0.0f, "apa_v_mp_h_01_b"),
                I("apt_mody_1", "Mody 1 Apartment", -787.0749f, 315.8198f, 217.6386f, 0.0f, "apa_v_mp_h_02_a"),
                I("apt_mody_2", "Mody 2 Apartment", -786.8195f, 315.5634f, 187.9137f, 0.0f, "apa_v_mp_h_02_c"),
                I("apt_mody_3", "Mody 3 Apartment", -774.1382f, 342.0316f, 196.6864f, 0.0f, "apa_v_mp_h_02_b"),
                I("apt_vibrant_1", "Vibrant 1 Apartment", -786.6245f, 315.6175f, 217.6385f, 0.0f, "apa_v_mp_h_03_a"),
                I("apt_vibrant_2", "Vibrant 2 Apartment", -786.9584f, 315.7974f, 187.9135f, 0.0f, "apa_v_mp_h_03_c"),
                I("apt_vibrant_3", "Vibrant 3 Apartment", -774.0223f, 342.1718f, 196.6863f, 0.0f, "apa_v_mp_h_03_b"),
                I("apt_sharp_1", "Sharp 1 Apartment", -787.0902f, 315.7039f, 217.6384f, 0.0f, "apa_v_mp_h_04_a"),
                I("apt_sharp_2", "Sharp 2 Apartment", -787.0155f, 315.7071f, 187.9135f, 0.0f, "apa_v_mp_h_04_c"),
                I("apt_sharp_3", "Sharp 3 Apartment", -773.8976f, 342.1525f, 196.6863f, 0.0f, "apa_v_mp_h_04_b"),
                I("apt_monochrome_1", "Monochrome 1 Apartment", -786.9887f, 315.7393f, 217.6386f, 0.0f, "apa_v_mp_h_05_a"),
                I("apt_monochrome_2", "Monochrome 2 Apartment", -786.8809f, 315.6634f, 187.9136f, 0.0f, "apa_v_mp_h_05_c"),
                I("apt_monochrome_3", "Monochrome 3 Apartment", -774.0675f, 342.0773f, 196.6864f, 0.0f, "apa_v_mp_h_05_b"),
                I("apt_seductive_1", "Seductive 1 Apartment", -787.1423f, 315.6943f, 217.6384f, 0.0f, "apa_v_mp_h_06_a"),
                I("apt_seductive_2", "Seductive 2 Apartment", -787.0961f, 315.815f, 187.9135f, 0.0f, "apa_v_mp_h_06_c"),
                I("apt_seductive_3", "Seductive 3 Apartment", -773.9552f, 341.9892f, 196.6862f, 0.0f, "apa_v_mp_h_06_b"),
                I("apt_regal_1", "Regal 1 Apartment", -787.029f, 315.7113f, 217.6385f, 0.0f, "apa_v_mp_h_07_a"),
                I("apt_regal_2", "Regal 2 Apartment", -787.0574f, 315.6567f, 187.9135f, 0.0f, "apa_v_mp_h_07_c"),
                I("apt_regal_3", "Regal 3 Apartment", -774.0109f, 342.0965f, 196.6863f, 0.0f, "apa_v_mp_h_07_b"),
                I("apt_aqua_1", "Aqua 1 Apartment", -786.9469f, 315.5655f, 217.6383f, 0.0f, "apa_v_mp_h_08_a"),
                I("apt_aqua_2", "Aqua 2 Apartment", -786.9756f, 315.723f, 187.9134f, 0.0f, "apa_v_mp_h_08_c"),
                I("apt_aqua_3", "Aqua 3 Apartment", -774.0349f, 342.0296f, 196.6862f, 0.0f, "apa_v_mp_h_08_b")
            ),

            Cat("Appartements / maisons sans IPL",
                I("garage_2_car", "Garage 2 places", 173.2903f, -1003.6f, -99.65707f, 0.0f),
                I("garage_6_car", "Garage 6 places", 197.8153f, -1002.293f, -99.65749f, 0.0f),
                I("garage_10_car", "Garage 10 places", 229.9559f, -981.7928f, -99.66071f, 0.0f),
                I("apt_low_end", "Low End Apartment", 261.4586f, -998.8196f, -99.00863f, 0.0f),
                I("apt_medium_end", "Medium End Apartment", 347.2686f, -999.2955f, -99.19622f, 0.0f),
                I("apt_4_integrity_28", "4 Integrity Way, Apt 28", -18.07856f, -583.6725f, 79.46569f, 0.0f),
                I("apt_4_integrity_30", "4 Integrity Way, Apt 30", -35.31277f, -580.4199f, 88.71221f, 0.0f),
                I("apt_dell_perro_4", "Dell Perro Heights, Apt 4", -1468.14f, -541.815f, 73.4442f, 0.0f),
                I("apt_dell_perro_7", "Dell Perro Heights, Apt 7", -1477.14f, -538.7499f, 55.5264f, 0.0f),
                I("apt_richard_majestic_2", "Richard Majestic, Apt 2", -915.811f, -379.432f, 113.6748f, 0.0f),
                I("apt_tinsel_42", "Tinsel Towers, Apt 42", -614.86f, 40.6783f, 97.60007f, 0.0f),
                I("apt_eclipse_3", "Eclipse Towers, Apt 3", -773.407f, 341.766f, 211.397f, 0.0f),
                I("house_wild_oats", "3655 Wild Oats Drive", -169.286f, 486.4938f, 137.4436f, 0.0f),
                I("house_north_conker_2044", "2044 North Conker Avenue", 340.9412f, 437.1798f, 149.3925f, 0.0f),
                I("house_north_conker_2045", "2045 North Conker Avenue", 373.023f, 416.105f, 145.7006f, 0.0f),
                I("house_hillcrest_2862", "2862 Hillcrest Avenue", -676.127f, 588.612f, 145.1698f, 0.0f),
                I("house_hillcrest_2868", "2868 Hillcrest Avenue", -763.107f, 615.906f, 144.1401f, 0.0f),
                I("house_hillcrest_2874", "2874 Hillcrest Avenue", -857.798f, 682.563f, 152.6529f, 0.0f),
                I("house_whispymound", "2677 Whispymound Drive", 120.5f, 549.952f, 184.097f, 0.0f),
                I("house_mad_wayne", "2133 Mad Wayne Thunder", -1288.0f, 440.748f, 97.69459f, 0.0f)
            ),

            Cat("Bureaux CEO - Arcadius",
                I("arcadius_exec_rich", "Arcadius - Executive Rich", -141.1987f, -620.913f, 168.8205f, 0.0f, "ex_dt1_02_office_02b"),
                I("arcadius_exec_cool", "Arcadius - Executive Cool", -141.5429f, -620.9524f, 168.8204f, 0.0f, "ex_dt1_02_office_02c"),
                I("arcadius_exec_contrast", "Arcadius - Executive Contrast", -141.2896f, -620.9618f, 168.8204f, 0.0f, "ex_dt1_02_office_02a"),
                I("arcadius_old_warm", "Arcadius - Old Spice Warm", -141.4966f, -620.8292f, 168.8204f, 0.0f, "ex_dt1_02_office_01a"),
                I("arcadius_old_classical", "Arcadius - Old Spice Classical", -141.3997f, -620.9006f, 168.8204f, 0.0f, "ex_dt1_02_office_01b"),
                I("arcadius_old_vintage", "Arcadius - Old Spice Vintage", -141.5361f, -620.9186f, 168.8204f, 0.0f, "ex_dt1_02_office_01c"),
                I("arcadius_power_ice", "Arcadius - Power Broker Ice", -141.392f, -621.0451f, 168.8204f, 0.0f, "ex_dt1_02_office_03a"),
                I("arcadius_power_conservative", "Arcadius - Power Broker Conservative", -141.1945f, -620.8729f, 168.8204f, 0.0f, "ex_dt1_02_office_03b"),
                I("arcadius_power_polished", "Arcadius - Power Broker Polished", -141.4924f, -621.0035f, 168.8205f, 0.0f, "ex_dt1_02_office_03c"),
                I("arcadius_garage_1", "Arcadius - Garage 1", -191.0133f, -579.1428f, 135.0f, 0.0f, "imp_dt1_02_cargarage_a"),
                I("arcadius_garage_2", "Arcadius - Garage 2", -117.4989f, -568.1132f, 135.0f, 0.0f, "imp_dt1_02_cargarage_b"),
                I("arcadius_garage_3", "Arcadius - Garage 3", -136.0780f, -630.1852f, 135.0f, 0.0f, "imp_dt1_02_cargarage_c"),
                I("arcadius_mod_shop", "Arcadius - Mod Shop", -146.6166f, -596.6301f, 166.0f, 0.0f, "imp_dt1_02_modgarage")
            ),

            Cat("Bureaux CEO - Maze Bank",
                I("maze_exec_rich", "Maze Bank - Executive Rich", -75.8466f, -826.9893f, 243.3859f, 0.0f, "ex_dt1_11_office_02b"),
                I("maze_exec_cool", "Maze Bank - Executive Cool", -75.49945f, -827.05f, 243.386f, 0.0f, "ex_dt1_11_office_02c"),
                I("maze_exec_contrast", "Maze Bank - Executive Contrast", -75.49827f, -827.1889f, 243.386f, 0.0f, "ex_dt1_11_office_02a"),
                I("maze_old_warm", "Maze Bank - Old Spice Warm", -75.44054f, -827.1487f, 243.3859f, 0.0f, "ex_dt1_11_office_01a"),
                I("maze_old_classical", "Maze Bank - Old Spice Classical", -75.63942f, -827.1022f, 243.3859f, 0.0f, "ex_dt1_11_office_01b"),
                I("maze_old_vintage", "Maze Bank - Old Spice Vintage", -75.47446f, -827.2621f, 243.386f, 0.0f, "ex_dt1_11_office_01c"),
                I("maze_power_ice", "Maze Bank - Power Broker Ice", -75.56978f, -827.1152f, 243.3859f, 0.0f, "ex_dt1_11_office_03a"),
                I("maze_power_conservative", "Maze Bank - Power Broker Conservative", -75.51953f, -827.0786f, 243.3859f, 0.0f, "ex_dt1_11_office_03b"),
                I("maze_power_polished", "Maze Bank - Power Broker Polished", -75.41915f, -827.1118f, 243.3858f, 0.0f, "ex_dt1_11_office_03c"),
                I("maze_garage_1", "Maze Bank - Garage 1", -84.2193f, -823.0851f, 221.0f, 0.0f, "imp_dt1_11_cargarage_a"),
                I("maze_garage_2", "Maze Bank - Garage 2", -69.8627f, -824.7498f, 221.0f, 0.0f, "imp_dt1_11_cargarage_b"),
                I("maze_garage_3", "Maze Bank - Garage 3", -80.4318f, -813.2536f, 221.0f, 0.0f, "imp_dt1_11_cargarage_c"),
                I("maze_mod_shop", "Maze Bank - Mod Shop", -73.9039f, -821.6204f, 284.0f, 0.0f, "imp_dt1_11_modgarage")
            ),

            Cat("Bureaux CEO - LOM Bank",
                I("lom_exec_rich", "LOM Bank - Executive Rich", -1579.756f, -565.0661f, 108.523f, 0.0f, "ex_sm_13_office_02b"),
                I("lom_exec_cool", "LOM Bank - Executive Cool", -1579.678f, -565.0034f, 108.5229f, 0.0f, "ex_sm_13_office_02c"),
                I("lom_exec_contrast", "LOM Bank - Executive Contrast", -1579.583f, -565.0399f, 108.5229f, 0.0f, "ex_sm_13_office_02a"),
                I("lom_old_warm", "LOM Bank - Old Spice Warm", -1579.702f, -565.0366f, 108.5229f, 0.0f, "ex_sm_13_office_01a"),
                I("lom_old_classical", "LOM Bank - Old Spice Classical", -1579.643f, -564.9685f, 108.5229f, 0.0f, "ex_sm_13_office_01b"),
                I("lom_old_vintage", "LOM Bank - Old Spice Vintage", -1579.681f, -565.0003f, 108.523f, 0.0f, "ex_sm_13_office_01c"),
                I("lom_power_ice", "LOM Bank - Power Broker Ice", -1579.677f, -565.0689f, 108.5229f, 0.0f, "ex_sm_13_office_03a"),
                I("lom_power_conservative", "LOM Bank - Power Broker Conservative", -1579.708f, -564.9634f, 108.5229f, 0.0f, "ex_sm_13_office_03b"),
                I("lom_power_polished", "LOM Bank - Power Broker Polished", -1579.693f, -564.8981f, 108.5229f, 0.0f, "ex_sm_13_office_03c"),
                I("lom_garage_1", "LOM Bank - Garage 1", -1581.1120f, -567.2450f, 85.5f, 0.0f, "imp_sm_13_cargarage_a"),
                I("lom_garage_2", "LOM Bank - Garage 2", -1568.7390f, -562.0455f, 85.5f, 0.0f, "imp_sm_13_cargarage_b"),
                I("lom_garage_3", "LOM Bank - Garage 3", -1563.5570f, -574.4314f, 85.5f, 0.0f, "imp_sm_13_cargarage_c"),
                I("lom_mod_shop", "LOM Bank - Mod Shop", -1578.0230f, -576.4251f, 104.2f, 0.0f, "imp_sm_13_modgarage")
            ),

            Cat("Bureaux CEO - Maze Bank West",
                I("maze_west_exec_rich", "Maze Bank West - Executive Rich", -1392.667f, -480.4736f, 72.04217f, 0.0f, "ex_sm_15_office_02b"),
                I("maze_west_exec_cool", "Maze Bank West - Executive Cool", -1392.542f, -480.4011f, 72.04211f, 0.0f, "ex_sm_15_office_02c"),
                I("maze_west_exec_contrast", "Maze Bank West - Executive Contrast", -1392.626f, -480.4856f, 72.04212f, 0.0f, "ex_sm_15_office_02a"),
                I("maze_west_old_warm", "Maze Bank West - Old Spice Warm", -1392.617f, -480.6363f, 72.04208f, 0.0f, "ex_sm_15_office_01a"),
                I("maze_west_old_classical", "Maze Bank West - Old Spice Classical", -1392.532f, -480.7649f, 72.04207f, 0.0f, "ex_sm_15_office_01b"),
                I("maze_west_old_vintage", "Maze Bank West - Old Spice Vintage", -1392.611f, -480.5562f, 72.04214f, 0.0f, "ex_sm_15_office_01c"),
                I("maze_west_power_ice", "Maze Bank West - Power Broker Ice", -1392.563f, -480.549f, 72.0421f, 0.0f, "ex_sm_15_office_03a"),
                I("maze_west_power_conservative", "Maze Bank West - Power Broker Conservative", -1392.528f, -480.475f, 72.04206f, 0.0f, "ex_sm_15_office_03b"),
                I("maze_west_power_polished", "Maze Bank West - Power Broker Polished", -1392.416f, -480.7485f, 72.04207f, 0.0f, "ex_sm_15_office_03c"),
                I("maze_west_garage_1", "Maze Bank West - Garage 1", -1388.8400f, -478.7402f, 56.1f, 0.0f, "imp_sm_15_cargarage_a"),
                I("maze_west_garage_2", "Maze Bank West - Garage 2", -1388.8600f, -478.7574f, 48.1f, 0.0f, "imp_sm_15_cargarage_b"),
                I("maze_west_garage_3", "Maze Bank West - Garage 3", -1374.6820f, -474.3586f, 56.1f, 0.0f, "imp_sm_15_cargarage_c"),
                I("maze_west_mod_shop", "Maze Bank West - Mod Shop", -1391.2450f, -473.9638f, 77.2f, 0.0f, "imp_sm_15_modgarage")
            ),

            Cat("MC / entrepots / business",
                I("clubhouse_1", "Clubhouse 1", 1107.04f, -3157.399f, -37.51859f, 0.0f, "bkr_biker_interior_placement_interior_0_biker_dlc_int_01_milo"),
                I("clubhouse_2", "Clubhouse 2", 998.4809f, -3164.711f, -38.90733f, 0.0f, "bkr_biker_interior_placement_interior_1_biker_dlc_int_02_milo"),
                I("meth_lab", "Meth Lab", 1009.5f, -3196.6f, -38.99682f, 0.0f, "bkr_biker_interior_placement_interior_2_biker_dlc_int_ware01_milo"),
                I("weed_farm", "Weed Farm", 1051.491f, -3196.536f, -39.14842f, 0.0f, "bkr_biker_interior_placement_interior_3_biker_dlc_int_ware02_milo"),
                I("cocaine_lockup", "Cocaine Lockup", 1093.6f, -3196.6f, -38.99841f, 0.0f, "bkr_biker_interior_placement_interior_4_biker_dlc_int_ware03_milo"),
                I("counterfeit_cash", "Counterfeit Cash Factory", 1121.897f, -3195.338f, -40.4025f, 0.0f, "bkr_biker_interior_placement_interior_5_biker_dlc_int_ware04_milo"),
                I("document_forgery", "Document Forgery Office", 1165.0f, -3196.6f, -39.01306f, 0.0f, "bkr_biker_interior_placement_interior_6_biker_dlc_int_ware05_milo"),
                I("warehouse_small", "Warehouse Small", 1094.988f, -3101.776f, -39.00363f, 0.0f, "ex_exec_warehouse_placement_interior_1_int_warehouse_s_dlc_milo"),
                I("warehouse_medium", "Warehouse Medium", 1056.486f, -3105.724f, -39.00439f, 0.0f, "ex_exec_warehouse_placement_interior_0_int_warehouse_m_dlc_milo"),
                I("warehouse_large", "Warehouse Large", 1006.967f, -3102.079f, -39.0035f, 0.0f, "ex_exec_warehouse_placement_interior_2_int_warehouse_l_dlc_milo"),
                I("vehicle_warehouse", "Vehicle Warehouse", 994.5925f, -3002.594f, -39.64699f, 0.0f, "imp_impexp_interior_placement_interior_1_impexp_intwaremed_milo_"),
                I("lost_mc_clubhouse", "Lost MC Clubhouse", 982.0083f, -100.8747f, 74.84512f, 0.0f, "bkr_bi_hw1_13_int")
            ),

            Cat("Diamond Casino & Resort",
                I("casino_main", "Casino", 1100.0f, 220.0f, -50.0f, 0.0f, "vw_casino_main"),
                I("casino_garage", "Casino Garage", 1295.0f, 230.0f, -50.0f, 0.0f, "vw_casino_garage"),
                I("casino_carpark", "Casino Car Park", 1380.0f, 200.0f, -50.0f, 0.0f, "vw_casino_carpark"),
                I("casino_penthouse", "Casino Penthouse", 976.636f, 70.295f, 115.164f, 0.0f, "vw_casino_penthouse")
            ),

            Cat("Histoire / missions sans IPL",
                I("char_creator", "CharCreator", 402.5164f, -1002.847f, -99.2587f, 0.0f),
                I("mission_carpark", "Mission Carpark", 405.9228f, -954.1149f, -99.6627f, 0.0f),
                I("torture_room", "Torture Room", 136.5146f, -2203.149f, 7.30914f, 0.0f),
                I("solomon_office", "Solomon's Office", -1005.84f, -478.92f, 50.02733f, 0.0f),
                I("psychiatrist_office", "Psychiatrist's Office", -1908.024f, -573.4244f, 19.09722f, 0.0f),
                I("omega_garage", "Omega's Garage", 2331.344f, 2574.073f, 46.68137f, 0.0f),
                I("movie_theatre", "Movie Theatre", -1427.299f, -245.1012f, 16.8039f, 0.0f),
                I("motel", "Motel", 152.2605f, -1004.471f, -98.99999f, 0.0f),
                I("madrazo_ranch", "Madrazo's Ranch", 1399.0f, 1150.0f, 115.0f, 0.0f),
                I("life_invader_office", "Life Invader Office", -1044.193f, -236.9535f, 37.96496f, 0.0f),
                I("lester_house", "Lester's House", 1273.9f, -1719.305f, 54.77141f, 0.0f),
                I("fbi_top_floor", "FBI Top Floor", 134.5835f, -749.339f, 258.152f, 0.0f),
                I("fbi_floor_47", "FBI Floor 47", 134.5835f, -766.486f, 234.152f, 0.0f),
                I("fbi_floor_49", "FBI Floor 49", 134.635f, -765.831f, 242.152f, 0.0f),
                I("iaa_office", "IAA Office", 117.22f, -620.938f, 206.1398f, 0.0f)
            ),

            Cat("Lieux speciaux IPL",
                I("cargo_ship_normal", "Normal Cargo Ship", -163.3628f, -2385.161f, 5.999994f, 0.0f, "cargoship"),
                I("cargo_ship_sunken", "Sunken Cargo Ship", -163.3628f, -2385.161f, 5.999994f, 0.0f, "sunkcargoship"),
                I("cargo_ship_burning", "Burning Cargo Ship", -163.3628f, -2385.161f, 5.999994f, 0.0f, "SUNK_SHIP_FIRE"),
                I("red_carpet", "Red Carpet", 300.5927f, 300.5927f, 104.3776f, 0.0f, "redCarpet"),
                I("union_depository", "Union Depository", 2.6968f, -667.0166f, 16.13061f, 0.0f, "FINBANK"),
                I("trevor_trailer_dirty", "Trevor's Trailer Dirty", 1975.552f, 3820.538f, 33.44833f, 0.0f, "TrevorsMP"),
                I("trevor_trailer_clean", "Trevor's Trailer Clean", 1975.552f, 3820.538f, 33.44833f, 0.0f, "TrevorsTrailerTidy"),
                I("stadium", "Stadium", -248.6731f, -2010.603f, 30.14562f, 0.0f, "SP1_10_real_interior"),
                I("max_renda_shop", "Max Renda Shop", -585.8247f, -282.72f, 35.45475f, 0.0f, "refit_unload"),
                I("jewel_store", "Jewel Store", -630.07f, -236.332f, 38.05704f, 0.0f, "post_hiest_unload"),
                I("fib_lobby", "FIB Lobby", 110.4f, -744.2f, 45.7496f, 0.0f, "FIBlobby"),
                I("morgue", "Morgue", 275.446f, -1361.11f, 24.5378f, 0.0f, "coronertrash", "Coroner_Int_On"),
                I("lester_factory", "Lester's Factory", 716.84f, -962.05f, 31.59f, 0.0f, "id2_14_during_door", "id2_14_during1"),
                I("oneil_farm", "O'Neil Farm", 2469.03f, 4955.278f, 45.11892f, 0.0f, "farm", "farm_lod", "farm_props", "farm_int"),
                I("oneil_farm_burnt", "O'Neil Farm Burnt", 2469.03f, 4955.278f, 45.11892f, 0.0f, "farmint", "farm_burnt", "farm_burnt_props", "des_farmhouse", "des_farmhs_endimap", "des_farmhs_end_occl"),
                I("gunrunning_yacht", "Gunrunning Heist Yacht", 1373.828f, 6737.393f, 6.707596f, 0.0f, "gr_heist_yacht2", "gr_heist_yacht2_bar", "gr_heist_yacht2_bedrm", "gr_heist_yacht2_bridge", "gr_heist_yacht2_enginrm", "gr_heist_yacht2_lounge"),
                I("dignity_heist_yacht", "Dignity Heist Yacht", -2027.946f, -1036.695f, 6.707587f, 0.0f, "hei_yacht_heist", "hei_yacht_heist_enginrm", "hei_yacht_heist_Lounge", "hei_yacht_heist_Bridge", "hei_yacht_heist_Bar", "hei_yacht_heist_Bedrm", "hei_yacht_heist_DistantLights", "hei_yacht_heist_LODLights"),
                I("dignity_party_yacht", "Dignity Party Yacht", -2023.643f, -1038.119f, 5.576781f, 0.0f, "smboat", "smboat_lod"),
                I("aircraft_carrier", "Aircraft Carrier", 3084.73f, -4770.709f, 15.26167f, 0.0f, "hei_carrier", "hei_carrier_DistantLights", "hei_Carrier_int1", "hei_Carrier_int2", "hei_Carrier_int3", "hei_Carrier_int4", "hei_Carrier_int5", "hei_Carrier_int6", "hei_carrier_LODLights"),
                I("north_yankton", "North Yankton", 3217.697f, -4834.826f, 111.8152f, 0.0f, "prologue01", "prologue01c", "prologue01d", "prologue01e", "prologue01f", "prologue01g", "prologue01h", "prologue01i", "prologue01j", "prologue01k", "prologue01z", "prologue02", "prologue03", "prologue03b", "prologue03_grv_dug", "prologue_grv_torch", "prologue04", "prologue04b", "prologue04_cover", "des_protree_end", "des_protree_start", "prologue05", "prologue05b", "prologue06", "prologue06b", "prologue06_int", "prologue06_pannel", "plg_occl_00", "prologue_occl", "prologuerd", "prologuerdb")
            )
        };
    }

    private static InteriorCategory Cat(string name, params InteriorOption[] options)
    {
        InteriorCategory category = new InteriorCategory
        {
            Name = name,
            Options = new List<InteriorOption>()
        };

        if (options != null)
        {
            for (int i = 0; i < options.Length; i++)
            {
                InteriorOption option = options[i];

                if (option == null)
                {
                    continue;
                }

                option.Category = name;
                category.Options.Add(option);
            }
        }

        return category;
    }

    private static InteriorOption I(string id, string displayName, float x, float y, float z, float heading, params string[] ipls)
    {
        List<string> cleanedIpls = new List<string>();

        if (ipls != null)
        {
            for (int i = 0; i < ipls.Length; i++)
            {
                string ipl = ipls[i];

                if (!string.IsNullOrWhiteSpace(ipl))
                {
                    cleanedIpls.Add(ipl.Trim().Trim('\uFEFF'));
                }
            }
        }

        return new InteriorOption
        {
            Id = id,
            Category = string.Empty,
            DisplayName = displayName,
            Position = new Vector3(x, y, z),
            Heading = heading,
            Ipls = cleanedIpls
        };
    }
}
