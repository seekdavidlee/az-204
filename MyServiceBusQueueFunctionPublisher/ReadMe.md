$appName = ""
$rg = ""
az servicebus namespace create -n $appName -g $rg

Invoke-RestMethod -UseBasicParsing -Body "{ \""bar\"": \""foo\"" }" -Method Post -Uri http://localhost:7071/api/MyServiceBusMessagePublisher

for($i = 0; $i -lt 50; $i++) { $localTimestamp = Get-Date; Invoke-WebRequest -UseBasicParsing -Body "{ ""value"": ""foo$i"", ""local"": ""$localTimestamp"" }" -Method Post -Uri http://localhost:7071/api/MyServiceBusMessagePublisher }