// Based on the example from: https://github.com/Azure-Samples/azure-iot-samples-csharp/tree/master/iot-hub/Samples/service/AutomaticDeviceManagementSample
// Prereqs (set following env vars):
//    - IOTHUB_DEVICE_CONN_STRING
//    - CONFIG_ID
// Other notes:
//  - This program reads from deployment.json file as its basis for the deployment.

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;

namespace Microsoft.Azure.Devices.Samples
{
  public class DeploySample
  {
    // Either set the IOTHUB_DEVICE_CONN_STRING environment variable or within launchSettings.json:
    private static string connectionString = Environment.GetEnvironmentVariable("IOTHUB_CONN_STRING_CSHARP");

    public async Task RunSampleAsync()
    {
      Console.WriteLine(connectionString);
      var registryManager = RegistryManager.CreateFromConnectionString(connectionString);      
      
      Console.WriteLine("Create configurations");
      await AddDeviceConfiguration(registryManager, Environment.GetEnvironmentVariable("CONFIG_ID")).ConfigureAwait(false);

      Console.WriteLine("List existing configurations");
      await GetConfigurations(registryManager, 1).ConfigureAwait(false);
    }

    private async Task AddDeviceConfiguration(RegistryManager registryManager, string configurationId)
    {
      Configuration configuration = new Configuration(configurationId);

      CreateModulesContent(configuration, configurationId);

      // Add target condition, using "*" for all devices
      configuration.TargetCondition = "*"; 

      await registryManager.AddConfigurationAsync(configuration).ConfigureAwait(false);

      Console.WriteLine("Configuration added, id: " + configurationId);
    }

    // Creates a module content based on a valid deployment.json file (relative to this file)
    /// <summary>
    /// Creates a module content based on a valid deployment.json file (relative to this file)
    /// </summary>
    /// <example>
    /// Example deployment.json payload structure
    /// Here "asaModule" could be any module name and any properties desired for that module
    /// <code>
    /// {
    ///   "modulesContent": {
    ///     "$edgeAgent": {
    ///       "properties.desired": {}
    ///     },
    ///     "$edgeHub": {
    ///       "properties.desired": {}
    ///     },
    ///     "asaModule": {
    ///       "properties.desired": {}
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>     
    private void CreateModulesContent(Configuration configuration, string configurationId)
    {
      configuration.Content = new ConfigurationContent();
      configuration.Content.ModulesContent = new Dictionary<string, IDictionary<string, object>>();

      // Read deployment.json file and convert to native objects
      var modulesContent = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(@"deployment.json"));

      const string PROPERTIES_DESIRED = "properties.desired";
      
      IDictionary<string, object> edge_agent = new Dictionary<string, object>();
      edge_agent[PROPERTIES_DESIRED] = modulesContent["modulesContent"]["$edgeAgent"][PROPERTIES_DESIRED];

      IDictionary<string, object> edge_hub = new Dictionary<string, object>();
      edge_hub[PROPERTIES_DESIRED] = modulesContent["modulesContent"]["$edgeHub"][PROPERTIES_DESIRED];

      IDictionary<string, object> asa_module = new Dictionary<string, object>();
      asa_module[PROPERTIES_DESIRED] = modulesContent["modulesContent"]["asaModule"][PROPERTIES_DESIRED];

      configuration.Content.ModulesContent["$edgeAgent"] = edge_agent;
      configuration.Content.ModulesContent["$edgeHub"] = edge_hub;
      configuration.Content.ModulesContent["asaModule"] = asa_module;
    }

    private async Task GetConfigurations(RegistryManager registryManager, int count)
    {
      IEnumerable<Configuration> configurations = await registryManager.GetConfigurationsAsync(count).ConfigureAwait(false);

      // Check configuration's metrics for expected conditions
      foreach (var configuration in configurations)
      {
        PrintConfiguration(configuration);
        Thread.Sleep(1000);
      }
      Console.WriteLine("Configurations received");
    }

    private void PrintConfiguration(Configuration configuration)
    {
      Console.WriteLine("Configuration Id: " + configuration.Id);
      Console.WriteLine("Configuration SchemaVersion: " + configuration.SchemaVersion);

      Console.WriteLine("Configuration Labels: " + configuration.Labels);

      PrintContent(configuration.ContentType, configuration.Content);

      Console.WriteLine("Configuration TargetCondition: " + configuration.TargetCondition);
      Console.WriteLine("Configuration CreatedTimeUtc: " + configuration.CreatedTimeUtc);
      Console.WriteLine("Configuration LastUpdatedTimeUtc: " + configuration.LastUpdatedTimeUtc);

      Console.WriteLine("Configuration Priority: " + configuration.Priority);

      Console.WriteLine("Configuration ETag: " + configuration.ETag);
      Console.WriteLine("------------------------------------------------------------");
    }

    private void PrintContent(string contentType, ConfigurationContent configurationContent)
    {
      Console.WriteLine("Configuration ContentType: " + contentType);

      Console.WriteLine("Configuration Content: " + configurationContent.ModulesContent);
      Console.WriteLine("Configuration Content: " + configurationContent.DeviceContent);
    }
  }
}
