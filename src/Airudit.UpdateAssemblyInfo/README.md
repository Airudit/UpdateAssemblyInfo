

dotnet UpdateAssemblyInfo
========================

This dotnet tool will help put git and package traces in .NET binaries.

## Install

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

## Restore

If the tool il already installed in your project, you need to restore the tools using

```console
https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use
```

## Use

Now you can use the command in your build process

```console
dotnet UpdateAssemblyInfo help/ README.md
```

Usage:

```
MarkdownToHtml command usage: 
    {file path}+ [options]

Options: 
    --Export <dir>        Exports the generated documentation to this directory
    --Single-File <file>  Exports the generated documentation to a single file
    --Template <file>     Specifies the HTML template file
```

## More info

[Airudit.UpdateAssemblyInfo](https://github.com/Airudit/UpdateAssemblyInfo) project

[Tutorial: Install and use a .NET local tool using the .NET CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use)
