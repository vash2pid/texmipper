Introduction
------------
The tool can be used to convert Warhammer 40,0000 Space Marine 2 texture files, *.tga to *.pct_mip files.
And *.pct_mip files to *.tga (Normal maps are not 'officially' supported when converting from *.pct_mip files to *.tga).
The tool will convert the normal maps but it might not be in the format that you want or need.

The tool will require *.tga or *.pct_mip file and the game's resoure.pak file in the same folder.
And the game's resoure.pak file is required to correctly convert the files.
*resources.pak file is found if game is installed on steam on (steam game installation directory)\Space Marine 2\client_pc\root\paks\client\resources.pak

When converting from *.tga to *.pct_mip (mip) files, multiple *.pct_mip files are created depending on 
the nMipMap property on the resource file, this is done automatically by the tool.
The output filenames of the mips are taken from the mipMaps property on the resource file.

When converting from *.pct_mip to *.tga file, you must use the highest quality mip. Usually the name has the suffix *_1.pct_mip.
And you must remove the suffix in the file name.
e.g. d_shldr_death_guard_02_1.pct_mip to d_shldr_death_guard_02.pct_mip

This ensures you are converting the highest quality mip.
*.pct_mip files are found on inside default_pct_<#>.pak in directory \Space Marine 2\client_pc\root\paks\client\default\

*.pak files are just zip files, you can use 7zip or WinRar if you want to open one.
*The *.tga file to convert from does not have to be created from this tool. Other tools can be used, or can be created from scratch.
*The *.tga file must have the same dimensions stated in the sx, sy properies in the *.resource file. *.resource files are just text files that can be opened in text processor like notepad.
*The *.resource file can be found inside (steam game installation directory)\Space Marine 2\client_pc\root\paks\client\resources.pak.
*The latest version of the tool does not need to have *.resource in the same folder but if you need to know the dimensions when creating from scrath you can refer to the *.resource.

Requirements
------------
Windows 10
.NET Desktop Runtime 8.0 or higher. 
 
Usage
-----
1. Run the executable texmipper.exe.

2. A multi select file dialog prompt will open, select the texture files that you want to convert.
   *.tga - will convert to *.pct_mip file(s) that is compatible with Space Marine 2
   *.pct_mip - will convert to *.tga (Normal maps are not 'officially' supported when converting from *.pct_mip files to *.tga).

3. The game's resource.pak file must be in the same folder.

4. The files are created in the same folder of the selected file.
   Existing ones are backed up when converting.

Limitations
-----------
The tool currently can only convert from *.tga file to *.pct_mip.
Normal maps are not 'officially' supported when converting from *.pct_mip to *.tga. The tool will convert it bt it might not be in the format that you want or need.
The person that created this tool have limited knowledge on image formats, image/texure editing.

Special thanks to
-----------------
Wildenhaus for his work on https://github.com/Wildenhaus/LibSaber
and Space Marine 2 Modding Community

Dependencies
------------
This software makes use of the following:

Copyright (c) 2010-2014 SharpDX - Alexandre Mutel

New DirectXTexNet
Copyright (c) 2024 Dennis Gocke

Original DirectXTexNet
Copyright (c) 2016 Simon Taylor

DirectXTex
Copyright (c) 2011-2024 Microsoft Corp

YamlDotNet
Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013, 2014 Antoine Aubry and contributors
Copyright (c) 2006 Kirill Simonov

Permission is hereby granted, free of charge, to any person obtaining a copy of this
software and associated documentation files (the "Software"), to deal in the Software
without restriction, including without limitation the rights to use, copy, modify,
merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be included in all copies
or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
