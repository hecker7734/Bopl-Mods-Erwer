$OutputDir = 'bin\Debug\net46';  
$ZipName = 'repviewer.zip';  
$ZipPath = Join-Path $OutputDir $ZipName;  

# Create a sample text file for testing
New-Item -Path (Join-Path $OutputDir 'test.txt') -ItemType File -Force

# Zip the test file
Add-Type -Assembly 'System.IO.Compression.FileSystem';  
$zip = [System.IO.Compression.ZipFile]::Open($ZipPath, 'Create');  
[System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, (Join-Path $OutputDir 'test.txt'), 'test.txt');  
$zip.Dispose();  
Write-Host 'Zip completed successfully.';  
