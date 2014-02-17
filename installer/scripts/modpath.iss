[Code]
procedure ModPath();
var
	pathdir:	String;
	oldpath:	String;
	newpath:	String;
	pathArr:	TArrayOfString;
	aExecFile:	String;
	aExecArr:	TArrayOfString;
	i:			Integer;
begin
	pathdir := ExpandConstant('{app}');

	// Get current path, split into an array
#ifdef PerUser
	RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'PATH', oldpath);
#else
	RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'PATH', oldpath);
#endif
	oldpath := oldpath + ';';
	i := 0;
	while (Pos(';', oldpath) > 0) do begin
		SetArrayLength(pathArr, i+1);
		pathArr[i] := Copy(oldpath, 0, Pos(';', oldpath)-1);
		oldpath := Copy(oldpath, Pos(';', oldpath)+1, Length(oldpath));
		i := i + 1;

		// Check if current directory matches app dir
		if pathdir = pathArr[i-1] then begin
			// if uninstalling, remove dir from path
			if IsUninstaller() = true then begin
				continue;
			// if installing, abort because dir was already in path
			end else begin
				abort;
			end;
		end;

		// Add current directory to new path
		if i = 1 then begin
			newpath := pathArr[i-1];
		end else begin
			newpath := newpath + ';' + pathArr[i-1];
		end;
	end;

	// Append app dir to path if not already included
	if IsUninstaller() = false then
		newpath := newpath + ';' + pathdir;

	// Write new path
#ifdef PerUser
	RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', 'PATH', newpath);
#else
	RegWriteStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'PATH', newpath);
#endif
end;
