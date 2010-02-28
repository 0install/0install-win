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
	version: string;
begin
	RegQueryStringValue(HKEY_LOCAL_MACHINE, 'Software\NanoByte\NanoGrid\Info', 'Major', version);
	if version <> '1' then begin
		AddProduct('nanogrid.exe',
			'/silent /norestart /mergetasks=!autostart',
			CustomMessage('nanogrid_title'),
			CustomMessage('nanogrid_size'),
			nanogrid_url);
	end;
end;
