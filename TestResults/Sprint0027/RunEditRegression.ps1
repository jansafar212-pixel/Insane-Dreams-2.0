$cli='C:/Users/jansa/AppData/Local/Unity/bin/unity.exe'
foreach($class in @('CombatRulesTests','SuppressionRulesTests','FirstOperationLayoutTests','Level1CombatHotfixRulesTests','Sprint024ProgressTests')){
 $raw=& $cli command run_tests --mode editor --filter $class --caller plugin --skill unity-cli --format json
 $raw | Set-Content ('TestResults/Sprint0027/Edit-'+$class+'.json')
 $r=($raw | ConvertFrom-Json).data.result
 Write-Output ($class+': '+($r.summary | ConvertTo-Json -Compress))
 if(-not ($raw | ConvertFrom-Json).success -or $r.summary.Failed -gt 0 -or $r.summary.Total -eq 0){$r.results | Where-Object Status -ne 'Passed' | ConvertTo-Json -Depth 6;throw ('Check failed '+$class)}
}
