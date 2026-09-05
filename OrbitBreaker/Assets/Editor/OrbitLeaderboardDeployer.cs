using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.DeploymentApi.Editor;
using Unity.Services.Core.Editor.Environments;
using UnityEditor;
using UnityEngine;

public static class OrbitLeaderboardDeployer
{
    private const string ConfigPath = "Assets/OrbitBreaker/Services/orbit_breaker_distance.lb";

    [MenuItem("Orbit Breaker/Deploy Global Leaderboard")]
    public static async void DeployGlobalLeaderboard()
    {
        AssetDatabase.ImportAsset(ConfigPath, ImportAssetOptions.ForceUpdate);
        await EnvironmentsApi.Instance.RefreshAsync();
        if (string.IsNullOrEmpty(EnvironmentsApi.Instance.ActiveEnvironmentName))
        {
            EnvironmentInfo environment = EnvironmentsApi.Instance.Environments.FirstOrDefault(item => item.IsDefault);
            if (environment.Id == System.Guid.Empty) environment = EnvironmentsApi.Instance.Environments.FirstOrDefault();
            if (environment.Id == System.Guid.Empty)
            {
                Debug.LogError("[OrbitLeaderboardDeploy] No Unity Services environment exists for this project.");
                return;
            }
            EnvironmentsApi.Instance.SetActiveEnvironment(environment);
            Debug.Log("[OrbitLeaderboardDeploy] Selected environment: " + environment.Name);
        }
        IDeploymentWindow deployment = Deployments.Instance.DeploymentWindow;
        if (deployment == null)
        {
            Debug.LogError("[OrbitLeaderboardDeploy] Deployment service is not ready.");
            return;
        }

        deployment.OpenWindow();
        await Task.Delay(750);
        deployment = Deployments.Instance.DeploymentWindow;

        List<IDeploymentItem> items = deployment.GetFromFiles(new[] { ConfigPath }).Where(item => item != null).ToList();
        if (items.Count == 0)
        {
            deployment.OpenWindow();
            Debug.LogError("[OrbitLeaderboardDeploy] Configuration not discovered. Open Services > Deployment and retry.");
            return;
        }

        Debug.Log("[OrbitLeaderboardDeploy] Deploying orbit_breaker_distance...");
        try
        {
            DeploymentResult<IDeploymentItem> result = await deployment.Deploy(items);
            bool failed = items.Any(item => item.Status.MessageSeverity == SeverityLevel.Error);
            if (failed)
                Debug.LogError("[OrbitLeaderboardDeploy] Deployment failed: " + string.Join(" | ", items.Select(item => item.Status.MessageDetail)));
            else
                Debug.Log("[OrbitLeaderboardDeploy] SUCCESS: " + result.Deployed.Count + " leaderboard configuration deployed.");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
