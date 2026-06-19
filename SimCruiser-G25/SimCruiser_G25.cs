using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace SimCruiser_G25;

[BepInPlugin(GUID, NAME, VERSION)]
public class SimCruiser_G25 : BaseUnityPlugin
{
    const string GUID = "com.BigSaltyBeans.SimCruiser_G25";
    const string NAME = "Sim Cruiser G25";
    const string VERSION = "1.2.0";
    
    private void Awake()
    {
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
        Logger.LogInfo("Patched Sim Cruiser G25");
    }
}