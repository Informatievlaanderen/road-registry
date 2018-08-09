$certificateLocation = "Cert:\CurrentUser\My"
$certificateRoot = "Cert:\CurrentUser\Root"

$executionPath = (Resolve-Path .\).Path
If(!$executionPath.ToLower().EndsWith('\cmds'))
{
  Write-Error "Script not executed from commands folder."
  Exit
}

$sourcePath = $executionPath.TrimEnd('\').Replace("\cmds", "\src\")
If(!(Test-Path $sourcePath))
{
  Write-Error "Source folder '$sourcePath' not found."
  Exit
}

function RemoveCertificate ([String] $url, [String[]] $files)
{
  # Remove installed certificates for url ($args[0])
  ($certificateLocation, $certificateRoot) | ForEach-Object {
    Get-ChildItem -Path $_ -DnsName $url | Remove-Item
  }

  # Remove files $args[1+]
  $files | ForEach-Object {
    if(Test-Path $_) {
      Remove-Item -Path $_
    }
  }
}

function CreateCertificate ([String] $register,[String] $url, [String] $project, [String] $password)
{
  $friendlyName = "Dns specific Certificate for " + $register + " .NET Core"
  $projectPath = $sourcePath + $project.TrimEnd('\') + "\"

  If(!(Test-Path $projectPath))
  {
    Write-Error "Project folder '$projectPath' not found. Certificate for '$url' not generated."
    return
  }

  $pfxFilePath = $projectPath + $url + ".pfx"
  $cerFilePath = $projectPath + $url + ".cer"

  # remove old certificate
  RemoveCertificate `
    -url $url `
    -files $pfxFilePath, $cerFilePath

  # setup certificate properties including the commonName (DNSName) property for Chrome 58+
  $certificate = New-SelfSignedCertificate `
      -Subject $url `
      -DnsName $url `
      -KeyAlgorithm RSA `
      -KeyLength 2048 `
      -NotBefore (Get-Date) `
      -NotAfter (Get-Date).AddYears(2) `
      -CertStoreLocation $certificateLocation `
      -FriendlyName $friendlyName `
      -HashAlgorithm SHA256 `
      -KeyUsage DigitalSignature, KeyEncipherment, DataEncipherment `
      -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")
  $certificatePath = $certificateLocation + "\" + ($certificate.ThumbPrint)

  # set certificate password here
  $pfxPassword = ConvertTo-SecureString -String $password -Force -AsPlainText

  # create pfx certificate
  Export-PfxCertificate -Cert $certificatePath -FilePath $pfxFilePath -Password $pfxPassword
  Export-Certificate -Cert $certificatePath -FilePath $cerFilePath

  # install the certificate
  ## import the pfx certificate
  Import-PfxCertificate -FilePath $pfxFilePath $certificateLocation -Password $pfxPassword -Exportable

  ## trust the certificate by importing the pfx certificate into your trusted root
  Import-Certificate -FilePath $cerFilePath -CertStoreLocation $certificateRoot

  Write-Output "Created and installed certificate for '$url'"
}

CreateCertificate `
  -register "wegenregister" `
  -url "develop-api.wegen.basisregisters.vlaanderen.be" `
  -project "RoadRegistry.API" `
  -password "!!dev-wegen-register"

CreateCertificate `
  -register "wegenregister" `
  -url "develop-ui.wegen.basisregisters.vlaanderen.be" `
  -project "RoadRegistry.UI" `
  -password "!!dev-wegen-register"
