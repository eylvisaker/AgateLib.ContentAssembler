try {
    mkdir -p "Test/App" -ea 0
    mkdir -p "Test/NugetFeed" -ea 0
    mkdir -p "Test/packages" -ea 0

    $testFolder = Resolve-Path "Test"
    $appFolder = Resolve-Path "$testFolder\App"
    $nugetFeed = Resolve-Path "$testFolder\NugetFeed"
    $packages = Resolve-Path "$testFolder\packages"
    
    echo "Packages folder: $packages"

    $env:NUGET_PACKAGES = $packages


    rm "$testFolder" -r -force 

    echo "Initializing a Nuget feed with our packages in it."
    echo "This assumes the build has completed and the .nupkg files are in the artifacts directory"
    mkdir -p "$testFolder/NugetFeed"

    nuget init "..\..\artifacts" "$testFolder/NugetFeed"

    echo "Installing monogame template and creating a new monogame project in $appFolder."
    Push-Location -Path $testFolder

    echo "Copying the test Game files"
    Copy-Item -recurse ..\AppSkeleton App
    #dotnet new --install "MonoGame.Templates.CSharp::3.8.0.1375-develop"
    #dotnet new "MonoGame Cross-Platform Desktop Application (OpenGL)" -o $appFolder --force

    #cp ..\Game1.cs $appFolder

    #mkdir $appFolder\ContentIn
    #cp ..\Pointer.png $appFolder\ContentIn
    #cp ..\content.alca $appFolder

    Write-Output "Adding AgateLib.ContentAssembler.Task to the project"
    Set-Location $appFolder

    dotnet add package AgateLib.ContentAssembler.Task --source "$nugetFeed" --package-directory "$packages"
    If ($lastExitCode -ne "0") {
        throw "FAILURE TO ADD PACKAGE"
    }

    dotnet restore --packages "$packages"
    If ($lastExitCode -ne "0") {
        throw "FAILURE TO RESTORE"
    }

    dotnet build
    If ($lastExitCode -ne "0") {
        throw "FAILURE TO BUILD TEST APP"
    }

    dotnet run
    If ($lastExitCode -ne "0") {
        throw "FAILURE TO RUN TEST APP"
    }
}
finally {
    Pop-Location
    Remove-Item env:NUGET_PACKAGES
}