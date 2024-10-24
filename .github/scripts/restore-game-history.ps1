## @file
# Restores the MTGO game history file for a specific user.
# Usage: .\restore-game-history.ps1 -username "TheQonfused"
#        .\restore-game-history.ps1 -username "TheQonfused" -force
##

[CmdletBinding(PositionalBinding=$false)]
param (
  [string]$username, # Username to search for (default: all users)
  [string]$filepath = $null, # File path to search for (default: all files)
  [switch]$force = $false # Force overwrite of existing game history files
)

function Error {
  param ([string]$message);
  Write-Host -ForegroundColor red $message;
  exit 1;
}
$ErrorActionPreference = "Stop";

# Create a function to search
function Search-Files {
  param ([string]$file);

  # Calculate the MD5 hash of the username (case-insensitive)
  $userHash = [System.Security.Cryptography.MD5]::Create().ComputeHash(
    [System.Text.Encoding]::ASCII.GetBytes($username.ToLower())
  );
  $hash = [string]::Join("",
      (0..($userHash.Length - 1) | % { $userHash[$_].ToString("X2") }));

  # Search the MTGO data directory for a specific user-data file
  $files = Get-ChildItem -Path "$env:LOCALAPPDATA\Apps\2.0\Data" -Recurse -File |
    Where-Object { $_.FullName -like "*\mtgo..tion_*\Data\AppFiles\$hash\$file" } |
    Sort-Object LastWriteTime -Descending;

  return $files;
}

# Check if the username is specified
if (-not $username) {
  Error "Please specify a username to search for.";
}

# Get the latest user data directory for the specified username
$userDirectory = (Search-Files -file "user_settings")[0].Directory;

if (!($force) -and (Test-Path -Path "$userDirectory\mtgo_game_history")) {
  Error -message (`
    "A 'mtgo_game_history' file already exists in the current installation.`n" +
    "Rerun the script with the -force switch to overwrite the existing file.");
}

if ($filepath -ne $null) {
  $gameHistoryFile = Get-Item -Path $filepath;
} else {
  $gameHistoryFile = (Search-Files -file "mtgo_game_history" |
    Where-Object { $_.Directory.FullName -ne $userDirectory.FullName })[0];
}

# Copy the old game history file to the current user directory
Copy-Item -Path $gameHistoryFile.FullName -Destination $userDirectory.FullName;

Write-Host (`
  "Successfully copied the game history file for user '$username' to the " +
  "current installation.`n" +
  "You may need to restart MTGO for the client to reflect these changes.`n");

Write-Host (`
  "Old file ($($gameHistoryFile.LastWriteTime)): $($gameHistoryFile.FullName)`n" +
  "New file ($($userDirectory.LastWriteTime)): $($userDirectory.FullName)\mtgo_game_history`n");
