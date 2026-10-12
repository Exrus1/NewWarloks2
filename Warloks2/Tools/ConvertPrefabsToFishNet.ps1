$ErrorActionPreference = "Stop"
$root = "C:\githubprojects\NewWarloks2\Warloks2"

function New-Meta($csPath, $guid) {
  $meta = "$csPath.meta"
  if (Test-Path $meta) {
    return (Select-String -Path $meta -Pattern '^guid: (.+)$').Matches[0].Groups[1].Value
  }
  @"
fileFormatVersion: 2
guid: $guid
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@ | Set-Content -Path $meta -NoNewline -Encoding ascii
  return $guid
}

$guidNetworkDespawn = New-Meta "$root\Assets\Scripts\NetworkDespawn.cs" "a1b2c3d4e5f6478899aabbccddeeff01"
$guidSimpleAnim = New-Meta "$root\Assets\Scripts\SimpleNetworkAnimator.cs" "b2c3d4e5f6a74899aabbccddeeff0202"
New-Meta "$root\Assets\Scripts\PlayerSession.cs" "c3d4e5f6a7b84900bbccddeeff030303" | Out-Null
New-Meta "$root\Assets\Scripts\NicknameBroadcast.cs" "d4e5f6a7b8c94a11ccddeeff04040404" | Out-Null
New-Meta "$root\Assets\Scripts\NetworkSceneHelper.cs" "e5f6a7b8c9da4b22ddeeff0505050505" | Out-Null

$photonGuids = @(
  "aa584fbee541324448dd18d8409c7a41",
  "627855c7f81362d41938ffe0b1475957",
  "64179f3720bbfe947b7724caa67b7c1d",
  "9b8c4a61274f60b4ea5fb4299cfdbf14"
)

