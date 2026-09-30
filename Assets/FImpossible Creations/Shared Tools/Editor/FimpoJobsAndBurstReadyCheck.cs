using System.Collections.Generic;
using FIMSpace.FEditor;
using UnityEditor;
using UnityEditor.PackageManager;
using PackageManagerPackageInfo = UnityEditor.PackageManager.PackageInfo;

[InitializeOnLoad]
internal static class FimpossibleJobsAndBurstReadyCheck
{
#if UNITY_2019_1_OR_NEWER
    private const string ReadyDefine = "FIMPOSSIBLE_JOBSANDBURST_READY";
    private const string BurstPackage = "com.unity.burst";
    private const string CollectionsPackage = "com.unity.collections";
    private const string MathematicsPackage = "com.unity.mathematics";

    static FimpossibleJobsAndBurstReadyCheck()
    {
        EditorApplication.delayCall += RefreshDefine;
        Events.registeringPackages += OnRegisteringPackages;
    }

    private static void OnRegisteringPackages(PackageRegistrationEventArgs packageRegistrationEventArgs)
    {
        if (HasRequiredPackageChanged(packageRegistrationEventArgs)) FDefinesCompilation.RemoveDefine(ReadyDefine);
    }

    private static bool HasRequiredPackageChanged(PackageRegistrationEventArgs packageRegistrationEventArgs)
    {
        return ContainsRequiredPackage(packageRegistrationEventArgs.added) ||
               ContainsRequiredPackage(packageRegistrationEventArgs.removed) ||
               ContainsRequiredPackage(packageRegistrationEventArgs.changedFrom) ||
               ContainsRequiredPackage(packageRegistrationEventArgs.changedTo);
    }

    private static bool ContainsRequiredPackage(IEnumerable<PackageManagerPackageInfo> packages)
    {
        if (packages == null) return false;

        foreach (PackageManagerPackageInfo package in packages)
        {
            if (package == null) continue;
            string packageName = package.name;
            if (packageName == BurstPackage || packageName == CollectionsPackage || packageName == MathematicsPackage) return true;
        }

        return false;
    }

    private static void RefreshDefine()
    {
        bool isReady = HasRequiredTypes();

        if (isReady)
            FDefinesCompilation.SetDefine(ReadyDefine);
        else
            FDefinesCompilation.RemoveDefine(ReadyDefine);
    }

    private static bool HasRequiredTypes()
    {
        return FDefinesCompilation.HasType("Unity.Burst.BurstCompileAttribute", "Unity.Burst") &&
               FDefinesCompilation.HasType("Unity.Jobs.IJob", "UnityEngine.CoreModule") &&
               FDefinesCompilation.HasType("Unity.Mathematics.float3", "Unity.Mathematics") &&
               FDefinesCompilation.HasType("Unity.Collections.NativeList`1", "Unity.Collections");
    }
#endif
}
