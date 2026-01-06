function SplitString(const S, Delimiter: string): TArrayOfString;
var
  Strings: TArrayOfString;
  S2: string;
  i, p, n: Integer;
begin
  n := 0;
  S2 := S;
  repeat
    SetArrayLength(Strings, n+1);
    p := Pos(Delimiter, S2);
    if p > 0 then begin
      Strings[n] := Copy(S2, 1, p-1);
      Delete(S2, 1, p + Length(Delimiter) - 1);
    end else begin
      Strings[n] := S2;
      S2 := '';
    end;
    Inc(n);
  until S2 = '';
  Result := Strings;
end;

function AddToPath(path: string): Boolean;
var
  origPath, newPath: string;
begin
  RegQueryStringValue(HKEY_LOCAL_MACHINE,
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
    'Path', origPath);
    
  if Pos(LowerCase(path), LowerCase(origPath)) = 0 then begin
    if (Length(origPath) > 0) and (origPath[Length(origPath)] <> ';') then
      origPath := origPath + ';';
    newPath := origPath + path;
    
    RegWriteStringValue(HKEY_LOCAL_MACHINE,
      'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
      'Path', newPath);
  end;

  Result := True;
end;

procedure RemoveFromPath(path: string);
var
  origPath, newPath: string;
  parts: TArrayOfString;
  i: Integer;
begin
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE,
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
    'Path', origPath) then
    exit;

  parts := SplitString(origPath, ';');
  newPath := '';
  for i := 0 to GetArrayLength(parts) - 1 do begin
    if CompareText(Trim(parts[i]), path) <> 0 then begin
      if newPath <> '' then
        newPath := newPath + ';';
      newPath := newPath + parts[i];
    end;
  end;

  RegWriteStringValue(HKEY_LOCAL_MACHINE,
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
    'Path', newPath);
end;
