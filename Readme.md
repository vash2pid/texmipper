Introduction
------------
The tool can be used to convert Warhammer 40,0000 Space Marine 2 texture files, *.tga to *.pct_mip files.

The tool will require *.tga file and the corresponding *.pct.resource file.
The *.pct.resource file is required to correctly convert the files.

When converting from *.tga to *.pct_mip (mip) files, multiple *.pct_mip files are created depending on 
the nMipMap property on the resource file. The output filenames of the mips are taken from the
mipMaps property on the resource file.

*The *.tga file to convert from does not have to be created from this tool. Other tools can be used, or can be created from scratch.
*The *.tga file must have the same dimensions stated in the sx, sy properies in the *.resource file.

During the conversion, it may take some time depending on the texture.

*.pct_mip files are found on default_pct_<#>.pak
*.pct.resource files are found on resources.pak

Every *.pct_mip has a corresponding *.pct.resource file, this file is required for the tool to convert the *.tga file to *.pct_mip.

Requirements
------------
Windows 10
.NET Desktop Runtime 8.0 or higher. 
 
Usage
-----
1. Run the executable texmipper.exe.
2. A multi select file dialog prompt will open, select the texture files that you want to convert.
   *.tga - will convert to *.pct_mip file(s) that is compatible with Space Marine 2
3. A corresponding *.pct.resource file must be in the same location per the selected texture file.
   The *.pct_mip.resource file must have the same name as the texture file.
4. The files are created in the same folder of the selected texture file.
   If existing filse with the same name as the output files, the existing ones are backed up.

Limitations
-----------
The tool currently can only convert *.tga file.
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
