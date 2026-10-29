$ErrorActionPreference='Stop'
$apk=Join-Path (Get-Location).Path 'build/HitFeedbackHotfix/AKUMU2-hit-feedback-hotfix-release.apk'
$sdk='C:/Program Files/Unity/Hub/Editor/6000.6.6f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK'
& "$sdk/build-tools/36.0.0/aapt.exe" dump badging $apk | Set-Content TestResults/HitFeedbackHotfix/ApkBadging.txt
$badging=Get-Content TestResults/HitFeedbackHotfix/ApkBadging.txt -Raw
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[IO.Compression.ZipFile]::OpenRead($apk)
try{
    $entry=$zip.Entries|Where-Object FullName -like '*global-metadata.dat'
    $reader=[IO.StreamReader]::new($entry.Open(),[Text.Encoding]::UTF8)
    try{$metadata=$reader.ReadToEnd()}finally{$reader.Dispose()}
    $symbols=@('CancelDamageFlash','CacheTintRenderers','pendingLivingImpact','damageFlashMaterial')
    $audit=@($symbols|ForEach-Object {[pscustomobject]@{symbol=$_;present=$metadata.Contains($_)}})
    $audit|ConvertTo-Json|Set-Content TestResults/HitFeedbackHotfix/ApkMetadataAudit.json
}finally{$zip.Dispose()}
$identity=[pscustomobject]@{
    path=$apk;bytes=(Get-Item -LiteralPath $apk).Length;sha256=(Get-FileHash -LiteralPath $apk -Algorithm SHA256).Hash
    version0141=$badging.Contains("versionName='0.14.1'");versionCode4=$badging.Contains("versionCode='4'")
    arm64=$badging.Contains("native-code: 'arm64-v8a'");debuggable=$badging.Contains('application-debuggable')
}
$identity|ConvertTo-Json|Set-Content TestResults/HitFeedbackHotfix/ApkIdentity.json
& "$sdk/platform-tools/adb.exe" devices | Set-Content TestResults/HitFeedbackHotfix/AdbDevices.txt
$identity|ConvertTo-Json -Compress
$audit|ConvertTo-Json -Compress
