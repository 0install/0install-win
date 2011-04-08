// requires Windows 2000 or newer
// requires .NET Framework 2.0 or newer
// http://www.nano-byte.de/download/nanogrid.htm

[CustomMessages]
nanogrid_title=NanoGrid

en.nanogrid_size=1.5 MB
de.nanogrid_size=1,5 MB


[Code]	
const
	nanogrid_url = 'http://download.nanobyte.de/nanogrid.exe';

procedure nanogrid();
var
	version1: string;
	version2: string;
begin
	RegQueryStringValue(HKEY_LOCAL_MACHINE, 'Software\NanoByte\NanoGrid\Info', 'Major', version1);
	RegQueryStringValue(HKEY_LOCAL_MACHINE, 'Software\Wow6432Node\NanoByte\NanoGrid\Info', 'Major', version2);
	if (version1 <> '1') and (version2 <> '1') then begin
		AddProduct('nanogrid.exe',
			'/silent /norestart /mergetasks=!autostart',
			CustomMessage('nanogrid_title'),
			CustomMessage('nanogrid_size'),
			nanogrid_url);
	end;
end;
