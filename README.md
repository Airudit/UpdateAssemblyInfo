
UpdateAssemblyInfo
===========================================

## What it does

This CLI executable will help put git and package traces in .NET binaries.

How to declare in a project? Set this pre-build event. 


## Install / Restore

Make a project local install with:

```console
# create a tool manifest file for your project
dotnet new tool-manifest

# verify
cat .config/dotnet-tools.json

# install
dotnet tool install Airudit.UpdateAssemblyInfo

# verify
cat .config/dotnet-tools.json

# verify command
dotnet UpdateAssemblyInfo --help
```

If the tool il already installed in your project, you need to restore the tools using

```console
dotnet tool restore
```

### Use v3

Here is for old-style csproj files:

```
dotnet UpdateAssemblyInfo $(ProjectDir)\Properties\AssemblyInfo.auto.cs  --build $(ConfigurationName) --company MyCompany --product MyProduct --Copyright "Copyright © MyCompany 2015" --trademark "MyCompany is a trademark" --using Airudit.AssemblyInfo --SCV --BI --Version 1.0.0.0 --FileVersion 1.0.0.0 --Package "$(PackageFullName)" --PackageName "$(PackageName)"
```

Here is for dotnet core-style csproj files:

```
  <PropertyGroup>
    <PreBuildEvent>dotnet UpdateAssemblyInfo $(MSBuildProjectDirectory)/Properties/AssemblyInfo.auto.cs  --build "$(Configuration)" --company MyCompany --product MyProduct --Copyright "Copyright © MyCompany 2015" --trademark "MyCompany is a trademark" --using Airudit.AssemblyInfo --SCV --Version 1.0.0.0 --FileVersion 1.0.0.0 --Package "$(PackageFullName)" --PackageName "$(PackageName)"</PreBuildEvent>
  </PropertyGroup>
```

And include the hidden file `Properties\AssemblyInfo.auto.cs` in your project.

