param(
    [Parameter(Mandatory = $true)][string]$Name, 
    [Parameter(Mandatory = $true)][string]$ResourceGroupName)

# This is a script to create word press site hosted on app service. 

function GetRandomNumberString {

    $i = Get-Random -Minimum 1 -Maximum 100
    if ($i -gt 50) {
        $abc = "abcdefhgijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        $index = (Get-Random -Minimum 0 -Maximum $abc.Length)
        return $abc[$index]
    }

    return (Get-Random -Minimum 1 -Maximum 10).ToString()
}

function GetAppPassword {
    Add-Type -AssemblyName 'System.Web'
    $appPassword = [System.Web.Security.Membership]::GeneratePassword(15, 0)
    $replacements = @('=', '$', '?', '%', '(', ')', '/', '\', '|', '^', '.', ';', '{', '}', ':', '>', '<', '&', '*')
    for ($i = 0; $i -lt $replacements.Count; $i++) {
        $c = $replacements[$i]
        $r = GetRandomNumberString
        $appPassword = $appPassword.Replace($c, $r)
    }
    return $appPassword
}

$rg = $ResourceGroupName
$location = (az group show --name $rg | ConvertFrom-Json).location
$appName = $Name
$appPassword = GetAppPassword
$appUser = "appuser"
$dbName = "wordpress"

az mysql flexible-server create --location $location -g $rg -n $appName `
    --admin-user $appUser `
    --admin-password $appPassword `
    --sku-name Standard_B1ms `
    --tier Burstable `
    --public-access 0.0.0.0 `
    --storage-size 32

az mysql flexible-server db create -g $rg -s $appName -d $dbName

$appPlanName = $appName + "plan"
az appservice plan create --name $appPlanName -g $rg --location $location --sku B1 --is-linux
az webapp create -n $appName -g $rg -p $appPlanName -i wordpress
$identity = (az webapp identity assign -n $appName -g $rg | ConvertFrom-Json)

# Allow IP range from App Service to DB
$ip = (Invoke-RestMethod https://api.ipify.org?format=json).ip
az mysql flexible-server firewall-rule create -g $rg -n $appName --rule-name "dev-ip" --start-ip-address $ip
$ips = (az webapp show -g $rg -n $appName --query outboundIpAddresses --output tsv).Split(',')
for ($i = 0; $i -lt $ips.Length; $i++) {
    $cip = $ips[$i]    
    az mysql flexible-server firewall-rule create -g $rg -n $appName --rule-name "appservice$i-ip" --start-ip-address $cip
}

# Disable require SSL on MySQL database
az mysql flexible-server parameter set  -g $rg --server-name $appName --name require_secure_transport --value OFF

# Create an instance of Azure Key Vault
az keyvault create --location $location -g $rg -n $appName --retention-days 7 `
    --enable-soft-delete false `
    --public-network-access Enabled

# Set access policy for managed identity of App Service to access Azure key Vault
az keyvault set-policy --name $appName --object-id $identity.principalId --secret-permissions get

# Set secret for word press db password.
$wpdbSecretId = (az keyvault secret set --vault-name $appName --name "wpdbpassword" --value $appPassword | ConvertFrom-Json).id

$settings = @(
    @{
        name  = "WORDPRESS_DB_HOST";
        value = "$appName.mysql.database.azure.com";
    };
    @{
        name  = "WORDPRESS_DB_USER";
        value = $appUser;
    };
    @{
        name  = "WORDPRESS_DB_PASSWORD";
        value = "@Microsoft.KeyVault(SecretUri=$wpdbSecretId)";
    };
    @{
        name  = "WORDPRESS_DB_NAME";
        value = $dbName;
    };
)

Set-Content .\settings.json -Value ($settings | ConvertTo-Json) 
az webapp config appsettings set -g $rg -n $appName --settings "@settings.json"
Remove-Item .\settings.json -Force