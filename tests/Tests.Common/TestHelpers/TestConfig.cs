using Newtonsoft.Json;

namespace EFCore.Tests.Common;

public class TestConfig
{
    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_USER", NullValueHandling = NullValueHandling.Ignore)]
    internal string user { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_PASSWORD", NullValueHandling = NullValueHandling.Ignore)]
    internal string password { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_ACCOUNT", NullValueHandling = NullValueHandling.Ignore)]
    internal string account { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_HOST", NullValueHandling = NullValueHandling.Ignore)]
    internal string host { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_PORT", NullValueHandling = NullValueHandling.Ignore)]
    internal string port { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_WAREHOUSE", NullValueHandling = NullValueHandling.Ignore)]
    internal string warehouse { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_DATABASE", NullValueHandling = NullValueHandling.Ignore)]
    internal string database { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_SCHEMA", NullValueHandling = NullValueHandling.Ignore)]
    internal string schema { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_ROLE", NullValueHandling = NullValueHandling.Ignore)]
    internal string role { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_PROTOCOL", NullValueHandling = NullValueHandling.Ignore)]
    internal string protocol { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_OKTA_USER", NullValueHandling = NullValueHandling.Ignore)]
    internal string oktaUser { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_OKTA_PASSWORD", NullValueHandling = NullValueHandling.Ignore)]
    internal string oktaPassword { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_OKTA_URL", NullValueHandling = NullValueHandling.Ignore)]
    internal string oktaUrl { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_JWT_USER", NullValueHandling = NullValueHandling.Ignore)]
    internal string jwtAuthUser { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_PEM_FILE", NullValueHandling = NullValueHandling.Ignore)]
    internal string pemFilePath { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_P8_FILE", NullValueHandling = NullValueHandling.Ignore)]
    internal string p8FilePath { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_PWD_PROTECTED_PK_FILE", NullValueHandling = NullValueHandling.Ignore)]
    internal string pwdProtectedPrivateKeyFilePath { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_PK_CONTENT", NullValueHandling = NullValueHandling.Ignore)]
    internal string privateKey { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_PROTECTED_PK_CONTENT", NullValueHandling = NullValueHandling.Ignore)]
    internal string pwdProtectedPrivateKey { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_PK_PWD", NullValueHandling = NullValueHandling.Ignore)]
    internal string privateKeyFilePwd { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_OAUTH_TOKEN", NullValueHandling = NullValueHandling.Ignore)]
    internal string oauthToken { get; set; }

    [JsonProperty(PropertyName = "SNOWFLAKE_TEST_EXP_OAUTH_TOKEN", NullValueHandling = NullValueHandling.Ignore)]
    internal string expOauthToken { get; set; }

    [JsonProperty(PropertyName = "PROXY_HOST", NullValueHandling = NullValueHandling.Ignore)]
    internal string proxyHost { get; set; }

    [JsonProperty(PropertyName = "PROXY_PORT", NullValueHandling = NullValueHandling.Ignore)]
    internal string proxyPort { get; set; }

    [JsonProperty(PropertyName = "AUTH_PROXY_HOST", NullValueHandling = NullValueHandling.Ignore)]
    internal string authProxyHost { get; set; }

    [JsonProperty(PropertyName = "AUTH_PROXY_PORT", NullValueHandling = NullValueHandling.Ignore)]
    internal string authProxyPort { get; set; }

    [JsonProperty(PropertyName = "AUTH_PROXY_USER", NullValueHandling = NullValueHandling.Ignore)]
    internal string authProxyUser { get; set; }

    [JsonProperty(PropertyName = "AUTH_PROXY_PWD", NullValueHandling = NullValueHandling.Ignore)]
    internal string authProxyPwd { get; set; }

    [JsonProperty(PropertyName = "NON_PROXY_HOSTS", NullValueHandling = NullValueHandling.Ignore)]
    internal string nonProxyHosts { get; set; }

    public TestConfig()
    {
        protocol = "https";
        port = "443";
    }

    public string ConnectionString =>
        $"schema={schema};host={host};port={port};protocol={protocol};" +
        $"account={account};role={role};db={database};warehouse={warehouse};" +
        $"user={user};{PasswordOrPrivateKey}";

    private string PasswordOrPrivateKey =>
        !string.IsNullOrEmpty(privateKeyFilePwd) 
            ? $"private_key_file={privateKeyFilePwd};authenticator=snowflake_jwt" 
            : $"password={password}";
}