function Convert-PrefabFile([string]$relPath, [string]$objectName, [bool]$isPlayer) {
  $path = Join-Path $root $relPath
  Write-Host "Converting $relPath ..."
  $lines = [System.Collections.Generic.List[string]]::new()
  $lines.AddRange([string[]][IO.File]::ReadAllLines($path))

  if (($lines | Where-Object { $_ -match "guid: 26b716c41e9b56b4baafaf13a523ba2e" }).Count -gt 0) {
    Write-Host "  Already has NetworkObject, skip"
    return
  }

  # Find and remove Photon MonoBehaviour blocks
  $removeIds = New-Object System.Collections.Generic.HashSet[string]
  for ($i = 0; $i -lt $lines.Count; $i++) {
    foreach ($pg in $photonGuids) {
      if ($lines[$i] -match "guid: $pg") {
        # walk back to --- !u!114 &
        $start = $i
        while ($start -gt 0 -and $lines[$start] -notmatch '^--- !u!114 &(-?\d+)') { $start-- }
        if ($lines[$start] -match '^--- !u!114 &(-?\d+)') {
          $fid = $Matches[1]
          [void]$removeIds.Add($fid)
          $end = $i + 1
          while ($end -lt $lines.Count -and $lines[$end] -notmatch '^--- !u!') { $end++ }
          Write-Host "  Removing block $fid lines $($start+1)-$end"
          $count = $end - $start
          $lines.RemoveRange($start, $count)
          $i = $start - 1
        }
      }
    }
  }

  # Remove component refs
  for ($i = $lines.Count - 1; $i -ge 0; $i--) {
    if ($lines[$i] -match 'component: \{fileID: (-?\d+)\}') {
      if ($removeIds.Contains($Matches[1])) {
        $lines.RemoveAt($i)
      }
    }
  }

  # Find root GO id by m_Name
  $goId = $null
  for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match "^\s*m_Name: $objectName\s*$") {
      $j = $i
      while ($j -gt 0 -and $lines[$j] -notmatch '^--- !u!1 &(\d+)') { $j-- }
      if ($lines[$j] -match '^--- !u!1 &(\d+)') {
        $goId = $Matches[1]
        break
      }
    }
  }
  if (-not $goId) { throw "Root GO not found for $objectName" }
  Write-Host "  Root GO $goId"

  $nobId = "9100000000000000001"
  $ntId = "9100000000000000002"
  $ndId = "9100000000000000003"
  $saId = "9100000000000000004"

  # Insert component entries after m_Component:
  for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match "^--- !u!1 &$goId\s*$") {
      for ($k = $i; $k -lt $lines.Count; $k++) {
        if ($lines[$k] -match '^\s*m_Component:\s*$') {
          $insert = @(
            "  - component: {fileID: $nobId}",
            "  - component: {fileID: $ntId}",
            "  - component: {fileID: $ndId}"
          )
          if ($isPlayer) { $insert += "  - component: {fileID: $saId}" }
          $lines.InsertRange($k + 1, [string[]]$insert)
          break
        }
      }
      break
    }
  }

  # Collect NB fileIDs on this GO (existing scripts)
  $nbIds = New-Object System.Collections.Generic.List[string]
  $scriptGuids = @(
    "0a7e6d4733f0554478af7eeebae8f6b4",
    "f046738f941c8494a9b8a7b9f1695613",
    "f23c22ff4a884ca498bfad9dc27ac544",
    "cfba0cd48082ec341b5e377b78d6816d",
    "56980da839a08764ab2271275d23b8bd",
    "fadc54827cf6491479dcc6d9b390572d",
    "0c9d0a66a55f88b4f844a617346f49cb",
    "a94e742f9dd10d142b464031235d02b6"
  )
  for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match '^--- !u!114 &(-?\d+)\s*$') {
      $fid = $Matches[1]
      $blockEnd = $i + 1
      while ($blockEnd -lt $lines.Count -and $lines[$blockEnd] -notmatch '^--- !u!') { $blockEnd++ }
      $block = ($lines.GetRange($i, [Math]::Max(1, $blockEnd - $i)) -join "`n")
      if ($block -match "m_GameObject: \{fileID: $goId\}") {
        foreach ($sg in $scriptGuids) {
          if ($block -match "guid: $sg") {
            $nbIds.Add($fid)
            # inject network object refs if missing
            $insertAt = $i + 1
            while ($insertAt -lt $blockEnd -and $lines[$insertAt] -notmatch 'm_EditorClassIdentifier') { $insertAt++ }
            if ($insertAt -lt $blockEnd -and $block -notmatch '_addedNetworkObject') {
              $lines.Insert($insertAt + 1, "  _componentIndexCache: 0")
              $lines.Insert($insertAt + 2, "  _addedNetworkObject: {fileID: $nobId}")
              $lines.Insert($insertAt + 3, "  _networkObjectCache: {fileID: $nobId}")
            }
            break
          }
        }
      }
    }
  }

  $nbIds.Add($ntId)
  $nbIds.Add($ndId)
  if ($isPlayer) { $nbIds.Add($saId) }
  $nbYaml = ($nbIds | ForEach-Object { "  - {fileID: $_}" }) -join "`r`n"

  $extra = @"
--- !u!114 &$nobId
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 26b716c41e9b56b4baafaf13a523ba2e, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  <IsNested>k__BackingField: 0
  WasActiveDuringEdit: 0
  WasActiveDuringEdit_Set1: 1
  <ComponentIndex>k__BackingField: 0
  <PredictedSpawn>k__BackingField: {fileID: 0}
  <PredictedOwner>k__BackingField: {fileID: 0}
  NetworkBehaviours:
$nbYaml
  InitializedParentNetworkBehaviour: {fileID: 0}
  InitializedNestedNetworkObjects: []
  RuntimeParentNetworkBehaviour: {fileID: 0}
  RuntimeChildNetworkBehaviours: []
  _isNetworked: 1
  _isSpawnable: 1
  _isGlobal: 0
  _initializeOrder: 0
  _preventDespawnOnDisconnect: 0
  _defaultDespawnType: 0
  NetworkObserver: {fileID: 0}
  _enablePrediction: 0
  _predictionType: 0
  _localReconcileCorrectionType: 2
  _graphicalObject: {fileID: 0}
  _detachGraphicalObject: 0
  _enableStateForwarding: 1
  _networkTransform: {fileID: $ntId}
  _ownerInterpolation: 1
  _ownerSmoothedProperties: 255
  _adaptiveInterpolation: 3
  _spectatorSmoothedProperties: 255
  _spectatorInterpolation: 1
  _enableTeleport: 0
  _teleportThreshold: 1
  <PrefabId>k__BackingField: 0
  <SpawnableCollectionId>k__BackingField: 0
  <AssetPathHash>k__BackingField: 0
  SceneId: 0
  SerializedTransformProperties:
    Position: {x: 0, y: 0, z: 0}
    Rotation: {x: 0, y: 0, z: 0, w: 1}
    Scale: {x: 1, y: 1, z: 1}
    IsValid: 0
