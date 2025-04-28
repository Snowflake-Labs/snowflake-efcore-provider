// <copyright file="TestUtil.cs" company="Snowflake Inc">
//         Copyright (c) 2019-2023 Snowflake Inc. All rights reserved.
//  </copyright>

using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EFCore.Tests.Common
{
    public class TestUtil
    {
        
        public static string GetConnectionStringFromParameters(string parametersJson, Action<TestConfig> overrideParams = null)
        {
            string connectionString;
            var reader = new StreamReader(parametersJson);
            var testConfigString = reader.ReadToEnd();
            // Local JSON settings to avoid using system wide settings which could be different 
            // than the default ones
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new DefaultNamingStrategy()
                }
            };
            var testConfigs = JsonConvert.DeserializeObject<Dictionary<string, TestConfig>>(testConfigString, jsonSettings);
            if (testConfigs == null)
            {
                throw new NoNullAllowedException(nameof(testConfigs));
            }
            if (testConfigs.TryGetValue("testconnection", out var testConnectionConfig))
            {
                overrideParams?.Invoke(testConnectionConfig);
                connectionString = testConnectionConfig.ConnectionString;
            }
            else
            {
                throw new NoNullAllowedException($"null `testconnection` in {parametersJson} config file");
            }

            return connectionString;
        }
    }
}