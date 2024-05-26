Creating automatic NuGet packages for your library each time you create a tag on GitHub can be achieved using GitHub Actions. Here’s a step-by-step guide to set this up:

### Step 1: Prepare Your Project
Ensure your project is properly configured for NuGet packaging. This includes a valid `.csproj` file with the necessary metadata for NuGet.

Example `.csproj` configuration:
```xml
<PropertyGroup>
  <Version>1.0.0</Version>
  <PackageId>YourPackageId</PackageId>
  <Authors>YourName</Authors>
  <Description>Your package description</Description>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageProjectUrl>https://github.com/yourusername/your-repo</PackageProjectUrl>
  <RepositoryUrl>https://github.com/yourusername/your-repo</RepositoryUrl>
  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
</PropertyGroup>
```

### Step 2: Create a GitHub Action Workflow
Create a GitHub Actions workflow file to automate the packaging and publishing process. Add the following file to your repository: `.github/workflows/nuget-publish.yml`.

Example `nuget-publish.yml`:
```yaml
name: Publish NuGet Package

on:
  push:
    tags:
      - 'v*'  # Matches tags starting with "v"

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
    - name: Checkout code
      uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '7.0.x'  # Specify the .NET version you're using

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Pack
      run: dotnet pack --configuration Release --no-restore --output ./nupkg

    - name: Publish NuGet package
      env:
        NUGET_API_KEY: ${{ secrets.NUGET_API_KEY }}
      run: dotnet nuget push ./nupkg/*.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

### Step 3: Set Up NuGet API Key
To publish packages to NuGet.org, you need an API key. Follow these steps:

1. Go to [NuGet.org](https://www.nuget.org/) and log in to your account.
2. Navigate to your profile, then to the "API Keys" section.
3. Create a new API key with the desired scope and permissions.
4. Copy the generated API key.

### Step 4: Add NuGet API Key to GitHub Secrets
1. Go to your GitHub repository.
2. Navigate to `Settings` > `Secrets and variables` > `Actions`.
3. Click on `New repository secret`.
4. Name the secret `NUGET_API_KEY` and paste the copied API key.

### Step 5: Create a Tag and Push
To trigger the GitHub Actions workflow, create a tag in your repository and push it.

```sh
git tag v1.0.0
git push origin v1.0.0
```

### Summary
- **Prepare your project**: Ensure your `.csproj` file is configured with the necessary NuGet metadata.
- **Create GitHub Actions workflow**: Add a workflow file that builds, packs, and publishes your package to NuGet.
- **Set up NuGet API key**: Generate an API key on NuGet.org and add it to your GitHub repository secrets.
- **Create a tag**: Push a tag to your repository to trigger the workflow.

With these steps, each time you create a tag in your repository, GitHub Actions will automatically build, pack, and publish your NuGet package.