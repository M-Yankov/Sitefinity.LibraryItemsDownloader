# Sitefinity.LibraryItemsDownloader

<p align="center">  
   <img alt="Logo" src="./Logo.png" />
</p>

[![Build status](https://ci.appveyor.com/api/projects/status/q7gb7kdr6gn46bcw/branch/development?svg=true)](https://ci.appveyor.com/project/M-Yankov/sitefinity-libraryitemsdownloader/branch/development) [![Coverage Status](https://coveralls.io/repos/github/M-Yankov/Sitefinity.LibraryItemsDownloader/badge.svg?branch=development)](https://coveralls.io/github/M-Yankov/Sitefinity.LibraryItemsDownloader?branch=development)

### Description 

A package that extends sitefinity back-end to allow multiple download of Images, Videos and Documents.

It adds an additional dropdown item, which allow you download the selected library items:

| Old Versions | New Versions  |
| ------- | ------- |
| ![ScreenShot Old Versions](./Images/SitefinityOld.png) | ![Screen Shot new version](./Images/SitefinityNew.png) |

_This package is supposed to be used with **sitefinity** projects only._  
[Get trial version of Sitefinity](http://sitefinity.com)

### How to use

Open an image (video/documents) library, select some of the items, expand `More actions` and click `Download selected images` _(for image libraries)_  
Also you can select a folder and it will collect all of the content recursively _(i.e. all nested forders)_. 

| Demo |
| ----- |
|   ![ScreenShot Old Versions](./Images/Demo.gif)  |


In order to download items from more than one library at once, you can use filter all library items. _(Usually located at the right site of the page)_

### How to install:

- if you are using Sitefinity between `7.3.5600` and `9.0.6041` inclusive.

`PM > Install-Package Sitefinity.LibraryItemsDownloader -Version 7.3.5600`

- if you are using Sitefinity `9.1.6100` or higher.

`PM > Install-Package Sitefinity.LibraryItemsDownloader -Version 9.1.6100`

The difference between versions is a little change in the Sitefinity API when saving configuration files.

#### Additional 

The package depends on these assemblies from Sitefinity:
- Telerik.Sitefinity
- Telerik.Sitefinity.Model
- Telerik.Sitefinity.Utilities
- Telerik.OpenAccess

You may need to add `<assemblyBinding />` configurations:

```
<dependentAssembly>
  <assemblyIdentity name="Telerik.Sitefinity" publicKeyToken="b28c218413bdf563" culture="neutral" />
  <bindingRedirect oldVersion="0.0.0.0-<sfVersion>" newVersion="<sfVersion>" />
</dependentAssembly>
<dependentAssembly>
  <assemblyIdentity name="Telerik.Sitefinity.Model" publicKeyToken="b28c218413bdf563" culture="neutral" />
  <bindingRedirect oldVersion="0.0.0.0-<sfVersion>" newVersion="<sfVersion>" />
</dependentAssembly>
<dependentAssembly>
  <assemblyIdentity name="Telerik.Sitefinity.Utilities" publicKeyToken="b28c218413bdf563" culture="neutral" />
  <bindingRedirect oldVersion="0.0.0.0-<sfVersion>" newVersion="<sfVersion>" />
</dependentAssembly>
<dependentAssembly>
  <assemblyIdentity name="Telerik.OpenAccess" publicKeyToken="b28c218413bdf563" culture="neutral" />
  <bindingRedirect oldVersion="0.0.0.0-<openAccessVersion>" newVersion="<openAccessVersion>" />
</dependentAssembly>
<dependentAssembly>

```

where `<sfVersion>` is the version of the Sitefinty in the project.
and `<openAccessVersion>` is the version of the Telerik.OpenAccess assembly.

**There are several ways to identify which version of sitefinity you are using**

1. If you have access to the backend - open the licensing page (under Administration).
1. At the home page open the page source and search for meta tag `<meta name="Generator" content="Sitefinity 8.1.5800.0 PU" />`
1. In Visual Studio, expand the references of the project, find Telerik.Sitefinity , Right click > properties


source: https://knowledgebase.progress.com/articles/Article/How-to-check-your-current-Sitefinity-version

**Backup**

It's recommended to make a backup of the project and database before proceed.

### Compatibility

#### Browsers

- Mozilla Firefox
- Google Chrome
- Internet Explorer 11
- Edge

#### .NET

| Version |  |
|--|--|
| 4.0 |  ✔️ |
| 4.5 | ✔️ | 
| 4.5+ |  ✔️ | 
| .NET CORE | ❌ | 


#### Sitefinity versions

- All versions equal to `7.3.5600` or higher (it may work with previous versions, but it's not tested.)
  - The latest version that it works with is `10.2.6602.0`
  

