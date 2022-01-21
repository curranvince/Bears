using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Object = System.Object;

namespace Bears
{
    [BepInPlugin("som.Bears", "Bears", "0.1.5")]

    public class Plugin : BaseUnityPlugin
    {
        private const string GUID = "som.Bears";
        private const string NAME = "Bears";
        private const string VERSION = "0.1.5";
        public static Plugin context;
        private ConfigFile config = new ConfigFile("som.Bears", true);
        private static readonly List<GameObject> NewPrefabs = new List<GameObject>();
        private static PieceTable hammer;
        public static GameObject rug;

        public static ConfigEntry<bool> bEnabled;
        public static ConfigEntry<string> sBiome;
        public static ConfigEntry<float> fSpawnInterval;
        public static ConfigEntry<float> fSpawnChance;
        public static ConfigEntry<int> iGroupSizeMin;
        public static ConfigEntry<int> iGroupSizeMax;
        public static ConfigEntry<int> iMaxSpawned;

        public static ConfigEntry<bool> bEnabledPolar;
        public static ConfigEntry<string> sBiomePolar;
        public static ConfigEntry<float> fSpawnIntervalPolar;
        public static ConfigEntry<float> fSpawnChancePolar;
        public static ConfigEntry<int> iGroupSizeMinPolar;
        public static ConfigEntry<int> iGroupSizeMaxPolar;
        public static ConfigEntry<int> iMaxSpawnedPolar;

        public static ConfigEntry<bool> bEnabledBlack;
        public static ConfigEntry<string> sBiomeBlack;
        public static ConfigEntry<float> fSpawnIntervalBlack;
        public static ConfigEntry<float> fSpawnChanceBlack;
        public static ConfigEntry<int> iGroupSizeMinBlack;
        public static ConfigEntry<int> iGroupSizeMaxBlack;
        public static ConfigEntry<int> iMaxSpawnedBlack;

