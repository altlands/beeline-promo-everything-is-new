pushd %~dp0
pushd ..
powershell buildtools\build.ps1 -MainVersion 1.2.0 -BuildNumber 64 -BuildScript "buildtools\releaseToOctopus.fsx"
popd
popd