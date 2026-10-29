param([string]$Mode,[string]$Filter)
$ErrorActionPreference='Stop'
$cli='C:/Users/jansa/AppData/Local/Unity/bin/unity.exe'
& $cli command cancel_tests --caller plugin --skill unity-cli --format json | Out-Null
Start-Sleep -Seconds 2
$start=& $cli command run_tests --caller plugin --skill unity-cli --mode $Mode --filter $Filter --async_tests --format json | Out-String | ConvertFrom-Json
if(-not $start.success){throw ($start|ConvertTo-Json -Depth 5)}
$deadline=[DateTime]::UtcNow.AddMinutes(7)
$run=@{status='running'}
do {
    Start-Sleep -Seconds 2
    if(Test-Path -LiteralPath Temp/pipeline_test_status.json){$run=Get-Content -LiteralPath Temp/pipeline_test_status.json -Raw | ConvertFrom-Json}
    if([DateTime]::UtcNow -gt $deadline){throw 'Test runner timeout'}
}while($run.status -eq 'running')
Copy-Item -LiteralPath Temp/pipeline_test_status.json -Destination ('TestResults/Sprint0039/'+$Mode+'_'+$Filter+'.json')
$run|Select-Object status,summary|ConvertTo-Json -Depth 4 -Compress
$run.results|Where-Object Status -ne Passed|Select-Object FullName,Message,StackTrace|ConvertTo-Json -Depth 3 -Compress