        private void Awake()
        {
            // set default values;
            string[] aNames = new string[] { "Grizzly Bear", "Polar Bear", "Black Bear" };
            string[] aPrefabs = new string[] { "Bear", "PolarBear", "BlackBear" };
            bool[] aEnableds = new bool[] { true, true, true };
            string[] aBiomes = new string[] { "Mountain", "DeepNorth", "BlackForest" };
            float[] aIntervals = new float[] { 75f, 75f, 75f };
            float[] aChances = new float[] { 18f, 18f, 18f };
            int[] aGroupMins = new int[] { 1, 1, 1 };
            int[] aGroupMaxes = new int[] { 1, 1, 1 };
            int[] aSpawnedMaxes = new int[] { 2, 1, 2 };
            // make config file
            bEnabled = this.Config.Bind<bool>("Grizzly Bear", "bEnabled", aEnableds[0],
                "If the Grizzly Bear spawner is enabled");
            sBiome = this.Config.Bind<string>("Grizzly Bear", "sBiome", aBiomes[0],
                "Biome the Grizzly Bear can spawn in, make blank for all");
            fSpawnInterval = this.Config.Bind<float>("Grizzly Bear", "fSpawnInterval", aIntervals[0],
                "How often the game tries to spawn Grizzly Bears");
            fSpawnChance = this.Config.Bind<float>("Grizzly Bear", "fSpawnChance", aChances[0],
                "Chance of Grizzly Bear spawning each Interval");
            iGroupSizeMin = this.Config.Bind<int>("Grizzly Bear", "iGroupSizeMin", aGroupMins[0],
                "Minimum amount of Grizzly Bears to spawn at once");
            iGroupSizeMax = this.Config.Bind<int>("Grizzly Bear", "iGroupSizeMax", aGroupMaxes[0],
                "Maximum amount of Grizzly Bears to spawn at once");
            iMaxSpawned = this.Config.Bind<int>("Grizzly Bear", "iMaxSpawned", aSpawnedMaxes[0],
                "Max amount of Grizzly Bears that can be spawned near you");

            bEnabledPolar = this.Config.Bind<bool>("Polar Bear", "bEnabled", aEnableds[1],
                "If the Polar Bear spawner is enabled");
            sBiomePolar = this.Config.Bind<string>("Polar Bear", "sBiomePolar", aBiomes[1],
                "Biome the Polar Bear can spawn in, make blank for all");
            fSpawnIntervalPolar = this.Config.Bind<float>("Polar Bear", "fSpawnInterval", aIntervals[1],
                "How often the game tries to spawn Polar Bears");
            fSpawnChancePolar = this.Config.Bind<float>("Polar Bear", "fSpawnChance", aChances[1],
                "Chance of Polar Bear spawning each Interval");
            iGroupSizeMinPolar = this.Config.Bind<int>("Polar Bear", "iGroupSizeMin", aGroupMins[1],
                "Minimum amount of Polar Bears to spawn at once");
            iGroupSizeMaxPolar = this.Config.Bind<int>("Polar Bear", "iGroupSizeMax", aGroupMaxes[1],
                "Maximum amount of Polar Bears to spawn at once");
            iMaxSpawnedPolar = this.Config.Bind<int>("Polar Bear", "iMaxSpawned", aSpawnedMaxes[1],
                "Max amount of Polar Bears that can be spawned near you");

            bEnabledBlack = this.Config.Bind<bool>("Black Bear", "bEnabled", aEnableds[2],
                "If the Black Bear spawner is enabled");
            sBiomeBlack = this.Config.Bind<string>("Black Bear", "sBiome", aBiomes[2],
                "Biome the Black Bear can spawn in, make blank for all");
            fSpawnIntervalBlack = this.Config.Bind<float>("Black Bear", "fSpawnInterval", aIntervals[2],
                "How often the game tries to spawn Black Bears");
            fSpawnChanceBlack = this.Config.Bind<float>("Black Bear", "fSpawnChance", aChances[2],
                "Chance of Black Bear spawning each Interval");
            iGroupSizeMinBlack = this.Config.Bind<int>("Black Bear", "iGroupSizeMin", aGroupMins[2],
                "Minimum amount of Black Bears to spawn at once");
            iGroupSizeMaxBlack = this.Config.Bind<int>("Black Bear", "iGroupSizeMax", aGroupMaxes[2],
                "Maximum amount of Black Bears to spawn at once");
            iMaxSpawnedBlack = this.Config.Bind<int>("Black Bear", "iMaxSpawned", aSpawnedMaxes[2],
                "Max amount of Black Bears that can be spawned near you");

            //Get assets
            AssetBundle bundleFromResources = GetAssetBundleFromResources("bearbundle");
            foreach (string name in new List<string>()
            {
                "Assets/CustomAnimals/Bear/Prefabs/Bear.prefab",
                "Assets/CustomAnimals/Bear/Prefabs/Bear_cub.prefab",
                "Assets/CustomAnimals/Bear/Prefabs/PolarBear.prefab",
                "Assets/CustomAnimals/Bear/Prefabs/BlackBear.prefab",

                "Assets/CustomAnimals/Bear/Prefabs/rug_bear.prefab",
                "Assets/CustomAnimals/Bear/Prefabs/BearHide.prefab",
                "Assets/CustomAnimals/Bear/Prefabs/TrophyBear.prefab",

                "Assets/CustomAnimals/Bear/Attacks/Bear4legbite.prefab",
                "Assets/CustomAnimals/Bear/Attacks/Bear4legbiteF.prefab",
                "Assets/CustomAnimals/Bear/Attacks/Bear4legClawL.prefab",
                "Assets/CustomAnimals/Bear/Attacks/Bear4legClawR.prefab",
                "Assets/CustomAnimals/Bear/Attacks/Bear4legClawLF.prefab",
                "Assets/CustomAnimals/Bear/Attacks/Bear2legbite.prefab",
                "Assets/CustomAnimals/Bear/Attacks/Bear2legClawR.prefab",
                "Assets/CustomAnimals/Bear/Attacks/Bear2legClawL.prefab",

                "Assets/CustomAnimals/Bear/FX/sfx_bear_low.prefab",
                "Assets/CustomAnimals/Bear/FX/sfx_bear_roar.prefab",
                "Assets/CustomAnimals/Bear/FX/sfx_bear_sond.prefab",
                "Assets/CustomAnimals/Bear/FX/Bear_ragdoll.prefab",
                "Assets/CustomAnimals/Bear/FX/Bearcub_ragdoll.prefab",
                "Assets/CustomAnimals/Bear/FX/PolarBear_ragdoll.prefab",
                "Assets/CustomAnimals/Bear/FX/BlackBear_ragdoll.prefab",
                "Assets/CustomAnimals/Bear/FX/vfx_bear_hit.prefab",
                "Assets/CustomAnimals/Bear/FX/fx_bear_pet.prefab",
            })
            {
                NewPrefabs.Add(bundleFromResources.LoadAsset<GameObject>(name));
            }

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public static AssetBundle GetAssetBundleFromResources(string fileName)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string name =
                ((IEnumerable<string>) executingAssembly.GetManifestResourceNames()).Single<string>(
                    (Func<string, bool>) (str => str.EndsWith(fileName)));
            using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(name))
                return AssetBundle.LoadFromStream(manifestResourceStream);
        }

