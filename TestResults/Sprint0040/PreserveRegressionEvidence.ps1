$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$backup=Join-Path $root 'TestResults/Sprint0040/RegressionArtifacts'
# Only tracked historical evidence overwritten by this sprint's test runs.
$paths=@(git diff --name-only -- TestResults | Where-Object {$_ -notlike 'TestResults/Sprint0040/*' -and $_ -notlike 'TestResults/Sprint0039/*'})
foreach($relative in $paths){
    $target=[IO.Path]::GetFullPath((Join-Path $root $relative))
    if(-not $target.StartsWith((Join-Path $root 'TestResults')+[IO.Path]::DirectorySeparatorChar)){throw 'Outside evidence directory'}
    $saved=Join-Path $backup $relative
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($saved))|Out-Null
    if(-not (Test-Path -LiteralPath $saved)){Copy-Item -LiteralPath $target -Destination $saved}
    $psi=New-Object Diagnostics.ProcessStartInfo
    $psi.FileName='git'; $psi.Arguments='show HEAD:'+$relative
    $psi.WorkingDirectory=$root; $psi.UseShellExecute=$false; $psi.RedirectStandardOutput=$true
    $proc=[Diagnostics.Process]::Start($psi)
    $temp=$target+'.restore-tmp'
    $stream=[IO.File]::Create($temp)
    try{$proc.StandardOutput.BaseStream.CopyTo($stream)}finally{$stream.Dispose()}
    $proc.WaitForExit(); if($proc.ExitCode -ne 0){Copy-Item -LiteralPath $saved -Destination $target; throw 'Historical evidence restore failed'}
    Move-Item -LiteralPath $temp -Destination $target -Force
}
foreach($relative in @('TestResults/ReloadEconomy/DefaultPistolInsertionLeft.png','TestResults/ReloadEconomy/DefaultPistolInsertionRight.png')){
    if(Test-Path -LiteralPath $relative){$saved=Join-Path $backup $relative;[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($saved))|Out-Null;Move-Item -LiteralPath $relative -Destination $saved}
}
'Preserved '+$paths.Count+' overwritten historical evidence files under Sprint0040/RegressionArtifacts.'
