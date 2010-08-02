// requires Windows 2000 Service Pack 4, Windows Server 2003, Windows Server 2008, Windows Vista, Windows XP
// requires windows installer 3.1
// http://www.microsoft.com/downloads/details.aspx?FamilyID=a5c84275-3b97-4ab7-a40d-3802b2af5fc2

[CustomMessages]
vcredist_title=Visual C++ 2008 SP1 Redistributable Package

en.vcredist_size=4.0 MB
de.vcredist_size=4,0 MB


[Code]	
const
	vcredist_url = 'http://download.microsoft.com/download/d/d/9/dd9a82d0-52ef-40db-8dab-795376989c03/vcredist_x86.exe';

procedure vcredist();
var
	installed: boolean;
begin
	installed :=
		//detect side-by-side install on Windows 2000, XP, 2003
		FileExists(GetEnv('windir') + '\WinSxS\x86_Microsoft.VC90.CRT_1fc8b3b9a1e18e3b_9.0.30729.1_x-ww_6f74963e\msvcr90.dll') or
		//detect side-by-side install on Windows Vista, 7
		FileExists(GetEnv('windir') + '\WinSxS\x86_microsoft.vc90.crt_1fc8b3b9a1e18e3b_9.0.30729.1_none_e163563597edeada\msvcr90.dll');
	if not installed then begin
		AddProduct('vcredist_x86.exe', '/q',
			CustomMessage('vcredist_title'),
			CustomMessage('vcredist_size'),
			vcredist_url);
	end;
end;