        private static void AddNewPrefabs()
        {
            if (ObjectDB.instance == (UnityEngine.Object) null || ObjectDB.instance.m_items.Count == 0 ||
                ObjectDB.instance.GetItemPrefab("Amber") == null)
                return;
            foreach (GameObject newPrefab in NewPrefabs)
            {
                if (newPrefab.GetComponent<ItemDrop>() != null &&
                    ObjectDB.instance.GetItemPrefab(newPrefab.name.GetHashCode()) == null)
                {
                    ObjectDB.instance.m_items.Add(newPrefab);
                    ((Dictionary<int, GameObject>) typeof(ObjectDB)
                        .GetField("m_itemByHash", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue((object) ObjectDB.instance))[newPrefab.name.GetHashCode()] = newPrefab;
                }
            }
        }

        //Add everything to ZNS
        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        public static class ZNetScene_Awake_Patch
        {
            public static void Prefix(ZNetScene __instance)
            {
                if (__instance == null)
                    return;
                __instance.m_prefabs.AddRange((IEnumerable<GameObject>) NewPrefabs);

                //Add rug to Hammer piece table
                foreach (PieceTable pieceTable in Resources.FindObjectsOfTypeAll(typeof(PieceTable)))
                {
                    if (pieceTable.gameObject.name.Contains("_HammerPieceTable"))
                    {
                        hammer = pieceTable;
                        break;
                    }
                }

                foreach (GameObject gameObject in NewPrefabs)
                {
                    if (gameObject.GetComponent<Piece>())
                        rug = gameObject;
                }

                Piece piece = rug.GetComponent<Piece>();
                piece.m_resources = new Piece.Requirement[1]
                {
                    new Piece.Requirement()
                    {
                        m_amount = 4,
                        m_resItem = ObjectDB.instance.GetItemPrefab("BearHide").GetComponent<ItemDrop>()
                    }
                };

                if (hammer.m_pieces.Contains(rug))
                    return;
                hammer.m_pieces.Add(rug);
            }
        }

        //Add things with ItemDrop to ODB
        [HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
        public static class ObjectDB_CopyOtherDB_Patch
        {
            public static void Postfix() => AddNewPrefabs();
        }

        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        public static class ObjectDB_Awake_Patch
        {
            public static void Postfix() => AddNewPrefabs();
        }

        //Add new spawner (w/ config)
        [HarmonyPatch(typeof(SpawnSystem))]
        public static class SpawnSystem_Patch
        {
            [HarmonyPatch("UpdateSpawnList")]
            private static void Prefix(SpawnSystem __instance, ref List<SpawnSystem.SpawnData> spawners)
            {
                if (bEnabled.Value) spawners.Add(GetBearData());
                if (bEnabledPolar.Value) spawners.Add(GetPolarBearData());
                if (bEnabledBlack.Value) spawners.Add(GetBlackBearData());
            }
        }

        private static SpawnSystem.SpawnData GetBearData()
        {
            SpawnSystem.SpawnData newSpawnData = new SpawnSystem.SpawnData
            {
                m_name = "Bear",
                m_prefab = ZNetScene.instance.GetPrefab("Bear"),
                m_maxLevel = 3,
                m_huntPlayer = false,

                m_spawnInterval = Plugin.fSpawnInterval.Value,
                m_spawnChance = Plugin.fSpawnChance.Value,
                m_groupSizeMin = Plugin.iGroupSizeMin.Value,
                m_groupSizeMax = Plugin.iGroupSizeMax.Value,
                m_maxSpawned = Plugin.iMaxSpawned.Value
            };

            if (sBiome.Value.Contains("Mountain")) { newSpawnData.m_biome = Heightmap.Biome.Mountain; }
            else if (sBiome.Value.Contains("Meadows")) { newSpawnData.m_biome = Heightmap.Biome.Meadows; }
            else if (sBiome.Value.Contains("BlackForest")) { newSpawnData.m_biome = Heightmap.Biome.BlackForest; }
            else if (sBiome.Value.Contains("Swamp")) { newSpawnData.m_biome = Heightmap.Biome.Swamp; }
            else if (sBiome.Value.Contains("Plains")) { newSpawnData.m_biome = Heightmap.Biome.Plains; }
            else if (sBiome.Value.Contains("AshLands")) { newSpawnData.m_biome = Heightmap.Biome.AshLands; }
            else if (sBiome.Value.Contains("DeepNorth")) { newSpawnData.m_biome = Heightmap.Biome.DeepNorth; }
            else if (sBiome.Value.Contains("Mistlands")) { newSpawnData.m_biome = Heightmap.Biome.Mistlands; }
            else if (sBiome.Value.Contains("Ocean")) { newSpawnData.m_biome = Heightmap.Biome.Ocean; }
            else { newSpawnData.m_biome = Heightmap.Biome.BiomesMax; }

            return newSpawnData;
        }

        private static SpawnSystem.SpawnData GetPolarBearData()
        {
            SpawnSystem.SpawnData newSpawnData = new SpawnSystem.SpawnData
            {
                m_name = "Polar Bear",
                m_prefab = ZNetScene.instance.GetPrefab("PolarBear"),
                m_maxLevel = 3,
                m_huntPlayer = false,

                m_spawnInterval = Plugin.fSpawnIntervalPolar.Value,
                m_spawnChance = Plugin.fSpawnChancePolar.Value,
                m_groupSizeMin = Plugin.iGroupSizeMinPolar.Value,
                m_groupSizeMax = Plugin.iGroupSizeMaxPolar.Value,
                m_maxSpawned = Plugin.iMaxSpawnedPolar.Value
            };

            if (sBiomePolar.Value.Contains("Mountain")) { newSpawnData.m_biome = Heightmap.Biome.Mountain; }
            else if (sBiomePolar.Value.Contains("Meadows")) { newSpawnData.m_biome = Heightmap.Biome.Meadows; }
            else if (sBiomePolar.Value.Contains("BlackForest")) { newSpawnData.m_biome = Heightmap.Biome.BlackForest; }
            else if (sBiomePolar.Value.Contains("Swamp")) { newSpawnData.m_biome = Heightmap.Biome.Swamp; }
            else if (sBiomePolar.Value.Contains("Plains")) { newSpawnData.m_biome = Heightmap.Biome.Plains; }
            else if (sBiomePolar.Value.Contains("AshLands")) { newSpawnData.m_biome = Heightmap.Biome.AshLands; }
            else if (sBiomePolar.Value.Contains("DeepNorth")) { newSpawnData.m_biome = Heightmap.Biome.DeepNorth; }
            else if (sBiomePolar.Value.Contains("Mistlands")) { newSpawnData.m_biome = Heightmap.Biome.Mistlands; }
            else if (sBiomePolar.Value.Contains("Ocean")) { newSpawnData.m_biome = Heightmap.Biome.Ocean; }
            else { newSpawnData.m_biome = Heightmap.Biome.BiomesMax; }

            return newSpawnData;
        }

        private static SpawnSystem.SpawnData GetBlackBearData()
        {
            SpawnSystem.SpawnData newSpawnData = new SpawnSystem.SpawnData
            {
                m_name = "Black Bear",
                m_prefab = ZNetScene.instance.GetPrefab("BlackBear"),
                m_maxLevel = 3,
                m_huntPlayer = false,

                m_spawnInterval = Plugin.fSpawnIntervalBlack.Value,
                m_spawnChance = Plugin.fSpawnChanceBlack.Value,
                m_groupSizeMin = Plugin.iGroupSizeMinBlack.Value,
                m_groupSizeMax = Plugin.iGroupSizeMaxBlack.Value,
                m_maxSpawned = Plugin.iMaxSpawnedBlack.Value
            };

            if (sBiomeBlack.Value.Contains("Mountain")) { newSpawnData.m_biome = Heightmap.Biome.Mountain; }
            else if (sBiomeBlack.Value.Contains("Meadows")) { newSpawnData.m_biome = Heightmap.Biome.Meadows; }
            else if (sBiomeBlack.Value.Contains("BlackForest")) { newSpawnData.m_biome = Heightmap.Biome.BlackForest; }
            else if (sBiomeBlack.Value.Contains("Swamp")) { newSpawnData.m_biome = Heightmap.Biome.Swamp; }
            else if (sBiomeBlack.Value.Contains("Plains")) { newSpawnData.m_biome = Heightmap.Biome.Plains; }
            else if (sBiomeBlack.Value.Contains("AshLands")) { newSpawnData.m_biome = Heightmap.Biome.AshLands; }
            else if (sBiomeBlack.Value.Contains("DeepNorth")) { newSpawnData.m_biome = Heightmap.Biome.DeepNorth; }
            else if (sBiomeBlack.Value.Contains("Mistlands")) { newSpawnData.m_biome = Heightmap.Biome.Mistlands; }
            else if (sBiomeBlack.Value.Contains("Ocean")) { newSpawnData.m_biome = Heightmap.Biome.Ocean; }
            else { newSpawnData.m_biome = Heightmap.Biome.BiomesMax; }

            return newSpawnData;
        }
    }
}

