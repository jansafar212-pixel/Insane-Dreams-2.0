$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$before=Join-Path $root 'TestResults/HitFeedbackHotfix/RegressionBefore'
$after=Join-Path $root 'TestResults/HitFeedbackHotfix/RegressionArtifacts'
foreach($file in Get-ChildItem -LiteralPath $before -Recurse -File){
    $relative=$file.FullName.Substring($before.Length+1)
    $target=[IO.Path]::GetFullPath((Join-Path $root $relative))
    if(-not $target.StartsWith((Join-Path $root 'TestResults')+[IO.Path]::DirectorySeparatorChar)){throw 'Unexpected evidence target'}
    if((Test-Path -LiteralPath $target) -and (Get-FileHash -LiteralPath $target).Hash -ne (Get-FileHash -LiteralPath $file.FullName).Hash){
        $saved=Join-Path $after $relative
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($saved))|Out-Null
        Copy-Item -LiteralPath $target -Destination $saved
        $temp=$target+'.hotfix-restore'
        Copy-Item -LiteralPath $file.FullName -Destination $temp
        Move-Item -LiteralPath $temp -Destination $target -Force
    }
}
foreach($relative in @('TestResults/ReloadEconomy/DefaultPistolInsertionLeft.png','TestResults/ReloadEconomy/DefaultPistolInsertionRight.png')){
    if(Test-Path -LiteralPath $relative){$saved=Join-Path $after $relative;[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($saved))|Out-Null;Move-Item -LiteralPath $relative -Destination $saved}
}
'Regression captures preserved; historical artifacts restored to exact pre-run bytes, including prior uncommitted evidence.'
