param([string[]]$Classes=@('Sprint0027','Sprint0026Tests','Sprint0224Tests','TraversalRegressionTests','Sprint022CoverTests','Sprint023Tests','ReactiveRagdollTests','Sprint0246Tests','FirstOperationTests','Level1CombatHotfixTests','Sprint024FlowTests'))
$cli='C:/Users/jansa/AppData/Local/Unity/bin/unity.exe'
foreach($class in $Classes){
 $start=& $cli command run_tests --mode playmode --filter $class --async_tests true --caller plugin --skill unity-cli --format json
 $start | Set-Content ('TestResults/ConcreteBunker/'+$class+'-start.json')
 if(-not ($start | ConvertFrom-Json).success){throw ('Failed starting '+$class)}
 $deadline=(Get-Date).AddMinutes(5)
 do {Start-Sleep -Seconds 5; $raw=& $cli command test_status --caller plugin --skill unity-cli --format json; $r=($raw | ConvertFrom-Json).data.result; if((Get-Date) -gt $deadline){throw ('Timed out '+$class)}}while($r.status -eq 'running')
 $raw | Set-Content ('TestResults/ConcreteBunker/'+$class+'.json')
 Write-Output ($class+': '+$r.status+' '+($r.summary | ConvertTo-Json -Compress))
 if($r.status -ne 'completed' -or $r.summary.failed -gt 0 -or $r.summary.total -eq 0){$r.results | Where-Object Status -ne 'Passed' | ConvertTo-Json -Depth 8;throw ('Check failed '+$class)}
 Start-Sleep -Seconds 2
}