--- !u!114 &$ntId
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a2836e36774ca1c4bbbee976e17b649c, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  _componentIndexCache: 0
  _addedNetworkObject: {fileID: $nobId}
  _networkObjectCache: {fileID: $nobId}
--- !u!114 &$ndId
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $guidNetworkDespawn, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  _componentIndexCache: 0
  _addedNetworkObject: {fileID: $nobId}
  _networkObjectCache: {fileID: $nobId}
"@

  if ($isPlayer) {
    $extra += @"

--- !u!114 &$saId
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $goId}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $guidSimpleAnim, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  _componentIndexCache: 0
  _addedNetworkObject: {fileID: $nobId}
  _networkObjectCache: {fileID: $nobId}
  animator: {fileID: 0}
"@
  }

  foreach ($line in ($extra -split "`r?`n")) {
    $lines.Add($line)
  }

  [IO.File]::WriteAllLines($path, $lines)
  Write-Host "  Saved"
}

Convert-PrefabFile "Assets\Resources\Player.prefab" "Player" $true
Convert-PrefabFile "Assets\Resources\Fireball.prefab" "Fireball" $false
Convert-PrefabFile "Assets\Resources\ElectricBall.prefab" "ElectricBall" $false
Convert-PrefabFile "Assets\Resources\WaterExplosion.prefab" "WaterExplosion" $false
Convert-PrefabFile "Assets\Resources\TeleportEffect.prefab" "TeleportEffect" $false

# NetworkManager
$prefabDir = "$root\Assets\Prefabs"
New-Item -ItemType Directory -Force -Path $prefabDir | Out-Null
$dpoGuid = "5dbde20e736456c4ca1327df4367f9db"
$nm = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100001
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 100002}
  - component: {fileID: 100003}
  - component: {fileID: 100004}
  - component: {fileID: 100005}
  - component: {fileID: 100006}
  m_Layer: 0
  m_Name: NetworkManager
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &100002
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100001}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children: []
  m_Father: {fileID: 0}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!114 &100003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100001}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: d2c95dfde7d73b54dbbdc23155d35d36, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  _logging: {fileID: 0}
  _spawnablePrefabs: {fileID: 11400000, guid: $dpoGuid, type: 2}
  _refreshDefaultPrefabs: 1
  _runInBackground: 1
  _dontDestroyOnLoad: 1
  _persistence: 0
--- !u!114 &100004
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100001}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 6f48f002b825cbd45a19bd96d90f9edb, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
--- !u!114 &100005
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100001}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 7d331f979d46e8e4a9fc90070c596d44, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  _defaultConditions: []
--- !u!114 &100006
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100001}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 57ce8bbb58966cb45a7140f32da5327a, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  _enableGlobalScenes: 1
  _startInOffline: 0
  _offlineScene: Assets/Scenes/Menu.unity
  _onlineScene: Assets/Scenes/Lobby.unity
  _replaceScenes: 0
"@
[IO.File]::WriteAllText("$prefabDir\NetworkManager.prefab", $nm)
@"
fileFormatVersion: 2
guid: f1a2b3c4d5e6477889abcdeffedcba98
PrefabImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@ | Set-Content "$prefabDir\NetworkManager.prefab.meta" -Encoding ascii
@"
fileFormatVersion: 2
guid: e9d8c7b6a5432109876543210fedcba9
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@ | Set-Content "$prefabDir.meta" -Encoding ascii

Write-Host "ALL DONE"
